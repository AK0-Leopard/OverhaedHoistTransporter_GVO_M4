// ***********************************************************************
// Assembly         : ScriptControl
// Author           : 
// Created          : 03-31-2016
//
// Last Modified By : 
// Last Modified On : 03-24-2016
// ***********************************************************************
// <copyright file="SECSConst.cs" company="">
//     Copyright ©  2014
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.stc.Common.SECS;

namespace com.mirle.ibg3k0.sc.Data.SECS.OHTC.AT_S
{
    /// <summary>
    /// Class SECSConst.
    /// </summary>
    public class SECSConst
    {
        #region Sample Flag
        /// <summary>
        /// The smplfla g_ selected
        /// </summary>
        public static readonly string SMPLFLAG_Selected = "Y";
        /// <summary>
        /// The smplfla g_ not_ selected
        /// </summary>
        public static readonly string SMPLFLAG_Not_Selected = "N";
        #endregion Sample Flag

        #region Host Control Mode
        public static readonly string HostCrtMode_EQ_Off_line = "1";
        public static readonly string HostCrtMode_Going_Online = "2";
        public static readonly string HostCrtMode_Host_Online = "3";
        public static readonly string HostCrtMode_On_Line_Local = "4";
        public static readonly string HostCrtMode_On_Line_Remote = "5";

        public static readonly string[] HOST_CRT_MODE =
        {
            HostCrtMode_EQ_Off_line,
            HostCrtMode_Going_Online,
            HostCrtMode_Host_Online,
            HostCrtMode_On_Line_Local,
            HostCrtMode_On_Line_Remote
        };
        #endregion Host Control Mode




        #region Vehicle State
        public static readonly string VHSTATE_NotRelated = "0";
        public static readonly string VHSTATE_Removed = "1";
        public static readonly string VHSTATE_NotAssigned = "2";
        public static readonly string VHSTATE_Enroute = "3";
        public static readonly string VHSTATE_Charging = "4";
        public static readonly string VHSTATE_Acquiring = "5";
        public static readonly string VHSTATE_Depositiong = "6";
        public static string convert2MCS(AVEHICLE.VehicleState vehicleState)
        {
            switch (vehicleState)
            {
                case AVEHICLE.VehicleState.REMOVED:
                    return VHSTATE_Removed;
                case AVEHICLE.VehicleState.NOT_ASSIGNED:
                    return VHSTATE_NotAssigned;
                case AVEHICLE.VehicleState.ENROUTE:
                    return VHSTATE_NotAssigned;
                case AVEHICLE.VehicleState.ACQUIRING:
                    return VHSTATE_Acquiring;
                case AVEHICLE.VehicleState.DEPOSITING:
                    return VHSTATE_Depositiong;
                default:
                    throw new Exception("參數錯誤"); //todo kevin 要帶入正確的Exception。
            }
        }
        #endregion Vehicle State
        #region Carrier State
        public const string CARRIERSTATE_WaitIn = "1";
        public const string CARRIERSTATE_Trnasferring = "2";
        public const string CARRIERSTATE_Completed = "3";
        public const string CARRIERSTATE_Alternate = "4";
        public const string CARRIERSTATE_WaitOut = "5";
        public const string CARRIERSTATE_Installed = "6";
        public static string convert2MCS(ProtocolFormat.OHTMessage.E_CARRIER_STATE carrierState)
        {
            switch (carrierState)
            {
                case ProtocolFormat.OHTMessage.E_CARRIER_STATE.WaitIn:
                    return CARRIERSTATE_WaitIn;
                case ProtocolFormat.OHTMessage.E_CARRIER_STATE.Installed:
                case ProtocolFormat.OHTMessage.E_CARRIER_STATE.IdReadFail:
                case ProtocolFormat.OHTMessage.E_CARRIER_STATE.IdMismatch:
                    return CARRIERSTATE_Trnasferring;
                case ProtocolFormat.OHTMessage.E_CARRIER_STATE.Complete:
                case ProtocolFormat.OHTMessage.E_CARRIER_STATE.OpRemove:
                case ProtocolFormat.OHTMessage.E_CARRIER_STATE.MoveError:
                case ProtocolFormat.OHTMessage.E_CARRIER_STATE.Agvremove:
                case ProtocolFormat.OHTMessage.E_CARRIER_STATE.IdDuplicate:
                    return CARRIERSTATE_Completed;
                case ProtocolFormat.OHTMessage.E_CARRIER_STATE.None:
                    return CARRIERSTATE_Installed;

                default:
                    throw new Exception($"{nameof(convert2MCS)} of carrier state:{carrierState} parameter,not exist");
            }
        }
        #endregion Carrier State

        #region TRSMODE
        /// <summary>
        /// The trsmod e_ automatic
        /// </summary>
        public static readonly string TRSMODE_AUTO = "1";
        /// <summary>
        /// The trsmod e_ manual
        /// </summary>
        public static readonly string TRSMODE_Manual = "2";
        #endregion TRSMODE

        #region PPTYPE
        /// <summary>
        /// The pptyp e_ equipment
        /// </summary>
        public static readonly string PPTYPE_Equipment = "E";
        /// <summary>
        /// The pptyp e_ unit
        /// </summary>
        public static readonly string PPTYPE_Unit = "U";
        /// <summary>
        /// The pptyp e_ sub_ unit
        /// </summary>
        public static readonly string PPTYPE_Sub_Unit = "S";
        #endregion PPTYPE

