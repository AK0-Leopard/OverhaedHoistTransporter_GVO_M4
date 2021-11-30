//*********************************************************************************
//      SCAppConstants.cs
//*********************************************************************************
// File Name: SCAppConstants.cs
// Description: SCAppConstants
//
//(c) Copyright 2014, MIRLE Automation Corporation
//
// Date                Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
//**********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.sc.Data.SECS;
using System.Diagnostics;
using System.Reflection;

using System.Transactions;

namespace com.mirle.ibg3k0.sc.App
{
    /// <summary>
    /// Class SCAppConstants.
    /// </summary>
    public class SCAppConstants
    {
        public const string System_ID = "OHxC";

        #region ECID
        //String.Format("{0:00000}", value)
        /// <summary>
        /// The eci d_ format
        /// </summary>
        public static readonly string ECID_Format = "{0:0000}";
        /// <summary>
        /// The eci d_ contro l_ stat e_ keepin g_ time
        /// </summary>
        public static readonly string ECID_CONTROL_STATE_KEEPING_TIME = "0001";
        /// <summary>
        /// The eci d_ ge m_ initia l_ contro l_ state
        /// </summary>
        public static readonly string ECID_GEM_INITIAL_CONTROL_STATE = "0003";
        /// <summary>
        /// The eci d_ devic e_ identifier
        /// </summary>
        public static readonly string ECID_DEVICE_ID = "0004";
        /// <summary>
        /// The eci d_ heartbeat
        /// </summary>
        public static readonly string ECID_HEARTBEAT = "0005";
        /// <summary>
        /// The eci d_ t3
        /// </summary>
        public static readonly string ECID_T3 = "0006";
        /// <summary>
        /// The eci d_ t5
        /// </summary>
        public static readonly string ECID_T5 = "0007";
        /// <summary>
        /// The eci d_ t6
        /// </summary>
        public static readonly string ECID_T6 = "0008";
        /// <summary>
        /// The eci d_ t7
        /// </summary>
        public static readonly string ECID_T7 = "0009";
        /// <summary>
        /// The eci d_ t8
        /// </summary>
        public static readonly string ECID_T8 = "0010";
        /// <summary>
        /// The eci d_ conversatio n_ timeout
        /// </summary>
        public static readonly string ECID_CONVERSATION_TIMEOUT = "0011";

        /// <summary>
        /// The syste m_ defaul t_ ecid
        /// </summary>
        public static readonly string[] SYSTEM_DEFAULT_ECID =
        {
            ECID_CONTROL_STATE_KEEPING_TIME,
            ECID_GEM_INITIAL_CONTROL_STATE,
            ECID_DEVICE_ID,
            ECID_HEARTBEAT,
            ECID_T3,
            ECID_T5,
            ECID_T6,
            ECID_T7,
            ECID_T8,
            ECID_CONVERSATION_TIMEOUT,
        };
        #endregion ECID

        #region Config Section Handler Name
        public static readonly string CONFIG_DATA_COLLECTION_SETTING = "DataCollectionSetting";
        #endregion

        #region SECS DATAID
        /// <summary>
        /// 用於標示SECS 的Dataa ID最大值
        /// </summary>
        public static readonly int MAX_DATA_ID = 9999;
        /// <summary>
        /// 用於標示SECS 的Dataa ID的長度
        /// </summary>
        public static readonly int DATA_ID_LENGTH = 4;
        #endregion SECS DATAID





        public static readonly IsolationLevel ISOLATION_LEVEL = IsolationLevel.ReadCommitted;
        /// <summary>
        /// The cei d_ al l_ ceid
        /// </summary>
        public static readonly string CEID_ALL_CEID = SECSConst.CEID_ALL_CEID;
        /// <summary>
        /// The alar m_ al l_ alarmid
        /// </summary>
        public static readonly string ALARM_ALL_ALARMID = "0000000000";

        /// <summary>
        /// The ye s_ flag
        /// </summary>
        public static readonly string YES_FLAG = "Y";
        /// <summary>
        /// The n o_ flag
        /// </summary>
        public static readonly string NO_FLAG = "N";

        /// <summary>
        /// The o_ flag
        /// </summary>
        public static readonly string O_FLAG = "O";
        /// <summary>
        /// The x_ flag
        /// </summary>
        public static readonly string X_FLAG = "X";

