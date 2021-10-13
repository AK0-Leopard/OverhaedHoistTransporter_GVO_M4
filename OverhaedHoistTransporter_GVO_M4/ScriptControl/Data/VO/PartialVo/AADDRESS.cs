using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using com.mirle.ibg3k0.sc.BLL;
using System.Collections;
using Newtonsoft.Json;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.App;
using NLog;

namespace com.mirle.ibg3k0.sc
{
    public partial class AADDRESS
    {
        private const int BIT_INDEX_COUPLER = 0;
        private const int BIT_INDEX_CANAVOID = 1;
        private const int BIT_INDEX_CANACCESS = 2;

        public Boolean[] AddressTypeFlags { get; set; }
        public string[] SegmentIDs { get; set; }
        protected NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public event EventHandler<string> VehicleRelease;
        //public AADDRESS()
        //{
        //    initialAddressType();
        //}
        public void initialAddressType()
        {
            BitArray b = new BitArray(new int[] { this.ADRTYPE });
            AddressTypeFlags = new bool[b.Count];
            b.CopyTo(AddressTypeFlags, 0);
        }
        public void initialSegmentID(SectionBLL sectionBLL)
        {
            var sections = sectionBLL.cache.GetSectionsByFromAddress(ADR_ID);
            SegmentIDs = sections.Select(sec => sec.SEG_NUM).Distinct().ToArray();
            if (SegmentIDs.Length == 0)
            {
                throw new Exception($"Adr id:{ADR_ID},no setting on section or segment");
            }
        }

        public void Release(string vhID)
        {
            OnAddressRelease(vhID);
        }

        private void OnAddressRelease(string vhID)
        {
            VehicleRelease?.Invoke(this, vhID);
        }


        [JsonIgnore]
        public bool IsCoupler
        //{ get { return false; } }
        { get { return AddressTypeFlags[BIT_INDEX_COUPLER]; } }
        [JsonIgnore]
        public bool IsAccessable
        //{ get { return false; } }
        { get { return !AddressTypeFlags[BIT_INDEX_CANACCESS]; } }
        //[JsonIgnore]
        //public bool IsPort
        //{ get { return false; } }
        [JsonIgnore]
        public bool IsControl
        { get { return false; } }
        [JsonIgnore]
        public bool IsSegment
        {
            get
            {
                return false;
            }
        }
        [JsonIgnore]
        public bool CanAvoid
        //{ get { return false; } }
        { get { return AddressTypeFlags[BIT_INDEX_CANAVOID]; } }


        [JsonIgnore]
        public bool IsSection
        {
            get
            {
                return false;
            }
        }

        public bool IsPort(BLL.PortStationBLL portStationBLL)
        {
            List<APORTSTATION> ports = portStationBLL.OperateCatch.getPortStationByAdrID(ADR_ID);
            //if (ports == null || ports.Count() == 0)
            //{
            //    string virtual_adr = replaceFirstChar(ADR_ID);
            //    ports = portStationBLL.OperateCatch.getPortStationByAdrID(virtual_adr);
            //}
            return ports != null && ports.Count() > 0;

        }
        private string replaceFirstChar(string curAdrID)
        {
            string replaced_cur_adr = curAdrID;
            try
            {
                replaced_cur_adr = $"9{replaced_cur_adr.Substring(1, curAdrID.Length - 1)}";
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            return replaced_cur_adr;
        }

        //public bool canAvoidVehicle
        //{
        //    get
        //    {
        //        //return IsCoupler || IsPort || IsControl;
        //        return true;
        //    }
        //}
        public bool canAvoidVehicle(SectionBLL sectionBLL)
        {
            //如果Address所連接的Section數量>2，則代表該車子是在該Section的叉路中，暫時不拿來作為避車使用
            var sections = sectionBLL.cache.GetSectionsByAddress(this.ADR_ID);
            return sections.Count <= 2;
        }


    }

    public class CouplerAddress : AADDRESS, ICpuplerType, IComparable<CouplerAddress>
    {
        public string ChargerID { get; set; }
        public CouplerNum CouplerNum { get; set; }
        public bool IsEnable { get; set; }
        public int Priority { get; set; }
        public string[] TrafficControlSegment { get; set; }

        public bool hasVh(VehicleBLL vehicleBLL)
        {
            return vehicleBLL.cache.hasVhOnAddress(ADR_ID);
        }
        public bool hasChargingVh(VehicleBLL vehicleBLL)
        {
            return vehicleBLL.cache.hasChargingVhOnAddress(ADR_ID);
        }
        public bool hasVhGoing(VehicleBLL vehicleBLL)
        {
            return vehicleBLL.cache.hasVhGoingAdr(ADR_ID);
        }