        #region ONLACK
        public static readonly string ONLACK_Accepted = "0";
        public static readonly string ONLACK_Not_Accepted = "1";
        public static readonly string ONLACK_Equipment_Already_On_Line = "2";
        #endregion ONLACK

        #region SFCD
        /// <summary>
        /// The SFC d_ module_ status_ request
        /// </summary>
        public static readonly string SFCD_Module_Status_Request = "1";
        /// <summary>
        /// The SFC d_ port_ status_ request
        /// </summary>
        public static readonly string SFCD_Port_Status_Request = "2";
        /// <summary>
        /// The SFC d_ reticle_ status_ request
        /// </summary>
        public static readonly string SFCD_Reticle_Status_Request = "3";
        /// <summary>
        /// The SFC d_ unit_ status_ request
        /// </summary>
        public static readonly string SFCD_Unit_Status_Request = "4";
        /// <summary>
        /// The SFC d_ sub_ unit_ status_ request
        /// </summary>
        public static readonly string SFCD_Sub_Unit_Status_Request = "5";
        /// <summary>
        /// The SFC d_ mask_ status_ request
        /// </summary>
        public static readonly string SFCD_Mask_Status_Request = "6";
        /// <summary>
        /// The SFC d_ material_ status_ request
        /// </summary>
        public static readonly string SFCD_Material_Status_Request = "7";
        /// <summary>
        /// The SFC d_ sorter_ job_ list_ request
        /// </summary>
        public static readonly string SFCD_Sorter_Job_List_Request = "8";
        /// <summary>
        /// The SFC d_ crate_ port_ status_ request
        /// </summary>
        public static readonly string SFCD_Crate_Port_Status_Request = "9";
        /// <summary>
        /// The SFC d_ finish
        /// </summary>
        public static readonly string SFCD_Finish = "F";
        #endregion SFCD

        #region CIACK
        /// <summary>
        /// The ciac k_ accepted
        /// </summary>
        public static readonly string CIACK_Accepted = "0";
        /// <summary>
        /// The ciac k_ busy
        /// </summary>
        public static readonly string CIACK_Busy = "1";
        /// <summary>
        /// The ciac k_ csti d_is_ invalid
        /// </summary>
        public static readonly string CIACK_CSTID_is_Invalid = "2";
        /// <summary>
        /// The ciac k_ ppi d_is_ invalid
        /// </summary>
        public static readonly string CIACK_PPID_is_Invalid = "3";
        /// <summary>
        /// The ciac k_ slo t_ information_ mismatch
        /// </summary>
        public static readonly string CIACK_SLOT_Information_Mismatch = "4";
        /// <summary>
        /// The ciac k_ already_ received
        /// </summary>
        public static readonly string CIACK_Already_Received = "5";
        /// <summary>
        /// The ciac k_ pai r_ lo t_ mismatch
        /// </summary>
        public static readonly string CIACK_PAIR_LOT_Mismatch = "6";
        /// <summary>
        /// The ciac k_ pro d_ i d_ invalid
        /// </summary>
        public static readonly string CIACK_PROD_ID_Invalid = "7";
        /// <summary>
        /// The ciac k_ glass_ type_ invalid
        /// </summary>
        public static readonly string CIACK_Glass_Type_Invalid = "8";
        /// <summary>
        /// The ciac k_ other_ error
        /// </summary>
        public static readonly string CIACK_Other_Error = "9";
        #endregion CIACK

        #region RCMD
        public const string RCMD_Abort = "ABORT";
        public const string RCMD_Cancel = "CANCEL";
        public const string RCMD_Pause = "PAUSE";
        public const string RCMD_Resume = "RESUME";
        public const string RCMD_PriorityUpdate = "PRIORITYUPDATE";
        public const string RCMD_Install = "INSTALL";
        public const string RCMD_Remove = "REMOVE";
        public const string RCMD_TransferEXt = "TRANSFEREXT";
        public const string RCMD_Rename = "RENAME";
        public const string RCMD_StageDelete = "STAGEDELETE";
        public const string RCMD_PortTypeChange = "PORTTYPECHG";
        #endregion RCMD

        #region CPNAME
        public const string CPNAME_CommandID = "COMMANDID";
        public const string CPNAME_SourcePort = "SOURCEPORT";
        public const string CPNAME_DestPort = "DESTPORT";
        public const string CPNAME_PortID = "PORTID";
        public const string CPNAME_PortUnitType = "PORTUNITTYPE";
        public const string CPNAME_PortType = "PORTTYPE";
        public const string CPNAME_Priority = "PRIORITY";
        #endregion CPNAME


        #region HCACK
        public static readonly string HCACK_Confirm_Executed = "0";
        public static readonly string HCACK_Command_Not_Exist = "1";
        public static readonly string HCACK_Cannot_Perform_Now = "2";
        public static readonly string HCACK_Param_Invalid = "3";
        public static readonly string HCACK_Confirm = "4";
        public static readonly string HCACK_Rejected = "5";
        public static readonly string HCACK_Obj_Not_Exist = "6";
        #endregion HCACK


