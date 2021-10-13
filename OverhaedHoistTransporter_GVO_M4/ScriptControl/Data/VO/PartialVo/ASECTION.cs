using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using com.mirle.ibg3k0.sc.Common;
using NLog;

namespace com.mirle.ibg3k0.sc
{
    public partial class ASECTION
    {
        public ConcurrentDictionary<string, string> CurrentVhs { get; private set; } = new ConcurrentDictionary<string, string>();
        public ConcurrentDictionary<string, string> DriveDirForwardVhs { get; private set; } = new ConcurrentDictionary<string, string>();
        public ConcurrentDictionary<string, string> DriveDirReverseVhs { get; private set; } = new ConcurrentDictionary<string, string>();

        private AADDRESS[] OnSectionAddress = null;

        public bool isOnlySingleVh = false;
        //public int TURNING_ANGLE = 0;
        public List<string> ALTERNATIVE_PATH = null;

        private EventHandler<string> vehicleLeave;
        private object _vehicleLeaveEventLock = new object();
        public event EventHandler<string> VehicleLeave
        {
            add
            {
                lock (_vehicleLeaveEventLock)
                {
                    vehicleLeave -= value;
                    vehicleLeave += value;
                }
            }
            remove
            {
                lock (_vehicleLeaveEventLock)
                {
                    vehicleLeave -= value;
                }
            }
        }

        private EventHandler<string> vehicleEntry;
        private object _vehicleEntryEventLock = new object();
        public event EventHandler<string> VehicleEntry
        {
            add
            {
                lock (_vehicleEntryEventLock)
                {
                    vehicleEntry -= value;
                    vehicleEntry += value;
                }
            }
            remove
            {
                lock (_vehicleEntryEventLock)
                {
                    vehicleEntry -= value;
                }
            }
        }

        //public string RealFromAdrID
        //{

        //}
        public string GetOrtherEndPoint(string endPoint)
        {
            if (Common.SCUtility.isMatche(this.FROM_ADR_ID, endPoint))
            {
                return this.TO_ADR_ID;
            }
            else if (Common.SCUtility.isMatche(this.TO_ADR_ID, endPoint))
            {
                return this.FROM_ADR_ID;
            }
            else
            {

                LogHelper.Log(logger: LogManager.GetCurrentClassLogger(), LogLevel: LogLevel.Warn, Class: nameof(ASECTION), Device: "OHTC",
                   Data: $"in section id:{SEC_ID}, unknow endpoint:{endPoint}");
                return "";
            }
        }
        public bool IsActive(BLL.SegmentBLL segmentBLL)
        {
            var sement = segmentBLL.cache.GetSegment(this.SEG_NUM);
            if (sement == null)
            {
                return false;
            }
            else
            {
                return sement.STATUS == E_SEG_STATUS.Active;
            }
            //return segmentBLL.cache.GetSegment(this.SEG_NUM).STATUS == E_SEG_STATUS.Active;
        }
        public void setOnSectionAddress(BLL.AddressesBLL addressesBLL)
        {
            OnSectionAddress = addressesBLL.cache.GetAddressesBySectionID(this.SEC_ID)?.ToArray();
        }
        public AADDRESS[] GetOnSectionAddresses()
        {
            return OnSectionAddress;
        }

        public string ZONE_ID
        {
            get
            {
                char first_char = SEC_ID[0];
                switch (first_char)
                {
                    case '1':
                        return "EQ_ZONE1";
                    case '2':
                        return "EQ_ZONE2";
                    case '3':
                        return "EQ_ZONE3";
                    default:
                        return "";
                }
            }
        }

        public void Leave(string vh_id)
        {
            string vVh_id = string.Empty;
            if (CurrentVhs.TryRemove(vh_id, out vVh_id))
            {
                onSectinoLeave(vh_id);
            }
        }
        public void Entry(string vh_id)
        {
            if (CurrentVhs.TryAdd(vh_id, vh_id))
            {
                onSectinoEntry(vh_id);
            }
        }

        public void AcquireSectionReservation(string vhID, DriveDirction driveDirction)
        {
            switch (driveDirction)
            {
                case DriveDirction.DriveDirForward:
                    DriveDirForwardVhs.TryAdd(vhID, vhID);
                    break;
                case DriveDirction.DriveDirReverse:
                    DriveDirReverseVhs.TryAdd(vhID, vhID);
                    break;
            }
        }

        public void ClaerSectionReservation(DriveDirction driveDirction)
        {
            switch (driveDirction)
            {
                case DriveDirction.DriveDirForward:
                    DriveDirForwardVhs.Clear();
                    break;
                case DriveDirction.DriveDirReverse:
                    DriveDirReverseVhs.Clear();
                    break;
            }
        }

        public void ReleaseSectionReservation(string vhID)
        {
            string vh_id;
            DriveDirForwardVhs.TryRemove(vhID, out vh_id);
            DriveDirReverseVhs.TryRemove(vhID, out vh_id);
        }

        private void onSectinoLeave(string vh_id)
        {
            vehicleLeave?.Invoke(this, vh_id);
        }
        private void onSectinoEntry(string vh_id)
        {
            vehicleEntry?.Invoke(this, vh_id);
        }


        public string REAL_FROM_ADR_ID
        {
            get
            {
                switch (SEC_DIR)
                {
                    case E_RAIL_DIR.F:
                        return FROM_ADR_ID;
                    case E_RAIL_DIR.R:
                        return TO_ADR_ID;
                    default:
                        return FROM_ADR_ID;
                }
            }
        }
        public string REAL_TO_ADR_ID
        {
            get
            {
                switch (SEC_DIR)
                {
                    case E_RAIL_DIR.F:
                        return TO_ADR_ID;
                    case E_RAIL_DIR.R:
                        return FROM_ADR_ID;
                    default:
                        return TO_ADR_ID;
                }
            }
        }
        public string[] NodeAddress
        {
            get
            {
                return new string[] { SCUtility.Trim(FROM_ADR_ID, true), SCUtility.Trim(TO_ADR_ID, true) };
            }
        }

    }



}
