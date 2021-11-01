using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.BLL;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.Data.PLC_Functions;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.Module;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using com.mirle.ibg3k0.sc.RouteKit;
using DocumentFormat.OpenXml.Spreadsheet;
using Google.Protobuf.Collections;
using KingAOP;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel.PeerResolvers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using static com.mirle.ibg3k0.sc.App.SCAppConstants;

namespace com.mirle.ibg3k0.sc.Service
{
    public enum AlarmLst
    {
        OHT_INTERLOCK_ERROR = 100010,
        OHT_VEHICLE_ABORT = 100011,
        OHT_BCR_READ_FAIL = 100012,
        PORT_BOXID_READ_FAIL = 100013,
        PORT_CSTID_READ_FAIL = 100014,
        OHT_IDLE_HasCMD_TimeOut = 100015,
        OHT_QueueCmdTimeOut = 100016,
        AGV_HasCmdsAccessTimeOut = 100017,
        AGVStation_DontHaveEnoughEmptyBox = 100018,
        PORT_CIM_OFF = 100019,
        PORT_DOWN = 100020,
        BOX_NumberIsNotEnough = 100021,
        OHT_IDMismatchUNKU = 100022,
        LINE_NotEmptyShelf = 100023,
        PORT_OP_WaitOutTimeOut = 100024,
        PORT_BP1_WaitOutTimeOut = 100025,
        PORT_BP2_WaitOutTimeOut = 100026,
        PORT_BP3_WaitOutTimeOut = 100027,
        PORT_BP4_WaitOutTimeOut = 100028,
        PORT_BP5_WaitOutTimeOut = 100029,
        PORT_LP_WaitOutTimeOut = 100030,
    }
    public class DeadLockEventArgs : EventArgs
    {
        public AVEHICLE Vehicle1;
        public AVEHICLE Vehicle2;
        public DeadLockEventArgs(AVEHICLE vehicle1, AVEHICLE vehicle2)
        {
            Vehicle1 = vehicle1;
            Vehicle2 = vehicle2;
        }
    }

    public class VehicleService : IDynamicMetaObjectProvider
    {
        protected static Logger logger = LogManager.GetCurrentClassLogger();
        protected static Logger VehiclePauserHandlerInfoLogger = LogManager.GetLogger("VehiclePauserHandlerInfo");
        protected static Logger MTLPauserHandlerInfoLogger = LogManager.GetLogger("MTLPauserHandlerInfo");
        protected static SCApplication scApp = null;
        public const string DEVICE_NAME_OHx = "OHx";
        public SendProcessor Send { get; protected set; }
        public ReceiveProcessor Receive { get; protected set; }
        public CommandProcessor Command { get; protected set; }
        public AvoidProcessor Avoid { get; protected set; }
        public class SendProcessor
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            CMDBLL cmdBLL = null;
            VehicleBLL vehicleBLL = null;
            ReportBLL reportBLL = null;
            TransferBLL transferBLL = null;
            GuideBLL guideBLL = null;
            public SendProcessor(SCApplication scApp)
            {
                cmdBLL = scApp.CMDBLL;
                vehicleBLL = scApp.VehicleBLL;
                reportBLL = scApp.ReportBLL;
                guideBLL = scApp.GuideBLL;
                transferBLL = scApp.TransferBLL;
            }
            //todo kevin 要重新整理SendMessage_ID_31的功能
            #region ID_31 TransferCommand 
            public bool Command(AVEHICLE assignVH, ACMD cmd)
            {
                bool isSuccess = ProcSendTransferCommandToVh(assignVH, cmd);
                if (isSuccess)
                {
                    Task.Run(() => vehicleBLL.web.commandSendCompleteNotify(assignVH.VEHICLE_ID));
                }
                return isSuccess;
            }
            public bool CommandHome(string vhID, string cmdID)
            {
                return sendMessage_ID_31_TRANS_REQUEST(vhID, cmdID, ActiveType.Home, "",
                                                fromAdr: "", destAdr: "",
                                                loadPort: "", unloadPort: "",
                                                null, null, null, null);
            }
            private bool ProcSendTransferCommandToVh(AVEHICLE assignVH, ACMD cmd)
            {
                SCUtility.TrimAllParameter(cmd);
                bool isSuccess = true;
                string vh_id = assignVH.VEHICLE_ID;
                string[] routeSections = null;
                string[] cycleRunSections = null;
                string[] minRouteSec_Vh2From = null;
                string[] minRouteSec_From2To = null;
                string[] minRouteAdr_Vh2From = null;
                string[] minRouteAdr_From2To = null;
                ActiveType active_type = cmdBLL.convertECmdType2ActiveType(cmd.CMD_TYPE);
                try
                {
                    if (scApp.CMDBLL.tryGenerateCmd_OHTC_Details(cmd, out active_type, out routeSections, out cycleRunSections
                                                                             , out minRouteSec_Vh2From, out minRouteSec_From2To
                                                                             , out minRouteAdr_Vh2From, out minRouteAdr_From2To))
                    {
                        if (active_type == ActiveType.Scan || active_type == ActiveType.Load || active_type == ActiveType.Loadunload)
                        {
                            // B0.04 補上原地取貨狀態之說明
                            // B0.04 若取貨之section address 為空 (原地取貨) 則在該guide section 與 guide address 去補上該車目前之位置資訊(因為目前新架構OHT版本需要至少一段section 去判定
                            if (minRouteSec_Vh2From == null || minRouteAdr_Vh2From == null)
                            {
                                if (assignVH.CUR_SEC_ID != null && assignVH.CUR_ADR_ID != null)
                                {
                                    minRouteSec_Vh2From = new string[] { assignVH.CUR_SEC_ID };
                                    minRouteAdr_Vh2From = new string[] { assignVH.CUR_ADR_ID };
                                }
                                else
                                {
                                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: string.Empty,
                                       Data: $"can't generate command road data, something is null,id:{SCUtility.Trim(cmd.ID)},vh id:{SCUtility.Trim(cmd.VH_ID)} current status not allowed." +
                                       $"assignVH.CUR_ADR_ID:{assignVH.CUR_ADR_ID }, assignVH.CUR_SEC_ID:{assignVH.CUR_SEC_ID } , current assign ohtc cmd id:{assignVH.CMD_ID}." +
                                       $"assignVH.ACT_STATUS:{assignVH.ACT_STATUS}.");
                                    return isSuccess;
                                }
                            }
                            // B0.04 補上 LoadUnload 原地放貨狀態之說明 與修改
                            // B0.04 若放貨之section address 為空 (原地放貨) 則在該guide section 與 guide address 去補上該車需要之資訊
                            if (active_type == ActiveType.Loadunload)
                            {
                                if (minRouteSec_From2To == null || minRouteAdr_From2To == null)
                                {
                                    // B0.04 對該string array 補上要去 load 路徑資訊的最後一段address與 section 資料
                                    minRouteSec_From2To = new string[] { minRouteSec_Vh2From[minRouteSec_Vh2From.Length - 1] };
                                    minRouteAdr_From2To = new string[] { minRouteAdr_Vh2From[minRouteAdr_Vh2From.Length - 1] };
                                }
                            }
                        }
                        // B0.04 補上 Unload 原地放貨狀態之說明 與修改
                        // B0.04 若放貨之section address 為空 (原地放貨) 則在該guide section 與 guide address 去補上該車需要之資訊
                        if (active_type == ActiveType.Unload) //B0.04 若為單獨放貨命令，在該空值處補上該車當下之位置資訊。
                        {
                            if (minRouteSec_From2To == null || minRouteAdr_From2To == null)
                            {
                                if (assignVH.CUR_SEC_ID != null && assignVH.CUR_ADR_ID != null)
                                {
                                    minRouteSec_From2To = new string[] { assignVH.CUR_SEC_ID };
                                    minRouteAdr_From2To = new string[] { assignVH.CUR_ADR_ID };
                                }
                                else
                                {
                                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: string.Empty,
                                       Data: $"can't generate command road data, something is null,id:{SCUtility.Trim(cmd.ID)},vh id:{SCUtility.Trim(cmd.VH_ID)} current status not allowed." +
                                       $"assignVH.CUR_ADR_ID:{assignVH.CUR_ADR_ID }, assignVH.CUR_SEC_ID:{assignVH.CUR_SEC_ID } , current assign ohtc cmd id:{assignVH.CMD_ID}." +
                                       $"assignVH.ACT_STATUS:{assignVH.ACT_STATUS}.");
                                    return isSuccess;
                                }
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }

                    bool isTransferCmd = !SCUtility.isEmpty(cmd.TRANSFER_ID);

                    if (isTransferCmd)
                    {
                        reportBLL.newReportTransferInitial(cmd.TRANSFER_ID, null);
                    }
                    List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
                    using (var tx = SCUtility.getTransactionScope())
                    {
                        using (DBConnection_EF con = DBConnection_EF.GetUContext())
                        {
                            isSuccess &= cmdBLL.updateCommand_OHTC_StatusByCmdID(vh_id, cmd.ID, E_CMD_STATUS.Execution);
                            //isSuccess &= vehicleBLL.updateVehicleExcuteCMD(cmd.VH_ID, cmd.ID, cmd.TRANSFER_ID);
                            if (isTransferCmd)
                            {
                                isSuccess &= transferBLL.db.transfer.updateTranStatus2InitialAndExcuteCmdID(cmd.TRANSFER_ID, cmd.ID);
                                isSuccess &= reportBLL.newReportBeginTransfer(cmd.TRANSFER_ID, reportqueues);
                                reportBLL.insertMCSReport(reportqueues);
                            }

                            if (isSuccess)
                            {
                                isSuccess &= sendMessage_ID_31_TRANS_REQUEST
                                    (cmd.VH_ID, cmd.ID, active_type, cmd.CARRIER_ID,
                                     cmd.SOURCE, cmd.DESTINATION,
                                     cmd.SOURCE_PORT, cmd.DESTINATION_PORT,
                                     minRouteSec_Vh2From, minRouteSec_From2To, minRouteAdr_Vh2From, minRouteAdr_From2To
                                     );




                            }
                            if (isSuccess)
                            {
                                tx.Complete();
                            }
                        }
                    }
                    if (isSuccess)
                    {
                        reportBLL.newSendMCSMessage(reportqueues);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exection:");
                    isSuccess = false;
                }
                return isSuccess;
            }






            private bool sendMessage_ID_31_TRANS_REQUEST(string vhID, string cmd_id, ActiveType activeType, string cst_id,
                                                         string fromAdr, string destAdr,
                                                         string loadPort, string unloadPort, string[] minRouteSec_Vh2From, string[] minRouteSec_From2To, string[] minRouteAdr_Vh2From, string[] minRouteAdr_From2To)
            {
                //TODO 要在加入Transfer Command的確認 scApp.CMDBLL.TransferCommandCheck(activeType,) 
                bool isSuccess = true;
                string reason = string.Empty;
                ID_131_TRANS_RESPONSE receive_gpp = null;
                AVEHICLE vh = vehicleBLL.cache.getVehicle(vhID);
                if (isSuccess)
                {
                    ID_31_TRANS_REQUEST send_gpp = new ID_31_TRANS_REQUEST()
                    {
                        CmdID = cmd_id,
                        ActType = activeType,
                        CSTID = cst_id ?? string.Empty,
                        LoadAdr = fromAdr ?? string.Empty,
                        ToAdr = destAdr ?? string.Empty,
                        LoadPortID = loadPort ?? string.Empty,
                        UnloadPortID = unloadPort ?? string.Empty
                    };

                    //ID_31_TRANS_REQUEST send_gpb = new ID_31_TRANS_REQUEST()
                    //{
                    //    CmdID = cmd_id,
                    //    ActType = activeType,
                    //    CSTID = cst_id ?? string.Empty,
                    //    BOXID = box_id ?? string.Empty,
                    //    LOTID = lot_id ?? string.Empty,
                    //    LoadPortID = fromPort_id,
                    //    UnloadPortID = toPort_id,
                    //    LoadAdr = fromAdr,
                    //    ToAdr = toAdr
                    //};
                    SCUtility.RecodeReportInfo(vh.VEHICLE_ID, 0, send_gpp);
                    isSuccess = vh.send_Str31(send_gpp, out receive_gpp, out reason);
                    SCUtility.RecodeReportInfo(vh.VEHICLE_ID, 0, receive_gpp, isSuccess.ToString());
                }
                if (isSuccess)
                {
                    int reply_code = receive_gpp.ReplyCode;
                    if (reply_code != 0)
                    {
                        isSuccess = false;
                        bcf.App.BCFApplication.onWarningMsg(string.Format("發送命令失敗,VH ID:{0}, CMD ID:{1}, Reason:{2}",
                                                                  vhID,
                                                                  cmd_id,
                                                                  reason));
                    }
                    //vh.NotifyVhExcuteCMDStatusChange();
                    vh.onExcuteCommandStatusChange();
                }
                else
                {
                    bcf.App.BCFApplication.onWarningMsg(string.Format("發送命令失敗,VH ID:{0}, CMD ID:{1}, Reason:{2}",
                                              vhID,
                                              cmd_id,
                                              reason));
                    StatusRequest(vhID, true);
                }
                return isSuccess;
            }
            #endregion ID_31 TransferCommand
            #region ID_35 Carrier Rename
            public bool CarrierIDRename(string vh_id, string newCarrierID, string oldCarrierID)
            {
                bool isSuccess = true;
                AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
                //ID_135_CST_ID_RENAME_RESPONSE receive_gpp;
                //ID_35_CST_ID_RENAME_REQUEST send_gpp = new ID_35_CST_ID_RENAME_REQUEST()
                //{
                //    OLDCSTID = oldCarrierID ?? string.Empty,
                //    NEWCSTID = newCarrierID ?? string.Empty,
                //};

                ID_135_CARRIER_ID_RENAME_RESPONSE receive_gpp;
                ID_35_CARRIER_ID_RENAME_REQUEST send_gpp = new ID_35_CARRIER_ID_RENAME_REQUEST()
                {
                    OLDCSTID = oldCarrierID ?? string.Empty,
                    NEWCSTID = newCarrierID ?? string.Empty,
                };

                SCUtility.RecodeReportInfo(vh.VEHICLE_ID, 0, send_gpp);
                isSuccess = vh.send_Str35(send_gpp, out receive_gpp);
                SCUtility.RecodeReportInfo(vh.VEHICLE_ID, 0, receive_gpp, isSuccess.ToString());
                return isSuccess;
            }
            #endregion ID_35 Carrier Rename
            #region ID_37 Cancel
            public bool Cancel(string vhID, string cmd_id, CMDCancelType actType)
            {
                var vh = scApp.VehicleBLL.cache.getVehicle(vhID);
                //bool isSuccess = false;
                //ID_37_TRANS_CANCEL_REQUEST stSend;
                //ID_137_TRANS_CANCEL_RESPONSE stRecv;
                //stSend = new ID_37_TRANS_CANCEL_REQUEST()
                //{
                //    CmdID = cmd_id,
                //    ActType = actType
                //};
                //SCUtility.RecodeReportInfo(vh.VEHICLE_ID, 0, stSend);
                //isSuccess = vh.sned_Str37(stSend, out stRecv);
                //SCUtility.RecodeReportInfo(vh.VEHICLE_ID, 0, stRecv, isSuccess.ToString());

                //return isSuccess;
                return vh.sned_Str37(cmd_id, actType);
            }
            #endregion ID_37 Cancel
            #region ID_41 ModeChange
            public bool ModeChange(string vh_id, OperatingVHMode mode)
            {
                bool isSuccess = false;
                AVEHICLE vh = vehicleBLL.cache.getVehicle(vh_id);
                ID_141_MODE_CHANGE_RESPONSE receive_gpp;
                ID_41_MODE_CHANGE_REQ sned_gpp = new ID_41_MODE_CHANGE_REQ()
                {
                    OperatingVHMode = mode
                };
                SCUtility.RecodeReportInfo(vh_id, 0, sned_gpp);
                isSuccess = vh.send_S41(sned_gpp, out receive_gpp);
                SCUtility.RecodeReportInfo(vh_id, 0, receive_gpp, isSuccess.ToString());
                return isSuccess;
            }
            #endregion ID_41 ModeChange
            #region ID_43 StatusRequest
            public bool StatusRequest(string vhID, bool isSync = false)
            {
                bool isSuccess = false;
                AVEHICLE vh = vehicleBLL.cache.getVehicle(vhID);
                ID_143_STATUS_RESPONSE statusResponse;
                (isSuccess, statusResponse) = sendMessage_ID_43_STATUS_REQUEST(vhID);
                if (isSync && isSuccess)
                {
                    isSuccess = PorcessSendStatusRequestResponse(isSuccess, vh, statusResponse);
                }
                return isSuccess;
            }
            protected virtual bool PorcessSendStatusRequestResponse(bool isSuccess, AVEHICLE vh, ID_143_STATUS_RESPONSE statusReqponse)
            {
                scApp.VehicleBLL.setAndPublishPositionReportInfo2Redis(vh.VEHICLE_ID, statusReqponse);
                VHModeStatus modeStat = scApp.VehicleBLL.DecideVhModeStatus(vh.VEHICLE_ID, statusReqponse.ModeStatus);
                VHActionStatus actionStat = statusReqponse.ActionStatus;
                VhPowerStatus powerStat = statusReqponse.PowerStatus;
                string cmd_id = statusReqponse.CmdID;
                //string cmd_id_1 = statusReqponse.CmdId1;
                //string cmd_id_2 = statusReqponse.CmdId2;
                //string cmd_id_3 = statusReqponse.CmdId3;
                //string cmd_id_4 = statusReqponse.CmdId4;
                //string current_excute_cmd_id = statusReqponse.CurrentExcuteCmdId;
                string cst_id = statusReqponse.CSTID;
                //string cst_id_l = statusReqponse.CstIdL;
                //string cst_id_r = statusReqponse.CstIdR;
                //VhChargeStatus chargeStatus = statusReqponse.ChargeStatus;
                VhStopSingle reserveStatus = statusReqponse.ReserveStatus;
                VhStopSingle obstacleStat = statusReqponse.ObstacleStatus;
                VhStopSingle blockingStat = statusReqponse.BlockingStatus;
                VhStopSingle pauseStat = statusReqponse.PauseStatus;
                VhStopSingle errorStat = statusReqponse.ErrorStatus;
                VhStopSingle safetyPauseStat = statusReqponse.SafetyPauseStatus;
                VhLoadCarrierStatus load_cst_status_l = statusReqponse.HasCst;
                //VhLoadCSTStatus load_cst_status_l = statusReqponse.HasCstL;
                //VhLoadCSTStatus load_cst_status_r = statusReqponse.HasCstR;
                bool has_cst = load_cst_status_l == VhLoadCarrierStatus.Exist;
                //bool has_cst_l = load_cst_status_l == VhLoadCSTStatus.Exist;
                //bool has_cst_r = load_cst_status_r == VhLoadCSTStatus.Exist;
                //string[] will_pass_section_id = statusReqponse.WillPassGuideSection.ToArray();
                int obstacleDIST = statusReqponse.ObstDistance;
                string obstacleVhID = statusReqponse.ObstVehicleID;
                //int steeringWheel = statusReqponse.SteeringWheel;

                //ShelfStatus shelf_status_l = statusReqponse.ShelfStatusL;
                //ShelfStatus shelf_status_r = statusReqponse.ShelfStatusR;
                //VhStopSingle op_pause_status = statusReqponse.OpPauseStatus;

                bool hasdifferent = vh.MODE_STATUS != modeStat ||
                                    vh.ACT_STATUS != actionStat ||
                                    SCUtility.isMatche(vh.CMD_ID, cmd_id) ||
                                    SCUtility.isMatche(vh.CST_ID, cst_id) ||
                                    vh.RESERVE_PAUSE != reserveStatus ||
                                    vh.OBS_PAUSE != obstacleStat ||
                                    vh.BLOCK_PAUSE != blockingStat ||
                                    vh.CMD_PAUSE != pauseStat ||
                                    vh.SAFETY_PAUSE != safetyPauseStat ||
                                    vh.ERROR != errorStat ||
                                    vh.HAS_CST != has_cst
                                    ;

                //if (!SCUtility.isMatche(current_excute_cmd_id, vh.CurrentExcuteCmdID))
                //{
                //    vh.onCurrentExcuteCmdChange(current_excute_cmd_id);
                //}
                if (errorStat != vh.ERROR)
                {
                    vh.onErrorStatusChange(errorStat);
                }
                if (modeStat != vh.MODE_STATUS)
                {
                    vh.onModeStatusChange(modeStat);
                }

                if (hasdifferent)
                {
                    scApp.VehicleBLL.cache.updateVehicleStatus(scApp.CMDBLL, vh.VEHICLE_ID,
                                                         cst_id, modeStat, actionStat,
                                                         blockingStat, pauseStat, obstacleStat, safetyPauseStat, VhStopSingle.StopSingleOff, errorStat, reserveStatus,
                                                         has_cst,
                                                         cmd_id);
                }
                //vh.setCurrentCanAssignCmdCount(shelf_status_l, shelf_status_r);
                return isSuccess;
            }
            private (bool isSuccess, ID_143_STATUS_RESPONSE statusResponse) sendMessage_ID_43_STATUS_REQUEST(string vhID)
            {
                bool isSuccess = false;
                AVEHICLE vh = vehicleBLL.cache.getVehicle(vhID);
                ID_143_STATUS_RESPONSE statusResponse = null;
                ID_43_STATUS_REQUEST send_gpp = new ID_43_STATUS_REQUEST()
                {
                    SystemTime = DateTime.Now.ToString(SCAppConstants.TimestampFormat_16)
                };
                SCUtility.RecodeReportInfo(vhID, 0, send_gpp);
                isSuccess = vh.send_S43(send_gpp, out statusResponse);
                SCUtility.RecodeReportInfo(vhID, 0, statusResponse, isSuccess.ToString());
                return (isSuccess, statusResponse);
            }
            #endregion ID_43 StatusRequest
            #region ID_45 PowerOperatorChange
            public bool PowerOperatorChange(string vhID, OperatingPowerMode mode)
            {
                bool isSuccess = false;
                AVEHICLE vh = vehicleBLL.cache.getVehicle(vhID);
                ID_145_POWER_OPE_RESPONSE receive_gpp;
                ID_45_POWER_OPE_REQ sned_gpp = new ID_45_POWER_OPE_REQ()
                {
                    OperatingPowerMode = mode
                };
                isSuccess = vh.send_S45(sned_gpp, out receive_gpp);
                return isSuccess;
            }
            #endregion ID_45 PowerOperatorChange
            #region ID_51 Avoid
            public (bool is_success, string result) Avoid(string vh_id, string avoidAddress)
            {
                AVEHICLE vh = vehicleBLL.cache.getVehicle(vh_id);
                List<string> guide_segment_ids = null;
                List<string> guide_section_ids = null;
                List<string> guide_address_ids = null;
                int total_cost = 0;
                bool is_success = true;
                string result = "";
                string vh_current_address = SCUtility.Trim(vh.CUR_ADR_ID, true);
                try
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"Start vh:{vh_id} avoid script,avoid to address:{avoidAddress}...",
                       VehicleID: vh.VEHICLE_ID,
                       CarrierID: vh.CST_ID);

                    if (SCUtility.isEmpty(vh.CMD_ID))
                    {
                        is_success = false;
                        result = $"vh:{vh_id} not excute ohtc command.";
                    }
                    if (!vh.IsReservePause)
                    {
                        is_success = false;
                        result = $"vh:{vh_id} current not in reserve pause.";
                    }
                    if (is_success)
                    {
                        int current_find_count = 0;
                        int max_find_count = 10;
                        List<string> need_by_pass_sec_ids = new List<string>();

                        do
                        {
                            //確認下一段Section，是否可以預約成功
                            string next_walk_section = "";
                            string next_walk_address = "";


                            //(is_success, guide_segment_ids, guide_section_ids, guide_address_ids, total_cost) =
                            //    scApp.GuideBLL.getGuideInfo_New2(vh_current_section, vh_current_address, avoidAddress);
                            (is_success, guide_segment_ids, guide_section_ids, guide_address_ids, total_cost) =
                                guideBLL.getGuideInfo(vh_current_address, avoidAddress, need_by_pass_sec_ids);
                            next_walk_section = guide_section_ids[0];
                            next_walk_address = guide_address_ids[0];

                            if (is_success)
                            {

                                var reserve_result = scApp.ReserveBLL.askReserveSuccess(scApp.SectionBLL, vh_id, next_walk_section, next_walk_address);
                                if (!reserve_result.isSuccess &&
                                    SCUtility.isMatche(vh.CanNotReserveInfo.ReservedVhID, reserve_result.reservedVhID))
                                {
                                    is_success = false;
                                    need_by_pass_sec_ids.Add(next_walk_section);
                                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                       Data: $"find the avoid path ,but section:{next_walk_section} is reserved for vh:{reserve_result.reservedVhID}" +
                                             $"add to need by pass sec ids,current by pass section:{string.Join(",", need_by_pass_sec_ids)}",
                                       VehicleID: vh.VEHICLE_ID,
                                       CarrierID: vh.CST_ID);
                                }
                                else
                                {
                                    is_success = true;
                                }
                            }
                            if (current_find_count++ > max_find_count)
                            {
                                is_success = false;
                                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                   Data: $"find the avoid path ,but over times:{max_find_count}",
                                       VehicleID: vh.VEHICLE_ID,
                                       CarrierID: vh.CST_ID);
                                break;
                            }
                        } while (!is_success);