        /// <summary>
        /// The timestamp format_10
        /// </summary>
        public static readonly string TimestampFormat_12 = "yyMMddHHmmss";
        /// <summary>
        /// The timestamp format_19
        /// </summary>
        public static readonly string TimestampFormat_19 = "yyyyMMddHHmmssfffff";
        /// <summary>
        /// The timestamp format_19
        /// </summary>
        public static readonly string TimestampFormat_18 = "yyyyMMddHHmmssffff";
        /// <summary>
        /// The timestamp format_17
        /// </summary>
        public static readonly string TimestampFormat_17 = "yyyyMMddHHmmssfff";
        /// <summary>
        /// The timestamp format_16
        /// </summary>
        public static readonly string TimestampFormat_16 = "yyyyMMddHHmmssff";
        /// <summary>
        /// The timestamp format_14
        /// </summary>
        public static readonly string TimestampFormat_14 = "yyyyMMddHHmmss";
        public static readonly string TimestampFormat_08 = "yyyyMMdd";
        /// <summary>
        /// The date time format_22
        /// </summary>
        public static readonly string DateTimeFormat_19 = "yyyy-MM-dd HH:mm:ss";
        public static readonly string DateTimeFormat_22 = "yyyy-MM-dd HH:mm:ss.ff";
        /// <summary>
        /// The date time format_11
        /// </summary>
        public static readonly string DateTimeFormat_11 = "HH:mm:ss.ff";//A0.11
        public static readonly string DateTimeFormat_23 = "yyyy-MM-dd HH:mm:ss.fff";

        //Sequence Name
        /// <summary>
        /// The sequenc e_ nam e_ cs t_ sequence
        /// </summary>
        public static readonly string SEQUENCE_NAME_CST_SEQUENCE = "CST_SEQ";
        public static readonly string SEQUENCE_NAME_COMMANDID_MANUAL = "CMD_MANUAL_SEQ";

        /// <summary>
        /// The cs t_ sequenc e_ numbe r_ length
        /// </summary>
        public static readonly int CST_SEQUENCE_NUMBER_LENGTH = 4;     //0000 ~ 1023
        /// <summary>
        /// The cs t_ sequenc e_ numbe r_ bi t_ length
        /// </summary>
        public static readonly int CST_SEQUENCE_NUMBER_BIT_LENGTH = 10;//0000000000~1111111111 (0000 ~ 1023)
        /// <summary>
        /// The cs t_ sequenc e_ numbe r_ maximum
        /// </summary>
        public static readonly int CST_SEQUENCE_NUMBER_MAX = 1023;
        /// <summary>
        /// The cs t_ sequenc e_ numbe r_ minimum
        /// </summary>
        public static readonly int CST_SEQUENCE_NUMBER_MIN = 0;


        public enum GenOHxCCommandType
        {
            Auto = 1,
            Manual = 2,
            Debug = 3
        }
        public static readonly int COMMANDID_MANUAL_NUMBER_LENGTH = 4;     //0000 ~ 9999
        public static readonly int COMMANDID_MANUAL_NUMBER_MAX = 9999;
        public static readonly int COMMANDID_MANUAL_NUMBER_MIN = 1;


        public static readonly int FIRST_PARK_PRIO = 11;
        public static readonly int LOCK_TIMEOUT_MS = 30000;
        public static readonly int POSITION_REFRESH_INTERVAL_TIME = 500;

        #region Config Use
        /////////////////////////////////Config Use Begin////////////////////////////////////////
        #region Unit Cate
        /******************* Unit Cate *********************/
        /// <summary>
        /// The uni t_ cat e_ VCR
        /// </summary>
        public static readonly string UNIT_CATE_VCR = "V";
        /// <summary>
        /// The uni t_ cat e_ port
        /// </summary>
        public static readonly string UNIT_CATE_PORT = "O";
        /// <summary>
        /// The uni t_ cat e_ stage
        /// </summary>
        public static readonly string UNIT_CATE_STAGE = "S";
        /// <summary>
        /// The uni t_ cat e_ buffer
        /// </summary>
        public static readonly string UNIT_CATE_BUFFER = "B";
        /// <summary>
        /// The uni t_ cat e_ robot
        /// </summary>
        public static readonly string UNIT_CATE_ROBOT = "R";
        /// <summary>
        /// The uni t_ cat e_ conveyer
        /// </summary>
        public static readonly string UNIT_CATE_CONVEYER = "C";
        /// <summary>
        /// The uni t_ cat e_ lift
        /// </summary>
        public static readonly string UNIT_CATE_LIFT = "L";
        /// <summary>
        /// The uni t_ cat e_ process
        /// </summary>
        public static readonly string UNIT_CATE_PROCESS = "P";
        /// <summary>
        /// The uni t_ cat e_ list
        /// </summary>
        public static readonly List<string> UNIT_CATE_LIST = new List<string>
        {
            UNIT_CATE_VCR,
            UNIT_CATE_PORT,
            UNIT_CATE_STAGE,
            UNIT_CATE_BUFFER,
            UNIT_CATE_ROBOT,
            UNIT_CATE_CONVEYER,
            UNIT_CATE_LIFT,
            UNIT_CATE_PROCESS
        };
        /***************************************************/
        #endregion Unit Cate

