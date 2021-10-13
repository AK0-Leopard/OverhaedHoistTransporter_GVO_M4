//*********************************************************************************
//      MESDefaultMapAction.cs
//*********************************************************************************
// File Name: MESDefaultMapAction.cs
// Description: 與EAP通訊的劇本
//
//(c) Copyright 2014, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
// 2019/07/16    Mark Chou      N/A            M0.01   修正回覆S1F4 SVID305會發生Exception的問題
// 2019/08/26    Kevin Wei      N/A            M0.02   修正原本在只要有From、To命令還是在Wating的狀態時，
//                                                     此時MCS若下達一筆命令則會拒絕，改成只要是From相同，就會拒絕。
//**********************************************************************************
using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Controller;
using com.mirle.ibg3k0.bcf.Data.ValueDefMapAction;
using com.mirle.ibg3k0.bcf.Data.VO;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.BLL;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.SECS.OHTC.AT_S;
using com.mirle.ibg3k0.sc.Data.SECSDriver;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using com.mirle.ibg3k0.stc.Common;
using com.mirle.ibg3k0.stc.Data.SecsData;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Transactions;

namespace com.mirle.ibg3k0.sc.Data.ValueDefMapAction
{
    public class MCSDefaultMapAction : IBSEMDriver, IValueDefMapAction
    {
        const string DEVICE_NAME_MCS = "MCS";
        const string CALL_CONTEXT_KEY_WORD_SERVICE_ID_MCS = "MCS Service";

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static Logger GlassTrnLogger = LogManager.GetLogger("GlassTransferRpt_EAP");
        protected static Logger logger_MapActionLog = LogManager.GetLogger("MapActioLog");
        private ReportBLL reportBLL = null;


        public virtual string getIdentityKey()
        {
            return this.GetType().Name;
        }

        public virtual void setContext(BaseEQObject baseEQ)
        {
            this.line = baseEQ as ALINE;
        }
        public virtual void unRegisterEvent()
        {

        }
        public virtual void doShareMemoryInit(BCFAppConstants.RUN_LEVEL runLevel)
        {
            try
            {
                switch (runLevel)
                {
                    case BCFAppConstants.RUN_LEVEL.ZERO:
                        SECSConst.setDicCEIDAndRPTID(scApp.CEIDBLL.loadDicCEIDAndRPTID());
                        SECSConst.setDicRPTIDAndVID(scApp.CEIDBLL.loadDicRPTIDAndVID());
                        break;
                    case BCFAppConstants.RUN_LEVEL.ONE:
                        break;
                    case BCFAppConstants.RUN_LEVEL.TWO:
                        break;
                    case BCFAppConstants.RUN_LEVEL.NINE:
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exection:");
            }
        }
        #region Receive 
        protected override void S1F3ReceiveSelectedEquipmentStatusRequest(object sender, SECSEventArgs e)
        {
            try
            {
                S1F3 s1f3 = ((S1F3)e.secsHandler.Parse<S1F3>(e));
                SCUtility.secsActionRecordMsg(scApp, true, s1f3);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                              Data: s1f3);

                int count = s1f3.SVID.Count();
                S1F4 s1f4 = new S1F4();
                s1f4.SECSAgentName = scApp.EAPSecsAgentName;
                s1f4.SystemByte = s1f3.SystemByte;
                s1f4.SV = new SXFY[count];
                for (int i = 0; i < count; i++)
                {
                    if (s1f3.SVID[i] == SECSConst.VID_AlarmSet)
                    {
                        line.CurrentStateChecked = true;
                        s1f4.SV[i] = buildAlarmsSetVIDItem();
                    }
                    else if (s1f3.SVID[i] == SECSConst.VID_ControlState)
                    {
                        line.EnhancedVehiclesChecked = true;
                        s1f4.SV[i] = buildControlStateVIDItem();
                    }
                    else if (s1f3.SVID[i] == SECSConst.VID_EnhancedALID)
                    {
                        line.EnhancedVehiclesChecked = true;
                        s1f4.SV[i] = buildEnhancedAlarmsSetVIDItem();
                    }

                    else if (s1f3.SVID[i] == SECSConst.VID_EnhancedCarriers)
                    {
                        line.EnhancedCarriersChecked = true;
                        s1f4.SV[i] = buildEnhanecdCarriersVIDItem();
                    }
                    else if (s1f3.SVID[i] == SECSConst.VID_EnhancedVehicles)
                    {
                        line.EnhancedTransfersChecked = true;
                        s1f4.SV[i] = buildEnhanecdVehiclesVIDItem();
                    }
                    else if (s1f3.SVID[i] == SECSConst.VID_TSCState)
                    {
                        line.TSCStateChecked = true;
                        s1f4.SV[i] = buildTCSStateVIDItem();
                    }
                    else if (s1f3.SVID[i] == SECSConst.VID_EnhancedTransfers)
                    {
                        line.CurrentPortStateChecked = true;
                        s1f4.SV[i] = buildEnhanecdTransfersItem();
                    }
                    else if (s1f3.SVID[i] == SECSConst.VID_CurrentPortStates)
                    {
                        line.CurrentPortStateChecked = true;
                        s1f4.SV[i] = buildCurrentPortStatus();
                    }
                    else if (s1f3.SVID[i] == SECSConst.VID_CurrEqPortStatus)
                    {
                        line.CurrentPortStateChecked = true;
                        s1f4.SV[i] = buildCurrEqPortStatus();
                    }
                    else if (s1f3.SVID[i] == SECSConst.VID_EqReqStatus)
                    {
                        line.CurrentPortStateChecked = true;
                        s1f4.SV[i] = buildEqReqStatus();
                    }
                    else if (s1f3.SVID[i] == SECSConst.VID_EqPortInfo)
                    {
                        line.CurrentPortStateChecked = true;
                        s1f4.SV[i] = buildEqPortInfo();
                    }
                    else if (s1f3.SVID[i] == SECSConst.VID_UnitAlarmList)
                    {
                        line.CurrentPortStateChecked = true;
                        s1f4.SV[i] = buildUnitAlarmList();
                    }
                    else if (s1f3.SVID[i] == SECSConst.VID_MonitoredVehicles)
                    {
                        line.CurrentPortStateChecked = true;
                        s1f4.SV[i] = buildMonitoredVehicleInfo();
                    }
                    else if (s1f3.SVID[i] == SECSConst.VID_PortsLocationList)
                    {
                        line.CurrentPortStateChecked = true;
                        s1f4.SV[i] = buildPortLocationInfos();
                    }
                    else if (s1f3.SVID[i] == SECSConst.VID_SpecVersion)
                    {
                        line.CurrentPortStateChecked = true;
                        s1f4.SV[i] = buildSpecVersionInfo();
                    }
                    else if (s1f3.SVID[i] == SECSConst.VID_PortTypes)
                    {
                        line.CurrentPortStateChecked = true;
                        s1f4.SV[i] = buildPortTypeInfos();
                    }
                    else
                    {
                        s1f4.SV[i] = new SXFY();
                    }
                }
                TrxSECS.ReturnCode rtnCode = ISECSControl.replySECS(bcfApp, s1f4);
                SCUtility.secsActionRecordMsg(scApp, false, s1f4);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                              Data: s1f4);
            }
            catch (Exception ex)
            {
                logger.Error("AUOMCSDefaultMapAction has Error[Line Name:{0}],[Error method:{1}],[Error Message:{2}",
                    line.LINE_ID, "S1F3_Receive_Eqpt_Stat_Req", ex.ToString());
            }
        }
        private S6F11.RPTINFO.RPTITEM.VIDITEM_114 buildSpecVersionInfo()
        {
            S6F11.RPTINFO.RPTITEM.VIDITEM_114 items = new S6F11.RPTINFO.RPTITEM.VIDITEM_114();
            items.SpecVersion = "v1.1";

            return items;
        }

        private S6F11.RPTINFO.RPTITEM.VIDITEM_04 buildAlarmsSetVIDItem()
        {
            //var alarms = scApp.AlarmBLL.getCurrentAlarms();
            var alarms = scApp.AlarmBLL.getCurrentErrorAlarms();
            string[] alaem_ids = alarms.Select(alarm => SCUtility.Trim(alarm.ALAM_CODE, true)).ToArray();
            S6F11.RPTINFO.RPTITEM.VIDITEM_04 item = new S6F11.RPTINFO.RPTITEM.VIDITEM_04()
            {
                ALIDs = alaem_ids
            };
            return item;
        }
        private S6F11.RPTINFO.RPTITEM.VIDITEM_06 buildControlStateVIDItem()
        {
            string control_state = SCAppConstants.LineHostControlState.convert2MES(line.Host_Control_State);
            S6F11.RPTINFO.RPTITEM.VIDITEM_06 item = new S6F11.RPTINFO.RPTITEM.VIDITEM_06()
            {
                CONTROLSTATE = control_state
            };
            return item;
        }
        private S6F11.RPTINFO.RPTITEM.VIDITEM_40 buildEnhancedAlarmsSetVIDItem()
        {
            //var alarms = scApp.AlarmBLL.getCurrentAlarms();
            var alarms = scApp.AlarmBLL.getCurrentErrorAlarms();
            S6F11.RPTINFO.RPTITEM.ENHANCEDALID[] alarm_enhanced_alids = new S6F11.RPTINFO.RPTITEM.ENHANCEDALID[alarms.Count];
            for (int i = 0; i < alarms.Count; i++)
            {
                var alarm = alarms[i];
                string alid = alarm.ALAM_CODE;
                string eq_id = alarm.EQPT_ID;
                string text = alarm.ALAM_DESC;

                var check_result = scApp.VehicleBLL.cache.IsVehicleExist(eq_id);
                S6F11.RPTINFO.RPTITEM.VEHICLEINFO vEHICLEINFO = new S6F11.RPTINFO.RPTITEM.VEHICLEINFO();
                if (check_result.isExist)
                {
                    vEHICLEINFO.VehicleID = check_result.vh.Real_ID;
                    vEHICLEINFO.VehicleState = ((int)check_result.vh.State).ToString();
                }
                else
                {
                    vEHICLEINFO.VehicleState = "0";
                }
                alarm_enhanced_alids[i] = new S6F11.RPTINFO.RPTITEM.ENHANCEDALID()
                {
                    ALID = alid,
                    VehicleInfo = vEHICLEINFO,
                    AlarmText = text
                };
            }

            S6F11.RPTINFO.RPTITEM.VIDITEM_40 item = new S6F11.RPTINFO.RPTITEM.VIDITEM_40()
            {
                ENHANCED_ALIDs = alarm_enhanced_alids
            };
            return item;
        }
        private S6F11.RPTINFO.RPTITEM.VIDITEM_51 buildEnhanecdCarriersVIDItem()
        {
            var in_line_carriers = scApp.CarrierBLL.db.loadCurrentInLineCarrier();

            //in_line_carriers = in_line_carriers.Where(cst => cst.STATE == E_CARRIER_STATE.Installed).ToList();

            List<S6F11.RPTINFO.RPTITEM.ENHANCEDCARRIERINFO> enhanced_carier_info =
                new List<S6F11.RPTINFO.RPTITEM.ENHANCEDCARRIERINFO>();
            foreach (ACARRIER carrier in in_line_carriers)
            {
                string mcs_carrier_state = SECSConst.convert2MCS(carrier.STATE);
                S6F11.RPTINFO.RPTITEM.ENHANCEDCARRIERINFO info = new S6F11.RPTINFO.RPTITEM.ENHANCEDCARRIERINFO();
                info.CarrierID = SCUtility.Trim(carrier.ID);
                info.CarrierLoc = SCUtility.Trim(carrier.LOCATION);
                info.CarrierZoneName = SCUtility.Trim(carrier.LOCATION);
                info.InstallTime = carrier.INSTALLED_TIME?.ToString(SCAppConstants.TimestampFormat_16);
                info.CarrierState = mcs_carrier_state;

                enhanced_carier_info.Add(info);
            }
            S6F11.RPTINFO.RPTITEM.VIDITEM_51 item = new S6F11.RPTINFO.RPTITEM.VIDITEM_51()
            {
                ENHANCED_CARRIER_INFO = enhanced_carier_info.ToArray()
            };
            return item;
        }
        private S6F11.RPTINFO.RPTITEM.VIDITEM_53 buildEnhanecdVehiclesVIDItem()
        {
            var vhs = scApp.VehicleBLL.cache.loadAllVh();
            int vhs_count = vhs.Count;
            S6F11.RPTINFO.RPTITEM.VIDITEM_53 item = new S6F11.RPTINFO.RPTITEM.VIDITEM_53();
            item.VehicleInfos = new S6F11.RPTINFO.RPTITEM.VEHICLEINFO[vhs_count];
            for (int i = 0; i < vhs_count; i++)
            {
                item.VehicleInfos[i] = new S6F11.RPTINFO.RPTITEM.VEHICLEINFO()
                {
                    VehicleID = vhs[i].Real_ID,
                    VehicleState = ((int)vhs[i].State).ToString()
                };
            }
            return item;
        }
        private S6F11.RPTINFO.RPTITEM.VIDITEM_73 buildTCSStateVIDItem()
        {
            string tsc_state = ((int)line.TSC_state_machine.State).ToString();
            if (line.TSC_state_machine.State == ALINE.TSCState.NONE)
            {
                tsc_state = "1";

            }
            else
            {
                tsc_state = ((int)line.TSC_state_machine.State).ToString();

            }
            S6F11.RPTINFO.RPTITEM.VIDITEM_73 item = new S6F11.RPTINFO.RPTITEM.VIDITEM_73()
            {
                TSCSTATE = tsc_state
            };
            return item;
        }
        private S6F11.RPTINFO.RPTITEM.VIDITEM_76 buildEnhanecdTransfersItem()
        {
            var transfers = scApp.TransferBLL.db.transfer.loadUnfinishedTransfer();
            List<S6F11.RPTINFO.RPTITEM.VIDITEM_13> enhanced_transfer_cmds = new List<S6F11.RPTINFO.RPTITEM.VIDITEM_13>();
            foreach (var tran in transfers)
            {
                string transfer_state = SECSConst.convert2MCS(tran.TRANSFERSTATE);
                var vid_itrm_13 = new S6F11.RPTINFO.RPTITEM.VIDITEM_13();
                vid_itrm_13.TransferState = transfer_state;
                vid_itrm_13.CommandInfo.CommandID = SCUtility.Trim(tran.ID, true);
                vid_itrm_13.CommandInfo.Priority = tran.PRIORITY.ToString();
                vid_itrm_13.CommandInfo.Replace = tran.REPLACE.ToString();
                vid_itrm_13.TransferInfo.CarrierID = SCUtility.Trim(tran.CARRIER_ID, true);
                vid_itrm_13.TransferInfo.SourcePort = SCUtility.Trim(tran.HOSTSOURCE, true);
                vid_itrm_13.TransferInfo.DestPort = SCUtility.Trim(tran.HOSTDESTINATION, true);
                enhanced_transfer_cmds.Add(vid_itrm_13);
            }
            S6F11.RPTINFO.RPTITEM.VIDITEM_76 item = new S6F11.RPTINFO.RPTITEM.VIDITEM_76()
            {
                EnhancedTransferCmd = enhanced_transfer_cmds.ToArray()
            };
            return item;
        }
        private S6F11.RPTINFO.RPTITEM.VIDITEM_118 buildCurrentPortStatus()
        {
            List<APORTSTATION> port_station = scApp.PortStationBLL.OperateCatch.loadAllPortStation().ToList();
            //List<AGVStation> agv_stations = scApp.EqptBLL.OperateCatch.loadAllAGVStation();
            //foreach (var agv_station in agv_stations)
            //{
            //    port_station.Add(new APORTSTATION()
            //    {
            //        PORT_ID = SCUtility.Trim(agv_station.EQPT_ID, true)
            //    });
            //}
            int port_count = port_station.Count;
            S6F11.RPTINFO.RPTITEM.VIDITEM_118 item = new S6F11.RPTINFO.RPTITEM.VIDITEM_118();
            item.PortInfos = new S6F11.RPTINFO.RPTITEM.PORTTINFO[port_count];
            for (int i = 0; i < port_count; i++)
            {
                item.PortInfos[i] = new S6F11.RPTINFO.RPTITEM.PORTTINFO()
                {
                    PortID = port_station[i].PORT_ID,
                    PortTransferState = ((int)port_station[i].PORT_SERVICE_STATUS).ToString()
                    //PortTransferState = "2"
                };
            }
            return item;
        }
        private S6F11.RPTINFO.RPTITEM.VIDITEM_351 buildPortTypeInfos()
        {
            //List<APORTSTATION> port_station = scApp.PortStationBLL.OperateCatch.loadAllPortStation().ToList();
            //List<APORTSTATION> port_station = scApp.PortStationBLL.OperateDB.loadAll();
            List<APORTSTATION> port_station = scApp.PortStationBLL.OperateDB.loadAllNonEQPort(); //只須回報非EQ的Port就可以
            int port_count = port_station.Count;
            S6F11.RPTINFO.RPTITEM.VIDITEM_351 item = new S6F11.RPTINFO.RPTITEM.VIDITEM_351();
            item.PortTypes = new S6F11.RPTINFO.RPTITEM.PORTTYPEINFO[port_count];
            for (int i = 0; i < port_count; i++)
            {
                item.PortTypes[i] = new S6F11.RPTINFO.RPTITEM.PORTTYPEINFO()
                {
                    PortID = port_station[i].PORT_ID,
                    PortUnitType = port_station[i].PORT_DIR.ToString(),
                    //PortUnitType = "0"
                };
            }
            return item;
        }

