using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.SECS.OHTC.AT_S;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using Nancy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.WebAPI
{
    public class VehicleInfo : NancyModule
    {
        SCApplication app = null;
        const string restfulContentType = "application/json; charset=utf-8";
        const string urlencodedContentType = "application/x-www-form-urlencoded; charset=utf-8";
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public VehicleInfo()
        {
            //app = SCApplication.getInstance();
            RegisterVehilceEvent();
            RegisterMapEvent();
            After += ctx => ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");

        }

        private void RegisterMapEvent()
        {
            Get["MapInfo/{MapInfoDataType}"] = (p) =>
            {
                bool isSuccess = false;
                SCApplication scApp = SCApplication.getInstance();
                string map_data_type = p.MapInfoDataType;
                SCAppConstants.MapInfoDataType dataType = default(SCAppConstants.MapInfoDataType);
                isSuccess = Enum.TryParse(map_data_type, out dataType);
                string query_data = null;
                switch (dataType)
                {
                    case SCAppConstants.MapInfoDataType.MapID:
                        query_data = scApp.BC_ID;
                        break;
                    case SCAppConstants.MapInfoDataType.EFConnectionString:
                        string connectionName = "OHTC_DevEntities";
                        query_data = System.Configuration.ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
                        break;
                    case SCAppConstants.MapInfoDataType.Rail:
                        query_data = JsonConvert.SerializeObject(scApp.MapBLL.loadAllRail());
                        break;
                    case SCAppConstants.MapInfoDataType.Point:
                        query_data = JsonConvert.SerializeObject(scApp.MapBLL.loadAllPoint());
                        break;
                    case SCAppConstants.MapInfoDataType.GroupRails:
                        query_data = JsonConvert.SerializeObject(scApp.MapBLL.loadAllGroupRail());
                        break;
                    case SCAppConstants.MapInfoDataType.Address:
                        //query_data = JsonConvert.SerializeObject(scApp.MapBLL.loadAllAddress());
                        query_data = JsonConvert.SerializeObject(scApp.AddressesBLL.cache.GetAddresses());
                        break;
                    case SCAppConstants.MapInfoDataType.Section:
                        query_data = JsonConvert.SerializeObject(scApp.MapBLL.loadAllSection());
                        break;
                    case SCAppConstants.MapInfoDataType.Segment:
                        query_data = JsonConvert.SerializeObject(scApp.MapBLL.loadAllSegments());
                        break;
                    case SCAppConstants.MapInfoDataType.Port:
                        query_data = JsonConvert.SerializeObject(scApp.getEQObjCacheManager().getALLPortStation());
                        break;
                    case SCAppConstants.MapInfoDataType.PortIcon:
                        query_data = JsonConvert.SerializeObject(scApp.MapBLL.loadAllPortIcon());
                        break;
                    case SCAppConstants.MapInfoDataType.Vehicle:
                        query_data = JsonConvert.SerializeObject(scApp.getEQObjCacheManager().getAllVehicle());
                        break;
                    case SCAppConstants.MapInfoDataType.Line:
                        query_data = JsonConvert.SerializeObject(scApp.getEQObjCacheManager().getLine());
                        break;
                    case SCAppConstants.MapInfoDataType.Alarm:
                        query_data = JsonConvert.SerializeObject(scApp.AlarmBLL.getCurrentAlarmsFromRedis());
                        break;
                    case SCAppConstants.MapInfoDataType.MTL:
                        query_data = JsonConvert.SerializeObject(scApp.EqptBLL.OperateCatch.loadMTLs());
                        break;
                }
                var response = (Response)query_data;
                response.ContentType = restfulContentType;

                return response;
            };

            Get["SystemExcuteInfo/{SystemExcuteInfoType}"] = (p) =>
            {
                bool isSuccess = false;
                SCApplication scApp = SCApplication.getInstance();
                string map_data_type = p.SystemExcuteInfoType;
                SCAppConstants.SystemExcuteInfoType dataType = default(SCAppConstants.SystemExcuteInfoType);
                isSuccess = Enum.TryParse(map_data_type, out dataType);
                string query_data = "";
                switch (dataType)
                {
                    case SCAppConstants.SystemExcuteInfoType.CommandInQueueCount:
                        query_data = scApp.CMDBLL.getCMD_MCSIsQueueCount().ToString();
                        break;
                    case SCAppConstants.SystemExcuteInfoType.CommandInExcuteCount:
                        query_data = scApp.CMDBLL.getCMD_MCSIsRunningCount().ToString();
                        break;
                }
                var response = (Response)query_data;
                response.ContentType = restfulContentType;

                return response;
            };
        }

        private void RegisterVehilceEvent()
        {
            Get["AVEHICLES/{ID}"] = (p) =>
            {
                string vh_id = p.ID;
                AVEHICLE vh = SCApplication.getInstance().VehicleBLL.cache.getVehicle(vh_id);
                var response = (Response)vh.ToString();
                response.ContentType = restfulContentType;

                return response;
            };
            Get["AVEHICLES"] = (p) =>
            {

                string vh_id = p.ID;
                List<AVEHICLE> vhs = SCApplication.getInstance().getEQObjCacheManager().getAllVehicle();
                var response = (Response)JsonConvert.SerializeObject(vhs);
                response.ContentType = restfulContentType;

                return response;
            };
            //Get["AVEHICLES/(?<all>)"] = (p) =>
            Get["AVEHICLES/_search"] = (p) =>
            {
                List<AVEHICLE> vhs = null;

                foreach (string name in Request.Query)
                {
                    switch (name)
                    {
                        case "SectionID":
                            string sec_id = Request.Query[name] ?? string.Empty;
                            vhs = SCApplication.getInstance().VehicleBLL.cache.loadVehicleBySEC_ID(sec_id);
                            break;
                    }
                }
                var response = (Response)JsonConvert.SerializeObject(vhs);
                response.ContentType = restfulContentType;

                return response;
            };

            Get["metrics"] = (p) =>
            {
                int total_idle_vh_clean = SCApplication.getInstance().VehicleBLL.cache.getNoExcuteMcsCmdVhCount(E_VH_TYPE.Clean);
                int total_idle_vh_Dirty = SCApplication.getInstance().VehicleBLL.cache.getNoExcuteMcsCmdVhCount(E_VH_TYPE.Dirty);
                int total_cmd_is_queue_count = SCApplication.getInstance().CMDBLL.getCMD_MCSIsQueueCount();
                int total_cmd_is_running_count = SCApplication.getInstance().CMDBLL.getCMD_MCSIsRunningCount();

                string ohxc_excute_info = string.Empty;

                StringBuilder sb = new StringBuilder();
                setOhxCContent(sb, nameof(total_idle_vh_clean), total_idle_vh_clean, "current idle clean car");
                setOhxCContent(sb, nameof(total_idle_vh_Dirty), total_idle_vh_Dirty, "current idle dirty car");
                setOhxCContent(sb, nameof(total_cmd_is_queue_count), total_cmd_is_queue_count, "cmd number being queued");
                setOhxCContent(sb, nameof(total_cmd_is_running_count), total_cmd_is_running_count, "cmd number being executed");

                var response = (Response)sb.ToString();
                response.ContentType = restfulContentType;
                return response;
            };

            Post["AVEHICLES/ViewerUpdate"] = (p) =>
            {
                SCApplication scApp = SCApplication.getInstance();
                List<AVEHICLE> vhs = scApp.getEQObjCacheManager().getAllVehicle();

                //foreach (AVEHICLE vh in vhs)
                //{
                //    scApp.VehicleService.PublishVhInfo(vh, null);
                //    SpinWait.SpinUntil(() => false, 10);
                //}

                var response = (Response)"OK";
                response.ContentType = restfulContentType;
                return response;
            };

            //Post["api/io/T2STK100T01/waitin/CST01"] = (p) =>
            //{

            //    var response = (Response)"OK";
            //    response.ContentType = restfulContentType;
            //    return response;
            //};

            Post["AVEHICLES/SendCommand"] = (p) =>
            {
                var scApp = SCApplication.getInstance();
                bool isSuccess = true;
                string vh_id = Request.Query.vh_id.Value ?? Request.Form.vh_id.Value ?? string.Empty;
                string carrier_id = Request.Query.carrier_id.Value ?? Request.Form.carrier_id.Value ?? string.Empty;
                string from_port_id = Request.Query.from_port_id.Value ?? Request.Form.from_port_id.Value ?? string.Empty;
                string to_port_id = Request.Query.to_port_id.Value ?? Request.Form.to_port_id.Value ?? string.Empty;
                E_CMD_TYPE e_cmd_type = default(E_CMD_TYPE);
                string cmd_type = Request.Query.cmd_type.Value ?? Request.Form.cmd_type.Value ?? string.Empty;
                string result = string.Empty;








                string from_adr = Request.Query.from_port_id.Value ?? Request.Form.from_port_id.Value ?? string.Empty;
                string to_adr = Request.Query.to_port_id.Value ?? Request.Form.to_port_id.Value ?? string.Empty;
                string cst_id = Request.Query.carrier_id.Value ?? Request.Form.carrier_id.Value ?? string.Empty;
                int port_priority = 0;


                try
                {
                    AVEHICLE vh = scApp.VehicleBLL.cache.getVehicle(vh_id);
                    if (vh == null)
                    {
                        result = "Fail to find Vehicle.";
                        isSuccess = false;
                    }
                    if (isSuccess)
                    {
                        isSuccess = Enum.TryParse(cmd_type, out e_cmd_type);
                        if (isSuccess)
                        {
                            if (e_cmd_type != E_CMD_TYPE.Move)
                            {
                                string commandID = scApp.SequenceBLL.getCommandID(SCAppConstants.GenOHxCCommandType.Manual);
                                S2F49_TRANSFER s2f49_transfer = new S2F49_TRANSFER();
                                s2f49_transfer.REPITEMS = new S2F49_TRANSFER.REPITEM();
                                s2f49_transfer.REPITEMS.COMMINFO = new S2F49_TRANSFER.REPITEM.COMM();
                                s2f49_transfer.REPITEMS.COMMINFO.COMMAINFOVALUE = new S2F49_TRANSFER.REPITEM.COMM.COMMVALUE();
                                s2f49_transfer.REPITEMS.COMMINFO.COMMAINFOVALUE.COMMANDID = new S2F49_TRANSFER.REPITEM.CP_ASCII();
                                s2f49_transfer.REPITEMS.COMMINFO.COMMAINFOVALUE.PRIORITY = new S2F49_TRANSFER.REPITEM.CP_U2();
                                s2f49_transfer.REPITEMS.COMMINFO.COMMAINFOVALUE.REPLACE = new S2F49_TRANSFER.REPITEM.CP_U2();
                                s2f49_transfer.REPITEMS.TRANINFO = new S2F49_TRANSFER.REPITEM.TRAN();
                                s2f49_transfer.REPITEMS.TRANINFO.TRANSFERINFOVALUE = new S2F49_TRANSFER.REPITEM.TRAN.TRANVALUE();
                                s2f49_transfer.REPITEMS.TRANINFO.TRANSFERINFOVALUE.CARRIERIDINFO = new S2F49_TRANSFER.REPITEM.CP_ASCII();
                                s2f49_transfer.REPITEMS.TRANINFO.TRANSFERINFOVALUE.SOUINFO = new S2F49_TRANSFER.REPITEM.CP_ASCII();
                                s2f49_transfer.REPITEMS.TRANINFO.TRANSFERINFOVALUE.DESTINFO = new S2F49_TRANSFER.REPITEM.CP_ASCII();
                                s2f49_transfer.REPITEMS.TRANINFO_CST = new S2F49_TRANSFER.REPITEM.TRAN_CST();
                                s2f49_transfer.REPITEMS.TRANINFO_CST.TRAN_CSTVALUE = new S2F49_TRANSFER.REPITEM.TRAN_CST_VALUE();
                                s2f49_transfer.REPITEMS.TRANINFO_CST.TRAN_CSTVALUE.LOTINFOVALUE = new S2F49_TRANSFER.REPITEM.CP_ASCII();
                                s2f49_transfer.REPITEMS.COMMINFO.COMMAINFOVALUE.COMMANDID.CPVAL = commandID;
                                s2f49_transfer.REPITEMS.COMMINFO.COMMAINFOVALUE.PRIORITY.CPVAL = "5";
                                s2f49_transfer.REPITEMS.COMMINFO.COMMAINFOVALUE.REPLACE.CPVAL = "0";
                                s2f49_transfer.REPITEMS.TRANINFO.TRANSFERINFOVALUE.CARRIERIDINFO.CPVAL = cst_id;

                                if (e_cmd_type == E_CMD_TYPE.Load)
                                {
                                    if (vh.HAS_CST)
                                    {
                                        result = "Vehicle already has CST.";
                                        isSuccess = false;
                                    }
                                    if (isSuccess)
                                    {
                                        APORTSTATION source_port = scApp.PortStationBLL.OperateCatch.getPortStation(from_adr);
                                        if (source_port == null)
                                        {
                                            source_port = scApp.PortStationBLL.OperateCatch.getPortStationByAdrIDSingle(from_adr);
                                        }
                                        if (source_port == null)
                                        {
                                            result = "Fail to find source port.";
                                            isSuccess = false;
                                        }
                                        if (isSuccess)
                                        {
                                            scApp.TransferService.AreSourceEnable(source_port.PORT_ID);
                                            s2f49_transfer.REPITEMS.TRANINFO.TRANSFERINFOVALUE.SOUINFO.CPVAL = source_port.PORT_ID;
                                            s2f49_transfer.REPITEMS.TRANINFO.TRANSFERINFOVALUE.DESTINFO.CPVAL = vh.Real_ID;

                                            from_adr = source_port.PORT_ID;
                                            to_adr = vh.Real_ID;
                                            port_priority = source_port.PRIORITY;
                                        }
                                    }
                                }
                                else if (e_cmd_type == E_CMD_TYPE.LoadUnload)
                                {
                                    if (vh.HAS_CST)
                                    {
                                        result = "Vehicle already has CST.";
                                        isSuccess = false;
                                    }
                                    if (isSuccess)
                                    {
                                        APORTSTATION source_port = scApp.PortStationBLL.OperateCatch.getPortStation(from_adr);
                                        if (source_port == null)
                                        {
                                            source_port = scApp.PortStationBLL.OperateCatch.getPortStationByAdrIDSingle(from_adr);
                                        }
                                        if (source_port == null)
                                        {
                                            result = "Fail to find source port.";
                                            isSuccess = false;
                                        }
                                        if (isSuccess)
                                        {
                                            APORTSTATION dest_port = scApp.PortStationBLL.OperateCatch.getPortStation(to_adr);
                                            if (dest_port == null)
                                            {
                                                dest_port = scApp.PortStationBLL.OperateCatch.getPortStationByAdrIDSingle(to_adr);
                                            }
                                            if (dest_port == null)
                                            {
                                                result = "Fail to find destinetion port.";
                                                isSuccess = false;
                                            }
                                            if (isSuccess)
                                            {
                                                s2f49_transfer.REPITEMS.TRANINFO.TRANSFERINFOVALUE.SOUINFO.CPVAL = source_port.PORT_ID;
                                                s2f49_transfer.REPITEMS.TRANINFO.TRANSFERINFOVALUE.DESTINFO.CPVAL = dest_port.PORT_ID;

                                                from_adr = source_port.PORT_ID;
                                                to_adr = dest_port.PORT_ID;

                                                port_priority = dest_port.PRIORITY;
                                            }

                                        }

                                    }


                                }
                                else if (e_cmd_type == E_CMD_TYPE.Unload)
                                {
                                    if (!vh.HAS_CST)
                                    {
                                        result = "Vehicle do not have CST.";
                                        isSuccess = false;
                                    }
                                    if (isSuccess)
                                    {
                                        if (!SCUtility.isMatche(vh.CST_ID, cst_id))
                                        {
                                            result = $"CST:[{vh.CST_ID}] on vehicle ID do not match input CST ID:{cst_id}.";
                                            isSuccess = false;
                                        }
                                        if (isSuccess)
                                        {

                                            APORTSTATION dest_port = scApp.PortStationBLL.OperateCatch.getPortStation(to_adr);
                                            if (dest_port == null)
                                            {
                                                dest_port = scApp.PortStationBLL.OperateCatch.getPortStationByAdrIDSingle(to_adr);
                                            }
                                            if (dest_port == null)
                                            {
                                                result = "Fail to find destinetion port.";
                                                isSuccess = false;
                                            }
                                            if (isSuccess)
                                            {
                                                s2f49_transfer.REPITEMS.TRANINFO.TRANSFERINFOVALUE.SOUINFO.CPVAL = vh.Real_ID;
                                                s2f49_transfer.REPITEMS.TRANINFO.TRANSFERINFOVALUE.DESTINFO.CPVAL = dest_port.PORT_ID;
                                                from_adr = vh.Real_ID;
                                                to_adr = dest_port.PORT_ID;

                                                port_priority = dest_port.PRIORITY;
                                            }

                                        }


                                    }



                                }
                                if (isSuccess)
                                {
                                    string rtnStr = "";
                                    S2F50 s2f50 = new S2F50();
                                    MCSDefaultMapAction mcsMapAction = SCApplication.getInstance().getEQObjCacheManager().getLine().getMapActionByIdentityKey(typeof(MCSDefaultMapAction).Name) as MCSDefaultMapAction;
                                    var check_result = mcsMapAction.doCheckMCSCommand(s2f49_transfer, ref s2f50, out rtnStr);
                                    s2f50.HCACK = check_result.result;
                                    if (!check_result.isSuccess)
                                    {
                                        result = rtnStr;
                                        isSuccess = false;
                                    }
                                    else
                                    {
                                        ATRANSFER transfer = s2f49_transfer.ToTRANSFER(scApp.PortStationBLL, scApp.EqptBLL);
                                        transfer.SetCheckResult(check_result.isSuccess, check_result.result);
                                        bool is_process_success = scApp.TransferService.Creat(transfer);
                                        if (!is_process_success)
                                        {
                                            result = "Transfer create fail.";
                                            isSuccess = false;
                                        }
                                        else
                                        {
                                            //上報MCS OperatorInitialAction
                                            scApp.ReportBLL.ReportdOperatorInitialAction(transfer.ID, "TRANSFER",
                                                transfer.CARRIER_ID, transfer.HOSTSOURCE, transfer.HOSTDESTINATION, transfer.PRIORITY.ToString());
                                        }
                                    }
                                }






                            }
                            else
                            {

                                //scApp.MapBLL.getAddressID(from_port_id, out from_adr);
                                //scApp.MapBLL.getAddressID(to_port_id, out to_adr);
                                ACMD cmd_obj = null;
                                scApp.CMDBLL.doCreatCommand(vh_id, out cmd_obj,
                                                                cmd_type: e_cmd_type,
                                                                source: from_adr,
                                                                destination: to_adr,
                                                                carrier_id: carrier_id,
                                                                gen_cmd_type: SCAppConstants.GenOHxCCommandType.Manual);
                                sc.BLL.CMDBLL.CommandCheckResult check_result_info =
                                                    sc.BLL.CMDBLL.getCallContext<sc.BLL.CMDBLL.CommandCheckResult>
                                                   (sc.BLL.CMDBLL.CALL_CONTEXT_KEY_WORD_OHTC_CMD_CHECK_RESULT);
                                isSuccess = check_result_info.IsSuccess;
                                result = check_result_info.ToString();
                                if (isSuccess)
                                {
                                    //isSuccess = scApp.VehicleService.doSendCommandToVh(assignVH, cmd_obj);
                                    isSuccess = scApp.VehicleService.Send.Command(vh, cmd_obj);
                                    if (isSuccess)
                                    {
                                        result = "OK";
                                    }
                                    else
                                    {
                                        result = "Send command to vehicle failed!";
                                    }
                                }
                                else
                                {
                                    result = "Command create failed!";
                                    //bcf.App.BCFApplication.onWarningMsg(this, new bcf.Common.LogEventArgs("Command create fail.", check_result_info.Num));
                                }

                                ////AADDRESS adr = scApp.MapBLL.getAddressByID(to_adr);
                                //AADDRESS adr = scApp.AddressesBLL.cache.GetAddress(to_adr);
                                //if (adr != null)
                                //{
                                //    if (adr.IsAccessable)
                                //    {
                                //        //do nothing
                                //    }
                                //    else
                                //    {
                                //        result = "To address is not accessable";
                                //        isSuccess = false;
                                //    }
                                //}

                                //var check_result = excuteCommandNew(cmd_type, from_adr, to_adr, cst_id, vh_id);
                                //if (!check_result.isSuccess)
                                //{
                                //    MessageBox.Show(check_result.result, "Command create fail.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                //}
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    isSuccess = false;
                    result = "Execption happend!";
                    logger.Error(ex, "Execption:");
                }

                if (isSuccess)
                {
                    result = "OK";
                }

                var response = (Response)result;
                response.ContentType = restfulContentType;
                return response;
            };

            Post["AVEHICLES/SendReset"] = (p) =>
            {
                var scApp = SCApplication.getInstance();
                bool isSuccess = true;
                string vh_id = Request.Query.vh_id.Value ?? Request.Form.vh_id.Value ?? string.Empty;

                string result = string.Empty;
                try
                {
                    AVEHICLE assignVH = null;
                    assignVH = scApp.VehicleBLL.cache.getVehicle(vh_id);

                    isSuccess = assignVH != null;

                    if (isSuccess)
                    {
                        isSuccess = scApp.VehicleService.Send.StatusRequest(vh_id, true);
                        if (isSuccess)
                        {
                            result = "OK";
                        }
                        else
                        {
                            result = "Send vehicle status request failed.";
                        }
                    }
                    else
                    {
                        result = $"Vehicle :[{vh_id}] not found!";
                    }
                }
                catch (Exception ex)
                {
                    isSuccess = false;
                    result = "Execption happend!";
                    logger.Error(ex, "Execption:");
                }

                var response = (Response)result;
                response.ContentType = restfulContentType;
                return response;
            };

            Post["AVEHICLES/SendCancelAbort"] = (p) =>
            {
                var scApp = SCApplication.getInstance();
                bool isSuccess = true;
                string vh_id = Request.Query.vh_id.Value ?? Request.Form.vh_id.Value ?? string.Empty;
                //string cmd_id = "";//todo kevin 需要指定cmd id
                string result = string.Empty;
                try
                {
                    AVEHICLE noticeCar = null;
                    noticeCar = scApp.VehicleBLL.cache.getVehicle(vh_id);
                    ATRANSFER aTRANSFER = scApp.TransferBLL.db.transfer.getTransferByExecutedCMDID(noticeCar.CMD_ID);
                    if (aTRANSFER != null)
                    {
                        scApp.ReportBLL.newReportTransferCancelInitial(aTRANSFER.ID, null);
                    }
                    isSuccess = scApp.VehicleService.Send.Cancel(noticeCar.VEHICLE_ID, noticeCar.CMD_ID, sc.ProtocolFormat.OHTMessage.CMDCancelType.CmdCancel);
                    if (isSuccess)
                    {
                        //do nothing
                    }
                    else
                    {
                        if (aTRANSFER != null)
                        {
                            scApp.ReportBLL.newReportTransferCancelFailed(aTRANSFER.ID, null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    isSuccess = false;
                    result = "Execption happend!";
                    logger.Error(ex, "Execption:");
                }

                var response = (Response)result;
                response.ContentType = restfulContentType;
                return response;
            };

            Post["AVEHICLES/PauseEvent"] = (p) =>
            {
                bool isSuccess = false;
                SCApplication scApp = SCApplication.getInstance();
                string vh_id = Request.Query.vh_id.Value ?? Request.Form.vh_id.Value ?? string.Empty;
                string event_type = Request.Query.event_type.Value ?? Request.Form.event_type.Value ?? string.Empty;
                PauseEvent pauseEvent = default(PauseEvent);
                isSuccess = Enum.TryParse(event_type, out pauseEvent);
                if (isSuccess)
                {
                    isSuccess = scApp.VehicleService.Send.Pause(vh_id, pauseEvent, PauseType.OhxC);
                }

                var response = (Response)(isSuccess ? "OK" : "NG");
                response.ContentType = restfulContentType;
                return response;
            };


            Post["AVEHICLES/PauseStatusChange"] = (p) =>
            {
                bool isSuccess = false;
                string result = string.Empty;
                SCApplication scApp = SCApplication.getInstance();
                string vh_id = Request.Query.vh_id.Value ?? Request.Form.vh_id.Value ?? string.Empty;
                string pauseType = Request.Query.pauseType.Value ?? Request.Form.pauseType.Value ?? string.Empty;
                string event_type = Request.Query.event_type.Value ?? Request.Form.event_type.Value ?? string.Empty;
                PauseType pause_type = default(PauseType);
                PauseEvent pauseEvent = default(PauseEvent);
                isSuccess = Enum.TryParse(pauseType, out pause_type);

                if (isSuccess)
                {
                    isSuccess = Enum.TryParse(event_type, out pauseEvent);

                    if (isSuccess)
                    {
                        isSuccess = scApp.VehicleService.Send.Pause(vh_id, pauseEvent, pause_type);
                        if (isSuccess)
                        {
                            result = "OK";
                        }
                        else
                        {
                            result = $"Send pause request to vehicle:{vh_id} failed.";
                        }
                    }
                    else
                    {
                        result = $"Can't recognize Pause Event:{event_type}.";

                    }

                }
                else
                {
                    result = $"Can't recognize Pause Type:{pauseType}.";
                }

                var response = (Response)result;
                response.ContentType = restfulContentType;
                return response;
            };

            Post["AVEHICLES/ModeStatusChange"] = (p) =>
            {
                string result = string.Empty;
                bool isSuccess = false;
                SCApplication scApp = SCApplication.getInstance();
                string vh_id = Request.Query.vh_id.Value ?? Request.Form.vh_id.Value ?? string.Empty;
                string modeStatus = Request.Query.modeStatus.Value ?? Request.Form.modeStatus.Value ?? string.Empty;
                VHModeStatus mode_status = default(VHModeStatus);
                isSuccess = Enum.TryParse(modeStatus, out mode_status);
                try
                {
                    if (isSuccess)
                    {
                        scApp.VehicleBLL.cache.updataVehicleMode(vh_id, mode_status);
                        result = "OK";
                    }
                    else
                    {
                        result = $"Can't recognize mode status:{modeStatus}.";
                    }
                }
                catch (Exception ex)
                {
                    result = $"Update vehicle:{vh_id} mode status failed.";
                    logger.Error(ex, "Exception");
                }
                var response = (Response)result;
                response.ContentType = restfulContentType;
                return response;
            };

            Post["AVEHICLES/ResetAlarm"] = (p) =>
            {
                var scApp = SCApplication.getInstance();
                string result = string.Empty;
                bool isSuccess = true;
                string vh_id = Request.Query.vh_id.Value ?? Request.Form.vh_id.Value ?? string.Empty;
                try
                {

                    isSuccess = scApp.VehicleService.Send.AlarmReset(vh_id);
                    if (isSuccess)
                    {
                        result = "OK";
                    }
                    else
                    {
                        result = "Reset alarm failed.";
                    }
                }
                catch (Exception ex)
                {
                    result = "Reset alarm failedwith exception happened.";
                }

                var response = (Response)result;
                response.ContentType = restfulContentType;
                return response;
            };

            Post["Engineer/ForceCmdFinish"] = (p) =>
            {
                var scApp = SCApplication.getInstance();
                string vh_id = Request.Query.vh_id.Value ?? Request.Form.vh_id.Value ?? string.Empty;
                bool isSuccess = scApp.CMDBLL.forceUpdataCmdStatus2FnishByVhID(vh_id);
                if (isSuccess)
                {
                    var vh = scApp.VehicleBLL.cache.getVehicle(vh_id);
                    //vh.NotifyVhExcuteCMDStatusChange();
                    vh.onExcuteCommandStatusChange();
                }
                var response = (Response)(isSuccess ? "OK" : "NG");
                response.ContentType = restfulContentType;
                return response;
            };
        }

        private static StringBuilder setOhxCContent(StringBuilder sb, string key, int value, string description)
        {
            sb.AppendLine($"#{PROMETHEUS_TOKEN_HELP} ohxc_{key} {description}");
            sb.AppendLine($"#{PROMETHEUS_TOKEN_TYPE} ohxc_{key} gauge");
            sb.AppendLine($"ohxc_{key} {value}");
            return sb;
        }
        const string PROMETHEUS_TOKEN_HELP = "HELP";
        const string PROMETHEUS_TOKEN_TYPE = "TYPE";
    }
}
