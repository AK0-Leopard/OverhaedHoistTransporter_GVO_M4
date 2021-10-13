//*********************************************************************************
//      FloydAlgorithmRouteGuide.cs
//*********************************************************************************
// File Name: FloydAlgorithmRouteGuide.cs
// Description: 
//
//(c) Copyright 2014, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
// 2019/08/26    Kevin Wei      N/A            A0.01   由於發現在上報CEID =108時，
//                                                     會有多上報一層的問題，針對該問題進行修正該問題。
//**********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.stc.Common;
using com.mirle.ibg3k0.stc.Data.SecsData;

namespace com.mirle.ibg3k0.sc.Data.SECS.OHTC.AT_S
{
    /// <summary>
    /// Event Report Send
    /// </summary>
    [Serializable]
    public class S6F11 : SXFY
    {
        [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_4_BYTE_UNSIGNED_INTEGER, Length = 1)]
        public string DATAID;
        [SecsElement(Index = 2, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
        public string CEID;
        [SecsElement(Index = 3)]
        public RPTINFO INFO;

        public S6F11()
        {
            StreamFunction = "S6F11";
            StreamFunctionName = "Transfer Event repor";
            W_Bit = 1;
            IsBaseType = true;
            INFO = new RPTINFO();
        }
        public override string ToString()
        {
            return string.Concat(StreamFunction, "-", CEID);
        }

        [Serializable]
        public class RPTINFO : SXFY
        {
            [SecsElement(Index = 1, ListSpreadOut = true)]
            public RPTITEM[] ITEM;
            [Serializable]
            public class RPTITEM : SXFY
            {

                [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                public string RPTID;
                [SecsElement(Index = 2)]
                public SXFY[] VIDITEM;
                [Serializable]
                public class VIDITEM_04 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, ListElementType = SecsElement.SecsElementType.TYPE_4_BYTE_UNSIGNED_INTEGER, ListElementLength = 1)]
                    public string[] ALIDs;
                }
                [Serializable]
                public class VIDITEM_06 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string CONTROLSTATE;
                }
                [Serializable]
                public class VIDITEM_10 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public ENHANCEDCARRIERINFO EnhancedCarrierinfo = new ENHANCEDCARRIERINFO();
                }
                [Serializable]
                public class ENHANCEDCARRIERINFO : SXFY
                {
                    [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string CarrierID;
                    [SecsElement(Index = 2, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string CarrierLoc;
                    [SecsElement(Index = 3, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string CarrierZoneName;
                    [SecsElement(Index = 4, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 16)]
                    public string InstallTime;
                    [SecsElement(Index = 5, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string CarrierState;
                    //[SecsElement(Index = 6, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    //public string CarrierType;
                    //[SecsElement(Index = 7, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    //public string LotID;
                }
                [Serializable]
                public class VIDITEM_12 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 12)]
                    public string InstallTime;
                }
                [Serializable]
                public class VIDITEM_13 : SXFY
                {
                    //[SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string TransferState;
                    [SecsElement(Index = 2)]
                    public COMMANDINFO CommandInfo = new COMMANDINFO();
                    [SecsElement(Index = 3)]
                    public TRANSFERINFO TransferInfo = new TRANSFERINFO();
                }
                [Serializable]
                public class VIDITEM_16 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string CarrierType;
                }

                [Serializable]
                public class VIDITEM_17 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string LotID;
                }

                [Serializable]
                public class VIDITEM_40 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public ENHANCEDALID[] ENHANCED_ALIDs;
                }
                [Serializable]
                public class VIDITEM_41 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public ENHANCEDALID ENHANCED_ALID;
                }
                [Serializable]
                public class ENHANCEDALID : SXFY
                {
                    [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string ALID;
                    [SecsElement(Index = 2, ListSpreadOut = true)]
                    public VEHICLEINFO VehicleInfo = new VEHICLEINFO();
                    [SecsElement(Index = 3, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 128)]
                    public string AlarmText;
                }