        #region EQPT Type
        /******************* EQPT Type *********************/
        /// <summary>
        /// The eqp t_ typ e_ transfer
        /// </summary>
        public static readonly string EQPT_TYPE_TRANSFER = "T";
        /// <summary>
        /// The eqp t_ typ e_ process
        /// </summary>
        public static readonly string EQPT_TYPE_PROCESS = "P";
        /// <summary>
        /// The eqp t_ typ e_ measurement
        /// </summary>
        public static readonly string EQPT_TYPE_MEASUREMENT = "M";
        /// <summary>
        /// The eqp t_ typ e_ list
        /// </summary>
        public static readonly List<string> EQPT_TYPE_LIST = new List<string>
        {
            EQPT_TYPE_TRANSFER,
            EQPT_TYPE_PROCESS,
            EQPT_TYPE_MEASUREMENT
        };
        /***************************************************/
        #endregion EQPT Type

        #region Port Type
        /******************* Port Type *********************/
        /// <summary>
        /// The por t_ typ e_ load
        /// </summary>
        public static readonly string PORT_TYPE_LOAD = "L";
        /// <summary>
        /// The por t_ typ e_ unload
        /// </summary>
        public static readonly string PORT_TYPE_UNLOAD = "U";
        /// <summary>
        /// The por t_ typ e_ commo n_ loa d_ unload
        /// </summary>
        public static readonly string PORT_TYPE_COMMON_LOAD_UNLOAD = "C";
        /// <summary>
        /// The por t_ typ e_ MGV
        /// </summary>
        public static readonly string PORT_TYPE_MGV = "M";
        /// <summary>
        /// The por t_ typ e_ list
        /// </summary>
        public static readonly List<string> PORT_TYPE_LIST = new List<string>
        {
            PORT_TYPE_LOAD,
            PORT_TYPE_UNLOAD,
            PORT_TYPE_COMMON_LOAD_UNLOAD,
            PORT_TYPE_MGV
        };
        /***************************************************/
        #endregion Port Type

        #region Relation Type
        /***************** Relation Type *******************/
        /// <summary>
        /// The re l_ typ e_ node
        /// </summary>
        public static readonly string REL_TYPE_NODE = "N";
        /***************************************************/
        #endregion Relation Type

        #region EQPT Object Cate
        /**************** EQPT Object Cate *****************/
        /// <summary>
        /// The eqp t_ objec t_ cat e_ line
        /// </summary>
        public static readonly string EQPT_OBJECT_CATE_LINE = "Line";
        /// <summary>
        /// The eqp t_ objec t_ cat e_ zone
        /// </summary>
        public static readonly string EQPT_OBJECT_CATE_ZONE = "Zone";
        /// <summary>
        /// The eqp t_ objec t_ cat e_ node
        /// </summary>
        public static readonly string EQPT_OBJECT_CATE_NODE = "Node";
        /// <summary>
        /// The eqp t_ objec t_ cat e_ eqpt
        /// </summary>
        public static readonly string EQPT_OBJECT_CATE_EQPT = "Equipment";
        /// <summary>
        /// The eqp t_ objec t_ cat e_ unit
        /// </summary>
        public static readonly string EQPT_OBJECT_CATE_UNIT = "Unit";
        /// <summary>
        /// The eqp t_ objec t_ cat e_ port
        /// </summary>
        public static readonly string EQPT_OBJECT_CATE_PORT = "Port";
        /// <summary>
        /// The eqp t_ objec t_ cat e_ buffer
        /// </summary>
        public static readonly string EQPT_OBJECT_CATE_BUFFER = "BufferPort";
        /***************************************************/
        #endregion EQPT Object Cate
        /////////////////////////////////Config Use End//////////////////////////////////////////
        #endregion Config Use


        /******************* BC Status *********************/
        /// <summary>
        /// Class BCSystemStatus.
        /// </summary>
        public class BCSystemStatus
        {
            /// <summary>
            /// The default
            /// </summary>
            public static readonly string Default = "0";
            /// <summary>
            /// The normal closed
            /// </summary>
            public static readonly string NormalClosed = "1";
        }

        /// <summary>
        /// Enum BCSystemInitialRtnCode
        /// </summary>
        public enum BCSystemInitialRtnCode
        {
            /// <summary>
            /// The normal
            /// </summary>
            Normal = 0,
            /// <summary>
            /// The error
            /// </summary>
            Error = 1,
            /// <summary>
            /// The non normal shutdown
            /// </summary>
            NonNormalShutdown = 2
        }
        /***************************************************/


