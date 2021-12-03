//*********************************************************************************
//      EQType2SecsMapAction.cs
//*********************************************************************************
// File Name: EQType2SecsMapAction.cs
// Description: Type2 EQ Map Action
//
//(c) Copyright 2021, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag          Description
// ------------- -------------  -------------  ------       -----------------------------
// 2021/05/20    Kevin Wei      N/A            A20210520-01 當在判斷是否可以指令命令給某台車狀態失敗後，
//                                                          就直接進入紀錄log並回傳false，避免最後紀錄出來的訊息，
//                                                          造成誤導。
//**********************************************************************************
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.BLL;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.Data.PLC_Functions;
using com.mirle.ibg3k0.sc.Data.SECS.OHTC.AT_S;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using DocumentFormat.OpenXml.Bibliography;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using static com.mirle.ibg3k0.sc.ALINE;
using static com.mirle.ibg3k0.sc.AVEHICLE;

namespace com.mirle.ibg3k0.sc.Service
{
    public class TransferService
    {
        protected NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public Logger TransferServiceLogger = NLog.LogManager.GetLogger("TransferServiceLogger");
        protected SCApplication scApp = null;
        private TransferBLL transferBLL = null;
        private CarrierBLL carrierBLL = null;
        private SysExcuteQualityBLL sysExcuteQualityBLL = null;
        private ReportBLL reportBLL = null;
        private LineBLL lineBLL = null;
        private CMDBLL cmdBLL = null;
        protected ALINE line = null;
        ITranAssigner NormalTranAssigner;
        ITranAssigner SwapTranAssigner;
        private Dictionary<string, string> stagedVehiclePortMapping = new Dictionary<string, string>();

        public TransferService()
        {

        }
        public void start(SCApplication _app)
        {
            scApp = _app;
            reportBLL = _app.ReportBLL;
            lineBLL = _app.LineBLL;
            transferBLL = _app.TransferBLL;
            carrierBLL = _app.CarrierBLL;
            sysExcuteQualityBLL = _app.SysExcuteQualityBLL;
            cmdBLL = _app.CMDBLL;
            line = scApp.getEQObjCacheManager().getLine();

            line.addEventHandler(nameof(ConnectionInfoService), nameof(line.MCSCommandAutoAssign), PublishTransferInfo);
            NormalTranAssigner = new TranAssignerNormal(scApp);
            SwapTranAssigner = new TransferAssignerSwap(scApp);

            initPublish(line);
        }
        private void initPublish(ALINE line)
        {
            PublishTransferInfo(line, null);
            //PublishOnlineCheckInfo(line, null);
            //PublishPingCheckInfo(line, null);
        }

        private void PublishTransferInfo(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                ALINE line = sender as ALINE;
                if (sender == null) return;
                byte[] line_serialize = BLL.LineBLL.Convert2GPB_TransferInfo(line);
                scApp.getNatsManager().PublishAsync
                    (SCAppConstants.NATS_SUBJECT_TRANSFER, line_serialize);


                //TODO 要改用GPP傳送
                //var line_Serialize = ZeroFormatter.ZeroFormatterSerializer.Serialize(line);
                //scApp.getNatsManager().PublishAsync
                //    (string.Format(SCAppConstants.NATS_SUBJECT_LINE_INFO), line_Serialize);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
        }

        public bool Creat(ATRANSFER transfer)
        {
            try
            {
                //var carrier_info = transfer.GetCarrierInfo();
                var carrier_info = transfer.GetCarrierInfo(scApp.VehicleBLL);
                var sys_excute_quality_info = transfer.GetSysExcuteQuality(scApp.VehicleBLL);
                transferBLL.db.transfer.add(transfer);
                if (transfer.TRANSFERSTATE == E_TRAN_STATUS.Queue)
                {

                    carrierBLL.db.addOrUpdate(carrier_info);
                    //sysExcuteQualityBLL.addSysExcuteQuality(sys_excute_quality_info);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return false;
            }
            return true;
        }

        public bool SimpleCreate(ATRANSFER transfer)
        {
            try
            {
                transferBLL.db.transfer.add(transfer);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return false;
            }
            return true;
        }

        public bool AbortOrCancel(string transferID, ProtocolFormat.OHTMessage.CMDCancelType actType)
        {
            ATRANSFER mcs_cmd = scApp.CMDBLL.GetTransferByID(transferID);
            if (mcs_cmd == null)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_AGV,
                   Data: $"want to cancel/abort mcs cmd:{transferID},but cmd not exist.");


                return false;
            }
            bool is_success = true;
            switch (actType)
            {
                case ProtocolFormat.OHTMessage.CMDCancelType.CmdCancel:
                    scApp.ReportBLL.newReportTransferCancelInitial(transferID, null);
                    if (IsInTransferTime(mcs_cmd))
                    {
                        scApp.ReportBLL.newReportTransferCancelFailed(transferID, null);
                        return false;
                    }
                    if (mcs_cmd.TRANSFERSTATE == E_TRAN_STATUS.Queue)
                    {
                        scApp.CMDBLL.updateTransferCmd_TranStatus2Canceled(transferID);
                        scApp.ReportBLL.newReportTransferCancelCompleted(transferID, false, null);
                    }
                    else if (mcs_cmd.TRANSFERSTATE >= E_TRAN_STATUS.PreInitial)
                    {
                        scApp.ReportBLL.newReportTransferCancelFailed(mcs_cmd.ID, null);
                    }
                    break;
                case ProtocolFormat.OHTMessage.CMDCancelType.CmdAbort:
                    scApp.ReportBLL.newReportTransferAbortInitial(transferID, null);
                    //if (mcs_cmd.TRANSFERSTATE >= E_TRAN_STATUS.Transferring)
                    if (IsInTransferTime(mcs_cmd))
                    {
                        scApp.ReportBLL.newReportTransferAbortFailed(transferID, null);
                        return false;
                    }
                    if (mcs_cmd.TRANSFERSTATE >= E_TRAN_STATUS.Initial)
                    {
                        ACMD excute_cmd = scApp.CMDBLL.GetCommandByTransferCmdID(transferID);
                        bool has_cmd_excute = excute_cmd != null;
                        if (has_cmd_excute)
                        {
                            is_success = scApp.VehicleService.Send.Cancel(excute_cmd.VH_ID, excute_cmd.ID, ProtocolFormat.OHTMessage.CMDCancelType.CmdAbort);
                            if (is_success)
                            {
                                scApp.CMDBLL.updateCMD_MCS_TranStatus2Aborting(transferID);
                            }
                            else
                            {
                                scApp.ReportBLL.newReportTransferAbortFailed(transferID, null);
                            }
                        }
                        else
                        {
                            scApp.ReportBLL.newReportTransferAbortFailed(transferID, null);
                            is_success = false;
                        }
                    }
                    //else if (mcs_cmd.TRANSFERSTATE >= E_TRAN_STATUS.Queue && mcs_cmd.TRANSFERSTATE < E_TRAN_STATUS.Transferring)
                    else
                    {
                        scApp.ReportBLL.newReportTransferAbortFailed(transferID, null);
                        is_success = false;
                    }
                    break;
            }
            return is_success;
        }