        //#region HCACK
        //public static readonly string HCACK_Confirm_Executed = "0";
        //public static readonly string HCACK_Command_Not_Exist = "1";
        //public static readonly string HCACK_Not_Able_Execute = "2";
        //public static readonly string HCACK_Param_Invalid = "3";
        //public static readonly string HCACK_Confirm = "4";
        //public static readonly string HCACK_Rejected = "5";
        //public static readonly string HCACK_Obj_Not_Exist = "6";
        //#endregion HCACK

        #region CEPACK
        public static readonly string CEPACK_No_Error = "0";
        public static readonly string CEPACK_CPNAME_Does_Not_Exist = "1";
        public static readonly string CEPACK_Incorrect_Value_In_CEPVAL = "2";
        public static readonly string CEPACK_Incorrect_Format_In_CEPVAL = "3";
        public static readonly string CEPACK_CPNAME_is_not_Valid = "4";
        #endregion HCACK

        #region CMD Result Code
        public const string CMD_Result_Successful = "0";
        public const string CMD_Result_OrtherError = "1";
        public const string CMD_Result_Reservered = "2";
        public const string CMD_Result_DuplicateID = "3";
        public const string CMD_Result_IDMismatch = "4";
        public const string CMD_Result_IDReadFailed = "5";
        public const string CMD_Result_VehicleAbort = "32";
        public const string CMD_Result_InterlockError = "64";
        public static string convert2MCS(ProtocolFormat.OHTMessage.CompleteStatus? tran_cmp_status)
        {
            switch (tran_cmp_status)
            {
                case ProtocolFormat.OHTMessage.CompleteStatus.CmpStatusMove:
                case ProtocolFormat.OHTMessage.CompleteStatus.CmpStatusLoad:
                case ProtocolFormat.OHTMessage.CompleteStatus.CmpStatusUnload:
                case ProtocolFormat.OHTMessage.CompleteStatus.CmpStatusLoadunload:
                case ProtocolFormat.OHTMessage.CompleteStatus.ForceNormalFinishByOp:
                    return CMD_Result_Successful;
                case ProtocolFormat.OHTMessage.CompleteStatus.CmpStatusCancel:
                case ProtocolFormat.OHTMessage.CompleteStatus.CmpStatusAbort:
                //case ProtocolFormat.OHTMessage.CompleteStatus.CmpStatusVehicleAbort:
                case ProtocolFormat.OHTMessage.CompleteStatus.LongTimeInaction:
                //case ProtocolFormat.OHTMessage.CompleteStatus.ForceFinishByOp:
                case ProtocolFormat.OHTMessage.CompleteStatus.CommandInitailFail:
                case ProtocolFormat.OHTMessage.CompleteStatus.ForceAbnormalFinishByOp:
                    return CMD_Result_OrtherError;
                case ProtocolFormat.OHTMessage.CompleteStatus.CmpStatusIdmisMatch:
                    return CMD_Result_IDMismatch;
                case ProtocolFormat.OHTMessage.CompleteStatus.CmpStatusIdreadFailed:
                    return CMD_Result_IDReadFailed;
                case ProtocolFormat.OHTMessage.CompleteStatus.CmpStatusVehicleAbort:
                    return CMD_Result_VehicleAbort;
                case ProtocolFormat.OHTMessage.CompleteStatus.CmpStatusInterlockError:
                    return CMD_Result_InterlockError;
                default:
                    //throw new Exception("參數錯誤"); //TODO 要帶入正確的Exception。
                    return CMD_Result_OrtherError;
            }
        }
        #endregion CMD Result Code
        #region ACK
        /// <summary>
        /// The ac k_ accepted
        /// </summary>
        public static readonly string ACK_Accepted = "0";
        /// <summary>
        /// The ac k_ not_ accepted
        /// </summary>
        public static readonly string ACK_Not_Accepted = "1";
        #endregion ACK

        #region CEID
        /// <summary>
        /// 用來代表所有的CEID（於Enable、Disable All CEID時會使用到）。
        /// </summary>
        //public const string CEID_ALL_CEID = "000";
        //CEID Control Related Events
        public const string CEID_Equipment_OFF_LINE = "001";
        public const string CEID_Control_Status_Local = "002";
        public const string CEID_Control_Status_Remote = "003";
        //SC Transition Events
        public const string CEID_Alarm_Cleared = "051";
        public const string CEID_Alarm_Set = "052";
        public const string CEID_TSC_Auto_Completed = "053";
        public const string CEID_TSC_Auto_Initiated = "054";
        public const string CEID_TSC_Pause_Completed = "055";
        public const string CEID_TSC_Paused = "056";
        public const string CEID_TSC_Pause_Initiated = "057";
        //Transfer Command Status Transition Events
        public const string CEID_Transfer_Abort_Completed = "101";
        public const string CEID_Transfer_Abort_Failed = "102";
        public const string CEID_Transfer_Abort_Initiated = "103";
        public const string CEID_Transfer_Cancel_Completed = "104";
        public const string CEID_Transfer_Cancel_Failed = "105";
        public const string CEID_Transfer_Cancel_Initiated = "106";
        public const string CEID_Transfer_Completed = "107";
        public const string CEID_Transfer_Initiated = "108";
        public const string CEID_Transfer_Pause = "109";
        public const string CEID_Transfer_Resumed = "110";
        public const string CEID_Transferring = "111";
        //Carrier Status Transition Events (CEID 151 ~ 152)
        public const string CEID_Carrier_Installed = "151";
        public const string CEID_Carrier_Removed_Complete = "152";
        public const string CEID_Carrier_Removed = "153";
        public const string CEID_Carrier_Wait_In = "158";
        public const string CEID_Carrier_Wait_Out = "161";
        public const string CEID_Carrier_InstallComplete = "160";
        //Vehicle Status Transition Events 
        public const string CEID_Vehicle_Assigned = "201";
        public const string CEID_Vehicle_Unassigned = "202";
        public const string CEID_Vehicle_Acquire_Started = "203";
        public const string CEID_Vehicle_Acquire_Completed = "204";
        public const string CEID_Vehicle_Departed = "205";
        public const string CEID_Vehicle_Arrived = "206";
        public const string CEID_Vehicle_Deposit_Started = "207";
        public const string CEID_Vehicle_Deposit_Completed = "208";
        public const string CEID_Vehicle_Installed = "210";
        public const string CEID_Vehicle_Removed = "211";
        //Supplier Definitions Events 
        public const string CEID_Unit_Alarm_Set = "162";
        public const string CEID_Unit_Alarm_Cleared = "163";