        #region current work version
        public class WorkVersion
        {
            public const string VERSION_NAME_ASE_K21 = "ASE_K21";
            public const string VERSION_NAME_ASE_K21_DEMO = "ASE_K21_DEMO";
            public const string VERSION_NAME_ASE_K21_TEST = "ASE_K21_test";
            public const string VERSION_NAME_UMTC = "UMTC";
            public const string VERSION_NAME_PTI = "PTI_AGVC_TEST";
            public const string VERSION_NAME_CHOT = "CHOT";
            //public const string VERSION_NAME_AT_S_OHTC = "AT_S_OHTC";
            public const string VERSION_NAME_AT_S_OHTC_301 = "AT_S_OHTC_301";
            public const string VERSION_NAME_AT_S_OHTC_302 = "AT_S_OHTC_302";
            public const string VERSION_NAME_M4_DEMO_HL = "M4_DEMO_HL";
        }

        #endregion current work version

        #region Alarm
        /*****************Alarm Source Type*****************/
        /// <summary>
        /// Class AlarmSourceType.
        /// </summary>
        public class AlarmSourceType
        {
            /// <summary>
            /// The line
            /// </summary>
            public static readonly string Line = "1";
            /// <summary>
            /// The zone
            /// </summary>
            public static readonly string Zone = "2";
            /// <summary>
            /// The node
            /// </summary>
            public static readonly string Node = "3";
            /// <summary>
            /// The eqpt
            /// </summary>
            public static readonly string EQPT = "4";
            /// <summary>
            /// The unit
            /// </summary>
            public static readonly string Unit = "5";
            /// <summary>
            /// The port
            /// </summary>
            public static readonly string Port = "6";
            /// <summary>
            /// The buffer
            /// </summary>
            public static readonly string Buffer = "7";
        }
        /***************************************************/
        /********************Alarm State********************/
        /// <summary>
        /// Class EQAlarmStatus.
        /// </summary>
        public class EQAlarmStatus
        {
            /// <summary>
            /// The on
            /// </summary>
            public static readonly string On = "1";
            /// <summary>
            /// The off
            /// </summary>
            public static readonly string Off = "2";
        }
        /********************Alarm State********************/
        /// <summary>
        /// Class AlarmStatus.
        /// </summary>
        public class AlarmStatus
        {
            public static string convert2MCS(ProtocolFormat.OHTMessage.ErrorStatus alarm_status)
            {
                switch (alarm_status)
                {
                    case ProtocolFormat.OHTMessage.ErrorStatus.ErrSet:
                        return SECSConst.ALCD_Alarm_Set;
                    case ProtocolFormat.OHTMessage.ErrorStatus.ErrReset:
                        return SECSConst.ALCD_Alarm_Clear;
                    default:
                        return "";
                }
            }
        }
        /***************************************************/

        /********************Alarm Level********************/
        /// <summary>
        /// Class AlarmLevel.
        /// </summary>
        public class AlarmLevel
        {
            /// <summary>
            /// The warning
            /// </summary>
            public static readonly int Warning = 1;
            /// <summary>
            /// The alarm
            /// </summary>
            public static readonly int Alarm = 2;
        }
        /***************************************************/
        public class SystemAlarmCode
        {
            public class OHCV_Issue
            {
                public const string CVALLAlarmReset = "0";
                public const string CVDoorAbnormallyOpen = "1";
                public const string CVOfAliveSignalAbnormal = "2";
            }
            public class OHT_Issue
            {
                public const string RejectCommandAlarm = "99990";
                public const string OHTAccidentOfflineWarning = "99999";
                public const string DoubleStorage = "89000";
                public const string EmptyRetrieval = "89001";
            }
            public class PLC_Issue
            {
                public const string FireAlarm = "70000";
                public const string MasterDisconnedted = "70001";
            }
        }
        #endregion Alarm

        /********************Link Status********************/
        /// <summary>
        /// Class LinkStatus.
        /// </summary>
        public enum LinkStatus
        {
            LinkFail,
            LinkOK
        }
        /***************************************************/

        /********************Exist Status********************/
        /// <summary>
        /// Class LinkStatus.
        /// </summary>
        public enum ExistStatus
        {
            NoExist,
            Exist
        }
        /***************************************************/
        #region Line

        public class LineHostControlState
        {
            public enum HostControlState
            {
                [System.ComponentModel.DataAnnotations.Display(Name = "O")]
                EQ_Off_line,
                [System.ComponentModel.DataAnnotations.Display(Name = "Going")]
                Going_Online,
                [System.ComponentModel.DataAnnotations.Display(Name = "R")]
                Host_Offline,
                [System.ComponentModel.DataAnnotations.Display(Name = "L")]
                On_Line_Local,
                [System.ComponentModel.DataAnnotations.Display(Name = "R")]
                On_Line_Remote
            }
            public static string convert2MES(HostControlState hostMode)
            {
                if (hostMode == HostControlState.EQ_Off_line)
                {
                    return SECSConst.HostCrtMode_EQ_Off_line;
                }
                else if (hostMode == HostControlState.Going_Online)
                {
                    return SECSConst.HostCrtMode_Going_Online;
                }
                else if (hostMode == HostControlState.Host_Offline)
                {
                    return SECSConst.HostCrtMode_Host_Offline;
                }
                else if (hostMode == HostControlState.On_Line_Local)
                {
                    return SECSConst.HostCrtMode_On_Line_Local;
                }
                else if (hostMode == HostControlState.On_Line_Remote)
                {
                    return SECSConst.HostCrtMode_On_Line_Remote;
                }
                return string.Empty;
            }
        }

