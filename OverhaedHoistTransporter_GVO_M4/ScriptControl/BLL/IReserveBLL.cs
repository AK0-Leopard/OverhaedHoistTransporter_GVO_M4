using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using Google.Protobuf.Collections;
using Mirle.Hlts.Utils;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace com.mirle.ibg3k0.sc.BLL
{
    public interface IReserveBLL
    {
        event EventHandler ReserveStatusChange;

        (bool isSuccess, string reservedVhID, string reservedSecID) askReserveSuccess(SectionBLL sectionBLL, string vhID, string sectionID, string addressID);
        bool DrawAllReserveSectionInfo();
        BitmapSource GetCurrentReserveInfoMap();
        List<string> GetCurrentReserveSectionList(string byPassVh = null);
        string GetCurrentReserveSectionString();
        (bool isSuccess, double x, double y, bool isTR50) GetHltMapAddress(string adrID);
        (bool isExist, string StartAddressID, string EndAddressID) GetMapSectionsInfo(string secID);
        (double X, double Y, float Angle) GetMapVehicleInfo(string vhID);
        double GetMaxHltMapAddress_x();
        (bool isSuccess, string reservedVhID, string reservedFailSection, RepeatedField<ReserveInfo> reserveSuccessInfos) IsMultiReserveSuccess(SCApplication scApp, string vhID, RepeatedField<ReserveInfo> reserveInfos, bool isAsk = false);
        bool IsR2000Section(string sectionID);
        (bool isSuccess, string reservedVhID, string reservedSecID) IsReserveSuccessNew(string vhID, RepeatedField<ReserveInfo> reserveInfos, bool isAsk = false);
        (bool isSuccess, string reservedVhID, string reservedSecID) IsReserveSuccessTest(string vhID, RepeatedField<ReserveInfo> reserveInfos);
        List<string> loadCurrentReserveSections(string vhID);
        void RemoveAllReservedSections();
        ReserveCheckResult RemoveAllReservedSectionsBySectionID(string sectionID);
        void RemoveAllReservedSectionsByVehicleID(string vhID);
        void RemoveManyReservedSectionsByVIDSID(string vhID, string sectionID);
        void RemoveVehicle(string vhID);
        void start(SCApplication _app);
        ReserveCheckResult TryAddReservedSection(string vhID, string sectionID, HltDirection sensorDir = HltDirection.None, HltDirection forkDir = HltDirection.None, bool isAsk = false);
        ReserveCheckResult TryAddVehicleOrUpdate(string vhID, string adrID);
        ReserveCheckResult TryAddVehicleOrUpdate(string vhID, string currentSectionID, double vehicleX, double vehicleY, float vehicleAngle, double speedMmPerSecond, HltDirection sensorDir, HltDirection forkDir);
        ReserveCheckResult TryAddVehicleOrUpdateResetSensorForkDir(string vhID);
    }
}