        public const string CEID_Port_Out_Of_Service = "260";
        public const string CEID_Port_In_Service = "261";
        public const string CEID_Port_Type_Input = "701";
        public const string CEID_Port_Type_Output = "702";
        public const string CEID_Port_Type_Changing = "703";

        public const string CEID_Port_LoadReq = "602";
        public const string CEID_Port_UnloadReq = "603";
        public const string CEID_Port_NoReq = "604";

        public const string CEID_Vehicle_RuntimeStatus = "200";
        public const string CEID_Carrier_ID_Read = "251";
        public const string CEID_OperatorInitiateAction = "254";


        //CEID Remark End


        #region CEID Array

        #region VID
        public const string VID_AlarmSet = "4";
        public const string VID_ControlState = "6";
        public const string VID_EnhancedCarrierInfo = "10";
        public const string VID_InstallTime = "12";
        public const string VID_EnhancedTransferCmd = "13";
        public const string VID_CarrierType = "16";
        public const string VID_LotID = "17";
        public const string VID_EnhancedALID = "40";
        public const string VID_EnhancedAlarmsSet = "41";
        public const string VID_EnhancedCarriers = "51";
        public const string VID_EnhancedVehicles = "53";
        public const string VID_CarrierID = "54";
        public const string VID_NewCarrierID = "50";
        public const string VID_CarrierLoc = "56";
        public const string VID_CommandID = "58";
        public const string VID_CommandInfo = "59";
        public const string VID_Dest = "60";
        public const string VID_EqpName = "61";
        public const string VID_Priority = "62";
        public const string VID_ResultCode = "64";
        public const string VID_Source = "65";
        public const string VID_HandoffType = "66";
        public const string VID_IDreadStatus = "67";
        public const string VID_VehicleID = "70";
        public const string VID_VehicleInfo = "71";
        public const string VID_VehicleState = "72";
        public const string VID_TSCState = "73";
        public const string VID_EnhancedTransfers = "76";
        public const string VID_TransferCompleteInfo = "77";
        public const string VID_CommandType = "80";
        public const string VID_AlarmID = "81";
        public const string VID_AlarmText = "82";
        public const string VID_UnitID = "83";
        public const string VID_TransferInfo = "84";
        public const string VID_BatteryValue = "101";
        public const string VID_VehicleLastPosition = "102";
        public const string VID_SpecVersion = "114";
        public const string VID_PortID = "115";
        public const string VID_PortType = "116";
        public const string VID_VichicleLocation = "117";
        public const string VID_CurrentPortStates = "118";
        public const string VID_CarrierState = "203";
        public const string VID_EQPortInfo = "349";
        public const string VID_CurrEqPortStatus = "350";
        public const string VID_PortTypes = "351";
        public const string VID_EqReqStatus = "352";
        public const string VID_EqPresenceStatus = "353";
        public const string VID_PortInfo = "354";
        public const string VID_PortTransferState = "355";
        public const string VID_EqPortInfo = "356";
        public const string VID_UnitAlarmList = "360";
        public const string VID_UnitAlarmInfo = "361";
        public const string VID_MaintState = "362";
        public const string VID_VehicleCurrentPosition = "363";
        public const string VID_CarrierZoneName = "370";
        public const string VID_PortTypeInfo = "601";
        public const string VID_PortUnitType = "602";
        public const string VID_TransferState = "722";
        public const string VID_MonitoredVehicles = "723";
        public const string VID_MonitoredVehicleInfo = "724";
        public const string VID_VehicleNextPosition = "725";
        public const string VID_VehiclecCommunication = "726";
        public const string VID_VehcileControlMode = "727";
        public const string VID_PortsLocationList = "728";
        public const string VID_PortLocInfo = "729";
        public const string VID_PortPosition = "730";



        #endregion VID