        public class LineSCState
        {
            public enum SCState
            {
                [System.ComponentModel.DataAnnotations.Display(Name = "Init")]
                Init,
                [System.ComponentModel.DataAnnotations.Display(Name = "Paused")]
                Paused,
                [System.ComponentModel.DataAnnotations.Display(Name = "Auto")]
                Auto,
                [System.ComponentModel.DataAnnotations.Display(Name = "Pausing")]
                Pausing
            }

            public static string convert2MES(SCState hostMode)
            {
                if (hostMode == SCState.Init)
                {
                    return SECSConst.SCSTATE_Init;
                }
                else if (hostMode == SCState.Paused)
                {
                    return SECSConst.SCSTATE_Paused;
                }
                else if (hostMode == SCState.Auto)
                {
                    return SECSConst.SCSTATE_Auto;
                }
                else if (hostMode == SCState.Pausing)
                {
                    return SECSConst.SCSTATE_Pausing;
                }
                return string.Empty;
            }
        }
        #endregion Line



        #region Zone
        /// <summary>
        /// Class ZoneStatus.
        /// </summary>
        public class ZoneStatus
        {
            /// <summary>
            /// The idle
            /// </summary>
            public static readonly int IDLE = 1;
            /// <summary>
            /// The run
            /// </summary>
            public static readonly int RUN = 2;
            /// <summary>
            /// Down
            /// </summary>
            public static readonly int DOWN = 3;
        }
        /****************************************************/
        #endregion Zone

        #region Node
        /// <summary>
        /// Class NodeStatus.
        /// </summary>
        public class NodeStatus
        {
            /// <summary>
            /// The idle
            /// </summary>
            public static readonly int IDLE = 1;
            /// <summary>
            /// The run
            /// </summary>
            public static readonly int RUN = 2;
            /// <summary>
            /// Down
            /// </summary>
            public static readonly int DOWN = 3;
        }
        #endregion


        #region Port

        #endregion Port

        #region Buffer
        /// <summary>
        /// Class BufferStatus.
        /// </summary>
        public class BufferStatus
        {
            /// <summary>
            /// The idle
            /// </summary>
            public static readonly int IDLE = 1;
            /// <summary>
            /// The run
            /// </summary>
            public static readonly int RUN = 2;
            /// <summary>
            /// Down
            /// </summary>
            public static readonly int DOWN = 3;
        }
        #endregion

        #region Eqpt Type
        public enum EqptType
        {
            Orther = 0,
            AGVStation = 1,
            NTB = 2,
            Equipment = 3,
            IOPort = 4,
            MTL = 5,
            HID = 6
        }
        #endregion Eqpt Type


        #region Vehicle

        #endregion Vehicle

        #region Cmd Status
        public class TaskCmdStatus
        {
            public static readonly int Queue = 0;
            public static readonly int Initial = 1;
            public static readonly int CMDWriteToMPLC = 3;
            public static readonly int Finish = 15;
            public static readonly int CommandError_Finish = 16;
            public static readonly int InterlockError_Finish = 17;
            public static readonly int Abnormal_Finish = 18;
            public static readonly int Force_Finish = 19;
            public static readonly int Force_Finish_CstIDMissMatch = 20;
            public static readonly int Force_Finish_LFCStart = 21;
            public static readonly int Null;

            public static string convert2String(int status)
            {
                return convert2String(status.ToString());
            }
            public static string convert2String(string status)
            {
                return SCApplication.getMessageString(string.Format("TaskCommandStatus_{0}", status));
            }
        }


        #endregion Cmd Status

        #region Vehicle State Machine
        //public enum E_VH_STS
        //{
        //    None,

        //    Initial,
        //    DataSyncing,

        //    PowerOff,
        //    PowerOn,

        //    OperationMode,
        //    Manual,
        //    Auto,

        //    Warning,
        //    Alarm,
        //    Error
        //}
        //public enum E_VH_EVENT
        //{
        //    CompensationDataRep,
        //    CompensationDataError,

        //    doDataSync,
        //    //DataSyncEnd,
        //    DataSyncComplete,
        //    DataSyncFail,

        //    VHPowerStatChg_PowerOn,
        //    VHPowerStatChg_PowerOff,

        //    OperationToAuto,
        //    OperationToManual,