        public bool Update(string transferID, int priority)
        {
            ATRANSFER mcs_cmd = scApp.CMDBLL.GetTransferByID(transferID);
            if (mcs_cmd == null)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_AGV,
                Data: $"want to update mcs cmd:{transferID},but cmd not exist.");
                return false;
            }
            bool is_success = true;

            if (mcs_cmd.TRANSFERSTATE >= E_TRAN_STATUS.Transferring) //當狀態變為Transferring時，即代表已經是Load complete
            {
                scApp.ReportBLL.newReportTransferUpdateFailed(mcs_cmd.ID, null);
            }
            else
            {
                scApp.CMDBLL.updateCMD_MCS_Priority(mcs_cmd, priority);
                scApp.ReportBLL.newReportTransferUpdateCompleted(transferID, null);
            }



            return is_success;
        }

        private bool IsInTransferTime(ATRANSFER mcs_cmd)
        {
            //如果在Load中就回傳TRUE
            if (mcs_cmd.COMMANDSTATE >= ATRANSFER.COMMAND_STATUS_BIT_INDEX_LOAD_ARRIVE &&
                mcs_cmd.COMMANDSTATE <= ATRANSFER.COMMAND_STATUS_BIT_INDEX_LOAD_COMPLETE)
            {
                return true;
            }
            //如果在Unload中就回傳TRUE
            if (mcs_cmd.COMMANDSTATE >= ATRANSFER.COMMAND_STATUS_BIT_INDEX_UNLOAD_ARRIVE)
            {
                return true;
            }
            return false;
        }

        private long syncTranCmdPoint = 0;




        protected (bool hasFind, AVEHICLE vh) findOtherSameSourcePortCmdExcuteAndVhCanService(string sourcePort, List<VTRANSFER> excuteVTran)
        {
            if (excuteVTran == null || excuteVTran.Count == 0) return (false, null);
            var other_excute_v_trans = excuteVTran.Where(tran => SCUtility.isMatche(tran.HOSTSOURCE, sourcePort)).ToList();
            if (other_excute_v_trans == null || other_excute_v_trans.Count == 0) return (false, null);
            foreach (var tran in other_excute_v_trans)
            {
                if (tran.TRANSFERSTATE >= E_TRAN_STATUS.Transferring) continue;
                AVEHICLE vh = scApp.VehicleBLL.cache.getVehicle(tran.VH_ID);
                if (vh == null) continue;
                if (scApp.VehicleBLL.cache.canAssignTransferCmd(scApp.CMDBLL, vh))
                {
                    return (true, vh);
                }
            }
            return (false, null);
        }

        protected void queueTimeOutCheck(List<VTRANSFER> un_finish_trnasfer)
        {
            try
            {
                bool has_mcs_cmd_time_out_in_queue = un_finish_trnasfer.Where(tran => tran.IsQueueTimeOut).Count() > 0;
                if (has_mcs_cmd_time_out_in_queue)
                {
                    scApp.LineService.ProcessAlarmReport("", "OHTC", AlarmBLL.OHTC_TRAN_COMMAND_IN_QUEUE_TIME_OUT, ProtocolFormat.OHTMessage.ErrorStatus.ErrSet,
                                $"OHTC has trnasfer commmand in queue over time:{SystemParameter.TransferCommandQueueTimeOut_mSec}ms");
                }
                else
                {
                    scApp.LineService.ProcessAlarmReport("", "OHTC", AlarmBLL.OHTC_TRAN_COMMAND_IN_QUEUE_TIME_OUT, ProtocolFormat.OHTMessage.ErrorStatus.ErrReset,
                                $"OHTC has trnasfer commmand in queue over time:{SystemParameter.TransferCommandQueueTimeOut_mSec}ms");
                }
            }
            catch { }
        }

        protected (bool hasFind, AVEHICLE hasCarrierOfVh, VTRANSFER tran) tryFindAssignOnVhCarrier(List<VTRANSFER> tran_queue_in_group)
        {
            foreach (var tran in tran_queue_in_group)
            {
                string hostsource = tran.HOSTSOURCE;
                string from_adr = string.Empty;
                bool source_is_a_port = scApp.PortStationBLL.OperateCatch.IsExist(hostsource);
                //if (!source_is_a_port) continue;
                if (!source_is_a_port)
                {
                    var bestSuitableVh = scApp.VehicleBLL.cache.getVehicleByLocationRealID(hostsource);
                    if (bestSuitableVh == null ||
                        bestSuitableVh.IsError ||
                        bestSuitableVh.MODE_STATUS != VHModeStatus.AutoRemote)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_AGV,
                           Data: $"Has transfer command:{SCUtility.Trim(tran.ID, true)} for vh:{hostsource}" +
                                 $"but it error happend or not auto remove or not this object.");
                        continue;
                    }
                    return (true, bestSuitableVh, tran);
                }
            }
            return (false, null, null);
        }

        protected (bool isFind, IEnumerable<IGrouping<AGVStation, VTRANSFER>> tranGroupsByAGVStation)
            checkAndFindReserveSuccessUnloadToAGVStationTransfer(List<VTRANSFER> unfinish_transfer)
        {
            var target_is_agv_stations = unfinish_transfer.
                                         Where(vtran => vtran.IsTargetPortAGVStation(scApp.EqptBLL));
            if (target_is_agv_stations.Count() == 0) { return (false, null); }
            target_is_agv_stations = target_is_agv_stations.OrderByDescending(tran => tran.PORT_PRIORITY).ToList();
            var target_is_agv_station_groups = target_is_agv_stations.
                                               GroupBy(tran => tran.getTragetPortEQ(scApp.EqptBLL) as AGVStation).
                                               ToList();
            foreach (var target_is_agv_station in target_is_agv_station_groups.ToList())
            {
                var agv_station = target_is_agv_station.Key;
                if (!agv_station.IsReservation)
                {
                    target_is_agv_station_groups.Remove(target_is_agv_station);
                    var group_tran_ids = target_is_agv_station.ToList().Select(tran => tran.ID);
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_AGV,
                       Data: $"agv station:{agv_station.getAGVStationID()},not reserve success ,remove it.group tran ids:{string.Join(",", group_tran_ids)}");
                }
            }
            return (target_is_agv_station_groups.Count != 0, target_is_agv_station_groups);
        }


        protected (bool isFind, VTRANSFER nearestTransfer) FindNearestTransferBySourcePort(VTRANSFER firstTrnasfer, List<VTRANSFER> transfers)
        {
            VTRANSFER nearest_transfer = null;
            double minimum_cost = double.MaxValue;
            try
            {
                string first_tran_source_port_id = SCUtility.Trim(firstTrnasfer.HOSTSOURCE);
                bool first_tran_source_is_port = scApp.PortStationBLL.OperateCatch.IsExist(first_tran_source_port_id);
                if (!first_tran_source_is_port) return (false, null);
                string first_tran_from_adr_id = "";
                scApp.MapBLL.getAddressID(first_tran_source_port_id, out first_tran_from_adr_id);
                foreach (var tran in transfers)
                {
                    string second_tran_source_port_id = tran.HOSTSOURCE;
                    bool source_is_a_port = scApp.PortStationBLL.OperateCatch.IsExist(second_tran_source_port_id);
                    if (!source_is_a_port) continue;

                    string second_tran_from_adr_id = string.Empty;
                    scApp.MapBLL.getAddressID(second_tran_source_port_id, out second_tran_from_adr_id);

                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(CMDBLL), Device: string.Empty,
                                  Data: $"Start calculation distance, command id:{tran.ID.Trim()} command source port:{tran.HOSTSOURCE?.Trim()}," +
                                        $"first transfer of source port:{first_tran_source_port_id} , prepare sencond port:{second_tran_source_port_id}...",
                                  XID: tran.ID);
                    //var result = scApp.GuideBLL.getGuideInfo(first_tran_from_adr_id, second_tran_from_adr_id);
                    var result = scApp.GuideBLL.IsRoadWalkable(first_tran_from_adr_id, second_tran_from_adr_id, out int totalCost);
                    //double total_section_distance = result.guideSectionIds != null && result.guideSectionIds.Count > 0 ?
                    //                                scApp.SectionBLL.cache.GetSectionsDistance(result.guideSectionIds) : 0;
                    double total_section_distance = 0;
                    //if (result.isSuccess)
                    if (result)
                    {
                        //total_section_distance = result.guideSectionIds != null && result.guideSectionIds.Count > 0 ?
                        //                                scApp.SectionBLL.cache.GetSectionsDistance(result.guideSectionIds) : 0;
                        total_section_distance = totalCost;
                    }
                    else
                    {
                        total_section_distance = double.MaxValue;
                    }
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(CMDBLL), Device: string.Empty,
                                  Data: $"Start calculation distance, command id:{tran.ID.Trim()} command source port:{tran.HOSTSOURCE?.Trim()}," +
                                        $"first transfer of source port:{first_tran_source_port_id} , prepare sencond port:{second_tran_source_port_id},distance:{total_section_distance}",
                                  XID: tran.ID);
                    if (total_section_distance < minimum_cost)
                    {
                        nearest_transfer = tran;
                        minimum_cost = total_section_distance;
                    }
                }
                if (minimum_cost == double.MaxValue)
                {
                    nearest_transfer = null;
                }
            }
            catch (Exception ex)
            {
                nearest_transfer = null;
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(CMDBLL), Device: string.Empty,
                   Data: ex);
            }
            return (nearest_transfer != null && minimum_cost != double.MaxValue,
                    nearest_transfer);
        }
        protected (bool isFind, VTRANSFER nearestTransfer) FindNearestTransferByTargetPort(VTRANSFER firstTrnasfer, List<VTRANSFER> inQueueTransfers)
        {
            VTRANSFER nearest_transfer = null;
            double minimum_cost = double.MaxValue;
            try
            {
                string first_tran_dest_port_id = SCUtility.Trim(firstTrnasfer.HOSTDESTINATION);
                bool first_tran_dest_is_port = scApp.PortStationBLL.OperateCatch.IsExist(first_tran_dest_port_id);
                if (!first_tran_dest_is_port) return (false, null);
                string first_tran_dest_adr_id = "";
                scApp.MapBLL.getAddressID(first_tran_dest_port_id, out first_tran_dest_adr_id);
                foreach (var tran in inQueueTransfers)
                {
                    string second_tran_source_port_id = tran.HOSTSOURCE;
                    bool second_tran_source_is_a_port = scApp.PortStationBLL.OperateCatch.IsExist(second_tran_source_port_id);
                    if (!second_tran_source_is_a_port) continue;

                    string second_tran_source_adr_id = string.Empty;
                    scApp.MapBLL.getAddressID(second_tran_source_port_id, out second_tran_source_adr_id);

                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(CMDBLL), Device: string.Empty,
                                  Data: $"Start calculation distance, command id:{tran.ID.Trim()} command source port:{tran.HOSTDESTINATION?.Trim()}," +
                                        $"first transfer of dest port:{first_tran_dest_port_id} , prepare sencond source port:{second_tran_source_port_id}...",
                                  XID: tran.ID);
                    //var result = scApp.GuideBLL.getGuideInfo(first_tran_dest_adr_id, second_tran_source_adr_id);
                    var result = scApp.GuideBLL.IsRoadWalkable(first_tran_dest_adr_id, second_tran_source_adr_id, out int totalCost);
                    //double total_section_distance = result.guideSectionIds != null && result.guideSectionIds.Count > 0 ?
                    //                                scApp.SectionBLL.cache.GetSectionsDistance(result.guideSectionIds) : 0;
                    double total_section_distance = 0;
                    if (result)
                    {
                        //total_section_distance = result.guideSectionIds != null && result.guideSectionIds.Count > 0 ?
                        //                                scApp.SectionBLL.cache.GetSectionsDistance(result.guideSectionIds) : 0;
                        total_section_distance = totalCost;
                    }
                    else
                    {
                        total_section_distance = double.MaxValue;
                    }
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(CMDBLL), Device: string.Empty,
                                  Data: $"Start calculation distance, command id:{tran.ID.Trim()} command source port:{tran.HOSTSOURCE?.Trim()}," +
                                        $"first transfer of source port:{first_tran_dest_port_id} , prepare sencond port:{second_tran_source_port_id},distance:{total_section_distance}",
                                  XID: tran.ID);
                    if (total_section_distance < minimum_cost)
                    {
                        nearest_transfer = tran;
                        minimum_cost = total_section_distance;
                    }
                }
                if (minimum_cost == double.MaxValue)
                {
                    nearest_transfer = null;
                }
            }
            catch (Exception ex)
            {
                nearest_transfer = null;
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(CMDBLL), Device: string.Empty,
                   Data: ex);
            }
            return (nearest_transfer != null && minimum_cost != double.MaxValue,
                    nearest_transfer);
        }

        private (bool isFind, List<VTRANSFER> portPriorityMaxCommands) checkPortPriorityMaxCommand(List<VTRANSFER> in_queue_transfer)
        {
            List<VTRANSFER> port_priority_max_command = new List<VTRANSFER>();
            foreach (VTRANSFER cmd in in_queue_transfer)
            {
                APORTSTATION source_port = scApp.getEQObjCacheManager().getPortStation(cmd.HOSTSOURCE);
                APORTSTATION destination_port = scApp.getEQObjCacheManager().getPortStation(cmd.HOSTDESTINATION);
                if (source_port != null && source_port.PRIORITY >= SystemParameter.PortMaxPriority)
                {
                    if (destination_port != null)
                    {
                        if (source_port.PRIORITY >= destination_port.PRIORITY)
                        {
                            cmd.PORT_PRIORITY = source_port.PRIORITY;
                        }
                        else
                        {
                            cmd.PORT_PRIORITY = destination_port.PRIORITY;
                        }
                    }
                    else
                    {
                        cmd.PORT_PRIORITY = source_port.PRIORITY;
                    }
                    port_priority_max_command.Add(cmd);
                    continue;
                }
                if (destination_port != null && destination_port.PRIORITY >= SystemParameter.PortMaxPriority)
                {
                    if (source_port != null)
                    {
                        if (destination_port.PRIORITY >= source_port.PRIORITY)
                        {
                            cmd.PORT_PRIORITY = destination_port.PRIORITY;
                        }
                        else
                        {
                            cmd.PORT_PRIORITY = source_port.PRIORITY;
                        }
                    }
                    else
                    {
                        cmd.PORT_PRIORITY = destination_port.PRIORITY;
                    }
                    port_priority_max_command.Add(cmd);
                    continue;
                }
            }

            if (port_priority_max_command.Count == 0)
            {
                port_priority_max_command = null;
            }
            else
            {
                port_priority_max_command = port_priority_max_command.OrderByDescending(cmd => cmd.PORT_PRIORITY).ToList();
            }
            return (port_priority_max_command != null, port_priority_max_command);
        }

        public (bool isFind, AVEHICLE nearestVh, VTRANSFER nearestTransfer) FindVhAndCommand(List<VTRANSFER> transfers)
        {
            List<AVEHICLE> idle_vhs = scApp.VehicleBLL.cache.loadAllVh().ToList();
            scApp.VehicleBLL.cache.filterCanNotExcuteTranVh(ref idle_vhs, scApp.CMDBLL, E_VH_TYPE.None);
            //return FindVhAndCommandOrderByTransfer(idle_vhs, transfers);
            return FindVhAndCommandOrderbyDistance(idle_vhs, transfers);
        }
        private (bool isFind, AVEHICLE nearestVh, VTRANSFER nearestTransfer) FindVhAndCommandOrderByTransfer(List<AVEHICLE> vhs, List<VTRANSFER> transfers)
        {
            try
            {
                foreach (var tran in transfers)
                {
                    foreach (var vh in vhs)
                    {
                        string hostsource = tran.HOSTSOURCE;
                        string from_adr = string.Empty;

                        scApp.MapBLL.getAddressID(hostsource, out from_adr);
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(CMDBLL), Device: string.Empty,
                                      Data: $"Start try find vh , command id:{tran.ID.Trim()} command source port:{tran.HOSTSOURCE?.Trim()}," +
                                            $"vh:{vh.VEHICLE_ID} current adr:{vh.CUR_ADR_ID},from adr:{from_adr} ...",
                                      XID: tran.ID);
                        //var result = scApp.GuideBLL.getGuideInfo(vh.CUR_ADR_ID, from_adr);
                        var result = scApp.GuideBLL.IsRoadWalkable(vh.CUR_ADR_ID, from_adr);
                        if (result)
                        {
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(CMDBLL), Device: string.Empty,
                                          Data: $"Find the vh success , command id:{tran.ID.Trim()} command source port:{tran.HOSTSOURCE?.Trim()}," +
                                                $"vh:{vh.VEHICLE_ID} current adr:{vh.CUR_ADR_ID},from adr:{from_adr}.",
                                          XID: tran.ID);
                            return (true,
                                    vh,
                                    tran);
                        }
                        else
                        {
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(CMDBLL), Device: string.Empty,
                                          Data: $"Find the vh fail continue check next vh, command id:{tran.ID.Trim()} command source port:{tran.HOSTSOURCE?.Trim()}," +
                                                $"vh:{vh.VEHICLE_ID} current adr:{vh.CUR_ADR_ID},from adr:{from_adr}.",
                                          XID: tran.ID);
                        }
                    }
                }
                return (false,
                        null,
                        null);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(CMDBLL), Device: string.Empty,
                   Data: ex);
                return (false,
                        null,
                        null);
            }
        }
        private (bool isFind, AVEHICLE nearestVh, VTRANSFER nearestTransfer) FindVhAndCommandOrderbyDistance(List<AVEHICLE> vhs, List<VTRANSFER> transfers)
        {
            AVEHICLE nearest_vh = null;
            VTRANSFER nearest_transfer = null;
            double minimum_cost = double.MaxValue;
            try
            {
                foreach (var tran in transfers)
                {
                    foreach (var vh in vhs)
                    {
                        string hostsource = tran.HOSTSOURCE;
                        string from_adr = string.Empty;

                        scApp.MapBLL.getAddressID(hostsource, out from_adr);
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(CMDBLL), Device: string.Empty,
                                      Data: $"Start try find vh , command id:{tran.ID.Trim()} command source port:{tran.HOSTSOURCE?.Trim()}," +
                                            $"vh:{vh.VEHICLE_ID} current adr:{vh.CUR_ADR_ID},from adr:{from_adr} ...",
                                      XID: tran.ID);
                        //var result = scApp.GuideBLL.getGuideInfo(vh.CUR_ADR_ID, from_adr);
                        bool result = scApp.GuideBLL.IsRoadWalkable(vh.CUR_ADR_ID, from_adr, out int totalCost);
                        double total_section_distance = 0;
                        if (result)
                        {
                            //total_section_distance = result.totalCost;
                            total_section_distance = totalCost;
                        }
                        else
                        {
                            total_section_distance = double.MaxValue;
                        }
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(CMDBLL), Device: string.Empty,
                                      Data: $"command id:{tran.ID.Trim()} command source port:{tran.HOSTSOURCE?.Trim()}," +
                                            $"vh:{vh.VEHICLE_ID} current adr:{vh.CUR_ADR_ID},from adr:{from_adr} distance:{total_section_distance}",
                                      XID: tran.ID);
                        if (total_section_distance < minimum_cost)
                        {
                            if (HasOtherVhInTargetBlockOrWillGoTo(vh.VEHICLE_ID, from_adr).hasOtherVh)
                            {
                                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(CMDBLL), Device: string.Empty,
                                              Data: $"Try find the vh:[{vh.VEHICLE_ID}] to block load cst , but has other vh in this block.",
                                              XID: tran.ID);
                                continue;
                            }
                            nearest_transfer = tran;
                            nearest_vh = vh;
                            minimum_cost = total_section_distance;
                        }
                    }
                    if (minimum_cost < double.MaxValue && nearest_transfer != null && nearest_vh != null)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(CMDBLL), Device: string.Empty,
                                      Data: $"command id:{nearest_transfer.ID.Trim()} ," +
                                            $"find the vh:{nearest_vh.VEHICLE_ID} to service",
                                      XID: tran.ID);
                        return (true,
                                nearest_vh,
                                nearest_transfer);
                    }
                }
                if (minimum_cost == double.MaxValue)
                {
                    nearest_transfer = null;
                    nearest_vh = null;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(CMDBLL), Device: string.Empty,
                   Data: ex);
                nearest_vh = null;
                nearest_transfer = null;
            }
            return (nearest_vh != null && nearest_transfer != null,
                    nearest_vh,
                    nearest_transfer);
        }

        protected (bool hasOtherVh, List<string> vhs) HasOtherVhInTargetBlockOrWillGoTo(string vhID, string targetAdr)
        {
            var section = scApp.SectionBLL.cache.GetSectionsByAddress(targetAdr).FirstOrDefault();
            if (section == null) return (false, new List<string>());
            string sec_id = SCUtility.Trim(section.SEC_ID, true);
            var block_control_check_result = scApp.getCommObjCacheManager().IsBlockControlSection(sec_id);
            if (block_control_check_result.isBlockControlSec)
            {
                var block_sec_ids = block_control_check_result.enhanceInfo.EnhanceControlSections;
                var vhs = scApp.VehicleBLL.cache.loadAllVh();
                var other_in_block_vh = vhs.
                    Where(vh => (block_sec_ids.Contains(SCUtility.Trim(vh.CUR_SEC_ID, true)))
                                 && !SCUtility.isMatche(vh.VEHICLE_ID, vhID)).
                    ToList();
                if (other_in_block_vh != null && other_in_block_vh.Count() > 0)
                    return (true, other_in_block_vh.Select(vh => vh.VEHICLE_ID).ToList());

                ALINE line = scApp.getEQObjCacheManager().getLine();
                var acmds = line.CurrentExcuteCommand;
                if (acmds != null)
                {
                    var orther_vh_excute_in_block_cmd = acmds.Where(cmd => block_sec_ids.Contains(cmd.TragetSection(scApp.SectionBLL))
                                                                         && !SCUtility.isMatche(cmd.VH_ID, vhID)).ToList();
                    if (orther_vh_excute_in_block_cmd != null && orther_vh_excute_in_block_cmd.Count > 0)
                        return (true, orther_vh_excute_in_block_cmd.Select(cmd => SCUtility.Trim(cmd.ID, true)).ToList());
                }
            }
            return (false, new List<string>());
        }
        /// <summary>
        /// 尋找可以一起搬出agv Station的命令
        /// </summary>
        /// <param name="inQueueTransfers"></param>
        /// <param name="excutingTransfers"></param>
        /// <returns></returns>
        private (bool isFind, AVEHICLE bestSuitableVh, VTRANSFER bestSuitabletransfer) checkBeforeOnTheWay_V2(List<VTRANSFER> inQueueTransfers, List<VTRANSFER> excutingTransfers)
        {
            AVEHICLE best_suitable_vh = null;
            VTRANSFER best_suitable_transfer = null;
            bool is_success = false;
            //1.找出正在執行的命令中，且他的命令是還沒Load Complete
            //2.接著再去找目前在Queue命令中，Host source port是有相同EQ的
            //3.找到後即可將兩筆命令進行配對
            List<VTRANSFER> can_excute_before_on_the_way_tran = excutingTransfers.
                                                                Where(tr => tr.COMMANDSTATE < ATRANSFER.COMMAND_STATUS_BIT_INDEX_LOAD_COMPLETE).
                                                                ToList();
            //List<VTRANSFER> can_excute_before_on_the_way_tran = excutingTransfers;
            var in_queue_transfers_traget_not_agvstation =
                inQueueTransfers.Where(tran => !(tran.getTragetPortEQ(scApp.EqptBLL) is IAGVStationType));
            foreach (var tran in can_excute_before_on_the_way_tran)
            {
                string excute_tran_eq_id = SCUtility.Trim(tran.getSourcePortEQID(scApp.PortStationBLL));
                var same_eq_ports = in_queue_transfers_traget_not_agvstation.
                                    Where(in_queue_tran => SCUtility.isMatche(in_queue_tran.getSourcePortEQID(scApp.PortStationBLL),
                                                                              excute_tran_eq_id)).
                                    ToList();
                var check_result = FindNearestTransferBySourcePort(tran, same_eq_ports);
                //best_suitable_transfer = inQueueTransfers.
                //                         Where(in_queue_tran => SCUtility.isMatche(in_queue_tran.getSourcePortEQID(scApp.PortStationBLL),
                //                                                                   excute_tran_eq_id)).
                //                         FirstOrDefault();
                //if (best_suitable_transfer != null)
                if (check_result.isFind)
                {
                    best_suitable_transfer = check_result.nearestTransfer;
                    string best_suitable_vh_id = SCUtility.Trim(tran.VH_ID, true);
                    best_suitable_vh = scApp.VehicleBLL.cache.getVehicle(best_suitable_vh_id);
                    if (scApp.VehicleBLL.cache.canAssignTransferCmd(scApp.CMDBLL, best_suitable_vh))
                    {
                        break;
                    }
                    else
                    {
                        best_suitable_vh = null;
                        continue;
                    }
                }
            }
            is_success = best_suitable_vh != null && best_suitable_transfer != null;
            return (is_success, best_suitable_vh, best_suitable_transfer);
        }
        private (bool isFind, AVEHICLE bestSuitableVh, VTRANSFER bestSuitabletransfer) checkAfterOnTheWay_V2(List<VTRANSFER> inQueueTransfers, List<VTRANSFER> excutingTransfers)
        {
            AVEHICLE best_suitable_vh = null;
            VTRANSFER best_suitable_transfer = null;
            bool is_success = false;
            //1.找出正在執行的命令中，且他的命令是還沒Load Complete
            //2.接著再去找目前在Queue命令中，目的地是有相同EQ的
            //3.找到後再從中找出一筆離執行中最近的命令，即可將兩筆命令進行配對
            List<VTRANSFER> can_excute_after_on_the_way_tran = excutingTransfers.
                                                    Where(tr => tr.COMMANDSTATE < ATRANSFER.COMMAND_STATUS_BIT_INDEX_LOAD_COMPLETE).
                                                    ToList();
            //List<VTRANSFER> can_excute_after_on_the_way_tran = excutingTransfers;
            //List<VTRANSFER> can_excute_after_on_the_way_tran = excutingTransfers.
            //                                                    Where(tr => tr.COMMANDSTATE >= ATRANSFER.COMMAND_STATUS_BIT_INDEX_LOAD_COMPLETE &&
            //                                                                tr.COMMANDSTATE < ATRANSFER.COMMAND_STATUS_BIT_INDEX_UNLOAD_COMPLETE).
            //                                                    ToList();

            foreach (var tran in can_excute_after_on_the_way_tran)
            {
                string best_suitable_vh_id = SCUtility.Trim(tran.VH_ID, true);
                best_suitable_vh = scApp.VehicleBLL.cache.getVehicle(best_suitable_vh_id);
                if (!scApp.VehicleBLL.cache.canAssignTransferCmd(scApp.CMDBLL, best_suitable_vh))
                {
                    best_suitable_vh = null;
                    continue;
                }

                var excute_tran_eq = tran.getTragetPortEQ(scApp.EqptBLL);
                if (excute_tran_eq is IAGVStationType)
                {
                    if (!(excute_tran_eq as IAGVStationType).IsReservation)
                    {
                        continue;
                    }
                }

                string excute_tran_eq_id = SCUtility.Trim(tran.getTragetPortEQID(scApp.PortStationBLL));
                var same_eq_ports = inQueueTransfers.
                                    Where(in_queue_tran => SCUtility.isMatche(in_queue_tran.getTragetPortEQID(scApp.PortStationBLL),
                                                                              excute_tran_eq_id)).
                                    ToList();

                var check_result = FindNearestTransferBySourcePort(tran, same_eq_ports);
                if (check_result.isFind)
                {
                    best_suitable_transfer = check_result.nearestTransfer;
                    break;
                }
                else
                {
                    best_suitable_transfer = null;
                }
            }
            is_success = best_suitable_vh != null && best_suitable_transfer != null;
            return (is_success, best_suitable_vh, best_suitable_transfer);
        }
        public bool AssignTransferToVehicle(ATRANSFER waittingExcuteMcsCmd, AVEHICLE bestSuitableVh, string forceAssignStPort)
        {
            bool is_success = true;
            ACMD assign_cmd = waittingExcuteMcsCmd.ConvertToCmd(scApp.PortStationBLL, scApp.SequenceBLL, bestSuitableVh);
            bool is_force_assign_st_port = !SCUtility.isEmpty(forceAssignStPort);
            if (is_force_assign_st_port)
            {
                APORTSTATION port_station = scApp.PortStationBLL.OperateCatch.getPortStation(forceAssignStPort);
                SCAppConstants.EqptType eq_type = port_station.GetEqptType(scApp.EqptBLL);
                if (eq_type != SCAppConstants.EqptType.AGVStation)
                {
                    CMDBLL.CommandCheckResult check_result = CMDBLL.getOrSetCallContext<CMDBLL.CommandCheckResult>(CMDBLL.CALL_CONTEXT_KEY_WORD_OHTC_CMD_CHECK_RESULT);
                    check_result.Result.AppendLine($"port station:{port_station.PORT_ID} can't force assign.it type:{eq_type}");
                    //todo log...
                    return false;
                }
                assign_cmd.DESTINATION = SCUtility.Trim(port_station.ADR_ID, true);
                assign_cmd.DESTINATION_PORT = SCUtility.Trim(port_station.PORT_ID, true);
            }
            else
            {
                //var destination_info = checkAndRenameDestinationPortIfAGVStationReady(assign_cmd);
                var destination_info = checkAndRenameDestinationPortIfAGVStation(assign_cmd);
                if (destination_info.checkSuccess)
                {
                    assign_cmd.DESTINATION = destination_info.destinationAdrID;
                    assign_cmd.DESTINATION_PORT = destination_info.destinationPortID;
                }
                else
                {
                    CMDBLL.CommandCheckResult check_result = CMDBLL.getOrSetCallContext<CMDBLL.CommandCheckResult>(CMDBLL.CALL_CONTEXT_KEY_WORD_OHTC_CMD_CHECK_RESULT);
                    check_result.Result.AppendLine($" vh:{assign_cmd.VH_ID} creat command to db unsuccess. destination port :{SCUtility.Trim(assign_cmd.DESTINATION_PORT, true)} not ready");
                    //todo log...
                    return false;
                }
            }

            is_success = is_success && scApp.CMDBLL.checkCmd(assign_cmd);
            if (is_success)
            {
                using (TransactionScope tx = SCUtility.getTransactionScope())
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        is_success = is_success && scApp.CMDBLL.addCmd(assign_cmd);
                        is_success = is_success && scApp.CMDBLL.updateTransferCmd_TranStatus2PreInitial(waittingExcuteMcsCmd.ID);
                        if (is_success)
                        {
                            tx.Complete();
                        }
                        else
                        {
                            CMDBLL.CommandCheckResult check_result = CMDBLL.getOrSetCallContext<CMDBLL.CommandCheckResult>(CMDBLL.CALL_CONTEXT_KEY_WORD_OHTC_CMD_CHECK_RESULT);
                            check_result.Result.AppendLine($" vh:{assign_cmd.VH_ID} creat command to db unsuccess.");
                            check_result.IsSuccess = false;
                        }
                    }
                }
            }
            return is_success;
        }

        private bool AssignTransferToVehicle_V2(VTRANSFER waittingExcuteMcsCmd, AVEHICLE bestSuitableVh)
        {
            bool is_success = true;
            ACMD assign_cmd = waittingExcuteMcsCmd.ConvertToCmd(scApp.PortStationBLL, scApp.SequenceBLL, bestSuitableVh);
            //var destination_info = checkAndRenameDestinationPortIfAGVStationReady(assign_cmd);
            (bool checkSuccess, string destinationPortID, string destinationAdrID) destination_info =
                default((bool checkSuccess, string destinationPortID, string destinationAdrID));
            //if (DebugParameter.isNeedCheckPortReady)
            //    destination_info = checkAndRenameDestinationPortIfAGVStationReady(assign_cmd);
            //else
            //    destination_info = checkAndRenameDestinationPortIfAGVStationAuto(assign_cmd);
            destination_info = checkAndRenameDestinationPortIfAGVStation(assign_cmd);
            if (destination_info.checkSuccess)
            {
                assign_cmd.DESTINATION = destination_info.destinationAdrID;
                assign_cmd.DESTINATION_PORT = destination_info.destinationPortID;
            }
            else
            {
                //暫時針對有指定vh的才進行預先移動
                if (assign_cmd.getTragetPortEQ(scApp.EqptBLL) is IAGVStationType)
                {
                    var sgv_station = assign_cmd.getTragetPortEQ(scApp.EqptBLL) as IAGVStationType;
                    if (SCUtility.isEmpty(sgv_station.BindingVh))
                        return false;
                }
                scApp.VehicleService.Command.preMoveToSourcePort(bestSuitableVh, assign_cmd);
                //todo log...
                return false;
            }
            is_success = is_success && scApp.CMDBLL.checkCmd(assign_cmd);
            using (TransactionScope tx = SCUtility.getTransactionScope())
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    is_success = is_success && scApp.CMDBLL.addCmd(assign_cmd);
                    is_success = is_success && scApp.CMDBLL.updateTransferCmd_TranStatus2PreInitial(waittingExcuteMcsCmd.ID);
                    if (is_success)
                    {
                        tx.Complete();
                    }
                    else
                    {
                        CMDBLL.CommandCheckResult check_result = CMDBLL.getOrSetCallContext<CMDBLL.CommandCheckResult>(CMDBLL.CALL_CONTEXT_KEY_WORD_OHTC_CMD_CHECK_RESULT);
                        check_result.Result.AppendLine($" vh:{assign_cmd.VH_ID} creat command to db unsuccess.");
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(TransferService), Device: DEVICE_NAME_AGV,
                           Data: $"Assign transfer command fail.transfer id:{waittingExcuteMcsCmd.ID}",
                           Details: check_result.ToString(),
                           XID: check_result.Num);
                    }
                }
            }
            return is_success;
        }
        public (bool checkSuccess, string destinationPortID, string destinationAdrID) checkAndRenameDestinationPortIfAGVStation(ACMD assignCmd)
        {
            if (assignCmd.getTragetPortEQ(scApp.EqptBLL) is IAGVStationType)
            {
                IAGVStationType unload_agv_station = assignCmd.getTragetPortEQ(scApp.EqptBLL) as IAGVStationType;
                //bool is_ready_double_port = unload_agv_station.IsReadyDoubleUnload;
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(TransferService), Device: DEVICE_NAME_AGV,
                   Data: $"check agv station:{unload_agv_station.getAGVStationID()},IsCheckPortReady:{unload_agv_station.IsCheckPortReady}");
                bool is_ready = false;
                List<APORTSTATION> port_stations = new List<APORTSTATION>();
                if (unload_agv_station.IsCheckPortReady)
                {
                    is_ready = unload_agv_station.IsReadySingleUnload;
                    port_stations = unload_agv_station.loadReadyAGVStationPort();
                }
                else
                {
                    is_ready = unload_agv_station.HasPortAuto;
                    port_stations = unload_agv_station.loadAutoAGVStationPorts();
                }
                if (!is_ready)
                {
                    return (false, "", "");
                }
                foreach (var port in port_stations)
                {
                    if (DebugParameter.isNeedCheckPortUpDateTime &&
                        port.Timestamp < unload_agv_station.ReservedSuccessTime)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(TransferService), Device: DEVICE_NAME_AGV,
                           Data: $"port id:[{port.PORT_ID}] not update ready, update time:[{port.Timestamp.ToString(SCAppConstants.DateTimeFormat_23)}]," +
                                 $"last reserved success time:[{unload_agv_station.ReservedSuccessTime.ToString(SCAppConstants.DateTimeFormat_23)}]");
                        continue;
                    }
                    bool has_command_excute = cmdBLL.hasExcuteCMDByDestinationPort(port.PORT_ID);
                    if (!has_command_excute)
                    {
                        return (true, port.PORT_ID, port.ADR_ID);
                    }
                }
                //todo log
                return (false, "", "");
            }
            else
            {
                return (true, assignCmd.DESTINATION_PORT, assignCmd.DESTINATION);
            }
        }

        public (bool isSuccess, string result) CommandShift(string transferID, string vhID)
        {
            return (false, ""); //todo kevin 需要實作 Command shift功能。

            try
            {
                bool is_success = false;
                string result = "";
                //1. Cancel命令
                ATRANSFER mcs_cmd = scApp.CMDBLL.GetTransferByID(transferID);
                if (mcs_cmd == null)
                {
                    result = $"want to cancel/abort mcs cmd:{transferID},but cmd not exist.";
                    //LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_AGV,
                    //   Details: result,
                    //   XID: transferID);
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(TransferService), Device: DEVICE_NAME_AGV,
                    Data: result);


                    is_success = false;
                    return (is_success, result);
                }
                //當命令還沒被初始化(即尚未被送下去)或者已經為Transferring時(已經將貨物載到車上)，則不能進Command shift的動作
                if (mcs_cmd.TRANSFERSTATE < E_TRAN_STATUS.Initial || mcs_cmd.TRANSFERSTATE >= E_TRAN_STATUS.Transferring)
                {
                    result = $"want to excute command shift mcs cmd:{transferID},but current transfer state is:{mcs_cmd.TRANSFERSTATE}, can't excute.";
                    //LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_AGV,
                    //   Details: result,
                    //   XID: transferID);
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(TransferService), Device: DEVICE_NAME_AGV,
                    Data: result);
                    is_success = false;
                    return (is_success, result);
                }


                ACMD excute_cmd = scApp.CMDBLL.GetCommandByTransferCmdID(transferID);
                bool has_cmd_excute = excute_cmd != null;
                if (!has_cmd_excute)
                {
                    result = $"want to excute command shift mcs cmd:{transferID},but current not vh in excute.";
                    //LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_AGV,
                    //   Details: result,
                    //   XID: transferID);
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(TransferService), Device: DEVICE_NAME_AGV,
                    Data: result);



                    is_success = false;
                    return (is_success, result);
                }
                bool btemp = scApp.VehicleService.Send.Cancel(excute_cmd.VH_ID, excute_cmd.ID, ProtocolFormat.OHTMessage.CMDCancelType.CmdCancel);
                if (btemp)
                {
                    result = "OK";
                }
                else
                {
                    is_success = false;
                    result = $"Transfer command:[{transferID}] cancel failed.";
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_AGV,
                       Data: result,
                       XID: transferID);

                    return (is_success, result);
                }
                //2. Unassign Vehicle
                //3. 分派命令給新車(不能報command initial)
                //ATRANSFER ACMD_MCS = scApp.CMDBLL.GetTransferByID(mcs_id);
                //if (ACMD_MCS != null)
                //{
                //    bool check_result = true;
                //    result = "OK";
                //    //ACMD_MCS excute_cmd = ACMD_MCSs[0];
                //    string hostsource = ACMD_MCS.HOSTSOURCE;
                //    string hostdest = ACMD_MCS.HOSTDESTINATION;
                //    string from_adr = string.Empty;
                //    string to_adr = string.Empty;
                //    AVEHICLE vh = null;
                //    E_VH_TYPE vh_type = E_VH_TYPE.None;
                //    E_CMD_TYPE cmd_type = default(E_CMD_TYPE);

                //    //確認 source 是否為Port
                //    bool source_is_a_port = scApp.PortStationBLL.OperateCatch.IsExist(hostsource);
                //    if (source_is_a_port)
                //    {
                //        scApp.MapBLL.getAddressID(hostsource, out from_adr, out vh_type);
                //        vh = scApp.VehicleBLL.cache.getVehicle(vh_id);
                //        cmd_type = E_CMD_TYPE.LoadUnload;
                //    }
                //    else
                //    {
                //        result = "Source must be a port.";
                //        return false;
                //    }
                //    scApp.MapBLL.getAddressID(hostdest, out to_adr);
                //    if (vh != null)
                //    {
                //        if (vh.ACT_STATUS != VHActionStatus.Commanding)
                //        {
                //            bool temp = AssignMCSCommand2Vehicle(ACMD_MCS, cmd_type, vh);
                //            if (!temp)
                //            {
                //                result = "Assign command to vehicle failed.";
                //                return false;
                //            }
                //        }
                //        else
                //        {
                //            result = "Vehicle already have command.";
                //            return false;

                //        }

                //    }
                //    else
                //    {
                //        result = $"Can not find vehicle:{vh_id}.";
                //        return false;
                //    }
                //    return true;
                //}
                //else
                //{
                //    result = $"Can not find command:{mcs_id}.";
                //    return false;
                //}
            }
            finally
            {
                //System.Threading.Interlocked.Exchange(ref syncTranCmdPoint, 0);
            }
        }
        public (bool isSuccess, string result) FinishTransferCommand(string cmdID, CompleteStatus completeStatus)
        {
            try
            {
                scApp.CMDBLL.updateCMD_MCS_TranStatus2Complete(cmdID, completeStatus);
                VTRANSFER vtran = cmdBLL.getVCMD_MCSByID(cmdID);
                //scApp.CMDBLL.moveCMD_MCSToHistory(cmdID);//移往history
                scApp.ReportBLL.newReportTransferCommandForceFinish(vtran, completeStatus, null);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return (false, ex.ToString());
            }
            return (true, $"Force finish mcs command sucess.");
        }
        public (bool isSuccess, string result) tryInstallCarrierInVehicle(string vhID, string vhLocation, string carrierID)
        {
            try
            {
                //1.需確認該Carrier原本是不再車上車，且車上要有CST存在
                //1.1.如果原本就在車上就忽略該次的動作
                //1.2.如果原本不再車上則要將他Install到車上並且上報給CS
                AVEHICLE vh = scApp.VehicleBLL.cache.getVehicle(vhID);
                //Location location_info = vh.CarrierLocation.
                //                            Where(loc => SCUtility.isMatche(loc.ID, vhLocation)).
                //                            FirstOrDefault();
                if (!vh.HAS_CST)
                {
                    return (false, $"Location:{vhLocation} no carrier exist.");
                }

                var check_has_carrier_on_location_result = carrierBLL.db.hasCarrierOnVhLocation(vhLocation);
                if (check_has_carrier_on_location_result.has)
                {
                    if (SCUtility.isMatche(check_has_carrier_on_location_result.onVhCarrier.ID, carrierID))
                    {
                        return (false, $"Location:{vhLocation} is already carrier:{check_has_carrier_on_location_result.onVhCarrier.ID} exist.");
                    }
                }
                ACARRIER carrier = new ACARRIER()
                {
                    ID = carrierID,
                    LOT_ID = "",
                    INSER_TIME = DateTime.Now,
                    INSTALLED_TIME = DateTime.Now,
                    LOCATION = vhLocation,
                    STATE = ProtocolFormat.OHTMessage.E_CARRIER_STATE.Installed
                };
                carrierBLL.db.addOrUpdate(carrier);
                scApp.ReportBLL.newReportCarrierInstalled(vh.Real_ID, carrierID, vhLocation, null);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return (false, ex.ToString());
            }
            return (true, $"Install carrier:{carrierID} in location:{vhLocation} success.");
        }

        public (bool isSuccess, string result) ForceInstallCarrierInVehicle(string vhID, string carrierID)
        {
            try
            {
                AVEHICLE vh = scApp.VehicleBLL.cache.getVehicle(vhID);
                if(vh == null)
                {
                    return (false, $"Vehicle:{vhID} no found.");
                }
                return ForceInstallCarrierInVehicle(vhID, vh.Real_ID, carrierID);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return (false, ex.ToString());
            }
        }

        public (bool isSuccess, string result) ForceInstallCarrierInVehicle(string vhID, string vhLocation, string carrierID)
        {
            try
            {
                //1.需確認該Location目前是有貨的
                //3.需確認該Location目前是沒有帳的
                //2.需確認該Carrier目前沒有在線內
                AVEHICLE vh = scApp.VehicleBLL.cache.getVehicle(vhID);
                //Location location_info = vh.CarrierLocation.
                //                            Where(loc => SCUtility.isMatche(loc.ID, vhLocation)).
                //                            FirstOrDefault();
                if (!vh.HAS_CST)
                {
                    return (false, $"Location:{vhLocation} no carrier exist.");
                }
                if(!SCUtility.isMatche( vh.CST_ID, carrierID))
                {
                    return (false, $"vehicle current carrier id:[{vh.CST_ID}] do not match input carrier id:[{carrierID}].");
                }
                var check_has_carrier_on_location_result = carrierBLL.db.hasCarrierOnVhLocation(vhLocation);
                if (check_has_carrier_on_location_result.has)
                {
                    return (false, $"Location:{vhLocation} is already carrier:{check_has_carrier_on_location_result.onVhCarrier.ID} exist.");
                }
                var check_has_carrier_in_line_result = carrierBLL.db.hasCarrierInLine(carrierID);
                if (check_has_carrier_in_line_result.has)
                {
                    return (false, $"Carrier:{carrierID} is already in line current location in:{SCUtility.Trim(check_has_carrier_in_line_result.inLineCarrier.LOCATION, true)}.");
                }

                ACARRIER carrier = new ACARRIER()
                {
                    ID = carrierID,
                    LOT_ID = "",
                    INSER_TIME = DateTime.Now,
                    INSTALLED_TIME = DateTime.Now,
                    LOCATION = vhLocation,
                    STATE = ProtocolFormat.OHTMessage.E_CARRIER_STATE.Installed
                };
                carrierBLL.db.addOrUpdate(carrier);
                //scApp.ReportBLL.newReportCarrierInstalled(vh.Real_ID, carrierID, vhLocation, null);
                scApp.ReportBLL.ReportCarrierInstallCompleted(carrier, null);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return (false, ex.ToString());
            }
            return (true, $"Install carrier:{carrierID} in location:{vhLocation} success.");
        }
        public (bool isSuccess, string result) ForceRemoveCarrierInVehicleByOP(string carrierID)
        {
            try
            {
                var check_has_carrier_in_line_result = carrierBLL.db.hasCarrierInLine(carrierID);
                //if (!check_has_carrier_in_line_result.has)
                //{
                //    return (false, $"Carrier:{carrierID} is not in OHTC system.");
                //}

                carrierBLL.db.updateLocationAndState(carrierID, string.Empty, ProtocolFormat.OHTMessage.E_CARRIER_STATE.OpRemove);
                if (check_has_carrier_in_line_result.inLineCarrier != null)
                {
                    string current_location = check_has_carrier_in_line_result.inLineCarrier.LOCATION;
                    var location_of_vh = scApp.VehicleBLL.cache.getVehicleByLocationID(current_location);
                    if (location_of_vh != null)
                    {
                        scApp.ReportBLL.ReportCarrierRemovedCompleted(SCUtility.Trim(carrierID, true), SCUtility.Trim(location_of_vh.Real_ID, true), null);
                    }
                }


            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return (false, ex.ToString());
            }
            return (true, $"Remove carrier:{carrierID} is success.");
        }

        public (bool isSuccess, string result) processIDReadFailAndMismatch(string commandCarrierID, CompleteStatus completeStatus)
        {
            var check_has_carrier_in_line_result = carrierBLL.db.hasCarrierInLine(commandCarrierID);
            if (!check_has_carrier_in_line_result.has)
            {
                return (false, $"Carrier:{commandCarrierID} is not in OHTC system.");
            }
            E_CARRIER_STATE carrier_state = E_CARRIER_STATE.None;
            switch (completeStatus)
            {
                case CompleteStatus.CmpStatusIdmisMatch:
                    carrier_state = E_CARRIER_STATE.IdMismatch;
                    break;
                case CompleteStatus.CmpStatusIdreadFailed:
                    carrier_state = E_CARRIER_STATE.IdReadFail;
                    break;
            }




            //將原本的帳移除
            ACARRIER remove_carrier = check_has_carrier_in_line_result.inLineCarrier;
            ATRANSFER du_transfer = scApp.CMDBLL.getExcuteCMD_MCSByCarrierID(remove_carrier.RENAME_ID);
            if (du_transfer != null)//有同樣CarrierID的命令，要先取消掉再Install正確CST 20210517 MarkChou
            {
                if (du_transfer.TRANSFERSTATE == E_TRAN_STATUS.Queue)
                {
                    AbortOrCancel(du_transfer.ID, ProtocolFormat.OHTMessage.CMDCancelType.CmdCancel);
                }
                else
                {
                    AbortOrCancel(du_transfer.ID, ProtocolFormat.OHTMessage.CMDCancelType.CmdAbort);
                }
            }


            carrierBLL.db.updateLocationAndState(remove_carrier.ID, string.Empty, carrier_state);
            //var location_of_vh = scApp.VehicleBLL.cache.getVehicleByLocationID(remove_carrier.LOCATION);
            //scApp.ReportBLL.ReportCarrierRemovedCompleted(SCUtility.Trim(remove_carrier.ID, true), null);
            scApp.ReportBLL.ReportCarrierRemovedCompleted(SCUtility.Trim(remove_carrier.ID, true), SCUtility.Trim(remove_carrier.LOCATION, true), null);
            //建入Rename後的帳
            ACARRIER install_carrier = new ACARRIER()
            {
                ID = remove_carrier.RENAME_ID,
                LOT_ID = remove_carrier.LOT_ID,
                INSER_TIME = DateTime.Now,
                INSTALLED_TIME = DateTime.Now,
                LOCATION = remove_carrier.LOCATION,
                CSTType = "0",
                STATE = E_CARRIER_STATE.Installed
            };
            carrierBLL.db.addOrUpdate(install_carrier);
            //scApp.ReportBLL.newReportCarrierInstalled(location_of_vh.Real_ID, install_carrier.ID, install_carrier.LOCATION, null);
            scApp.ReportBLL.ReportCarrierInstallCompleted(install_carrier, null);
            return (true, $"process[{completeStatus}] success,remove carrier:{commandCarrierID} install:{install_carrier.ID}");
        }

        public bool isTransferCommandScanning()
        {
            return syncTranCmdPoint != 0;
        }

        public void ScanByVTransfer_v3(AVEHICLE finishCmdVh = null)
        {
            if (System.Threading.Interlocked.Exchange(ref syncTranCmdPoint, 1) == 0)
            {
                try
                {
                    bool is_trigger_by_vh_cmd_finish = finishCmdVh != null;
                    if (scApp.getEQObjCacheManager().getLine().ServiceMode
                        != SCAppConstants.AppServiceMode.Active)
                        return;
                    List<VTRANSFER> un_finish_trnasfer = scApp.TransferBLL.db.vTransfer.loadUnfinishedVTransfer()
                        .Where(cmd => !IsStagedPort(SCUtility.Trim(cmd.HOSTSOURCE))).ToList();
                    line.CurrentExcuteTransferCommand = un_finish_trnasfer;

                    Task.Run(() => queueTimeOutCheck(un_finish_trnasfer));
                    if (un_finish_trnasfer == null || un_finish_trnasfer.Count == 0) return;
                    //2021.11.22 Hsinyu Chang: MCS offline時也允許自己執行命令
                    if (DebugParameter.CanAutoRandomGeneratesCommand ||
                        (scApp.getEQObjCacheManager().getLine().SCStats == ALINE.TSCState.AUTO && scApp.getEQObjCacheManager().getLine().MCSCommandAutoAssign) ||
                        (scApp.getEQObjCacheManager().getLine().Host_Control_State == SCAppConstants.LineHostControlState.HostControlState.EQ_Off_line) ||
                        (scApp.getEQObjCacheManager().getLine().Host_Control_State == SCAppConstants.LineHostControlState.HostControlState.Host_Offline))
                    {
                        List<VTRANSFER> excuting_transfer = un_finish_trnasfer.
                            Where(tr => tr.TRANSFERSTATE > E_TRAN_STATUS.Queue &&
                                        tr.TRANSFERSTATE <= E_TRAN_STATUS.Transferring &&
                                        tr.CMDTYPE != ATRANSFER.CmdType.PortTypeChange.ToString() &&
                                        !SCUtility.isEmpty(tr.VH_ID)).
                                                    ToList();
                        List<VTRANSFER> in_queue_transfer = un_finish_trnasfer.
                                                    Where(tr => tr.TRANSFERSTATE == E_TRAN_STATUS.Queue &&
                                                    tr.CMDTYPE != ATRANSFER.CmdType.PortTypeChange.ToString()).
                                                    ToList();

                        try
                        {
                            foreach (VTRANSFER queue_tran in in_queue_transfer)
                            {
                                string hostsource = queue_tran.HOSTSOURCE;
                                string hostdest = queue_tran.HOSTDESTINATION;
                                string from_adr = string.Empty;
                                string to_adr = string.Empty;
                                AVEHICLE bestSuitableVh = null;
                                E_VH_TYPE vh_type = E_VH_TYPE.None;

                                //確認 source 是否為Port
                                bool source_is_a_port = scApp.PortStationBLL.OperateCatch.IsExist(hostsource);
                                if (source_is_a_port)
                                {

                                    ////需確認是否已經有其他車在搬送同SourcePort的CST，且還沒到Transferring
                                    //scApp.MapBLL.getAddressID(hostsource, out from_adr, out vh_type);

                                    ////如果觸發是由於車子在命令結束時而進入的且他所在的位置與該命令的source相同，
                                    ////就不用再去檢查是否有車子要來的條件
                                    ////因為就有機會去派給該台車
                                    //if (finishCmdVh != null && SCUtility.isMatche(from_adr, finishCmdVh.CUR_ADR_ID))
                                    //{
                                    //    //not thing...
                                    //}
                                    //else
                                    //{
                                    //    //如果已經有車子在往這個address移動的話，就先pass該筆命令找車子的動作，
                                    //    //等到移動結束時就使他可以找到該台車
                                    //    bool has_vh_will_go_source = cmdBLL.cache.hasMoveWillGoAdr(from_adr);
                                    //    if (has_vh_will_go_source)
                                    //    {
                                    //        continue;
                                    //    }
                                    //}
                                    bestSuitableVh = scApp.VehicleBLL.cache.findBestSuitableVhStepByStepFromAdr(scApp.GuideBLL, scApp.CMDBLL, from_adr, vh_type);
                                    //}
                                }
                                else
                                {
                                    //bestSuitableVh = scApp.VehicleBLL.cache.getVehicleByRealID(hostsource);
                                    bestSuitableVh = scApp.VehicleBLL.cache.getVehicleByLocationRealID(hostsource);
                                    if (bestSuitableVh.IsError ||
                                        bestSuitableVh.MODE_STATUS != VHModeStatus.AutoRemote ||
                                        bestSuitableVh.ACT_STATUS != VHActionStatus.NoCommand)
                                    {
                                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_AGV,
                                           Data: $"Has transfer command:{SCUtility.Trim(queue_tran.ID, true)} for vh:{bestSuitableVh.VEHICLE_ID}" +
                                                 $"but it error happend or not auto remote or not no command.",
                                           VehicleID: bestSuitableVh.VEHICLE_ID);
                                        continue;
                                    }
                                }

                                if (bestSuitableVh != null)
                                {
                                    if (AssignTransferCommmand(queue_tran, bestSuitableVh))
                                    {
                                        scApp.VehicleService.Command.Scan();
                                        return;
                                    }

                                    //bool isSourceOK = false;
                                    //bool isDestOK = false;
                                    //if (AreSourceEnable(queue_tran.HOSTSOURCE))
                                    //{
                                    //    isSourceOK = true;
                                    //}
                                    //if (AreDestEnable(queue_tran.HOSTSOURCE, queue_tran.HOSTDESTINATION))
                                    //{
                                    //    isDestOK = true;
                                    //}
                                    //if (isSourceOK && isDestOK)
                                    //{
                                    //    if (AssignTransferCommmand(queue_tran, bestSuitableVh))
                                    //    {
                                    //        scApp.VehicleService.Command.Scan();
                                    //        return;
                                    //    }
                                    //}

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, "Exception");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                }
                finally
                {
                    System.Threading.Interlocked.Exchange(ref syncTranCmdPoint, 0);
                }
            }
        }

        /// <summary>
        /// 選出可以順便帶走的命令
        ///1.找出正在執行的命令中，且他的命令是還沒到達Load Complete
        ///2.接著再去找目前在Queue命令中，目的地是是有相同AGV St./EQ的
        ///3.找到後即可將兩筆命令進行配對
        /// </summary>
        /// <param name="inQueueTransfers"></param>
        /// <param name="excutingTransfers"></param>
        /// <returns></returns>
        private (bool isFind, AVEHICLE bestSuitableVh, VTRANSFER bestSuitabletransfer) checkBeforeOnTheWay(List<VTRANSFER> inQueueTransfers, List<VTRANSFER> excutingTransfers)
        {
            AVEHICLE best_suitable_vh = null;
            VTRANSFER best_suitable_transfer = null;
            bool is_success = false;

            List<VTRANSFER> can_excute_after_on_the_way_tran = excutingTransfers.
                                                    //Where(tr => tr.COMMANDSTATE < ATRANSFER.COMMAND_STATUS_BIT_INDEX_LOAD_COMPLETE).
                                                    ToList();

            foreach (var excute_tran in can_excute_after_on_the_way_tran)
            {
                string source = excute_tran.HOSTSOURCE;
                string dest = excute_tran.HOSTDESTINATION;
                string best_suitable_vh_id = SCUtility.Trim(excute_tran.VH_ID, true);
                best_suitable_vh = scApp.VehicleBLL.cache.getVehicle(best_suitable_vh_id);

                if (!scApp.VehicleBLL.cache.canAssignTransferCmd(scApp.CMDBLL, best_suitable_vh, excute_tran.GetTransferDir()))
                {
                    best_suitable_vh = null;
                    continue;
                }

                string excute_tran_eq_id = SCUtility.Trim(excute_tran.getTragetPortNodeID(scApp.PortStationBLL, scApp.EqptBLL));
                //var same_eq_ports = inQueueTransfers.
                //                    Where(in_queue_tran => SCUtility.isMatche(in_queue_tran.getTragetPortEQID(scApp.PortStationBLL),
                //                                                              excute_tran_eq_id)).
                //                    ToList();
                var same_eq_ports = inQueueTransfers.
                                    Where(in_queue_tran => SCUtility.isMatche(in_queue_tran.getTragetPortNodeID(scApp.PortStationBLL, scApp.EqptBLL),
                                                                              excute_tran_eq_id)).
                                    ToList();

                var check_result = FindNearestTransferBySourcePort(excute_tran, same_eq_ports);
                if (check_result.isFind)
                {
                    best_suitable_transfer = check_result.nearestTransfer;
                    break;
                }
                else
                {
                    best_suitable_transfer = null;
                }
            }
            is_success = best_suitable_vh != null && best_suitable_transfer != null;
            return (is_success, best_suitable_vh, best_suitable_transfer);
        }
        /// <summary>
        /// 尋找可以命令結束後，可順便一起帶走的CST
        /// 1.找出尚未結束的命令且目標為AGV st的
        /// 2.再找出Queue命令-Source port = Excute命令-Target port
        /// 3.找到後確認AGV還可接收該Type的命令時，即可下達
        /// </summary>
        /// <param name="inQueueTransfers"></param>
        /// <param name="excutingTransfers"></param>
        /// <returns></returns>
        private (bool isFind, AVEHICLE bestSuitableVh, VTRANSFER bestSuitabletransfer) checkAfterOnTheWay(List<VTRANSFER> inQueueTransfers, List<VTRANSFER> excutingTransfers)
        {
            AVEHICLE best_suitable_vh = null;
            VTRANSFER best_suitable_transfer = null;
            bool is_success = false;
            List<VTRANSFER> can_excute_after_on_the_way_tran = excutingTransfers.Where(tran => tran.getTragetPortEQ(scApp.EqptBLL) is IAGVStationType
                                                                                            && tran.COMMANDSTATE > ATRANSFER.COMMAND_STATUS_BIT_INDEX_LOAD_COMPLETE).ToList();

            foreach (var excute_tran in can_excute_after_on_the_way_tran)
            {
                string source = excute_tran.HOSTSOURCE;
                string dest = excute_tran.HOSTDESTINATION;
                string best_suitable_vh_id = SCUtility.Trim(excute_tran.VH_ID, true);
                best_suitable_vh = scApp.VehicleBLL.cache.getVehicle(best_suitable_vh_id);

                if (!scApp.VehicleBLL.cache.canAssignTransferCmd(scApp.CMDBLL, best_suitable_vh, CMDBLL.CommandTranDir.OutAGVStation))
                {
                    best_suitable_vh = null;
                    continue;
                }

                string excute_tran_target_node_id = SCUtility.Trim(excute_tran.getTragetPortNodeID(scApp.PortStationBLL, scApp.EqptBLL));
                var same_node_tran = inQueueTransfers.
                                    Where(in_queue_tran => SCUtility.isMatche(in_queue_tran.getSourcePortNodeID(scApp.PortStationBLL, scApp.EqptBLL),
                                                                              excute_tran_target_node_id)).
                                    ToList();

                var check_result = FindNearestTransferByTargetPort(excute_tran, same_node_tran);
                if (check_result.isFind)
                {
                    best_suitable_transfer = check_result.nearestTransfer;
                    break;
                }
                else
                {
                    best_suitable_transfer = null;
                }
            }
            is_success = best_suitable_vh != null && best_suitable_transfer != null;
            return (is_success, best_suitable_vh, best_suitable_transfer);
        }


        bool AssignTransferCommmand(VTRANSFER waittingExcuteMcsCmd, AVEHICLE bestSuitableVh)
        {
            var tran_assigner = GetTranAssigner(bestSuitableVh.VEHICLE_TYPE);
            return tran_assigner.AssignTransferToVehicle(waittingExcuteMcsCmd, bestSuitableVh);
        }
        ITranAssigner GetTranAssigner(E_VH_TYPE vhType)
        {
            switch (vhType)
            {
                //case E_VH_TYPE.Swap:
                //    return SwapTranAssigner;
                default:
                    return NormalTranAssigner;
            }

        }

        public virtual bool ManualCommand(string vhID, string cstID, string source, string destination, string sourcePortID, string destinationPortID)
        {
            return scApp.VehicleService.Command.Loadunload(vhID, cstID, source, destination, sourcePortID, destinationPortID);
        }

        public bool AreSourceEnable(string sourceName)  //檢查來源狀態是否正確
        {
            try
            {
                sourceName = sourceName.Trim();
                bool sourcePortType = false;
                string sourceState = "";

                AVEHICLE vehicle = scApp.getEQObjCacheManager().getVehicletByRealID(sourceName);
                if (vehicle != null)
                {
                    if (vehicle.HAS_CST)
                    {
                        sourcePortType = true;
                        return true;
                    }
                    else
                    {
                        sourcePortType = false;
                        return false;
                    }
                }


                APORTSTATION portStation = scApp.getEQObjCacheManager().getPortStation(sourceName);
                if (portStation == null)
                {
                    return false; //找不到對應Port，無法執行
                }
                else
                {
                    PortValueDefMapAction mapAction = portStation.getMapActionByIdentityKey(typeof(PortValueDefMapAction).Name) as PortValueDefMapAction;
                    if (mapAction == null)
                    {
                        return true; //沒有MapAction 代表是EQ Port，不必檢查直接放行
                    }
                    else
                    {
                        if (portStation.IsAutoMode)
                        {
                            if (portStation.IsReadyToUnload)
                            {
                                if (portStation.IsInPutMode)
                                {
                                    sourcePortType = true;
                                }
                                else
                                {
                                    sourceState = sourceState + " IsInputMode:" + portStation.IsInPutMode;
                                }
                            }
                            else
                            {
                                if (portStation.IsInPutMode == false
                                    && portStation.IsModeChangeable
                                   )
                                {
                                    string cmdID = "PortTypeChange-" + portStation.PORT_ID.Trim() + ">>" + E_PortType.In;

                                    if (cmdBLL.GetTransferByID(cmdID) == null)
                                    {
                                        //若來源流向錯誤且沒有流向切換命令，就新建
                                        SetPortTypeCmd(portStation.PORT_ID.Trim(), E_PortType.In);  //要測時，把註解拿掉
                                    }
                                }

                                sourceState = sourceState + " IsReadyToUnload: " + portStation.IsReadyToUnload + " IsInputMode: " + portStation.IsInPutMode;
                            }
                        }
                        else
                        {
                            sourceState = sourceState + " OpAutoMode:" + portStation.IsAutoMode;
                        }
                    }
                }

                return sourcePortType;
            }
            catch (Exception ex)
            {
                TransferServiceLogger.Error(ex, "AreSourceEnable");
                return false;
            }
        }
        public bool AreDestEnable(string sourceName, string destName)    //檢查目的狀態是否正確
        {
            try
            {
                destName = destName.Trim();
                bool destPortType = false;
                string destState = "";

                AVEHICLE sourceVehicle = scApp.getEQObjCacheManager().getVehicletByRealID(sourceName);
                if (sourceVehicle != null)//如果命令起點是車子，不檢查目的Port狀態，直接執行 20210521
                {
                    destPortType = true;
                    return true;
                }
                AVEHICLE vehicle = scApp.getEQObjCacheManager().getVehicletByRealID(destName);
                if (vehicle != null)
                {
                    if (vehicle.HAS_CST)
                    {
                        destPortType = false;
                        return false;
                    }
                    else
                    {
                        destPortType = true;
                        return true;
                    }
                }


                #region 檢查目的 Port 流向   

                APORTSTATION portStation = scApp.getEQObjCacheManager().getPortStation(destName);
                if (portStation == null)
                {
                    return false; //找不到對應Port，無法執行
                }
                else
                {
                    PortValueDefMapAction mapAction = portStation.getMapActionByIdentityKey(typeof(PortValueDefMapAction).Name) as PortValueDefMapAction;
                    if (mapAction == null)
                    {
                        return true; //沒有MapAction 代表是EQ Port，不必檢查直接放行
                    }
                    else
                    {
                        int command_count = cmdBLL.GetCmdDataByDest(destName).Where(data => data.TRANSFERSTATE > E_TRAN_STATUS.Queue && data.TRANSFERSTATE < E_TRAN_STATUS.Complete).Count();

                        if (portStation.IsAutoMode)
                        {
                            //if (portStation.IsReadyToLoad)
                            //{

                            if (portStation.IsOutPutMode)
                            {
                                if (portStation.stageCount == 1)//PORT只有一節的情況
                                {
                                    if ((portStation.stageCount) > portStation.CSTCount + command_count)
                                    {
                                        destPortType = true;
                                    }
                                }
                                else
                                {
                                    //if ((portStation.stageCount - 1) > portStation.CSTCount + command_count) //20210528先把減一移除以增進效率
                                    if ((portStation.stageCount) > portStation.CSTCount + command_count)
                                    {
                                        destPortType = true;
                                    }
                                }

                            }
                            else
                            {
                                destState = destState + " IsOutputMode:" + portStation.IsOutPutMode;
                            }

                            //}
                            //else
                            //{
                            if (portStation.IsOutPutMode == false
                                    && portStation.IsModeChangeable
                                   )
                            {
                                string cmdID = "PortTypeChange-" + portStation.PORT_ID.Trim() + ">>" + E_PortType.Out;
                                if (cmdBLL.GetTransferByID(cmdID) == null)
                                {
                                    //若來源流向錯誤且沒有流向切換命令，就新建
                                    SetPortTypeCmd(portStation.PORT_ID.Trim(), E_PortType.Out);  //要測時，把註解拿掉
                                }
                            }

                            destState = destState + " IsReadyToLoad: " + portStation.IsReadyToLoad + " IsOutputMode: " + portStation.IsOutPutMode;
                            //}
                        }
                        else
                        {
                            destState = destState + " OpAutoMode:" + portStation.IsAutoMode;
                        }
                    }


                }

                #endregion

                //if (destPortType == false)
                //{
                //    TimeSpan timeSpan = DateTime.Now - portINIData[destName].portStateErrorLogTime;

                //    if (timeSpan.TotalSeconds >= 10)
                //    {
                //        portINIData[destName].portStateErrorLogTime = DateTime.Now;

                //        TransferServiceLogger.Info
                //        (
                //            DateTime.Now.ToString("HH:mm:ss.fff ") +
                //            "OHB >> PLC|目的 " + destName + " 狀態錯誤 " + destState
                //        );
                //    }
                //}

                return destPortType;
            }
            catch (Exception ex)
            {
                TransferServiceLogger.Error(ex, "AreDestEnable");
                return false;
            }
        }
        public bool PortTimeoutSetting(string portID, int t1_timeout, int t2_timeout, int t3_timeout, int t4_timeout, int t5_timeout, int t6_timeout)
        {
            try
            {
                TransferServiceLogger.Info
                (
                    DateTime.Now.ToString("HH:mm:ss.fff ") +
                    "OHB >> PLC|PortTimeoutSetting"
                    + "    portID:" + portID
                    + "    T1 Timeout:" + t1_timeout
                    + "    T2 Timeout:" + t2_timeout
                    + "    T3 Timeout:" + t3_timeout
                    + "    T4 Timeout:" + t4_timeout
                    + "    T5 Timeout:" + t5_timeout
                    + "    T6 Timeout:" + t6_timeout
                );

                PLCSystemInfoMapAction plcystemInfoMapAction = scApp.getEQObjCacheManager().getPortByPortID(portID).getMapActionByIdentityKey(typeof(PLCSystemInfoMapAction).Name) as PLCSystemInfoMapAction;

                plcystemInfoMapAction.Port_Timeout_Set(t1_timeout, t2_timeout, t3_timeout, t4_timeout, t5_timeout, t6_timeout);
                return true;
            }
            catch (Exception ex)
            {
                TransferServiceLogger.Error(ex, "PortTimeoutSetting");
                return false;
            }
        }
        public bool PortBCR_Enable(string portID, bool enable)
        {
            try
            {
                TransferServiceLogger.Info
                (
                    DateTime.Now.ToString("HH:mm:ss.fff ") +
                    "OHB >> PLC|PortBCR_Enable"
                    + "    portID:" + portID
                    + "    Enable:" + enable
                );

                PortValueDefMapAction portValueDefMapAction = scApp.getEQObjCacheManager().getPortStation(portID).getMapActionByIdentityKey(typeof(PortValueDefMapAction).Name) as PortValueDefMapAction;

                //portValueDefMapAction.Port_BCR_Enable(enable);
                portValueDefMapAction.Port_BCR_Disable(!enable); // PLC D5016-00 定義為 IO Port BCR Disable, 所以送!enable
                return true;
            }
            catch (Exception ex)
            {
                TransferServiceLogger.Error(ex, "toAGV_Mode");
                return false;
            }
        }
        public void ReportPortType(string portID, E_PortType portType, string cmdSource)
        {
            try
            {
                portID = portID.Trim();
                TransferServiceLogger.Info
                (
                    DateTime.Now.ToString("HH:mm:ss.fff ") +
                    "PLC >> OHB|ReportPortType"
                    + " portID:" + portID
                    + " inout:" + portType
                    + " 誰呼叫:" + cmdSource
                );


                if (reportBLL != null)
                {
                    reportBLL.newReportPortModeChange(portID, null);
                    //reportBLL.ReportPortTypeChanging(portID);

                    if (portType == E_PortType.In)
                    {
                        reportBLL.newReportPortInMode(portID, null);
                        //reportBLL.ReportTypeInput(portID);
                    }
                    else if (portType == E_PortType.Out)
                    {
                        reportBLL.newReportPortOutMode(portID, null);
                        //reportBLL.ReportPortTypeOutput(portID);
                    }
                }


            }
            catch (Exception ex)
            {
                TransferServiceLogger.Error(ex, "ReportPortType");
            }
        }
        public bool PortTypeChange(string portID, E_PortType mode, string apiSource)
        {
            try
            {
                TransferServiceLogger.Info
                (
                    DateTime.Now.ToString("HH:mm:ss.fff ")
                    + "OHB >> PLC|PortTypeChange"
                    + "    誰呼叫:" + apiSource
                    + "    portID:" + portID
                    + "    inout:" + mode
                );

                //PortPLCInfo plcInfo = GetPLC_PortData(portID);
                portID = portID.Trim();
                APORTSTATION portStation = scApp.getEQObjCacheManager().getPortStation(portID);
                if (portStation == null)
                {
                    return false;
                }
                PortValueDefMapAction portValueDefMapAction = portStation.getMapActionByIdentityKey(typeof(PortValueDefMapAction).Name) as PortValueDefMapAction;
                if (portValueDefMapAction == null)
                {
                    return false;
                }
                bool typeEnable = portStation.IsModeChangeable;

                if (typeEnable == false)
                {
                    TransferServiceLogger.Info
                    (
                        DateTime.Now.ToString("HH:mm:ss.fff ") +
                        "PLC >> OHB|目前不能切流向 IsModeChangable = " + typeEnable
                    );

                    string cmdID = "PortTypeChange-" + portID + ">>" + mode;

                    if (cmdBLL.getTransferCmdIDByCmdID(cmdID) != null)
                    {
                        return true;
                    }

                    SetPortTypeCmd(portID, mode);

                    return true;
                }

                if (mode == E_PortType.In)
                {
                    //portValueDefMapAction.Port_ChangeToOutput(false);
                    portValueDefMapAction.Port_ChangeToInput(true);
                    TransferServiceLogger.Info
                    (
                        DateTime.Now.ToString("HH:mm:ss.fff ") +
                        "OHB >> PLC|對"
                        + " PortID:" + portID
                        + " InMode: true"
                        + " OutMode: False"
                        + " 目前狀態 InputMode:" + portStation.IsInPutMode + "  OutputMode:" + portStation.IsOutPutMode
                    );

                    if (portStation.IsInPutMode)
                    {
                        ReportPortType(portID, mode, "PortTypeChange");
                    }
                }
                else if (mode == E_PortType.Out)
                {
                    //portValueDefMapAction.Port_ChangeToInput(false);
                    portValueDefMapAction.Port_ChangeToOutput(true);
                    TransferServiceLogger.Info
                    (
                        DateTime.Now.ToString("HH:mm:ss.fff ") +
                        "OHB >> PLC|對"
                        + " PortID:" + portID
                        + " InMode: False"
                        + " OutMode: true "
                        + " 目前狀態 InputMode:" + portStation.IsInPutMode + "  OutputMode:" + portStation.IsOutPutMode
                    );

                    if (portStation.IsOutPutMode)
                    {
                        ReportPortType(portID, mode, "PortTypeChange");
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                TransferServiceLogger.Error(ex, "PortTypeChange");
                return false;
            }
        }
        public bool SetPortTypeCmd(string portName, E_PortType type)    //新增控制流向命令
        {
            try
            {
                portName = portName.Trim();

                TransferServiceLogger.Info
                (
                    DateTime.Now.ToString("HH:mm:ss.fff ") +
                    "OHB >> OHB|SetPortTypeCmd 新增切流向命令 portID:" + portName + "    inout:" + type
                );

                ATRANSFER datainfo = new ATRANSFER();

                datainfo.ID = "PortTypeChange-" + portName + ">>" + type;
                datainfo.CARRIER_ID = "";
                //datainfo.BOX_ID = "";

                datainfo.HOSTSOURCE = portName;
                datainfo.HOSTDESTINATION = type.ToString();

                datainfo.CMDTYPE = ATRANSFER.CmdType.PortTypeChange.ToString();

                if (cmdBLL.GetTransferByID(datainfo.ID) != null)
                {
                    return false;
                }

                datainfo.LOT_ID = "";
                datainfo.CMD_INSER_TIME = DateTime.Now;
                datainfo.TRANSFERSTATE = E_TRAN_STATUS.Queue;
                datainfo.COMMANDSTATE = ATRANSFER.COMMAND_iIdle;
                datainfo.PRIORITY = 50;
                datainfo.CHECKCODE = SECSConst.HCACK_Confirm;
                datainfo.PAUSEFLAG = "";
                datainfo.TIME_PRIORITY = 0;
                datainfo.PORT_PRIORITY = 0;
                datainfo.REPLACE = 1;
                datainfo.PRIORITY_SUM = datainfo.PRIORITY + datainfo.TIME_PRIORITY + datainfo.PORT_PRIORITY;
                //datainfo.CRANE = "";

                if (cmdBLL.GetTransferByID(datainfo.ID) == null)
                {
                    SimpleCreate(datainfo);
                    //cmdBLL.DeleteCmd(datainfo.CMD_ID);
                }

                return true;
            }
            catch (Exception ex)
            {
                TransferServiceLogger.Error(ex, "SetPortTypeCmd");
                return false;
            }
        }
        public bool SetPortRun(string portID)
        {
            try
            {
                TransferServiceLogger.Info
                (
                    DateTime.Now.ToString("HH:mm:ss.fff ") +
                    "OHB >> PLC|SetPortRun"
                    + "    portID:" + portID
                );

                PortValueDefMapAction portValueDefMapAction = scApp.getEQObjCacheManager().getPortStation(portID).getMapActionByIdentityKey(typeof(PortValueDefMapAction).Name) as PortValueDefMapAction;

                portValueDefMapAction.Port_RUN();
                return true;
            }
            catch (Exception ex)
            {
                TransferServiceLogger.Error(ex, "SetPortRun");
                return false;
            }
        }
        public bool SetPortStop(string portID)
        {
            try
            {
                TransferServiceLogger.Info
                (
                    DateTime.Now.ToString("HH:mm:ss.fff ") +
                    "OHB >> PLC|SetPortStop"
                    + "    portID:" + portID
                );

                PortValueDefMapAction portValueDefMapAction = scApp.getEQObjCacheManager().getPortStation(portID).getMapActionByIdentityKey(typeof(PortValueDefMapAction).Name) as PortValueDefMapAction;

                portValueDefMapAction.Port_STOP();
                return true;
            }
            catch (Exception ex)
            {
                TransferServiceLogger.Error(ex, "SetPortStop");
                return false;
            }
        }
        public bool SetPortBCR_Read(string portID)
        {
            try
            {
                TransferServiceLogger.Info
                (
                    DateTime.Now.ToString("HH:mm:ss.fff ") +
                    "OHB >> PLC|SetPortBCR_Read"
                    + "    portID:" + portID
                );

                PortValueDefMapAction portValueDefMapAction = scApp.getEQObjCacheManager().getPortStation(portID).getMapActionByIdentityKey(typeof(PortValueDefMapAction).Name) as PortValueDefMapAction;

                portValueDefMapAction.Port_BCR_Read(true);
                Thread.Sleep(500);
                portValueDefMapAction.Port_BCR_Read(false);
                return true;
            }
            catch (Exception ex)
            {
                TransferServiceLogger.Error(ex, "SetPortBCR_Read");
                return false;
            }
        }
        public bool RstPortBCR_Read(string portID)
        {
            try
            {
                TransferServiceLogger.Info
                (
                    DateTime.Now.ToString("HH:mm:ss.fff ") +
                    "OHB >> PLC|RstPortBCR_Read"
                    + "    portID:" + portID
                );

                PortValueDefMapAction portValueDefMapAction = scApp.getEQObjCacheManager().getPortStation(portID).getMapActionByIdentityKey(typeof(PortValueDefMapAction).Name) as PortValueDefMapAction;

                portValueDefMapAction.Port_BCR_Read(false);
                return true;
            }
            catch (Exception ex)
            {
                TransferServiceLogger.Error(ex, "RstPortBCR_Read");
                return false;
            }
        }
        public bool PortAlarrmReset(string portID)
        {
            try
            {
                TransferServiceLogger.Info
                (
                    DateTime.Now.ToString("HH:mm:ss.fff ") +
                    "OHB >> PLC|PortAlarrmReset"
                    + "    portID:" + portID
                );

                PortValueDefMapAction portValueDefMapAction = scApp.getEQObjCacheManager().getPortStation(portID).getMapActionByIdentityKey(typeof(PortValueDefMapAction).Name) as PortValueDefMapAction;

                portValueDefMapAction.Port_PortAlarrmReset(true);
                Thread.Sleep(2000);
                portValueDefMapAction.Port_PortAlarrmReset(false);
                return true;
            }
            catch (Exception ex)
            {
                TransferServiceLogger.Error(ex, "PortAlarrmReset");
                return false;
            }
        }
        public bool assignCSTIDtoPort(string portID, string cst_id)
        {
            try
            {
                TransferServiceLogger.Info
                (
                    DateTime.Now.ToString("HH:mm:ss.fff ") +
                    "OHB >> PLC|assignCSTIDtoPort"
                    + "    portID:" + portID
                    + "    Cassette ID:" + cst_id
                );
                APORTSTATION port = scApp.getEQObjCacheManager().getPortStation(portID);
                if (port != null)
                {
                    PortValueDefMapAction portValueDefMapAction = port.getMapActionByIdentityKey(typeof(PortValueDefMapAction).Name) as PortValueDefMapAction;
                    if (portValueDefMapAction != null)
                    {
                        portValueDefMapAction.Port_AssignCSTID(cst_id);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                TransferServiceLogger.Error(ex, "assignCSTIDtoPort");
                return false;
            }
        }


        public bool PortCommanding(string portID, bool Commanding)  //通知PLC有命令要過去，不能切換流向
        {
            try
            {
                TransferServiceLogger.Info
                (
                    DateTime.Now.ToString("HH:mm:ss.fff ") +
                    "OHB >> PLC|PortCommanding"
                    + "    portID:" + portID
                    + "    Commanding:" + Commanding
                );
                portID = portID.Trim();
                APORTSTATION portStation = scApp.getEQObjCacheManager().getPortStation(portID);
                if (portStation == null)
                {
                    return false;
                }
                PortValueDefMapAction portValueDefMapAction = portStation.getMapActionByIdentityKey(typeof(PortValueDefMapAction).Name) as PortValueDefMapAction;
                if (portValueDefMapAction == null)
                {
                    return false;
                }
                portValueDefMapAction.Port_OHCV_Commanding(Commanding);

                return true;
            }
            catch (Exception ex)
            {
                TransferServiceLogger.Error(ex, "Port_OHCV_Commanding portID:" + portID + " Commanding:" + Commanding);
                return false;
            }
        }

        //public bool SetPortTypeCmd(string portName, E_PortType type)    //新增控制流向命令
        //{
        //    try
        //    {
        //        portName = portName.Trim();

        //        TransferServiceLogger.Info
        //        (
        //            DateTime.Now.ToString("HH:mm:ss.fff ") +
        //            "OHB >> OHB|SetPortTypeCmd 新增切流向命令 portID:" + portName + "    inout:" + type
        //        );

        //        ACMD_MCS datainfo = new ACMD_MCS();

        //        datainfo.CMD_ID = "PortTypeChange-" + portName + ">>" + type;
        //        datainfo.CARRIER_ID = "";
        //        datainfo.BOX_ID = "";

        //        datainfo.HOSTSOURCE = portName;
        //        datainfo.HOSTDESTINATION = type.ToString();

        //        datainfo.CMDTYPE = CmdType.PortTypeChange.ToString();

        //        if (cmdBLL.getNowCMD_MCSByID(datainfo.CMD_ID) != null)
        //        {
        //            return false;
        //        }

        //        datainfo.LOT_ID = "";
        //        datainfo.CMD_INSER_TIME = DateTime.Now;
        //        datainfo.TRANSFERSTATE = E_TRAN_STATUS.Queue;
        //        datainfo.COMMANDSTATE = ACMD_MCS.COMMAND_iIdle;
        //        datainfo.PRIORITY = 50;
        //        datainfo.CHECKCODE = "";
        //        datainfo.PAUSEFLAG = "";
        //        datainfo.TIME_PRIORITY = 0;
        //        datainfo.PORT_PRIORITY = 0;
        //        datainfo.REPLACE = 1;
        //        datainfo.PRIORITY_SUM = datainfo.PRIORITY + datainfo.TIME_PRIORITY + datainfo.PORT_PRIORITY;
        //        datainfo.CRANE = "";

        //        if (cmdBLL.getCMD_MCSByID(datainfo.CMD_ID) == null)
        //        {
        //            cmdBLL.creatCommand_MCS(datainfo);
        //            //cmdBLL.DeleteCmd(datainfo.CMD_ID);
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        TransferServiceLogger.Error(ex, "SetPortTypeCmd");
        //        return false;
        //    }
        //}
        /*
        #region PLC 控制命令

        public bool SetPortTypeCmd(string portName, E_PortType type)    //新增控制流向命令
        {
            try
            {
                portName = portName.Trim();

                TransferServiceLogger.Info
                (
                    DateTime.Now.ToString("HH:mm:ss.fff ") +
                    "OHB >> OHB|SetPortTypeCmd 新增切流向命令 portID:" + portName + "    inout:" + type
                );

                ACMD_MCS datainfo = new ACMD_MCS();

                datainfo.CMD_ID = "PortTypeChange-" + portName + ">>" + type;
                datainfo.CARRIER_ID = "";
                datainfo.BOX_ID = "";

                datainfo.HOSTSOURCE = portName;
                datainfo.HOSTDESTINATION = type.ToString();

                datainfo.CMDTYPE = CmdType.PortTypeChange.ToString();

                if (cmdBLL.getNowCMD_MCSByID(datainfo.CMD_ID) != null)
                {
                    return false;
                }

                datainfo.LOT_ID = "";
                datainfo.CMD_INSER_TIME = DateTime.Now;
                datainfo.TRANSFERSTATE = E_TRAN_STATUS.Queue;
                datainfo.COMMANDSTATE = ACMD_MCS.COMMAND_iIdle;
                datainfo.PRIORITY = 50;
                datainfo.CHECKCODE = "";
                datainfo.PAUSEFLAG = "";
                datainfo.TIME_PRIORITY = 0;
                datainfo.PORT_PRIORITY = 0;
                datainfo.REPLACE = 1;
                datainfo.PRIORITY_SUM = datainfo.PRIORITY + datainfo.TIME_PRIORITY + datainfo.PORT_PRIORITY;
                datainfo.CRANE = "";

                if (cmdBLL.getCMD_MCSByID(datainfo.CMD_ID) == null)
                {
                    cmdBLL.creatCommand_MCS(datainfo);
                    //cmdBLL.DeleteCmd(datainfo.CMD_ID);
                }

                return true;
            }
            catch (Exception ex)
            {
                TransferServiceLogger.Error(ex, "SetPortTypeCmd");
                return false;
            }
        }

        public string OpenAGV_Station(string portName, bool open, string sourceCmd)
        {
            if (portINIData[portName].openAGV_Station != open)
            {
                TransferServiceLogger.Info
                (
                    DateTime.Now.ToString("HH:mm:ss.fff ") +
                    "OHB >> OHB|開關自動退補BOX功能 portID:" + portName + " 動作:" + open + " 誰呼叫: " + sourceCmd
                );
            }

            portName = portName.Trim();
            portINIData[portName].openAGV_Station = open;

            return GetAGV_StationStatus(portName);
        }

        public string OpenAGV_AutoPortType(string portName, bool open)
        {
            portName = portName.Trim();
            portINIData[portName].openAGV_AutoPortType = open;

            return GetAGV_AutoPortType(portName);
        }

        public string GetAGV_StationStatus(string portName)
        {
            portName = portName.Trim();
            return portINIData[portName].openAGV_Station.ToString();
        }
        public string GetAGV_AutoPortType(string portName)
        {
            portName = portName.Trim();
            return portINIData[portName].openAGV_AutoPortType.ToString();
        }
        public string GetCVPortHelp(string portName)   //取得狀態說明
        {
            PortPLCInfo plcInof = GetPLC_PortData(portName);
            string log = "狀態：\n";
            if (isUnitType(plcInof.EQ_ID, UnitType.AGV))
            {
                if (plcInof.IsReadyToLoad == false && plcInof.IsReadyToUnload == false)
                {
                    log = log + "IsReadyToLoad 與 IsReadyToLoad 為 False";
                }

                if (plcInof.IsInputMode)
                {

                }

                if (plcInof.IsOutputMode)
                {

                }
            }
            else
            {
                if (plcInof.OpAutoMode)
                {
                    log = log + E_PORT_STATUS.InService.ToString();
                }
                else
                {
                    log = log + E_PORT_STATUS.OutOfService.ToString();
                }
            }
            return log;
        }
        #endregion
        */
        public void PLC_ReportPortWaitIn(PortPLCInfo plcInfo, string sourceCmd)
        {
            try
            {

                ACARRIER cstData = new ACARRIER();
                if (!string.IsNullOrWhiteSpace(plcInfo.RFIDCassetteID))
                {
                    cstData.ID = SCUtility.Trim(plcInfo.RFIDCassetteID, true);        //填CSTID
                }
                else
                {
                    cstData.ID = $"UNKF{plcInfo.EQ_ID.Trim()}{DateTime.Now.ToString(SCAppConstants.TimestampFormat_12)}";// 美微說如果PLC讀不到就報UNK上去
                }
                cstData.INSER_TIME = DateTime.Now;
                cstData.LOCATION = plcInfo.EQ_ID.Trim();  //填PortID
                cstData.CSTState = E_CSTState.Installed;
                cstData.CSTInDT = DateTime.Now.ToString("yy/MM/dd HH:mm:ss");
                cstData.READ_STATUS = E_ID_READ_STSTUS.Successful;
                cstData.STATE = E_CARRIER_STATE.WaitIn;
                cstData.Stage = 1;

                ACARRIER cst = carrierBLL.db.getCarrier(cstData.ID);
                if (cst == null || cst.CSTState != E_CSTState.WaitIn || !SCUtility.isMatche(cst.LOCATION, cstData.LOCATION))
                {
                    carrierBLL.db.addOrUpdate(cstData);
                    reportBLL.ReportCarrierIDRead(cstData, "0", null);
                    reportBLL.ReportCarrierWaitIn(cstData);
                }
                else
                {
                    //已經建帳了，無須進行上報
                    TransferServiceLogger.Info
                    (
                        DateTime.Now.ToString("HH:mm:ss.fff ") +
                        "PLC_ReportPortWaitIn decide not to report wait in."
                        + " CST_ID:" + cst.ID
                        + " CSTState:" + cst.CSTState
                        + " LOCATION:" + cst.LOCATION
                    );
                }
            }
            catch (Exception ex)
            {
                TransferServiceLogger.Error(ex, "PLC_ReportPortWaitIn");
            }
        }

        public void PortPositionWaitOut(ACARRIER datainfo, int outStage, string sourceCmd = "PLC")
        {
            try
            {
                UPStage(datainfo, outStage, "PortPositionWaitOut");
            }
            catch (Exception ex)
            {
                TransferServiceLogger.Error(ex, "PortPositionWaitOut");
            }
        }

        public bool UPStage(ACARRIER outData, int outStage, string sourceCmd)
        {
            try
            {

                TransferServiceLogger.Info
                (
                    DateTime.Now.ToString("HH:mm:ss.fff ") +
                    "PLC >> OHB|UPStage PortID:" + outData.LOCATION
                    + " 誰呼叫:" + sourceCmd
                    + " outStage:" + outStage
                );
                reportBLL.ReportCarrierWaitOut(outData, "1");

                return true;
            }
            catch (Exception ex)
            {
                TransferServiceLogger.Error(ex, "UPStage");
                return false;
            }
        }

        public void portInServeice(string port_id)
        {
            APORTSTATION aPORTSTATION = scApp.PortStationBLL.OperateCatch.getPortStation(port_id);
            if (aPORTSTATION != null)
            {
                if (aPORTSTATION.PORT_SERVICE_STATUS == sc.ProtocolFormat.OHTMessage.PortStationServiceStatus.InService) return;
                bool result = scApp.PortStationBLL.OperateDB.updateServiceStatus(port_id, sc.ProtocolFormat.OHTMessage.PortStationServiceStatus.InService);
                if (result)
                {
                    scApp.PortStationBLL.OperateCatch.updateServiceStatus(port_id, sc.ProtocolFormat.OHTMessage.PortStationServiceStatus.InService);
                    List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
                    reportBLL.newReportPortInServeice(port_id, reportqueues);
                    reportBLL.newSendMCSMessage(reportqueues);
                }
            }
        }
        public void portOutServeice(string port_id)
        {
            APORTSTATION aPORTSTATION = scApp.PortStationBLL.OperateCatch.getPortStation(port_id);
            if (aPORTSTATION != null)
            {
                if (aPORTSTATION.PORT_SERVICE_STATUS == sc.ProtocolFormat.OHTMessage.PortStationServiceStatus.OutOfService) return;
                bool result = scApp.PortStationBLL.OperateDB.updateServiceStatus(port_id, sc.ProtocolFormat.OHTMessage.PortStationServiceStatus.OutOfService);
                if (result)
                {
                    scApp.PortStationBLL.OperateCatch.updateServiceStatus(port_id, sc.ProtocolFormat.OHTMessage.PortStationServiceStatus.OutOfService);
                    List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
                    reportBLL.newReportPortOutServeice(port_id, reportqueues);
                    reportBLL.newSendMCSMessage(reportqueues);
                }
            }
        }

        public void portOutMode(string port_id)
        {

            List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
            reportBLL.newReportPortModeChange(port_id, reportqueues);
            reportBLL.newReportPortOutMode(port_id, reportqueues);
            reportBLL.newSendMCSMessage(reportqueues);

        }
        public void portInMode(string port_id)
        {
            List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
            reportBLL.newReportPortModeChange(port_id, reportqueues);
            reportBLL.newReportPortInMode(port_id, reportqueues);
            reportBLL.newSendMCSMessage(reportqueues);
        }


        public void DeleteOHCVPortCst(string portName, string apiSource)  //刪除 OHCV Port 上的所有卡匣
        {
            TransferServiceLogger.Info
            (
                DateTime.Now.ToString("HH:mm:ss.fff ") +
                "OHB >> DB|DeleteOHCVPortCst 誰呼叫:" + apiSource + "  刪除: " + portName + "  上所有卡匣"
            );

            List<ACARRIER> cstList = carrierBLL.db.LoadCassetteDataByOHCV(portName);

            if (cstList.Count != 0)
            {
                foreach (ACARRIER cstData in cstList)
                {
                    DeleteCst(cstData.ID, "DeleteOHCVPortCst");
                }
            }

        }
        public string DeleteCst(string cstID, string cmdSource)
        {
            TransferServiceLogger.Info
            (
                DateTime.Now.ToString("HH:mm:ss.fff ") +
                "OHB >> DB|DeleteCst：cstID:" + cstID + "  誰呼叫:" + cmdSource
            );

            ATRANSFER cmdData = cmdBLL.getExcuteCMD_MCSByCarrierID(cstID);

            if (cmdData != null)
            {
                if (cmdData.TRANSFERSTATE != E_TRAN_STATUS.Transferring)
                {
                    cmdBLL.updateCMD_MCS_TranStatus2Complete(cmdData.ID, CompleteStatus.CommandInitailFail);
                    //cmdBLL.moveCMD_MCSToHistory(cmdData.ID);//移往history
                }
                else
                {
                    TransferServiceLogger.Info
                    (
                        DateTime.Now.ToString("HH:mm:ss.fff ") +
                        "OHB >> DB|DeleteCst:有命令正在使用此卡匣"
                    );
                    return "有命令正在使用此卡匣";
                }
            }

            if (reportBLL.ReportCarrierRemovedCompleted(cstID, null))
            {
                TransferServiceLogger.Info
                (
                    DateTime.Now.ToString("HH:mm:ss.fff ") +
                    "OHB >> DB|Manual_DeleteCst:刪帳成功"
                );
                return "OK";
            }
            else
            {
                TransferServiceLogger.Info
                (
                    DateTime.Now.ToString("HH:mm:ss.fff ") +
                    "Manual >> OHB|Manual_DeleteCst:刪帳失敗"
                );
                return "失敗";
            }
        }

        public void RegisterStagedVehicle(string vehicleID, string portID)
        {
            try
            {
                stagedVehiclePortMapping.Add(vehicleID, portID);
            }
            finally { }
        }

        public void UnregisterStagedVehicle(string vehicleID)
        {
            try
            {
                if (stagedVehiclePortMapping.ContainsKey(vehicleID))
                {
                    stagedVehiclePortMapping.Remove(vehicleID);
                }
            }
            finally { }
        }

        public bool IsStagedVehicle(string vehicleID)
        {
            try
            {
                return stagedVehiclePortMapping.ContainsKey(vehicleID);
            }
            catch
            {
                return false;
            }
        }

        public bool IsStagedPort(string portID)
        {
            try
            {
                return stagedVehiclePortMapping.ContainsValue(portID);
            }
            catch
            {
                return false;
            }
        }
    }


    interface ITranAssigner
    {
        bool AssignTransferToVehicle(VTRANSFER waittingExcuteMcsCmd, AVEHICLE bestSuitableVh);
    }

    public class TranAssignerNormal : ITranAssigner
    {
        SCApplication scApp = null;
        protected NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public TranAssignerNormal(SCApplication _scApp)
        {
            scApp = _scApp;
        }
        public bool AssignTransferToVehicle(VTRANSFER waittingExcuteMcsCmd, AVEHICLE bestSuitableVh)
        {
            bool is_success = true;
            ACMD assign_cmd = waittingExcuteMcsCmd.ConvertToCmd(scApp.PortStationBLL, scApp.SequenceBLL, bestSuitableVh);
            //var destination_info = checkAndRenameDestinationPortIfAGVStationReady(assign_cmd);
            (bool checkSuccess, string destinationPortID, string destinationAdrID) destination_info =
                default((bool checkSuccess, string destinationPortID, string destinationAdrID));
            //if (DebugParameter.isNeedCheckPortReady)
            //    destination_info = checkAndRenameDestinationPortIfAGVStationReady(assign_cmd);
            //else
            //    destination_info = checkAndRenameDestinationPortIfAGVStationAuto(assign_cmd);
            destination_info = scApp.TransferService.checkAndRenameDestinationPortIfAGVStation(assign_cmd);
            if (destination_info.checkSuccess)
            {
                assign_cmd.DESTINATION = destination_info.destinationAdrID;
                assign_cmd.DESTINATION_PORT = destination_info.destinationPortID;
            }
            else
            {
                //暫時針對有指定vh的才進行預先移動
                if (assign_cmd.getTragetPortEQ(scApp.EqptBLL) is IAGVStationType)
                {
                    var sgv_station = assign_cmd.getTragetPortEQ(scApp.EqptBLL) as IAGVStationType;
                    if (SCUtility.isEmpty(sgv_station.BindingVh))
                        return false;
                }
                scApp.VehicleService.Command.preMoveToSourcePort(bestSuitableVh, assign_cmd);
                //todo log...
                return false;
            }
            is_success = is_success && scApp.CMDBLL.checkCmd(assign_cmd);
            using (TransactionScope tx = SCUtility.getTransactionScope())
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    is_success = is_success && scApp.CMDBLL.addCmd(assign_cmd);
                    is_success = is_success && scApp.CMDBLL.updateTransferCmd_TranStatus2PreInitial(waittingExcuteMcsCmd.ID);
                    if (is_success)
                    {
                        tx.Complete();
                    }
                    else
                    {
                        CMDBLL.CommandCheckResult check_result = CMDBLL.getOrSetCallContext<CMDBLL.CommandCheckResult>(CMDBLL.CALL_CONTEXT_KEY_WORD_OHTC_CMD_CHECK_RESULT);
                        check_result.Result.AppendLine($" vh:{assign_cmd.VH_ID} creat command to db unsuccess.");
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(TransferService), Device: DEVICE_NAME_AGV,
                           Data: $"Assign transfer command fail.transfer id:{waittingExcuteMcsCmd.ID}",
                           Details: check_result.ToString(),
                           XID: check_result.Num);
                    }
                }
            }
            return is_success;
        }
    }

    public class TransferAssignerSwap : ITranAssigner
    {
        SCApplication scApp = null;
        protected NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public TransferAssignerSwap(SCApplication _scApp)
        {
            scApp = _scApp;
        }
        public bool AssignTransferToVehicle(VTRANSFER waittingExcuteMcsCmd, AVEHICLE bestSuitableVh)
        {
            bool is_success = true;
            ACMD assign_cmd = waittingExcuteMcsCmd.ConvertToCmd(scApp.PortStationBLL, scApp.SequenceBLL, bestSuitableVh);

            //is_success = is_success && scApp.CMDBLL.checkCmdSwap(assign_cmd);
            is_success = is_success && scApp.CMDBLL.checkCmd(assign_cmd);
            //A20210520-01 start
            if (!is_success)
            {
                CMDBLL.CommandCheckResult check_result = CMDBLL.getOrSetCallContext<CMDBLL.CommandCheckResult>(CMDBLL.CALL_CONTEXT_KEY_WORD_OHTC_CMD_CHECK_RESULT);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(TransferService), Device: DEVICE_NAME_AGV,
                   Data: $"Assign transfer command fail.transfer id:{waittingExcuteMcsCmd.ID}",
                   Details: check_result.ToString(),
                   XID: check_result.Num);
                return false;
            }
            //A20210520-01 end

            using (TransactionScope tx = SCUtility.getTransactionScope())
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    is_success = is_success && scApp.CMDBLL.addCmd(assign_cmd);
                    is_success = is_success && scApp.CMDBLL.updateTransferCmd_TranStatus2PreInitial(waittingExcuteMcsCmd.ID);
                    if (is_success)
                    {
                        tx.Complete();
                    }
                    else
                    {
                        CMDBLL.CommandCheckResult check_result = CMDBLL.getOrSetCallContext<CMDBLL.CommandCheckResult>(CMDBLL.CALL_CONTEXT_KEY_WORD_OHTC_CMD_CHECK_RESULT);
                        check_result.Result.AppendLine($" vh:{assign_cmd.VH_ID} creat command to db unsuccess.");
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(TransferService), Device: DEVICE_NAME_AGV,
                           Data: $"Assign transfer command fail.transfer id:{waittingExcuteMcsCmd.ID}",
                           Details: check_result.ToString(),
                           XID: check_result.Num);
                    }
                }
            }
            return is_success;
        }
    }


}