        public static readonly string[] CEID_ARRAY =
        {
             CEID_Equipment_OFF_LINE,
             CEID_Control_Status_Local,
             CEID_Control_Status_Remote,

             CEID_Alarm_Cleared ,
             CEID_Alarm_Set,
             CEID_TSC_Auto_Completed ,
             CEID_TSC_Auto_Initiated,
             CEID_TSC_Pause_Completed,
             CEID_TSC_Paused,
             CEID_TSC_Pause_Initiated,

             CEID_Transfer_Abort_Completed,
             CEID_Transfer_Abort_Failed,
             CEID_Transfer_Abort_Initiated,
             CEID_Transfer_Cancel_Completed,
             CEID_Transfer_Cancel_Failed,
             CEID_Transfer_Cancel_Initiated,
             CEID_Transfer_Completed ,
             CEID_Transfer_Initiated,
             CEID_Transfer_Pause,
             CEID_Transfer_Resumed ,
             CEID_Transferring,

             CEID_Carrier_Installed ,
             CEID_Carrier_Removed_Complete,
             CEID_Carrier_Removed,
             CEID_Carrier_Wait_In,
             CEID_Carrier_Wait_Out,
             CEID_Carrier_InstallComplete,

             CEID_Vehicle_Assigned,
             CEID_Vehicle_Unassigned,
             CEID_Vehicle_Acquire_Started,
             CEID_Vehicle_Acquire_Completed,
             CEID_Vehicle_Departed,
             CEID_Vehicle_Arrived,
             CEID_Vehicle_Deposit_Started,
             CEID_Vehicle_Deposit_Completed,
             CEID_Vehicle_Installed,
             CEID_Vehicle_Removed ,

             CEID_Unit_Alarm_Set,
             CEID_Unit_Alarm_Cleared,

             CEID_Port_Out_Of_Service,
             CEID_Port_In_Service ,
             CEID_Port_Type_Input ,
             CEID_Port_Type_Output ,
             CEID_Port_Type_Changing ,


             CEID_Port_LoadReq ,
             CEID_Port_UnloadReq,
             CEID_Port_NoReq,

             CEID_Vehicle_RuntimeStatus,
             CEID_Carrier_ID_Read,
             CEID_OperatorInitiateAction
        };
        public static Dictionary<string, string> CEID_Dictionary = new Dictionary<string, string>()
        {
            {CEID_Equipment_OFF_LINE,"Equipment_OFF_LINE" },
            {CEID_Control_Status_Local,"Equipment_OFF_LINE" },
            {CEID_Control_Status_Remote,"Control_Status_Remote" },
            {CEID_Alarm_Cleared,"Alarm_Cleared" },
            {CEID_Alarm_Set,"Alarm_Set" },
            {CEID_TSC_Auto_Completed,"TSC_Auto_Completed" },
            {CEID_TSC_Auto_Initiated,"TSC_Auto_Initiated" },
            {CEID_TSC_Pause_Completed,"TSC_Pause_Completed" },
            {CEID_TSC_Paused,"TSC_Paused" },
            {CEID_TSC_Pause_Initiated,"TSC_Pause_Initiated" },
            {CEID_Transfer_Abort_Completed,"Transfer_Abort_Completed" },
            {CEID_Transfer_Abort_Failed,"Transfer_Abort_Failed" },
            {CEID_Transfer_Abort_Initiated,"Transfer_Abort_Initiated" },
            {CEID_Transfer_Cancel_Completed,"Transfer_Cancel_Completed" },
            {CEID_Transfer_Cancel_Failed,"Transfer_Cancel_Failed" },
            {CEID_Transfer_Cancel_Initiated,"Transfer_Cancel_Initiated" },
            {CEID_Transfer_Completed,"Transfer_Completed" },
            {CEID_Transfer_Initiated,"Transfer_Initiated" },
            {CEID_Transfer_Pause,"Transfer_Pause" },
            {CEID_Transfer_Resumed,"Transfer_Resumed" },
            {CEID_Transferring,"Transferring" },
            {CEID_Carrier_Installed,"Carrier_Installed" },
            {CEID_Carrier_Removed,"Carrier_Removed" },
            {CEID_Vehicle_Arrived,"Vehicle_Arrived" },
            {CEID_Vehicle_Acquire_Started,"Vehicle_Acquire_Started" },
            {CEID_Vehicle_Acquire_Completed,"Vehicle_Acquire_Completed" },
            {CEID_Vehicle_Assigned,"Vehicle_Assigned" },
            {CEID_Vehicle_Departed,"Vehicle_Departed" },
            {CEID_Vehicle_Deposit_Started,"Vehicle_Deposit_Started" },
            {CEID_Vehicle_Deposit_Completed,"Vehicle_Deposit_Completed" },
            {CEID_Vehicle_Installed,"Vehicle_Installed" },
            {CEID_Vehicle_Removed,"Vehicle_Removed" },
            {CEID_Vehicle_Unassigned,"Vehicle_Unassigned" },
            {CEID_OperatorInitiateAction,"Operator_Initiate_Action" },

            {CEID_Unit_Alarm_Set,"CEID_Unit_Alarm_Set" },
            {CEID_Unit_Alarm_Cleared,"CEID_Unit_Alarm_Cleared" },
            {CEID_Port_Out_Of_Service,"CEID_Port_Out_Of_Service" },
            {CEID_Port_In_Service,"CEID_Port_In_Service" },
            {CEID_Port_LoadReq,"CEID_Port_LoadReq" },
            {CEID_Port_UnloadReq,"CEID_Port_UnloadReq" },
            {CEID_Port_NoReq,"CEID_Port_NoReq" }
        };
        #endregion CEID Array
        #endregion CEID