        //    AlarmHappend,
        //    ErrorHappend,

        //    AlarmClear,
        //    ErrorClear
        //}
        #endregion Vehicle State Machine
        #region Command State Machine
        public enum E_Cmd_STS
        {
            None,

            Sent,

            //AcceptedCmd,
            InExecution,
            LoadUnload,
            Load,
            Unload,
            Teaching,
            GripperTeaching,
            Cycling,
            HomeMove,
            Move,
            Rejected,


            Completed,
            Abort,
            Cancel,
            ForcedCmp,
            NormalCmp,
            Error
        }
        public enum E_Cmd_EVENT
        {
            HasBeenSent,

            CommandHasBeenAccepted,
            CommandWasRejected,

            HasBeenCompleted_Abort,
            HasBeenCompleted_Cancel,
            HasBeenCompleted_ForcedCmp,
            HasBeenCompleted_NormalCmp,
            HasBeenCompleted_Error,

            Finish
        }
        #endregion  Command State Machine

        #region MainAlarm
        public class MainAlarmCode
        {
            public const string VH_WAIT_REPLY_TIME_OUT_0_1_2 = "WE001";
            public const string VH_SEND_MSG_ERROR_0_1_2 = "WE002";
            public const string OHxC_BOLCKED_BY_THE_ERROR_VEHICLE_0_1 = "AE001";
        }
        #endregion MainAlarm

        #region Zabbix Server Info
        public class ZabbixServerInfo
        {
            //public static readonly string Name = "OHxC_Tester";
            public static readonly string ZABBIX_OHXC_IDLE_DIRTY_CAR = "ZABBIX_OHXC_IDLE_DIRTY_CAR";
            public static readonly string ZABBIX_OHXC_IDLE_CLEAR_CAR = "ZABBIX_OHXC_IDLE_CLEAR_CAR";
            public static readonly string ZABBIX_OHXC_IS_ACTIVE = "ZABBIX_OHXC_IS_ACTIVE";
            public static readonly string ZABBIX_OHXC_ALIVE = "ZABBIX_OHXC_ALIVE";
            public static readonly string ZABBIX_MCS_CMD_QUEUE = "ZABBIX_MCS_CMD_QUEUE";
            public static readonly string ZABBIX_MCS_CMD_RUNNING = "ZABBIX_MCS_CMD_RUNNING";
            public static readonly string ZABBIX_MCS_CMD_RECIVED_HOUR = "ZABBIX_MCS_CMD_RECIVED_HOUR";
            public static readonly string ZABBIX_MCS_CMD_FINISHED_HOUR = "ZABBIX_MCS_CMD_FINISHED_HOUR";
        }

        public class ZabbixOHxCAlive
        {
            public static readonly int ZABBIX_OHXC_ALIVE_CLOSE = 0;
            public static readonly int ZABBIX_OHXC_ALIVE_INITIAL = 1;
            public static readonly int ZABBIX_OHXC_ALIVE_HEARTBEAT_ON = 3;
            public static readonly int ZABBIX_OHXC_ALIVE_HEARTBEAT_OFF = 2;
        }


        #endregion Zabbix Server Info

        #region Redis Constants 
        public const string REDIS_KEY_WORD_POSITION_REPORT = "POSITION_REPORT";

        public const string REDIS_EVENT_KEY = "REDIS_EVENT_KEY";
        public const string REDIS_EVENT_CODE_ILLEGAL_ENTRY_BLOCK_ZONE = "0001";
        public const string REDIS_EVENT_CODE_ADVANCE_NOTICE_OBSTRUCT_VH = "0002";
        public const string REDIS_EVENT_CODE_VEHICLE_IDEL_WARNING = "0003";
        public const string REDIS_EVENT_CODE_VEHICLE_LOADUNLOAD_TOO_LONG_WARNING = "0004";
        public const string REDIS_EVENT_CODE_EARTHQUAKE_ON = "0005";
        public const string REDIS_EVENT_CODE_ADVANCE_NOTICE_OBSTRUCTED_VH = "0006";
        public const string REDIS_EVENT_CODE_NOTICE_OBSTRUCTED_VH_CONTINUE = "0007";
        public const string REDIS_EVENT_CODE_NOTICE_THE_VH_NEEDS_TO_CHANGE_THE_PATH = "0008";
        #endregion Redis Constants 

        public class ConnectionSetting
        {
            public const string REDIS_SERVER_CONFIGURATION = "redis.ohxc.mirle.com.tw:6379";
            //public const string REDIS_SERVER_CONFIGURATION = "redis.ohxc.mirle.com.tw:6379,syncTimeout =3000";
            //public const string REDIS_SERVER_CONFIGURATION = "localhost:6379,syncTimeout =3000";
        }


