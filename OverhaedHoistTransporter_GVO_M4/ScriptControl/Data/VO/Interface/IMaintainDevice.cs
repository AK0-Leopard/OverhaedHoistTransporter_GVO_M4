using com.mirle.ibg3k0.sc.Data.SECS;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.VO.Interface
{
    public interface IMaintainDevice
    {
        string DeviceID { get; set; }
        string DeviceSegment { get; set; }
        string DeviceAddress { get; set; }
        string CurrentCarID { get; set; }
        bool HasVehicle { get; set; }
        bool StopSignal { get; set; }
        MTxMode MTxMode { get; set; }
        bool MTLCarInSafetyCheck { get; set; }
        bool MTLCarOutSafetyCheck { get; set; }
        bool CarOutActionTypeSystemOutToMTS { get; set; }
        bool CarOutActionTypeSystemOutToMTL { get; set; }
        bool CarOutActionTypeMTSToMTL { get; set; }
        bool CarOutActionTypeSystemInFronMTS { get; set; }
        bool CarOutActionTypeOnlyPass { get; set; }
        string PreCarOutVhID { get; set; }
        (bool isSendSuccess, UInt16 returnCode) carOutRequest(UInt16 carNum);
        bool SetCarOutInterlock(bool onOff);
        bool SetCarInMoving(bool onOff);

        bool CancelCarOutRequest { get; set; }
        bool CarOurSuccess { get; set; }
        bool IsAlive { get; set; }
        #region OHTC to MTx
        UInt16 CurrentPreCarOurID { get; set; }
        UInt16 CurrentPreCarOurActionMode { get; set; }
        UInt16 CurrentPreCarOurCSTExist { get; set; }
        UInt16 CurrentPreCarOurSectionID { get; set; }
        UInt32 CurrentPreCarOurAddressID { get; set; }
        UInt32 CurrentPreCarOurDistance { get; set; }
        UInt16 CurrentPreCarOurSpeed { get; set; }
        bool OHTCCarOutInterlock { get; set; }
        bool OHTCCarOutMoving { get; set; }

        void setCarRealTimeInfo(UInt16 car_id, UInt16 action_mode, UInt16 cst_exist, UInt16 current_section_id, UInt32 current_address_id,
                                            UInt32 buffer_distance, UInt16 speed);
        #endregion OHTC to MTx

    }
}