                [Serializable]
                public class VIDITEM_50 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string NewCarrierID;
                }
                [Serializable]
                public class VIDITEM_51 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public ENHANCEDCARRIERINFO[] ENHANCED_CARRIER_INFO;
                }
                [Serializable]
                public class VIDITEM_53 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public VEHICLEINFO[] VehicleInfos;
                }
                [Serializable]
                public class VIDITEM_54 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string CarrierID;
                }
                [Serializable]
                public class VIDITEM_56 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string CarrierLoc;
                }
                [Serializable]
                public class VIDITEM_58 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string CommandID;
                }
                [Serializable]
                public class VIDITEM_59 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public COMMANDINFO CommandInfo = new COMMANDINFO();
                }
                [Serializable]
                public class COMMANDINFO : SXFY
                {
                    [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string CommandID;
                    [SecsElement(Index = 2, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string Priority;
                    [SecsElement(Index = 3, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string Replace;
                }
                [Serializable]
                public class VIDITEM_60 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string Dest;
                }
                [Serializable]
                public class VIDITEM_61 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string EqpName;
                }
                [Serializable]
                public class VIDITEM_62 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string Priority;
                }
                [Serializable]
                public class VIDITEM_64 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string ResultCode;
                }
                [Serializable]
                public class VIDITEM_65 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string Source;
                }
                [Serializable]
                public class VIDITEM_66 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string HandoffType;
                }
                [Serializable]
                public class VIDITEM_67 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string IDreadStatus;
                }
                [Serializable]
                public class VIDITEM_70 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 32)]
                    public string VehicleID;
                }
                [Serializable]
                public class VIDITEM_71 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public VEHICLEINFO VehicleInfo = new VEHICLEINFO();
                }
                [Serializable]
                public class VEHICLEINFO : SXFY
                {
                    [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 32)]
                    public string VehicleID;
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string VehicleState;
                }
                [Serializable]
                public class VIDITEM_72 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string VehicleState;
                }
                [Serializable]
                public class VIDITEM_73 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string TSCSTATE;
                }
                [Serializable]
                public class VIDITEM_76 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public VIDITEM_13[] EnhancedTransferCmd;
                }
                [Serializable]
                public class VIDITEM_77 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public TRANSFERCOMPLETEINFO TransferCompleteInfo = new TRANSFERCOMPLETEINFO();
                }
                [Serializable]
                public class TRANSFERCOMPLETEINFO : SXFY
                {
                    [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public TRANSFERINFO TransferInfo = new TRANSFERINFO();
                    [SecsElement(Index = 2, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string CarrierLoc;
                }
                [Serializable]
                public class VIDITEM_80 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string CommandType;
                }
                [Serializable]
                public class VIDITEM_81 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string AlarmID;
                }
                [Serializable]
                public class VIDITEM_82 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 128)]
                    public string AlarmText;
                }
                [Serializable]
                public class VIDITEM_83 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string UnitID;
                }
                [Serializable]
                public class VIDITEM_84 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public TRANSFERINFO TransferInfo = new TRANSFERINFO();
                }
                [Serializable]
                public class TRANSFERINFO : SXFY
                {
                    [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string CarrierID;
                    [SecsElement(Index = 2, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string SourcePort;
                    [SecsElement(Index = 3, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string DestPort;
                }
                [Serializable]
                public class VIDITEM_101 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string BatteryValue;
                }
                [Serializable]
                public class VIDITEM_102 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string VehicleLastPosition;
                }
                [Serializable]
                public class VIDITEM_114 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 20)]
                    public string SpecVersion;
                }
                [Serializable]
                public class VIDITEM_115 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string PortID;
                }
                [Serializable]
                public class VIDITEM_117 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string VichicleLocation;
                }
                [Serializable]
                public class VIDITEM_118 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public PORTTINFO[] PortInfos;
                }
                [Serializable]
                public class VIDITEM_203 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string CarrierState;
                }
                [Serializable]
                public class VIDITEM_350 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public EQPORTINFO[] EqPortInfos;
                }
                [Serializable]
                public class VIDITEM_351 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public PORTTYPEINFO[] PortTypes;
                }
                [Serializable]
                public class PORTTYPEINFO : SXFY
                {
                    [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string PortID;
                    [SecsElement(Index = 2, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string PortUnitType;
                }
                [Serializable]
                public class VIDITEM_352 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string EqReqSatus;
                }
                [Serializable]
                public class VIDITEM_353 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string EqPresenceStatus;
                }
                [Serializable]
                public class VIDITEM_354 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public PORTTINFO PortInfo = new PORTTINFO();
                }
                [Serializable]
                public class PORTTINFO : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string PortID;
                    [SecsElement(Index = 2, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string PortTransferState;
                }
                [Serializable]
                public class VIDITEM_355 : SXFY
                {
                    [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string PortTransferState;
                }
                [Serializable]
                public class VIDITEM_356 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public EQPORTINFO PortInfo = new EQPORTINFO();
                }
                [Serializable]
                public class EQPORTINFO : SXFY
                {
                    [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string PortID;
                    [SecsElement(Index = 2, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string PortTransferState;
                    [SecsElement(Index = 3, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string EqReqSatus;
                    [SecsElement(Index = 4, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string EqPresenceStatus;
                }
                [Serializable]
                public class VIDITEM_360 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public UNITALARMINFO[] UnitAlarmInfos;
                }
                [Serializable]
                public class VIDITEM_361 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public UNITALARMINFO UnitAlarmInfo = new UNITALARMINFO();
                }
                [Serializable]
                public class UNITALARMINFO : SXFY
                {
                    [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string UnitID;
                    [SecsElement(Index = 2, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string AlarmID;
                    [SecsElement(Index = 3, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 128)]
                    public string AlarmText;
                    [SecsElement(Index = 4, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string MaintState;
                }
                [Serializable]
                public class VIDITEM_362 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string MaintState;
                }
                [Serializable]
                public class VIDITEM_363 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string VehicleCurrentPosition;
                }
                [Serializable]
                public class VIDITEM_370 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string CarrierZoneName;
                }
                [Serializable]
                public class VIDITEM_722 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string TransferState;
                }
                [Serializable]
                public class VIDITEM_723 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public MONITOREDVEHICLEINFO[] MonitoredVehicles = new MONITOREDVEHICLEINFO[0];
                }
                [Serializable]
                public class VIDITEM_724 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public MONITOREDVEHICLEINFO MonitoredVehicleInfo = new MONITOREDVEHICLEINFO();
                }
                [Serializable]
                public class VIDITEM_725 : SXFY
                {
                    [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string VehicleNextPosition;
                }
                [Serializable]
                public class VIDITEM_726 : SXFY
                {
                    [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string VehiclecCommunication;
                }
                [Serializable]
                public class VIDITEM_727 : SXFY
                {
                    [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string VehcileControlMode;
                }
                [Serializable]
                public class VIDITEM_728 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public PORTLOCATIONINFO[] PortsLocationList = new PORTLOCATIONINFO[0];
                }

                [Serializable]
                public class VIDITEM_729 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public PORTLOCATIONINFO PortLocationInfo = new PORTLOCATIONINFO();
                }

                [Serializable]
                public class VIDITEM_730 : SXFY
                {
                    [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string PortPosition;
                }

                [Serializable]
                public class PORTLOCATIONINFO : SXFY
                {
                    [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string PortID;
                    [SecsElement(Index = 2, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string PortPosition;
                }

                [Serializable]
                public class MONITOREDVEHICLEINFO : SXFY
                {
                    [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 32)]
                    public string VehicleID;
                    [SecsElement(Index = 2, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string VehicleLastPosition;
                    [SecsElement(Index = 3, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string VehicleCurrentPosition;
                    [SecsElement(Index = 4, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string VehicleNextPosition;
                    [SecsElement(Index = 5, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string VehicleStatus;
                    [SecsElement(Index = 6, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string VehiclecCommunication;
                    [SecsElement(Index = 7, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string VehcileControlMode;
                }
            }
        }
    }
    public class VIDCollection
    {
        public VIDCollection()
        {
            VID_06_ControlState = new S6F11.RPTINFO.RPTITEM.VIDITEM_06();
            VID_10_EnhancedCarriersInfo = new S6F11.RPTINFO.RPTITEM.VIDITEM_10();
            VID_12_InstallTime = new S6F11.RPTINFO.RPTITEM.VIDITEM_12();
            VID_13_EnhancedTransferCmd = new S6F11.RPTINFO.RPTITEM.VIDITEM_13();
            VID_16_CarrierType = new S6F11.RPTINFO.RPTITEM.VIDITEM_16();
            VID_17_LotID = new S6F11.RPTINFO.RPTITEM.VIDITEM_17();
            VID_50_NewCarrierID = new S6F11.RPTINFO.RPTITEM.VIDITEM_50();
            VID_54_CarrierID = new S6F11.RPTINFO.RPTITEM.VIDITEM_54();
            VID_56_CarrierLoc = new S6F11.RPTINFO.RPTITEM.VIDITEM_56();
            VID_58_CommandID = new S6F11.RPTINFO.RPTITEM.VIDITEM_58();
            VID_59_CommandInfo = new S6F11.RPTINFO.RPTITEM.VIDITEM_59();
            VID_60_Dest = new S6F11.RPTINFO.RPTITEM.VIDITEM_60();
            VID_61_EqpName = new S6F11.RPTINFO.RPTITEM.VIDITEM_61();
            VID_62_Priority = new S6F11.RPTINFO.RPTITEM.VIDITEM_62();
            VID_64_ResultCode = new S6F11.RPTINFO.RPTITEM.VIDITEM_64();
            VID_65_Source = new S6F11.RPTINFO.RPTITEM.VIDITEM_65();
            VID_66_HandoffType = new S6F11.RPTINFO.RPTITEM.VIDITEM_66();
            VID_67_IDreadStatus = new S6F11.RPTINFO.RPTITEM.VIDITEM_67();
            VID_70_VehicleID = new S6F11.RPTINFO.RPTITEM.VIDITEM_70();
            VID_71_VehicleInfo = new S6F11.RPTINFO.RPTITEM.VIDITEM_71();
            VID_72_VehicleState = new S6F11.RPTINFO.RPTITEM.VIDITEM_72();
            VID_77_TransferCompleteInfo = new S6F11.RPTINFO.RPTITEM.VIDITEM_77();
            VID_80_CommandType = new S6F11.RPTINFO.RPTITEM.VIDITEM_80();
            VID_81_AlarmID = new S6F11.RPTINFO.RPTITEM.VIDITEM_81();
            VID_82_AlarmText = new S6F11.RPTINFO.RPTITEM.VIDITEM_82();
            VID_83_UnitID = new S6F11.RPTINFO.RPTITEM.VIDITEM_83();
            VID_84_TransferInfo = new S6F11.RPTINFO.RPTITEM.VIDITEM_84();
            VID_101_BatteryValue = new S6F11.RPTINFO.RPTITEM.VIDITEM_101();
            VID_102_VehicleLastPosition = new S6F11.RPTINFO.RPTITEM.VIDITEM_102();
            VID_114_SpecVersion = new S6F11.RPTINFO.RPTITEM.VIDITEM_114();
            VID_115_PortID = new S6F11.RPTINFO.RPTITEM.VIDITEM_115();
            VID_117_VichicleLocation = new S6F11.RPTINFO.RPTITEM.VIDITEM_117();
            VID_203_CarrierState = new S6F11.RPTINFO.RPTITEM.VIDITEM_203();
            VID_353_EqPresenceStatus = new S6F11.RPTINFO.RPTITEM.VIDITEM_353();
            VID_354_PortInfo = new S6F11.RPTINFO.RPTITEM.VIDITEM_354();
            VID_355_PortTransferState = new S6F11.RPTINFO.RPTITEM.VIDITEM_355();
            VID_361_UnitAlarmInfo = new S6F11.RPTINFO.RPTITEM.VIDITEM_361();
            VID_362_MaintState = new S6F11.RPTINFO.RPTITEM.VIDITEM_362();
            VID_363_VehicleCurrentPosition = new S6F11.RPTINFO.RPTITEM.VIDITEM_363();
            VID_370_CarrierZoneName = new S6F11.RPTINFO.RPTITEM.VIDITEM_370();
            VID_722_TransferState = new S6F11.RPTINFO.RPTITEM.VIDITEM_722();
            VID_723_MonitoredVehicles = new S6F11.RPTINFO.RPTITEM.VIDITEM_723();
            VID_724_MonitoredVehicleInfo = new S6F11.RPTINFO.RPTITEM.VIDITEM_724();
            VID_725_VehicleNextPosition = new S6F11.RPTINFO.RPTITEM.VIDITEM_725();
            VID_726_VehiclecCommunication = new S6F11.RPTINFO.RPTITEM.VIDITEM_726();
            VID_727_VehcileControlMode = new S6F11.RPTINFO.RPTITEM.VIDITEM_727();


        }
        public string VH_ID;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_06 VID_06_ControlState;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_10 VID_10_EnhancedCarriersInfo;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_12 VID_12_InstallTime;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_13 VID_13_EnhancedTransferCmd;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_16 VID_16_CarrierType;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_17 VID_17_LotID;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_50 VID_50_NewCarrierID;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_54 VID_54_CarrierID;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_56 VID_56_CarrierLoc;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_58 VID_58_CommandID;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_59 VID_59_CommandInfo;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_60 VID_60_Dest;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_61 VID_61_EqpName;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_62 VID_62_Priority;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_64 VID_64_ResultCode;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_65 VID_65_Source;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_66 VID_66_HandoffType;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_67 VID_67_IDreadStatus;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_70 VID_70_VehicleID;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_71 VID_71_VehicleInfo;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_72 VID_72_VehicleState;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_77 VID_77_TransferCompleteInfo;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_80 VID_80_CommandType;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_81 VID_81_AlarmID;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_82 VID_82_AlarmText;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_83 VID_83_UnitID;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_84 VID_84_TransferInfo;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_101 VID_101_BatteryValue;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_102 VID_102_VehicleLastPosition;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_114 VID_114_SpecVersion;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_115 VID_115_PortID;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_117 VID_117_VichicleLocation;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_203 VID_203_CarrierState;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_353 VID_353_EqPresenceStatus;

        public S6F11.RPTINFO.RPTITEM.VIDITEM_354 VID_354_PortInfo;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_355 VID_355_PortTransferState;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_361 VID_361_UnitAlarmInfo;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_362 VID_362_MaintState;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_363 VID_363_VehicleCurrentPosition;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_370 VID_370_CarrierZoneName;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_722 VID_722_TransferState;

        public S6F11.RPTINFO.RPTITEM.VIDITEM_723 VID_723_MonitoredVehicles;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_724 VID_724_MonitoredVehicleInfo;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_725 VID_725_VehicleNextPosition;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_726 VID_726_VehiclecCommunication;
        public S6F11.RPTINFO.RPTITEM.VIDITEM_727 VID_727_VehcileControlMode;
    }

}
