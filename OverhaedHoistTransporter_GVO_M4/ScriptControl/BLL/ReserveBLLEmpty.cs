using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using System;
using System.Text;
using System.Linq;
using NLog;
using Google.Protobuf.Collections;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using System.Collections.Generic;
using Quartz.Impl.Triggers;
using Mirle.Hlts.Utils;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class ReserveBLLEmpty : IReserveBLL
    {
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private EventHandler reserveStatusChange;
        private object _reserveStatusChangeEventLock = new object();
        public event EventHandler ReserveStatusChange
        {
            add
            {
                lock (_reserveStatusChangeEventLock)
                {
                    reserveStatusChange -= value;
                    reserveStatusChange += value;
                }
            }
            remove
            {
                lock (_reserveStatusChangeEventLock)
                {
                    reserveStatusChange -= value;
                }
            }
        }

        private void onReserveStatusChange()
        {
            reserveStatusChange?.Invoke(this, EventArgs.Empty);
        }

        public ReserveBLLEmpty()
        {
        }
        public void start(SCApplication _app)
        {
            //mapAPI = _app.getReserveSectionAPI();
        }

        public bool DrawAllReserveSectionInfo()
        {
            bool is_success = false;
            return is_success;

        }

        public System.Windows.Media.Imaging.BitmapSource GetCurrentReserveInfoMap()
        {
            return null;
        }

        double MAX_X = double.MinValue;
        public double GetMaxHltMapAddress_x()
        {
            return MAX_X;
        }

        public (bool isSuccess, double x, double y, bool isTR50) GetHltMapAddress(string adrID)
        {
            bool is_exist = false;
            double x = double.MaxValue;
            double y = double.MaxValue;
            bool is_tr_50 = false;

            return (is_exist, x, y, is_tr_50);
        }
        public ReserveCheckResult TryAddVehicleOrUpdateResetSensorForkDir(string vhID)
        {
            ReserveCheckResult result = new ReserveCheckResult();
            return result;
        }
        //public HltResult TryAddVehicleOrUpdate(string vhID,
        //                                       string currentSectionID, double vehicleX, double vehicleY, float vehicleAngle, double speedMmPerSecond,
        //                                       HltDirection sensorDir, HltDirection forkDir)
        //{
        //    HltResult result = new HltResult();

        //    return result;
        //}
        public ReserveCheckResult TryAddVehicleOrUpdate(string vhID, string adrID)
        {
            ReserveCheckResult result = new ReserveCheckResult();
            onReserveStatusChange();

            return result;
        }


        public void RemoveManyReservedSectionsByVIDSID(string vhID, string sectionID)
        {
        }
        public List<string> loadCurrentReserveSections(string vhID)
        {
            return new List<string>();
        }

        public void RemoveVehicle(string vhID)
        {
        }

        public string GetCurrentReserveSectionString()
        {
            return "";
        }

        public List<string> GetCurrentReserveSectionList(string byPassVh = null)
        {
            return new List<string>();
        }
        //public HltVehicle GetHltVehicle(string vhID)
        //{
        //    return mapAPI.GetVehicleObjectByID(vhID);
        //}

        //public HltResult TryAddReservedSection(string vhID, string sectionID, HltDirection sensorDir = HltDirection.None, HltDirection forkDir = HltDirection.None, bool isAsk = false)
        //{
        //    return new HltResult();
        //}

        public ReserveCheckResult RemoveAllReservedSectionsBySectionID(string sectionID)
        {

            return new ReserveCheckResult();

        }

        public void RemoveAllReservedSectionsByVehicleID(string vhID)
        {

        }
        public void RemoveAllReservedSections()
        {

        }

        public bool IsR2000Section(string sectionID)
        {
            return false;
        }

        enum HtlSectionType
        {
            Horizontal,
            Vertical,
            R2000
        }

        public (bool isSuccess, string reservedVhID, string reservedSecID) IsReserveSuccessTest(string vhID, RepeatedField<ReserveInfo> reserveInfos)
        {
            return IsReserveSuccessNew(vhID, reserveInfos);
        }
        public (bool isSuccess, string reservedVhID, string reservedSecID) askReserveSuccess(SectionBLL sectionBLL, string vhID, string sectionID, string addressID)
        {
            RepeatedField<ReserveInfo> reserveInfos = new RepeatedField<ReserveInfo>();
            ASECTION current_section = sectionBLL.cache.GetSection(sectionID);
            DriveDirction driveDirction = SCUtility.isMatche(current_section.FROM_ADR_ID, addressID) ?
                DriveDirction.DriveDirForward : DriveDirction.DriveDirReverse;
            ReserveInfo info = new ReserveInfo()
            {
                //DriveDirction = DriveDirction.DriveDirForward,
                DriveDirction = driveDirction,
                ReserveSectionID = sectionID
            };
            reserveInfos.Add(info);
            return IsReserveSuccessNew(vhID, reserveInfos, isAsk: true);
        }
        public (bool isSuccess, string reservedVhID, string reservedSecID) IsReserveSuccessNew(string vhID, RepeatedField<ReserveInfo> reserveInfos, bool isAsk = false)
        {
            return (true, string.Empty, string.Empty);
        }

        public (bool isSuccess, string reservedVhID, string reservedFailSection, RepeatedField<ReserveInfo> reserveSuccessInfos) IsMultiReserveSuccess
                (SCApplication scApp, string vhID, RepeatedField<ReserveInfo> reserveInfos, bool isAsk = false)
        {
            return (true, string.Empty, string.Empty, reserveInfos);
        }

        private (bool isSuccess, string reservedVhID) IsReserveBlockSuccess(SCApplication scApp, string vhID, string reserveSectionID)
        {

            return (true, "");
        }

        public (bool isExist, string StartAddressID, string EndAddressID) GetMapSectionsInfo(string secID)
        {
            return (false, "", "");
        }

        public ReserveCheckResult TryAddReservedSection(string vhID, string sectionID, HltDirection sensorDir = HltDirection.None, HltDirection forkDir = HltDirection.None, bool isAsk = false)
        {
            return ReserveCheckResult.Empty();
        }

        public ReserveCheckResult TryAddVehicleOrUpdate(string vhID, string currentSectionID, double vehicleX, double vehicleY, float vehicleAngle, double speedMmPerSecond, HltDirection sensorDir, HltDirection forkDir)
        {
            return ReserveCheckResult.Empty();
        }

        public (double X, double Y, float Angle) GetMapVehicleInfo(string vhID)
        {
            return (0, 0, 0);
        }
    }
    public class ReserveCheckResult
    {
        public static ReserveCheckResult Empty() { return new ReserveCheckResult(); }
        public ReserveCheckResult() { }
        public ReserveCheckResult(bool ok, string vehicleID, string sectionID, string description)
        {
            OK = ok;
            VehicleID = vehicleID;
            SectionID = sectionID;
            Description = description;
        }

        public virtual bool OK { get; } = true;
        public virtual string VehicleID { get; } = "";
        public virtual string SectionID { get; } = "";
        public virtual string Description { get; } = "";
    }

}