                        string vh_current_section = SCUtility.Trim(vh.CUR_SEC_ID, true);
                        if (is_success)
                        {
                            is_success = sendMessage_ID_51_AVOID_REQUEST(vh_id, avoidAddress, guide_section_ids.ToArray(), guide_address_ids.ToArray());
                            if (!is_success)
                            {
                                result = $"send avoid to vh fail.vh:{vh_id}, vh current adr:{vh_current_address} ,avoid address:{avoidAddress}.";
                            }
                        }
                        else
                        {
                            result = $"find avoid path fail.vh:{vh_id}, vh current adr:{vh_current_address} ,avoid address:{avoidAddress}.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: ex,
                       Details: $"AvoidRequest fail.vh:{vh_id}, vh current adr:{vh_current_address} ,avoid address:{avoidAddress}.",
                       VehicleID: vh_id);
                }
                return (is_success, result);
            }
            private bool sendMessage_ID_51_AVOID_REQUEST(string vh_id, string avoidAddress, string[] guideSection, string[] guideAddresses)
            {
                bool isSuccess = false;
                AVEHICLE vh = vehicleBLL.cache.getVehicle(vh_id);
                ID_151_AVOID_RESPONSE receive_gpp;
                ID_51_AVOID_REQUEST send_gpp = new ID_51_AVOID_REQUEST();
                send_gpp.DestinationAdr = avoidAddress;
                send_gpp.GuideSections.AddRange(guideSection);
                send_gpp.GuideAddresses.AddRange(guideAddresses);

                SCUtility.RecodeReportInfo(vh.VEHICLE_ID, 0, send_gpp);
                isSuccess = vh.send_Str51(send_gpp, out receive_gpp);
                SCUtility.RecodeReportInfo(vh.VEHICLE_ID, 0, receive_gpp, isSuccess.ToString());
                return isSuccess;
            }
            #endregion ID_51 Avoid
            #region ID_39 Pause
            public bool Pause(string vhID, PauseEvent pause_event, PauseType pauseType)
            {
                bool isSuccess = false;
                AVEHICLE vh = vehicleBLL.cache.getVehicle(vhID);
                ID_139_PAUSE_RESPONSE receive_gpp;
                ID_39_PAUSE_REQUEST send_gpp = new ID_39_PAUSE_REQUEST()
                {
                    PauseType = pauseType,
                    EventType = pause_event
                };
                SCUtility.RecodeReportInfo(vh.VEHICLE_ID, 0, send_gpp);
                isSuccess = vh.send_Str39(send_gpp, out receive_gpp);
                SCUtility.RecodeReportInfo(vh.VEHICLE_ID, 0, receive_gpp, isSuccess.ToString());
                return isSuccess;
            }
            #endregion ID_39 Pause
            #region ID_71 Teaching
            public bool Teaching(string vh_id, string from_adr, string to_adr)
            {
                bool isSuccess = false;
                AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
                ID_171_RANGE_TEACHING_RESPONSE receive_gpp;
                ID_71_RANGE_TEACHING_REQUEST send_gpp = new ID_71_RANGE_TEACHING_REQUEST()
                {
                    FromAdr = from_adr,
                    ToAdr = to_adr
                };
                SCUtility.RecodeReportInfo(vh.VEHICLE_ID, 0, send_gpp);
                isSuccess = vh.send_Str71(send_gpp, out receive_gpp);
                SCUtility.RecodeReportInfo(vh.VEHICLE_ID, 0, receive_gpp, isSuccess.ToString());
                return isSuccess;
            }
            #endregion ID_71 Teaching
            #region ID_91 Alamr Reset
            public bool AlarmReset(string vh_id)
            {
                bool isSuccess = false;
                AVEHICLE vh = vehicleBLL.cache.getVehicle(vh_id);
                ID_191_ALARM_RESET_RESPONSE receive_gpp;
                ID_91_ALARM_RESET_REQUEST sned_gpp = new ID_91_ALARM_RESET_REQUEST()
                {

                };
                isSuccess = vh.send_S91(sned_gpp, out receive_gpp);
                if (isSuccess)
                {
                    isSuccess = receive_gpp?.ReplyCode == 0;
                }
                return isSuccess;
            }
            #endregion ID_91 Alamr Reset
        }
        public class ReceiveProcessor : IDynamicMetaObjectProvider
        {
            protected Logger logger = LogManager.GetCurrentClassLogger();
            protected CMDBLL cmdBLL = null;
            protected VehicleBLL vehicleBLL = null;
            protected ReportBLL reportBLL = null;
            protected GuideBLL guideBLL = null;
            protected VehicleService service = null;
            public ReceiveProcessor(VehicleService _service)
            {
                cmdBLL = scApp.CMDBLL;
                vehicleBLL = scApp.VehicleBLL;
                reportBLL = scApp.ReportBLL;
                guideBLL = scApp.GuideBLL;
                service = _service;
            }
            #region ID_132 TransferCompleteReport
            [ClassAOPAspect]
            public void CommandCompleteReport(string tcpipAgentName, BCFApplication bcfApp, AVEHICLE vh, ID_132_TRANS_COMPLETE_REPORT recive_str, int seq_num)
            {
                //scApp.VehicleBLL.setAndPublishPositionReportInfo2Redis(vh.VEHICLE_ID, recive_str);
                try
                {
                    vh.isCommandEnding = true;
                    if (scApp.getEQObjCacheManager().getLine().ServerPreStop)
                        return;
                    SCUtility.RecodeReportInfo(vh.VEHICLE_ID, seq_num, recive_str);
                    string cmd_id = recive_str.CmdID;
                    int travel_dis = recive_str.CmdDistance;
                    CompleteStatus completeStatus = recive_str.CmpStatus;
                    string cur_sec_id = recive_str.CurrentSecID;
                    string cur_adr_id = recive_str.CurrentAdrID;
                    string cur_cst_id = recive_str.CSTID;
                    string vh_id = vh.VEHICLE_ID.ToString();
                    string finish_cmd_id = "";
                    ACMD cmd = scApp.CMDBLL.GetCMD_OHTCByID(cmd_id);
                    if (cmd != null)
                    {
                        if (completeStatus == CompleteStatus.CmpStatusMove ||
                            completeStatus == CompleteStatus.CmpStatusLoad ||
                            completeStatus == CompleteStatus.CmpStatusUnload ||
                            completeStatus == CompleteStatus.CmpStatusLoadunload ||
                            completeStatus == CompleteStatus.CmpStatusMtlhome ||
                            completeStatus == CompleteStatus.CmpStatusMoveToMtl ||
                            completeStatus == CompleteStatus.CmpStatusSystemOut ||
                            completeStatus == CompleteStatus.CmpStatusSystemIn)//20210727 命令正常完成，則直接把車輛移至目的地
                        {
                            string set_address = string.Empty;
                            if (!string.IsNullOrWhiteSpace(cmd.DESTINATION))
                            {
                                set_address = cmd.DESTINATION.Trim();
                            }
                            else
                            {
                                set_address = cmd.SOURCE.Trim();//部分命令種類Destination沒有值，如Move跟Load命令。
                            }
                            scApp.VehicleBLL.setAndPublishPositionReportInfo2Redis(vh.VEHICLE_ID, set_address);
                        }
                    }

                    //scApp.VehicleBLL.setAndPublishPositionReportInfo2Redis(vh.VEHICLE_ID, cur_sec_id, cur_adr_id, 0); //20210609 於命令結束時更新OHT位置

                    //scApp.VehicleBLL.setAndPublishPositionReportInfo2Redis(vh.VEHICLE_ID, recive_str);//20210709因為車子結束命令時上報的Current Address不一定式目的地(可能會報在前一個點)所以取消在132更新位置
                    //using (TransactionScope tx = SCUtility.getTransactionScope())
                    //{
                    //    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    //    {
                    bool is_success = true;
                    var finish_result = service.Command.Finish(cmd_id, completeStatus, travel_dis);
                    is_success = (is_success && finish_result.isSuccess);
                    is_success = is_success && reply_ID_32_TRANS_COMPLETE_RESPONSE(vh, seq_num, finish_cmd_id, finish_result.transferID);
                    if (is_success)
                    {
                        //tx.Complete();
                        vehicleBLL.doInitialVhCommandInfo(vh_id);
                        scApp.VehicleBLL.cache.resetWillPassSectionInfo(vh_id);
                        //2021.9.29 命令結束後解除vehicle - port綁定
                        scApp.TransferService.UnregisterStagedVehicle(vh_id);

                        //當命令結束時主動去觸發一次找尋命令的邏輯，讓車子有機會直接載到門口的貨 add 20210624 by Kevin
                        tryToScnaTransferCommand(vh, completeStatus);
                    }
                    else
                    {
                        return;
                    }
                    //    }
                    //}


                    //vh.NotifyVhExcuteCMDStatusChange();
                    vh.onExcuteCommandStatusChange();
                    vh.onCommandComplete(completeStatus);
                    sendCommandCompleteEventToNats(vh.VEHICLE_ID, recive_str);
                    scApp.ReserveBLL.RemoveAllReservedSectionsByVehicleID(vh.VEHICLE_ID);
                    vh.VhAvoidInfo = null;


                    MaintainLift maintainLift = null;
                    switch (completeStatus)
                    {
                        case CompleteStatus.CmpStatusMoveToMtl:
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"Process vh:{vh.VEHICLE_ID} system out complete, current address:{cur_adr_id},current mode:{vh.MODE_STATUS}",
                               VehicleID: vh.VEHICLE_ID,
                               CarrierID: vh.CST_ID);
                            if (vh.MODE_STATUS == VHModeStatus.AutoMtl)
                            {
                                //在收到OHT的ID:132-SystemOut完成後，創建一個Transfer command，讓Vh移至移動至MTL上
                                //doAskVhToMaintainsAddress(eqpt.VEHICLE_ID, MTLService.MTL_ADDRESS);
                                maintainLift = scApp.EqptBLL.OperateCatch.GetMaintainLiftBySystemOutAdr(cur_adr_id);
                                if (maintainLift != null)
                                {
                                    vh.IsReadyForCarOut = true;
                                    maintainLift.SetCarOutReady(true);// CarOutReady
                                    if (!SpinWait.SpinUntil(() => maintainLift.MTLCarOutSafetyCheck == true, 10000))
                                    {
                                        string result = $"Process car out scenario,but mtl:{maintainLift.DeviceID} status not ready " +
                                        $"{nameof(maintainLift.MTLCarOutInterlock)}:{maintainLift.MTLCarOutInterlock}";
                                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                                                 Data: result,
                                                 XID: maintainLift.DeviceID);
                                    }
                                    else
                                    {
                                        vh.ACT_STATUS = VHActionStatus.NoCommand;
                                        bool create_ok = scApp.VehicleService.doAskVhToMaintainsAddress(vh.VEHICLE_ID, maintainLift.MTL_ADDRESS);
                                        if (create_ok)
                                        {
                                            maintainLift.SetCarOutMoving(true);
                                        }
                                    }
                                }
                            }
                            scApp.ReportBLL.newReportVehicleRemoved(vh.VEHICLE_ID, null);
                            break;
                        case CompleteStatus.CmpStatusSystemOut:
                            maintainLift = scApp.EqptBLL.OperateCatch.GetMaintainLift();
                            //maintainLift = scApp.EqptBLL.OperateCatch.GetMaintainLiftByMTLAdr(cur_adr_id);
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"Process vh:{vh.VEHICLE_ID} move to mtl complete, current address:{cur_adr_id},current mode:{vh.MODE_STATUS}",
                               VehicleID: vh.VEHICLE_ID,
                               CarrierID: vh.CST_ID);
                            if (maintainLift != null)
                            {
                                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                   Data: $"Process vh:{vh.VEHICLE_ID} move to mtl complete, notify mtx:{maintainLift.DeviceID} is complete",
                                   VehicleID: vh.VEHICLE_ID,
                                   CarrierID: vh.CST_ID);
                                //1.通知MTL Car out完成
                                scApp.MTLService.carOutAtMTLComplete(maintainLift);
                                //2.將該VH上報 Remove
                                //scApp.VehicleService.Remove(vh.VEHICLE_ID, true);
                            }
                            break;
                        case CompleteStatus.CmpStatusMtlhome:
                            maintainLift = scApp.EqptBLL.OperateCatch.GetMaintainLift();
                            //maintainLift = scApp.EqptBLL.OperateCatch.GetMaintainLiftByMTLHomeAdr(cur_adr_id);
                            if (maintainLift != null && maintainLift.MTLCarInSafetyCheck)
                            {
                                vh.ACT_STATUS = VHActionStatus.NoCommand;
                                scApp.VehicleService.doAskVhToSystemInAddress(vh.VEHICLE_ID, maintainLift.MTL_SYSTEM_IN_ADDRESS);
                            }
                            //doAskVhToSystemInAddress(eqpt.VEHICLE_ID, MTLService.MTL_SYSTEM_IN_ADDRESS);
                            break;
                        case CompleteStatus.CmpStatusSystemIn:
                            var maintain_device = scApp.EqptBLL.OperateCatch.GetMaintainLift();
                            //var maintain_device = scApp.EqptBLL.OperateCatch.GetMaintainDeviceBySystemInAdr(cur_adr_id);
                            if (maintain_device != null)
                            {
                                scApp.MTLService.carInComplete(maintain_device, vh.VEHICLE_ID);
                                if (maintain_device is MaintainLift)
                                {
                                    scApp.VehicleService.Install(vh.VEHICLE_ID);
                                    //InstallNew(eqpt.VEHICLE_ID);
                                }
                            }
                            break;
                        case CompleteStatus.CmpStatusCancel:
                            maintainLift = scApp.EqptBLL.OperateCatch.GetMaintainLift();
                            //maintainLift = scApp.EqptBLL.OperateCatch.GetMaintainLiftByMTLHomeAdr(cur_adr_id);
                            if (maintainLift != null)
                            {
                                if (SCUtility.isMatche(maintainLift.PreCarOutVhID, vh.VEHICLE_ID))
                                {
                                    scApp.MTLService.carOutCancelComplete(maintainLift);

                                }
                            }
                            break;
                            //default:
                            //    if (eqpt.MODE_STATUS == VHModeStatus.AutoMtl && eqpt.HAS_CST == 0)
                            //    {
                            //        maintainLift = scApp.EquipmentBLL.cache.GetExcuteCarOutMTL(eqpt.VEHICLE_ID);
                            //        if (maintainLift != null)
                            //        {
                            //            if (maintainLift.DokingMaintainDevice != null && maintainLift.DokingMaintainDevice.CarOutSafetyCheck)
                            //            {
                            //                if (maintainLift.DokingMaintainDevice.CarOutSafetyCheck)//如果SafetyCheck已經解除則不能進行出車
                            //                {
                            //                    doAskVhToSystemOutAddress(eqpt.VEHICLE_ID, maintainLift.MTL_SYSTEM_OUT_ADDRESS);
                            //                }
                            //            }
                            //            else
                            //            {
                            //                if (maintainLift.CarOutSafetyCheck)//如果SafetyCheck已經解除則不能進行出車
                            //                {
                            //                    doAskVhToMaintainsAddress(eqpt.VEHICLE_ID, maintainLift.MTL_ADDRESS);
                            //                }
                            //            }

                            //        }

                            //    }
                            //    else if (eqpt.MODE_STATUS == VHModeStatus.AutoMts && eqpt.HAS_CST == 0)
                            //    {
                            //        maintainSpace = scApp.EquipmentBLL.cache.GetExcuteCarOutMTS(eqpt.VEHICLE_ID);
                            //        if (maintainSpace != null && maintainSpace.CarOutSafetyCheck)
                            //            doAskVhToSystemOutAddress(eqpt.VEHICLE_ID, maintainSpace.MTS_ADDRESS);
                            //    }
                            //    else if ((eqpt.MODE_STATUS == VHModeStatus.AutoRemote) && eqpt.HAS_CST == 0)
                            //    {
                            //        scApp.CMDBLL.checkMCS_TransferCommand();
                            //        scApp.VehicleBLL.DoIdleVehicleHandle_NoAction(eqpt.VEHICLE_ID);
                            //    }
                            break;
                    }


                    if (scApp.getEQObjCacheManager().getLine().SCStats == ALINE.TSCState.PAUSING)
                    {
                        List<ATRANSFER> cmd_mcs_lst = scApp.CMDBLL.loadUnfinishedTransfer();
                        if (cmd_mcs_lst.Count == 0)
                        {
                            scApp.LineService.TSCStateToPause();
                        }
                    }
                    //tryAskVh2ChargerIdle(vh);
                }
                finally
                {
                    vh.isCommandEnding = false;
                }
            }

            private void tryToScnaTransferCommand(AVEHICLE vh, CompleteStatus completeStatus)
            {
                if (vh.MODE_STATUS == VHModeStatus.AutoRemote &&
                    completeStatus == CompleteStatus.CmpStatusMove)
                {
                    bool is_wait_scan_time_out = !SpinWait.SpinUntil(() => !scApp.TransferService.isTransferCommandScanning(), 3000);
                    if (is_wait_scan_time_out)
                    {
                        //time out的話就代表無法進入執行transfer command的scan
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                           Data: $"vh:{vh.VEHICLE_ID} move command finish, but wait to scan transfer command time out",
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                    }
                    else
                    {
                        scApp.TransferService.ScanByVTransfer_v3(finishCmdVh: vh);
                    }
                }
            }



            /// <summary>
            /// 如果等待時間超過了"MAX_WAIT_COMMAND_TIME"，
            /// 就可以讓車子回去充電站待命了。
            /// </summary>
            /// <param name="vh"></param>
            //const int MAX_WAIT_COMMAND_TIME = 10000;
            //private void tryAskVh2ChargerIdle(AVEHICLE vh)
            //{
            //    string vh_id = vh.VEHICLE_ID;
            //    SpinWait.SpinUntil(() => false, 3000);
            //    bool has_cmd_excute = SpinWait.SpinUntil(() => scApp.CMDBLL.cache.hasCmdExcute(vh_id), MAX_WAIT_COMMAND_TIME);
            //    if (!has_cmd_excute)
            //    {
            //        scApp.VehicleChargerModule.askVhToChargerForWait(vh);
            //    }
            //}



            private bool reply_ID_32_TRANS_COMPLETE_RESPONSE(AVEHICLE vh, int seq_num, string finish_cmd_id, string finish_fransfer_cmd_id)
            {
                ID_32_TRANS_COMPLETE_RESPONSE send_str = new ID_32_TRANS_COMPLETE_RESPONSE
                {
                    ReplyCode = 0,
                };
                WrapperMessage wrapper = new WrapperMessage
                {
                    SeqNum = seq_num,
                    TranCmpResp = send_str
                };
                Boolean resp_cmp = vh.sendMessage(wrapper, true);
                SCUtility.RecodeReportInfo(vh.VEHICLE_ID, seq_num, send_str, finish_cmd_id, finish_fransfer_cmd_id, resp_cmp.ToString());
                return resp_cmp;
            }


            private void sendCommandCompleteEventToNats(string vhID, ID_132_TRANS_COMPLETE_REPORT recive_str)
            {
                byte[] arrayByte = new byte[recive_str.CalculateSize()];
                recive_str.WriteTo(new Google.Protobuf.CodedOutputStream(arrayByte));
                scApp.getNatsManager().PublishAsync
                    (string.Format(SCAppConstants.NATS_SUBJECT_VH_COMMAND_COMPLETE_0, vhID), arrayByte);
            }

            #endregion ID_132 TransferCompleteReport
            #region ID_134 TransferEventReport (Position)
            [ClassAOPAspect]
            public void PositionReport(BCFApplication bcfApp, AVEHICLE vh, ID_134_TRANS_EVENT_REP receiveStr, int current_seq_num)
            {
                if (scApp.getEQObjCacheManager().getLine().ServerPreStop)
                    return;
                SCUtility.RecodeReportInfo(vh.VEHICLE_ID, current_seq_num, receiveStr);
                int pre_position_seq_num = vh.PrePositionSeqNum;
                bool need_process_position = checkPositionSeqNum(current_seq_num, pre_position_seq_num);
                vh.PrePositionSeqNum = current_seq_num;
                if (!need_process_position)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleBLL), Device: Service.VehicleService.DEVICE_NAME_OHx,
                       Data: $"The vehicles updata position report of seq num is old,by pass this one.old seq num;{pre_position_seq_num},current seq num:{current_seq_num}",
                       VehicleID: vh.VEHICLE_ID,
                       CarrierID: vh.CST_ID);
                    return;
                }

                scApp.VehicleBLL.setAndPublishPositionReportInfo2Redis(vh.VEHICLE_ID, receiveStr);



                EventType eventType = receiveStr.EventType;
                string current_adr_id = SCUtility.isEmpty(receiveStr.CurrentAdrID) ? string.Empty : receiveStr.CurrentAdrID;
                string current_sec_id = SCUtility.isEmpty(receiveStr.CurrentSecID) ? string.Empty : receiveStr.CurrentSecID;
                ASECTION sec_obj = scApp.SectionBLL.cache.GetSection(current_sec_id);
                string current_seg_id = sec_obj == null ? string.Empty : sec_obj.SEG_NUM;
                string last_adr_id = vh.CUR_ADR_ID;
                string last_sec_id = vh.CUR_SEC_ID;
                uint sec_dis = receiveStr.SecDistance;

                //scApp.ReportBLL.newReportRunTimetatus(vh.VEHICLE_ID); //20210424 MCS 說暫時不要報
            }
            const int TOLERANCE_SCOPE = 50;
            private const ushort SEQNUM_MAX = 999;
            private bool checkPositionSeqNum(int currnetNum, int preNum)
            {

                int lower_limit = preNum - TOLERANCE_SCOPE;
                if (lower_limit >= 0)
                {
                    //如果該次的Num介於上次的值減去容錯值(TOLERANCE_SCOPE = 50) 至 上次的值
                    //就代表是舊的資料
                    if (currnetNum > (lower_limit) && currnetNum < preNum)
                    {
                        return false;
                    }
                }
                else
                {
                    //如果上次的值減去容錯值變成負的，代表要再由SENDSEQNUM_MAX往回推
                    lower_limit = SEQNUM_MAX + lower_limit;
                    if (currnetNum > (lower_limit) && currnetNum < preNum)
                    {
                        return false;
                    }
                }
                return true;
            }



            #endregion ID_134 TransferEventReport (Position)
            #region ID_136 TransferEventReport
            [ClassAOPAspect]
            public virtual void TranEventReport(BCFApplication bcfApp, AVEHICLE vh, ID_136_TRANS_EVENT_REP recive_str, int seq_num)
            {
                if (scApp.getEQObjCacheManager().getLine().ServerPreStop)
                    return;

                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   seq_num: seq_num,
                   Data: recive_str,
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                SCUtility.RecodeReportInfo(vh.VEHICLE_ID, seq_num, recive_str);

                EventType eventType = recive_str.EventType;
                string current_adr_id = recive_str.CurrentAdrID;
                string current_sec_id = recive_str.CurrentSecID;
                string carrier_id = recive_str.BOXID;
                string last_adr_id = vh.CUR_ADR_ID;
                string last_sec_id = vh.CUR_SEC_ID;
                string req_block_id = recive_str.RequestBlockID;
                string cmd_id = vh.TRANSFER_ID;
                BCRReadResult bCRReadResult = recive_str.BCRReadResult;
                string load_port_id = recive_str.LoadPortID;     //B0.01
                string unload_port_id = recive_str.UnloadPortID; //B0.01
                var reserveInfos = recive_str.ReserveInfos;

                //scApp.VehicleBLL.updateVehicleActionStatus(vh, eventType);


                switch (eventType)
                {
                    //case EventType.BlockReq:
                    //case EventType.Hidreq:
                    //case EventType.BlockHidreq:
                    //    ProcessBlockOrHIDReq(bcfApp, vh, eventType, seq_num, recive_str.RequestBlockID, recive_str.RequestHIDID);
                    //    break;
                    case EventType.ReserveReq:
                        if (DebugParameter.testRetryReserveReq) return;
                        TranEventReport_PathReserveReq(bcfApp, vh, seq_num, reserveInfos);
                        break;
                    case EventType.LoadArrivals:
                        if (DebugParameter.testRetryLoadArrivals) return;
                        TranEventReport_LoadArrivals(bcfApp, vh, seq_num, eventType, vh.CMD_ID);
                        break;
                    case EventType.LoadComplete:
                        if (DebugParameter.testRetryLoadComplete) return;
                        TranEventReport_LoadComplete(bcfApp, vh, seq_num, eventType, vh.CMD_ID);
                        break;
                    case EventType.UnloadArrivals:
                        if (DebugParameter.testRetryUnloadArrivals) return;
                        TranEventReport_UnloadArrive(bcfApp, vh, seq_num, eventType, vh.CMD_ID);
                        break;
                    case EventType.UnloadComplete:
                        if (DebugParameter.testRetryUnloadComplete) return;
                        //TranEventReport_UnloadComplete(bcfApp, vh, seq_num, eventType, excute_cmd_id);
                        TranEventReport_UnloadComplete(bcfApp, vh, seq_num, eventType, vh.CMD_ID);
                        break;
                    case EventType.Vhloading:
                        if (DebugParameter.testRetryVhloading) return;
                        TranEventReport_Loading(bcfApp, vh, seq_num, eventType, vh.CMD_ID);
                        break;
                    case EventType.Vhunloading:
                        if (DebugParameter.testRetryVhunloading) return;
                        TranEventReport_Unloading(bcfApp, vh, seq_num, eventType, vh.CMD_ID);
                        break;
                    //case EventType.BlockRelease:
                    //    PositionReport_BlockRelease(bcfApp, eqpt, recive_str, seq_num);
                    //    replyTranEventReport(bcfApp, recive_str.EventType, eqpt, seq_num);
                    //    break;
                    //case EventType.Hidrelease:
                    //    PositionReport_HIDRelease(bcfApp, eqpt, recive_str, seq_num);
                    //    replyTranEventReport(bcfApp, recive_str.EventType, eqpt, seq_num);
                    //    break;
                    //case EventType.BlockHidrelease:
                    //    PositionReport_BlockRelease(bcfApp, eqpt, recive_str, seq_num);
                    //    PositionReport_HIDRelease(bcfApp, eqpt, recive_str, seq_num);
                    //    replyTranEventReport(bcfApp, recive_str.EventType, eqpt, seq_num);
                    //    break;
                    case EventType.DoubleStorage:
                        PositionReport_DoubleStorage(bcfApp, vh, seq_num, recive_str.EventType, recive_str.CurrentAdrID, recive_str.CurrentSecID, carrier_id);
                        break;
                    case EventType.EmptyRetrieval:
                        PositionReport_EmptyRetrieval(bcfApp, vh, seq_num, recive_str.EventType, recive_str.CurrentAdrID, recive_str.CurrentSecID, carrier_id);
                        break;

                    case EventType.Bcrread:
                        if (DebugParameter.testRetryBcrread) return;
                        TranEventReport_BCRRead(bcfApp, vh, seq_num, eventType, carrier_id, bCRReadResult, vh.CMD_ID);
                        break;
                    //case EventType.Cstremove:
                    //    TranEventReport_CSTRemove(bcfApp, vh, seq_num, eventType, cst_location, carrier_id, excute_cmd_id);
                    //    break;
                    default:
                        replyTranEventReport(bcfApp, eventType, vh, seq_num);
                        break;
                }
            }
            private void PositionReport_DoubleStorage(BCFApplication bcfApp, AVEHICLE eqpt, int seqNum
                                            , EventType eventType, string current_adr_id, string current_sec_id, string carrier_id)
            {
                try
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                    Data: $"Process report {eventType}",
                    VehicleID: eqpt.VEHICLE_ID,
                    CarrierID: eqpt.CST_ID);

                    if (!SCUtility.isEmpty(eqpt.TRANSFER_ID))
                    {
                        //bool retryOrAbort = true;
                        //retryOrAbort = scApp.TransferService.OHT_TransferStatus(eqpt.OHTC_CMD,
                        //        eqpt.VEHICLE_ID, ACMD_MCS.COMMAND_STATUS_BIT_INDEX_DOUBLE_STORAGE);
                        Boolean resp_cmp;

                        resp_cmp = replyTranEventReport(bcfApp, eventType, eqpt, seqNum, true, true, true, "", CMDCancelType.CmdCancel);

                    }
                    else
                    {
                        replyTranEventReport(bcfApp, eventType, eqpt, seqNum, true, true, true, "", CMDCancelType.CmdCancel);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: ex,
                       VehicleID: eqpt.VEHICLE_ID,
                       CarrierID: eqpt.CST_ID);
                }
            }

            private void PositionReport_EmptyRetrieval(BCFApplication bcfApp, AVEHICLE eqpt, int seqNum
                                                        , EventType eventType, string current_adr_id, string current_sec_id, string carrier_id)
            {
                try
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                    Data: $"Process report {eventType}",
                    VehicleID: eqpt.VEHICLE_ID,
                    CarrierID: eqpt.CST_ID);

                    if (!SCUtility.isEmpty(eqpt.TRANSFER_ID))
                    {
                        //bool retryOrAbort = true;
                        //retryOrAbort = scApp.TransferService.OHT_TransferStatus(eqpt.OHTC_CMD,
                        //        eqpt.VEHICLE_ID, ACMD_MCS.COMMAND_STATUS_BIT_INDEX_EMPTY_RETRIEVAL);
                        Boolean resp_cmp;
                        resp_cmp = replyTranEventReport(bcfApp, eventType, eqpt, seqNum, true, true, true, "", CMDCancelType.CmdCancel);
                    }
                    else
                    {
                        replyTranEventReport(bcfApp, eventType, eqpt, seqNum, true, true, true, "", CMDCancelType.CmdCancel);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: ex,
                       VehicleID: eqpt.VEHICLE_ID,
                       CarrierID: eqpt.CST_ID);
                }
            }
            private void ProcessBlockOrHIDReq(BCFApplication bcfApp, AVEHICLE eqpt, EventType eventType, int seqNum, string req_block_id, string req_hid_secid)
            {
                bool can_block_pass = true;
                bool can_hid_pass = true;
                bool isSuccess = false;
                using (TransactionScope tx = SCUtility.getTransactionScope())
                {
                    if (eventType == EventType.BlockReq || eventType == EventType.BlockHidreq)
                        can_block_pass = ProcessBlockReqNew(bcfApp, eqpt, req_block_id);
                    if (eventType == EventType.Hidreq || eventType == EventType.BlockHidreq)
                        can_hid_pass = ProcessHIDRequest(bcfApp, eqpt, req_hid_secid);
                    isSuccess = replyTranEventReport(bcfApp, eventType, eqpt, seqNum, canBlockPass: can_block_pass, canHIDPass: can_hid_pass);
                    if (isSuccess)
                    {
                        tx.Complete();
                    }
                }

                //if (isSuccess &&
                //    (eventType == EventType.Hidreq || eventType == EventType.BlockHidreq))
                //{
                //    scApp.HIDBLL.VHEntryHIDZone(req_hid_secid);
                //    Task.Run(() => checkHIDSpaceIsSufficient(eqpt, req_hid_secid));
                //}
            }
            private bool ProcessBlockReqNew(BCFApplication bcfApp, AVEHICLE eqpt, string req_block_id)
            {
                bool canBlockPass = false;
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"Process block request,request block id:{req_block_id}",
                   VehicleID: eqpt.VEHICLE_ID,
                   CarrierID: eqpt.CST_ID);
                //if (DebugParameter.isForcedPassBlockControl)
                //{
                //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                //       Data: "test flag: Force pass block control is open, will driect reply to vh can pass block",
                //       VehicleID: eqpt.VEHICLE_ID,
                //       CarrierID: eqpt.CST_ID);
                //    canBlockPass = true;
                //}
                //else if (DebugParameter.isForcedRejectBlockControl)
                //{
                //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                //       Data: "test flag: Force reject block control is open, will driect reply to vh can't pass block",
                //       VehicleID: eqpt.VEHICLE_ID,
                //       CarrierID: eqpt.CST_ID);
                //    canBlockPass = false;
                //}
                //else
                //{
                //    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                //    {
                //        //先確認在Redis上是否該台VH 已經有要過的Block
                //        //bool hasAskedBlock = scApp.MapBLL.HasBlockControlAskedFromRedis
                //        //    (eqpt.VEHICLE_ID, out string current_asked_block_id, out string current_asked_block_status);
                //        List<BLOCKZONEQUEUE> ask_block_queues = scApp.MapBLL.loadNonReleaseBlockQueueByCarID(eqpt.VEHICLE_ID);
                //        bool hasAskedBlock = ask_block_queues != null && ask_block_queues.Count > 0;
                //        if (hasAskedBlock)
                //        {
                //            //bool isBlocking = SCUtility.isMatche(current_asked_block_status, SCAppConstants.BlockQueueState.Blocking)
                //            //               || SCUtility.isMatche(current_asked_block_status, SCAppConstants.BlockQueueState.Through);

                //            //確認當前要的Block是否有存在目前的DB中。
                //            BLOCKZONEQUEUE current_request_again_block_queue = ask_block_queues.
                //                                                         Where(queue => SCUtility.isMatche(queue.ENTRY_SEC_ID, req_block_id)).
                //                                                         FirstOrDefault();
                //            //if (SCUtility.isMatche(req_block_id, current_asked_block_id))
                //            if (current_request_again_block_queue != null)
                //            {
                //                //如果要的是同一個，則確認是否已經給該台VH
                //                //if (isBlocking)
                //                if (SCUtility.isMatche(current_request_again_block_queue.STATUS, SCAppConstants.BlockQueueState.Blocking) ||
                //                    SCUtility.isMatche(current_request_again_block_queue.STATUS, SCAppConstants.BlockQueueState.Through))
                //                {
                //                    //如果已經給過該台VH通行權，則直接讓它通過。
                //                    canBlockPass = true;
                //                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                //                       Data: $"Vh:{eqpt.VEHICLE_ID} ask again block:{req_block_id},but it is the owner so ask result:{canBlockPass}",
                //                       VehicleID: eqpt.VEHICLE_ID,
                //                       CarrierID: eqpt.CST_ID);
                //                }
                //                else
                //                {
                //                    //如果還沒有給過該台VH通行權，則需再判斷一次該Vh是否已經可以通過
                //                    canBlockPass = canPassBlockZone(eqpt, req_block_id);
                //                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                //                       Data: $"Vh:{eqpt.VEHICLE_ID} ask again block:{req_block_id},ask result:{canBlockPass}",
                //                       VehicleID: eqpt.VEHICLE_ID,
                //                       CarrierID: eqpt.CST_ID);
                //                    if (canBlockPass)
                //                    {
                //                        scApp.MapBLL.updateBlockZoneQueue_BlockTime(eqpt.VEHICLE_ID, req_block_id);
                //                        scApp.MapBLL.ChangeBlockControlStatus_Blocking(eqpt.VEHICLE_ID);
                //                    }
                //                }
                //            }
                //            else
                //            {
                //                bool has_in_request = ask_block_queues.Where(queue => SCUtility.isMatche(queue.STATUS, SCAppConstants.BlockQueueState.Request))
                //                                                      .Count() > 0;
                //                string[] current_using_block_ids = ask_block_queues.Select(queue => queue.ENTRY_SEC_ID).ToArray();
                //                //如果不是同一個，則要判斷目前asked的Blocks狀態是否沒有在Request中的                           
                //                //if (isBlocking)
                //                if (!has_in_request)
                //                {
                //                    //如果是的話才可以再進行新的BlockControlRequest的建立流程
                //                    canBlockPass = tryCreatBlockControlRequest(eqpt, req_block_id);
                //                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                //                       Data: $"Vh:{eqpt.VEHICLE_ID} already has a block:{string.Join(",", current_using_block_ids)}," +
                //                       $"asking for another one at a time ,block:{req_block_id}, ask result:{canBlockPass}",
                //                       VehicleID: eqpt.VEHICLE_ID,
                //                       CarrierID: eqpt.CST_ID);
                //                }
                //                else
                //                {
                //                    //如果不是，則不可以再給他另外一個Block
                //                    canBlockPass = false;
                //                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                //                       Data: $"Vh:{eqpt.VEHICLE_ID} already has a block:{string.Join(",", current_using_block_ids)}," +
                //                       $"but the status is Request,so ask block:{req_block_id} result:{canBlockPass}",
                //                       VehicleID: eqpt.VEHICLE_ID,
                //                       CarrierID: eqpt.CST_ID);
                //                    DateTime reqest_time = DateTime.Now;
                //                    //scApp.MapBLL.doCreatBlockZoneQueueByReqStatus(eqpt.VEHICLE_ID, req_block_id, canBlockPass, reqest_time);
                //                    //scApp.MapBLL.CreatBlockControlKeyWordToRedis(eqpt.VEHICLE_ID, req_block_id, canBlockPass, reqest_time);
                //                }
                //            }
                //        }
                //        else
                //        {
                //            //如果目前Redis上沒有要求的Block的話，則可以嘗試建立新的BlocControlRequest，
                //            //並判斷是否可以給其通行權
                //            canBlockPass = tryCreatBlockControlRequest(eqpt, req_block_id);
                //            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                //               Data: $"Vh:{eqpt.VEHICLE_ID} ask block:{req_block_id},ask result:{canBlockPass}",
                //               VehicleID: eqpt.VEHICLE_ID,
                //               CarrierID: eqpt.CST_ID);
                //        }
                //    }
                //}
                return canBlockPass;
            }

            private bool ProcessHIDRequest(BCFApplication bcfApp, AVEHICLE eqpt, string req_hid_secid)
            {
                bool isSuccess = true;
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    DateTime req_dateTime = DateTime.Now;

                    scApp.HIDBLL.doCreatHIDZoneQueueByReqStatus(eqpt.VEHICLE_ID, req_hid_secid, true, req_dateTime);
                }
                return isSuccess;
            }

            object reserve_lock = new object();
            private void TranEventReport_PathReserveReq(BCFApplication bcfApp, AVEHICLE vh, int seqNum, RepeatedField<ReserveInfo> reserveInfos)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"Process path reserve request,request path id:{reserveInfos.ToString()}",
               VehicleID: vh.VEHICLE_ID,
               CarrierID: vh.CST_ID);

                lock (reserve_lock)
                {
                    //var ReserveResult = scApp.ReserveBLL.IsReserveSuccessNew(vh.VEHICLE_ID, reserveInfos);
                    var ReserveResult = scApp.ReserveBLL.IsMultiReserveSuccess(scApp, vh.VEHICLE_ID, reserveInfos);
                    if (ReserveResult.isSuccess)
                    {
                        scApp.VehicleBLL.cache.ResetCanNotReserveInfo(vh.VEHICLE_ID);//TODO Mark check
                                                                                     //防火門機制要檢查其影響區域有沒要被預約了。
                                                                                     //if (scApp.getCommObjCacheManager().isSectionAtFireDoorArea(ReserveResult.reservedSecID))
                                                                                     //{
                                                                                     //    //Task. scApp.getCommObjCacheManager().sectionReserveAtFireDoorArea(reserveInfo.Value);
                                                                                     //    Task.Run(() => scApp.getCommObjCacheManager().sectionReserveAtFireDoorArea(ReserveResult.reservedSecID));
                                                                                     //}
                    }
                    else
                    {
                        //string reserve_fail_section = reserveInfos[0].ReserveSectionID;
                        string reserve_fail_section = ReserveResult.reservedFailSection;
                        ASECTION reserve_fail_sec_obj = scApp.SectionBLL.cache.GetSection(reserve_fail_section);
                        scApp.VehicleBLL.cache.SetUnsuccessReserveInfo(vh.VEHICLE_ID, new AVEHICLE.ReserveUnsuccessInfo(ReserveResult.reservedVhID, "", reserve_fail_section));
                        Task.Run(() => service.Avoid.tryNotifyVhAvoid(vh.VEHICLE_ID, ReserveResult.reservedVhID));
                    }
                    replyTranEventReport(bcfApp, EventType.ReserveReq, vh, seqNum,
                                         canReservePass: ReserveResult.isSuccess,
                                         reserveInfos: ReserveResult.reserveSuccessInfos);
                }
            }
            //protected void TranEventReport_CSTRemove(BCFApplication bcfApp, AVEHICLE vh, int seq_num, EventType eventType, string cstID, string excute_cmd_id)
            //{
            //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
            //               Data: $"Process  cst remove event:{eventType} cst id:{cstID} cmd id:{excute_cmd_id}",
            //               VehicleID: vh.VEHICLE_ID,
            //               CarrierID: vh.CST_ID);

            //    if (vh.HAS_CST==1)
            //    {
            //        var on_vh_carrier = check_cst_location_result.onVhCarrier;

            //        LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
            //                   Data: $"Process cst remove event:{eventType} cst id:{cstID} cmd id:{excute_cmd_id},find the cst:{on_vh_carrier.ID} start remove it...",
            //                   VehicleID: vh.VEHICLE_ID,
            //               VehicleID: vh.VEHICLE_ID,
            //               CarrierID: vh.CST_ID);

            //        //var remove_result = scApp.TransferService.ForceRemoveCarrierInVehicleByAGV(on_vh_carrier.ID, Location, "");
            //        var remove_result = scApp.TransferService.ForceRemoveCarrierInVehicleByAGV(vh.VEHICLE_ID, Location, "");

            //        LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
            //                   Data: $"Process cst remove event:{eventType} cst id:{cstID} cmd id:{excute_cmd_id} agv location:{Location},remove result:{remove_result.result}",
            //                   VehicleID: vh.VEHICLE_ID,
            //                   CST_ID_L: vh.CST_ID_L,
            //                   CST_ID_R: vh.CST_ID_R);
            //    }
            //    replyTranEventReport(bcfApp, eventType, vh, seq_num, excute_cmd_id);
            //}

            protected virtual void TranEventReport_LoadArrivals(BCFApplication bcfApp, AVEHICLE vh, int seqNum
                                                    , EventType eventType, string cmdID)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"Process report {eventType}",
                   VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                ACMD cmd = scApp.CMDBLL.GetCMD_OHTCByID(cmdID);
                //vh.LastLoadCompleteCommandID = cmdID;
                bool isTranCmd = !SCUtility.isEmpty(cmd.TRANSFER_ID);
                if (isTranCmd)
                {
                    scApp.TransferBLL.db.transfer.updateTranStatus2LoadArrivals(cmd.TRANSFER_ID);
                    List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
                    using (TransactionScope tx = SCUtility.getTransactionScope())
                    {
                        using (DBConnection_EF con = DBConnection_EF.GetUContext())
                        {
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"do report {eventType} to mcs.",
                   VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                            bool isCreatReportInfoSuccess = scApp.ReportBLL.newReportLoadArrivals(cmd.TRANSFER_ID, reportqueues);
                            if (!isCreatReportInfoSuccess)
                            {
                                return;
                            }
                            scApp.ReportBLL.insertMCSReport(reportqueues);
                        }
                        Boolean resp_cmp = replyTranEventReport(bcfApp, eventType, vh, seqNum);
                        if (resp_cmp)
                        {
                            tx.Complete();
                        }
                        else
                        {
                            return;
                        }
                    }
                    scApp.ReportBLL.newSendMCSMessage(reportqueues);
                    scApp.SysExcuteQualityBLL.updateSysExecQity_ArrivalSourcePort(cmd.TRANSFER_ID);
                }
                else
                {
                    replyTranEventReport(bcfApp, eventType, vh, seqNum);
                }

                scApp.VehicleBLL.doLoadArrivals(vh.VEHICLE_ID);
                scApp.ReserveBLL.RemoveAllReservedSectionsByVehicleID(vh.VEHICLE_ID);
            }
            protected virtual void TranEventReport_LoadComplete(BCFApplication bcfApp, AVEHICLE vh, int seqNum
                                                    , EventType eventType, string cmdID)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                           Data: $"Process report {eventType}",
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                //vh.IsCloseToAGVStation = false;
                //scApp.MapBLL.getPortID(vh.CUR_ADR_ID, out string port_id);
                ACMD cmd = scApp.CMDBLL.GetCMD_OHTCByID(cmdID);
                //vh.LastLoadCompleteCommandID = cmdID;
                updateCarrierInVehicleLocation(vh, cmd, "");
                bool isTranCmd = !SCUtility.isEmpty(cmd.TRANSFER_ID);
                if (isTranCmd)
                {
                    string transfer_id = cmd.TRANSFER_ID;
                    //scApp.TransferBLL.db.transfer.updateTranStatus2Transferring(transfer_id); 20210517 Mark By Mark Chou 將變更命令狀態為Transfer改為在BCRRead時進行
                    List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
                    using (TransactionScope tx = SCUtility.getTransactionScope())
                    {
                        using (DBConnection_EF con = DBConnection_EF.GetUContext())
                        {
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"do report {eventType} to mcs.",
                               VehicleID: vh.VEHICLE_ID,
                               CarrierID: vh.CST_ID);
                            bool isCreatReportInfoSuccess = scApp.ReportBLL.newReportLoadComplete(cmd.TRANSFER_ID, reportqueues);
                            if (!isCreatReportInfoSuccess)
                            {
                                return;
                            }
                            scApp.ReportBLL.insertMCSReport(reportqueues);
                        }

                        Boolean resp_cmp = replyTranEventReport(bcfApp, eventType, vh, seqNum);

                        if (resp_cmp)
                        {
                            tx.Complete();
                        }
                        else
                        {
                            return;
                        }
                    }
                    scApp.ReportBLL.newSendMCSMessage(reportqueues);
                }
                else
                {
                    //if (!SCUtility.isEmpty(cmd.CARRIER_ID))
                    //    scApp.ReportBLL.newReportLoadComplete(vh.Real_ID, cmd.CARRIER_ID, vh.Real_ID, null);
                    replyTranEventReport(bcfApp, eventType, vh, seqNum);
                }

                scApp.PortBLL.OperateCatch.updatePortStationCSTExistStatus(cmd.SOURCE_PORT, string.Empty);
                scApp.VehicleBLL.doLoadComplete(vh.VEHICLE_ID);
                //Task.Run(() => checkHasOrtherCommandExcuteAndIsNeedToPreOpenCover(vh, cmdID));
            }

            //private void checkHasOrtherCommandExcuteAndIsNeedToPreOpenCover(AVEHICLE vh, string currentExcuteCmd)
            //{
            //    try
            //    {
            //        if (vh.IsCloseToAGVStation)
            //        {
            //            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
            //               Data: $"Close to agv station is on",
            //               VehicleID: vh.VEHICLE_ID,
            //               CST_ID_L: vh.CST_ID_L,
            //               CST_ID_R: vh.CST_ID_R);

            //            return;
            //        }
            //        var has_orther_cmd = scApp.VehicleBLL.cache.hasOrtherCmd(vh.VEHICLE_ID, currentExcuteCmd);
            //        if (has_orther_cmd.has)
            //        {
            //            string next_excute_cmd_id = SCUtility.Trim(has_orther_cmd.cmdID, true);
            //            if (SCUtility.isEmpty(next_excute_cmd_id)) return;
            //            //先確認目前執行的命令，是否是要去AGV Station 進行Load/Unload
            //            //是的話則判斷是否已經進入到N公尺m內
            //            //如果是 則將通知OHBC將此AGV ST進行開蓋
            //            bool has_excute_cmd = !SCUtility.isEmpty(vh.CurrentExcuteCmdID);
            //            if (!has_excute_cmd)
            //                return;
            //            ACMD current_excute_cmd = scApp.CMDBLL.cache.getExcuteCmd(next_excute_cmd_id);
            //            if (current_excute_cmd == null)
            //                return;
            //            if (current_excute_cmd.CMD_TYPE == E_CMD_TYPE.LoadUnload || current_excute_cmd.CMD_TYPE == E_CMD_TYPE.Unload)
            //            {
            //                //not thing...
            //            }
            //            else
            //            {
            //                return;
            //            }
            //            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
            //               Data: $"Start check pre open cover scenario,vh id:{vh.VEHICLE_ID} current excute cmd:{next_excute_cmd_id} " +
            //                     $"source port:{SCUtility.Trim(current_excute_cmd.SOURCE_PORT, true)} target port:{SCUtility.Trim(current_excute_cmd.DESTINATION_PORT, true)} ...,",
            //               VehicleID: vh.VEHICLE_ID,
            //               CST_ID_L: vh.CST_ID_L,
            //               CST_ID_R: vh.CST_ID_R);

            //            scApp.VehicleService.checkWillGoToPortIsAGVStationAndIsNeedPreOpenCover(vh, current_excute_cmd);

            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.Error(ex, "Exception:");
            //    }
            //}

            protected void updateCarrierInVehicleLocation(AVEHICLE vh, ACMD cmd, string readCarrierID)
            {
                scApp.CarrierBLL.db.updateLocationAndState
                (cmd.CARRIER_ID, vh.Real_ID, E_CARRIER_STATE.Installed);
                //var carrier_location = tryFindCarrierLocationOnVehicle(vh.VEHICLE_ID, cmd.CARRIER_ID, readCarrierID);
                //if (carrier_location.isExist)
                //{
                //    scApp.CarrierBLL.db.updateLocationAndState
                //        (cmd.CARRIER_ID, carrier_location.Location.ID, E_CARRIER_STATE.Installed);
                //}
                //else
                //{
                //    string location_id_r = vh.LocationRealID_R;
                //    string location_id_l = vh.LocationRealID_L;
                //    //在找不到在哪個CST時，要找自己的Table是否有該Vh carrier如果有就上報另一個沒carrier的
                //    var check_has_carrier_on_location_result = scApp.CarrierBLL.db.hasCarrierOnVhLocation(location_id_l);
                //    if (check_has_carrier_on_location_result.has)
                //    {
                //        scApp.CarrierBLL.db.updateLocationAndState
                //            (cmd.CARRIER_ID, location_id_r, E_CARRIER_STATE.Installed);
                //    }
                //    else
                //    {
                //        scApp.CarrierBLL.db.updateLocationAndState
                //            (cmd.CARRIER_ID, location_id_l, E_CARRIER_STATE.Installed);
                //    }

                //    //scApp.CarrierBLL.db.updateLocationAndState
                //    //    (cmd.CARRIER_ID, "", E_CARRIER_STATE.Installed);
                //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                //       Data: $"vh:{vh.VEHICLE_ID} report load complete cst id:{SCUtility.Trim(cmd.CARRIER_ID, true)}, " +
                //             $"but no find carrier in vh. location r cst id:{vh.CST_ID_R},location l cst id:{vh.CST_ID_L}",
                //       VehicleID: vh.VEHICLE_ID,
                //       CST_ID_L: vh.CST_ID_L,
                //       CST_ID_R: vh.CST_ID_R);
                //}
            }

            //private (bool isExist, AVEHICLE.Location Location) tryFindCarrierLocationOnVehicle(string vhID, string commandCarrierID, string readCarrierID)
            //{
            //    (bool isExist, AVEHICLE.Location Location) location = (false, null);
            //    var vh = vehicleBLL.cache.getVehicle(vhID);
            //    bool is_exist = SpinWait.SpinUntil(() => vh.IsCarreirExist(commandCarrierID), 1000);
            //    if (is_exist)
            //    {
            //        location = vh.getCarreirLocation(commandCarrierID);
            //    }
            //    else
            //    {
            //        if (!SCUtility.isEmpty(readCarrierID))
            //        {
            //            is_exist = SpinWait.SpinUntil(() => vh.IsCarreirExist(readCarrierID), 1000);
            //            if (is_exist)
            //            {
            //                location = vh.getCarreirLocation(readCarrierID);
            //            }
            //        }
            //    }
            //    return location;
            //}

            protected virtual void TranEventReport_UnloadArrive(BCFApplication bcfApp, AVEHICLE vh, int seqNum
                                                    , EventType eventType, string cmdID)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                           Data: $"Process report {eventType}",
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                ACMD cmd = scApp.CMDBLL.GetCMD_OHTCByID(cmdID);
                bool isTranCmd = !SCUtility.isEmpty(cmd.TRANSFER_ID);
                if (isTranCmd)
                {
                    scApp.TransferBLL.db.transfer.updateTranStatus2UnloadArrive(cmd.TRANSFER_ID);
                    List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
                    using (TransactionScope tx = SCUtility.getTransactionScope())
                    {
                        using (DBConnection_EF con = DBConnection_EF.GetUContext())
                        {

                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"do report {eventType} to mcs.",
                               VehicleID: vh.VEHICLE_ID,
                               CarrierID: vh.CST_ID);
                            bool isCreatReportInfoSuccess = scApp.ReportBLL.newReportUnloadArrivals(cmd.TRANSFER_ID, reportqueues);
                            if (!isCreatReportInfoSuccess)
                            {
                                return;
                            }
                            scApp.ReportBLL.insertMCSReport(reportqueues);
                        }
                        Boolean resp_cmp = replyTranEventReport(bcfApp, eventType, vh, seqNum);

                        if (resp_cmp)
                        {
                            tx.Complete();
                        }
                        else
                        {
                            return;
                        }
                    }
                    scApp.ReportBLL.newSendMCSMessage(reportqueues);
                    scApp.SysExcuteQualityBLL.updateSysExecQity_ArrivalDestnPort(cmd.TRANSFER_ID);

                }
                else
                {
                    replyTranEventReport(bcfApp, eventType, vh, seqNum);
                }

                scApp.VehicleBLL.doUnloadArrivals(vh.VEHICLE_ID, cmdID);
                scApp.ReserveBLL.RemoveAllReservedSectionsByVehicleID(vh.VEHICLE_ID);
                //APORTSTATION port = scApp.getEQObjCacheManager().getPortStation(cmd.SOURCE_PORT);
                //if (port != null)
                //{
                //PortValueDefMapAction mapAction = port.getMapActionByIdentityKey(typeof(PortValueDefMapAction).Name) as PortValueDefMapAction;
                //if (mapAction != null)
                //{
                scApp.TransferService.assignCSTIDtoPort(cmd.DESTINATION_PORT, cmd.CARRIER_ID);
                //}
                //}
                //checkIsAGVStationToCloseReservedFlag(vh, currentPortID);
            }

            //protected void checkIsAGVStationToCloseReservedFlag(AVEHICLE vh, string currentPortID)
            //{
            //    try
            //    {
            //        bool is_agv_station = scApp.EqptBLL.OperateCatch.IsAGVStation(currentPortID);
            //        if (is_agv_station)
            //        {
            //            var agv_station = scApp.EqptBLL.OperateCatch.getAGVStation(currentPortID);
            //            agv_station.IsReservation = false;
            //            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
            //               Data: $"Closed agv station:{currentPortID} reserved flag by agv unload arrive,flag:{agv_station.IsReservation}.",
            //               VehicleID: vh.VEHICLE_ID,
            //               CST_ID_L: vh.CST_ID_L,
            //               CST_ID_R: vh.CST_ID_R);
            //            return;
            //        }
            //        bool is_agv_station_port = scApp.PortStationBLL.OperateCatch.IsAGVStationPort(scApp.EqptBLL, currentPortID);
            //        if (is_agv_station_port)
            //        {
            //            var agv_station_port = scApp.PortStationBLL.OperateCatch.getPortStation(currentPortID);
            //            var agv_station = agv_station_port.GetEqpt(scApp.EqptBLL) as AGVStation;
            //            if (agv_station == null)
            //            {
            //                return;
            //            }
            //            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
            //               Data: $"Closed agv station:{agv_station.EQPT_ID} reserved flag by agv unload arrive,flag:{agv_station.IsReservation}.",
            //               VehicleID: vh.VEHICLE_ID,
            //               CST_ID_L: vh.CST_ID_L,
            //               CST_ID_R: vh.CST_ID_R);
            //            agv_station.IsReservation = false;
            //            return;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.Error(ex, "Exception:");
            //    }
            //}

            protected virtual void TranEventReport_UnloadComplete(BCFApplication bcfApp, AVEHICLE vh, int seqNum
                                                , EventType eventType, string cmdID)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"do report {eventType} to mcs.",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                //vh.IsCloseToAGVStation = false;
                ACMD cmd = scApp.CMDBLL.GetCMD_OHTCByID(cmdID);
                bool isTranCmd = !SCUtility.isEmpty(cmd.TRANSFER_ID);
                if (isTranCmd)
                {
                    scApp.TransferBLL.db.transfer.updateTranStatus2UnloadComplete(cmd.TRANSFER_ID);
                    List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
                    using (TransactionScope tx = SCUtility.getTransactionScope())
                    {
                        using (DBConnection_EF con = DBConnection_EF.GetUContext())
                        {
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"do report {eventType} to mcs.",
                               VehicleID: vh.VEHICLE_ID,
                               CarrierID: vh.CST_ID);
                            bool isCreatReportInfoSuccess = true;
                            //if (!scApp.PortStationBLL.OperateCatch.IsEqPort(scApp.EqptBLL, cmd.DESTINATION_PORT))
                            isCreatReportInfoSuccess = scApp.ReportBLL.newReportUnloadComplete(cmd.TRANSFER_ID, reportqueues);

                            if (!isCreatReportInfoSuccess)
                            {
                                return;
                            }
                            scApp.ReportBLL.insertMCSReport(reportqueues);
                            scApp.ReportBLL.newSendMCSMessage(reportqueues);
                        }
                        scApp.CarrierBLL.db.updateLocationAndState(cmd.CARRIER_ID, cmd.DESTINATION_PORT, E_CARRIER_STATE.Complete);
                        scApp.CarrierBLL.db.updateStoredTime(cmd.CARRIER_ID);

                        Boolean resp_cmp = replyTranEventReport(bcfApp, eventType, vh, seqNum);

                        if (resp_cmp)
                        {
                            tx.Complete();
                        }
                        else
                        {
                            return;
                        }
                    }
                    //scApp.ReportBLL.newSendMCSMessage(reportqueues);
                }
                else
                {
                    //如果是swap的vh 他在放置貨物位置將會是虛擬port，需要看最後車子上報的位置來決定更新到哪邊

                    scApp.CarrierBLL.db.updateLocationAndState(cmd.CARRIER_ID, cmd.DESTINATION_PORT, E_CARRIER_STATE.Complete);
                    scApp.CarrierBLL.db.updateStoredTime(cmd.CARRIER_ID);

                    //if (!SCUtility.isEmpty(cmd.CARRIER_ID))
                    //{
                    //    scApp.CarrierBLL.db.updateLocationAndState(cmd.CARRIER_ID, cmd.DESTINATION_PORT, E_CARRIER_STATE.Complete);
                    //    scApp.ReportBLL.newReportUnloadComplete(vh.Real_ID, cmd.CARRIER_ID, cmd.DESTINATION_PORT, null);
                    //}
                    replyTranEventReport(bcfApp, eventType, vh, seqNum);
                }
                scApp.VehicleBLL.doUnloadComplete(vh.VEHICLE_ID);
                //Task.Run(() => checkHasOrtherCommandExcuteAndIsNeedToPreOpenCover(vh, cmdID));
            }


            protected virtual void TranEventReport_BCRRead(BCFApplication bcfApp, AVEHICLE vh, int seqNum,
                                               EventType eventType, string readCarrierID, BCRReadResult bCRReadResult,
                                               string cmdID)
            {
                ACMD cmd = scApp.CMDBLL.GetCMD_OHTCByID(cmdID);
                string rename_carrier_id = string.Empty;
                CMDCancelType replyActionType = CMDCancelType.CmdNone;
                if (cmd == null)
                {
                    //if (!SCUtility.isEmpty(vh.LastLoadCompleteCommandID))
                    //{
                    //    cmd = scApp.CMDBLL.GetCMD_OHTCByID(vh.LastLoadCompleteCommandID);
                    //}
                    //if (cmd == null)
                    //{

                    switch (bCRReadResult)
                    {
                        case BCRReadResult.BcrNormal:
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"Try install carrier in vehicle by bcr read event(no cmd id)...",
                               VehicleID: vh.VEHICLE_ID,
                               CarrierID: vh.CST_ID);
                            //string action_lcation_real_id = vh.getLoctionRealID(location);
                            string action_lcation_real_id = vh.Real_ID;
                            var try_install_result = scApp.TransferService.tryInstallCarrierInVehicle(vh.VEHICLE_ID, action_lcation_real_id, readCarrierID);
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"Try install carrier in vehicle by bcr read event(no cmd id),result:[{try_install_result.isSuccess}] " +
                                     $"raeson:{try_install_result.result}",
                               VehicleID: vh.VEHICLE_ID,
                               CarrierID: vh.CST_ID);
                            break;
                        case BCRReadResult.BcrMisMatch:
                            if (DebugParameter.isContinueByIDReadFail)
                            {
                                replyActionType = CMDCancelType.CmdNone;
                            }
                            else
                            {
                                replyActionType = CMDCancelType.CmdCancelIdMismatch;
                            }
                            rename_carrier_id = readCarrierID;
                            break;
                        case BCRReadResult.BcrReadFail:
                            if (DebugParameter.isContinueByIDReadFail)
                            {
                                replyActionType = CMDCancelType.CmdNone;
                            }
                            else
                            {
                                replyActionType = CMDCancelType.CmdCancelIdReadFailed;
                            }
                            string new_carrier_id =
                                $"UNKF{vh.Real_ID.Trim()}{DateTime.Now.ToString(SCAppConstants.TimestampFormat_12)}";
                            rename_carrier_id = new_carrier_id;
                            break;
                        default:
                            replyActionType = CMDCancelType.CmdNone;
                            break;
                    }
                    replyTranEventReport(bcfApp, eventType, vh, seqNum,
                        renameCarrierID: rename_carrier_id,
                        cancelType: replyActionType);
                    return;
                    //}
                }
                //updateCarrierInVehicleLocation(vh, cmd, readCarrierID);
                bool is_tran_cmd = !SCUtility.isEmpty(cmd.TRANSFER_ID);
                if (is_tran_cmd)
                {
                    scApp.TransferBLL.db.transfer.updateTranStatus2Transferring(cmd.TRANSFER_ID);//20210517 Abb by Mark Chou 將變更命令狀態為Transfer改為在BCRRead時進行
                }
                switch (bCRReadResult)
                {
                    case BCRReadResult.BcrMisMatch:
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                           Data: $"BCR mismatch happend,start abort command id:{cmd.ID.Trim()}",
                               VehicleID: vh.VEHICLE_ID,
                               CarrierID: vh.CST_ID);
                        if (DebugParameter.isContinueByIDReadFail)
                        {
                            rename_carrier_id = SCUtility.Trim(cmd.CARRIER_ID, true);
                            replyActionType = CMDCancelType.CmdNone;
                        }
                        else
                        {
                            rename_carrier_id = readCarrierID;
                            replyActionType = CMDCancelType.CmdCancelIdMismatch;
                        }

                        ACARRIER cst = new ACARRIER();
                        cst.ID = readCarrierID;
                        cst.LOCATION = vh.Real_ID;
                        Task.Run(() => scApp.ReportBLL.ReportCarrierIDRead(cst, "3", null));

                        scApp.CarrierBLL.db.updateRenameID(cmd.CARRIER_ID, rename_carrier_id);

                        //todo kevin 要重新Review mismatch fail時候的流程
                        //todo kevin 要加入duplicate 的流程
                        replyTranEventReport(bcfApp, eventType, vh, seqNum,
                            renameCarrierID: rename_carrier_id,
                            cancelType: replyActionType);
                        //scApp.CarrierBLL.db.updateRenameID(cmd.CARRIER_ID, readCarrierID);
                        break;
                    case BCRReadResult.BcrReadFail:
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                           Data: $"BCR read fail happend,start abort command id:{cmd.ID.Trim()}",
                               VehicleID: vh.VEHICLE_ID,
                               CarrierID: vh.CST_ID);
                        string new_carrier_id = string.Empty;
                        if (DebugParameter.isContinueByIDReadFail)
                        //if (true)
                        {
                            rename_carrier_id = SCUtility.Trim(cmd.CARRIER_ID, true);
                            replyActionType = CMDCancelType.CmdNone;
                        }
                        else
                        {
                            new_carrier_id =
                                $"UNKF{vh.Real_ID.Trim()}{DateTime.Now.ToString(SCAppConstants.TimestampFormat_12)}";
                            rename_carrier_id = new_carrier_id;
                            replyActionType = CMDCancelType.CmdCancelIdReadFailed;
                        }

                        //string cst_id = "UNKF";
                        string cst_id = new_carrier_id;
                        ACARRIER _cst = new ACARRIER();
                        _cst.ID = cst_id;
                        _cst.LOCATION = vh.Real_ID;
                        Task.Run(() => scApp.ReportBLL.ReportCarrierIDRead(_cst, "1", null));

                        scApp.CarrierBLL.db.updateRenameID(cmd.CARRIER_ID, rename_carrier_id);

                        replyTranEventReport(bcfApp, eventType, vh, seqNum,
                            renameCarrierID: rename_carrier_id,
                            cancelType: replyActionType);
                        //scApp.CarrierBLL.db.updateRenameID(cmd.CARRIER_ID, new_carrier_id);
                        break;
                    case BCRReadResult.BcrNormal:
                        replyTranEventReport(bcfApp, eventType, vh, seqNum);
                        break;
                }
                //if (is_tran_cmd)
                //{
                //    List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
                //    scApp.ReportBLL.newReportCarrierIDReadReport(cmd.TRANSFER_ID, reportqueues);
                //    scApp.ReportBLL.insertMCSReport(reportqueues);
                //    scApp.ReportBLL.newSendMCSMessage(reportqueues);
                //}
            }
            protected void TranEventReport_Loading(BCFApplication bcfApp, AVEHICLE vh, int seqNum, EventType eventType, string cmdID)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"Process report {eventType}",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                ACMD cmd = scApp.CMDBLL.GetCMD_OHTCByID(cmdID);
                if (cmd == null)
                {
                    replyTranEventReport(bcfApp, eventType, vh, seqNum);
                    return;
                }
                bool isTranCmd = !SCUtility.isEmpty(cmd.TRANSFER_ID);

                if (isTranCmd)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"do report {eventType} to mcs.",
                       VehicleID: vh.VEHICLE_ID,
                       CarrierID: vh.CST_ID);
                    scApp.TransferBLL.db.transfer.updateTranStatus2Loading(cmd.TRANSFER_ID);
                    List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
                    using (TransactionScope tx = SCUtility.getTransactionScope())
                    {
                        using (DBConnection_EF con = DBConnection_EF.GetUContext())
                        {
                            bool isSuccess = true;
                            scApp.ReportBLL.newReportLoading(cmd.TRANSFER_ID, reportqueues);
                            scApp.ReportBLL.insertMCSReport(reportqueues);

                            if (isSuccess)
                            {
                                if (replyTranEventReport(bcfApp, eventType, vh, seqNum))
                                {
                                    tx.Complete();
                                    scApp.ReportBLL.newSendMCSMessage(reportqueues);
                                }
                            }
                        }
                    }
                }
                else
                {
                    replyTranEventReport(bcfApp, eventType, vh, seqNum);
                }
                scApp.VehicleBLL.doLoading(vh.VEHICLE_ID);
            }
            protected void TranEventReport_Unloading(BCFApplication bcfApp, AVEHICLE vh, int seqNum, EventType eventType, string cmdID)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"Process report {eventType}",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                ACMD cmd = scApp.CMDBLL.GetCMD_OHTCByID(cmdID);
                if (cmd == null)
                {
                    replyTranEventReport(bcfApp, eventType, vh, seqNum);
                    return;
                }

                bool isTranCmd = !SCUtility.isEmpty(cmd.TRANSFER_ID);
                if (isTranCmd)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"do report {eventType} to mcs.",
                       VehicleID: vh.VEHICLE_ID,
                       CarrierID: vh.CST_ID);
                    scApp.TransferBLL.db.transfer.updateTranStatus2Unloading(cmd.TRANSFER_ID);
                    List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
                    using (TransactionScope tx = SCUtility.getTransactionScope())
                    {
                        using (DBConnection_EF con = DBConnection_EF.GetUContext())
                        {
                            bool isSuccess = true;
                            scApp.ReportBLL.newReportUnloading(cmd.TRANSFER_ID, reportqueues);
                            scApp.ReportBLL.insertMCSReport(reportqueues);
                            if (isSuccess)
                            {
                                if (replyTranEventReport(bcfApp, eventType, vh, seqNum))
                                {
                                    tx.Complete();
                                    scApp.ReportBLL.newSendMCSMessage(reportqueues);
                                }
                            }
                        }
                    }
                }
                else
                {
                    replyTranEventReport(bcfApp, eventType, vh, seqNum);
                }
                scApp.VehicleBLL.doUnloading(vh.VEHICLE_ID);

                //scApp.MapBLL.getPortID(vh.CUR_ADR_ID, out string port_id);
                scApp.PortBLL.OperateCatch.updatePortStationCSTExistStatus(cmd.DESTINATION_PORT, cmd.CARRIER_ID);
            }

            private bool replyTranEventReport(BCFApplication bcfApp, EventType eventType, AVEHICLE eqpt, int seq_num,
                                              bool canBlockPass = false, bool canHIDPass = false, bool canReservePass = false,
                                              string renameCarrierID = "", CMDCancelType cancelType = CMDCancelType.CmdNone,
                                              RepeatedField<ReserveInfo> reserveInfos = null)
            {
                ID_36_TRANS_EVENT_RESPONSE send_str = new ID_36_TRANS_EVENT_RESPONSE
                {
                    IsBlockPass = canBlockPass ? PassType.Pass : PassType.Block,
                    IsHIDPass = canHIDPass ? PassType.Pass : PassType.Block,
                    IsReserveSuccess = canReservePass ? ReserveResult.Success : ReserveResult.Unsuccess,
                    ReplyCode = 0,
                    //RenameBOXID = renameCarrierID,
                    RenameCSTID = renameCarrierID,
                    ReplyActiveType = cancelType,
                };
                if (reserveInfos != null)
                {
                    send_str.ReserveInfos.AddRange(reserveInfos);
                }

                WrapperMessage wrapper = new WrapperMessage
                {
                    SeqNum = seq_num,
                    ImpTransEventResp = send_str
                };
                Boolean resp_cmp = eqpt.sendMessage(wrapper, true);
                SCUtility.RecodeReportInfo(eqpt.VEHICLE_ID, seq_num, send_str, resp_cmp.ToString());
                return resp_cmp;
            }
            #endregion ID_136 TransferEventReport
            #region ID_138 GuideInfoRequest
            [ClassAOPAspect]
            public void GuideInfoRequest(BCFApplication bcfApp, AVEHICLE vh, ID_138_GUIDE_INFO_REQUEST recive_str, int seq_num)
            {
                if (scApp.getEQObjCacheManager().getLine().ServerPreStop)
                    return;

                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   seq_num: seq_num,
                   Data: recive_str,
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                SCUtility.RecodeReportInfo(vh.VEHICLE_ID, seq_num, recive_str);
                var request_from_to_list = recive_str.FromToAdrList;

                List<GuideInfo> guide_infos = new List<GuideInfo>();
                foreach (FromToAdr from_to_adr in request_from_to_list)
                {
                    //var guide_info = scApp.GuideBLL.getGuideInfo(from_to_adr.From, from_to_adr.To);
                    var guide_info = CalculationPath(vh, from_to_adr.From, from_to_adr.To);

                    GuideInfo guide = new GuideInfo();
                    guide.FromTo = from_to_adr;
                    if (guide_info.isSuccess)
                    {
                        guide.GuideAddresses.AddRange(guide_info.guideAddressIds);
                        guide.GuideSections.AddRange(guide_info.guideSectionIds);
                        guide.Distance = (uint)guide_info.totalCost;
                    }
                    guide_infos.Add(guide);
                }

                bool is_success = reply_ID_38_TRANS_COMPLETE_RESPONSE(vh, seq_num, guide_infos);
                if (is_success && guide_infos.Count > 0)
                {
                    vh.VhAvoidInfo = null;
                    var shortest_path = guide_infos.OrderBy(info => info.Distance).First();
                    scApp.VehicleBLL.cache.setWillPassSectionInfo(vh.VEHICLE_ID, shortest_path.GuideSections.ToList(), shortest_path.GuideAddresses.ToList());
                }
            }
            private (bool isSuccess, List<string> guideSegmentIds, List<string> guideSectionIds, List<string> guideAddressIds, int totalCost)
                CalculationPath(AVEHICLE vh, string fromAdr, string toAdr)
            {
                bool is_success = false;
                List<string> guide_segment_isd = null;
                List<string> guide_section_isd = null;
                List<string> guide_address_isd = null;
                int total_cost = 0;

                (is_success, guide_segment_isd, guide_section_isd, guide_address_isd, total_cost) =
                    scApp.GuideBLL.getGuideInfo(fromAdr, toAdr);
                return (is_success, guide_segment_isd, guide_section_isd, guide_address_isd, total_cost);
            }

            private (bool isSuccess, List<string> guideSegmentIds, List<string> guideSectionIds, List<string> guideAddressIds, int totalCost)
                CalculationPathAfterAvoid(AVEHICLE vh, string fromAdr, string toAdr, List<string> needByPassSecIDs = null)
            {
                int current_find_count = 0;
                int max_find_count = 10;

                bool is_success = true;
                List<string> guide_segment_isd = null;
                List<string> guide_section_isd = null;
                List<string> guide_address_isd = null;
                int total_cost = 0;
                bool is_need_check_reserve_status = true;
                List<string> need_by_pass_sec_ids = new List<string>();
                //if (needByPassSecIDs != null)
                //{
                //    need_by_pass_sec_ids.AddRange(needByPassSecIDs);
                //}
                do
                {
                    //如果有找到路徑則確認一下段是否可以預約的到
                    if (current_find_count != max_find_count) //如果是最後一次的話，就不要在確認預約狀態了。
                    {
                        (is_success, guide_segment_isd, guide_section_isd, guide_address_isd, total_cost)
                            = scApp.GuideBLL.getGuideInfo(fromAdr, toAdr, need_by_pass_sec_ids);
                        if (is_success)
                        {
                            //確認下一段Section，是否可以預約成功
                            string next_walk_section = "";
                            string next_walk_address = "";
                            if (guide_section_isd != null && guide_section_isd.Count > 0)
                            {
                                next_walk_section = guide_section_isd[0];
                                next_walk_address = guide_address_isd[0];
                            }

                            if (!SCUtility.isEmpty(next_walk_section)) //由於有可能找出來後，是剛好在原地
                            {
                                if (is_success)
                                {
                                    var reserve_result = scApp.ReserveBLL.askReserveSuccess
                                        (scApp.SectionBLL, vh.VEHICLE_ID, next_walk_section, next_walk_address);
                                    if (reserve_result.isSuccess)
                                    {
                                        is_success = true;
                                    }
                                    else
                                    {
                                        is_success = false;
                                        need_by_pass_sec_ids.Add(next_walk_section);
                                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                           Data: $"find the override path ,but section:{next_walk_section} is reserved for vh:{reserve_result.reservedVhID}" +
                                                 $"add to need by pass sec ids",
                                           VehicleID: vh.VEHICLE_ID);
                                    }

                                    //4.在準備送出前，如果是因Avoid完成所下的Over ride，要判斷原本block section是否已經可以預約到了，是才可以下給車子
                                    if (is_success && vh.VhAvoidInfo != null && is_need_check_reserve_status)
                                    {
                                        bool is_pass_before_blocked_section = true;
                                        if (guide_section_isd != null)
                                        {
                                            is_pass_before_blocked_section &= guide_section_isd.Contains(vh.VhAvoidInfo.BlockedSectionID);
                                        }
                                        if (is_pass_before_blocked_section)
                                        {
                                            //is_success = false;
                                            //string before_block_section_id = vh.VhAvoidInfo.BlockedSectionID;
                                            //need_by_pass_sec_ids.Add(before_block_section_id);

                                            //如果有則要嘗試去預約，如果等了20秒還是沒有釋放出來則嘗試別條路徑
                                            string before_block_section_id = vh.VhAvoidInfo.BlockedSectionID;
                                            if (!SpinWait.SpinUntil(() => scApp.ReserveBLL.TryAddReservedSection
                                            (vh.VEHICLE_ID, before_block_section_id, isAsk: true).OK, 15000))
                                            {
                                                is_success = false;
                                                need_by_pass_sec_ids.Add(before_block_section_id);
                                                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                                   Data: $"wait more than 5 seconds,before block section id:{before_block_section_id} not release, by pass section:{before_block_section_id} find next path.current by pass section:{string.Join(",", need_by_pass_sec_ids)}",
                                                   VehicleID: vh.VEHICLE_ID);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            ////如果在找不到路的時候，就把原本By pass的路徑給打開，然後再找一次
                            ////該次就不檢查原本預約不到的路是否已經可以過了，即使不能過也再下一次走看看
                            if (need_by_pass_sec_ids != null && need_by_pass_sec_ids.Count > 0)
                            {
                                is_success = false;
                                need_by_pass_sec_ids.Clear();
                                is_need_check_reserve_status = false;
                                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                   Data: $"find path fail vh:{vh.VEHICLE_ID}, current address:{vh.CUR_ADR_ID} ," +
                                   $" by pass section:{string.Join(",", need_by_pass_sec_ids)},clear all by pass section and then continue find override path.",
                                   VehicleID: vh.VEHICLE_ID);

                            }
                            else
                            {
                                //如果找不到路徑，則就直接跳出搜尋的Loop
                                is_success = false;
                                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                   Data: $"find path fail vh:{vh.VEHICLE_ID}, current address:{vh.CUR_ADR_ID} ," +
                                   $" by pass section{string.Join(",", need_by_pass_sec_ids)}",
                                   VehicleID: vh.VEHICLE_ID);
                                break;
                            }
                        }
                    }
                    else
                    {
                        (is_success, guide_segment_isd, guide_section_isd, guide_address_isd, total_cost)
                            = scApp.GuideBLL.getGuideInfo(fromAdr, toAdr);
                    }
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"find the override path result:{is_success} vh:{vh.VEHICLE_ID} vh current address:{vh.CUR_ADR_ID} ," +
                       $". by pass section:{string.Join(",", need_by_pass_sec_ids)}",
                       VehicleID: vh.VEHICLE_ID);

                }
                while (!is_success && current_find_count++ <= max_find_count);
                return (is_success, guide_segment_isd, guide_section_isd, guide_address_isd, total_cost);
            }

            private bool reply_ID_38_TRANS_COMPLETE_RESPONSE(AVEHICLE vh, int seq_num, List<GuideInfo> guideInfos)
            {
                ID_38_GUIDE_INFO_RESPONSE send_str = new ID_38_GUIDE_INFO_RESPONSE();
                send_str.GuideInfoList.Add(guideInfos);
                WrapperMessage wrapper = new WrapperMessage
                {
                    SeqNum = seq_num,
                    GuideInfoResp = send_str
                };
                Boolean resp_cmp = vh.sendMessage(wrapper, true);
                SCUtility.RecodeReportInfo(vh.VEHICLE_ID, seq_num, send_str, resp_cmp.ToString());
                return resp_cmp;
            }

            #endregion ID_138 GuideInfoRequest
            #region ID_144 StatusReport
            public void ReserveStopTest(string vhID, bool is_reserve_stop)
            {
                AVEHICLE vh = scApp.VehicleBLL.cache.getVehicle(vhID);
                scApp.VehicleBLL.cache.SetReservePause(vhID, is_reserve_stop ? VhStopSingle.StopSingleOn : VhStopSingle.StopSingleOff);
            }
            public void CST_R_DisaplyTest(string vhID, bool hasCst)
            {
                AVEHICLE vh = scApp.VehicleBLL.cache.getVehicle(vhID);
                scApp.VehicleBLL.cache.SetCSTR(vhID, hasCst);
            }
            public void CST_L_DisaplyTest(string vhID, bool hasCst)
            {
                AVEHICLE vh = scApp.VehicleBLL.cache.getVehicle(vhID);
                scApp.VehicleBLL.cache.SetCSTL(vhID, hasCst);
            }
            //[ClassAOPAspect]
            //public virtual void StatusReport(BCFApplication bcfApp, AVEHICLE vh, ID_144_STATUS_CHANGE_REP recive_str, int seq_num)
            //{
            //    if (scApp.getEQObjCacheManager().getLine().ServerPreStop)
            //        return;
            //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
            //       seq_num: seq_num,
            //       Data: recive_str,
            //       VehicleID: vh.VEHICLE_ID,
            //       CST_ID_L: vh.CST_ID_L,
            //       CST_ID_R: vh.CST_ID_R);
            //    SCUtility.RecordReportInfo(vh.VEHICLE_ID, seq_num, recive_str);

            //    VHModeStatus modeStat = scApp.VehicleBLL.DecideVhModeStatus(vh.VEHICLE_ID, recive_str.ModeStatus);
            //    VHActionStatus actionStat = recive_str.ActionStatus;
            //    VhPowerStatus powerStat = recive_str.PowerStatus;
            //    string cmd_id_1 = recive_str.CmdId1;
            //    string cmd_id_2 = recive_str.CmdId2;
            //    string cmd_id_3 = recive_str.CmdId3;
            //    string cmd_id_4 = recive_str.CmdId4;

            //    string current_excute_cmd_id = recive_str.CurrentExcuteCmdId;
            //    string cst_id_l = recive_str.CstIdL;
            //    string cst_id_r = recive_str.CstIdR;
            //    VhChargeStatus chargeStatus = recive_str.ChargeStatus;
            //    VhStopSingle reserveStatus = recive_str.ReserveStatus;
            //    VhStopSingle obstacleStat = recive_str.ObstacleStatus;
            //    VhStopSingle blockingStat = recive_str.BlockingStatus;
            //    VhStopSingle pauseStat = recive_str.PauseStatus;
            //    VhStopSingle errorStat = recive_str.ErrorStatus;
            //    VhLoadCSTStatus load_cst_status_l = recive_str.HasCstL;
            //    VhLoadCSTStatus load_cst_status_r = recive_str.HasCstR;
            //    bool has_cst_l = load_cst_status_l == VhLoadCSTStatus.Exist;
            //    bool has_cst_r = load_cst_status_r == VhLoadCSTStatus.Exist;
            //    string[] will_pass_section_id = recive_str.WillPassGuideSection.ToArray();

            //    int obstacleDIST = recive_str.ObstDistance;
            //    string obstacleVhID = recive_str.ObstVehicleID;
            //    int steeringWheel = recive_str.SteeringWheel;

            //    ShelfStatus shelf_status_l = recive_str.ShelfStatusL;
            //    ShelfStatus shelf_status_r = recive_str.ShelfStatusR;


            //    if (!SCUtility.isMatche(current_excute_cmd_id, vh.CurrentExcuteCmdID))
            //    {
            //        vh.onCurrentExcuteCmdChange(current_excute_cmd_id);
            //    }
            //    if (errorStat != vh.ERROR)
            //    {
            //        vh.onErrorStatusChange(errorStat);
            //    }
            //    if (modeStat != vh.MODE_STATUS)
            //    {
            //        vh.onModeStatusChange(modeStat);
            //    }
            //    VhStopSingle op_pause = recive_str.OpPauseStatus;




            //    bool hasdifferent = vh.BATTERYCAPACITY != batteryCapacity ||
            //                        vh.MODE_STATUS != modeStat ||
            //                        vh.ACT_STATUS != actionStat ||
            //                        !SCUtility.isMatche(vh.CMD_ID_1, cmd_id_1) ||
            //                        !SCUtility.isMatche(vh.CMD_ID_2, cmd_id_2) ||
            //                        !SCUtility.isMatche(vh.CMD_ID_3, cmd_id_3) ||
            //                        !SCUtility.isMatche(vh.CMD_ID_4, cmd_id_4) ||
            //                        !SCUtility.isMatche(vh.CurrentExcuteCmdID, current_excute_cmd_id) ||
            //                        !SCUtility.isMatche(vh.CST_ID_L, cst_id_l) ||
            //                        !SCUtility.isMatche(vh.CST_ID_R, cst_id_r) ||
            //                        vh.ChargeStatus != chargeStatus ||
            //                        vh.RESERVE_PAUSE != reserveStatus ||
            //                        vh.OBS_PAUSE != obstacleStat ||
            //                        vh.BLOCK_PAUSE != blockingStat ||
            //                        vh.CMD_PAUSE != pauseStat ||
            //                        vh.ERROR != errorStat ||
            //                        vh.HAS_CST_L != has_cst_l ||
            //                        vh.HAS_CST_R != has_cst_r ||
            //                        vh.ShelfStatus_L != shelf_status_l ||
            //                        vh.ShelfStatus_R != shelf_status_r ||
            //                        !SCUtility.isMatche(vh.PredictSections, will_pass_section_id) ||
            //                        vh.OP_PAUSE != op_pause;
            //    if (hasdifferent)
            //    {
            //        scApp.VehicleBLL.cache.updateVehicleStatus(scApp.CMDBLL, vh.VEHICLE_ID,
            //                                             cst_id_l, cst_id_r, modeStat, actionStat, chargeStatus,
            //                                             blockingStat, pauseStat, obstacleStat, VhStopSingle.Off, errorStat, reserveStatus, op_pause,
            //                                             shelf_status_l, shelf_status_r,
            //                                             has_cst_l, has_cst_r,
            //                                             cmd_id_1, cmd_id_2, cmd_id_3, cmd_id_4, current_excute_cmd_id,
            //                                             batteryCapacity, will_pass_section_id);
            //    }

            //    if (modeStat != vh.MODE_STATUS)
            //    {
            //        //vh.onModeStatusChange(modeStat);
            //    }
            //    //cmdBLL.setCurrentCanAssignCmdCount(shelf_status_l, shelf_status_r);
            //    vh.setCurrentCanAssignCmdCount(shelf_status_l, shelf_status_r);
            //    //  reply_status_event_report(bcfApp, eqpt, seq_num);
            //}

            const string VEHICLE_ERROR_REPORT_DESCRIPTION = "Vehicle:{0} ,error happend.";
            [ClassAOPAspect]
            public virtual void StatusReport(BCFApplication bcfApp, AVEHICLE eqpt, ID_144_STATUS_CHANGE_REP recive_str, int seq_num)
            {
                if (scApp.getEQObjCacheManager().getLine().ServerPreStop)
                    return;
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   seq_num: seq_num,
                   Data: recive_str,
                   VehicleID: eqpt.VEHICLE_ID,
                   CarrierID: eqpt.CST_ID);

                SCUtility.RecordReportInfo(eqpt.VEHICLE_ID, seq_num, recive_str);

                VHModeStatus modeStat = scApp.VehicleBLL.DecideVhModeStatus(eqpt.VEHICLE_ID, recive_str.ModeStatus);
                VHActionStatus actionStat = recive_str.ActionStatus;
                VhPowerStatus powerStat = recive_str.PowerStatus;
                string cmd_id = recive_str.CmdID;
                string cst_id = recive_str.CSTID;

                VhStopSingle reserveStatus = recive_str.ReserveStatus;
                VhStopSingle obstacleStat = recive_str.ObstacleStatus;
                VhStopSingle hidStat = recive_str.HIDStatus;
                VhStopSingle blockingStat = recive_str.BlockingStatus;
                VhStopSingle pauseStat = recive_str.PauseStatus;
                VhStopSingle safetyPauseStat = recive_str.SafetyPauseStatus;
                VhStopSingle errorStat = recive_str.ErrorStatus;
                VhLoadCarrierStatus load_cst_status = recive_str.HasCst;
                bool has_cst = load_cst_status == VhLoadCarrierStatus.Exist;

                //VhGuideStatus leftGuideStat = recive_str.LeftGuideLockStatus;
                //VhGuideStatus rightGuideStat = recive_str.RightGuideLockStatus;
                // 0317 Jason 此部分之loadBOXStatus 原為loadCSTStatus ，現在之狀況為暫時解法
                //bool hasdifferent =
                //        !SCUtility.isMatche(eqpt.CST_ID, cst_id) ||
                //        eqpt.MODE_STATUS != modeStat ||
                //        eqpt.ACT_STATUS != actionStat ||
                //        eqpt.ObstacleStatus != obstacleStat ||
                //        eqpt.BlockingStatus != blockingStat ||
                //        eqpt.PauseStatus != pauseStat ||
                //        eqpt.HIDStatus != hidStat ||
                //        eqpt.ERROR != errorStat ||
                //        eqpt.HAS_CST != has_cst;

                bool hasdifferent =
                    eqpt.MODE_STATUS != modeStat ||
                    eqpt.ACT_STATUS != actionStat ||
                    !SCUtility.isMatche(eqpt.CMD_ID, cmd_id) ||
                    !SCUtility.isMatche(eqpt.CST_ID, cst_id) ||
                    eqpt.RESERVE_PAUSE != reserveStatus ||
                    eqpt.OBS_PAUSE != obstacleStat ||
                    eqpt.BLOCK_PAUSE != blockingStat ||
                    eqpt.SAFETY_PAUSE != safetyPauseStat ||
                    eqpt.CMD_PAUSE != pauseStat ||
                    eqpt.ERROR != errorStat ||
                    eqpt.HAS_CST != has_cst;

                if (!SCUtility.isMatche(cmd_id, eqpt.CMD_ID))
                {
                    eqpt.onCurrentExcuteCmdChange(cmd_id);
                }
                if (errorStat != eqpt.ERROR)
                {
                    eqpt.onErrorStatusChange(errorStat);
                }
                if (modeStat != eqpt.MODE_STATUS)
                {
                    eqpt.onModeStatusChange(modeStat);
                }

                if (hasdifferent)
                {
                    scApp.VehicleBLL.cache.updateVehicleStatus(scApp.CMDBLL, eqpt.VEHICLE_ID,
                                                         cst_id, modeStat, actionStat,
                                                         blockingStat, pauseStat, obstacleStat, safetyPauseStat, VhStopSingle.StopSingleOff, errorStat, reserveStatus,
                                                         has_cst,
                                                         cmd_id);
                }

                if (modeStat != eqpt.MODE_STATUS)
                {
                    //vh.onModeStatusChange(modeStat);
                }
                //cmdBLL.setCurrentCanAssignCmdCount(shelf_status_l, shelf_status_r);
                //vh.setCurrentCanAssignCmdCount(shelf_status_l, shelf_status_r);
                //  reply_status_event_report(bcfApp, eqpt, seq_num);




            }

            private bool reply_status_event_report(BCFApplication bcfApp, AVEHICLE vh, int seq_num)
            {
                ID_44_STATUS_CHANGE_RESPONSE send_str = new ID_44_STATUS_CHANGE_RESPONSE
                {
                    ReplyCode = 0
                };
                WrapperMessage wrapper = new WrapperMessage
                {
                    SeqNum = seq_num,
                    StatusChangeResp = send_str
                };

                //Boolean resp_cmp = ITcpIpControl.sendGoogleMsg(bcfApp, eqpt.TcpIpAgentName, wrapper, true);
                Boolean resp_cmp = vh.sendMessage(wrapper, true);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                  seq_num: seq_num, Data: send_str,
                  VehicleID: vh.VEHICLE_ID,
                  CarrierID: vh.CST_ID);
                SCUtility.RecodeReportInfo(vh.VEHICLE_ID, seq_num, send_str, resp_cmp.ToString());
                return resp_cmp;
            }
            #endregion ID_144 StatusReport
            #region ID_152 AvoidCompeteReport
            [ClassAOPAspect]
            public void AvoidCompleteReport(BCFApplication bcfApp, AVEHICLE vh, ID_152_AVOID_COMPLETE_REPORT recive_str, int seq_num)
            {
                if (scApp.getEQObjCacheManager().getLine().ServerPreStop)
                    return;
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"Process Avoid complete report.vh current address:{vh.CUR_ADR_ID}, current section:{vh.CUR_SEC_ID}",
                  VehicleID: vh.VEHICLE_ID,
                  CarrierID: vh.CST_ID);

                ID_52_AVOID_COMPLETE_RESPONSE send_str = null;
                SCUtility.RecodeReportInfo(vh.VEHICLE_ID, seq_num, recive_str);
                send_str = new ID_52_AVOID_COMPLETE_RESPONSE
                {
                    ReplyCode = 0
                };
                WrapperMessage wrapper = new WrapperMessage
                {
                    SeqNum = seq_num,
                    AvoidCompleteResp = send_str
                };

                //Boolean resp_cmp = ITcpIpControl.sendGoogleMsg(bcfApp, tcpipAgentName, wrapper, true);
                Boolean resp_cmp = vh.sendMessage(wrapper, true);

                SCUtility.RecodeReportInfo(vh.VEHICLE_ID, seq_num, send_str, resp_cmp.ToString());

                //在避車完成之後，先清除掉原本已經預約的路徑，接著再將自己當下的路徑預約回來，確保不會被預約走
                scApp.ReserveBLL.RemoveAllReservedSectionsByVehicleID(vh.VEHICLE_ID);
                //SpinWait.SpinUntil(() => false, 1000);
                var result = scApp.ReserveBLL.TryAddReservedSection(vh.VEHICLE_ID, vh.CUR_SEC_ID,
                                                                    sensorDir: Mirle.Hlts.Utils.HltDirection.None,
                                                                    forkDir: Mirle.Hlts.Utils.HltDirection.None);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(ReserveBLL), Device: "AGV",
                   Data: $"vh:{vh.VEHICLE_ID} reserve section:{vh.CUR_SEC_ID} after remove all reserved(avoid complete),result:{result.ToString()}",
                   VehicleID: vh.VEHICLE_ID);

            }
            #endregion ID_152 AvoidCompeteReport
            #region ID_172 RangeTeachingCompleteReport
            [ClassAOPAspect]
            public void RangeTeachingCompleteReport(string tcpipAgentName, BCFApplication bcfApp, AVEHICLE eqpt, ID_172_RANGE_TEACHING_COMPLETE_REPORT recive_str, int seq_num)
            {
                ID_72_RANGE_TEACHING_COMPLETE_RESPONSE response = null;
                response = new ID_72_RANGE_TEACHING_COMPLETE_RESPONSE()
                {
                    ReplyCode = 0
                };

                WrapperMessage wrapper = new WrapperMessage
                {
                    SeqNum = seq_num,
                    RangeTeachingCmpResp = response
                };
                Boolean resp_cmp = eqpt.sendMessage(wrapper, true);
                SCUtility.RecodeReportInfo(eqpt.VEHICLE_ID, seq_num, response, resp_cmp.ToString());
            }
            #endregion ID_172 RangeTeachingCompleteReport
            #region ID_194 AlarmReport
            [ClassAOPAspect]
            public void AlarmReport(BCFApplication bcfApp, AVEHICLE vh, ID_194_ALARM_REPORT recive_str, int seq_num)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                  seq_num: seq_num, Data: recive_str,
                  VehicleID: vh.VEHICLE_ID,
                  CarrierID: vh.CST_ID);
                try
                {
                    string node_id = vh.NODE_ID;
                    string eq_id = vh.VEHICLE_ID;
                    string err_code = recive_str.ErrCode;
                    string err_desc = recive_str.ErrDescription;
                    ErrorStatus status = recive_str.ErrStatus;
                    scApp.LineService.ProcessAlarmReport(vh, err_code, status, err_desc);
                    ID_94_ALARM_RESPONSE send_str = new ID_94_ALARM_RESPONSE
                    {
                        ReplyCode = 0
                    };
                    WrapperMessage wrapper = new WrapperMessage
                    {
                        SeqNum = seq_num,
                        AlarmResp = send_str
                    };
                    SCUtility.RecodeReportInfo(vh.VEHICLE_ID, seq_num, recive_str);
                    Boolean resp_cmp = vh.sendMessage(wrapper, true);
                    SCUtility.RecodeReportInfo(vh.VEHICLE_ID, seq_num, send_str, resp_cmp.ToString());
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception:");
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: ex,
                      VehicleID: vh.VEHICLE_ID,
                      CarrierID: vh.CST_ID);
                }
            }
            #endregion ID_194 AlarmReport
            public DynamicMetaObject GetMetaObject(Expression parameter)
            {
                return new AspectWeaver(parameter, this);
            }
        }
        public class CommandProcessor
        {
            private ALINE line = null;
            VehicleService service;
            public CommandProcessor(VehicleService _service, ALINE _line)
            {
                service = _service;
                line = _line;
            }

            //public bool Move(string vhID, string destination)
            public (bool isSuccess, ACMD moveCmd) Move(string vhID, string destination)
            {
                bool is_success = false;
                ACMD cmd_obj = null;
                is_success = scApp.CMDBLL.doCreatCommand(vhID, out cmd_obj, cmd_type: E_CMD_TYPE.Move, destination: destination);
                if (is_success)
                    setPreExcuteTranCmdID(vhID, "");
                //return scApp.CMDBLL.doCreatCommand(vhID, cmd_type: E_CMD_TYPE.Move, destination: destination);
                return (is_success, cmd_obj);
            }
            public bool MoveToCharge(string vhID, string destination)
            {
                bool is_success = scApp.CMDBLL.doCreatCommand(vhID, cmd_type: E_CMD_TYPE.Move_Charger, destination: destination);
                if (is_success)
                    setPreExcuteTranCmdID(vhID, "");
                return is_success;
            }
            public bool Load(string vhID, string cstID, string source, string sourcePortID)
            {
                bool is_success = scApp.CMDBLL.doCreatCommand(vhID, carrier_id: cstID, cmd_type: E_CMD_TYPE.Load, source: source,
                                                   sourcePort: sourcePortID);
                if (is_success)
                    setPreExcuteTranCmdID(vhID, "");
                return is_success;
            }
            public bool Unload(string vhID, string cstID, string destination, string destinationPortID)
            {
                bool is_success = scApp.CMDBLL.doCreatCommand(vhID, carrier_id: cstID, cmd_type: E_CMD_TYPE.Unload, destination: destination,
                                                   destinationPort: destinationPortID);
                if (is_success)
                    setPreExcuteTranCmdID(vhID, "");
                return is_success;
            }
            public bool Loadunload(string vhID, string cstID, string source, string destination, string sourcePortID, string destinationPortID)
            {
                bool is_success = scApp.CMDBLL.doCreatCommand(vhID, carrier_id: cstID, cmd_type: E_CMD_TYPE.LoadUnload, source: source, destination: destination,
                                                   sourcePort: sourcePortID, destinationPort: destinationPortID);
                if (is_success)
                    setPreExcuteTranCmdID(vhID, "");
                return is_success;
            }


            public (bool isSuccess, string transferID) CommandInitialFail(ACMD initial_cmd)
            {
                bool is_success = true;
                string finish_fransfer_cmd_id = "";
                try
                {
                    if (initial_cmd != null)
                    {
                        string vh_id = initial_cmd.VH_ID;
                        string initial_cmd_id = initial_cmd.ID;
                        finish_fransfer_cmd_id = initial_cmd.TRANSFER_ID;
                        is_success = is_success && scApp.CMDBLL.updateCommand_OHTC_StatusToFinish(initial_cmd_id, CompleteStatus.CommandInitailFail);
                        bool isTransfer = !SCUtility.isEmpty(finish_fransfer_cmd_id);
                        if (isTransfer)
                        {
                            scApp.CarrierBLL.db.updateState(initial_cmd.CARRIER_ID, E_CARRIER_STATE.MoveError);
                            scApp.TransferService.FinishTransferCommand(finish_fransfer_cmd_id, CompleteStatus.CommandInitailFail);
                        }
                    }
                }
                catch (Exception ex)
                {
                    is_success = false;
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: ex,
                       Details: $"process commamd initial fail ,has exception happend.cmd id:{initial_cmd?.ID}");
                }
                return (is_success, finish_fransfer_cmd_id);
            }
            public (bool isSuccess, string transferID) Finish(string finish_cmd_id, CompleteStatus completeStatus, int totalTravelDis = 0)
            {
                ACMD cmd = scApp.CMDBLL.getExcuteCMD_OHTCByCmdID(finish_cmd_id);
                string finish_fransfer_cmd_id = "";
                string vh_id = "";
                //確認是否為尚未結束的Task
                bool is_success = true;
                if (cmd != null)
                {
                    carrierStateCheck(cmd, completeStatus);
                    vh_id = cmd.VH_ID;
                    finish_fransfer_cmd_id = cmd.TRANSFER_ID;
                    is_success = is_success && scApp.CMDBLL.updateCommand_OHTC_StatusToFinish(finish_cmd_id, completeStatus);
                    //再確認是否為Transfer command
                    //是的話
                    //1.要上報MCS
                    //2.要將該Transfer改為結束
                    bool isTransfer = !SCUtility.isEmpty(finish_fransfer_cmd_id);
                    if (isTransfer)
                    {
                        //if (scApp.PortStationBLL.OperateCatch.IsEqPort(scApp.EqptBLL, cmd.DESTINATION_PORT))
                        //scApp.ReportBLL.newReportUnloadComplete(cmd.TRANSFER_ID, null);

                        is_success = is_success && scApp.CMDBLL.updateCMD_MCS_TranStatus2Complete(finish_fransfer_cmd_id, completeStatus);
                        //if (is_success)
                        //{
                        //    scApp.CMDBLL.moveCMD_MCSToHistory(finish_fransfer_cmd_id);//移往history
                        //}
                        is_success = is_success && scApp.ReportBLL.ReportTransferResult2MCS(finish_fransfer_cmd_id, completeStatus);
                        is_success = is_success && scApp.SysExcuteQualityBLL.SysExecQityfinish(finish_fransfer_cmd_id, completeStatus, totalTravelDis);
                        if (completeStatus == CompleteStatus.CmpStatusIdmisMatch ||
                            completeStatus == CompleteStatus.CmpStatusIdreadFailed)
                        {
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"start process:[{completeStatus}] script. finish cmd id:{finish_cmd_id}...",
                               VehicleID: vh_id);
                            var result = scApp.TransferService.processIDReadFailAndMismatch(cmd.CARRIER_ID, completeStatus);
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"process:[{completeStatus}] script success:[{result.isSuccess}], result:[{result.result}]." +
                                     $" finish cmd id:{finish_cmd_id}",
                               VehicleID: vh_id);

                        }
                        else if (completeStatus == CompleteStatus.CmpStatusUnload
                            || completeStatus == CompleteStatus.CmpStatusLoadunload) //命令正常完成，且目的地為EQ Port，要直接上報Carrier Remove
                        {
                            ////
                            ///

                            if (scApp.PortStationBLL.OperateDB.isPortStationEQPort(cmd.DESTINATION))
                            {
                                // 20210506 美微說OHTC上報EQ Port Carrier Remove就流程上是正確的，但是客戶會在MCS看不到CST造成恐慌。所以特此加入可以關閉上報的邏輯。
                                if (DebugParameter.isReportEQPortCarrierRemove)
                                {
                                    ACARRIER datainfo = new ACARRIER();
                                    datainfo.ID = cmd.CARRIER_ID;        //填CSTID
                                    datainfo.LOCATION = cmd.DESTINATION.Trim();  //填Port 名稱
                                    scApp.ReportBLL.ReportCarrierRemovedFromPort(datainfo, "2", null);
                                }
                                else
                                {
                                    scApp.CarrierBLL.db.delete(cmd.CARRIER_ID);
                                }
                            }
                        }
                        else if (completeStatus == CompleteStatus.CmpStatusInterlockError) //命令interlock error結束，要上一個Alarm 通知MCS
                        {
                            AVEHICLE vh = scApp.VehicleBLL.cache.getVehicle(vh_id);
                            if (vh != null)
                            {
                                APORTSTATION port = scApp.PortStationBLL.OperateCatch.getPortStation(vh.CUR_ADR_ID);
                                if (port == null)
                                {
                                    port = scApp.PortStationBLL.OperateCatch.getPortStationByAdrIDSingle(vh.CUR_ADR_ID);
                                }
                                if (port != null)
                                {
                                    scApp.LineService.ProcessAlarmReport("", vh_id, AlarmBLL.INTERLOCK_ERROR_OCCURED, ErrorStatus.ErrSet, $"Vehicle:[{vh.Real_ID}] report interlock error. Location:[{port.PORT_ID}].");
                                    scApp.LineService.ProcessAlarmReport("", vh_id, AlarmBLL.INTERLOCK_ERROR_OCCURED, ErrorStatus.ErrReset, $"Vehicle:[{vh.Real_ID}] report interlock error. Location:[{port.PORT_ID}].");
                                }
                                else
                                {
                                    scApp.LineService.ProcessAlarmReport("", vh_id, AlarmBLL.INTERLOCK_ERROR_OCCURED, ErrorStatus.ErrSet, $"Vehicle:[{vh.Real_ID}] report interlock error. Location Address:[{vh.CUR_ADR_ID}].");
                                    scApp.LineService.ProcessAlarmReport("", vh_id, AlarmBLL.INTERLOCK_ERROR_OCCURED, ErrorStatus.ErrReset, $"Vehicle:[{vh.Real_ID}] report interlock error. Location Address:[{vh.CUR_ADR_ID}].");
                                }

                            }


                        }
                    }
                }
                return (is_success, finish_fransfer_cmd_id);
            }

            /// <summary>
            /// 在命令132上報結束時，確認一下當下Carrier的狀態，
            /// 如果不是在Installed的狀態時，一律當作是MoveError
            /// </summary>
            /// <param name="cmd"></param>
            /// <param name="completeStatus"></param>
            private void carrierStateCheck(ACMD cmd, CompleteStatus completeStatus)
            {
                if (cmd == null) return;
                string carrier_id = cmd.CARRIER_ID;
                try
                {
                    bool is_carrier_trnasfer = !SCUtility.isEmpty(carrier_id);
                    if (is_carrier_trnasfer)
                    {
                        switch (completeStatus)
                        {
                            case CompleteStatus.CmpStatusAbort:
                            case CompleteStatus.CmpStatusCancel:
                            case CompleteStatus.CmpStatusInterlockError:
                            case CompleteStatus.CmpStatusVehicleAbort:
                                ACARRIER transfer_carrier = scApp.CarrierBLL.db.getCarrier(carrier_id);
                                if (transfer_carrier != null &&
                                    transfer_carrier.STATE != E_CARRIER_STATE.Installed)
                                {
                                    scApp.CarrierBLL.db.updateState(carrier_id, E_CARRIER_STATE.MoveError);
                                }
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: ex,
                       Details: $"process carrier state fail. carrier id:{carrier_id}");
                }
            }

            private long cmd_SyncPoint = 0;
            //public void Scan()
            public void Scan_backup()
            {
                if (System.Threading.Interlocked.Exchange(ref cmd_SyncPoint, 1) == 0)
                {
                    try
                    {
                        if (scApp.getEQObjCacheManager().getLine().ServiceMode
                            != SCAppConstants.AppServiceMode.Active)
                            return;
                        List<ACMD> CMD_OHTC_Queues = scApp.CMDBLL.loadCMD_OHTCMDStatusIsQueue();
                        if (CMD_OHTC_Queues == null || CMD_OHTC_Queues.Count == 0)
                            return;
                        foreach (ACMD cmd in CMD_OHTC_Queues)
                        {
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(CMDBLL), Device: string.Empty,
                               Data: $"Start process command ,id:{SCUtility.Trim(cmd.ID)},vh id:{SCUtility.Trim(cmd.VH_ID)},from:{SCUtility.Trim(cmd.SOURCE)},to:{SCUtility.Trim(cmd.DESTINATION)}");

                            string vehicle_id = cmd.VH_ID.Trim();
                            AVEHICLE assignVH = scApp.VehicleBLL.cache.getVehicle(vehicle_id);
                            if (!assignVH.isTcpIpConnect ||
                                !scApp.CMDBLL.canSendCmd(assignVH)) //todo kevin 需要確認是否要再判斷是否有命令的執行?
                                                                    //!scApp.CMDBLL.canSendCmd(vehicle_id)) //todo kevin 需要確認是否要再判斷是否有命令的執行?
                            {
                                continue;
                            }

                            bool is_success = service.Send.Command(assignVH, cmd);
                            if (!is_success)
                            {
                                //Finish(cmd.ID, CompleteStatus.Cancel);
                                //Finish(cmd.ID, CompleteStatus.VehicleAbort);
                                CommandInitialFail(cmd);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Exection:");
                    }
                    finally
                    {
                        System.Threading.Interlocked.Exchange(ref cmd_SyncPoint, 0);
                    }
                }
            }
            public void Scan()
            {
                if (System.Threading.Interlocked.Exchange(ref cmd_SyncPoint, 1) == 0)
                {
                    try
                    {
                        if (scApp.getEQObjCacheManager().getLine().ServiceMode
                            != SCAppConstants.AppServiceMode.Active)
                            return;
                        //List<ACMD> CMD_OHTC_Queues = scApp.CMDBLL.loadCMD_OHTCMDStatusIsQueue();
                        List<ACMD> unfinish_cmd = scApp.CMDBLL.loadUnfinishCmd();
                        line.CurrentExcuteCommand = unfinish_cmd;
                        if (unfinish_cmd == null || unfinish_cmd.Count == 0)
                            return;
                        List<ACMD> CMD_OHTC_Queues = unfinish_cmd.Where(cmd => cmd.CMD_STATUS == E_CMD_STATUS.Queue).ToList();
                        if (CMD_OHTC_Queues == null || CMD_OHTC_Queues.Count == 0)
                            return;
                        foreach (ACMD cmd in CMD_OHTC_Queues)
                        {
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(CMDBLL), Device: string.Empty,
                               Data: $"Start process command ,id:{SCUtility.Trim(cmd.ID)},vh id:{SCUtility.Trim(cmd.VH_ID)},from:{SCUtility.Trim(cmd.SOURCE)},to:{SCUtility.Trim(cmd.DESTINATION)}");

                            string vehicle_id = cmd.VH_ID.Trim();
                            AVEHICLE assignVH = scApp.VehicleBLL.cache.getVehicle(vehicle_id);
                            if (!assignVH.isTcpIpConnect ||
                                assignVH.ACT_STATUS != VHActionStatus.NoCommand ||
                                //!scApp.CMDBLL.canSendCmd(assignVH)) //todo kevin 需要確認是否要再判斷是否有命令的執行?
                                !scApp.CMDBLL.canSendCmdNew(assignVH)) //todo kevin 需要確認是否要再判斷是否有命令的執行?
                                                                       //!scApp.CMDBLL.canSendCmd(vehicle_id)) //todo kevin 需要確認是否要再判斷是否有命令的執行?
                            {
                                continue;
                            }

                            bool is_success = service.Send.Command(assignVH, cmd);
                            if (!is_success)
                            {
                                //Finish(cmd.ID, CompleteStatus.Cancel);
                                //Finish(cmd.ID, CompleteStatus.VehicleAbort);
                                CommandInitialFail(cmd);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Exection:");
                    }
                    finally
                    {
                        System.Threading.Interlocked.Exchange(ref cmd_SyncPoint, 0);
                    }
                }
            }


            /// <summary>
            /// 確認vh是否已經在準備要他去的Address上，如果還沒且
            /// </summary>
            /// <param name="assignVH"></param>
            /// <param name="cmd"></param>
            public void preMoveToSourcePort(AVEHICLE assignVH, ACMD cmd)
            {
                string vh_current_adr = assignVH.CUR_ADR_ID;
                string cmd_source_adr = cmd.SOURCE;
                //如果一樣 則代表已經在待命位上
                if (SCUtility.isMatche(vh_current_adr, cmd_source_adr)) return;
                var creat_result = service.Command.Move(assignVH.VEHICLE_ID, cmd.SOURCE);
                if (creat_result.isSuccess)
                    setPreExcuteTranCmdID(assignVH.VEHICLE_ID, cmd.TRANSFER_ID);
                //if (creat_result.isSuccess)
                //{
                //    bool is_success = service.Send.Command(assignVH, creat_result.moveCmd);
                //    if (!is_success)
                //    {
                //        CommandInitialFail(cmd);
                //    }
                //}
            }
            public void setPreExcuteTranCmdID(string vhID, string transferID)
            {
                AVEHICLE assignVH = scApp.VehicleBLL.cache.getVehicle(vhID);
                if (assignVH == null) return;
                assignVH.PreExcute_Transfer_ID = SCUtility.Trim(transferID, true);
            }
        }
        public class AvoidProcessor
        {
            VehicleService service;
            public AvoidProcessor(VehicleService _service)
            {
                service = _service;
            }
            public const string VehicleVirtualSymbol = "virtual";
            public void tryNotifyVhAvoid(string requestVhID, string reservedVhID)
            {
                if (System.Threading.Interlocked.Exchange(ref syncPoint_NotifyVhAvoid, 1) == 0)
                {
                    try
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                           Data: $"Try to notify vh avoid...,requestVh:{requestVhID} reservedVh:{reservedVhID}",
                           VehicleID: requestVhID);
                        if (SCUtility.isEmpty(reservedVhID)) return;
                        AVEHICLE reserved_vh = scApp.VehicleBLL.cache.getVehicle(reservedVhID);
                        AVEHICLE request_vh = scApp.VehicleBLL.cache.getVehicle(requestVhID);

                        //先確認是否可以進行趕車的確認，如果當前Reserved的車子狀態是
                        //1.發出Error的
                        //2.正在進行長充電的
                        //則要將來要得車子進行路徑Override
                        var check_can_creat_avoid_command = canCreatAvoidCommand(reserved_vh);
                        //if (canCreatAvoidCommand(reserved_vh))
                        if (check_can_creat_avoid_command.is_can)
                        {
                            string reserved_vh_current_section = reserved_vh.CUR_SEC_ID;

                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"start search section:{reserved_vh_current_section}",
                               VehicleID: requestVhID);

                            var findResult = findNotConflictSectionAndAvoidAddressNew(request_vh, reserved_vh, false);
                            if (!findResult.isFind)
                            {
                                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                   Data: $"find not conflict section fail. reserved section:{reserved_vh_current_section},",
                                   VehicleID: requestVhID);
                                return;
                            }
                            else
                            {
                                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                   Data: $"find not conflict section:{findResult.notConflictSection?.SEC_ID}.avoid address:{findResult.avoidAdr}",
                                   VehicleID: requestVhID);
                            }

                            string avoid_address = findResult.avoidAdr;


                            if (!SCUtility.isEmpty(avoid_address))
                            {
                                //bool is_success = scApp.CMDBLL.doCreatCommand(reserved_vh.VEHICLE_ID, string.Empty, string.Empty,
                                //                                    E_CMD_TYPE.Move,
                                //                                    string.Empty,
                                //                                    avoid_address);
                                bool is_success = service.Command.Move(reserved_vh.VEHICLE_ID, avoid_address).isSuccess;
                                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                   Data: $"Try to notify vh avoid,requestVh:{requestVhID} reservedVh:{reservedVhID}, is success :{is_success}.",
                                   VehicleID: requestVhID);
                            }
                            else
                            {
                                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                   Data: $"Try to notify vh avoid,requestVh:{requestVhID} reservedVh:{reservedVhID}, fail.",
                                   VehicleID: requestVhID);
                            }
                        }
                        else
                        {
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"Try to notify vh avoid,requestVh:{requestVhID} reservedVh:{reservedVhID}," +
                                     $"but reservedVh:{reservedVhID} status not ready." +
                                     $" isTcpIpConnect:{reserved_vh.isTcpIpConnect}" +
                                     $" MODE_STATUS:{reserved_vh.MODE_STATUS}" +
                                     $" ACT_STATUS:{reserved_vh.ACT_STATUS}" +
                                     $" result:{check_can_creat_avoid_command.result}",
                               VehicleID: requestVhID);

                            switch (check_can_creat_avoid_command.result)
                            {
                                case CAN_NOT_AVOID_RESULT.VehicleInError:
                                case CAN_NOT_AVOID_RESULT.VehicleInLongCharge:
                                    if (request_vh.IsReservePause)
                                    {
                                        //20210428 由於AT&S OHTC不需要51避車命令，將該處改為不使用
                                        //LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                        //   Data: $"Try to notify vh avoid fail,start over request of vh..., because reserved of status:{check_can_creat_avoid_command.result}," +
                                        //         $" requestVh:{requestVhID} reservedVh:{reservedVhID}.",
                                        //   VehicleID: requestVhID);
                                        ////todo 要實作要求避車的功能，在擋住路的
                                        //scApp.VehicleService.Avoid.trydoAvoidCommandToVh(request_vh, reserved_vh);
                                    }
                                    break;
                                default:
                                    if (request_vh.IsReservePause && reserved_vh.IsReservePause)
                                    {
                                        //如果兩台車都已經Reserve Pause了，就不再透過這邊進行避車
                                        //而是透過Deadlock的Timer來解除。
                                    }
                                    else if (request_vh.IsReservePause)
                                    {
                                        //20210428 由於AT&S OHTC不需要51避車命令，將該處改為不使用
                                        //if (IsBlockEachOrther(reserved_vh, request_vh))
                                        //{
                                        //    if (scApp.VehicleService.Avoid.trydoAvoidCommandToVh(request_vh, reserved_vh))
                                        //    {
                                        //        SpinWait.SpinUntil(() => false, 15000);
                                        //    }
                                        //}
                                        //else
                                        //{
                                        //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                        //       Data: $"request vh:{requestVhID} with reserved vh:{reservedVhID} of can't reserve info not same,don't excute Avoid",
                                        //       VehicleID: requestVhID);
                                        //}
                                    }
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                           Data: ex,
                           Details: $"excute tryNotifyVhAvoid has exception happend.requestVh:{requestVhID},reservedVh:{reservedVhID}");
                    }
                    finally
                    {
                        System.Threading.Interlocked.Exchange(ref syncPoint_NotifyVhAvoid, 0);
                    }
                }
            }

            private bool IsBlockEachOrther(AVEHICLE reserved_vh, AVEHICLE request_vh)
            {
                return (reserved_vh.CanNotReserveInfo != null && request_vh.CanNotReserveInfo != null) &&
                        SCUtility.isMatche(reserved_vh.CanNotReserveInfo.ReservedVhID, request_vh.VEHICLE_ID) &&
                        SCUtility.isMatche(request_vh.CanNotReserveInfo.ReservedVhID, reserved_vh.VEHICLE_ID);
            }

            public bool trydoAvoidCommandToVh(AVEHICLE avoidVh, AVEHICLE willPassVh)
            {
                var find_avoid_result = findNotConflictSectionAndAvoidAddressNew(willPassVh, avoidVh, true);
                string blocked_section = avoidVh.CanNotReserveInfo.ReservedSectionID;
                string blocked_vh_id = avoidVh.CanNotReserveInfo.ReservedVhID;
                if (find_avoid_result.isFind)
                {
                    avoidVh.VhAvoidInfo = null;
                    var avoid_request_result = service.Send.Avoid(avoidVh.VEHICLE_ID, find_avoid_result.avoidAdr);
                    if (avoid_request_result.is_success)
                    {
                        avoidVh.VhAvoidInfo = new AVEHICLE.AvoidInfo(blocked_section, blocked_vh_id);
                    }
                    return avoid_request_result.is_success;
                }
                else
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"No find the can avoid address. avoid vh:{avoidVh.VEHICLE_ID} current adr:{avoidVh.CUR_ADR_ID}," +
                             $"will pass vh:{willPassVh.VEHICLE_ID} current adr:{willPassVh.CUR_ADR_ID}",
                       VehicleID: avoidVh.VEHICLE_ID,
                       CarrierID: avoidVh.CST_ID);
                    return false;
                }
            }
            private long syncPoint_NotifyVhAvoid = 0;
            private enum CAN_NOT_AVOID_RESULT
            {
                Normal,
                VehicleInLongCharge,
                VehicleInError
            }
            private (bool is_can, CAN_NOT_AVOID_RESULT result) canCreatAvoidCommand(AVEHICLE reservedVh)
            {
                if (reservedVh.IsError)
                {
                    return (false, CAN_NOT_AVOID_RESULT.VehicleInError);
                }
                else
                {
                    bool is_can = reservedVh.isTcpIpConnect &&
                           (reservedVh.MODE_STATUS == VHModeStatus.AutoRemote || reservedVh.MODE_STATUS == VHModeStatus.AutoLocal) &&
                           reservedVh.ACT_STATUS == VHActionStatus.NoCommand &&
                           !reservedVh.isCommandEnding &&                           //如果是在結束命令中的話，就也先不要進行趕車因為此時可能有機會把準備在該port的貨給他載走 By Kevin
                           !scApp.CMDBLL.isCMD_OHTCQueueByVh(reservedVh.VEHICLE_ID);
                    //!scApp.CMDBLL.HasCMD_MCSInQueue();
                    return (is_can, CAN_NOT_AVOID_RESULT.Normal);
                }

            }
            private (bool isFind, ASECTION notConflictSection, string entryAdr, string avoidAdr) findNotConflictSectionAndAvoidAddressNew
                (AVEHICLE willPassVh, AVEHICLE findAvoidAdrOfVh, bool isDeadLock)
            {
                string will_pass_vh_cur_adr = willPassVh.CUR_ADR_ID;
                string find_avoid_vh_cur_adr = findAvoidAdrOfVh.CUR_ADR_ID;
                string needToAvoidAdr = string.Empty;

                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"In findNotConflictSectionAndAvoidAddressNew, Avoid vh:{findAvoidAdrOfVh.VEHICLE_ID},WillPass vh:{willPassVh.VEHICLE_ID} WillPass vh CMDID:{willPassVh.CMD_ID}",
                   VehicleID: findAvoidAdrOfVh.VEHICLE_ID);
                if (willPassVh.CMD_ID != null)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"In findNotConflictSectionAndAvoidAddressNew, willPassVh CMD_ID not null ,willPassVh CMDID:{willPassVh.CMD_ID} Avoid vh:{findAvoidAdrOfVh.VEHICLE_ID},WillPass vh:{willPassVh.VEHICLE_ID} WillPass vh CMDID:{willPassVh.CMD_ID}",
                       VehicleID: findAvoidAdrOfVh.VEHICLE_ID);
                    string mcs_cmd_id = scApp.CMDBLL.getTransferCmdIDByCmdID(willPassVh.CMD_ID);
                    if (!string.IsNullOrWhiteSpace(mcs_cmd_id))
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                           Data: $"In findNotConflictSectionAndAvoidAddressNew, mcs_cmd_id not null ,mcs_cmd_id:{mcs_cmd_id} Avoid vh:{findAvoidAdrOfVh.VEHICLE_ID},WillPass vh:{willPassVh.VEHICLE_ID} WillPass vh CMDID:{willPassVh.CMD_ID}",
                           VehicleID: findAvoidAdrOfVh.VEHICLE_ID);
                        ATRANSFER transfer = scApp.CMDBLL.GetTransferByID(mcs_cmd_id);
                        if (transfer != null)
                        {
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"In findNotConflictSectionAndAvoidAddressNew, transfer not null ,transfer cmd id:{transfer.ID} Avoid vh:{findAvoidAdrOfVh.VEHICLE_ID},WillPass vh:{willPassVh.VEHICLE_ID} WillPass vh CMDID:{willPassVh.CMD_ID}",
                               VehicleID: findAvoidAdrOfVh.VEHICLE_ID);
                            if (transfer.TRANSFERSTATE < E_TRAN_STATUS.Transferring)
                            {
                                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                   Data: $"In findNotConflictSectionAndAvoidAddressNew, transfer state not tranferring yet,transfer cmd id:{transfer.ID} transfer state:{transfer.TRANSFERSTATE} Avoid vh:{findAvoidAdrOfVh.VEHICLE_ID},WillPass vh:{willPassVh.VEHICLE_ID} WillPass vh CMDID:{willPassVh.CMD_ID}",
                                   VehicleID: findAvoidAdrOfVh.VEHICLE_ID);
                                var source_port_station = scApp.PortStationBLL.OperateCatch.getPortStation(transfer.HOSTSOURCE);
                                needToAvoidAdr = source_port_station == null ? string.Empty : source_port_station.ADR_ID;
                            }
                            else
                            {
                                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                   Data: $"In findNotConflictSectionAndAvoidAddressNew, transfer state already tranferring,transfer cmd id:{transfer.ID} transfer state:{transfer.TRANSFERSTATE} Avoid vh:{findAvoidAdrOfVh.VEHICLE_ID},WillPass vh:{willPassVh.VEHICLE_ID} WillPass vh CMDID:{willPassVh.CMD_ID}",
                                   VehicleID: findAvoidAdrOfVh.VEHICLE_ID);
                                var dest_port_station = scApp.PortStationBLL.OperateCatch.getPortStation(transfer.HOSTDESTINATION);
                                needToAvoidAdr = dest_port_station == null ? string.Empty : dest_port_station.ADR_ID;
                            }
                        }
                    }
                    else
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                           Data: $"In findNotConflictSectionAndAvoidAddressNew, mcs_cmd_id is null ,ohtc_cmd_id:{willPassVh.CMD_ID} Avoid vh:{findAvoidAdrOfVh.VEHICLE_ID},WillPass vh:{willPassVh.VEHICLE_ID} WillPass vh CMDID:{willPassVh.CMD_ID}",
                           VehicleID: findAvoidAdrOfVh.VEHICLE_ID);
                        ACMD cmd = scApp.CMDBLL.getExcuteCMD_OHTCByCmdID(willPassVh.CMD_ID);
                        if (cmd != null)
                        {
                            if (cmd.CMD_TYPE == E_CMD_TYPE.Move || cmd.CMD_TYPE == E_CMD_TYPE.Move_MTL || cmd.CMD_TYPE == E_CMD_TYPE.MTLHome
                                || cmd.CMD_TYPE == E_CMD_TYPE.SystemIn || cmd.CMD_TYPE == E_CMD_TYPE.SystemOut)
                            {
                                needToAvoidAdr = cmd.DESTINATION;
                            }
                            else
                            {
                                //do nothing
                            }
                        }
                    }
                }
                else
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
   Data: $"In findNotConflictSectionAndAvoidAddressNew, willPassVh CMD_ID is null , Avoid vh:{findAvoidAdrOfVh.VEHICLE_ID},WillPass vh:{willPassVh.VEHICLE_ID} WillPass vh CMDID:{willPassVh.CMD_ID}",
   VehicleID: findAvoidAdrOfVh.VEHICLE_ID);
                }
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
   Data: $"In findNotConflictSectionAndAvoidAddressNew,search needToAvoidAdr end,needToAvoidAdr:{needToAvoidAdr} Avoid vh:{findAvoidAdrOfVh.VEHICLE_ID},WillPass vh:{willPassVh.VEHICLE_ID} WillPass vh CMDID:{willPassVh.CMD_ID}",
   VehicleID: findAvoidAdrOfVh.VEHICLE_ID);




                var block_control_check_result = scApp.getCommObjCacheManager().IsBlockControlSection(findAvoidAdrOfVh.CUR_SEC_ID);
                if (block_control_check_result.isBlockControlSec)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"find avoid vh:{findAvoidAdrOfVh.VEHICLE_ID} is in block, find avoid adr id:{block_control_check_result.enhanceInfo.WayOutAddress}",
                       VehicleID: findAvoidAdrOfVh.VEHICLE_ID);
                    return (true, new ASECTION(), "", block_control_check_result.enhanceInfo.WayOutAddress);
                }
                else
                {
                    //先找找看有沒有不會衝突的避車點
                    var is_find_not_conflict = findNotConflictAvoidAdr(find_avoid_vh_cur_adr, needToAvoidAdr);
                    if (is_find_not_conflict.isFind)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                           Data: $"find avoid vh:{findAvoidAdrOfVh.VEHICLE_ID} is not in block, find not conflict avoid adr id:{is_find_not_conflict.canAvoidAdrID}",
                           VehicleID: findAvoidAdrOfVh.VEHICLE_ID);
                        return (true, new ASECTION(), "", is_find_not_conflict.canAvoidAdrID);
                    }
                    //找不到再找一個會衝突但最近的
                    var is_find_closest = findClosestAvoidAdr(find_avoid_vh_cur_adr);
                    if (is_find_closest.isFind)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                           Data: $"find avoid vh:{findAvoidAdrOfVh.VEHICLE_ID} is not in block, find closest avoid adr id:{is_find_closest.canAvoidAdrID}",
                           VehicleID: findAvoidAdrOfVh.VEHICLE_ID);
                        return (true, new ASECTION(), "", is_find_closest.canAvoidAdrID);
                    }
                }
                ASECTION find_avoid_vh_current_section = scApp.SectionBLL.cache.GetSection(findAvoidAdrOfVh.CUR_SEC_ID);
                //先找出哪個Address是距離即將到來的車子比較遠，即反方向
                string first_search_adr = findTheOppositeOfAddress(will_pass_vh_cur_adr, find_avoid_vh_current_section);

                (string next_address, ASECTION source_section) first_search_section_infos = (first_search_adr, find_avoid_vh_current_section);
                var searchResult = tryFindAvoidAddressByOneWay(willPassVh, findAvoidAdrOfVh, first_search_section_infos, false);
                if (!isDeadLock && !searchResult.isFind)
                {
                    string second_search_adr = SCUtility.isMatche(first_search_adr, find_avoid_vh_current_section.FROM_ADR_ID) ?
                        find_avoid_vh_current_section.TO_ADR_ID : find_avoid_vh_current_section.FROM_ADR_ID;

                    (string next_address, ASECTION source_section) second_search_section_infos = (second_search_adr, find_avoid_vh_current_section);
                    searchResult = tryFindAvoidAddressByOneWay(willPassVh, findAvoidAdrOfVh, second_search_section_infos, false);
                }
                return searchResult;
            }
            private (bool isFind, string canAvoidAdrID) findClosestAvoidAdr(string vhCurAdrID)
            {
                double minimum_cost = double.MaxValue;
                string closest_avoid_adr = "";
                var can_avoid_adrs = scApp.AddressesBLL.cache.LoadCanAvoidAddresses();
                foreach (var adr in can_avoid_adrs)
                {
                    if (!adr.IsAccessable)
                    {
                        continue;
                    }
                    if (SCUtility.isMatche(adr.ADR_ID, vhCurAdrID))
                        continue;
                    double total_section_distance = 0;
                    //var result = scApp.GuideBLL.getGuideInfo(vhCurAdrID, adr.ADR_ID);
                    var result = scApp.GuideBLL.IsRoadWalkable(vhCurAdrID, adr.ADR_ID, out int totalCost);
                    if (result)
                    {
                        //total_section_distance = result.totalCost;
                        total_section_distance = totalCost;
                    }
                    else
                    {
                        total_section_distance = double.MaxValue;
                    }
                    if (total_section_distance < minimum_cost)
                    {
                        minimum_cost = total_section_distance;
                        closest_avoid_adr = SCUtility.Trim(adr.ADR_ID, true);
                    }
                }
                return (!SCUtility.isEmpty(closest_avoid_adr), closest_avoid_adr);
            }


            public (bool isFind, string canAvoidAdrID) findNotConflictAvoidAdr(string vhCurAdrID, string needToAvoidAdr)
            {
                double minimum_cost = double.MaxValue;
                string closest_avoid_adr = "";
                var can_avoid_adrs = scApp.AddressesBLL.cache.LoadCanAvoidAddresses();

                var _result = scApp.GuideBLL.IsRoadWalkable(vhCurAdrID, needToAvoidAdr, out int leastCost);
                if (_result)
                {
                    foreach (var adr in can_avoid_adrs)
                    {
                        if (!adr.IsAccessable)
                        {
                            continue;
                        }
                        if (SCUtility.isMatche(adr.ADR_ID, vhCurAdrID))
                            continue;
                        double total_section_distance = 0;
                        //var result = scApp.GuideBLL.getGuideInfo(vhCurAdrID, adr.ADR_ID);
                        var result = scApp.GuideBLL.IsRoadWalkable(vhCurAdrID, adr.ADR_ID, out int totalCost);
                        if (result)
                        {
                            //total_section_distance = result.totalCost;
                            total_section_distance = totalCost;
                        }
                        else
                        {
                            total_section_distance = double.MaxValue;
                        }
                        if (total_section_distance < minimum_cost && leastCost < total_section_distance)//cost要大過到要求趕車車輛的目的地，才不會衝突，只能用在AT&S因為只有一條環路
                        {
                            minimum_cost = total_section_distance;
                            closest_avoid_adr = SCUtility.Trim(adr.ADR_ID, true);
                        }
                    }
                    return (!SCUtility.isEmpty(closest_avoid_adr), closest_avoid_adr);
                }
                else
                {
                    return (!SCUtility.isEmpty(closest_avoid_adr), closest_avoid_adr);
                }

            }

            private string findTheOppositeOfAddress(string req_vh_cur_adr, ASECTION reserved_vh_current_section)
            {
                string opposite_address = "";
                int from_distance = 0;
                //var from_adr_guide_result = scApp.GuideBLL.getGuideInfo(req_vh_cur_adr, reserved_vh_current_section.FROM_ADR_ID);
                var from_adr_guide_result = scApp.GuideBLL.IsRoadWalkable(req_vh_cur_adr, reserved_vh_current_section.FROM_ADR_ID, out int totalCost_vh_fromAdr);
                if (from_adr_guide_result)
                {
                    //from_distance = from_adr_guide_result.totalCost;
                    from_distance = totalCost_vh_fromAdr;
                }
                int to_distance = 0;
                //var to_adr_guide_result = scApp.GuideBLL.getGuideInfo(req_vh_cur_adr, reserved_vh_current_section.TO_ADR_ID);
                var to_adr_guide_result = scApp.GuideBLL.IsRoadWalkable(req_vh_cur_adr, reserved_vh_current_section.TO_ADR_ID, out int totalCost_vh_toAdr);
                if (to_adr_guide_result)
                {
                    //to_distance = to_adr_guide_result.totalCost;
                    to_distance = totalCost_vh_toAdr;
                }
                if (from_distance > to_distance)
                {
                    opposite_address = reserved_vh_current_section.FROM_ADR_ID;
                }
                else
                {
                    opposite_address = reserved_vh_current_section.TO_ADR_ID;
                }
                return opposite_address;
            }
            private (bool isFind, ASECTION notConflictSection, string entryAdr, string avoidAdr) tryFindAvoidAddressByOneWay
                (AVEHICLE willPassVh, AVEHICLE findAvoidAdrVh, (string next_address, ASECTION source_section) startSearchInfo, bool isForceCrossing)
            {
                int calculation_count = 0;
                int max_calculation_count = 20;

                List<(string next_address, ASECTION source_section)> next_search_infos =
                    new List<(string next_address, ASECTION source_section)>() { startSearchInfo };
                List<(string next_address, ASECTION source_section)> next_search_address_temp =
                    new List<(string, ASECTION)>();

                ASECTION not_conflict_section = null;
                string avoid_address = null;
                string orther_end_point = "";
                //string virtual_vh_id = "";
                List<string> virtual_vh_ids = new List<string>();

                try
                {
                    //在一開始的時候就先Set一台虛擬車在相同位置，防止找到鄰近的Address
                    var hlt_vh_obj = scApp.ReserveBLL.GetMapVehicleInfo(findAvoidAdrVh.VEHICLE_ID);
                    string virtual_vh_id = $"{VehicleVirtualSymbol}_{findAvoidAdrVh.VEHICLE_ID}";
                    scApp.ReserveBLL.TryAddVehicleOrUpdate(virtual_vh_id, "", hlt_vh_obj.X, hlt_vh_obj.Y, hlt_vh_obj.Angle, 0,
                        sensorDir: Mirle.Hlts.Utils.HltDirection.None,
                          forkDir: Mirle.Hlts.Utils.HltDirection.None);
                    virtual_vh_ids.Add(virtual_vh_id);
                    do
                    {
                        foreach (var search_info in next_search_infos.ToArray())
                        {
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"start search address:{search_info.next_address}",
                               VehicleID: willPassVh.VEHICLE_ID);
                            //next_search_address.Clear();
                            List<ASECTION> next_sections = scApp.SectionBLL.cache.GetSectionsByAddress(search_info.next_address);

                            //先把自己的Section移除
                            next_sections.Remove(search_info.source_section);
                            if (next_sections != null && next_sections.Count() > 0)
                            {
                                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                   Data: $"next search section:{string.Join(",", next_sections.Select(sec => sec.SEC_ID).ToArray())}",
                                   VehicleID: willPassVh.VEHICLE_ID);
                                //過濾掉已經Disable的Segment
                                next_sections = next_sections.Where(sec => sec.IsActive(scApp.SegmentBLL)).ToList();
                                if (next_sections != null && next_sections.Count() > 0)
                                {
                                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                       Data: $"next search section:{string.Join(",", next_sections.Select(sec => sec.SEC_ID).ToArray())} after filter not in active",
                                       VehicleID: willPassVh.VEHICLE_ID);
                                }
                                else
                                {
                                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                       Data: $"search result is empty after filter not in active ,search adr:{search_info.next_address}",
                                       VehicleID: willPassVh.VEHICLE_ID);
                                }
                            }
                            else
                            {
                                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                   Data: $"search result is empty,search adr:{search_info.next_address}",
                                   VehicleID: willPassVh.VEHICLE_ID);
                            }

                            //當找出兩段以上的Section時且他的Source為會與另一台vh前進路徑交錯的車，
                            //代表找到了叉路，因此要在入口加入一台虛擬車來幫助找避車路徑時確保不會卡住的點。
                            if (next_sections.Count >= 2 &&
                                hasCrossWithPredictSection(search_info.source_section.SEC_ID, willPassVh.WillPassSectionID))
                            //hasCrossWithPredictSection(search_info.source_section.SEC_ID, requestVh.PredictSections))
                            {
                                string virtual_vh_section_id = $"{virtual_vh_id}_{search_info.next_address}";
                                scApp.ReserveBLL.TryAddVehicleOrUpdate(virtual_vh_section_id, search_info.next_address);
                                virtual_vh_ids.Add(virtual_vh_section_id);
                                //scApp.ReserveBLL.ForceUpdateVehicle(virtual_vh_id, search_info.next_address);
                                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                   Data: $"Add virtual in reserve system vh:{virtual_vh_section_id} in address id:{search_info.next_address}",
                                   VehicleID: findAvoidAdrVh.VEHICLE_ID);
                            }
                            foreach (ASECTION sec in next_sections)
                            {
                                //if (sec == search_info.source_section) continue;
                                orther_end_point = sec.GetOrtherEndPoint(search_info.next_address);
                                //如果跟目前找停車位的車子同一個點位時，代表找回到了原點，因此要把它濾掉。
                                if (SCUtility.isMatche(findAvoidAdrVh.CUR_ADR_ID, orther_end_point))
                                {
                                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                       Data: $"sec id:{SCUtility.Trim(sec.SEC_ID)} of orther end point:{orther_end_point} same with vh current address pass this section",
                                       VehicleID: willPassVh.VEHICLE_ID);
                                    continue;
                                }
                                //if (!isForceCrossing)
                                //{
                                //if (requestVh.PredictSections != null && requestVh.PredictSections.Count() > 0)
                                if (willPassVh.WillPassSectionID != null && willPassVh.WillPassSectionID.Count() > 0)
                                {
                                    //if (requestVh.PredictSections.Contains(SCUtility.Trim(sec.SEC_ID)))
                                    if (willPassVh.WillPassSectionID.Contains(SCUtility.Trim(sec.SEC_ID)))
                                    {
                                        //next_search_address.Add(next_calculation_address);
                                        next_search_address_temp.Add((orther_end_point, sec));
                                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                           Data: $"sec id:{SCUtility.Trim(sec.SEC_ID)} is request_vh of will sections:{string.Join(",", willPassVh.WillPassSectionID)}.by pass it,continue find next address{orther_end_point}",
                                           VehicleID: willPassVh.VEHICLE_ID);
                                        continue;
                                    }
                                }
                                //}
                                //取得沒有相交的Section後，在確認是否該Orther end point是一個可以避車且不是R2000的任一端點，如果是的話就可以拿來作為一個避車點
                                AADDRESS orther_end_address = scApp.AddressesBLL.cache.GetAddress(orther_end_point);
                                if (!orther_end_address.IsPort(scApp.PortStationBLL))
                                {
                                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                       Data: $"sec id:{SCUtility.Trim(sec.SEC_ID)} of orther end point:{orther_end_point} is not can avoid address(not port), continue find next address{orther_end_point}",
                                       VehicleID: willPassVh.VEHICLE_ID);
                                    next_search_address_temp.Add((orther_end_point, sec));
                                    continue;
                                }

                                //if (!orther_end_address.canAvoidVhecle)
                                //if (!orther_end_address.canAvoidVehicle(scApp.SectionBLL))
                                //{
                                //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                //       Data: $"sec id:{SCUtility.Trim(sec.SEC_ID)} of orther end point:{orther_end_point} is not can avoid address, continue find next address{orther_end_point}",
                                //       VehicleID: willPassVh.VEHICLE_ID);
                                //    next_search_address_temp.Add((orther_end_point, sec));
                                //    continue;
                                //}
                                //找到以後嘗試去預約看看，確保該路徑是否還會干涉到該台VH
                                //還是有干涉到的話就繼續往下找
                                //var reserve_check_result = scApp.ReserveBLL.TryAddReservedSection(findAvoidAdrVh.VEHICLE_ID, sec.SEC_ID, isAsk: true);
                                //if (!reserve_check_result.OK &&
                                //    !reserve_check_result.VehicleID.StartsWith(VehicleVirtualSymbol))
                                //{
                                //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                //       Data: $"sec id:{SCUtility.Trim(sec.SEC_ID)} try to reserve fail,result:{reserve_check_result.Description}.",
                                //       VehicleID: willPassVh.VEHICLE_ID);
                                //    if (isForceCrossing)
                                //        next_search_address_temp.Add((orther_end_point, sec));
                                //    else
                                //    {
                                //        AVEHICLE obstruct_vh = scApp.VehicleBLL.cache.getVehicle(reserve_check_result.VehicleID);
                                //        if (obstruct_vh != null && !SCUtility.isMatche(sec.SEC_ID, obstruct_vh.CUR_SEC_ID))
                                //        {
                                //            next_search_address_temp.Add((orther_end_point, sec));
                                //        }
                                //    }
                                //    continue;
                                //}
                                //else
                                //{
                                //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                //       Data: $"sec id:{SCUtility.Trim(sec.SEC_ID)} try to reserve success,result:{reserve_check_result.Description}.",
                                //       VehicleID: willPassVh.VEHICLE_ID);
                                //}
                                not_conflict_section = sec;
                                avoid_address = orther_end_point;
                                return (true, not_conflict_section, search_info.next_address, avoid_address);
                            }
                        }
                        next_search_infos = next_search_address_temp.ToList();
                        next_search_address_temp.Clear();
                        calculation_count++;
                    } while (next_search_infos.Count() != 0 && calculation_count < max_calculation_count);
                }
                finally
                {
                    if (virtual_vh_ids != null && virtual_vh_ids.Count > 0)
                    {
                        foreach (string virtual_vh_id in virtual_vh_ids)
                        {
                            scApp.ReserveBLL.RemoveVehicle(virtual_vh_id);
                            //scApp.ReserveBLL.ForceUpdateVehicle(virtual_vh_id, 0, 0, 0);
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"remove virtual in reserve system vh:{virtual_vh_id} ",
                               VehicleID: findAvoidAdrVh.VEHICLE_ID);
                        }
                    }
                }
                return (false, null, null, null);
            }
            private bool hasCrossWithPredictSection(string checkSection, List<string> willPassSection)
            {
                if (willPassSection == null || willPassSection.Count() == 0) return false;
                if (SCUtility.isEmpty(checkSection)) return false;
                return willPassSection.Contains(SCUtility.Trim(checkSection));
            }
        }
        #region Event
        public event EventHandler<DeadLockEventArgs> DeadLockProcessFail;
        public void onDeadLockProcessFail(AVEHICLE vehicle1, AVEHICLE vehicle2)
        {
            SystemParameter.setAutoOverride(false);
            DeadLockProcessFail?.Invoke(this, new DeadLockEventArgs(vehicle1, vehicle2));
        }
        #endregion Event
        public virtual void Start(SCApplication app)
        {
            scApp = app;
            Send = new SendProcessor(scApp);
            Receive = new ReceiveProcessor(this);
            Command = new CommandProcessor(this, scApp.getEQObjCacheManager().getLine());
            Avoid = new AvoidProcessor(this);
            List<AVEHICLE> vhs = scApp.getEQObjCacheManager().getAllVehicle();

            foreach (var vh in vhs)
            {
                vh.ConnectionStatusChange += (s1, e1) => PublishVhInfo(s1, ((AVEHICLE)s1).VEHICLE_ID);
                vh.ExcuteCommandStatusChange += (s1, e1) => PublishVhInfo(s1, e1);
                vh.VehicleStatusChange += (s1, e1) => PublishVhInfo(s1, e1);
                vh.VehiclePositionChange += (s1, e1) => PublishVhInfo(s1, e1);
                vh.ErrorStatusChange += (s1, e1) => Vh_ErrorStatusChange(s1, e1);


                vh.addEventHandler(nameof(VehicleService), nameof(vh.isTcpIpConnect), PublishVhInfo);
                vh.PositionChange += Vh_PositionChange;
                vh.LocationChange += Vh_LocationChange;
                vh.SegmentChange += Vh_SegementChange;
                vh.LongTimeNoCommuncation += Vh_LongTimeNoCommuncation;
                vh.LongTimeInaction += Vh_LongTimeInaction;
                vh.LongTimeDisconnection += Vh_LongTimeDisconnection;
                vh.ModeStatusChange += Vh_ModeStatusChange;
                vh.Idling += Vh_Idling;
                //vh.CurrentExcuteCmdChange += Vh_CurrentExcuteCmdChange;
                vh.StatusRequestFailOverTimes += Vh_StatusRequestFailOverTimes;
                vh.SetupTimerAction();
            }
        }

        //protected void Vh_CurrentExcuteCmdChange(object sender, string e)
        //{
        //    try
        //    {
        //        AVEHICLE vh = sender as AVEHICLE;
        //        if (vh.IsCloseToAGVStation)
        //        {
        //            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
        //               Data: $"Close to agv station is on",
        //               VehicleID: vh.VEHICLE_ID,
        //               CST_ID_L: vh.CST_ID_L,
        //               CST_ID_R: vh.CST_ID_R);

        //            return;
        //        }
        //        string current_excute_cmd_id = SCUtility.Trim(e, true);
        //        if (SCUtility.isEmpty(current_excute_cmd_id)) return;
        //        //先確認目前執行的命令，是否是要去AGV Station 進行Load/Unload
        //        //是的話則判斷是否已經進入到N公尺m內
        //        //如果是 則將通知OHBC將此AGV ST進行開蓋
        //        bool has_excute_cmd = !SCUtility.isEmpty(vh.CurrentExcuteCmdID);
        //        if (!has_excute_cmd)
        //            return;
        //        ACMD current_excute_cmd = scApp.CMDBLL.cache.getExcuteCmd(current_excute_cmd_id);
        //        if (current_excute_cmd == null)
        //            return;
        //        if (current_excute_cmd.CMD_TYPE == E_CMD_TYPE.LoadUnload || current_excute_cmd.CMD_TYPE == E_CMD_TYPE.Unload)
        //        {
        //            //not thing...
        //        }
        //        else
        //        {
        //            return;
        //        }
        //        LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
        //           Data: $"Start check pre open cover scenario,vh id:{vh.VEHICLE_ID} current excute cmd:{current_excute_cmd_id} " +
        //                 $"source port:{SCUtility.Trim(current_excute_cmd.SOURCE_PORT, true)} target port:{SCUtility.Trim(current_excute_cmd.DESTINATION_PORT, true)} ...,",
        //           VehicleID: vh.VEHICLE_ID,
        //           CST_ID_L: vh.CST_ID_L,
        //           CST_ID_R: vh.CST_ID_R);

        //        checkWillGoToPortIsAGVStationAndIsNeedPreOpenCover(vh, current_excute_cmd);

        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "Exception:");
        //    }
        //}

        protected void Vh_StatusRequestFailOverTimes(object sender, int e)
        {
            try
            {
                AVEHICLE vh = sender as AVEHICLE;
                vh.StatusRequestFailTimes = 0;
                vh.stopVehicleTimer();

                //1.當Status要求失敗超過3次時，要將對應的Port關閉再開啟。
                //var endPoint = vh.getIPEndPoint(scApp.getBCFApplication());
                int port_num = vh.getPortNum(scApp.getBCFApplication());
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"Over {AVEHICLE.MAX_STATUS_REQUEST_FAIL_TIMES} times request status fail, begin restart tcpip server port:{port_num}...",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                stopVehicleTcpIpServer(vh);
                SpinWait.SpinUntil(() => false, 2000);
                startVehicleTcpIpServer(vh);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex);
            }
        }
        #region Vh Event Handler
        protected void Vh_ErrorStatusChange(object sender, VhStopSingle vhStopSingle)
        {
            AVEHICLE vh = sender as AVEHICLE;
            if (vh == null) return;
            try
            {
                if (vhStopSingle == VhStopSingle.StopSingleOn)
                {
                    Task.Run(() => scApp.VehicleBLL.web.errorHappendNotify());
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
            }
        }
        protected virtual void Vh_ModeStatusChange(object sender, VHModeStatus e)
        {
            AVEHICLE vh = sender as AVEHICLE;
            if (vh == null) return;
            try
            {
                //LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                //   Data: $"Process vehicle mode change ,change to mode status:{e}",
                //   VehicleID: vh.VEHICLE_ID,
                //   CST_ID_L: vh.CST_ID_L,
                //   CST_ID_R: vh.CST_ID_R);

                ////如果他是變成manual mode的話，則需要報告無法服務的Alarm給 MCS
                //if (e == VHModeStatus.AutoCharging ||
                //    e == VHModeStatus.AutoLocal ||
                //    e == VHModeStatus.AutoRemote)
                //{
                //    scApp.LineService.ProcessAlarmReport(vh, AlarmBLL.VEHICLE_CAN_NOT_SERVICE, ErrorStatus.ErrReset, $"vehicle cannot service");
                //}
                //else
                //{
                //    if (vh.IS_INSTALLED)
                //        scApp.LineService.ProcessAlarmReport(vh, AlarmBLL.VEHICLE_CAN_NOT_SERVICE, ErrorStatus.ErrSet, $"vehicle cannot service");
                //}
                if (e == VHModeStatus.AutoLocal ||
                    e == VHModeStatus.AutoRemote)
                {
                    doDataSysc(vh.VEHICLE_ID);
                    Send.AlarmReset(vh.VEHICLE_ID);
                }
                if (e == VHModeStatus.Manual)
                {
                    //AGVC,200001,2,Vehicle switch to manual mode.
                    //string error_code = "65535";
                    //var error_status = sc.ProtocolFormat.OHTMessage.ErrorStatus.ErrSet;
                    //scApp.LineService.ProcessAlarmReport(vh, error_code, error_status, "Vehicle switch to manual mode");
                    scApp.LineService.ProcessAlarmReport(vh, AlarmBLL.OHTC_VEHICLE_MANUAL, ErrorStatus.ErrSet, $"Vehicle switch to manual mode.");
                    vh.ToSectionID = string.Empty;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
            }
        }
        protected void Vh_LongTimeDisconnection(object sender, EventArgs e)
        {
            AVEHICLE vh = sender as AVEHICLE;
            if (vh == null) return;
            try
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"Process vehicle long time disconnection",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);

                //要再上報Alamr Rerport給MCS
                if (vh.IS_INSTALLED)
                    //scApp.LineService.ProcessAlarmReport(vh, AlarmBLL.VEHICLE_CAN_NOT_SERVICE, ErrorStatus.ErrSet, $"vehicle cannot service");
                    scApp.LineService.ProcessAlarmReport(vh, AlarmBLL.OHTC_VEHICLE_DISCONNECTED, ErrorStatus.ErrSet, $"Vehicle has been disconnected.");
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
            }
        }
        private long syncPoint_ProcLongTimeInaction = 0;
        protected void Vh_LongTimeInaction(object sender, List<string> cmdIDs)
        {
            AVEHICLE vh = sender as AVEHICLE;
            if (vh == null) return;
            if (System.Threading.Interlocked.Exchange(ref syncPoint_ProcLongTimeInaction, 1) == 0)
            {

                try
                {
                    string cmd_ids = string.Join(",", cmdIDs);
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"Process vehicle long time inaction, cmd id:{cmd_ids}",
                       VehicleID: vh.VEHICLE_ID,
                       CarrierID: vh.CST_ID);

                    //當發生命令執行過久之後要將該筆命令改成Abormal end，如果該筆命令是MCS的Command則需要將命令上報給MCS作為結束
                    //Command.Finish(cmdID, CompleteStatus.LongTimeInaction);
                    //要再上報Alamr Rerport給MCS
                    scApp.LineService.ProcessAlarmReport(vh, AlarmBLL.VEHICLE_LONG_TIME_INACTION_0, ErrorStatus.ErrSet, $"vehicle long time inaction, cmd ids:{cmd_ids}");
                }
                catch (Exception ex)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: ex,
                       VehicleID: vh.VEHICLE_ID,
                       CarrierID: vh.CST_ID);
                }
                finally
                {
                    System.Threading.Interlocked.Exchange(ref syncPoint_ProcLongTimeInaction, 0);
                }
            }
        }
        protected void Vh_LongTimeNoCommuncation(object sender, EventArgs e)
        {
            AVEHICLE vh = sender as AVEHICLE;
            if (vh == null) return;
            //當發生很久沒有通訊的時候，就會發送143去進行狀態的詢問，確保Control還與Vehicle連線著
            bool is_success = Send.StatusRequest(vh.VEHICLE_ID);
            //如果連續三次 都沒有得到回覆時，就將Port關閉在重新打開
            if (!is_success)
            {
                vh.StatusRequestFailTimes++;
            }
            else
            {
                vh.StatusRequestFailTimes = 0;
            }
        }

        private void Vh_PositionChangeOld(object sender, PositionChangeEventArgs e)
        {
            try
            {

                AVEHICLE vh = sender as AVEHICLE;

                if (vh.IsCloseToAGVStation) return;
                //先確認目前執行的命令，是否是要去AGV Station 進行Load/Unload
                //是的話則判斷是否已經進入到N公尺m內
                //如果是 則將通知OHBC將此AGV ST進行開蓋
                bool has_excute_cmd = !SCUtility.isEmpty(vh.CurrentExcuteCmdID);
                if (!has_excute_cmd)
                    return;
                string excute_cmd_id = vh.CurrentExcuteCmdID;
                ACMD excute_cmd = scApp.CMDBLL.cache.getExcuteCmd(excute_cmd_id);
                if (excute_cmd == null)
                    return;
                if (excute_cmd.CMD_TYPE == E_CMD_TYPE.LoadUnload || excute_cmd.CMD_TYPE == E_CMD_TYPE.Unload)
                {
                    //not thing...
                }
                else
                {
                    return;
                }
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"Start check pre open cover scenario,vh id:{vh.VEHICLE_ID} current excute cmd:{excute_cmd_id} " +
                         $"source port:{SCUtility.Trim(excute_cmd.SOURCE_PORT, true)} target port:{SCUtility.Trim(excute_cmd.DESTINATION_PORT, true)} ...,",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);

                bool is_carry_cmd_cst = scApp.VehicleBLL.cache.IsCarryCstByCstID(vh.VEHICLE_ID, excute_cmd.CARRIER_ID);
                if (is_carry_cmd_cst)
                {
                    bool is_agv_station_traget = excute_cmd.IsTargetPortAGVStation(scApp.PortStationBLL, scApp.EqptBLL);
                    if (is_agv_station_traget)
                    {
                        var target_port_station = scApp.PortStationBLL.OperateCatch.getPortStation(excute_cmd.DESTINATION_PORT);
                        var get_axis_result = target_port_station.getAxis(scApp.ReserveBLL);
                        if (get_axis_result.isSuccess)
                        {
                            double x_port_station = get_axis_result.x;
                            double y_port_station = get_axis_result.y;
                            double vh_port_distance = getDistance(vh.X_Axis, vh.Y_Axis, x_port_station, y_port_station);
                            if (vh_port_distance < SystemParameter.OpenAGVStationCoverDistance_mm)
                            {
                                vh.IsCloseToAGVStation = true;
                                var agv_station = excute_cmd.getTragetPortEQ(scApp.PortStationBLL, scApp.EqptBLL);
                                List<APORTSTATION> pre_open_port_cover_list = (agv_station as AGVStation).loadAutoAGVStationPorts();
                                //string notify_port_id = excute_cmd.DESTINATION_PORT;
                                int open_count = 0;
                                foreach (var port_sation in pre_open_port_cover_list)
                                {
                                    Task.Run(() => scApp.TransferBLL.web.preOpenAGVStationCover(agv_station as IAGVStationType, port_sation.PORT_ID));
                                    open_count++;
                                    if (open_count >= 2)
                                    {
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                   Data: $"Start check pre open cover scenario,vh id:{vh.VEHICLE_ID} current excute cmd:{excute_cmd_id} " +
                                         $"source port:{SCUtility.Trim(excute_cmd.SOURCE_PORT, true)} target port:{SCUtility.Trim(excute_cmd.DESTINATION_PORT, true)}," +
                                         $"dis:{vh_port_distance} not enogh with target port",
                               VehicleID: vh.VEHICLE_ID,
                               CarrierID: vh.CST_ID);
                            }

                        }
                        else
                        {
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"Start check pre open cover scenario,vh id:{vh.VEHICLE_ID} current excute cmd:{excute_cmd_id} " +
                                     $"source port:{SCUtility.Trim(excute_cmd.SOURCE_PORT, true)} target port:{SCUtility.Trim(excute_cmd.DESTINATION_PORT, true)}," +
                                     $"target port adr(x,y) not exist",
                               VehicleID: vh.VEHICLE_ID,
                               CarrierID: vh.CST_ID);
                        }

                    }
                    else
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                           Data: $"Start check pre open cover scenario,vh id:{vh.VEHICLE_ID} current excute cmd:{excute_cmd_id} " +
                                 $"source port:{SCUtility.Trim(excute_cmd.SOURCE_PORT, true)} target port:{SCUtility.Trim(excute_cmd.DESTINATION_PORT, true)}," +
                                 $"target port not agvstation",
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                    }
                }
                else
                {
                    bool is_agv_station_source = excute_cmd.IsSourcePortAGVStation(scApp.PortStationBLL, scApp.EqptBLL);
                    if (is_agv_station_source)
                    {
                        var source_port_station = scApp.PortStationBLL.OperateCatch.getPortStation(excute_cmd.SOURCE_PORT);
                        var get_axis_result = source_port_station.getAxis(scApp.ReserveBLL);
                        if (get_axis_result.isSuccess)
                        {
                            double x_port_station = get_axis_result.x;
                            double y_port_station = get_axis_result.y;
                            double vh_port_distance = getDistance(vh.X_Axis, vh.Y_Axis, x_port_station, y_port_station);
                            if (vh_port_distance < SystemParameter.OpenAGVStationCoverDistance_mm)
                            {
                                vh.IsCloseToAGVStation = true;
                                var agv_station = excute_cmd.getSourcePortEQ(scApp.PortStationBLL, scApp.EqptBLL);
                                string notify_port_id = excute_cmd.SOURCE_PORT;
                                List<APORTSTATION> pre_open_port_cover_list = (agv_station as AGVStation).loadAutoAGVStationPorts();
                                int open_count = 0;
                                foreach (var port_sation in pre_open_port_cover_list)
                                {
                                    Task.Run(() => scApp.TransferBLL.web.preOpenAGVStationCover(agv_station as IAGVStationType, port_sation.PORT_ID));
                                    open_count++;
                                    if (open_count >= 2)
                                    {
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                   Data: $"Start check pre open cover scenario,vh id:{vh.VEHICLE_ID} current excute cmd:{excute_cmd_id} " +
                                         $"source port:{SCUtility.Trim(excute_cmd.SOURCE_PORT, true)} target port:{SCUtility.Trim(excute_cmd.DESTINATION_PORT, true)}," +
                                         $"dis:{vh_port_distance} not enogh, with source port",
                               VehicleID: vh.VEHICLE_ID,
                               CarrierID: vh.CST_ID);
                            }
                        }
                        else
                        {
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"Start check pre open cover scenario,vh id:{vh.VEHICLE_ID} current excute cmd:{excute_cmd_id} " +
                                     $"source port:{SCUtility.Trim(excute_cmd.SOURCE_PORT, true)} target port:{SCUtility.Trim(excute_cmd.DESTINATION_PORT, true)}," +
                                     $"source port adr(x,y) not exist",
                               VehicleID: vh.VEHICLE_ID,
                               CarrierID: vh.CST_ID);
                        }
                    }
                    else
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                           Data: $"Start check pre open cover scenario,vh id:{vh.VEHICLE_ID} current excute cmd:{excute_cmd_id} " +
                                 $"source port:{SCUtility.Trim(excute_cmd.SOURCE_PORT, true)} target port:{SCUtility.Trim(excute_cmd.DESTINATION_PORT, true)}," +
                                 $"source port not agvstation",
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
        }
        protected void Vh_PositionChange(object sender, PositionChangeEventArgs e)
        {
            try
            {
                AVEHICLE vh = sender as AVEHICLE;
                if (vh.IsCloseToAGVStation)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"Close to agv station is on",
                       VehicleID: vh.VEHICLE_ID,
                       CarrierID: vh.CST_ID);

                    return;
                }
                //先確認目前執行的命令，是否是要去AGV Station 進行Load/Unload
                //是的話則判斷是否已經進入到N公尺m內
                //如果是 則將通知OHBC將此AGV ST進行開蓋
                bool has_excute_cmd = !SCUtility.isEmpty(vh.CurrentExcuteCmdID);
                if (!has_excute_cmd)
                    return;
                string current_excute_cmd_id = vh.CurrentExcuteCmdID;
                ACMD current_excute_cmd = scApp.CMDBLL.cache.getExcuteCmd(current_excute_cmd_id);
                if (current_excute_cmd == null)
                    return;
                if (current_excute_cmd.CMD_TYPE == E_CMD_TYPE.LoadUnload || current_excute_cmd.CMD_TYPE == E_CMD_TYPE.Unload)
                {
                    //not thing...
                }
                else
                {
                    return;
                }
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"Start check pre open cover scenario,vh id:{vh.VEHICLE_ID} current excute cmd:{current_excute_cmd_id} " +
                         $"source port:{SCUtility.Trim(current_excute_cmd.SOURCE_PORT, true)} target port:{SCUtility.Trim(current_excute_cmd.DESTINATION_PORT, true)} ...,",
                       VehicleID: vh.VEHICLE_ID,
                       CarrierID: vh.CST_ID);

                checkWillGoToPortIsAGVStationAndIsNeedPreOpenCover(vh, current_excute_cmd);

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
        }
        private void checkWillGoToPortIsAGVStationAndIsNeedPreOpenCover(AVEHICLE vh, ACMD excute_cmd)
        {

            bool is_carry_cmd_cst = scApp.VehicleBLL.cache.IsCarryCstByCstID(vh.VEHICLE_ID, excute_cmd.CARRIER_ID);
            if (is_carry_cmd_cst)
            {
                bool is_agv_station_traget = excute_cmd.IsTargetPortAGVStation(scApp.PortStationBLL, scApp.EqptBLL);
                if (is_agv_station_traget)
                {
                    checkIsNeedPreOpenAGVStationCover(vh, excute_cmd.DESTINATION_PORT);
                }
                else
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"Start check pre open cover scenario,vh id:{vh.VEHICLE_ID} current excute cmd:{SCUtility.Trim(excute_cmd.ID, true)} " +
                             $"source port:{SCUtility.Trim(excute_cmd.SOURCE_PORT, true)} target port:{SCUtility.Trim(excute_cmd.DESTINATION_PORT, true)}," +
                             $"target port not agvstation",
                       VehicleID: vh.VEHICLE_ID,
                       CarrierID: vh.CST_ID);
                }
            }
            else
            {
                bool is_agv_station_source = excute_cmd.IsSourcePortAGVStation(scApp.PortStationBLL, scApp.EqptBLL);
                if (is_agv_station_source)
                {
                    checkIsNeedPreOpenAGVStationCover(vh, excute_cmd.SOURCE_PORT);
                }
                else
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"Start check pre open cover scenario,vh id:{vh.VEHICLE_ID} current excute cmd:{SCUtility.Trim(excute_cmd.ID, true)} " +
                             $"source port:{SCUtility.Trim(excute_cmd.SOURCE_PORT, true)} target port:{SCUtility.Trim(excute_cmd.DESTINATION_PORT, true)}," +
                             $"source port not agvstation",
                       VehicleID: vh.VEHICLE_ID,
                       CarrierID: vh.CST_ID);
                }
            }
        }

        //private void checkIsNeedPreOpenCover(AVEHICLE vh, ACMD excute_cmd)
        private void checkIsNeedPreOpenAGVStationCover(AVEHICLE vh, string checkPortStationID)
        {
            var port_station = scApp.PortStationBLL.OperateCatch.getPortStation(checkPortStationID);
            var get_axis_result = port_station.getAxis(scApp.ReserveBLL);
            if (get_axis_result.isSuccess)
            {
                double x_port_station = get_axis_result.x;
                double y_port_station = get_axis_result.y;
                double vh_port_distance = getDistance(vh.X_Axis, vh.Y_Axis, x_port_station, y_port_station);
                if (vh_port_distance < SystemParameter.OpenAGVStationCoverDistance_mm)
                {
                    vh.IsCloseToAGVStation = true;
                    //var agv_station = excute_cmd.getTragetPortEQ(scApp.PortStationBLL, scApp.EqptBLL);
                    var agv_station = port_station.GetEqpt(scApp.EqptBLL);
                    string notify_port_id = checkPortStationID;
                    Task.Run(() => scApp.TransferBLL.web.preOpenAGVStationCover(agv_station as IAGVStationType, notify_port_id));
                }
                else
                {
                    //todo log...
                }

            }
            else
            {
                //todo log...
            }
        }

        private double getDistance(double x1, double y1, double x2, double y2)
        {
            double dx, dy;
            dx = x2 - x1;
            dy = y2 - y1;
            return Math.Sqrt(dx * dx + dy * dy);
        }


        protected void Vh_LocationChange(object sender, LocationChangeEventArgs e)
        {
            AVEHICLE vh = sender as AVEHICLE;
            ASECTION leave_section = scApp.SectionBLL.cache.GetSection(e.LeaveSection);
            ASECTION entry_section = scApp.SectionBLL.cache.GetSection(e.EntrySection);
            entry_section?.Entry(vh.VEHICLE_ID);
            leave_section?.Leave(vh.VEHICLE_ID);
            if (leave_section != null)
            {
                scApp.ReserveBLL.RemoveManyReservedSectionsByVIDSID(vh.VEHICLE_ID, leave_section.SEC_ID);
                if (entry_section != null)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
   Data: $"vh:{vh.VEHICLE_ID} leave section {entry_section.SEC_ID},remove reserved.",
   VehicleID: vh.VEHICLE_ID);
                }

            }
            scApp.VehicleBLL.cache.removeAlreadyPassedSection(vh.VEHICLE_ID, e.LeaveSection);

            //如果在進入該Section後，還有在該Section之前的Section沒有清掉的，就把它全部釋放
            if (entry_section != null)
            {
                List<string> current_resreve_section = scApp.ReserveBLL.loadCurrentReserveSections(vh.VEHICLE_ID);
                int current_section_index_in_reserve_section = current_resreve_section.IndexOf(entry_section.SEC_ID);
                if (current_section_index_in_reserve_section > 0)//代表不是在第一個
                {
                    for (int i = 0; i < current_section_index_in_reserve_section; i++)
                    {
                        scApp.ReserveBLL.RemoveManyReservedSectionsByVIDSID(vh.VEHICLE_ID, current_resreve_section[i]);
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                           Data: $"vh:{vh.VEHICLE_ID} force release omission section {current_resreve_section[i]},remove reserved.",
                           VehicleID: vh.VEHICLE_ID);
                    }
                }
            }
        }
        protected void Vh_SegementChange(object sender, SegmentChangeEventArgs e)
        {
            AVEHICLE vh = sender as AVEHICLE;
            ASEGMENT leave_section = scApp.SegmentBLL.cache.GetSegment(e.LeaveSegment);
            ASEGMENT entry_section = scApp.SegmentBLL.cache.GetSegment(e.EntrySegment);
            //if (leave_section != null && entry_section != null)
            //{
            //    AADDRESS release_adr = FindReleaseAddress(leave_section, entry_section);
            //    release_adr?.Release(vh.VEHICLE_ID);
            //}
        }

        public bool stopVehicleTcpIpServer(string vhID)
        {
            AVEHICLE vh = scApp.VehicleBLL.cache.getVehicle(vhID);
            return stopVehicleTcpIpServer(vh);
        }
        private bool stopVehicleTcpIpServer(AVEHICLE vh)
        {
            if (!vh.IsTcpIpListening(scApp.getBCFApplication()))
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"vh:{vh.VEHICLE_ID} of tcp/ip server already stopped!,IsTcpIpListening:{vh.IsTcpIpListening(scApp.getBCFApplication())}",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                return false;
            }

            int port_num = vh.getPortNum(scApp.getBCFApplication());
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
               Data: $"Stop vh:{vh.VEHICLE_ID} of tcp/ip server, port num:{port_num}",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
            scApp.stopTcpIpServer(port_num);
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
               Data: $"Stop vh:{vh.VEHICLE_ID} of tcp/ip server finish, IsTcpIpListening:{vh.IsTcpIpListening(scApp.getBCFApplication())}",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
            return true;
        }

        //public bool startVehicleTcpIpServer(string vhID)
        //{
        //    AVEHICLE vh = scApp.VehicleBLL.cache.getVhByID(vhID);
        //    return startVehicleTcpIpServer(vh);
        //}
        public bool startVehicleTcpIpServer(string vhID)
        {
            AVEHICLE vh = scApp.VehicleBLL.cache.getVehicle(vhID);
            return startVehicleTcpIpServer(vh);
        }

        private bool startVehicleTcpIpServer(AVEHICLE vh)
        {
            if (vh.IsTcpIpListening(scApp.getBCFApplication()))
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"vh:{vh.VEHICLE_ID} of tcp/ip server already listening!,IsTcpIpListening:{vh.IsTcpIpListening(scApp.getBCFApplication())}",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                return false;
            }

            int port_num = vh.getPortNum(scApp.getBCFApplication());
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
               Data: $"Start vh:{vh.VEHICLE_ID} of tcp/ip server, port num:{port_num}",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
            scApp.startTcpIpServerListen(port_num);
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
               Data: $"Start vh:{vh.VEHICLE_ID} of tcp/ip server finish, IsTcpIpListening:{vh.IsTcpIpListening(scApp.getBCFApplication())}",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
            return true;
        }

        protected void PublishVhInfo(object sender, EventArgs e)
        {
            try
            {
                //string vh_id = e.PropertyValue as string;
                //AVEHICLE vh = scApp.VehicleBLL.getVehicleByID(vh_id);
                AVEHICLE vh = sender as AVEHICLE;
                if (sender == null) return;
                byte[] vh_Serialize = BLL.VehicleBLL.Convert2GPB_VehicleInfo(vh);
                RecoderVehicleObjInfoLog(vh.VEHICLE_ID, vh_Serialize);

                scApp.getNatsManager().PublishAsync
                    (string.Format(SCAppConstants.NATS_SUBJECT_VH_INFO_0, vh.VEHICLE_ID.Trim()), vh_Serialize);

                scApp.getRedisCacheManager().ListSetByIndexAsync
                    (SCAppConstants.REDIS_LIST_KEY_VEHICLES, vh.VEHICLE_ID, vh.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
        }
        protected void PublishVhInfo(object sender, string vhID)
        {
            try
            {
                AVEHICLE vh = scApp.VehicleBLL.cache.getVehicle(vhID);
                if (vh == null) return;
                byte[] vh_Serialize = BLL.VehicleBLL.Convert2GPB_VehicleInfo(vh);
                RecoderVehicleObjInfoLog(vhID, vh_Serialize);

                scApp.getNatsManager().PublishAsync
                    (string.Format(SCAppConstants.NATS_SUBJECT_VH_INFO_0, vh.VEHICLE_ID.Trim()), vh_Serialize);

                scApp.getRedisCacheManager().ListSetByIndexAsync
                    (SCAppConstants.REDIS_LIST_KEY_VEHICLES, vh.VEHICLE_ID, vh.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
        }
        private static void RecoderVehicleObjInfoLog(string vh_id, byte[] arrayByte)
        {
            string compressStr = SCUtility.CompressArrayByte(arrayByte);
            dynamic logEntry = new JObject();
            logEntry.RPT_TIME = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture);
            logEntry.OBJECT_ID = vh_id;
            logEntry.RAWDATA = compressStr;
            logEntry.Index = "ObjectHistoricalInfo";
            var json = logEntry.ToString(Newtonsoft.Json.Formatting.None);
            json = json.Replace("RPT_TIME", "@timestamp");
            LogManager.GetLogger("ObjectHistoricalInfo").Info(json);
        }

        protected void Vh_Idling(object sender, EventArgs e)
        {
            try
            {
                AVEHICLE vh = sender as AVEHICLE;
                if (vh == null) return;
                //bool has_cmd_excute = scApp.CMDBLL.cache.hasCmdExcute(vh.VEHICLE_ID);
                //if (!has_cmd_excute)
                //{
                //    scApp.VehicleChargerModule.askVhToChargerForWait(vh);
                //}

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
        }

        #endregion Vh Event Handler
        #region Send Message To Vehicle
        #region Data syne
        public bool HostBasicVersionReport(string vh_id)
        {
            bool isSuccess = false;
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            DateTime crtTime = DateTime.Now;
            ID_101_HOST_BASIC_INFO_VERSION_RESPONSE receive_gpp = null;
            ID_1_HOST_BASIC_INFO_VERSION_REP sned_gpp = new ID_1_HOST_BASIC_INFO_VERSION_REP()
            {
                DataDateTimeYear = "2018",
                DataDateTimeMonth = "10",
                DataDateTimeDay = "25",
                DataDateTimeHour = "15",
                DataDateTimeMinute = "22",
                DataDateTimeSecond = "50",
                CurrentTimeYear = crtTime.Year.ToString(),
                CurrentTimeMonth = crtTime.Month.ToString(),
                CurrentTimeDay = crtTime.Day.ToString(),
                CurrentTimeHour = crtTime.Hour.ToString(),
                CurrentTimeMinute = crtTime.Minute.ToString(),
                CurrentTimeSecond = crtTime.Second.ToString()
            };
            isSuccess = vh.send_Str1(sned_gpp, out receive_gpp);
            isSuccess = isSuccess && receive_gpp.ReplyCode == 0;
            return isSuccess;
        }
        //public bool CoplerInfosReport(string vhID)
        //{
        //    bool isSuccess = false;
        //    AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vhID);
        //    DateTime crtTime = DateTime.Now;
        //    ID_111_COUPLER_INFO_RESPONSE receive_gpp = null;
        //    ID_11_COUPLER_INFO_REP send_gpp = new ID_11_COUPLER_INFO_REP();
        //    var all_coupler = scApp.AddressesBLL.cache.GetCouplerAddresses();
        //    List<CouplerInfo> couplerInfos = new List<CouplerInfo>();
        //    foreach (var coupler in all_coupler)
        //    {
        //        string adr_id = coupler.ADR_ID;
        //        ProtocolFormat.OHTMessage.CouplerStatus couplerStatus = coupler.IsWork(scApp.UnitBLL) ?
        //                                                                ProtocolFormat.OHTMessage.CouplerStatus.Enable : ProtocolFormat.OHTMessage.CouplerStatus.Disable;
        //        couplerInfos.Add(new CouplerInfo()
        //        {
        //            AddressID = adr_id,
        //            CouplerStatus = couplerStatus
        //        });
        //    }
        //    send_gpp.CouplerInfos.AddRange(couplerInfos);
        //    isSuccess = vh.send_S11(send_gpp, out receive_gpp);
        //    isSuccess = isSuccess && receive_gpp.ReplyCode == 0;
        //    return isSuccess;
        //}
        public bool TavellingDataReport(string vh_id)
        {
            bool isSuccess = false;
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            DateTime crtTime = DateTime.Now;
            AVEHICLE_CONTROL_100 data = scApp.DataSyncBLL.getReleaseVehicleControlData_100(vh_id);

            ID_113_TAVELLING_DATA_RESPONSE receive_gpp = null;
            ID_13_TAVELLING_DATA_REP sned_gpp = new ID_13_TAVELLING_DATA_REP()
            {
                Resolution = (UInt32)data.TRAVEL_RESOLUTION,
                StartStopSpd = (UInt32)data.TRAVEL_START_STOP_SPEED,
                MaxSpeed = (UInt32)data.TRAVEL_MAX_SPD,
                AccelTime = (UInt32)data.TRAVEL_ACCEL_DECCEL_TIME,
                SCurveRate = (UInt16)data.TRAVEL_S_CURVE_RATE,
                OriginDir = (UInt16)data.TRAVEL_HOME_DIR,
                OriginSpd = (UInt32)data.TRAVEL_HOME_SPD,
                BeaemSpd = (UInt32)data.TRAVEL_KEEP_DIS_SPD,
                ManualHSpd = (UInt32)data.TRAVEL_MANUAL_HIGH_SPD,
                ManualLSpd = (UInt32)data.TRAVEL_MANUAL_LOW_SPD,
                TeachingSpd = (UInt32)data.TRAVEL_TEACHING_SPD,
                RotateDir = (UInt16)data.TRAVEL_TRAVEL_DIR,
                EncoderPole = (UInt16)data.TRAVEL_ENCODER_POLARITY,
                PositionCompensation = 0, //TODO 要填入正確的資料
                //FLimit = (UInt16)data.TRAVEL_F_DIR_LIMIT, //TODO 要填入正確的資料
                //RLimit = (UInt16)data.TRAVEL_R_DIR_LIMIT,
                KeepDistFar = (UInt32)data.TRAVEL_OBS_DETECT_LONG,
                KeepDistNear = (UInt32)data.TRAVEL_OBS_DETECT_SHORT,
            };
            isSuccess = vh.send_S13(sned_gpp, out receive_gpp);
            isSuccess = isSuccess && receive_gpp.ReplyCode == 0;
            return isSuccess;
        }
        public bool AddressDataReport(string vh_id)
        {
            bool isSuccess = false;

            return isSuccess;
        }
        public bool ScaleDataReport(string vh_id)
        {
            bool isSuccess = false;
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            SCALE_BASE_DATA data = scApp.DataSyncBLL.getReleaseSCALE_BASE_DATA();

            ID_119_SCALE_DATA_RESPONSE receive_gpp = null;
            ID_19_SCALE_DATA_REP sned_gpp = new ID_19_SCALE_DATA_REP()
            {
                Resolution = (UInt32)data.RESOLUTION,
                InposArea = (UInt32)data.INPOSITION_AREA,
                InposStability = (UInt32)data.INPOSITION_STABLE_TIME,
                ScalePulse = (UInt32)data.TOTAL_SCALE_PULSE,
                ScaleOffset = (UInt32)data.SCALE_OFFSET,
                ScaleReset = (UInt32)data.SCALE_RESE_DIST,
                ReadDir = (UInt16)data.READ_DIR

            };
            isSuccess = vh.send_S19(sned_gpp, out receive_gpp);
            isSuccess = isSuccess && receive_gpp.ReplyCode == 0;
            return isSuccess;
        }
        public bool ControlDataReport(string vh_id)
        {
            bool isSuccess = false;
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);

            CONTROL_DATA data = scApp.DataSyncBLL.getReleaseCONTROL_DATA();
            string rtnMsg = string.Empty;
            ID_121_CONTROL_DATA_RESPONSE receive_gpp;
            ID_21_CONTROL_DATA_REP sned_gpp = new ID_21_CONTROL_DATA_REP()
            {
                TimeoutT1 = (UInt32)data.T1,
                TimeoutT2 = (UInt32)data.T2,
                TimeoutT3 = (UInt32)data.T3,
                TimeoutT4 = (UInt32)data.T4,
                TimeoutT5 = (UInt32)data.T5,
                TimeoutT6 = (UInt32)data.T6,
                TimeoutT7 = (UInt32)data.T7,
                TimeoutT8 = (UInt32)data.T8,
                TimeoutBlock = (UInt32)data.BLOCK_REQ_TIME_OUT
            };
            isSuccess = vh.send_S21(sned_gpp, out receive_gpp);
            isSuccess = isSuccess && receive_gpp.ReplyCode == 0;
            return isSuccess;
        }
        public bool GuideDataReport(string vh_id)
        {
            bool isSuccess = false;
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            AVEHICLE_CONTROL_100 data = scApp.DataSyncBLL.getReleaseVehicleControlData_100(vh_id);
            ID_123_GUIDE_DATA_RESPONSE receive_gpp;
            ID_23_GUIDE_DATA_REP sned_gpp = new ID_23_GUIDE_DATA_REP()
            {
                StartStopSpd = (UInt32)data.GUIDE_START_STOP_SPEED,
                MaxSpeed = (UInt32)data.GUIDE_MAX_SPD,
                AccelTime = (UInt32)data.GUIDE_ACCEL_DECCEL_TIME,
                SCurveRate = (UInt16)data.GUIDE_S_CURVE_RATE,
                NormalSpd = (UInt32)data.GUIDE_RUN_SPD,
                ManualHSpd = (UInt32)data.GUIDE_MANUAL_HIGH_SPD,
                ManualLSpd = (UInt32)data.GUIDE_MANUAL_LOW_SPD,
                LFLockPos = (UInt32)data.GUIDE_LF_LOCK_POSITION,
                LBLockPos = (UInt32)data.GUIDE_LB_LOCK_POSITION,
                RFLockPos = (UInt32)data.GUIDE_RF_LOCK_POSITION,
                RBLockPos = (UInt32)data.GUIDE_RB_LOCK_POSITION,
                ChangeStabilityTime = (UInt32)data.GUIDE_CHG_STABLE_TIME,
            };
            isSuccess = vh.send_S23(sned_gpp, out receive_gpp);
            isSuccess = isSuccess && receive_gpp.ReplyCode == 0;
            return isSuccess;
        }
        public bool doDataSyscAllVh()
        {
            bool isSyscCmp = true;
            try
            {
                var vhs = scApp.VehicleBLL.cache.loadAllVh();
                foreach (AVEHICLE vh in vhs)
                {
                    if (!vh.isTcpIpConnect)
                        continue;

                    string syc_vh_id = vh.VEHICLE_ID;
                    Task.Run(() =>
                    {
                        try
                        {
                            //CoplerInfosReport(syc_vh_id);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, "Exception");
                            isSyscCmp = false;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                isSyscCmp = false;
            }
            return isSyscCmp;
        }
        public bool doDataSysc(string vh_id)
        {
            bool isSyscCmp = false;
            //if (CoplerInfosReport(vh_id))
            //{
            //    isSyscCmp = true;
            //}
            return isSyscCmp;
        }
        public bool IndividualUploadRequest(string vh_id)
        {
            bool isSuccess = false;
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            ID_161_INDIVIDUAL_UPLOAD_RESPONSE receive_gpp;
            ID_61_INDIVIDUAL_UPLOAD_REQ sned_gpp = new ID_61_INDIVIDUAL_UPLOAD_REQ()
            {

            };
            isSuccess = vh.send_S61(sned_gpp, out receive_gpp);
            //TODO Set info 2 DB
            if (isSuccess)
            {

            }
            return isSuccess;
        }
        public bool IndividualChangeRequest(string vh_id)
        {
            bool isSuccess = false;
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            ID_163_INDIVIDUAL_CHANGE_RESPONSE receive_gpp;
            ID_63_INDIVIDUAL_CHANGE_REQ sned_gpp = new ID_63_INDIVIDUAL_CHANGE_REQ()
            {
                OffsetGuideFL = 1,
                OffsetGuideRL = 2,
                OffsetGuideFR = 3,
                OffsetGuideRR = 4
            };
            isSuccess = vh.send_S63(sned_gpp, out receive_gpp);
            return isSuccess;
        }
        #endregion Data syne
        private (bool isSuccess, int total_code,
            List<string> guide_start_to_from_segment_ids, List<string> guide_start_to_from_section_ids, List<string> guide_start_to_from_address_ids,
            List<string> guide_to_dest_segment_ids, List<string> guide_to_dest_section_ids, List<string> guide_to_dest_address_ids)
            FindGuideInfo(string vh_current_address, string source_adr, string dest_adr, ActiveType active_type, bool has_carray = false, List<string> byPassSectionIDs = null)
        {
            bool isSuccess = false;
            List<string> guide_start_to_from_segment_ids = null;
            List<string> guide_start_to_from_section_ids = null;
            List<string> guide_start_to_from_address_ids = null;
            List<string> guide_to_dest_segment_ids = null;
            List<string> guide_to_dest_section_ids = null;
            List<string> guide_to_dest_address_ids = null;
            int total_cost = 0;
            //1.取得行走路徑的詳細資料
            switch (active_type)
            {
                case ActiveType.Loadunload:
                    if (!SCUtility.isMatche(vh_current_address, source_adr))
                    {
                        (isSuccess, guide_start_to_from_segment_ids, guide_start_to_from_section_ids, guide_start_to_from_address_ids, total_cost)
                            = scApp.GuideBLL.getGuideInfo(vh_current_address, source_adr, byPassSectionIDs);
                    }
                    else
                    {
                        isSuccess = true;//如果相同 代表是在同一個點上
                    }
                    if (isSuccess && !SCUtility.isMatche(source_adr, dest_adr))
                    {
                        (isSuccess, guide_to_dest_segment_ids, guide_to_dest_section_ids, guide_to_dest_address_ids, total_cost)
                            = scApp.GuideBLL.getGuideInfo(source_adr, dest_adr, null);
                    }
                    break;
                case ActiveType.Load:
                    if (!SCUtility.isMatche(vh_current_address, source_adr))
                    {
                        (isSuccess, guide_start_to_from_segment_ids, guide_start_to_from_section_ids, guide_start_to_from_address_ids, total_cost)
                            = scApp.GuideBLL.getGuideInfo(vh_current_address, source_adr, byPassSectionIDs);
                    }
                    else
                    {
                        isSuccess = true; //如果相同 代表是在同一個點上
                    }
                    break;
                case ActiveType.Unload:
                    if (!SCUtility.isMatche(vh_current_address, dest_adr))
                    {
                        (isSuccess, guide_to_dest_segment_ids, guide_to_dest_section_ids, guide_to_dest_address_ids, total_cost)
                            = scApp.GuideBLL.getGuideInfo(vh_current_address, dest_adr, byPassSectionIDs);
                    }
                    else
                    {
                        isSuccess = true;//如果相同 代表是在同一個點上
                    }
                    break;
                case ActiveType.Move:
                    if (!SCUtility.isMatche(vh_current_address, dest_adr))
                    {
                        (isSuccess, guide_to_dest_segment_ids, guide_to_dest_section_ids, guide_to_dest_address_ids, total_cost)
                            = scApp.GuideBLL.getGuideInfo(vh_current_address, dest_adr, byPassSectionIDs);
                    }
                    else
                    {
                        isSuccess = false;
                    }
                    break;
            }
            return (isSuccess, total_cost,
                    guide_start_to_from_segment_ids, guide_start_to_from_section_ids, guide_start_to_from_address_ids,
                    guide_to_dest_segment_ids, guide_to_dest_section_ids, guide_to_dest_address_ids);
        }


        #endregion Send Message To Vehicle
        #region Vh connection / disconnention
        [ClassAOPAspect]
        public void Connection(BCFApplication bcfApp, AVEHICLE vh)
        {
            lock (vh.connection_sync)
            {
                MaintainLift maintainLift = scApp.EqptBLL.OperateCatch.GetMaintainLift();
                bool isCarIn = false;
                if (maintainLift != null)
                {
                    if (SCUtility.isMatche(maintainLift.CurrentCarID, vh.Num.ToString()))
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: "Vehicle Resume Connection At MTL, Do update to AutoMTL!",
                       VehicleID: vh.VEHICLE_ID,
                       CarrierID: vh.CST_ID);
                        isCarIn = true;
                    }
                }
                if (!isCarIn)
                {
                    //20210602 因為硬體有問題的OHT一上線狀態如果是AutoRemote，可能馬上被派發命令，會造成問題。所以上線的車都先設為AutoLocal
                    //scApp.VehicleBLL.cache.updataVehicleMode(vh.VEHICLE_ID, VHModeStatus.AutoLocal);
                    //20210629 因為斷線重連會切為AutoLocal導致搬送的車輛減少，預設改為AutoRemote。
                    //scApp.VehicleBLL.cache.updataVehicleMode(vh.VEHICLE_ID, VHModeStatus.AutoRemote);
                    //20210701 預設為AutoRemote，但可透過Debug Form進行預設。
                    if (DebugParameter.isDefaultOHTModeAutoRemote)
                    {
                        scApp.VehicleBLL.cache.updataVehicleMode(vh.VEHICLE_ID, VHModeStatus.AutoRemote);
                    }
                    else
                    {
                        scApp.VehicleBLL.cache.updataVehicleMode(vh.VEHICLE_ID, VHModeStatus.AutoLocal);
                    }
                }
                else
                {
                    scApp.VehicleBLL.cache.updataVehicleMode(vh.VEHICLE_ID, VHModeStatus.AutoMtl);
                }
                vh.isTcpIpConnect = true;
                vh.startVehicleTimer();

                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: "Connection ! Begin synchronize with vehicle...",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                VehicleInfoSynchronize(vh.VEHICLE_ID);
                doDataSysc(vh.VEHICLE_ID);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: "Connection ! End synchronize with vehicle.",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                //scApp.LineService.ProcessAlarmReport
                //    (vh, AlarmBLL.VEHICLE_CAN_NOT_SERVICE, ErrorStatus.ErrReset, $"vehicle cannot service");
                scApp.LineService.ProcessAlarmReport(vh, AlarmBLL.OHTC_VEHICLE_DISCONNECTED, ErrorStatus.ErrReset, $"Vehicle has been disconnected.");
                SCUtility.RecodeConnectionInfo
                    (vh.VEHICLE_ID,
                    SCAppConstants.RecodeConnectionInfo_Type.Connection.ToString(),
                    vh.getDisconnectionIntervalTime(bcfApp));
            }
        }
        /// <summary>
        /// 與Vehicle進行資料同步。(通常使用剛與Vehicle連線時)
        /// </summary>
        /// <param name="vh_id"></param>
        private void VehicleInfoSynchronize(string vh_id)
        {
            /*與Vehicle進行狀態同步*/
            Send.StatusRequest(vh_id, true);
            /*要求Vehicle進行Alarm的Reset*/
            Send.AlarmReset(vh_id);
        }
        [ClassAOPAspect]
        public void Disconnection(BCFApplication bcfApp, AVEHICLE vh)
        {
            lock (vh.connection_sync)
            {
                vh.isTcpIpConnect = false;
                vh.ToSectionID = string.Empty;

                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: "Disconnection !",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                scApp.LineService.ProcessAlarmReport(vh, AlarmBLL.OHTC_VEHICLE_DISCONNECTED, ErrorStatus.ErrSet, $"Vehicle has been disconnected.");
                //MaintainLift maintainLift = scApp.EqptBLL.OperateCatch.GetMaintainLiftByMTLAdr(vh.CUR_ADR_ID);
                MaintainLift maintainLift = scApp.EqptBLL.OperateCatch.GetMaintainLift();
                if (maintainLift != null)//離線時在MTL上
                {
                    if (SCUtility.isMatche(maintainLift.CurrentCarID, vh.Num.ToString()))
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: "Vehicle Disconnection At MTL,Begin Remove!",
                       VehicleID: vh.VEHICLE_ID,
                       CarrierID: vh.CST_ID);
                        Remove(vh.VEHICLE_ID);
                    }
                }

                SCUtility.RecodeConnectionInfo
                    (vh.VEHICLE_ID,
                    SCAppConstants.RecodeConnectionInfo_Type.Disconnection.ToString(),
                    vh.getConnectionIntervalTime(bcfApp));
            }
            Task.Run(() => scApp.VehicleBLL.web.vehicleDisconnection());
        }
        #endregion Vh Connection / disconnention
        #region Vehicle Install/Remove
        public (bool isSuccess, string result) Install(string vhID)
        {
            try
            {
                AVEHICLE vh_vo = scApp.VehicleBLL.cache.getVehicle(vhID);
                if (!vh_vo.isTcpIpConnect)
                {
                    string message = $"vh:{vhID} current not connection, can't excute action:Install";
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: message,
                       VehicleID: vhID);
                    return (false, message);
                }
                ASECTION current_section = scApp.SectionBLL.cache.GetSection(vh_vo.CUR_SEC_ID);
                if (current_section == null)
                {
                    string message = $"vh:{vhID} current section:{SCUtility.Trim(vh_vo.CUR_SEC_ID, true)} is not exist, can't excute action:Install";
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: message,
                       VehicleID: vhID);
                    return (false, message);
                }

                var ReserveResult = scApp.ReserveBLL.askReserveSuccess(scApp.SectionBLL, vhID, vh_vo.CUR_SEC_ID, vh_vo.CUR_ADR_ID);
                if (!ReserveResult.isSuccess)
                {
                    string message = $"vh:{vhID} current section:{SCUtility.Trim(vh_vo.CUR_SEC_ID, true)} can't reserved," +
                                     $" can't excute action:Install";
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: message,
                       VehicleID: vhID);
                    return (false, message);
                }

                scApp.VehicleBLL.updataVehicleInstall(vhID);
                if (vh_vo.MODE_STATUS == VHModeStatus.Manual)
                {
                    scApp.LineService.ProcessAlarmReport(vh_vo, AlarmBLL.OHTC_VEHICLE_MANUAL, ErrorStatus.ErrSet, $"Vehicle switch to manual mode.");
                }
                List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
                scApp.ReportBLL.newReportVehicleInstalled(vh_vo.Real_ID, reportqueues);
                scApp.ReportBLL.newSendMCSMessage(reportqueues);
                return (true, "");
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   VehicleID: vhID);
                return (false, "");
            }
        }
        public (bool isSuccess, string result) Remove(string vhID)
        {
            try
            {
                //1.確認該VH 是否可以進行Remove
                //  a.是否為斷線狀態
                //2.將該台VH 更新成Remove狀態
                //3.將位置的資訊清空。(包含Reserve的路段、紅綠燈、Block)
                //4.上報給MCS
                AVEHICLE vh_vo = scApp.VehicleBLL.cache.getVehicle(vhID);

                //測試期間，暫時不看是否已經連線中
                //因為會讓車子在連線狀態下跑CycleRun
                //此時車子會是連線狀態但要把它Remove
                if (vh_vo.isTcpIpConnect)
                {
                    string message = $"vh:{vhID} current is connection, can't excute action:remove";
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: message,
                       VehicleID: vhID);
                    return (false, message);
                }
                scApp.VehicleBLL.updataVehicleRemove(vhID);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"vh id:{vhID} remove success. start release reserved control...",
                   VehicleID: vhID);
                scApp.VehicleBLL.clearAndPublishPositionReportInfo2Redis(vhID);//20210713 移除車輛所在位置

                scApp.ReserveBLL.RemoveAllReservedSectionsByVehicleID(vh_vo.VEHICLE_ID);
                scApp.ReserveBLL.RemoveVehicle(vh_vo.VEHICLE_ID);

                //scApp.LineService.ProcessAlarmReport(vh_vo, AlarmBLL.VEHICLE_CAN_NOT_SERVICE, ErrorStatus.ErrReset, $"vehicle cannot service");
                scApp.LineService.ProcessAlarmReport(vh_vo, AlarmBLL.OHTC_VEHICLE_DISCONNECTED, ErrorStatus.ErrReset, $"Vehicle has been disconnected.");
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"vh id:{vhID} remove success. end release reserved control.",
                   VehicleID: vhID);
                List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
                scApp.ReportBLL.newReportVehicleRemoved(vh_vo.Real_ID, reportqueues);
                scApp.ReportBLL.newSendMCSMessage(reportqueues);
                //20210825 VHModeStatus設成None
                scApp.VehicleBLL.cache.updataVehicleMode(vhID, default(VHModeStatus));
                return (true, "");
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   VehicleID: vhID);
                return (false, "");
            }
        }

        public (bool isSuccess, string result) Remove(string vhID, bool isForce)//MTL CarOut流程,需要將還在線上的車子強制Remove
        {
            try
            {
                //1.確認該VH 是否可以進行Remove
                //  a.是否為斷線狀態
                //2.將該台VH 更新成Remove狀態
                //3.將位置的資訊清空。(包含Reserve的路段、紅綠燈、Block)
                //4.上報給MCS
                AVEHICLE vh_vo = scApp.VehicleBLL.cache.getVehicle(vhID);

                //測試期間，暫時不看是否已經連線中
                //因為會讓車子在連線狀態下跑CycleRun
                //此時車子會是連線狀態但要把它Remove
                if (vh_vo.isTcpIpConnect && !isForce)
                {
                    string message = $"vh:{vhID} current is connection, can't excute action:remove";
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: message,
                       VehicleID: vhID);
                    return (false, message);
                }
                scApp.VehicleBLL.updataVehicleRemove(vhID);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"vh id:{vhID} remove success. start release reserved control...",
                   VehicleID: vhID);
                scApp.VehicleBLL.clearAndPublishPositionReportInfo2Redis(vhID);//20210713 移除車輛所在位置
                scApp.ReserveBLL.RemoveAllReservedSectionsByVehicleID(vh_vo.VEHICLE_ID);
                scApp.ReserveBLL.RemoveVehicle(vh_vo.VEHICLE_ID);

                //scApp.LineService.ProcessAlarmReport(vh_vo, AlarmBLL.VEHICLE_CAN_NOT_SERVICE, ErrorStatus.ErrReset, $"vehicle cannot service");
                scApp.LineService.ProcessAlarmReport(vh_vo, AlarmBLL.OHTC_VEHICLE_DISCONNECTED, ErrorStatus.ErrReset, $"Vehicle has been disconnected.");
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"vh id:{vhID} remove success. end release reserved control.",
                   VehicleID: vhID);
                List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
                scApp.ReportBLL.newReportVehicleRemoved(vh_vo.Real_ID, reportqueues);
                scApp.ReportBLL.newSendMCSMessage(reportqueues);
                return (true, "");
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   VehicleID: vhID);
                return (false, "");
            }
        }
        #endregion Vehicle Install/Remove

        #region MTL Handle
        public bool doReservationVhToModeChange(string vhID)
        {
            bool isSuccess = true;
            using (TransactionScope tx = SCUtility.getTransactionScope())
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    isSuccess = isSuccess && VehicleAutoModeCahnge(vhID, VHModeStatus.AutoMtl);
                    if (isSuccess)
                    {
                        tx.Complete();
                    }
                }
            }
            return isSuccess;
        }
        public bool doReservationVhToMaintainsSpace(string vhID)
        {
            bool isSuccess = true;
            using (TransactionScope tx = SCUtility.getTransactionScope())
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    isSuccess = isSuccess && VehicleAutoModeCahnge(vhID, VHModeStatus.AutoMts);
                    if (isSuccess)
                    {
                        tx.Complete();
                    }
                }
            }
            return isSuccess;
        }
        public bool doAskVhToSystemOutAddress(string vhID, string carOutBufferAdr)
        {
            bool isSuccess = true;
            //isSuccess = scApp.CMDBLL.doCreatTransferCommand(vh_id: vhID, cmd_type: E_CMD_TYPE.SystemOut, destination: carOutBufferAdr);
            isSuccess = scApp.CMDBLL.doCreatCommand(vhID, cmd_type: E_CMD_TYPE.Move_MTL, destination: carOutBufferAdr);
            return isSuccess;
        }

        public bool doAskVhToMaintainsAddress(string vhID, string mtlAdtID)
        {

            bool is_success = false;

            is_success = scApp.CMDBLL.doCreatCommand(vhID, cmd_type: E_CMD_TYPE.SystemOut, destination: mtlAdtID);

            return is_success;
        }
        public bool doAskVhToCarInBufferAddress(string vhID, string carInBufferAdr)
        {
            bool isSuccess = true;
            isSuccess = scApp.CMDBLL.doCreatCommand(vhID, cmd_type: E_CMD_TYPE.MTLHome, destination: carInBufferAdr);
            //isSuccess = scApp.CMDBLL.doCreatTransferCommand(vh_id: vhID, cmd_type: E_CMD_TYPE.MTLHome, destination: carInBufferAdr);
            return isSuccess;
        }

        public bool doAskVhToSystemInAddress(string vhID, string systemInAdr)
        {
            bool isSuccess = true;
            isSuccess = scApp.CMDBLL.doCreatCommand(vhID, cmd_type: E_CMD_TYPE.SystemIn, destination: systemInAdr);
            //isSuccess = scApp.CMDBLL.doCreatTransferCommand(vh_id: vhID, cmd_type: E_CMD_TYPE.SystemIn, destination: systemInAdr);
            return isSuccess;
        }

        public bool doRecoverModeStatusToAutoRemote(string vh_id)
        {
            return VehicleAutoModeCahnge(vh_id, VHModeStatus.AutoRemote);
        }


        #endregion MTL Handle




        object pauseCheckLock = new object();
        #region VehiclePauseByVehicleStatus



        bool vhpauseSet = false;
        private long vhsyncPoint = 0;
        public void checkThenSetVehiclePauseByVehicleStatus(string caller) //20210706 AT&S客戶要求，當已Install的OHT斷線，需要暫停餘下的OHT
        {
    //        VehiclePauserHandlerInfoLogger.Info($"Entry checkThenSetVehiclePauseByVehicleStatus method, caller:[{caller}], before into lock. Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //        if (System.Threading.Interlocked.Exchange(ref vhsyncPoint, 1) == 0)
    //        {
    //            lock (pauseCheckLock)
    //            {
    //                VehiclePauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByVehicleStatus method, caller:[{caller}], Just In lock. Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                try
    //                {
    //                    List<AVEHICLE> vhs = scApp.getEQObjCacheManager().getAllVehicle();
    //                    StringBuilder sb = new StringBuilder();
    //                    sb.AppendLine("");
    //                    bool need_pause = false;
    //                    foreach (AVEHICLE vh in vhs)
    //                    {
    //                        VehiclePauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByVehicleStatus method, caller:[{caller}], Before Check." +
    //                         $" Vehicle ID:[{vh.VEHICLE_ID}] TCP Connection:[{vh.isTcpIpConnect}] Installed:[{vh.IS_INSTALLED}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                        if (vh.isTcpIpConnect == false && vh.IS_INSTALLED)
    //                        {
    //                            sb.AppendLine($"Installed Vehicle:[{vh.VEHICLE_ID}] is disconneted.");
    //                            need_pause = true;
    //                        }

    //                        VehiclePauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByVehicleStatus method, caller:[{caller}], after Check." +
    //                        $" Vehicle ID:[{vh.VEHICLE_ID}] TCP Connection:[{vh.isTcpIpConnect}] Installed:[{vh.IS_INSTALLED}] needPause:[{need_pause}] " +
    //                        $"ForcePass:[{DebugParameter.isForceBypassVehclePauseCheck}] pauseSet:[{vhpauseSet}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                    }



    //                    if (DebugParameter.isForceBypassVehclePauseCheck)
    //                    {
    //                        if (vhpauseSet)
    //                        {
    //                            vhpauseSet = false;
    //                            bcf.App.BCFApplication.onInfoMsg("Vehicle Connection Status is OK now, resume all vehicle.");
    //                            VehiclePauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByVehicleStatus method, caller:[{caller}], In Force Pass before resume OHT." +
    //                            $" needPause:[{need_pause}] " +
    //                            $"ForcePass:[{DebugParameter.isForceBypassVehclePauseCheck}] pauseSet:[{vhpauseSet}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                            //Task.Run(() => scApp.VehicleService.ResumeAllVehicleBySafetyPause());

    //                            //因為Vehicle Connection Status跟MTL Status都會Resume，必須兩邊都沒有要Pause才能Resume。
    //                            if (vhpauseSet == false && pauseSet == false)
    //                            {
    //                                ResumeAllVehicleBySafetyPause();
    //                            }

    //                            VehiclePauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByVehicleStatus method, caller:[{caller}], In Force Pass after resume OHT." +
    //                            $" needPause:[{need_pause}] " +
    //                            $"ForcePass:[{DebugParameter.isForceBypassVehclePauseCheck}] pauseSet:[{vhpauseSet}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                        }
    //                        VehiclePauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByVehicleStatus method, caller:[{caller}], In Force Pass ready to leave." +
    //                        $" needPause:[{need_pause}] " +
    //                        $"ForcePass:[{DebugParameter.isForceBypassVehclePauseCheck}] pauseSet:[{vhpauseSet}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                    }
    //                    else
    //                    {
    //                        if (vhpauseSet)
    //                        {
    //                            if (need_pause)
    //                            {
    //                                PauseAllVehicleBySafetyPause();
    //                                VehiclePauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByVehicleStatus method, caller:[{caller}], In Normal process do pause." +
    //                                $" needPause:[{need_pause}] " +
    //                                $"ForcePass:[{DebugParameter.isForceBypassVehclePauseCheck}] pauseSet:[{vhpauseSet}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                            }
    //                            else
    //                            {
    //                                vhpauseSet = false;
    //                                bcf.App.BCFApplication.onInfoMsg("Vehicle Connection Status is OK now, resume all vehicle.");
    //                                VehiclePauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByVehicleStatus method, caller:[{caller}], In Normal process before resume OHT." +
    //                                $" needPause:[{need_pause}] " +
    //                                $"ForcePass:[{DebugParameter.isForceBypassVehclePauseCheck}] pauseSet:[{vhpauseSet}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                                //因為Vehicle Connection Status跟MTL Status都會Resume，必須兩邊都沒有要Pause才能Resume。
    //                                if (vhpauseSet == false && pauseSet == false)
    //                                {
    //                                    ResumeAllVehicleBySafetyPause();
    //                                }
    //                                VehiclePauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByVehicleStatus method, caller:[{caller}], In Normal process after resume OHT." +
    //    $" needPause:[{need_pause}] " +
    //    $"ForcePass:[{DebugParameter.isForceBypassVehclePauseCheck}] pauseSet:[{vhpauseSet}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                            }
    //                        }
    //                        else
    //                        {
    //                            if (need_pause)
    //                            {
    //                                vhpauseSet = true;
    //                                bcf.App.BCFApplication.onErrorMsg("Vehicle Connection Status is wrong, pause all vehicle!!!" + sb);
    //                                VehiclePauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByVehicleStatus method, caller:[{caller}], In Normal process before Stop OHT." +
    //    $" needPause:[{need_pause}] " +
    //$"ForcePass:[{DebugParameter.isForceBypassVehclePauseCheck}] pauseSet:[{vhpauseSet}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                                //Task.Run(() => scApp.VehicleService.PauseAllVehicleBySafetyPause());
    //                                PauseAllVehicleBySafetyPause();
    //                                VehiclePauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByVehicleStatus method, caller:[{caller}], In Normal process after Stop OHT." +
    //       $" needPause:[{need_pause}] " +
    //    $"ForcePass:[{DebugParameter.isForceBypassVehclePauseCheck}] pauseSet:[{vhpauseSet}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                            }
    //                            else
    //                            {
    //                                //因為Vehicle Connection Status跟MTL Status都會Resume，必須兩邊都沒有要Pause才能Resume。
    //                                if (vhpauseSet == false && pauseSet == false)
    //                                {
    //                                    ResumeAllVehicleBySafetyPause();
    //                                }
    //                                VehiclePauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByVehicleStatus method, caller:[{caller}], In Normal process do continue." +
    //       $" needPause:[{need_pause}] " +
    //    $"ForcePass:[{DebugParameter.isForceBypassVehclePauseCheck}] pauseSet:[{vhpauseSet}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                                //pauseSet = false;
    //                                //scApp.VehicleService.ResumeAllVehicleBySafetyPause();
    //                            }
    //                        }
    //                    }


    //                }
    //                catch (Exception ex)
    //                {
    //                    logger.Error(ex, "Exception");
    //                    VehiclePauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByVehicleStatus method, caller:[{caller}],Exception happan. Exception:[{ex.Message}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                }
    //                finally
    //                {
    //                    VehiclePauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByVehicleStatus method, caller:[{caller}], before leave lock." +
    //    $"ForcePass:[{DebugParameter.isForceBypassVehclePauseCheck}] pauseSet:[{vhpauseSet}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                    System.Threading.Interlocked.Exchange(ref vhsyncPoint, 0);
    //                }

    //                //}
    //            }
    //        }
        }

        #endregion VehiclePauseByVehicleStatus

        #region VehiclePauseByMTLStatus
        bool pauseSet = false;
        private long syncPoint = 0;
        public void checkThenSetVehiclePauseByMTLStatus(string caller)
        {
    //        MTLPauserHandlerInfoLogger.Info($"Entry checkThenSetVehiclePauseByMTLStatus method, caller:[{caller}], before into lock. Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //        if (System.Threading.Interlocked.Exchange(ref syncPoint, 1) == 0)
    //        {
    //            lock (pauseCheckLock)
    //            {


    //                //lock (pauseByMTLStatusObj)
    //                //{
    //                MaintainLift MTL = scApp.EqptBLL.OperateCatch.GetMaintainLift();
    //                MTLPauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByMTLStatus method, caller:[{caller}], Just In lock. Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                try
    //                {
    //                    MTLPauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByMTLStatus method, caller:[{caller}], Before Check." +
    //                        $" Alive:[{MTL.Is_Eq_Alive}] Mode:[{MTL.MTxMode}] Location:[{MTL.MTLLocation}] Error:[{MTL.StopSignal}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                    StringBuilder sb = new StringBuilder();
    //                    bool need_pause = false;
    //                    sb.AppendLine("");
    //                    if (MTL.Is_Eq_Alive == false)
    //                    {
    //                        sb.AppendLine($"MTL Alive Index did not change for 10 seconds more. Last Change Time:[{MTL.Eq_Alive_Last_Change_time}]");
    //                        need_pause = true;
    //                    }
    //                    if (MTL.MTLLocation != ProtocolFormat.OHTMessage.MTLLocation.Bottorm)
    //                    {
    //                        sb.AppendLine($"MTL Location is not Bottom.Current Location:[{MTL.MTLLocation}]");
    //                        need_pause = true;
    //                    }

    //                    if (MTL.MTLRailStatus != ProtocolFormat.OHTMessage.MTLRailStatus.Closed)//20210610
    //                    {
    //                        sb.AppendLine($"MTL RailStatus is not Closed.Current RailStatus:[{MTL.MTLRailStatus}]");
    //                        need_pause = true;
    //                    }
    //                    //if (MTL.MTxMode != ProtocolFormat.OHTMessage.MTxMode.Auto)// 20210602 客戶要求不要看Manual訊號決定是否暫停全線
    //                    //{
    //                    //    sb.AppendLine($"MTL Mode is not Auto.Current Mode:[{MTL.MTxMode}]");
    //                    //    need_pause = true;
    //                    //}
    //                    if (MTL.StopSignal)
    //                    {
    //                        sb.AppendLine($"MTL Error ocurred.");
    //                        need_pause = true;
    //                    }
    //                    //2021.9.7: Hsinyu Chang Add Safety Signal here
    //                    if (!MTL.SafetySignal)
    //                    {
    //                        sb.AppendLine($"MTL safety signal is not ready.");
    //                        need_pause = true;
    //                    }
    //                    MTLPauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByMTLStatus method, caller:[{caller}], after Check." +
    //    $" Alive:[{MTL.Is_Eq_Alive}] Mode:[{MTL.MTxMode}] Location:[{MTL.MTLLocation}] RailStatus:[{MTL.MTLRailStatus}] Error:[{MTL.StopSignal}] needPause:[{need_pause}] " +
    //    $"ForcePass:[{DebugParameter.isForceBypassMTLPauseCheck}] pauseSet:[{pauseSet}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");

    //                    if (DebugParameter.isForceBypassMTLPauseCheck)
    //                    {
    //                        if (pauseSet)
    //                        {
    //                            pauseSet = false;
    //                            bcf.App.BCFApplication.onInfoMsg("MTL Status is OK now, resume all vehicle.");
    //                            MTLPauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByMTLStatus method, caller:[{caller}], In Force Pass before resume OHT." +
    //$" Alive:[{MTL.Is_Eq_Alive}] Mode:[{MTL.MTxMode}] Location:[{MTL.MTLLocation}] RailStatus:[{MTL.MTLRailStatus}] Error:[{MTL.StopSignal}] needPause:[{need_pause}] " +
    //$"ForcePass:[{DebugParameter.isForceBypassMTLPauseCheck}] pauseSet:[{pauseSet}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                            //因為Vehicle Connection Status跟MTL Status都會Resume，必須兩邊都沒有要Pause才能Resume。
    //                            if (vhpauseSet == false && pauseSet == false)
    //                            {
    //                                ResumeAllVehicleBySafetyPause();
    //                            }

    //                            MTLPauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByMTLStatus method, caller:[{caller}], In Force Pass after resume OHT." +
    //$" Alive:[{MTL.Is_Eq_Alive}] Mode:[{MTL.MTxMode}] Location:[{MTL.MTLLocation}] RailStatus:[{MTL.MTLRailStatus}] Error:[{MTL.StopSignal}] needPause:[{need_pause}] " +
    //$"ForcePass:[{DebugParameter.isForceBypassMTLPauseCheck}] pauseSet:[{pauseSet}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                        }
    //                        MTLPauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByMTLStatus method, caller:[{caller}], In Force Pass ready to leave." +
    //$" Alive:[{MTL.Is_Eq_Alive}] Mode:[{MTL.MTxMode}] Location:[{MTL.MTLLocation}] RailStatus:[{MTL.MTLRailStatus}] Error:[{MTL.StopSignal}] needPause:[{need_pause}] " +
    //$"ForcePass:[{DebugParameter.isForceBypassMTLPauseCheck}] pauseSet:[{pauseSet}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                    }
    //                    else
    //                    {
    //                        if (pauseSet)
    //                        {
    //                            if (need_pause)
    //                            {
    //                                //pauseSet = true;
    //                                //bcf.App.BCFApplication.onErrorMsg("MTL Status is wrong, pause all vehicle!!!" + sb);
    //                                PauseAllVehicleBySafetyPause();
    //                                //Task.Run(() => scApp.VehicleService.PauseAllVehicleBySafetyPause());
    //                                //scApp.VehicleService.PauseAllVehicleBySafetyPause();
    //                                MTLPauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByMTLStatus method, caller:[{caller}], In Normal process do pause." +
    //$" Alive:[{MTL.Is_Eq_Alive}] Mode:[{MTL.MTxMode}] Location:[{MTL.MTLLocation}] RailStatus:[{MTL.MTLRailStatus}] Error:[{MTL.StopSignal}] needPause:[{need_pause}] " +
    //$"ForcePass:[{DebugParameter.isForceBypassMTLPauseCheck}] pauseSet:[{pauseSet}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                            }
    //                            else
    //                            {
    //                                pauseSet = false;
    //                                bcf.App.BCFApplication.onInfoMsg("MTL Status is OK now, resume all vehicle.");
    //                                MTLPauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByMTLStatus method, caller:[{caller}], In Normal process before resume OHT." +
    //$" Alive:[{MTL.Is_Eq_Alive}] Mode:[{MTL.MTxMode}] Location:[{MTL.MTLLocation}] RailStatus:[{MTL.MTLRailStatus}] Error:[{MTL.StopSignal}] needPause:[{need_pause}] " +
    //$"ForcePass:[{DebugParameter.isForceBypassMTLPauseCheck}] pauseSet:[{pauseSet}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                                //因為Vehicle Connection Status跟MTL Status都會Resume，必須兩邊都沒有要Pause才能Resume。
    //                                if (vhpauseSet == false && pauseSet == false)
    //                                {
    //                                    ResumeAllVehicleBySafetyPause();
    //                                }
    //                                MTLPauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByMTLStatus method, caller:[{caller}], In Normal process after resume OHT." +
    //    $" Alive:[{MTL.Is_Eq_Alive}] Mode:[{MTL.MTxMode}] Location:[{MTL.MTLLocation}] RailStatus:[{MTL.MTLRailStatus}] Error:[{MTL.StopSignal}] needPause:[{need_pause}] " +
    //    $"ForcePass:[{DebugParameter.isForceBypassMTLPauseCheck}] pauseSet:[{pauseSet}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                            }
    //                        }
    //                        else
    //                        {
    //                            if (need_pause)
    //                            {
    //                                pauseSet = true;
    //                                bcf.App.BCFApplication.onErrorMsg("MTL Status is wrong, pause all vehicle!!!" + sb);
    //                                MTLPauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByMTLStatus method, caller:[{caller}], In Normal process before Stop OHT." +
    //$" Alive:[{MTL.Is_Eq_Alive}] Mode:[{MTL.MTxMode}] Location:[{MTL.MTLLocation}] RailStatus:[{MTL.MTLRailStatus}] Error:[{MTL.StopSignal}] needPause:[{need_pause}] " +
    //$"ForcePass:[{DebugParameter.isForceBypassMTLPauseCheck}] pauseSet:[{pauseSet}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                                //Task.Run(() => scApp.VehicleService.PauseAllVehicleBySafetyPause());
    //                                PauseAllVehicleBySafetyPause();
    //                                MTLPauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByMTLStatus method, caller:[{caller}], In Normal process after Stop OHT." +
    //    $" Alive:[{MTL.Is_Eq_Alive}] Mode:[{MTL.MTxMode}] Location:[{MTL.MTLLocation}] RailStatus:[{MTL.MTLRailStatus}] Error:[{MTL.StopSignal}] needPause:[{need_pause}] " +
    //    $"ForcePass:[{DebugParameter.isForceBypassMTLPauseCheck}] pauseSet:[{pauseSet}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                            }
    //                            else
    //                            {
    //                                //因為Vehicle Connection Status跟MTL Status都會Resume，必須兩邊都沒有要Pause才能Resume。
    //                                if (vhpauseSet == false && pauseSet == false)
    //                                {
    //                                    ResumeAllVehicleBySafetyPause();
    //                                }
    //                                MTLPauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByMTLStatus method, caller:[{caller}], In Normal process do continue." +
    //    $" Alive:[{MTL.Is_Eq_Alive}] Mode:[{MTL.MTxMode}] Location:[{MTL.MTLLocation}] RailStatus:[{MTL.MTLRailStatus}] Error:[{MTL.StopSignal}] needPause:[{need_pause}] " +
    //    $"ForcePass:[{DebugParameter.isForceBypassMTLPauseCheck}] pauseSet:[{pauseSet}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                                //pauseSet = false;
    //                                //scApp.VehicleService.ResumeAllVehicleBySafetyPause();
    //                            }
    //                        }
    //                    }


    //                }
    //                catch (Exception ex)
    //                {
    //                    logger.Error(ex, "Exception");
    //                    MTLPauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByMTLStatus method, caller:[{caller}],Exception happan. Exception:[{ex.Message}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                }
    //                finally
    //                {
    //                    MTLPauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByMTLStatus method, caller:[{caller}], before leave lock." +
    //    $" Alive:[{MTL.Is_Eq_Alive}] Mode:[{MTL.MTxMode}] Location:[{MTL.MTLLocation}] RailStatus:[{MTL.MTLRailStatus}] Error:[{MTL.StopSignal}]  " +
    //    $"ForcePass:[{DebugParameter.isForceBypassMTLPauseCheck}] pauseSet:[{pauseSet}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
    //                    System.Threading.Interlocked.Exchange(ref syncPoint, 0);
    //                }

    //                //}
    //            }

    //        }
        }
        #endregion VehiclePauseByMTLStatus



        #region Specially Control
        public bool VehicleAutoModeCahnge(string vh_id, VHModeStatus mode_status)
        {
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            //AVEHICLE vh = scApp.VehicleBLL.getVehicleByID(vh_id);
            //lock (vh.StatusUpdate_Sync)
            //{
            if (vh.MODE_STATUS != VHModeStatus.Manual)
            {
                scApp.VehicleBLL.cache.updataVehicleMode(vh_id, mode_status);
                vh?.onVehicleStatusChange();
                return true;
            }
            else
            {
                return false;
            }
            //}
        }

        public bool changeVhStatusToAutoRemote(string vhID)
        {
            scApp.VehicleBLL.cache.updataVehicleMode(vhID, VHModeStatus.AutoRemote);
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vhID);
            vh?.onVehicleStatusChange();
            return true;
        }
        public bool changeVhStatusToAutoLocal(string vhID)
        {
            scApp.VehicleBLL.cache.updataVehicleMode(vhID, VHModeStatus.AutoLocal);
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vhID);
            vh?.onVehicleStatusChange();
            return true;
        }

        public bool changeVhStatusToAutoMTL(string vhID)
        {
            scApp.VehicleBLL.cache.updataVehicleMode(vhID, VHModeStatus.AutoMtl);
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vhID);
            vh?.onVehicleStatusChange();
            return true;
        }
        //public bool changeVhStatusToAutoCharging(string vhID)
        //{
        //    scApp.VehicleBLL.cache.updataVehicleMode(vhID, VHModeStatus.AutoCharging);
        //    AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vhID);
        //    vh?.onVehicleStatusChange();
        //    return true;
        //}
        public void PauseAllVehicleByNormalPause()
        {
            List<AVEHICLE> vhs = scApp.getEQObjCacheManager().getAllVehicle();
            foreach (var vh in vhs)
            {
                Send.Pause(vh.VEHICLE_ID, PauseEvent.Pause, PauseType.OhxC);
            }
        }
        public void ResumeAllVehicleByNormalPause()
        {
            List<AVEHICLE> vhs = scApp.getEQObjCacheManager().getAllVehicle();
            foreach (var vh in vhs)
            {
                Send.Pause(vh.VEHICLE_ID, PauseEvent.Continue, PauseType.OhxC);
            }
        }


        public void PauseAllVehicleBySafetyPause()
        {
            List<AVEHICLE> vhs = scApp.getEQObjCacheManager().getAllVehicle();
            foreach (var vh in vhs)
            {
                if (vh.isTcpIpConnect && vh.SAFETY_PAUSE == VhStopSingle.StopSingleOff)
                {
                    Send.Pause(vh.VEHICLE_ID, PauseEvent.Pause, PauseType.Safety);
                }
            }
        }
        public void ResumeAllVehicleBySafetyPause()
        {


            List<AVEHICLE> vhs = scApp.getEQObjCacheManager().getAllVehicle();
            foreach (var vh in vhs)
            {
                if (vh.isTcpIpConnect && vh.SAFETY_PAUSE == VhStopSingle.StopSingleOn)
                {
                    Send.Pause(vh.VEHICLE_ID, PauseEvent.Continue, PauseType.Safety);
                }
            }
        }

        public void updateVhType(string vhID, E_VH_TYPE vhType)
        {
            try
            {
                scApp.VehicleBLL.updataVehicleType(vhID, vhType);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex);
            }
        }
        #endregion Specially Control
        #region RoadService
        public (bool isSuccess, ASEGMENT segment) doEnableDisableSegment(string segment_id, E_PORT_STATUS port_status)
        {
            ASEGMENT segment = null;
            try
            {
                //List<APORTSTATION> port_stations = scApp.MapBLL.loadAllPortBySegmentID(segment_id);

                using (TransactionScope tx = SCUtility.getTransactionScope())
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {

                        switch (port_status)
                        {
                            case E_PORT_STATUS.InService:
                                segment = scApp.GuideBLL.unbanRouteTwoDirect(segment_id);
                                scApp.SegmentBLL.cache.EnableSegment(segment_id);
                                break;
                            case E_PORT_STATUS.OutOfService:
                                segment = scApp.GuideBLL.banRouteTwoDirect(segment_id);
                                scApp.SegmentBLL.cache.DisableSegment(segment_id);
                                break;
                        }
                        //foreach (APORTSTATION port_station in port_stations)
                        //{
                        //    scApp.MapBLL.updatePortStatus(port_station.PORT_ID, port_status);
                        //}
                        tx.Complete();
                    }
                }
                //List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
                //foreach (APORTSTATION port_station in port_stations)
                //{
                //    switch (port_status)
                //    {
                //        case E_PORT_STATUS.InService:
                //            scApp.ReportBLL.ReportPortInServeice(port_station.PORT_ID, reportqueues);
                //            break;
                //        case E_PORT_STATUS.OutOfService:
                //            scApp.ReportBLL.ReportPortOutServeice(port_station.PORT_ID, reportqueues);
                //            break;
                //    }
                //}
                //scApp.ReportBLL.sendMCSMessageAsyn(reportqueues);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
            return (segment != null, segment);
        }
        #endregion RoadService

        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new AspectWeaver(parameter, this);
        }

    }
}