        private string convertToMCS(PortStationStatus status)
        {
            switch (status)
            {
                case PortStationStatus.Disabled:
                case PortStationStatus.Down:
                case PortStationStatus.Wait:
                    return "0"; //Load REQ & Unload REQ off
                case PortStationStatus.LoadRequest:
                    return "1"; //Load REQ on
                case PortStationStatus.UnloadRequest:
                    return "2"; //Unload REQ on
                default:
                    return "0"; //Load REQ & Unload REQ off
            }
        }

        private S6F11.RPTINFO.RPTITEM.VIDITEM_350 buildCurrEqPortStatus()
        {
            List<APORTSTATION> port_station = scApp.PortStationBLL.OperateCatch.loadAllEQPortStation().ToList();
            int port_count = port_station.Count;
            S6F11.RPTINFO.RPTITEM.VIDITEM_350 item = new S6F11.RPTINFO.RPTITEM.VIDITEM_350();
            item.EqPortInfos = new S6F11.RPTINFO.RPTITEM.EQPORTINFO[port_count];

            for (int i = 0; i < port_count; i++)
            {
                item.EqPortInfos[i] = new S6F11.RPTINFO.RPTITEM.EQPORTINFO()
                {
                    PortID = port_station[i].PORT_ID,
                    PortTransferState = ((int)port_station[i].PORT_SERVICE_STATUS).ToString(),
                    EqReqSatus = convertToMCS(port_station[i].PORT_STATUS),
                    EqPresenceStatus = convertToMCS(port_station[i].PORT_STATUS) == "2" ? "1" : "0"
                };
            }
            return item;
        }



        private S6F11.RPTINFO.RPTITEM.VIDITEM_352 buildEqReqStatus()
        {
            S6F11.RPTINFO.RPTITEM.VIDITEM_352 item = new S6F11.RPTINFO.RPTITEM.VIDITEM_352()
            {
                EqReqSatus = ""
            };
            return item;
        }
        private S6F11.RPTINFO.RPTITEM.VIDITEM_356 buildEqPortInfo()
        {
            S6F11.RPTINFO.RPTITEM.VIDITEM_356 item = new S6F11.RPTINFO.RPTITEM.VIDITEM_356();
            return item;
        }
        private S6F11.RPTINFO.RPTITEM.VIDITEM_360 buildUnitAlarmList()
        {
            //var alarms = scApp.AlarmBLL.getCurrentAlarms();
            var alarms = scApp.AlarmBLL.getCurrentMinorAlarms();
            int alarmCount = alarms.Count;
            S6F11.RPTINFO.RPTITEM.VIDITEM_360 item = new S6F11.RPTINFO.RPTITEM.VIDITEM_360();
            item.UnitAlarmInfos = new S6F11.RPTINFO.RPTITEM.UNITALARMINFO[alarmCount];
            for (int i = 0; i < alarmCount; i++)
            {
                item.UnitAlarmInfos[i] = new S6F11.RPTINFO.RPTITEM.UNITALARMINFO()
                {

                    UnitID = alarms[i].EQPT_ID,
                    AlarmID = alarms[i].ALAM_CODE,
                    AlarmText = alarms[i].ALAM_DESC,
                    MaintState = "0"//不知道做什麼用，先填0
                };
            }

            return item;
        }

        private S6F11.RPTINFO.RPTITEM.VIDITEM_723 buildMonitoredVehicleInfo()
        {
            var vhs = scApp.VehicleBLL.cache.loadAllVh();
            int vh_count = vhs.Count;
            S6F11.RPTINFO.RPTITEM.VIDITEM_723 items = new S6F11.RPTINFO.RPTITEM.VIDITEM_723();
            items.MonitoredVehicles = new S6F11.RPTINFO.RPTITEM.MONITOREDVEHICLEINFO[vh_count];

            //foreach (var vh in vhs)
            for (int i = 0; i < vh_count; i++)
            {
                var vh = vhs[i];
                string vh_real_id = vh.Real_ID;
                string vh_last_position = "";
                string vh_current_position = $"[{vh.X_Axis},{vh.Y_Axis}]";
                string vh_next_position = "";
                string vh_state = SECSConst.convert2MCS(vh.State);
                string vh_communication = SECSConst.convert2MCS(vh.isTcpIpConnect, vh.IsCommunication(scApp.getBCFApplication()));
                string vh_control_mode = SECSConst.convert2MCS(vh.isTcpIpConnect, vh.MODE_STATUS);

                S6F11.RPTINFO.RPTITEM.MONITOREDVEHICLEINFO item = new S6F11.RPTINFO.RPTITEM.MONITOREDVEHICLEINFO();
                item.VehicleID = vh_real_id;
                item.VehicleLastPosition = vh_current_position;
                item.VehicleCurrentPosition = vh_current_position;
                item.VehicleNextPosition = vh_current_position;
                item.VehicleStatus = vh_state;
                item.VehiclecCommunication = vh_communication;
                item.VehcileControlMode = vh_control_mode;
                items.MonitoredVehicles[i] = item;
            }



            return items;
        }