        #region ACKC6
        /// <summary>
        /// The ack C6_ accepted
        /// </summary>
        public static readonly string ACKC6_Accepted = "0";
        /// <summary>
        /// The ack C6_ not accepted
        /// </summary>
        public static readonly string ACKC6_NotAccepted = "1";
        #endregion ACKC6

        #region TIACK
        /// <summary>
        /// The tiac k_ accepted
        /// </summary>
        public static readonly string TIACK_Accepted = "0";
        /// <summary>
        /// The tiac k_ error_not_done
        /// </summary>
        public static readonly string TIACK_Error_not_done = "1";
        #endregion TIACK

        #region OFLACK
        /// <summary>
        /// The oflac k_ accepted
        /// </summary>
        public static readonly string OFLACK_Accepted = "0";
        /// <summary>
        /// The oflac k_ not_ accepted
        /// </summary>
        public static readonly string OFLACK_Not_Accepted = "1";
        #endregion OFLACK

        #region EAC
        /// <summary>
        /// The ea c_ accept
        /// </summary>
        public static readonly string EAC_Accept = "0";
        /// <summary>
        /// The ea c_ denied_ at_ least_one_constant_does_not_exist
        /// </summary>
        public static readonly string EAC_Denied_At_Least_one_constant_does_not_exist = "1";
        /// <summary>
        /// The ea c_ denied_ busy
        /// </summary>
        public static readonly string EAC_Denied_Busy = "2";
        /// <summary>
        /// The ea c_ denied_ at_least_one_constant_out_of_range
        /// </summary>
        public static readonly string EAC_Denied_At_least_one_constant_out_of_range = "3";
        /// <summary>
        /// The ea c_ other_equipment_specific_error
        /// </summary>
        public static readonly string EAC_Other_equipment_specific_error = "4";
        #endregion EAC

        #region TIAACK
        /// <summary>
        /// The tiaac k_ everything_correct
        /// </summary>
        public static readonly string TIAACK_Everything_correct = "0";
        /// <summary>
        /// The tiaac k_ too_many_ svi ds
        /// </summary>
        public static readonly string TIAACK_Too_many_SVIDs = "1";
        /// <summary>
        /// The tiaac k_ no_more_traces_allowed
        /// </summary>
        public static readonly string TIAACK_No_more_traces_allowed = "2";
        /// <summary>
        /// The tiaac k_ invalid_period
        /// </summary>
        public static readonly string TIAACK_Invalid_period = "3";
        /// <summary>
        /// The tiaac k_ equipment_specified_error
        /// </summary>
        public static readonly string TIAACK_Equipment_specified_error = "4";
        #endregion TIAACK

        #region ERACK
        /// <summary>
        /// The erac k_ accepted
        /// </summary>
        public static readonly string ERACK_Accepted = "0";
        /// <summary>
        /// The erac k_ denied_ at_least_one_ cei d_dose_not_exist
        /// </summary>
        public static readonly string ERACK_Denied_At_least_one_CEID_dose_not_exist = "1";
        /// <summary>
        /// The erac k_ other_ errors
        /// </summary>
        public static readonly string ERACK_Other_Errors = "2";
        #endregion ERACK

        #region ACKC5
        /// <summary>
        /// The ack C5_ accepted
        /// </summary>
        public static readonly string ACKC5_Accepted = "0";
        /// <summary>
        /// The ack C5_ not_ accepted
        /// </summary>
        public static readonly string ACKC5_Not_Accepted = "1";
        #endregion ACKC5

        #region ACKC7
        /// <summary>
        /// The ack C7_ accepted
        /// </summary>
        public static readonly string ACKC7_Accepted = "0";
        /// <summary>
        /// The ack C7_ not_ accepted
        /// </summary>
        public static readonly string ACKC7_Not_Accepted = "1";
        /// <summary>
        /// The ack C7_ unit_ i d_is_not_exist
        /// </summary>
        public static readonly string ACKC7_Unit_ID_is_not_exist = "2";
        /// <summary>
        /// The ack C7_ pptyp e_is_not_match
        /// </summary>
        public static readonly string ACKC7_PPTYPE_is_not_match = "3";
        /// <summary>
        /// The ack C7_ ppi d_is_not_match
        /// </summary>
        public static readonly string ACKC7_PPID_is_not_match = "4";
        #endregion ACKC7

        #region ACKC10
        /// <summary>
        /// The ack C10_ accepted
        /// </summary>
        public static readonly string ACKC10_Accepted = "0";
        /// <summary>
        /// The ack C10_ not_ accepted
        /// </summary>
        public static readonly string ACKC10_Not_Accepted = "1";
        #endregion ACKC10

        #region CEED
        /// <summary>
        /// The cee d_ enable
        /// </summary>
        public static readonly string CEED_Enable = "0";
        /// <summary>
        /// The cee d_ disable
        /// </summary>
        public static readonly string CEED_Disable = "1";
        #endregion CEED

        #region ALED
        /// <summary>
        /// The ale d_ enable
        /// </summary>
        public static readonly string ALED_Enable = "1";
        /// <summary>
        /// The ale d_ disable
        /// </summary>
        public static readonly string ALED_Disable = "128";
        #endregion ALED