        public class BlockQueueState
        {
            public const string Request = "1";
            public const string Blocking = "2";
            public const string Through = "3";
            public const string Release = "4";
            public const string Abnormal_Release_OrtherNonRelease = "5";
            public const string Abnormal_Release_TimerCheck = "6";
            public const string Abnormal_Release_ForceRelease = "7";
        }

        public class TransferState
        {
            public static string convert2MES(E_TRAN_STATUS tran_status)
            {
                if (tran_status == E_TRAN_STATUS.Queue ||
                    tran_status == E_TRAN_STATUS.PreInitial)
                {
                    return SECSConst.TRANSFERSTATE_Queued;
                }
                else if (tran_status == E_TRAN_STATUS.Initial)
                {
                    return SECSConst.TRANSFERSTATE_Waiting;
                }
                else if (tran_status == E_TRAN_STATUS.Transferring)
                {
                    return SECSConst.TRANSFERSTATE_Transsfring;
                }
                else if (tran_status == E_TRAN_STATUS.Canceled)
                {
                    return SECSConst.TRANSFERSTATE_Canceling;
                }
                else
                {
                    return SECSConst.TRANSFERSTATE_Transsfring;
                }
            }
        }

        public enum MirleActiveType
        {
            Move = 0,
            Load = 1,
            Unload = 2,
            LoadUnload = 3,
            Teaching = 4,
            Override = 5,
            Rename = 6,
            MTLHome = 7,
            TechMove = 13,
        }
        public enum MirleCompleteStatus
        {
            Normal = 0,
            Cancel = 1,
            Abort = 2,
            IDMisMatch = 4,
            IDReadFailed = 5,
            VehicleAbort = 12,
            InterlockError = 64,

        }


        #region 頁面共用方法
        //================================================
        // 頁面共用方法
        //================================================

        //取得主頁面 版本號碼
        /// <summary>
        /// Gets the main form version.
        /// </summary>
        /// <param name="appendStr">The append string.</param>
        /// <returns>String.</returns>
        public static String getMainFormVersion(String appendStr)
        {
            return FileVersionInfo.GetVersionInfo(
                Assembly.GetExecutingAssembly().Location).FileVersion.ToString() + appendStr;
        }

        /// <summary>
        /// Gets the build date time.
        /// </summary>
        /// <returns>DateTime.</returns>
        public static DateTime GetBuildDateTime()
        {
            string filePath = System.Reflection.Assembly.GetCallingAssembly().Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;
            byte[] b = new byte[2048];
            System.IO.Stream s = null;

            try
            {
                s = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                }
            }

            int i = System.BitConverter.ToInt32(b, c_PeHeaderOffset);
            int secondsSince1970 = System.BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.ToLocalTime();
            return dt;
        }

        #endregion 頁面共用方法

        public enum PassEvent : int
        {
            Pass = 0,
            ArrivalFromAdr = 1,
            ArrivalToAdr = 2
        }

        public enum RecodeConnectionInfo_Type
        {
            Connection,
            Disconnection
        }

        public enum AppServiceMode
        {
            None,
            Standby,
            Active
        }




        #region NATS
        public const string NATS_SUBJECT_VH_INFO_0 = "NATS_SUBJECT_KEY_VH_INFO_{0}_TEST";
        public const string NATS_SUBJECT_VH_COMMAND_COMPLETE_0 = "NATS_SUBJECT_KEY_VH_COMMAND_COMPLETE_{0}";
        public const string NATS_SUBJECT_LINE_INFO = "NATS_SUBJECT_KEY_LINE_INFO";
        public const string NATS_SUBJECT_TIP_MESSAGE_INFO = "NATS_SUBJECT_KEY_TIP_MESSAGE_INFO";
        public const string NATS_SUBJECT_CURRENT_ALARM = "NATS_SUBJECT_KEY_CURRENT_ALARMS";
        public const string NATS_SUBJECT_POSTATION_CHANGE = "NATS_SUBJECT_POSTATION_CHANGE";
        public const string NATS_SUBJECT_CONNECTION_INFO = "NATS_SUBJECT_KEY_CONNECTION_INFO_INFO";
        public const string NATS_SUBJECT_ONLINE_CHECK_INFO = "NATS_SUBJECT_KEY_ONLINE_CHECK_INFO";
        public const string NATS_SUBJECT_PING_CHECK_INFO = "NATS_SUBJECT_KEY_PING_CHECK_INFO";
        public const string NATS_SUBJECT_TRANSFER = "NATS_SUBJECT_KEY_TRANSFER_INFO";
        public const string NATS_SUBJECT_MTLMTS = "NATS_SUBJECT_KEY_MTLMTS_INFO";

        public const string NATS_SUBJECT_SYSTEM_LOG = "NATS_SUBJECT_KEY_SYSTEM_LOG";
        public const string NATS_SUBJECT_TCPIP_LOG = "NATS_SUBJECT_KEY_TCPIP_LOG";
        public const string NATS_SUBJECT_SECS_LOG = "NATS_SUBJECT_KEY_SECS_LOG";