        public void setDistanceWithTargetAdr(GuideBLL guideBLL, string targetAdr)
        {
            //if (!guideBLL.IsRoadWalkable(this.ADR_ID, targetAdr))
            if (!guideBLL.IsRoadWalkable(this.ADR_ID, targetAdr, out int totalCost))
            {
                DistanceWithTargetAdr = int.MaxValue;
            }
            else
            {
                //DistanceWithTargetAdr = guideBLL.getGuideInfo(this.ADR_ID, targetAdr).totalCost;
                DistanceWithTargetAdr = totalCost;
            }
        }

        public bool IsWork(UnitBLL unitBLL)
        {
            if (DebugParameter.isPassCouplerStatus)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(AADDRESS), Device: "OHTC",
                   Data: $"pass coupler status,flag:{DebugParameter.isPassCouplerStatus}");
                return true;
            }

            //return true;
            AUNIT charger = unitBLL.OperateCatch.getUnit(ChargerID);
            if (charger != null)
            {
                switch (CouplerNum)
                {
                    case CouplerNum.NumberOne:
                        return charger.Coupler1Status == SCAppConstants.CouplerStatus.Auto ||
                               charger.Coupler1Status == SCAppConstants.CouplerStatus.Charging;
                    case CouplerNum.NumberTwo:
                        return charger.Coupler1Status == SCAppConstants.CouplerStatus.Auto ||
                               charger.Coupler1Status == SCAppConstants.CouplerStatus.Charging;
                    case CouplerNum.NumberThree:
                        return charger.Coupler1Status == SCAppConstants.CouplerStatus.Auto ||
                               charger.Coupler1Status == SCAppConstants.CouplerStatus.Charging;
                }
            }
            return false;
        }

        public int DistanceWithTargetAdr { get; private set; } = 0;
        public int CompareTo(CouplerAddress other)
        {
            int result;
            if (this.Priority == other.Priority && this.DistanceWithTargetAdr == other.DistanceWithTargetAdr)
            {
                result = 0;
            }
            else
            {
                if (this.Priority > other.Priority)
                {
                    result = -1;
                }
                else if (this.Priority == other.Priority && this.DistanceWithTargetAdr < other.DistanceWithTargetAdr)
                {
                    result = -1;
                }
                else
                {
                    result = 1;
                }
            }

            return result;
        }
    }

    public class ReserveEnhanceAddress : AADDRESS, IReserveEnhance
    {
        public string[] EnhanceControlAddress { get; set; }
        public List<Data.VO.ReserveEnhanceInfo> infos { get; set; }
    }


    public class CouplerAndReserveEnhanceAddress : AADDRESS, ICpuplerType, IReserveEnhance
    {
        public string ChargerID { get; set; }
        public CouplerNum CouplerNum { get; set; }
        public bool IsEnable { get; set; }
        public int Priority { get; set; }
        public string[] TrafficControlSegment { get; set; }

        public bool hasVh(VehicleBLL vehicleBLL)
        {
            return vehicleBLL.cache.hasVhOnAddress(ADR_ID);
        }
        public bool hasVhGoing(VehicleBLL vehicleBLL)
        {
            return vehicleBLL.cache.hasVhGoingAdr(ADR_ID);
        }

        public bool IsWork(UnitBLL unitBLL)
        {
            //return true;
            AUNIT charger = unitBLL.OperateCatch.getUnit(ChargerID);
            if (charger != null)
            {
                switch (CouplerNum)
                {
                    case CouplerNum.NumberOne:
                        return charger.Coupler1Status == SCAppConstants.CouplerStatus.Auto ||
                               charger.Coupler1Status == SCAppConstants.CouplerStatus.Charging;
                    case CouplerNum.NumberTwo:
                        return charger.Coupler1Status == SCAppConstants.CouplerStatus.Auto ||
                               charger.Coupler1Status == SCAppConstants.CouplerStatus.Charging;
                    case CouplerNum.NumberThree:
                        return charger.Coupler1Status == SCAppConstants.CouplerStatus.Auto ||
                               charger.Coupler1Status == SCAppConstants.CouplerStatus.Charging;
                }
            }
            return false;
        }

        public string[] EnhanceControlAddress { get; set; }
        public List<Data.VO.ReserveEnhanceInfo> infos { get; set; }
    }

    public enum CouplerNum
    {
        NumberOne = 1,
        NumberTwo = 2,
        NumberThree = 3
    }

    public interface ICpuplerType
    {
        string ChargerID { get; set; }
        CouplerNum CouplerNum { get; set; }
        bool IsEnable { get; set; }
        int Priority { get; set; }
        string[] TrafficControlSegment { get; set; }
        bool hasVh(VehicleBLL vehicleBLL);
        bool hasVhGoing(VehicleBLL vehicleBLL);
        bool IsWork(UnitBLL unitBLL);

    }

    public interface IReserveEnhance
    {
        //string WillPassSection { get; set; }
        string[] EnhanceControlAddress { get; set; }
        List<Data.VO.ReserveEnhanceInfo> infos { get; set; }
    }
}