        #region PPCINFO
        /// <summary>
        /// A new PPID is created and registered
        /// </summary>
        public static readonly string PPCINFO_Created = "1";
        /// <summary>
        /// Some parameters of a PPID are modified
        /// </summary>
        public static readonly string PPCINFO_Modified = "2";
        /// <summary>
        /// Any PPID is deleted
        /// </summary>
        public static readonly string PPCINFO_Deleted = "3";
        /// <summary>
        /// Equipment sets up any PPID which different from current PPID
        /// </summary>
        public static readonly string PPCINFO_Changed = "4";
        #endregion PPCINFO

        #region ALST
        /// <summary>
        /// The als t_ set
        /// </summary>
        public static readonly string ALST_SET = "1";
        /// <summary>
        /// The als t_ clear
        /// </summary>
        public static readonly string ALST_CLEAR = "2";
        #endregion ALST

        #region ALCD
        /// <summary>
        /// The alc d_ light_ alarm
        /// </summary>
        public static readonly string ALCD_Alarm_Set = "80";
        /// <summary>
        /// The alc d_ serious_ alarm
        /// </summary>
        public static readonly string ALCD_Alarm_Clear = "0";
        #endregion ALCD


        #region SCACK
        /// <summary>
        /// The scac k_ accepted
        /// </summary>
        public static readonly string SCACK_Accepted = "0";
        /// <summary>
        /// The scac k_ busy
        /// </summary>
        public static readonly string SCACK_Busy = "1";
        /// <summary>
        /// The scac k_ csti d_is_ invalid
        /// </summary>
        public static readonly string SCACK_CSTID_is_Invalid = "2";
        /// <summary>
        /// The scac k_ already_ received
        /// </summary>
        public static readonly string SCACK_Already_Received = "3";
        /// <summary>
        /// The scac k_ slo t_ information_ mismatch
        /// </summary>
        public static readonly string SCACK_SLOT_Information_Mismatch = "4";
        /// <summary>
        /// The scac k_ net yet_ prepared_ for_ this_ sorter_ job
        /// </summary>
        public static readonly string SCACK_NetYet_Prepared_For_This_Sorter_Job = "5";
        #endregion


        #region CPACK
        public static readonly string CPACK_No_Error = "0";
        public static readonly string CPACK_Name_Not_Exist = "1";
        public static readonly string CPACK_Invalid_Value = "2";
        public static readonly string CPACK_Invalid_Format = "3";
        public static readonly string CPACK_Other_Error = "4";
        #endregion

        #region SCSTATE
        public static readonly string SCSTATE_Init = "1";
        public static readonly string SCSTATE_Paused = "2";
        public static readonly string SCSTATE_Auto = "3";
        public static readonly string SCSTATE_Pausing = "4";
        #endregion SCSTATE

        #region TRANSFERSTATE
        public const string TRANSFERSTATE_Queued = "1";
        public const string TRANSFERSTATE_Transsfring = "2";
        public const string TRANSFERSTATE_Paused = "3";
        public const string TRANSFERSTATE_Canceling = "4";
        public const string TRANSFERSTATE_Aborting = "5";
        public const string TRANSFERSTATE_Waiting = "6";
        public const string TRANSFERSTATE_Orther = "99";
        public static string convert2MCS(E_TRAN_STATUS tranStatus)
        {
            switch (tranStatus)
            {
                case E_TRAN_STATUS.Queue:
                case E_TRAN_STATUS.PreInitial:
                    return TRANSFERSTATE_Queued;
                case E_TRAN_STATUS.Initial:
                    return TRANSFERSTATE_Waiting;
                case E_TRAN_STATUS.Transferring:
                    return TRANSFERSTATE_Transsfring;
                case E_TRAN_STATUS.Canceling:
                    return TRANSFERSTATE_Canceling;
                case E_TRAN_STATUS.Aborting:
                    return TRANSFERSTATE_Aborting;
                default:
                    return TRANSFERSTATE_Orther;
            }
        }
        #endregion TRANSFERSTATE

        #region Communication Status
        public const string COMMUNICATION_STATUS_DISCONNECTED = "0";
        public const string COMMUNICATION_STATUS_COMMUNICATING = "1";
        public const string COMMUNICATION_STATUS_NOT_COMMUNICATING = "3";
        public static string convert2MCS(bool isVhConnection, bool isVhCommunicating)
        {
            if (isVhConnection)
            {
                if (isVhCommunicating)
                {
                    return COMMUNICATION_STATUS_COMMUNICATING;
                }
                else
                {
                    return COMMUNICATION_STATUS_NOT_COMMUNICATING;
                }
            }
            else
            {
                return COMMUNICATION_STATUS_DISCONNECTED;
            }
        }
        #endregion Communication Status
        #region ControlMode
        public const string VEHICLE_CONTORL_MODE_MANUAL = "0";
        public const string VEHICLE_CONTORL_MODE_AUTO = "1";
        public static string convert2MCS(bool isVhConnection, ProtocolFormat.OHTMessage.VHModeStatus mode)
        {
            if (!isVhConnection) return VEHICLE_CONTORL_MODE_MANUAL;
            switch (mode)
            {
                case ProtocolFormat.OHTMessage.VHModeStatus.AutoRemote:
                    return VEHICLE_CONTORL_MODE_AUTO;
                default:
                    return VEHICLE_CONTORL_MODE_MANUAL;
            }
        }
        #endregion