        public const string NATS_SUBJECT_PORTSTATION_INFO_0 = "NATS_SUBJECT_KEY_PORTSTATION_INFO_{0}";
        #endregion NATS
        #region Redis Key
        public const string REIDS_KEY_MCS_CMD_WAITING = "REIDS_KEY_MCS_CMD_WAITING";
        public const string REIDS_KEY_MCS_CMD_ASSIGNED = "REIDS_KEY_MCS_CMD_ASSIGNED";
        public const string REIDS_KEY_CST_TRANSFER = "REIDS_KEY_CST_TRANSFER";
        public const string REIDS_KEY_CST_WATTING = "REIDS_KEY_CST_WATTING";

        public const string REDIS_LIST_KEY_VEHICLES = "Redis_List_Vehicles";
        public const string REDIS_KEY_CURRENT_ALARM = "Current Alarm";
        public const string REDIS_KEY_CURRENT_PORTS_INFO = "CURRENT_PORTS_INFO";

        /// <summary>
        /// 用來作為在Redis中，Addresses的KEY
        /// </summary>
        public const string REDIS_KEY_ADDRESS_RESERVE_INFO_0 = "REIDS_KEY_RESERVE_INFO_ADDRESS_{0}";
        /// <summary>
        /// 用來作為在Redis中，Addresses剛被Release的KEY，確保同一台車不會短時間又要到同一段
        /// </summary>
        public const string REDIS_KEY_ADDRESS_RELEASE_INFO_0_1 = "REIDS_KEY_RELEASE_INFO_ADDRESS_{0}_{1}";
        /// <summary>
        /// 用來作為在Redis中，Traffic control的KEY
        /// </summary>
        public const string REDIS_KEY_TRAFFIC_CONTROL_INFO_0 = "REDIS_KEY_TRAFFIC_CONTROL_INFO_{0}";
        #endregion Redis Key


        #region MapInfoDataType
        public enum MapInfoDataType
        {
            MapID,
            EFConnectionString,
            Rail,
            Point,
            GroupRails,
            Address,
            Section,
            Segment,
            Port,
            PortIcon,
            Vehicle,
            Line,
            BlockZoneDetail,
            Alarm,
            MTL
        }
        #endregion MapInfoDataType

        #region SystemExcuteInfo
        public enum SystemExcuteInfoType
        {
            CommandInQueueCount,
            CommandInExcuteCount
        }
        #endregion SystemExcuteInfo


        #region PauseType
        //public enum OHxCPauseType
        //{
        //    Normal,
        //    Block,
        //    Hid,
        //    Earthquake,
        //    Safty,
        //    Obstacle,
        //    OHxCBlock,
        //    ALL
        //}
        #endregion PauseType

        #region PortStation
        public enum PortEventState
        {
            DOWN = 0,
            RTU = 1,
            RTL = 2,
            WAIT = 3,
            DISABLED = 4
        }
        public enum PortServiceState
        {
            Disable = 0,
            Enable = 1
        }
        #endregion PortStation
        #region UAS
        /****************************** UAS *******************************/
        //System
        public const string FUNC_LOGIN = "FUNC_LOGIN";
        public const string FUNC_CLOSE_SYSTEM = "FUNC_CLOSE_SYSTEM";
        public const string FUNC_ACCOUNT_MANAGEMENT = "FUNC_ACCOUNT_MANAGEMENT";

        //Operation
        public const string FUNC_SYSTEM_CONCROL_MODE = "FUNC_SYSTEM_CONCROL_MODE";
        public const string FUNC_VEHICLE_MANAGEMENT = "FUNC_VEHICLE_MANAGEMENT";
        public const string FUNC_TRANSFER_MANAGEMENT = "FUNC_TRANSFER_MANAGEMENT";

        //Query


        //Maintenance
        public const string FUNC_PORT_MAINTENANCE = "FUNC_PORT_MAINTENANCE";
        public const string FUNC_ADVANCED_SETTING = "FUNC_ADVANCED_SETTING";
        //Debug
        public const string FUNC_DEBUG = "FUNC_DEBUG";
        /******************************************************************/
        #endregion UAS
        public enum CouplerStatus
        {
            None = 0,
            Manual = 1,
            Auto = 2,
            Charging = 3,
            Error = 4
        }
        public enum CouplerHPSafety
        {
            NonSafety = 0,
            Safyte = 1,
        }
        public class DeviceName
        {
            public const string DEVICE_NAME_OHT = "OHT";
            public const string DEVICE_NAME_MTx = "MTx";
            public const string DEVICE_NAME_MCS = "MCS";
            public const string DEVICE_NAME_HID = "HID";
        }


    }
}