        private S6F11.RPTINFO.RPTITEM.VIDITEM_728 buildPortLocationInfos()
        {
            var ports = scApp.PortStationBLL.OperateCatch.loadAllPortStation();
            int port_count = ports.Count;
            S6F11.RPTINFO.RPTITEM.VIDITEM_728 items = new S6F11.RPTINFO.RPTITEM.VIDITEM_728();
            List<S6F11.RPTINFO.RPTITEM.PORTLOCATIONINFO> port_location_info = new List<S6F11.RPTINFO.RPTITEM.PORTLOCATIONINFO>();
            foreach (var port in ports)
            {
                string port_id = port.PORT_ID;
                string port_position = "";
                var get_result = tryGetPortPositionByAddressID(port.ADR_ID);
                if (get_result.is_exist)
                {
                    port_position = get_result.port_position;
                }
                else
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                                  Data: $"Port ID:{port_id} of adr:{port.ADR_ID} no define x、y");
                    continue;
                }
                port_location_info.Add(new S6F11.RPTINFO.RPTITEM.PORTLOCATIONINFO()
                {
                    PortID = port_id,
                    PortPosition = port_position
                });

            }
            items.PortsLocationList = port_location_info.ToArray();


            return items;
        }

        private (bool is_exist, string port_position) tryGetPortPositionByAddressID(string adrID)
        {
            (bool isSuccess, double x, double y, bool isTR50) get_result =
                default((bool isSuccess, double x, double y, bool isTR50));
            double t_x = 0;
            double max_x = 0;

            try
            {
                max_x = scApp.ReserveBLL.GetMaxHltMapAddress_x();
                get_result = scApp.ReserveBLL.GetHltMapAddress(adrID);
                t_x = (get_result.x * -1) + max_x;
            }
            catch { }
            return (get_result.isSuccess, $"[{t_x},{get_result.y}]");
        }

        protected override void S1F15OffLineRequest(object sender, SECSEventArgs e)
        {
            try
            {
                S1F15 s1f15 = ((S1F15)e.secsHandler.Parse<S1F15>(e));
                SCUtility.secsActionRecordMsg(scApp, true, s1f15);
                SCUtility.actionRecordMsg(scApp, s1f15.StreamFunction, line.Real_ID,
                        "Receive Establish Communication From MES.", "");
                S1F16 s1f16 = new S1F16();
                s1f16.SECSAgentName = scApp.EAPSecsAgentName;
                s1f16.SystemByte = s1f15.SystemByte;
                s1f16.OFLACK = "0";
                SCUtility.secsActionRecordMsg(scApp, false, s1f16);
                TrxSECS.ReturnCode rtnCode = ISECSControl.replySECS(bcfApp, s1f16);
                SCUtility.actionRecordMsg(scApp, s1f15.StreamFunction, line.Real_ID,
                        "Reply Establish Communication To MES.", rtnCode.ToString());
                if (rtnCode != TrxSECS.ReturnCode.Normal)
                {
                    logger.Warn("Reply EAP S1F16 Error:{0}", rtnCode);
                }
            }
            catch (Exception ex)
            {
                logger.Error("MESDefaultMapAction has Error[Line Name:{0}],[Error method:{1}],[Error Message:{2}",
                    line.LINE_ID, nameof(S1F15OffLineRequest), ex.ToString());
            }
        }

        protected override void S2F15ReceiveSetNewEquiptment(object sender, SECSEventArgs e)
        {
            try
            {
                S2F15 s2f15 = ((S2F15)e.secsHandler.Parse<S2F15>(e));
                SCUtility.secsActionRecordMsg(scApp, true, s2f15);
                SCUtility.actionRecordMsg(scApp, s2f15.StreamFunction, line.Real_ID,
                        "Receive New EQPT Constant Data From MES.", "");
                if (!isProcess(s2f15)) { return; }

                S2F16 s2f16 = new S2F16();
                s2f16.SECSAgentName = scApp.EAPSecsAgentName;
                s2f16.SystemByte = s2f15.SystemByte;
                s2f16.EAC = "0";

                SCUtility.secsActionRecordMsg(scApp, false, s2f16);
                TrxSECS.ReturnCode rtnCode = ISECSControl.replySECS(bcfApp, s2f16);
                SCUtility.actionRecordMsg(scApp, s2f16.StreamFunction, line.Real_ID,
                        "Reply OK To MES.", rtnCode.ToString());
            }
            catch (Exception ex)
            {
                logger.Error("MESDefaultMapAction has Error[Line Name:{0}],[Error method:{1}],[Error Message:{2}",
                    line.LINE_ID, "S2F15_Receive_New_EQConstants", ex.ToString());
            }
        }
        protected override void S2F31ReceiveDateTimeSetReq(object sender, SECSEventArgs e)
        {
            try
            {
                S2F31 s2f31 = ((S2F31)e.secsHandler.Parse<S2F31>(e));

                SCUtility.secsActionRecordMsg(scApp, true, s2f31);
                SCUtility.actionRecordMsg(scApp, s2f31.StreamFunction, line.Real_ID,
                        "Receive Date Time Set Request From MES.", "");
                if (!isProcess(s2f31)) { return; }

                S2F32 s2f32 = new S2F32();
                s2f32.SECSAgentName = scApp.EAPSecsAgentName;
                s2f32.SystemByte = s2f31.SystemByte;
                s2f32.TIACK = SECSConst.TIACK_Accepted;

                string timeStr = s2f31.TIME;
                DateTime mesDateTime = DateTime.Now;
                try
                {
                    mesDateTime = DateTime.ParseExact(timeStr.Trim(), SCAppConstants.TimestampFormat_16, CultureInfo.CurrentCulture);
                }
                catch (Exception dtEx)
                {
                    s2f32.TIACK = SECSConst.TIACK_Error_not_done;
                }

                SCUtility.secsActionRecordMsg(scApp, false, s2f32);
                ISECSControl.replySECS(bcfApp, s2f32);

                if (!DebugParameter.DisableSyncTime)
                {
                    SCUtility.updateSystemTime(mesDateTime);
                }

                //TODO 與設備同步
            }
            catch (Exception ex)
            {
                logger.Error("MESDefaultMapAction has Error[Line Name:{0}],[Error method:{1}],[Error Message:{2}",
                    line.LINE_ID, "S2F31_Receive_Date_Time_Set_Req", ex.ToString());
            }
        }
        protected override void S2F37ReceiveEnableDisableEventReport(object sender, SECSEventArgs e)
        {
            try
            {
                S2F37 s2f37 = ((S2F37)e.secsHandler.Parse<S2F37>(e));
                SCUtility.secsActionRecordMsg(scApp, true, s2f37);
                //if (!isProcess(s2f37)) { return; }
                Boolean isValid = true;
                //Boolean isEnable = SCUtility.isMatche(s2f37.CEED, SECSConst.CEED_Enable);
                Boolean isEnable = s2f37.CEED[0] != 0;
                //Boolean isEnable = s2f37.CEED[0] == 255;

                int cnt = s2f37.CEIDS.Length;
                if (cnt == 0)
                {
                    isValid &= scApp.EventBLL.enableAllEventReport(isEnable);
                }
                else
                {
                    //Check Data
                    for (int ix = 0; ix < cnt; ++ix)
                    {
                        string ceid = s2f37.CEIDS[ix];
                        Boolean isContain = SECSConst.CEID_ARRAY.Contains(ceid.Trim());
                        if (!isContain)
                        {
                            isValid = false;
                            break;
                        }
                    }
                    if (isValid)
                    {
                        for (int ix = 0; ix < cnt; ++ix)
                        {
                            string ceid = s2f37.CEIDS[ix];
                            isValid &= scApp.EventBLL.enableEventReport(ceid, isEnable);
                        }
                    }
                }

                S2F38 s2f18 = null;
                s2f18 = new S2F38()
                {
                    SystemByte = s2f37.SystemByte,
                    SECSAgentName = scApp.EAPSecsAgentName,
                    ERACK = isValid ? SECSConst.ERACK_Accepted : SECSConst.ERACK_Denied_At_least_one_CEID_dose_not_exist
                };

                TrxSECS.ReturnCode rtnCode = ISECSControl.replySECS(bcfApp, s2f18);
                SCUtility.secsActionRecordMsg(scApp, false, s2f18);
                if (rtnCode != TrxSECS.ReturnCode.Normal)
                {
                    logger.Warn("Reply EQPT S2F18 Error:{0}", rtnCode);
                }
            }
            catch (Exception ex)
            {
                logger.Error("MESDefaultMapAction has Error[Line Name:{0}],[Error method:{1}],[Error Message:{2}", line.LINE_ID, "S2F17_Receive_Date_Time_Req", ex.ToString());
            }
        }
        protected override void S2F41ReceiveHostCommand(object sender, SECSEventArgs e)
        {
            try
            {
                S2F41 s2f41 = ((S2F41)e.secsHandler.Parse<S2F41>(e));
                SCUtility.secsActionRecordMsg(scApp, true, s2f41);

                S2F42 s2f42 = null;
                s2f42 = new S2F42();
                s2f42.SystemByte = s2f41.SystemByte;
                s2f42.SECSAgentName = scApp.EAPSecsAgentName;
                s2f42.HCACK = SECSConst.HCACK_Confirm_Executed;

                string mcs_cmd_id = string.Empty;
                bool needToResume = false;
                bool needToPause = false;
                bool canCancelCmd = false;
                bool canAbortCmd = false;
                string cancel_abort_cmd_id = string.Empty;
                bool canPortTypeChange = false;
                string portTypeChangePort_id = string.Empty;
                string portTypeChangePort_type = string.Empty;
                bool canDoPriorityUpdate = false;
                int priority = 0;
                string priorityUpdateCMDID = string.Empty;
                switch (s2f41.RCMD)
                {
                    case SECSConst.RCMD_Resume:
                        if (line.TSC_state_machine.State == ALINE.TSCState.PAUSED || line.TSC_state_machine.State == ALINE.TSCState.PAUSING)
                        {
                            s2f42.HCACK = SECSConst.HCACK_Confirm_Executed;
                            needToResume = true;
                        }
                        else
                        {
                            s2f42.HCACK = SECSConst.HCACK_Cannot_Perform_Now;
                            needToResume = false;
                        }
                        break;
                    case SECSConst.RCMD_Pause:
                        if (line.TSC_state_machine.State == ALINE.TSCState.AUTO)
                        {
                            s2f42.HCACK = SECSConst.HCACK_Confirm_Executed;
                            needToPause = true;
                        }
                        else
                        {
                            s2f42.HCACK = SECSConst.HCACK_Cannot_Perform_Now;
                            needToResume = false;
                        }
                        break;
                    case SECSConst.RCMD_Abort:
                        var abort_check_result = checkHostCommandAbort(s2f41);
                        canAbortCmd = abort_check_result.isOK;
                        s2f42.HCACK = abort_check_result.checkResult;
                        cancel_abort_cmd_id = abort_check_result.cmdID;
                        break;
                    case SECSConst.RCMD_Cancel:
                        var cancel_check_result = checkHostCommandCancel(s2f41);
                        canCancelCmd = cancel_check_result.isOK;
                        s2f42.HCACK = cancel_check_result.checkResult;
                        cancel_abort_cmd_id = cancel_check_result.cmdID;
                        break;
                    case SECSConst.RCMD_PortTypeChange:
                        var PortTypeChange_check_result = checkHostCommandPortTypeChange(s2f41);
                        canPortTypeChange = PortTypeChange_check_result.isOK;
                        s2f42.HCACK = PortTypeChange_check_result.checkResult;
                        portTypeChangePort_id = PortTypeChange_check_result.port_id;
                        portTypeChangePort_type = PortTypeChange_check_result.port_unit_type;
                        break;
                    case SECSConst.RCMD_StageDelete: //未實做，先都回OK
                        s2f42.HCACK = SECSConst.HCACK_Confirm;
                        break;
                    case SECSConst.RCMD_PriorityUpdate:
                        var priority_update_check_result = checkHostCommandPriorityUpdate(s2f41);
                        s2f42.HCACK = priority_update_check_result.checkResult;
                        canDoPriorityUpdate = priority_update_check_result.isOK;
                        priority = priority_update_check_result.priority;
                        priorityUpdateCMDID = priority_update_check_result.cmdID;
                        s2f42.HCACK = SECSConst.HCACK_Confirm;
                        break;



                }
                TrxSECS.ReturnCode rtnCode = ISECSControl.replySECS(bcfApp, s2f42);
                SCUtility.secsActionRecordMsg(scApp, false, s2f42);
                if (rtnCode != TrxSECS.ReturnCode.Normal)
                {
                    logger.Warn("Reply EQPT S2F18 Error:{0}", rtnCode);
                }
                if (needToResume)
                {
                    line.ResumeToAuto(reportBLL);
                }
                if (needToPause)
                {
                    //line.RequestToPause(reportBLL);
                    scApp.LineService.TSCStateToPause();
                }
                if (canCancelCmd)
                {
                    scApp.TransferService.AbortOrCancel(cancel_abort_cmd_id, ProtocolFormat.OHTMessage.CMDCancelType.CmdCancel);
                }
                if (canAbortCmd)
                {
                    scApp.TransferService.AbortOrCancel(cancel_abort_cmd_id, ProtocolFormat.OHTMessage.CMDCancelType.CmdAbort);

                }
                if (canPortTypeChange)
                {
                    //20210422 MarkChou收到命令不要直接發出轉換方向Event而要等到真的轉向再發。
                    //scApp.ReportBLL.newReportPortModeChange(portTypeChangePort_id, null);
                    //if (portTypeChangePort_type.Trim() == "0")
                    //{
                    //    scApp.ReportBLL.newReportPortInMode(portTypeChangePort_id, null);
                    //}
                    //else if (portTypeChangePort_type.Trim() == "1")
                    //{
                    //    scApp.ReportBLL.newReportPortOutMode(portTypeChangePort_id, null);
                    //}
                    APORTSTATION portStation = scApp.getEQObjCacheManager().getPortStation(portTypeChangePort_id);
                    if (portTypeChangePort_type.Trim() == "0") //in
                    {
                        string cmdID = "PortTypeChange-" + portStation.PORT_ID.Trim() + ">>" + E_PortType.In;
                        if (scApp.CMDBLL.GetTransferByID(cmdID) == null)
                        {
                            //若來源流向錯誤且沒有流向切換命令，就新建
                            scApp.TransferService.SetPortTypeCmd(portStation.PORT_ID.Trim(), E_PortType.In);  //要測時，把註解拿掉
                        }
                        else
                        {
                            //do nothing
                            //命令已經產生
                        }
                    }
                    else if (portTypeChangePort_type.Trim() == "1") //out
                    {
                        string cmdID = "PortTypeChange-" + portStation.PORT_ID.Trim() + ">>" + E_PortType.Out;
                        if (scApp.CMDBLL.GetTransferByID(cmdID) == null)
                        {
                            //若來源流向錯誤且沒有流向切換命令，就新建
                            scApp.TransferService.SetPortTypeCmd(portStation.PORT_ID.Trim(), E_PortType.Out);  //要測時，把註解拿掉
                        }
                        else
                        {
                            //do nothing
                            //命令已經產生
                        }
                    }

                }


                if (canDoPriorityUpdate)
                {
                    scApp.TransferService.Update(priorityUpdateCMDID, priority);
                    //scApp.ReportBLL.newReportPortModeChange(portTypeChangePort_id, null);
                    //if (portTypeChangePort_type.Trim() == "0")
                    //{
                    //    scApp.ReportBLL.newReportPortInMode(portTypeChangePort_id, null);
                    //}
                    //else if (portTypeChangePort_type.Trim() == "1")
                    //{
                    //    scApp.ReportBLL.newReportPortOutMode(portTypeChangePort_id, null);
                    //}

                }


            }
            catch (Exception ex)
            {
                logger.Error("MESDefaultMapAction has Error[Line Name:{0}],[Error method:{1}],[Error Message:{2}", line.LINE_ID, "S2F17_Receive_Date_Time_Req", ex.ToString());
            }
        }
        private (bool isOK, string checkResult, string cmdID) checkHostCommandAbort(S2F41 s2F41)
        {
            bool is_ok = true;
            string check_result = SECSConst.HCACK_Confirm;
            string command_id = string.Empty;
            var command_id_item = s2F41.REPITEMS.Where(item => SCUtility.isMatche(item.CPNAME, SECSConst.CPNAME_CommandID)).FirstOrDefault();
            if (command_id_item != null)
            {
                command_id = command_id_item.CPVAL;
                ATRANSFER cmd_mcs = scApp.CMDBLL.GetTransferByID(command_id);
                if (cmd_mcs == null)
                {
                    check_result = SECSConst.HCACK_Obj_Not_Exist;
                    is_ok = false;
                }
            }
            else
            {
                check_result = SECSConst.HCACK_Param_Invalid;
                is_ok = false;
            }
            return (is_ok, check_result, command_id);
        }
        private (bool isOK, string checkResult, string cmdID) checkHostCommandCancel(S2F41 s2F41)
        {
            bool is_ok = true;
            string check_result = SECSConst.HCACK_Confirm;
            string command_id = string.Empty;
            var command_id_item = s2F41.REPITEMS.Where(item => SCUtility.isMatche(item.CPNAME, SECSConst.CPNAME_CommandID)).FirstOrDefault();
            if (command_id_item != null)
            {
                command_id = command_id_item.CPVAL;
                ATRANSFER cmd_mcs = scApp.CMDBLL.GetTransferByID(command_id);
                if (cmd_mcs == null)
                {
                    check_result = SECSConst.HCACK_Obj_Not_Exist;
                    is_ok = false;
                }
            }
            else
            {
                check_result = SECSConst.HCACK_Param_Invalid;
                is_ok = false;
            }
            return (is_ok, check_result, command_id);
        }


        private (bool isOK, string checkResult, string port_id,string port_unit_type) checkHostCommandPortTypeChange(S2F41 s2F41)
        {
            bool is_ok = true;
            string check_result = SECSConst.HCACK_Confirm;
            string port_id = string.Empty;
            APORTSTATION portStation = null;
            var port_id_item = s2F41.REPITEMS.Where(item => SCUtility.isMatche(item.CPNAME, SECSConst.CPNAME_PortID)).FirstOrDefault();
            if (port_id_item != null)
            {
                port_id = port_id_item.CPVAL;
                //APORTSTATION port_station = scApp.PortStationBLL.OperateDB.get(port_id);
                portStation = scApp.getEQObjCacheManager().getPortStation(port_id);
                if (portStation == null)
                {
                    check_result = SECSConst.HCACK_Obj_Not_Exist;
                    is_ok = false;
                }


                //if (port_station == null)
                //{
                //    check_result = SECSConst.HCACK_Obj_Not_Exist;
                //    is_ok = false;
                //}
            }
            else
            {
                check_result = SECSConst.HCACK_Param_Invalid;
                is_ok = false;
            }


            string port_unit_type = string.Empty;
            var port_unit_type_item = s2F41.REPITEMS.Where(item => SCUtility.isMatche(item.CPNAME, SECSConst.CPNAME_PortUnitType)).FirstOrDefault();
            if(port_unit_type_item == null)
            {
                port_unit_type_item = s2F41.REPITEMS.Where(item => SCUtility.isMatche(item.CPNAME, SECSConst.CPNAME_PortType)).FirstOrDefault();
            }

            if (port_unit_type_item != null)
            {
                port_unit_type = port_unit_type_item.CPVAL;
                if (port_unit_type.Trim() != "0" && port_unit_type.Trim() != "1")
                {
                    check_result = SECSConst.HCACK_Obj_Not_Exist;
                    is_ok = false;
                }
            }
            else
            {
                check_result = SECSConst.HCACK_Param_Invalid;
                is_ok = false;
            }

            if (portStation != null)
            {
                PortValueDefMapAction mapAction = portStation.getMapActionByIdentityKey(typeof(PortValueDefMapAction).Name) as PortValueDefMapAction;
                if (mapAction == null)
                {
                    check_result = SECSConst.HCACK_Obj_Not_Exist;
                    is_ok = false;
                }
                else
                {
                    //if (portStation.IsAutoMode)
                    //{
                    if (port_unit_type.Trim() == "0")
                    {
                        if (portStation.IsInPutMode == false)
                        {
                            //if (portStation.IsModeChangeable)
                            //{
                            //}
                            //else
                            //{
                            //    check_result = SECSConst.HCACK_Cannot_Perform_Now;//Port當前無法切換狀態
                            //    is_ok = false;
                            //}
                        }
                        else
                        {
                            check_result = SECSConst.HCACK_Rejected;//Port已經是in mode了
                            is_ok = false;
                        }
                    }else if (port_unit_type.Trim() == "1")
                    {
                        if (portStation.IsOutPutMode == false)
                        {
                            //if (portStation.IsModeChangeable)
                            //{
                            //}
                            //else
                            //{
                            //    check_result = SECSConst.HCACK_Cannot_Perform_Now;//Port當前無法切換狀態
                            //    is_ok = false;
                            //}
                        }
                        else
                        {
                            check_result = SECSConst.HCACK_Rejected;//Port已經是out mode了
                            is_ok = false;
                        }
                    }
                    else
                    {
                        check_result = SECSConst.HCACK_Param_Invalid; //不知道要切in還是out
                        is_ok = false;
                    }
                    //}
                    //else
                    //{
                    //    check_result = SECSConst.HCACK_Cannot_Perform_Now;//Port不是Auto狀態
                    //    is_ok = false;
                    //}
                }
            }




            return (is_ok, check_result, port_id, port_unit_type);
        }

        private (bool isOK, string checkResult, string cmdID, int priority) checkHostCommandPriorityUpdate(S2F41 s2F41)
        {
            bool is_ok = true;
            string check_result = SECSConst.HCACK_Confirm;
            string command_id = string.Empty;
            string sPriority = string.Empty;
            int priority = 0;
            var command_id_item = s2F41.REPITEMS.Where(item => SCUtility.isMatche(item.CPNAME, SECSConst.CPNAME_CommandID)).FirstOrDefault();
            if (command_id_item != null)
            {
                command_id = command_id_item.CPVAL;
                ATRANSFER cmd_mcs = scApp.CMDBLL.GetTransferByID(command_id);
                if (cmd_mcs == null)
                {
                    check_result = SECSConst.HCACK_Obj_Not_Exist;
                    is_ok = false;
                }
            }
            else
            {
                check_result = SECSConst.HCACK_Param_Invalid;
                is_ok = false;
            }

            var priority_item = s2F41.REPITEMS.Where(item => SCUtility.isMatche(item.CPNAME, SECSConst.CPNAME_Priority)).FirstOrDefault();
            if (priority_item != null)
            {
                sPriority = priority_item.CPVAL;
                if (!int.TryParse(sPriority, out priority))
                {
                    check_result = SECSConst.HCACK_Obj_Not_Exist;
                    is_ok = false;
                }
            }
            else
            {
                check_result = SECSConst.HCACK_Param_Invalid;
                is_ok = false;
            }


            return (is_ok, check_result, command_id, priority);
        }

        protected override void S2F49ReceiveEnhancedRemoteCommandExtension(object sender, SECSEventArgs e)
        {
            try
            {
                if (scApp.getEQObjCacheManager().getLine().ServerPreStop)
                    return;
                string errorMsg = string.Empty;
                S2F49 s2f49 = ((S2F49)e.secsHandler.Parse<S2F49>(e));

                switch (s2f49.RCMD)
                {
                    case "TRANSFER":
                        S2F49_TRANSFER s2f49_transfer = ((S2F49_TRANSFER)e.secsHandler.Parse<S2F49_TRANSFER>(e));
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                           Data: s2f49_transfer);
                        SCUtility.secsActionRecordMsg(scApp, true, s2f49_transfer);
                        //SCUtility.RecodeReportInfo(s2f49_transfer);
                        //if (!isProcessEAP(s2f49)) { return; }

                        S2F50 s2f50 = new S2F50();
                        s2f50.SystemByte = s2f49_transfer.SystemByte;
                        s2f50.SECSAgentName = scApp.EAPSecsAgentName;
                        s2f50.HCACK = SECSConst.HCACK_Confirm;

                        string rtnStr = "";
                        var check_result = doCheckMCSCommand(s2f49_transfer, ref s2f50, out rtnStr);
                        s2f50.HCACK = check_result.result;
                        ATRANSFER transfer = s2f49_transfer.ToTRANSFER(scApp.PortStationBLL, scApp.EqptBLL);
                        transfer.SetCheckResult(check_result.isSuccess, check_result.result);
                        // todo continue 20200131
                        bool is_process_success = true;
                        using (TransactionScope tx = SCUtility.getTransactionScope())
                        {
                            using (DBConnection_EF con = DBConnection_EF.GetUContext())
                            {
                                is_process_success &= scApp.TransferService.Creat(transfer);
                                if (is_process_success)
                                {
                                    //not thing...
                                }
                                else
                                {
                                    s2f50.HCACK = SECSConst.HCACK_Rejected;
                                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: string.Empty,
                                                  Data: $"creat mcs command fail, command info:{transfer.ToString()}");
                                }

                                TrxSECS.ReturnCode rtnCode = ISECSControl.replySECS(bcfApp, s2f50);
                                SCUtility.secsActionRecordMsg(scApp, false, s2f50);
                                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                                   Data: s2f50);
                                if (rtnCode != TrxSECS.ReturnCode.Normal)
                                {
                                    logger.Warn("Reply EQPT S2F50) Error:{0}", rtnCode);
                                    is_process_success = false;
                                }

                                if (is_process_success)
                                {
                                    tx.Complete();
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                        if (is_process_success && check_result.isSuccess)
                        {
                            //scApp.TransferService.Scan();
                            scApp.TransferBLL.web.receiveMCSCommandNotify();
                        }
                        else
                        {
                            string xid = DateTime.Now.ToString(SCAppConstants.TimestampFormat_19);
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: string.Empty,
                                          Data: rtnStr,
                                          XID: xid);
                            BCFApplication.onWarningMsg(this, new bcf.Common.LogEventArgs(rtnStr, xid));
                        }
                        break;
                    case "STAGE":
                        S2F49_STAGE s2f49_stage = ((S2F49_STAGE)e.secsHandler.Parse<S2F49_STAGE>(e));

                        S2F50 s2f50_stage = new S2F50();
                        s2f50_stage.SystemByte = s2f49_stage.SystemByte;
                        s2f50_stage.SECSAgentName = scApp.EAPSecsAgentName;
                        s2f50_stage.HCACK = SECSConst.HCACK_Confirm;

                        string source_port_id = s2f49_stage.REPITEMS.TRANSFERINFO.CPVALUE.SOURCEPORT_CP.CPVAL_ASCII;
                        TrxSECS.ReturnCode rtnCode_stage = ISECSControl.replySECS(bcfApp, s2f50_stage);
                        SCUtility.secsActionRecordMsg(scApp, false, s2f50_stage);


                        if (scApp.getEQObjCacheManager().getLine().Host_Control_State
                             == SCAppConstants.LineHostControlState.HostControlState.On_Line_Local)
                        {
                            //如果已經切到OnlineLocal狀態，則不處理Stage命令
                            break;
                        }

                        //TODO Stage
                        //將收下來的Stage命令先放到Redis上
                        //等待Timer發現後會將此命令取下來並下命令給車子去執行
                        //(此處將再考慮是要透過Timer或是開Thread來監控這件事)
                        var port = scApp.MapBLL.getPortByPortID(source_port_id);
                        AVEHICLE vh_test = scApp.VehicleBLL.cache.findBestSuitableVhStepByStepFromAdr(scApp.GuideBLL, scApp.CMDBLL, port.ADR_ID, port.LD_VH_TYPE);
                        scApp.VehicleService.Command.Move(vh_test.VEHICLE_ID, port.ADR_ID);
                        //2021.9.29 綁定vehicle - port
                        scApp.TransferService.RegisterStagedVehicle(vh_test.VEHICLE_ID, source_port_id);
                        break;
                }
                line.CommunicationIntervalWithMCS.Restart();

            }
            catch (Exception ex)
            {
                logger.Error("MESDefaultMapAction has Error[Line Name:{0}],[Error method:{1}],[Error Message:{2}", line.LINE_ID, "S2F49_Receive_Remote_Command", ex);
            }
        }
        public (bool isSuccess, string result) doCheckMCSCommand(S2F49_TRANSFER s2F49_TRANSFER, ref S2F50 s2F50, out string check_result)
        {
            check_result = string.Empty;
            string checkcode = SECSConst.HCACK_Confirm;
            bool isSuccess = true;
            List<S2F50.CP_U1> comminfo_check_result = new List<S2F50.CP_U1>();
            List<S2F50.CP_U1> traninfo_check_result = new List<S2F50.CP_U1>();
            string command_id = s2F49_TRANSFER.REPITEMS.COMMINFO.COMMAINFOVALUE.COMMANDID.CPVAL;
            string priority = s2F49_TRANSFER.REPITEMS.COMMINFO.COMMAINFOVALUE.PRIORITY.CPVAL;
            string replace = s2F49_TRANSFER.REPITEMS.COMMINFO.COMMAINFOVALUE.REPLACE.CPVAL;

            string carrier_id = s2F49_TRANSFER.REPITEMS.TRANINFO.TRANSFERINFOVALUE.CARRIERIDINFO.CPVAL;
            string source_port_or_vh_location_id = s2F49_TRANSFER.REPITEMS.TRANINFO.TRANSFERINFOVALUE.SOUINFO.CPVAL;
            string dest_port = s2F49_TRANSFER.REPITEMS.TRANINFO.TRANSFERINFOVALUE.DESTINFO.CPVAL;

            //確認當前Control State是不是OnlineLocal
            if (isSuccess)
            {
                if (scApp.getEQObjCacheManager().getLine().Host_Control_State
                    == SCAppConstants.LineHostControlState.HostControlState.On_Line_Local)
                {
                    check_result = $"Host Control State is [{SCAppConstants.LineHostControlState.HostControlState.On_Line_Local}],reject command.";
                    return (false, SECSConst.HCACK_Rejected);
                }
            }


            //確認命令是否已經執行中
            if (isSuccess)
            {
                var cmd_obj = scApp.CMDBLL.GetTransferByID(command_id);
                if (cmd_obj != null)
                {
                    check_result = $"MCS command id:{command_id} already exist.";
                    return (false, SECSConst.HCACK_Rejected);
                }
            }
            //確認參數是否正確
            isSuccess &= checkCommandID(comminfo_check_result, s2F49_TRANSFER.REPITEMS.COMMINFO.COMMAINFOVALUE.COMMANDID.CPNAME, command_id);
            isSuccess &= checkPriorityID(comminfo_check_result, s2F49_TRANSFER.REPITEMS.COMMINFO.COMMAINFOVALUE.PRIORITY.CPNAME, priority);
            isSuccess &= checkReplace(comminfo_check_result, s2F49_TRANSFER.REPITEMS.COMMINFO.COMMAINFOVALUE.REPLACE.CPNAME, replace);

            isSuccess &= checkCarierID(traninfo_check_result, s2F49_TRANSFER.REPITEMS.TRANINFO.TRANSFERINFOVALUE.CARRIERIDINFO.CPNAME, carrier_id);
            isSuccess &= checkPortID(traninfo_check_result, s2F49_TRANSFER.REPITEMS.TRANINFO.TRANSFERINFOVALUE.SOUINFO.CPNAME, source_port_or_vh_location_id);
            isSuccess &= checkPortID(traninfo_check_result, s2F49_TRANSFER.REPITEMS.TRANINFO.TRANSFERINFOVALUE.DESTINFO.CPNAME, dest_port);

            List<SXFY> cep_items = new List<SXFY>();
            if (comminfo_check_result.Count > 0)
            {
                S2F50.CEPITEM comm_info_cepack = new S2F50.CEPITEM()
                {
                    NAME = s2F49_TRANSFER.REPITEMS.COMMINFO.COMMANDINFONAME,
                    CPINFO = comminfo_check_result.ToArray()
                };
                cep_items.Add(comm_info_cepack);
            }
            if (traninfo_check_result.Count > 0)
            {
                S2F50.CEPITEMS transfer_info_cepack = new S2F50.CEPITEMS();
                transfer_info_cepack.CEPINFO = new S2F50.CEPITEM[1];
                transfer_info_cepack.CEPINFO[0] = new S2F50.CEPITEM();
                transfer_info_cepack.CEPINFO[0].NAME = s2F49_TRANSFER.REPITEMS.TRANINFO.TRANSFERINFONAME;
                transfer_info_cepack.CEPINFO[0].CPINFO = traninfo_check_result.ToArray();
                cep_items.Add(transfer_info_cepack);
            }
            s2F50.CEPCOLLECT = cep_items.ToArray();

            if (!isSuccess)
            {
                check_result = $"MCS command id:{command_id} has parameter invalid";
                return (false, SECSConst.HCACK_Param_Invalid);
            }

            //確認是否有同一顆正在搬送的CST ID
            if (isSuccess)
            {
                var cmd_obj = scApp.CMDBLL.getExcuteCMD_MCSByCarrierID(carrier_id);
                if (cmd_obj != null)
                {
                    check_result = $"MCS command id:{command_id} of carrier id:{carrier_id} already excute by command id:{cmd_obj.ID.Trim()}";
                    return (false, SECSConst.HCACK_Rejected);
                }
            }

            //確認是否有在相同Load Port的Transfer Command且該命令狀態還沒有變成Transferring(代表還在Port上還沒搬走)
            if (isSuccess)
            {
                //M0.02 var cmd_obj = scApp.CMDBLL.getWatingCMDByFromTo(source_port_or_vh_id, dest_port);
                var cmd_obj = scApp.CMDBLL.getWatingCMDByFrom(source_port_or_vh_location_id);//M0.02 
                if (cmd_obj != null)
                {
                    check_result = $"MCS command id:{command_id} is same as orther mcs command id {cmd_obj.ID.Trim()} of load port.";//M0.02 
                                                                                                                                     //M0.02 check_result = $"MCS command id:{command_id} of transfer load port is same command id:{cmd_obj.CMD_ID.Trim()}";
                    return (false, SECSConst.HCACK_Rejected);
                }
            }

            //確認 Port是否存在
            bool source_is_a_port = scApp.PortStationBLL.OperateCatch.IsExist(source_port_or_vh_location_id);
            if (source_is_a_port)
            {
                isSuccess = true;
            }
            //如果不是PortID的話，則可能是VehicleID
            else
            {
                //isSuccess = scApp.VehicleBLL.cache.IsVehicleExistByRealID(source_port_or_vh_id);
                isSuccess = scApp.VehicleBLL.cache.IsVehicleLocationExistByLocationRealID(source_port_or_vh_location_id);
            }
            if (!isSuccess)
            {
                check_result = $"MCS command id:{command_id} - source Port:{source_port_or_vh_location_id} not exist.{Environment.NewLine}please confirm the port name";
                return (false, SECSConst.HCACK_Obj_Not_Exist);
            }

            isSuccess = scApp.PortStationBLL.OperateCatch.IsExist(dest_port);
            if (!isSuccess)
            {
                isSuccess = scApp.VehicleBLL.cache.IsVehicleLocationExistByLocationRealID(dest_port);
                if (!isSuccess)
                {
                    check_result = $"MCS command id:{command_id} - destination Port:{dest_port} not exist.{Environment.NewLine}please confirm the port name";
                    return (false, SECSConst.HCACK_Obj_Not_Exist);
                }
            }

            //如果Source是個Port才需要檢查
            if (source_is_a_port)
            {

                ////確認路徑是否可以行走
                APORTSTATION source_port_station = scApp.PortStationBLL.OperateCatch.getPortStation(source_port_or_vh_location_id);
                APORTSTATION dest_port_station = scApp.PortStationBLL.OperateCatch.getPortStation(dest_port);
                if (dest_port_station != null)
                {
                    isSuccess = scApp.GuideBLL.IsRoadWalkable(source_port_station.ADR_ID, dest_port_station.ADR_ID);
                    if (!isSuccess)
                    {
                        check_result = $"MCS command id:{command_id} ,source port:{source_port_or_vh_location_id} to destination port:{dest_port} no path to go{Environment.NewLine}," +
                            $"please check the road traffic status.";
                        return (false, SECSConst.HCACK_Cannot_Perform_Now);
                    }
                }
                else
                {
                    AVEHICLE carry_vh = scApp.VehicleBLL.cache.getVehicleByLocationRealID(dest_port);
                    isSuccess = scApp.GuideBLL.IsRoadWalkable(carry_vh.CUR_ADR_ID, source_port_station.ADR_ID);
                    if (!isSuccess)
                    {
                        check_result = $"MCS command id:{command_id} ,source port:{source_port_or_vh_location_id} to destination port:{dest_port} no path to go{Environment.NewLine}," +
                            $"please check the road traffic status.";
                        return (false, SECSConst.HCACK_Cannot_Perform_Now);
                    }
                }

            }
            //如果不是Port(則為指定車號)，要檢查是否從該車位置可以到達放貨地點
            else
            {
                //AVEHICLE carry_vh = scApp.VehicleBLL.cache.getVehicleByRealID(source_port_or_vh_id);
                AVEHICLE carry_vh = scApp.VehicleBLL.cache.getVehicleByLocationRealID(source_port_or_vh_location_id);
                APORTSTATION dest_port_station = scApp.PortStationBLL.OperateCatch.getPortStation(dest_port);
                isSuccess = scApp.GuideBLL.IsRoadWalkable(carry_vh.CUR_ADR_ID, dest_port_station.ADR_ID);
                if (!isSuccess)
                {
                    check_result = $"MCS command id:{command_id} ,vh:{source_port_or_vh_location_id} current address:{carry_vh.CUR_ADR_ID} to destination port:{dest_port}:{dest_port_station.ADR_ID} no path to go{Environment.NewLine}," +
                        $"please check the road traffic status.";
                    return (false, SECSConst.HCACK_Cannot_Perform_Now);
                }
            }

            return (true, SECSConst.HCACK_Confirm);
        }
        private bool checkCommandID(List<S2F50.CP_U1> comminfo_check_result, string name, string value)
        {
            bool is_success = !SCUtility.isEmpty(value);
            string cepack = is_success ? SECSConst.CEPACK_No_Error : SECSConst.CEPACK_Incorrect_Value_In_CEPVAL;
            if (!is_success)
            {
                S2F50.CP_U1 info = new S2F50.CP_U1()
                {
                    CPNAME = name,
                    CEPACK = cepack
                };
                comminfo_check_result.Add(info);
            }
            return is_success;
        }
        private bool checkPriorityID(List<S2F50.CP_U1> comminfo_check_result, string name, string value)
        {
            int i_priority = 0;
            bool is_success = int.TryParse(value, out i_priority);
            string cepack = is_success ?
                             SECSConst.CEPACK_No_Error : SECSConst.CEPACK_Incorrect_Value_In_CEPVAL;
            if (!is_success)
            {
                S2F50.CP_U1 info = new S2F50.CP_U1()
                {
                    CPNAME = name,
                    CEPACK = cepack
                };
                comminfo_check_result.Add(info);
            }
            return is_success;
        }
        private bool checkReplace(List<S2F50.CP_U1> comminfo_check_result, string name, string value)
        {
            bool is_success = true;
            int i_replace = 0;
            if (SCUtility.isEmpty(value))
            {
                is_success = true;
            }
            else
            {
                is_success = int.TryParse(value, out i_replace);
                string cepack = is_success ?
                                 SECSConst.CEPACK_No_Error : SECSConst.CEPACK_Incorrect_Value_In_CEPVAL;
                if (!is_success)
                {
                    S2F50.CP_U1 info = new S2F50.CP_U1()
                    {
                        CPNAME = name,
                        CEPACK = cepack
                    };
                    comminfo_check_result.Add(info);
                }
            }
            return is_success;
        }
        private bool checkCarierID(List<S2F50.CP_U1> trnasferinfo_check_result, string name, string value)
        {
            bool is_success = !SCUtility.isEmpty(value);
            string cepack = is_success ?
                            SECSConst.CEPACK_No_Error : SECSConst.CEPACK_Incorrect_Value_In_CEPVAL;
            if (!is_success)
            {
                S2F50.CP_U1 info = new S2F50.CP_U1()
                {
                    CPNAME = name,
                    CEPACK = cepack
                };
                trnasferinfo_check_result.Add(info);
            }
            return is_success;
        }
        private bool checkPortID(List<S2F50.CP_U1> trnasferinfo_check_result, string name, string value)
        {
            bool is_success = !SCUtility.isEmpty(value);
            string cepack = is_success ?
                            SECSConst.CEPACK_No_Error : SECSConst.CEPACK_Incorrect_Value_In_CEPVAL;
            if (!is_success)
            {
                S2F50.CP_U1 info = new S2F50.CP_U1()
                {
                    CPNAME = name,
                    CEPACK = cepack
                };
                trnasferinfo_check_result.Add(info);
            }
            return is_success;
        }

        #endregion Receive 

        #region Send
        public override bool S5F1SendAlarmReport(string alcd, string alid, string altx)
        {
            try
            {
                S5F1 s5f1 = new S5F1()
                {
                    SECSAgentName = scApp.EAPSecsAgentName,
                    ALCD = alcd,
                    ALID = alid,
                    ALTX = altx
                };
                S5F2 s5f2 = null;
                SXFY abortSecs = null;
                String rtnMsg = string.Empty;
                if (isSend())
                {
                    TrxSECS.ReturnCode rtnCode = ISECSControl.sendRecv<S5F2>(bcfApp, s5f1, out s5f2,
                        out abortSecs, out rtnMsg, null);
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(GEMDriver), Device: DEVICE_NAME_MCS,
                       Data: s5f1);
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(GEMDriver), Device: DEVICE_NAME_MCS,
                       Data: s5f2);
                    SCUtility.actionRecordMsg(scApp, s5f1.StreamFunction, line.Real_ID,
                        "Send Alarm Report.", rtnCode.ToString());
                    if (rtnCode != TrxSECS.ReturnCode.Normal)
                    {
                        logger.Warn("Send Alarm Report[S5F1] Error![rtnCode={0}]", rtnCode);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
                return false;
            }
        }

        public override bool S6F11SendEquiptmentOffLine()
        {
            try
            {
                VIDCollection Vids = new VIDCollection();
                Vids.VID_61_EqpName.EqpName = line.LINE_ID;
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Equipment_OFF_LINE, Vids);
                scApp.ReportBLL.insertMCSReport(mcs_queue);
                S6F11SendMessage(mcs_queue);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
            }
            return true;
        }
        public override bool S6F11SendControlStateLocal()
        {
            try
            {
                string control_state = SCAppConstants.LineHostControlState.convert2MES(line.Host_Control_State);

                VIDCollection Vids = new VIDCollection();
                //Vids.VID_06_ControlState.CONTROLSTATE = line.LINE_ID;
                //Vids.VID_61_EqpName.EqpName = control_state;
                Vids.VID_06_ControlState.CONTROLSTATE = control_state;
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Control_Status_Local, Vids);
                scApp.ReportBLL.insertMCSReport(mcs_queue);
                S6F11SendMessage(mcs_queue);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
            }
            return true;
        }
        public override bool S6F11SendControlStateRemote()
        {
            try
            {
                VIDCollection Vids = new VIDCollection();
                Vids.VID_61_EqpName.EqpName = line.LINE_ID;
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Control_Status_Remote, Vids);
                scApp.ReportBLL.insertMCSReport(mcs_queue);
                S6F11SendMessage(mcs_queue);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
            }
            return true;
        }

        public override bool S6F11SendTSCAutoInitiated()
        {
            try
            {
                VIDCollection Vids = new VIDCollection();
                Vids.VID_61_EqpName.EqpName = line.LINE_ID;
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_TSC_Auto_Initiated, Vids);
                scApp.ReportBLL.insertMCSReport(mcs_queue);
                S6F11SendMessage(mcs_queue);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
            }
            return true;
        }

        public override bool S6F11SendTSCPaused()
        {
            try
            {
                VIDCollection Vids = new VIDCollection();
                Vids.VID_61_EqpName.EqpName = line.LINE_ID;
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_TSC_Paused, Vids);
                scApp.ReportBLL.insertMCSReport(mcs_queue);
                S6F11SendMessage(mcs_queue);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
            }
            return true;
        }

        public override bool S6F11SendTSCAutoCompleted()
        {
            try
            {
                VIDCollection Vids = new VIDCollection();
                Vids.VID_61_EqpName.EqpName = line.LINE_ID;
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_TSC_Auto_Completed, Vids);
                scApp.ReportBLL.insertMCSReport(mcs_queue);
                S6F11SendMessage(mcs_queue);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
            }
            return true;
        }

        public override bool S6F11SendTSCPauseInitiated()
        {
            try
            {
                VIDCollection Vids = new VIDCollection();
                Vids.VID_61_EqpName.EqpName = line.LINE_ID;
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_TSC_Pause_Initiated, Vids);
                scApp.ReportBLL.insertMCSReport(mcs_queue);
                S6F11SendMessage(mcs_queue);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
            }
            return true;
        }

        public override bool S6F11SendTSCPauseCompleted()
        {
            try
            {
                VIDCollection Vids = new VIDCollection();
                Vids.VID_61_EqpName.EqpName = line.LINE_ID;
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_TSC_Pause_Completed, Vids);
                scApp.ReportBLL.insertMCSReport(mcs_queue);
                S6F11SendMessage(mcs_queue);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
            }
            return true;
        }

        public override bool S6F11SendAlarmCleared()
        {
            try
            {
                VIDCollection Vids = new VIDCollection();
                Vids.VID_61_EqpName.EqpName = line.LINE_ID;
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Alarm_Cleared, Vids);
                scApp.ReportBLL.insertMCSReport(mcs_queue);
                S6F11SendMessage(mcs_queue);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
            }
            return true;
        }

        public override bool S6F11SendAlarmSet()
        {
            try
            {
                VIDCollection Vids = new VIDCollection();
                Vids.VID_61_EqpName.EqpName = line.LINE_ID;
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Alarm_Set, Vids);
                scApp.ReportBLL.insertMCSReport(mcs_queue);
                S6F11SendMessage(mcs_queue);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
            }
            return true;
        }




        public override bool S6F11SendTransferInitial(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                VTRANSFER vtransfer = scApp.TransferBLL.db.vTransfer.GetVTransferByTransferID(cmdID);
                VIDCollection vid_collection = AVIDINFO2VIDCollection(vtransfer);
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Transfer_Initiated, vid_collection);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
            }
            return true;
        }

        public override bool S6F11SendTransferring(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                if (!isSend()) return true;
                VTRANSFER vtransfer = scApp.TransferBLL.db.vTransfer.GetVTransferByTransferID(cmdID);
                VIDCollection vid_collection = AVIDINFO2VIDCollection(vtransfer);
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Transferring, vid_collection);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }
        public override bool S6F11SendVehicleArrived(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                if (!isSend()) return true;
                VTRANSFER vtransfer = scApp.TransferBLL.db.vTransfer.GetVTransferByTransferID(cmdID);
                VIDCollection vid_collection = AVIDINFO2VIDCollection(vtransfer);
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Vehicle_Arrived, vid_collection);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }

        public override bool S6F11SendVehicleAcquireStarted(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                if (!isSend()) return true;
                VTRANSFER vtransfer = scApp.TransferBLL.db.vTransfer.GetVTransferByTransferID(cmdID);
                VIDCollection vid_collection = AVIDINFO2VIDCollection(vtransfer);
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Vehicle_Acquire_Started, vid_collection);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }

        public override bool S6F11SendVehicleAcquireCompleted(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                if (!isSend()) return true;
                VTRANSFER vtransfer = scApp.TransferBLL.db.vTransfer.GetVTransferByTransferID(cmdID);
                VIDCollection vid_collection = AVIDINFO2VIDCollection(vtransfer);
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Vehicle_Acquire_Completed, vid_collection);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }

        public override bool S6F11SendVehicleAssigned(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                if (!isSend()) return true;
                VTRANSFER vtransfer = scApp.TransferBLL.db.vTransfer.GetVTransferByTransferID(cmdID);
                VIDCollection vid_collection = AVIDINFO2VIDCollection(vtransfer);
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Vehicle_Assigned, vid_collection);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }

        public override bool S6F11SendVehicleDeparted(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                if (!isSend()) return true;
                VTRANSFER vtransfer = scApp.TransferBLL.db.vTransfer.GetVTransferByTransferID(cmdID);
                VIDCollection vid_collection = AVIDINFO2VIDCollection(vtransfer);
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Vehicle_Departed, vid_collection);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }

        public override bool S6F11SendVehicleDepositStarted(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                if (!isSend()) return true;
                VTRANSFER vtransfer = scApp.TransferBLL.db.vTransfer.GetVTransferByTransferID(cmdID);
                VIDCollection vid_collection = AVIDINFO2VIDCollection(vtransfer);
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Vehicle_Deposit_Started, vid_collection);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }

        public override bool S6F11SendVehicleDepositCompleted(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                if (!isSend()) return true;
                VTRANSFER vtransfer = scApp.TransferBLL.db.vTransfer.GetVTransferByTransferID(cmdID);
                VIDCollection vid_collection = AVIDINFO2VIDCollection(vtransfer);
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Vehicle_Deposit_Completed, vid_collection);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }

        public override bool S6F11SendCarrierIDRead(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                //if (!isSend()) return true;
                //VTRANSFER vtransfer = scApp.TransferBLL.db.vTransfer.GetVTransferByTransferID(cmdID);
                //VIDCollection vid_collection = AVIDINFO2VIDCollection(vtransfer);
                //AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Carrier_ID_Read, vid_collection);
                //if (reportQueues == null)
                //{
                //    S6F11SendMessage(mcs_queue);
                //}
                //else
                //{
                //    reportQueues.Add(mcs_queue);
                //}
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }
        public override bool S6F11SendCarrierInstalled(string vhRealID, string carrierID, string location, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                if (!isSend()) return true;
                VIDCollection vid_collection = new VIDCollection();
                vid_collection.VID_70_VehicleID.VehicleID = SCUtility.Trim(vhRealID, true);
                vid_collection.VID_54_CarrierID.CarrierID = SCUtility.Trim(carrierID, true);
                vid_collection.VID_56_CarrierLoc.CarrierLoc = SCUtility.Trim(location, true);
                vid_collection.VID_58_CommandID.CommandID = "";

                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Carrier_Installed, vid_collection);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }
        public override bool S6F11SendCarrierInstalled(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                if (!isSend()) return true;
                VTRANSFER vtransfer = scApp.TransferBLL.db.vTransfer.GetVTransferByTransferID(cmdID);
                VIDCollection vid_collection = AVIDINFO2VIDCollection(vtransfer);
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Carrier_Installed, vid_collection);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }

        public override bool S6F11SendCarrierRemoved(string vhRealID, string carrierID, string location, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            //try
            //{
            //    if (!isSend()) return true;
            //    VIDCollection vid_collection = new VIDCollection();
            //    vid_collection.VID_70_VehicleID.VehicleID = SCUtility.Trim(vhRealID, true);
            //    vid_collection.VID_54_CarrierID.CarrierID = SCUtility.Trim(carrierID, true);
            //    vid_collection.VID_56_CarrierLoc.CarrierLoc = SCUtility.Trim(location, true);
            //    vid_collection.VID_58_CommandID.CommandID = "";

            //    AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Carrier_Removed, vid_collection);
            //    if (reportQueues == null)
            //    {
            //        S6F11SendMessage(mcs_queue);
            //    }
            //    else
            //    {
            //        reportQueues.Add(mcs_queue);
            //    }
            //    return true;
            //}
            //catch (Exception ex)
            //{
            //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(ASEMCSDefaultMapAction), Device: DEVICE_NAME_MCS,
            //       Data: ex);
            //    return false;
            //}
            return false;
        }
        public override bool S6F11SendCarrierRemoved(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            //try
            //{
            //    if (!isSend()) return true;
            //    VTRANSFER vtransfer = scApp.TransferBLL.db.vTransfer.GetVTransferByTransferID(cmdID);
            //    VIDCollection vid_collection = AVIDINFO2VIDCollection(vtransfer);
            //    AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Carrier_Removed, vid_collection);
            //    if (reportQueues == null)
            //    {
            //        S6F11SendMessage(mcs_queue);
            //    }
            //    else
            //    {
            //        reportQueues.Add(mcs_queue);
            //    }
            //    return true;
            //}
            //catch (Exception ex)
            //{
            //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(ASEMCSDefaultMapAction), Device: DEVICE_NAME_MCS,
            //       Data: ex);
            //    return false;
            //}
            return false;

        }

        public override bool S6F11SendVehicleUnassinged(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                if (!isSend()) return true;
                VTRANSFER vtransfer = scApp.TransferBLL.db.vTransfer.GetVTransferByTransferID(cmdID);
                VIDCollection vid_collection = AVIDINFO2VIDCollection(vtransfer);
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Vehicle_Unassigned, vid_collection);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }

        public override bool S6F11SendTransferCompleted(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                if (!isSend()) return true;
                VTRANSFER vtransfer = scApp.TransferBLL.db.vTransfer.GetVTransferByTransferID(cmdID);
                VIDCollection vid_collection = AVIDINFO2VIDCollection(vtransfer);
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Transfer_Completed, vid_collection);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }
        public override bool S6F11SendTransferCompleted(VTRANSFER vtransfer, CompleteStatus completeStatus, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                if (!isSend()) return true;
                if (vtransfer == null) return true;
                SCUtility.TrimAllParameter(vtransfer);
                string result_code = SECSConst.convert2MCS(completeStatus);
                VIDCollection vid_collection = new VIDCollection();
                vid_collection.VID_59_CommandInfo.CommandInfo.CommandID = vtransfer.ID;
                vid_collection.VID_59_CommandInfo.CommandInfo.Priority = vtransfer.PRIORITY.ToString();
                vid_collection.VID_59_CommandInfo.CommandInfo.Replace = vtransfer.REPLACE.ToString();

                vid_collection.VID_64_ResultCode.ResultCode = result_code;

                vid_collection.VID_77_TransferCompleteInfo.TransferCompleteInfo.TransferInfo.CarrierID = vtransfer.CARRIER_ID;
                vid_collection.VID_77_TransferCompleteInfo.TransferCompleteInfo.TransferInfo.SourcePort = vtransfer.HOSTSOURCE;
                vid_collection.VID_77_TransferCompleteInfo.TransferCompleteInfo.TransferInfo.DestPort = vtransfer.HOSTDESTINATION;
                vid_collection.VID_77_TransferCompleteInfo.TransferCompleteInfo.CarrierLoc = vtransfer.CARRIER_LOCATION;

                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Transfer_Completed, vid_collection);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }


        public override bool S6F11SendTransferAbortCompleted(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                if (!isSend()) return true;
                VTRANSFER vtransfer = scApp.TransferBLL.db.vTransfer.GetVTransferByTransferID(cmdID);
                VIDCollection vid_collection = AVIDINFO2VIDCollection(vtransfer);
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Transfer_Abort_Completed, vid_collection);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }

        public override bool S6F11SendTransferCancelCompleted(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                if (!isSend()) return true;
                VTRANSFER vtransfer = scApp.TransferBLL.db.vTransfer.GetVTransferByTransferID(cmdID);
                VIDCollection vid_collection = AVIDINFO2VIDCollection(vtransfer);
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Transfer_Cancel_Completed, vid_collection);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }

        public override bool S6F11PortEventStateChanged(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                if (!isSend()) return true;
                VTRANSFER vtransfer = scApp.TransferBLL.db.vTransfer.GetVTransferByTransferID(cmdID);
                VIDCollection vid_collection = AVIDINFO2VIDCollection(vtransfer);
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Transfer_Abort_Completed, vid_collection);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }

        public override bool S6F11SendAlarmCleared(string vhID, string transferID, string alarmID, string alarmTest, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                string transfer_id = "";
                string carrier_id = "";
                string carrier_loc = "";
                VTRANSFER vtransfer = scApp.TransferBLL.db.vTransfer.GetVTransferByTransferID(transferID);
                if (vtransfer != null)
                {
                    transfer_id = SCUtility.Trim(vtransfer.ID, true);
                    carrier_id = SCUtility.Trim(vtransfer.CARRIER_ID, true);
                    carrier_loc = SCUtility.Trim(vtransfer.CARRIER_LOCATION, true);
                }
                //if (string.IsNullOrWhiteSpace(carrier_loc))//20210601 AT&S MCS要求要把發生地點填到Carrier Loc的位置。
                //{
                //    carrier_loc = vhID;
                //}
                var vh = scApp.VehicleBLL.cache.getVehicle(vhID);
                VIDCollection vid_collection = new VIDCollection();
                vid_collection.VID_58_CommandID.CommandID = transfer_id;
                vid_collection.VID_71_VehicleInfo.VehicleInfo.VehicleID = vh == null ? vhID : vh.Real_ID; //20210601noon AT&S MCS要求要把發生地點填到Vehicle ID的位置。
                //vid_collection.VID_71_VehicleInfo.VehicleInfo.VehicleID = vh == null ? "" : vh.Real_ID;
                vid_collection.VID_71_VehicleInfo.VehicleInfo.VehicleState = vh == null ? "0" : ((int)vh.State).ToString();
                vid_collection.VID_81_AlarmID.AlarmID = alarmID;
                vid_collection.VID_82_AlarmText.AlarmText = alarmTest;
                vid_collection.VID_54_CarrierID.CarrierID = carrier_id;
                vid_collection.VID_56_CarrierLoc.CarrierLoc = carrier_loc;
                //AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Alarm_Set, vid_collection);
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Alarm_Cleared, vid_collection);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
            }
            return true;
        }

        public override bool S6F11SendAlarmSet(string vhID, string transferID, string alarmID, string alarmTest, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                string transfer_id = "";
                string carrier_id = "";
                string carrier_loc = "";
                VTRANSFER vtransfer = scApp.TransferBLL.db.vTransfer.GetVTransferByTransferID(transferID);
                if (vtransfer != null)
                {
                    transfer_id = SCUtility.Trim(vtransfer.ID, true);
                    carrier_id = SCUtility.Trim(vtransfer.CARRIER_ID, true);
                    carrier_loc = SCUtility.Trim(vtransfer.CARRIER_LOCATION, true);
                }
                //if(string.IsNullOrWhiteSpace(carrier_loc))//20210601morning AT&S MCS要求要把發生地點填到Carrier Loc的位置。
                //{
                //    carrier_loc = vhID;
                //} 
                var vh = scApp.VehicleBLL.cache.getVehicle(vhID);
                VIDCollection vid_collection = new VIDCollection();
                vid_collection.VID_58_CommandID.CommandID = transfer_id;
                vid_collection.VID_71_VehicleInfo.VehicleInfo.VehicleID = vh == null ? vhID : vh.Real_ID; //20210601noon AT&S MCS要求要把發生地點填到Vehicle ID的位置。
                //vid_collection.VID_71_VehicleInfo.VehicleInfo.VehicleID = vh == null ? "" : vh.Real_ID;
                vid_collection.VID_71_VehicleInfo.VehicleInfo.VehicleState = vh == null ? "0" : ((int)vh.State).ToString();
                vid_collection.VID_81_AlarmID.AlarmID = alarmID;
                vid_collection.VID_82_AlarmText.AlarmText = alarmTest;
                vid_collection.VID_54_CarrierID.CarrierID = carrier_id;
                vid_collection.VID_56_CarrierLoc.CarrierLoc = carrier_loc;
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Alarm_Set, vid_collection);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
            }
            return true;
        }



        public override AMCSREPORTQUEUE S6F11BulibMessage(string ceid, object vidCollection)
        {
            try
            {
                VIDCollection Vids = vidCollection as VIDCollection;
                string ceidOfname = string.Empty;
                SECSConst.CEID_Dictionary.TryGetValue(ceid, out ceidOfname);
                string ceid_name = $"CEID:[{ceidOfname}({ceid})]";
                S6F11 s6f11 = new S6F11()
                {
                    SECSAgentName = scApp.EAPSecsAgentName,
                    CEID = ceid,
                    StreamFunctionName = ceid_name
                };
                List<string> RPTIDs = SECSConst.DicCEIDAndRPTID[ceid];
                s6f11.INFO.ITEM = new S6F11.RPTINFO.RPTITEM[RPTIDs.Count];

                for (int i = 0; i < RPTIDs.Count; i++)
                {
                    string rpt_id = RPTIDs[i];
                    s6f11.INFO.ITEM[i] = new S6F11.RPTINFO.RPTITEM();
                    List<ARPTID> AVIDs = SECSConst.DicRPTIDAndVID[rpt_id];
                    List<string> VIDs = AVIDs.OrderBy(avid => avid.ORDER_NUM).Select(avid => avid.VID.Trim()).ToList();
                    s6f11.INFO.ITEM[i].RPTID = rpt_id;
                    s6f11.INFO.ITEM[i].VIDITEM = new SXFY[AVIDs.Count];
                    for (int j = 0; j < AVIDs.Count; j++)
                    {
                        string vid = VIDs[j];
                        SXFY vid_item = null;
                        switch (vid)
                        {
                            case SECSConst.VID_ControlState:
                                vid_item = Vids.VID_06_ControlState;
                                break;
                            case SECSConst.VID_EnhancedCarrierInfo:
                                vid_item = Vids.VID_10_EnhancedCarriersInfo;
                                break;
                            case SECSConst.VID_InstallTime:
                                vid_item = Vids.VID_12_InstallTime;
                                break;
                            case SECSConst.VID_EnhancedTransferCmd:
                                vid_item = Vids.VID_13_EnhancedTransferCmd;
                                break;
                            case SECSConst.VID_CarrierType:
                                vid_item = Vids.VID_16_CarrierType;
                                break;
                            case SECSConst.VID_LotID:
                                vid_item = Vids.VID_17_LotID;
                                break;
                            case SECSConst.VID_CarrierID:
                                vid_item = Vids.VID_54_CarrierID;
                                break;
                            case SECSConst.VID_NewCarrierID:
                                vid_item = Vids.VID_50_NewCarrierID;
                                break;
                            case SECSConst.VID_CarrierLoc:
                                vid_item = Vids.VID_56_CarrierLoc;
                                break;
                            case SECSConst.VID_CommandID:
                                vid_item = Vids.VID_58_CommandID;
                                break;
                            case SECSConst.VID_CommandInfo:
                                vid_item = Vids.VID_59_CommandInfo;
                                break;
                            case SECSConst.VID_Dest:
                                vid_item = Vids.VID_60_Dest;
                                break;
                            case SECSConst.VID_EqpName:
                                vid_item = Vids.VID_61_EqpName;
                                break;
                            case SECSConst.VID_Priority:
                                vid_item = Vids.VID_62_Priority;
                                break;
                            case SECSConst.VID_ResultCode:
                                vid_item = Vids.VID_64_ResultCode;
                                break;
                            case SECSConst.VID_Source:
                                vid_item = Vids.VID_65_Source;
                                break;
                            case SECSConst.VID_HandoffType:
                                vid_item = Vids.VID_66_HandoffType;
                                break;
                            case SECSConst.VID_IDreadStatus:
                                vid_item = Vids.VID_67_IDreadStatus;
                                break;
                            case SECSConst.VID_VehicleID:
                                vid_item = Vids.VID_70_VehicleID;
                                break;
                            case SECSConst.VID_VehicleInfo:
                                vid_item = Vids.VID_71_VehicleInfo;
                                break;
                            case SECSConst.VID_VehicleState:
                                vid_item = Vids.VID_72_VehicleState;
                                break;
                            case SECSConst.VID_TransferCompleteInfo:
                                vid_item = Vids.VID_77_TransferCompleteInfo;
                                break;
                            case SECSConst.VID_CommandType:
                                vid_item = Vids.VID_80_CommandType;
                                break;
                            case SECSConst.VID_AlarmID:
                                vid_item = Vids.VID_81_AlarmID;
                                break;
                            case SECSConst.VID_AlarmText:
                                vid_item = Vids.VID_82_AlarmText;
                                break;
                            case SECSConst.VID_UnitID:
                                vid_item = Vids.VID_83_UnitID;
                                break;
                            case SECSConst.VID_TransferInfo:
                                vid_item = Vids.VID_84_TransferInfo;
                                break;
                            case SECSConst.VID_BatteryValue:
                                vid_item = Vids.VID_101_BatteryValue;
                                break;
                            case SECSConst.VID_VehicleLastPosition:
                                vid_item = Vids.VID_102_VehicleLastPosition;
                                break;
                            case SECSConst.VID_SpecVersion:
                                vid_item = Vids.VID_114_SpecVersion;
                                break;
                            case SECSConst.VID_PortID:
                                vid_item = Vids.VID_115_PortID;
                                break;
                            case SECSConst.VID_VichicleLocation:
                                vid_item = Vids.VID_117_VichicleLocation;
                                break;
                            case SECSConst.VID_CarrierState:
                                vid_item = Vids.VID_203_CarrierState;
                                break;
                            case SECSConst.VID_EqPresenceStatus:
                                vid_item = Vids.VID_353_EqPresenceStatus;
                                break;
                            case SECSConst.VID_PortInfo:
                                vid_item = Vids.VID_354_PortInfo;
                                break;
                            case SECSConst.VID_PortTransferState:
                                vid_item = Vids.VID_355_PortTransferState;
                                break;
                            case SECSConst.VID_UnitAlarmInfo:
                                vid_item = Vids.VID_361_UnitAlarmInfo;
                                break;
                            case SECSConst.VID_MaintState:
                                vid_item = Vids.VID_362_MaintState;
                                break;
                            case SECSConst.VID_VehicleCurrentPosition:
                                vid_item = Vids.VID_363_VehicleCurrentPosition;
                                break;
                            case SECSConst.VID_CarrierZoneName:
                                vid_item = Vids.VID_370_CarrierZoneName;
                                break;
                            case SECSConst.VID_TransferState:
                                vid_item = Vids.VID_722_TransferState;
                                break;
                            case SECSConst.VID_MonitoredVehicles:
                                vid_item = Vids.VID_723_MonitoredVehicles;
                                break;
                            case SECSConst.VID_MonitoredVehicleInfo:
                                vid_item = Vids.VID_724_MonitoredVehicleInfo;
                                break;
                            case SECSConst.VID_VehicleNextPosition:
                                vid_item = Vids.VID_725_VehicleNextPosition;
                                break;
                            case SECSConst.VID_VehiclecCommunication:
                                vid_item = Vids.VID_726_VehiclecCommunication;
                                break;
                            case SECSConst.VID_VehcileControlMode:
                                vid_item = Vids.VID_727_VehcileControlMode;
                                break;

                        }
                        s6f11.INFO.ITEM[i].VIDITEM[j] = vid_item;
                    }
                }

                return BuildMCSReport
                (s6f11,
                  Vids.VID_58_CommandID.CommandID
                , Vids.VH_ID
                , Vids.VID_115_PortID.PortID);
            }
            catch (Exception ex)
            {
                ceid = SCUtility.Trim(ceid, true);
                string error_message = $"Buide ceid:{ceid} error happend.";
                logger.Error(ex, error_message);
                return null;
            }
        }
        private AMCSREPORTQUEUE BuildMCSReport(S6F11 sxfy, string cmd_id, string vh_id, string port_id)
        {
            byte[] byteArray = SCUtility.ToByteArray(sxfy);
            DateTime reportTime = DateTime.Now;
            AMCSREPORTQUEUE queue = new AMCSREPORTQUEUE()
            {
                SERIALIZED_SXFY = byteArray,
                INTER_TIME = reportTime,
                REPORT_TIME = reportTime,
                STREAMFUNCTION_NAME = string.Concat(sxfy.StreamFunction, '-', sxfy.StreamFunctionName),
                STREAMFUNCTION_CEID = sxfy.CEID,
                MCS_CMD_ID = cmd_id,
                VEHICLE_ID = vh_id,
                PORT_ID = port_id
            };
            return queue;
        }

        public override bool S6F11SendMessage(AMCSREPORTQUEUE queue)
        {
            try
            {

                LogHelper.setCallContextKey_ServiceID(CALL_CONTEXT_KEY_WORD_SERVICE_ID_MCS);

                S6F11 s6f11 = (S6F11)SCUtility.ToObject(queue.SERIALIZED_SXFY);

                S6F12 s6f12 = null;
                SXFY abortSecs = null;
                String rtnMsg = string.Empty;

                if (!isSend(s6f11)) return true;


                //SCUtility.RecodeReportInfo(vh_id, mcs_cmd_id, s6f11, s6f11.CEID);
                SCUtility.secsActionRecordMsg(scApp, false, s6f11);
                TrxSECS.ReturnCode rtnCode = ISECSControl.sendRecv<S6F12>(bcfApp, s6f11, out s6f12,
                    out abortSecs, out rtnMsg, null);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: s6f11,
                   VehicleID: queue.VEHICLE_ID,
                   XID: queue.MCS_CMD_ID);
                SCUtility.secsActionRecordMsg(scApp, false, s6f12);
                SCUtility.actionRecordMsg(scApp, s6f11.StreamFunction, line.Real_ID,
                            "sendS6F11_common.", rtnCode.ToString());
                //SCUtility.RecodeReportInfo(vh_id, mcs_cmd_id, s6f12, s6f11.CEID, rtnCode.ToString());
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: s6f12,
                   VehicleID: queue.VEHICLE_ID,
                   XID: queue.MCS_CMD_ID);

                if (rtnCode != TrxSECS.ReturnCode.Normal)
                {
                    logger_MapActionLog.Warn("Send Transfer Initiated[S6F11] Error![rtnCode={0}]", rtnCode);
                    return false;
                }
                line.CommunicationIntervalWithMCS.Restart();

                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
                return false;
            }
        }
        protected Boolean isSend(S6F11 sxfy)
        {
            Boolean result = false;
            try
            {
                if (sxfy is S6F11)
                {
                    //S6F11 s6f11 = (sxfy as S6F11);
                    //if (s6f11.CEID == SECSConst.CEID_Equipment_OFF_LINE ||
                    //    s6f11.CEID == SECSConst.CEID_Control_Status_Local ||
                    //    s6f11.CEID == SECSConst.CEID_Control_Status_Remote)
                    //{
                    //    return true;
                    //}
                    //else
                    //{
                    //    result = result || scApp.EventBLL.isEnableReport(s6f11.CEID);
                    //}
                    result = true;//20210607 因為S2f37不知為何在現場無法正常作動，暫時Bypass
                }
                result = result && (scApp.getEQObjCacheManager().getLine().Host_Control_State == SCAppConstants.LineHostControlState.HostControlState.On_Line_Local ||
                    scApp.getEQObjCacheManager().getLine().Host_Control_State == SCAppConstants.LineHostControlState.HostControlState.On_Line_Remote);
            }
            catch (Exception ex)
            {
                logger.Error("MESDefaultMapAction has Error[Line Name:{0}],[Error method:{1}],[Error Message:{2}",
                    line.LINE_ID, "isSendEAP", ex.ToString());
            }
            return result;
        }
        #endregion Send


        #region VID Info
        //private VIDCollection AVIDINFO2VIDCollection(AVIDINFO vid_info)
        //{
        //    if (vid_info == null)
        //        return null;
        //    //string carrier_loc = string.Empty;
        //    //string port_id = string.Empty;
        //    //scApp.MapBLL.getPortID(vid_info.CARRIER_LOC, out carrier_loc);
        //    //scApp.MapBLL.getPortID(vid_info.PORT_ID, out port_id);

        //    VIDCollection vid_collection = new VIDCollection();
        //    vid_collection.VH_ID = vid_info.EQ_ID;


        //    AVEHICLE vh = scApp.VehicleBLL.getVehicleByID(vid_info.EQ_ID);
        //    //VID_01_AlarmID
        //    vid_collection.VID_01_AlarmID.ALID = vid_info.ALARM_ID;

        //    //VID_54_CarrierID
        //    //vid_collection.VID_54_CarrierID.CARRIER_ID = vid_info.CARRIER_ID;
        //    vid_collection.VID_54_CarrierID.CARRIER_ID = vid_info.MCS_CARRIER_ID;

        //    //VID_55_CarrierInfo
        //    vid_collection.VID_55_CarrierInfo.CARRIER_ID = vid_info.CARRIER_ID;
        //    vid_collection.VID_55_CarrierInfo.VEHICLE_ID = vh.Real_ID;
        //    vid_collection.VID_55_CarrierInfo.CARRIER_LOC = vid_info.CARRIER_LOC;

        //    //VID_58_CommandID
        //    vid_collection.VID_58_CommandID.COMMAND_ID = vid_info.COMMAND_ID;

        //    //VID_59_CommandInfo
        //    vid_collection.VID_59_CommandInfo.COMMAND_ID = vid_info.COMMAND_ID;
        //    vid_collection.VID_59_CommandInfo.PRIORITY = vid_info.PRIORITY.ToString();
        //    vid_collection.VID_59_CommandInfo.REPLACE = vid_info.REPLACE.ToString();

        //    //VID_60_CommandType
        //    vid_collection.VID_60_CommandType.COMMAND_TYPE = vid_info.COMMAND_TYPE;

        //    //VID_61_DestPort
        //    vid_collection.VID_61_DestPort.DEST_PORT = vid_info.DESTPORT;

        //    //VID_65_Priority
        //    vid_collection.VID_65_Priority.PRIORITY = vid_info.PRIORITY.ToString();

        //    //VID_67_ResultCode
        //    vid_collection.VID_67_ResultCode.RESULT_CODE = vid_info.RESULT_CODE.ToString();

        //    //VID_68_SourcePort
        //    vid_collection.VID_68_SourcePort.SOURCE_PORT = vid_info.SOURCEPORT;

        //    //VID_69_TransferCommand
        //    vid_collection.VID_69_TransferCommand.COMMAND_INFO.COMMAND_ID = vid_info.COMMAND_ID;
        //    vid_collection.VID_69_TransferCommand.COMMAND_INFO.PRIORITY = vid_info.PRIORITY.ToString();
        //    vid_collection.VID_69_TransferCommand.COMMAND_INFO.REPLACE = vid_info.REPLACE.ToString();
        //    //vid_collection.VID_69_TransferCommand.TRANSFER_INFOs[0].CARRIER_ID.CARRIER_ID = vid_info.CARRIER_ID;
        //    vid_collection.VID_69_TransferCommand.TRANSFER_INFO.CARRIER_ID.CARRIER_ID = vid_info.MCS_CARRIER_ID;
        //    vid_collection.VID_69_TransferCommand.TRANSFER_INFO.SOURCE_PORT.SOURCE_PORT = vid_info.SOURCEPORT;
        //    vid_collection.VID_69_TransferCommand.TRANSFER_INFO.DEST_PORT.DEST_PORT = vid_info.DESTPORT;

        //    //VID_71_TransferPort
        //    vid_collection.VID_71_TransferPort.TRANSFER_PORT = vid_info.PORT_ID;//不確定Transfer Port要填什麼 , For Kevin Wei to Confirm

        //    //VID_72_TransferPortList
        //    vid_collection.VID_72_TransferPortList.TRANSFER_PORT_LIST[0].TRANSFER_PORT = vid_info.PORT_ID;//不確定Transfer Port要填什麼 , For Kevin Wei to Confirm

        //    //VID_74_VehicleID
        //    vid_collection.VID_74_VehicleID.VEHICLE_ID = vh.Real_ID;

        //    //VID_301_TransferCompleteInfo
        //    //vid_collection.VID_301_TransferCompleteInfo.TRANSFER_COMPLETE_INFOs[0].TRANSFER_INFO_OBJ.CARRIER_ID.CARRIER_ID = vid_info.CARRIER_ID;
        //    vid_collection.VID_301_TransferCompleteInfo.TRANSFER_COMPLETE_INFOs[0].TRANSFER_INFO_OBJ.CARRIER_ID.CARRIER_ID = vid_info.MCS_CARRIER_ID;
        //    vid_collection.VID_301_TransferCompleteInfo.TRANSFER_COMPLETE_INFOs[0].TRANSFER_INFO_OBJ.SOURCE_PORT.SOURCE_PORT = vid_info.SOURCEPORT;
        //    vid_collection.VID_301_TransferCompleteInfo.TRANSFER_COMPLETE_INFOs[0].TRANSFER_INFO_OBJ.DEST_PORT.DEST_PORT = vid_info.DESTPORT;
        //    vid_collection.VID_301_TransferCompleteInfo.TRANSFER_COMPLETE_INFOs[0].CARRIER_LOC_OBJ.CARRIER_LOC = vid_info.CARRIER_LOC;

        //    //VID_304_PortEventState
        //    vid_collection.VID_304_PortEventState.PESTATE.PORT_ID.PORT_ID = vid_info.PORT_ID;
        //    vid_collection.VID_304_PortEventState.PESTATE.PORT_EVT_STATE.PORT_EVT_STATE = string.Empty;//不知道Port Event State要填什麼 , For Kevin Wei to Confirm

        //    //VID_310_NearStockerPort
        //    vid_collection.VID_310_NearStockerPort.NEAR_STOCKER_PORT = string.Empty;//不知道NEAR_STOCKER_PORT要填什麼 , For Kevin Wei to Confirm

        //    //VID_311_NearStockerPort
        //    vid_collection.VID_311_CurrentNode.CURRENT_NODE = string.Empty;//不知道Current Node要填什麼 , For Kevin Wei to Confirm

        //    //VID_901_AlarmText
        //    vid_collection.VID_901_AlarmText.ALARM_TEXT = vid_info.ALARM_TEXT;

        //    //VID_902_ChargerID
        //    vid_collection.VID_902_ChargerID.CHANGER_ID = string.Empty;//不知道Charger ID要填什麼 , For Kevin Wei to Confirm

        //    //VID_903_ErrorCode
        //    vid_collection.VID_903_ErrorCode.ERROR_CODE = string.Empty;//不知道Error Code要填什麼 , For Kevin Wei to Confirm

        //    //VID_904_ErrorCode
        //    vid_collection.VID_904_UnitID.UNIT_ID = vid_info.UNIT_ID;


        //    return vid_collection;
        //}
        #endregion VID Info
        #region VTRANSFER Info
        private VIDCollection AVIDINFO2VIDCollection(VTRANSFER vtransfer)
        {
            if (vtransfer == null)
                return null;
            VIDCollection vid_collection = new VIDCollection();
            vid_collection.VH_ID = vtransfer.VH_ID;
            var line = scApp.getEQObjCacheManager().getLine();
            var vh = scApp.VehicleBLL.cache.getVehicle(vtransfer.VH_ID);
            string vh_real_id = "";
            string vh_state = "";
            //string vh_battery_value = "";
            string vh_communication = "";
            string vh_control_mode = "";
            string vh_last_position = "";
            if (vh != null)
            {
                vh_real_id = vh?.Real_ID;
                vh_state = SECSConst.convert2MCS(vh.State);
                //vh_battery_value = vh.BatteryCapacity.ToString();
                vh_communication = SECSConst.convert2MCS(vh.isTcpIpConnect, vh.IsCommunication(scApp.getBCFApplication()));
                vh_control_mode = SECSConst.convert2MCS(vh.isTcpIpConnect, vh.MODE_STATUS);
                double max_x = scApp.ReserveBLL.GetMaxHltMapAddress_x();
                double t_x = (vh.X_Axis * -1) + max_x;
                //vh_last_position = $"[{vh.X_Axis},{vh.Y_Axis}]";
                vh_last_position = $"[{t_x},{vh.Y_Axis}]";
            }

            string eqp_name = line.LINE_ID;
            string command_id = vtransfer.ID;
            string priority = vtransfer.PRIORITY.ToString();
            string carrier_id = vtransfer.CARRIER_ID;
            string source_port = vtransfer.HOSTSOURCE;
            string dest_port = vtransfer.HOSTDESTINATION;
            string transfer_port_id = getTransferPort(vtransfer.COMMANDSTATE, source_port, dest_port);
            //if (vh.State == AVEHICLE.VehicleState.ACQUIRING)
            //{
            //    transfer_port_id = source_port;
            //}
            //else if (vh.State == AVEHICLE.VehicleState.DEPOSITING)
            //{
            //    transfer_port_id = dest_port;
            //}
            string carrier_loc = vtransfer.CARRIER_LOCATION;
            string bcr_read_result = SECSConst.convert2MCS(vtransfer.CARRIER_READ_STATUS);
            string replace = vtransfer.REPLACE.ToString();
            string install_time = vtransfer.CARRIER_INSER_TIME?.ToString(SCAppConstants.TimestampFormat_16);
            string transfer_state = SECSConst.convert2MCS(vtransfer.TRANSFERSTATE);
            string result_code = SECSConst.convert2MCS(vtransfer.COMPLETE_STATUS);
            string lot_id = vtransfer.LOT_ID;

            vid_collection.VID_10_EnhancedCarriersInfo.EnhancedCarrierinfo.CarrierID = carrier_id;
            vid_collection.VID_10_EnhancedCarriersInfo.EnhancedCarrierinfo.CarrierLoc = carrier_loc;
            vid_collection.VID_10_EnhancedCarriersInfo.EnhancedCarrierinfo.CarrierZoneName = ""; //todo kevin add 
            vid_collection.VID_10_EnhancedCarriersInfo.EnhancedCarrierinfo.InstallTime = install_time;
            vid_collection.VID_10_EnhancedCarriersInfo.EnhancedCarrierinfo.CarrierState = "";//todo kevin check
            //vid_collection.VID_10_EnhancedCarriersInfo.EnhancedCarrierinfo.CarrierType = "0";//todo kevin check
            //vid_collection.VID_10_EnhancedCarriersInfo.EnhancedCarrierinfo.LotID = lot_id;//todo kevin check

            vid_collection.VID_12_InstallTime.InstallTime = install_time;

            vid_collection.VID_13_EnhancedTransferCmd.TransferState = transfer_state;
            vid_collection.VID_13_EnhancedTransferCmd.CommandInfo.CommandID = command_id;
            vid_collection.VID_13_EnhancedTransferCmd.CommandInfo.Priority = priority;
            vid_collection.VID_13_EnhancedTransferCmd.CommandInfo.Replace = replace;
            vid_collection.VID_13_EnhancedTransferCmd.TransferInfo.CarrierID = carrier_id;
            vid_collection.VID_13_EnhancedTransferCmd.TransferInfo.SourcePort = source_port;
            vid_collection.VID_13_EnhancedTransferCmd.TransferInfo.DestPort = dest_port;

            vid_collection.VID_50_NewCarrierID.NewCarrierID = "";

            vid_collection.VID_54_CarrierID.CarrierID = carrier_id;

            vid_collection.VID_56_CarrierLoc.CarrierLoc = carrier_loc;

            vid_collection.VID_58_CommandID.CommandID = command_id;

            vid_collection.VID_59_CommandInfo.CommandInfo.CommandID = command_id;
            vid_collection.VID_59_CommandInfo.CommandInfo.Priority = priority;
            vid_collection.VID_59_CommandInfo.CommandInfo.Replace = replace;

            vid_collection.VID_60_Dest.Dest = dest_port;

            vid_collection.VID_61_EqpName.EqpName = eqp_name;

            vid_collection.VID_62_Priority.Priority = priority;

            vid_collection.VID_64_ResultCode.ResultCode = result_code;

            vid_collection.VID_65_Source.Source = source_port;

            vid_collection.VID_66_HandoffType.HandoffType = "";

            vid_collection.VID_67_IDreadStatus.IDreadStatus = bcr_read_result;

            vid_collection.VID_70_VehicleID.VehicleID = vh_real_id;

            vid_collection.VID_71_VehicleInfo.VehicleInfo.VehicleID = vh_real_id;
            vid_collection.VID_71_VehicleInfo.VehicleInfo.VehicleState = vh_state;

            vid_collection.VID_72_VehicleState.VehicleState = vh_state;

            vid_collection.VID_77_TransferCompleteInfo.TransferCompleteInfo.CarrierLoc = carrier_loc;
            vid_collection.VID_77_TransferCompleteInfo.TransferCompleteInfo.TransferInfo.CarrierID = carrier_id;
            vid_collection.VID_77_TransferCompleteInfo.TransferCompleteInfo.TransferInfo.SourcePort = source_port;
            vid_collection.VID_77_TransferCompleteInfo.TransferCompleteInfo.TransferInfo.DestPort = dest_port;

            //vid_collection.VID_80_CommandType.CommandType = "";

            //vid_collection.VID_81_AlarmID.AlarmID = "";

            vid_collection.VID_82_AlarmText.AlarmText = "";

            vid_collection.VID_83_UnitID.UnitID = "";
            vid_collection.VID_84_TransferInfo.TransferInfo.CarrierID = carrier_id;
            vid_collection.VID_84_TransferInfo.TransferInfo.SourcePort = source_port;
            vid_collection.VID_84_TransferInfo.TransferInfo.DestPort = dest_port;

            //vid_collection.VID_101_BatteryValue.BatteryValue = vh_battery_value;
            vid_collection.VID_101_BatteryValue.BatteryValue = "";

            //vid_collection.VID_102_VehicleLastPosition.VehicleLastPosition = "";

            vid_collection.VID_102_VehicleLastPosition.VehicleLastPosition = vh_last_position;

            vid_collection.VID_114_SpecVersion.SpecVersion = "E82";

            vid_collection.VID_115_PortID.PortID = transfer_port_id;

            vid_collection.VID_117_VichicleLocation.VichicleLocation = "";

            vid_collection.VID_203_CarrierState.CarrierState = "";

            vid_collection.VID_353_EqPresenceStatus.EqPresenceStatus = "";

            vid_collection.VID_354_PortInfo.PortInfo.PortID = "";
            vid_collection.VID_354_PortInfo.PortInfo.PortTransferState = "";

            vid_collection.VID_355_PortTransferState.PortTransferState = "";

            vid_collection.VID_361_UnitAlarmInfo.UnitAlarmInfo.UnitID = "";
            vid_collection.VID_361_UnitAlarmInfo.UnitAlarmInfo.AlarmID = "";
            vid_collection.VID_361_UnitAlarmInfo.UnitAlarmInfo.AlarmID = "";
            vid_collection.VID_361_UnitAlarmInfo.UnitAlarmInfo.MaintState = "";

            vid_collection.VID_362_MaintState.MaintState = "";

            vid_collection.VID_370_CarrierZoneName.CarrierZoneName = "";

            vid_collection.VID_722_TransferState.TransferState = transfer_state;

            //vid_collection.VID_723_MonitoredVehicles

            vid_collection.VID_724_MonitoredVehicleInfo.MonitoredVehicleInfo.VehicleID = vh_real_id;
            vid_collection.VID_724_MonitoredVehicleInfo.MonitoredVehicleInfo.VehicleLastPosition = "";
            vid_collection.VID_724_MonitoredVehicleInfo.MonitoredVehicleInfo.VehicleCurrentPosition = "";
            vid_collection.VID_724_MonitoredVehicleInfo.MonitoredVehicleInfo.VehicleNextPosition = "";
            vid_collection.VID_724_MonitoredVehicleInfo.MonitoredVehicleInfo.VehicleStatus = vh_state;
            vid_collection.VID_724_MonitoredVehicleInfo.MonitoredVehicleInfo.VehiclecCommunication = vh_communication;
            vid_collection.VID_724_MonitoredVehicleInfo.MonitoredVehicleInfo.VehcileControlMode = vh_control_mode;

            vid_collection.VID_725_VehicleNextPosition.VehicleNextPosition = "";

            vid_collection.VID_727_VehcileControlMode.VehcileControlMode = vh_control_mode;

            return vid_collection;
        }
        private string getTransferPort(int commandState, string sourcePort, string descPort)
        {
            if (commandState > ATRANSFER.COMMAND_STATUS_BIT_INDEX_UNLOAD_ARRIVE)
            {
                return descPort;
            }
            else
            {
                return sourcePort;
            }
        }
        private string getTransferPort(PortStationBLL portStationBLL, string vhCurrentAdrID, string sourcePort, string descPort)
        {
            //1.用目前車子所在位置
            var port_stations = portStationBLL.OperateCatch.getPortStationByAdrID(vhCurrentAdrID);
            bool is_in_port = port_stations.Where(station => SCUtility.isMatche(station.PORT_ID, sourcePort)).Count() > 0;
            if (is_in_port)
            {
                return sourcePort;
            }
            else
            {
                return descPort;
            }
        }
        #endregion VTRANSFER Info

        public virtual void doInit()
        {
            string eapSecsAgentName = scApp.EAPSecsAgentName;
            reportBLL = scApp.ReportBLL;

            ISECSControl.addSECSReceivedHandler(bcfApp, eapSecsAgentName, "S1F1", S1F1ReceiveAreYouThere);
            ISECSControl.addSECSReceivedHandler(bcfApp, eapSecsAgentName, "S1F3", S1F3ReceiveSelectedEquipmentStatusRequest);
            ISECSControl.addSECSReceivedHandler(bcfApp, eapSecsAgentName, "S1F13", S1F13ReceiveEstablishCommunicationRequest);
            ISECSControl.addSECSReceivedHandler(bcfApp, eapSecsAgentName, "S1F15", S1F15OffLineRequest);
            ISECSControl.addSECSReceivedHandler(bcfApp, eapSecsAgentName, "S1F17", S1F17ReceiveRequestOnLine);

            ISECSControl.addSECSReceivedHandler(bcfApp, eapSecsAgentName, "S2F13", S2F13ReceiveNewEquiptment);
            ISECSControl.addSECSReceivedHandler(bcfApp, eapSecsAgentName, "S2F15", S2F15ReceiveSetNewEquiptment);
            ISECSControl.addSECSReceivedHandler(bcfApp, eapSecsAgentName, "S2F31", S2F31ReceiveDateTimeSetReq);
            ISECSControl.addSECSReceivedHandler(bcfApp, eapSecsAgentName, "S2F33", S2F33ReceiveDefineReport);
            ISECSControl.addSECSReceivedHandler(bcfApp, eapSecsAgentName, "S2F35", S2F35ReceiveLinkEventReport);
            ISECSControl.addSECSReceivedHandler(bcfApp, eapSecsAgentName, "S2F37", S2F37ReceiveEnableDisableEventReport);
            ISECSControl.addSECSReceivedHandler(bcfApp, eapSecsAgentName, "S2F41", S2F41ReceiveHostCommand);
            ISECSControl.addSECSReceivedHandler(bcfApp, eapSecsAgentName, "S2F49", S2F49ReceiveEnhancedRemoteCommandExtension);

            ISECSControl.addSECSReceivedHandler(bcfApp, eapSecsAgentName, "S5F3", S5F3ReceiveEnableDisableAlarm);

            ISECSControl.addSECSConnectedHandler(bcfApp, eapSecsAgentName, secsConnected);
            ISECSControl.addSECSDisconnectedHandler(bcfApp, eapSecsAgentName, secsDisconnected);
        }
        public override bool S6F11SendTransferAbortFailed(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                if (!isSend()) return true;
                VTRANSFER vtransfer = scApp.TransferBLL.db.vTransfer.GetVTransferByTransferID(cmdID);
                VIDCollection vid_collection = AVIDINFO2VIDCollection(vtransfer);
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Transfer_Abort_Failed, vid_collection);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }

        public override bool S6F11SendTransferAbortInitial(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                if (!isSend()) return true;
                VTRANSFER vtransfer = scApp.TransferBLL.db.vTransfer.GetVTransferByTransferID(cmdID);
                VIDCollection vid_collection = AVIDINFO2VIDCollection(vtransfer);
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Transfer_Abort_Initiated, vid_collection);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }



        public override bool S6F11SendTransferCancelFailed(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                if (!isSend()) return true;
                VTRANSFER vtransfer = scApp.TransferBLL.db.vTransfer.GetVTransferByTransferID(cmdID);
                VIDCollection vid_collection = AVIDINFO2VIDCollection(vtransfer);
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Transfer_Cancel_Failed, vid_collection);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }

        public override bool S6F11SendTransferCancelInitial(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                if (!isSend()) return true;
                VTRANSFER vtransfer = scApp.TransferBLL.db.vTransfer.GetVTransferByTransferID(cmdID);
                VIDCollection vid_collection = AVIDINFO2VIDCollection(vtransfer);
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Transfer_Cancel_Initiated, vid_collection);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }

        public override bool S6F11SendTransferPaused(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                if (!isSend()) return true;
                VTRANSFER vtransfer = scApp.TransferBLL.db.vTransfer.GetVTransferByTransferID(cmdID);
                VIDCollection vid_collection = AVIDINFO2VIDCollection(vtransfer);
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Transfer_Pause, vid_collection);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }

        public override bool S6F11SendTransferResumed(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                if (!isSend()) return true;
                VTRANSFER vtransfer = scApp.TransferBLL.db.vTransfer.GetVTransferByTransferID(cmdID);
                VIDCollection vid_collection = AVIDINFO2VIDCollection(vtransfer);
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Transfer_Resumed, vid_collection);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }


        public override bool S6F11SendVehicleInstalled(string vhID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            if (!isSend()) return true;
            VIDCollection vid_collection = new VIDCollection();

            vid_collection.VID_70_VehicleID.VehicleID = vhID;

            AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Vehicle_Installed, vid_collection);
            if (reportQueues == null)
            {
                S6F11SendMessage(mcs_queue);
            }
            else
            {
                reportQueues.Add(mcs_queue);
            }
            return true;
        }

        public override bool S6F11SendVehicleRemoved(string vhID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            if (!isSend()) return true;
            VIDCollection vid_collection = new VIDCollection();

            vid_collection.VID_70_VehicleID.VehicleID = vhID;

            AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Vehicle_Removed, vid_collection);
            if (reportQueues == null)
            {
                S6F11SendMessage(mcs_queue);
            }
            else
            {
                reportQueues.Add(mcs_queue);
            }
            return true;
        }

        public override bool S6F11SendRunTimeStatus(string vhID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            if (!isSend()) return true;
            var vh = scApp.VehicleBLL.cache.getVehicle(vhID);
            string vh_real_id = "";
            string vh_state = "";
            string vh_battery_value = "";
            string vh_communication = "";
            string vh_control_mode = "";
            string vh_last_position = "";
            if (vh != null)
            {
                vh_real_id = vh?.Real_ID;
                vh_state = SECSConst.convert2MCS(vh.State);
                //vh_battery_value = vh.BatteryCapacity.ToString();
                vh_communication = SECSConst.convert2MCS(vh.isTcpIpConnect, vh.IsCommunication(scApp.getBCFApplication()));
                vh_control_mode = SECSConst.convert2MCS(vh.isTcpIpConnect, vh.MODE_STATUS);
                double max_x = scApp.ReserveBLL.GetMaxHltMapAddress_x();
                double t_x = (vh.X_Axis * -1) + max_x;
                //vh_last_position = $"[{vh.X_Axis},{vh.Y_Axis}]";
                vh_last_position = $"[{t_x},{vh.Y_Axis}]";
            }
            VIDCollection vid_collection = new VIDCollection();

            vid_collection.VID_70_VehicleID.VehicleID = vh_real_id;
            vid_collection.VID_72_VehicleState.VehicleState = vh_state;
            vid_collection.VID_102_VehicleLastPosition.VehicleLastPosition = vh_last_position;
            //vid_collection.VID_101_BatteryValue.BatteryValue = vh_battery_value;
            vid_collection.VID_101_BatteryValue.BatteryValue = "";

            AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Vehicle_RuntimeStatus, vid_collection);
            if (reportQueues == null)
            {
                S6F11SendMessage(mcs_queue);
            }
            else
            {
                reportQueues.Add(mcs_queue);
            }
            return true;
        }


        public override bool S6F11SendCarrierWaitIn(ACARRIER cst, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                //if (!isSend()) return true;
                VIDCollection Vids = new VIDCollection();

                Vids.VID_54_CarrierID.CarrierID = cst.ID;
                Vids.VID_56_CarrierLoc.CarrierLoc = cst.LOCATION;
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Carrier_Wait_In, Vids);
                if (reportQueues == null)
                {
                    if (S6F11SendMessage(mcs_queue))
                    {
                        scApp.CarrierBLL.db.updateState(cst.LOCATION, E_CARRIER_STATE.WaitIn);
                        //scApp.TransferService.SetWaitInOutLog(cst, ProtocolFormat.OHTMessage.E_CARRIER_STATE.WaitIn);
                    }
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }


        public override bool S6F11SendCarrierWaitOut(ACARRIER cst, string portType, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                //if (!isSend()) return true;
                VIDCollection Vids = new VIDCollection();
                Vids.VID_54_CarrierID.CarrierID = cst.ID;
                Vids.VID_56_CarrierLoc.CarrierLoc = cst.LOCATION;

                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Carrier_Wait_Out, Vids);
                if (reportQueues == null)
                {
                    if (S6F11SendMessage(mcs_queue))
                    {
                        //scApp.CarrierBLL.db.updateState(cst.LOCATION, E_CARRIER_STATE.WaitOut);
                        //scApp.TransferService.SetWaitInOutLog(cst, E_CSTState.WaitOut);
                        APORTSTATION portstation = scApp.PortStationBLL.OperateCatch.getPortStation(cst.LOCATION);//WaitOut Location等於PortID，代表已到LP側
                        if (portstation != null)
                        {
                            scApp.CarrierBLL.db.updateWaitOutLPTime(cst.ID);
                        }
                        else if (cst.LOCATION.Trim().EndsWith("OP"))//WaitOut Location結尾是OP，代表是OP側
                        {
                            scApp.CarrierBLL.db.updateWaitOutOPTime(cst.ID);
                        }
                        else
                        {

                        }

                    }
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }

        public override bool S6F11SendCarrierIDRead(ACARRIER cst, string IDreadStatus, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                VIDCollection Vids = new VIDCollection();
                //CassetteData cst = scApp.CassetteDataBLL.loadCassetteDataByBoxID(BOXID);
                Vids.VID_54_CarrierID.CarrierID = cst.ID;
                Vids.VID_56_CarrierLoc.CarrierLoc = cst.LOCATION;
                Vids.VID_67_IDreadStatus.IDreadStatus = IDreadStatus;

                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Carrier_ID_Read, Vids);
                scApp.ReportBLL.insertMCSReport(mcs_queue);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
            }
            return true;
        }

        public override bool S6F11SendCarrierInstallCompleted(ACARRIER cst, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                VIDCollection Vids = new VIDCollection();
                //CassetteData cst = scApp.CassetteDataBLL.loadCassetteDataByBoxID(BOXID);
                Vids.VID_54_CarrierID.CarrierID = cst.ID;
                Vids.VID_56_CarrierLoc.CarrierLoc = cst.LOCATION;
                Vids.VID_16_CarrierType.CarrierType = cst.CSTType == null ? "0" : cst.CSTType;
                Vids.VID_17_LotID.LotID = cst.LOT_ID;

                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Carrier_InstallComplete, Vids);
                scApp.ReportBLL.insertMCSReport(mcs_queue);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
            }
            return true;
        }

        public override bool S6F11SendUnitAlarmSet(string unitID, string alarmID, string alarmTest, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                VIDCollection Vids = new VIDCollection();
                Vids.VID_83_UnitID.UnitID = unitID;
                Vids.VID_81_AlarmID.AlarmID = alarmID;
                Vids.VID_82_AlarmText.AlarmText = alarmTest;
                Vids.VID_72_VehicleState.VehicleState = "0";//一律報0

                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Unit_Alarm_Set, Vids);
                scApp.ReportBLL.insertMCSReport(mcs_queue);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
            }
            return true;

        }
        public override bool S6F11SendUnitAlarmCleared(string unitID, string alarmID, string alarmTest, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                VIDCollection Vids = new VIDCollection();
                Vids.VID_83_UnitID.UnitID = unitID;
                Vids.VID_81_AlarmID.AlarmID = alarmID;
                Vids.VID_82_AlarmText.AlarmText = alarmTest;
                Vids.VID_72_VehicleState.VehicleState = "0";//一律報0

                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Unit_Alarm_Cleared, Vids);
                scApp.ReportBLL.insertMCSReport(mcs_queue);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
            }
            return true;
        }
        public override bool S6F11SendOperatorInitialAction(string command_id, string command_type, string carrier_id, string source, string destination, string priority, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                VIDCollection Vids = new VIDCollection();
                Vids.VID_58_CommandID.CommandID = command_id;
                Vids.VID_80_CommandType.CommandType = command_type;
                Vids.VID_54_CarrierID.CarrierID = carrier_id;
                Vids.VID_65_Source.Source = source;
                Vids.VID_60_Dest.Dest = destination;
                Vids.VID_62_Priority.Priority = priority;

                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_OperatorInitiateAction, Vids);
                scApp.ReportBLL.insertMCSReport(mcs_queue);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
            }
            return true;
        }



        public override bool S6F11SendCarrierRemovedCompleted(string cst_id, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                VIDCollection Vids = new VIDCollection();
                //var cassette = scApp.CassetteDataBLL.loadCassetteDataByCstBoxID(cst_id, box_id);
                //string zonename = scApp.CassetteDataBLL.GetZoneName(cassette.Carrier_LOC);
                ACARRIER cst = scApp.CarrierBLL.db.getCarrier(cst_id);

                Vids.VID_54_CarrierID.CarrierID = cst.ID;
                Vids.VID_56_CarrierLoc.CarrierLoc = cst.LOCATION;

                scApp.CarrierBLL.db.delete(cst_id);

                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Carrier_Removed_Complete, Vids);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }

        public override bool S6F11SendCarrierRemovedCompleted(string cst_id, string location, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                VIDCollection Vids = new VIDCollection();
                //ACARRIER cst = scApp.CarrierBLL.db.getCarrier(cst_id);

                Vids.VID_54_CarrierID.CarrierID = cst_id;
                Vids.VID_56_CarrierLoc.CarrierLoc = location;

                scApp.CarrierBLL.db.delete(cst_id);

                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Carrier_Removed_Complete, Vids);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }
        public override bool S6F11SendCarrierRemovedFromPort(ACARRIER cst, string Handoff_Type, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                //if (!isSend()) return true;
                VIDCollection Vids = new VIDCollection();

                Vids.VID_54_CarrierID.CarrierID = cst.ID;
                Vids.VID_66_HandoffType.HandoffType = Handoff_Type;

                scApp.CarrierBLL.db.delete(cst.ID);
                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Carrier_Removed, Vids);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
                return false;
            }
        }


        public override bool S6F11SendLoadReq(string port_id, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                VIDCollection Vids = new VIDCollection();
                Vids.VID_115_PortID.PortID = port_id;

                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Port_LoadReq, Vids);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
            }
            return true;
        }

        public override bool S6F11SendUnLoadReq(string port_id, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                VIDCollection Vids = new VIDCollection();
                Vids.VID_115_PortID.PortID = port_id;

                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Port_UnloadReq, Vids);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
            }
            return true;
        }

        public override bool S6F11SendNoReq(string port_id, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            try
            {
                VIDCollection Vids = new VIDCollection();
                Vids.VID_115_PortID.PortID = port_id;

                AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Port_NoReq, Vids);
                if (reportQueues == null)
                {
                    S6F11SendMessage(mcs_queue);
                }
                else
                {
                    reportQueues.Add(mcs_queue);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MCSDefaultMapAction), Device: DEVICE_NAME_MCS,
                   Data: ex);
            }
            return true;
        }

        public override bool S6F11SendOperatorInitiatedAction(string vhID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            return false;
        }
        public override bool S6F11SendVehicleChargeRequest(string vhId, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            return false;
        }
        public override bool S6F11SendVehicleChargeStarted(string vhId, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            return false;
        }
        public override bool S6F11SendVehicleChargeCompleted(string vhId, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            return false;
        }
        public override bool S6F11SendPortInService(string portID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            if (!isSend()) return true;
            VIDCollection vid_collection = new VIDCollection();

            //vid_collection.VID_61_EqpName.EqpName = line.LINE_ID;
            vid_collection.VID_115_PortID.PortID = portID;

            AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Port_In_Service, vid_collection);
            if (reportQueues == null)
            {
                S6F11SendMessage(mcs_queue);
            }
            else
            {
                reportQueues.Add(mcs_queue);
            }
            return true;
        }

        public override bool S6F11SendPortOutService(string portID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            if (!isSend()) return true;
            VIDCollection vid_collection = new VIDCollection();

            //vid_collection.VID_61_EqpName.EqpName = line.LINE_ID;
            vid_collection.VID_115_PortID.PortID = portID;

            AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Port_Out_Of_Service, vid_collection);
            if (reportQueues == null)
            {
                S6F11SendMessage(mcs_queue);
            }
            else
            {
                reportQueues.Add(mcs_queue);
            }
            return true;
        }


        public override bool S6F11SendPortInMode(string portID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            if (!isSend()) return true;
            VIDCollection vid_collection = new VIDCollection();

            //vid_collection.VID_61_EqpName.EqpName = line.LINE_ID;
            vid_collection.VID_115_PortID.PortID = portID;

            AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Port_Type_Input, vid_collection);
            if (reportQueues == null)
            {
                S6F11SendMessage(mcs_queue);
            }
            else
            {
                reportQueues.Add(mcs_queue);
            }
            return true;
        }

        public override bool S6F11SendPortOutMode(string portID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            if (!isSend()) return true;
            VIDCollection vid_collection = new VIDCollection();

            //vid_collection.VID_61_EqpName.EqpName = line.LINE_ID;
            vid_collection.VID_115_PortID.PortID = portID;

            AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Port_Type_Output, vid_collection);
            if (reportQueues == null)
            {
                S6F11SendMessage(mcs_queue);
            }
            else
            {
                reportQueues.Add(mcs_queue);
            }
            return true;
        }

        public override bool S6F11SendPortModeChange(string portID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            if (!isSend()) return true;
            VIDCollection vid_collection = new VIDCollection();

            //vid_collection.VID_61_EqpName.EqpName = line.LINE_ID;
            vid_collection.VID_115_PortID.PortID = portID;

            AMCSREPORTQUEUE mcs_queue = S6F11BulibMessage(SECSConst.CEID_Port_Type_Changing, vid_collection);
            if (reportQueues == null)
            {
                S6F11SendMessage(mcs_queue);
            }
            else
            {
                reportQueues.Add(mcs_queue);
            }
            return true;
        }

        public override bool S6F11SendTransferUpdateCompleted(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            return false;
        }

        public override bool S6F11SendTransferUpdateFailed(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            return false;
        }

        //protected override void S1F17ReceiveRequestOnLine(object sender, SECSEventArgs e)
        //{
        //    try
        //    {
        //        string msg = string.Empty;
        //        SECS.S1F17 s1f17 = ((SECS.S1F17)e.secsHandler.Parse<SECS.S1F17>(e));
        //        SCUtility.secsActionRecordMsg(scApp, true, s1f17);

        //        if (!isProcess(s1f17)) { return; }

        //        SECS.S1F18 s1f18 = new SECS.S1F18();
        //        s1f18.SystemByte = s1f17.SystemByte;
        //        s1f18.SECSAgentName = scApp.EAPSecsAgentName;

        //        bool is_online_ready = false;
        //        //檢查狀態是否允許連線
        //        MaintainLift maintainLift = scApp.EqptBLL.OperateCatch.GetMaintainLift();
        //        if (DebugParameter.RejectEAPOnline)
        //        {
        //            s1f18.ONLACK = SECSConst.ONLACK_Not_Accepted;
        //        }
        //        else if (maintainLift.OHTCCarOutInterlock || maintainLift.OHTCCarInInterlock)
        //        {
        //            s1f18.ONLACK = SECSConst.ONLACK_Not_Accepted;
        //            msg = "Car in/out is processing..."; //A0.05
        //        }
        //        else if (line.Host_Control_State == SCAppConstants.LineHostControlState.HostControlState.On_Line_Remote)
        //        {
        //            //s1f18.ONLACK = SECSConst.ONLACK_Equipment_Already_On_Line;
        //            s1f18.ONLACK = SECSConst.ONLACK_Accepted;
        //            is_online_ready = true;
        //            msg = "OHS is online remote ready!!"; //A0.05
        //        }
        //        else
        //        {
        //            s1f18.ONLACK = SECSConst.ONLACK_Accepted;
        //        }

        //        TrxSECS.ReturnCode rtnCode = ISECSControl.replySECS(bcfApp, s1f18);
        //        SCUtility.secsActionRecordMsg(scApp, false, s1f18);
        //        if (rtnCode != TrxSECS.ReturnCode.Normal)
        //        {
        //            logger.Warn("Reply EQPT S1F18 Error:{0}", rtnCode);
        //        }
        //        if (!is_online_ready && SCUtility.isMatche(s1f18.ONLACK, SECSConst.ONLACK_Accepted))
        //        {
        //            scApp.LineService.OnlineWithHostByHost();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("MESDefaultMapAction has Error[Line Name:{0}],[Error method:{1}],[Error Message:{2}", line.LINE_ID, "S1F17_Receive_OnlineRequest", ex.ToString());
        //    }
        //}
    }
}