        #region Carrier ID Read Status
        public const string IDREADSTATUS_Successful = "0";
        public const string IDREADSTATUS_Failed = "1";
        public const string IDREADSTATUS_Duplicate = "2";
        public const string IDREADSTATUS_Mismatch = "3";
        public static string convert2MCS(ProtocolFormat.OHTMessage.E_ID_READ_STSTUS? idReadStatus)
        {
            //if (!idReadStatus.HasValue)
            //{
            //    throw new Exception("CST Read status no value");
            //}
            switch (idReadStatus)
            {
                case ProtocolFormat.OHTMessage.E_ID_READ_STSTUS.Successful:
                    return IDREADSTATUS_Successful;
                case ProtocolFormat.OHTMessage.E_ID_READ_STSTUS.Failed:
                    return IDREADSTATUS_Failed;
                case ProtocolFormat.OHTMessage.E_ID_READ_STSTUS.Duplicate:
                    return IDREADSTATUS_Duplicate;
                case ProtocolFormat.OHTMessage.E_ID_READ_STSTUS.Mismatch:
                    return IDREADSTATUS_Mismatch;
                default:
                    return IDREADSTATUS_Failed;

                    //throw new Exception("參數錯誤"); //TODO 要帶入正確的Exception。
            }
        }
        #endregion Carrier ID Read Status
        //#region PPTYPE
        //public static readonly string PPTYPE_Equipment = "E";
        //public static readonly string PPTYPE_Unit = "U";
        //public static readonly string PPTYPE_SubUnit = "S";
        //#endregion PPTYPE

        /// <summary>
        /// Checks the data value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns>com.mirle.ibg3k0.stc.Common.SECS.SECSAgent.SECS_STREAM_FUNCTION_CHECK_RESULT.</returns>
        public static com.mirle.ibg3k0.stc.Common.SECS.SECSAgent.SECS_STREAM_FUNCTION_CHECK_RESULT checkDataValue(
            string name, string value)
        {
            com.mirle.ibg3k0.stc.Common.SECS.SECSAgent.SECS_STREAM_FUNCTION_CHECK_RESULT result =
                SECSAgent.SECS_STREAM_FUNCTION_CHECK_RESULT.Recognize;

            //if (name.Trim().Equals("CRST"))
            //{
            //    //SECSConst.CRST
            //    if (!SECSConst.CRST.Contains(value.Trim()))
            //    {
            //        return SECSAgent.SECS_STREAM_FUNCTION_CHECK_RESULT.Illegal_Data_Value_Format;
            //    }
            //}

            return result;
        }

        /// <summary>
        /// The stream identifier array
        /// </summary>
        public static readonly int[] StreamIDArray = { 1, 2, 5, 6, 9, 10 };
        /// <summary>
        /// The function identifier array
        /// </summary>
        public static readonly int[] FunctionIDArray =
        {
            0, 1, 2, 3, 4, 5, 6, 7, 9,
            11, 12, 13, 14, 15, 16, 17, 18,
            23, 24, 25, 26,
            31, 32, 33, 34, 35, 36, 37, 38,
            41, 42, 49, 50,
        };

        public static Dictionary<string, List<string>> DicCEIDAndRPTID { get; private set; }
        public static Dictionary<string, List<ARPTID>> DicRPTIDAndVID { get; private set; }

        public static void setDicCEIDAndRPTID(Dictionary<string, List<string>> _dic)
        {
            DicCEIDAndRPTID = _dic;
        }
        public static void setDicRPTIDAndVID(Dictionary<string, List<ARPTID>> _dic)
        {
            DicRPTIDAndVID = _dic;
        }
        /// <summary>
        /// Checks the type of the sf.
        /// </summary>
        /// <param name="S">The s.</param>
        /// <param name="F">The f.</param>
        /// <returns>com.mirle.ibg3k0.stc.Common.SECS.SECSAgent.SECS_STREAM_FUNCTION_CHECK_RESULT.</returns>
        public static com.mirle.ibg3k0.stc.Common.SECS.SECSAgent.SECS_STREAM_FUNCTION_CHECK_RESULT checkSFType(int S, int F)
        {
            com.mirle.ibg3k0.stc.Common.SECS.SECSAgent.SECS_STREAM_FUNCTION_CHECK_RESULT result =
                SECSAgent.SECS_STREAM_FUNCTION_CHECK_RESULT.Recognize;
            string streamFunction = string.Format("S{0}F{1}", S, F);

            if (!StreamIDArray.Contains(S))
            {
                result = SECSAgent.SECS_STREAM_FUNCTION_CHECK_RESULT.Unrecognized_Stream_Type;
            }
            else if (!FunctionIDArray.Contains(F))
            {
                result = SECSAgent.SECS_STREAM_FUNCTION_CHECK_RESULT.Unrecognized_Function_Type;
            }
            else
            {
                Type type = Type.GetType("com.mirle.ibg3k0.sc.Data.SECS." + streamFunction);
                Type typeBase = Type.GetType("com.mirle.ibg3k0.stc.Data.SecsData." + streamFunction);
                if (type == null && typeBase == null && F != 0)
                {
                    result = SECSAgent.SECS_STREAM_FUNCTION_CHECK_RESULT.Unrecognized_Stream_Type;
                }
            }
            return result;
        }



    }
}
