//*********************************************************************************
//      EQType2SecsMapAction.cs
//*********************************************************************************
// File Name: EQType2SecsMapAction.cs
// Description: Type2 EQ Map Action
//
//(c) Copyright 2021, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag            Description
// ------------- -------------  -------------  ------------   -----------------------------
// 2021/05/20    Kevin Wei      N/A            A202105120-01  加入filter條件，在車子已經有載貨時，
//                                                            選擇車子的條件要把它濾掉
//**********************************************************************************

using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.Data.DAO;
using com.mirle.ibg3k0.sc.Data.SECS;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using com.mirle.iibg3k0.ttc.Common;
using NLog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class VehicleBLL
    {
        public DB db { get; protected set; }
        public Cache cache { get; protected set; }
        public Redis redis { get; protected set; }
        public Web web { get; protected set; }


        protected VehicleDao vehicleDAO = null;
        protected SCApplication scApp = null;
        protected static Logger logger = LogManager.GetCurrentClassLogger();


        public VehicleBLL()
        {

        }
        public virtual void start(SCApplication app)
        {
            scApp = app;
            vehicleDAO = scApp.VehicleDao;
            cache = new Cache(scApp.getEQObjCacheManager());
            db = new DB(scApp);
            redis = new Redis(scApp.getRedisCacheManager());
            web = new Web(scApp.webClientManager);
        }
        public void startMapAction()
        {

        }

        public void updateVehicleExcuteCMD(string vh_id, string cmd_id, string mcs_cmd_id)
        {
            db.updateVehicleExcuteCMD(vh_id, cmd_id, mcs_cmd_id);
            cache.updateVehicleExcuteCMD(vh_id, cmd_id, mcs_cmd_id);
        }
        public void updataVehicleLastFullyChargerTime(string vh_id)
        {
            db.updataVehicleLastFullyChargerTime(vh_id);
            cache.updataVehicleLastFullyChargerTime(vh_id);

        }
        public void updataVehicleInstall(string vh_id)
        {
            db.updataVehicleInstall(vh_id);
            cache.updataVehicleInstall(vh_id);
        }
        public void updataVehicleRemove(string vh_id)
        {
            db.updataVehicleRemove(vh_id);
            cache.updataVehicleRemove(vh_id);
        }
        public void updataVehicleType(string vh_id, E_VH_TYPE vhType)
        {
            db.updataVehicleType(vh_id, vhType);
            cache.updataVehicleType(vh_id, vhType);
        }
        public VHModeStatus DecideVhModeStatus(string vhID, VHModeStatus vh_current_mode_status)
        {
            AVEHICLE vh = cache.getVehicle(vhID);
            VHModeStatus modeStat = default(VHModeStatus);
            if (vh_current_mode_status == VHModeStatus.AutoRemote)
            {
                if (vh.MODE_STATUS == VHModeStatus.AutoLocal|| vh.MODE_STATUS == VHModeStatus.AutoMtl)
                {
                    modeStat = vh.MODE_STATUS;
                }else if(vh.MODE_STATUS == VHModeStatus.Manual)
                {
                    MaintainLift MTL = scApp.EqptBLL.OperateCatch.GetMaintainLift();
                    if (SCUtility.isMatche(vh.CUR_ADR_ID, MTL.MTL_ADDRESS) && MTL.CurrentCarID == vh.Num.ToString())
                    {
                        //切到
                        modeStat = VHModeStatus.AutoMtl;
                    }
                    else
                    {
                        modeStat = vh_current_mode_status;
                    }
                }
                else
                {
                    modeStat = vh_current_mode_status;
                }
            }
            else
            {
                modeStat = vh_current_mode_status;
            }
            return modeStat;
        }




        public class BlockedByTheErrorVehicleException : Exception
        {
            public BlockedByTheErrorVehicleException(string msg) : base(msg)
            {

            }
        }




        #region DoSomeThing


        public void doLoadArrivals(string eq_id)
        {
            AVEHICLE vh = cache.getVehicle(eq_id);
            vh.VehicleArrive();
        }
        public void doLoading(string vhID)
        {
            AVEHICLE vh = cache.getVehicle(vhID);
            vh.VehicleAcquireStart();
        }
        public void doLoadComplete(string vhID)
        {
            AVEHICLE vh = cache.getVehicle(vhID);
            //沒有Vh夾取完成後，開始行走的事件，因此在夾取完成後，直接將狀態改成Enroute(Depart)
            vh.VehilceAcquireComplete();
            vh.VehicleDepart();
        }
        public void doUnloadArrivals(string eq_id, string cmdID)
        {
            AVEHICLE vh = cache.getVehicle(eq_id);
            vh.VehicleArrive();
        }
        public void doUnloading(string vhID)
        {
            AVEHICLE vh = cache.getVehicle(vhID);
            vh.VehicleDepositStart();
        }
        public void doUnloadComplete(string vhID)
        {
            AVEHICLE vh = cache.getVehicle(vhID);
            vh.VehicleDepositComplete();
        }
        public bool doInitialVhCommandInfo(string vh_id)
        {
            AVEHICLE vh = cache.getVehicle(vh_id); 
            vh.StartAdr = string.Empty;
            vh.FromAdr = string.Empty;
            vh.ToAdr = string.Empty;
            //vh.ACT_STATUS = VHActionStatus.NoCommand;// marked by Mark 20210701 避免車輛在收到32前就被下命令，不主動清狀態。
            vh.vh_CMD_Status = E_CMD_STATUS.NormalEnd;
            vh.VehicleUnassign();
            vh.Stop();
            return true;
        }




        public void doAdrArrivals(string eq_id, string current_adr_id, string current_sec_id)
        {
            NetworkQualityTest(eq_id, current_adr_id, current_sec_id, 0);
        }
        private void NetworkQualityTest(string eq_id, string current_adr_id, string current_sec_id, int acc_dist)
        {
            if (scApp.getBCFApplication().NetworkQualityTest)
            {
                //Task.Run(() => scApp.NetWorkQualityBLL.VhNetworkQualityTest(eq_id, current_adr_id, current_sec_id, acc_dist));
            }
        }

        #endregion


        TimeSpan POSITION_TIMEOUT = new TimeSpan(0, 5, 0);
        public void clearAndPublishPositionReportInfo2Redis(string vhID)
        {
            setAndPublishPositionReportInfo2Redis(vhID, string.Empty, string.Empty, 0);
        }
        public void setAndPublishPositionReportInfo2Redis(string vh_id, string sec_id, string adr_id, uint distance)
        {
            AVEHICLE vh = cache.getVehicle(vh_id);
            ID_134_TRANS_EVENT_REP id_134_trans_event_rep = new ID_134_TRANS_EVENT_REP()
            {
                //CSTID = vh.CST_ID,
                CurrentAdrID = adr_id,
                CurrentSecID = sec_id,
                EventType = EventType.AdrPass,
                SecDistance = distance
            };
            setAndPublishPositionReportInfo2Redis(vh_id, id_134_trans_event_rep);
        }
        public void setAndPublishPositionReportInfo2Redis(string vh_id, ID_143_STATUS_RESPONSE report_obj)
        {
            AVEHICLE vh = cache.getVehicle(vh_id);

            ID_134_TRANS_EVENT_REP id_134_trans_event_rep = new ID_134_TRANS_EVENT_REP()
            {
                //CSTID = vh.CST_ID == null ? "" : vh.CST_ID,
                CurrentAdrID = report_obj.CurrentAdrID,
                CurrentSecID = report_obj.CurrentSecID,
                SecDistance = report_obj.SecDistance,
                XAxis = report_obj.XAxis,
                YAxis = report_obj.YAxis,
                //DirectionAngle = report_obj.DirectionAngle,
                Angle = report_obj.Angle
            };
            setAndPublishPositionReportInfo2Redis(vh_id, id_134_trans_event_rep);
        }
        public void setAndPublishPositionReportInfo2Redis(string vh_id, ID_132_TRANS_COMPLETE_REPORT report_obj)
        {
            AVEHICLE vh = cache.getVehicle(vh_id);
            string current_adr = report_obj.CurrentAdrID;
            var adrObject = scApp.ReserveBLL.GetHltMapAddress(current_adr);
            ID_134_TRANS_EVENT_REP id_134_trans_event_rep = new ID_134_TRANS_EVENT_REP()
            {
                //CSTID = report_obj.CSTID,
                CurrentAdrID = report_obj.CurrentAdrID,
                CurrentSecID = report_obj.CurrentSecID,
                //SecDistance = report_obj.SecDistance == 0 ? (uint)vh.ACC_SEC_DIST : report_obj.SecDistance,
                SecDistance = 0, //20210615 看log感覺OHT上報的132 section distance怪怪的，目前都先給0
                //DrivingDirection = vh.CurrentDriveDirction,
                XAxis = adrObject.x,
                YAxis = adrObject.y,

                //DirectionAngle = report_obj.DirectionAngle,
                Angle = vh.VehicleAngle
            };
            setAndPublishPositionReportInfo2Redis(vh_id, id_134_trans_event_rep);
        }
        public void setAndPublishPositionReportInfo2Redis(string vh_id, string address)
        {
            AVEHICLE vh = cache.getVehicle(vh_id);
            string current_adr = address;
            ASECTION section = scApp.SectionBLL.cache.GetSectionsByFromAddress(address).FirstOrDefault();
            var adrObject = scApp.ReserveBLL.GetHltMapAddress(current_adr);
            ID_134_TRANS_EVENT_REP id_134_trans_event_rep = new ID_134_TRANS_EVENT_REP()
            {
                //CSTID = report_obj.CSTID,
                CurrentAdrID = address,
                CurrentSecID = section.SEC_ID.Trim(),
                //SecDistance = report_obj.SecDistance == 0 ? (uint)vh.ACC_SEC_DIST : report_obj.SecDistance,
                SecDistance = 0, //20210615 看log感覺OHT上報的132 section distance怪怪的，目前都先給0
                //DrivingDirection = vh.CurrentDriveDirction,
                XAxis = adrObject.x,
                YAxis = adrObject.y,

                //DirectionAngle = report_obj.DirectionAngle,
                Angle = vh.VehicleAngle
            };
            setAndPublishPositionReportInfo2Redis(vh_id, id_134_trans_event_rep);
        }
        public void setAndPublishPositionReportInfo2Redis(string vh_id, ID_134_TRANS_EVENT_REP report_obj)
        {
            redis.setPositionReportInfo2Redis(vh_id, report_obj);
            doUpdateVheiclePositionAndCmdSchedule(vh_id, report_obj);
        }

        public void updateVehicleActionStatus(AVEHICLE vh, EventType vhPassEvent)
        {
            //vh.VhRecentTranEvent = vhPassEvent;
            //vh.NotifyVhStatusChange();
        }

        private void doUpdateVheiclePositionAndCmdSchedule
                    (string vhID, ID_134_TRANS_EVENT_REP report_obj)
        {
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vhID);


            string current_sec_id = SCUtility.isEmpty(report_obj.CurrentSecID) ? string.Empty : report_obj.CurrentSecID;
            string current_adr_id = SCUtility.isEmpty(report_obj.CurrentAdrID) ? string.Empty : report_obj.CurrentAdrID;
            double current_x_axis = report_obj.XAxis;
            double current_y_axis = report_obj.YAxis;
            //double dir_angle = report_obj.DirectionAngle;
            float vh_angle =(float)report_obj.Angle;
            //DriveDirction drive_dirction = report_obj.DrivingDirection;
            double speed = report_obj.Speed;
            speed = speed == 0 ? 1 : speed;
            DriveDirction drive_dirction = getDrivingDirection(current_sec_id, vh.sWillPassAddressIDs);
            double dri_speed = drive_dirction == DriveDirction.DriveDirReverse ? -speed : speed;
            //如果這次上報的x、y 為0，則繼續拿上一次地來更新
            //if(current_x_axis == 0&& current_y_axis == 0)//更新位置X,Y皆為0
            //{

            //}
            //current_x_axis = current_x_axis == 0 ? vh.X_Axis : current_x_axis;
            //current_y_axis = current_y_axis == 0 ? vh.Y_Axis : current_y_axis;
            //double after_check_x_axis = current_x_axis;
            //double after_check_y_axis = current_y_axis;
            (double after_check_x_axis, double after_check_y_axis) = checkVehicleAxis(vhID, current_adr_id, current_x_axis, current_y_axis);

            if (SCUtility.isEmpty(current_adr_id))
            {
                ASECTION cur_sec_obj = scApp.SectionBLL.cache.GetSection(current_sec_id);
                current_adr_id = cur_sec_obj == null ? string.Empty : cur_sec_obj.FROM_ADR_ID;
            }

            ASECTION sec_obj = scApp.SectionBLL.cache.GetSection(current_sec_id);
            string current_seg_id = sec_obj == null ? string.Empty : sec_obj.SEG_NUM;
            string last_adr_id = vh.CUR_ADR_ID;
            string last_sec_id = vh.CUR_SEC_ID;
            string last_seg_id = vh.CUR_SEG_ID;

            double last_x_axis = vh.X_Axis;
            double last_y_axis = vh.Y_Axis;

            uint sec_dis = report_obj.SecDistance;
            lock (vh.PositionRefresh_Sync)
            {
                //cache.updateVheiclePosition_CacheManager(vhID, current_adr_id, current_sec_id, current_seg_id, sec_dis, drive_dirction, current_x_axis, current_y_axis);
                cache.updateVheiclePosition_CacheManager(vhID, current_adr_id, current_sec_id, current_seg_id, sec_dis, drive_dirction, after_check_x_axis, after_check_y_axis, vh_angle);

                var update_result = updateVheiclePositionToReserveControlModule(scApp.ReserveBLL, vh, current_sec_id, after_check_x_axis, after_check_y_axis, dri_speed, vh_angle,
                                                                                Mirle.Hlts.Utils.HltDirection.Forward, Mirle.Hlts.Utils.HltDirection.None);
                if (!update_result.OK)
                {
                    string message = $"The vehicles bumped, vh:{vh.VEHICLE_ID} with vh:{update_result.VehicleID}";
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleBLL), Device: Service.VehicleService.DEVICE_NAME_OHx,
                       Data: message,
                       Details: update_result.Description,
                       VehicleID: vh.VEHICLE_ID,
                       CarrierID: vh.CST_ID);
                    //如果發生碰撞或踩入別人預約的不是虛擬車的話，則就要對兩台車下達EMS
                    if (!update_result.VehicleID.StartsWith(Service.VehicleService.AvoidProcessor.VehicleVirtualSymbol))
                    {
                        bcf.App.BCFApplication.onErrorMsg(message);
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleBLL), Device: Service.VehicleService.DEVICE_NAME_OHx,
                           Data: $"The vehicles bumped will happend. send ems to {vhID}",
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                        //scApp.VehicleService.Send.Cancel(vh.VEHICLE_ID, "", CancelActionType.CmdEms);
                        AVEHICLE will_bumped_vh = cache.getVehicle(update_result.VehicleID);
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleBLL), Device: Service.VehicleService.DEVICE_NAME_OHx,
                           Data: $"The vehicles bumped will happend. send ems to {update_result.VehicleID}",
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                        //scApp.VehicleService.Send.Cancel(will_bumped_vh.VEHICLE_ID, "", CancelActionType.CmdEms);
                    }
                }
                ALINE line = scApp.getEQObjCacheManager().getLine();
                if (line.ServiceMode == SCAppConstants.AppServiceMode.Active)
                {
                    if (!SCUtility.isMatche(last_sec_id, current_sec_id))
                    {
                        vh.onLocationChange(current_sec_id, last_sec_id);
                    }
                    if (!SCUtility.isMatche(current_seg_id, last_seg_id))
                    {
                        vh.onSegmentChange(current_seg_id, last_seg_id);
                    }
                    if (last_x_axis != after_check_x_axis || last_y_axis != after_check_y_axis)
                    {
                        vh.onPositionChange(last_x_axis, last_y_axis, after_check_x_axis, after_check_y_axis);
                    }
                }
            }
        }


        /// <summary>
        /// 當X、
        /// </summary>
        /// <param name="vhID"></param>
        /// <param name="curAdrID"></param>
        /// <param name="real_x_axis"></param>
        /// <param name="real_y_axis"></param>
        /// <returns></returns>
        private (double after_check_x_axis, double after_check_y_axis) checkVehicleAxis(string vhID, string curAdrID, double real_x_axis, double real_y_axis)
        {
            if (SystemParameter.PassAxisDistance <= 0)
                return (real_x_axis, real_y_axis);
            if(string.IsNullOrWhiteSpace(curAdrID))
            {
                return (real_x_axis, real_y_axis);
            }
            var adrObject = scApp.ReserveBLL.GetHltMapAddress(curAdrID);
            if (!adrObject.isSuccess)
                return (real_x_axis, real_y_axis);
            double distance = getDistance(adrObject.x, adrObject.y, real_x_axis, real_y_axis);
            if (distance > SystemParameter.PassAxisDistance)
                return (real_x_axis, real_y_axis);
            else
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleBLL), Device: "OHT",
                   Data: $"vh:{vhID} of report x:{real_x_axis} y:{real_y_axis} and cur adr:{curAdrID} distance:{SystemParameter.PassAxisDistance} " +
                         $"less than distance:{SystemParameter.PassAxisDistance} fource change to x:{adrObject.x} y:{adrObject.y}",
                   VehicleID: vhID);
                return (adrObject.x, adrObject.y);
            }
        }
        private double getDistance(double x1, double y1, double x2, double y2)
        {
            double dx, dy;
            dx = x2 - x1;
            dy = y2 - y1;
            return Math.Sqrt(dx * dx + dy * dy);
        }



        public DriveDirction getDrivingDirection(string currentSec, string currentGuideAddress)
        {
            if (currentGuideAddress == null) return DriveDirction.DriveDirNone;
            var get_result = scApp.ReserveBLL.GetMapSectionsInfo(currentSec);
            if (get_result.isExist)
            {
                return 0;
            }
            string from_to_addresses = $"{SCUtility.Trim(get_result.StartAddressID)},{SCUtility.Trim(get_result.EndAddressID)}";

            string current_guide_Addresses = string.Join(",", currentGuideAddress);

            return current_guide_Addresses.Contains(from_to_addresses) ?
                   DriveDirction.DriveDirForward : DriveDirction.DriveDirReverse;
        }



        public ReserveCheckResult updateVheiclePositionToReserveControlModule(BLL.IReserveBLL reserveBLL, AVEHICLE vh, string currentSectionID, double x_axis, double y_axis, double speed, float angle,
                                                                                      Mirle.Hlts.Utils.HltDirection sensorDir, Mirle.Hlts.Utils.HltDirection forkDir)
        {
            string vh_id = vh.VEHICLE_ID;
            string section_id = currentSectionID;
            return reserveBLL.TryAddVehicleOrUpdate(vh_id, section_id, x_axis, y_axis, angle, speed, sensorDir, forkDir);
        }
        #region Vehicle Object Info

        /// <summary>
        /// 將AVEHICLE物件轉換成GBP的VEHICLE_INFO物件
        /// 要用來做物件的序列化所使用
        /// </summary>
        /// <param name="vh"></param>
        /// <returns></returns>
        public static byte[] Convert2GPB_VehicleInfo(AVEHICLE vh)
        {
            int vehicleType = (int)vh.VEHICLE_TYPE;
            int cmd_tpye = (int)vh.CmdType;
            int cmd_status = (int)vh.vh_CMD_Status;
            VEHICLE_INFO vh_gpb = new VEHICLE_INFO()
            {
                VEHICLEID = vh.VEHICLE_ID,
                IsTcpIpConnect = vh.isTcpIpConnect,
                VEHICLETYPE = (com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage.VehicleType)vehicleType,
                CURADRID = vh.CUR_ADR_ID == null ? string.Empty : vh.CUR_ADR_ID,
                CURSECID = vh.CUR_SEC_ID == null ? string.Empty : vh.CUR_SEC_ID,
                ACCSECDIST = vh.ACC_SEC_DIST,
                MODESTATUS = vh.MODE_STATUS,
                ACTSTATUS = vh.ACT_STATUS,
                TransferCmdID1 = vh.TRANSFER_ID == null ? string.Empty : vh.TRANSFER_ID,
                TransferCmdID2 = vh.TRANSFER_ID == null ? string.Empty : vh.TRANSFER_ID,
                CmdID1 = vh.CMD_ID == null ? string.Empty : vh.CMD_ID,
                CmdID2 = vh.CMD_ID == null ? string.Empty : vh.CMD_ID,
                PAUSESTATUS = vh.PauseStatus,
                CMDPAUSE = vh.CMD_PAUSE,
                BLOCKPAUSE = vh.BLOCK_PAUSE,
                OBSPAUSE = vh.OBS_PAUSE,
                HIDPAUSE = vh.HIDStatus,
                SAFETYDOORPAUSE = vh.SAFETY_PAUSE,
                EARTHQUAKEPAUSE = vh.EARTHQUAKE_PAUSE,
                RESERVEPAUSE = vh.RESERVE_PAUSE,
                ERROR = vh.ERROR,
                OBSDIST = vh.OBS_DIST,
                HasCstL = vh.HAS_CST ? 1 : 0,
                HasCstR = vh.HAS_CST ? 1 : 0,
                CstIdL = vh.CST_ID == null ? string.Empty : vh.CST_ID,
                CstIdR = vh.CST_ID == null ? string.Empty : vh.CST_ID,
                VEHICLEACCDIST = vh.VEHICLE_ACC_DIST,
                MANTACCDIST = vh.MANT_ACC_DIST,
                GRIPCOUNT = vh.GRIP_COUNT,
                GRIPMANTCOUNT = vh.GRIP_MANT_COUNT,

                StartAdr = vh.StartAdr == null ? string.Empty : vh.StartAdr,
                FromAdr = vh.FromAdr == null ? string.Empty : vh.FromAdr,
                ToAdr = vh.ToAdr == null ? string.Empty : vh.ToAdr,
                Speed = vh.Speed,
                ObsVehicleID = vh.ObsVehicleID == null ? string.Empty : vh.ToAdr,
                CmdType = (com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage.CommandType)cmd_tpye,
                VhCMDStatus = (com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage.CommandStatus)cmd_status,
                CurrentDriveDirction = vh.CurrentDriveDirction,
            };
            if (vh.PredictSections != null)
                vh_gpb.PredictPath.AddRange(vh.PredictSections);
            LogManager.GetLogger("VehicleHistoricalInfo").Trace(vh_gpb.ToString());

            byte[] arrayByte = new byte[vh_gpb.CalculateSize()];
            vh_gpb.WriteTo(new Google.Protobuf.CodedOutputStream(arrayByte));
            return arrayByte;
        }

        public static VEHICLE_INFO Convert2Object_VehicleInfo(byte[] raw_data)
        {
            return ToObject<VEHICLE_INFO>(raw_data);
        }
        private static T ToObject<T>(byte[] buf) where T : Google.Protobuf.IMessage<T>, new()
        {
            if (buf == null)
                return default(T);
            Google.Protobuf.MessageParser<T> parser = new Google.Protobuf.MessageParser<T>(() => new T());
            return parser.ParseFrom(buf);
        }
        #endregion Vehicle Object Info
        public class DB
        {
            VehicleDao vehicleDAO = null;
            ObjectPool<AVEHICLE> VehiclPool = null;
            public DB(SCApplication scApp)
            {
                vehicleDAO = scApp.VehicleDao;
                VehiclPool = scApp.VehiclPool;
            }
            public bool addVehicle(AVEHICLE _vh)
            {
                bool isSuccess = true;

                using (DBConnection_EF con = new DBConnection_EF())
                {
                    AVEHICLE vh = new AVEHICLE
                    {
                        VEHICLE_ID = _vh.VEHICLE_ID,
                        ACC_SEC_DIST = 0,
                        MODE_STATUS = 0,
                        ACT_STATUS = 0,
                        TRANSFER_ID = string.Empty,
                        CMD_ID = string.Empty,
                        BLOCK_PAUSE = 0,
                        CMD_PAUSE = 0,
                        OBS_PAUSE = 0,
                        //HAS_CST_L = false,
                        //HAS_CST_R = false,
                        VEHICLE_ACC_DIST = 0,
                        MANT_ACC_DIST = 0,
                        GRIP_COUNT = 0,
                        GRIP_MANT_COUNT = 0
                    };
                    //var vehicle_map_info = SCApplication.getInstance().getEQObjCacheManager().getVhRealID(vh_id);
                    //string real_id = vehicle_map_info.real_id;
                    //string location_id_r = vehicle_map_info.location_id_r;
                    //string location_id_l = vehicle_map_info.location_id_l;
                    //vh.setCarrierLocationInfo("", "");
                    vh.HAS_CST = false;

                    vehicleDAO.add(con, vh);
                }
                return isSuccess;
            }
            public virtual bool updateVehicleExcuteCMD(string vh_id, string cmd_id, string mcs_cmd_id)
            {
                bool isSuccess = false;
                AVEHICLE vh = new AVEHICLE();
                try
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext()) //todo kevin 需要確認是否要再使用此資料
                    {
                        //vh = vehicleDAO.getByID(con, vh_id);
                        //vh.VEHICLE_ID = vh_id;
                        //con.AVEHICLE.Attach(vh);
                        //vh.OHTC_CMD = cmd_id;
                        //vh.MCS_CMD = mcs_cmd_id;
                        ////vehicleDAO.update(con, vh);
                        //con.Entry(vh).Property(p => p.OHTC_CMD).IsModified = true;
                        //con.Entry(vh).Property(p => p.MCS_CMD).IsModified = true;
                        //vehicleDAO.doUpdate(scApp, con, vh);
                        //con.Entry(vh).State = EntityState.Detached;
                        isSuccess = true;
                    }
                }
                finally
                {
                    //scApp.VehiclPool.PutObject(vh);
                }
                return isSuccess;
            }
            public virtual bool updataVehicleLastFullyChargerTime(string vh_id)
            {
                bool isSuccess = false;
                AVEHICLE vh = VehiclPool.GetObject();
                try
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        vh.VEHICLE_ID = vh_id;
                        con.AVEHICLE.Attach(vh);
                        vh.LAST_FULLY_CHARGED_TIME = DateTime.Now;
                        con.Entry(vh).Property(p => p.LAST_FULLY_CHARGED_TIME).IsModified = true;
                        vehicleDAO.doUpdate(con, vh);
                        con.Entry(vh).State = EntityState.Detached;
                        isSuccess = true;
                    }
                }
                finally
                {
                    VehiclPool.PutObject(vh);
                }
                return isSuccess;
            }
            public virtual bool updataVehicleInstall(string vh_id)
            {
                bool isSuccess = false;
                AVEHICLE vh = VehiclPool.GetObject();
                try
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        vh.VEHICLE_ID = vh_id;
                        con.AVEHICLE.Attach(vh);
                        vh.IS_INSTALLED = true;
                        vh.INSTALLED_TIME = DateTime.Now;
                        con.Entry(vh).Property(p => p.IS_INSTALLED).IsModified = true;
                        con.Entry(vh).Property(p => p.INSTALLED_TIME).IsModified = true;
                        vehicleDAO.doUpdate(con, vh);
                        con.Entry(vh).State = EntityState.Detached;
                        isSuccess = true;
                    }
                }
                finally
                {
                    VehiclPool.PutObject(vh);
                }
                return isSuccess;
            }
            public virtual bool updataVehicleRemove(string vh_id)
            {
                bool isSuccess = false;
                AVEHICLE vh = VehiclPool.GetObject();
                try
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        vh.VEHICLE_ID = vh_id;
                        con.AVEHICLE.Attach(vh);
                        vh.IS_INSTALLED = false;
                        vh.REMOVED_TIME = DateTime.Now;
                        con.Entry(vh).Property(p => p.IS_INSTALLED).IsModified = true;
                        con.Entry(vh).Property(p => p.REMOVED_TIME).IsModified = true;
                        vehicleDAO.doUpdate(con, vh);
                        con.Entry(vh).State = EntityState.Detached;
                        isSuccess = true;
                    }
                }
                finally
                {
                    VehiclPool.PutObject(vh);
                }
                return isSuccess;
            }
            public virtual bool updataVehicleType(string vhID, E_VH_TYPE vhType)
            {
                bool isSuccess = false;
                AVEHICLE vh = VehiclPool.GetObject();
                try
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        vh.VEHICLE_ID = vhID;
                        con.AVEHICLE.Attach(vh);
                        vh.VEHICLE_TYPE = vhType;
                        con.Entry(vh).Property(p => p.VEHICLE_TYPE).IsModified = true;
                        vehicleDAO.doUpdate(con, vh);
                        con.Entry(vh).State = EntityState.Detached;
                        isSuccess = true;
                    }
                }
                finally
                {
                    VehiclPool.PutObject(vh);
                }
                return isSuccess;
            }
            public virtual AVEHICLE getVehicleByIDFromDB(string vh_id, bool isAttached = false)
            {
                AVEHICLE vh = null;
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    vh = vehicleDAO.getByID(con, vh_id);
                    if (vh != null && !isAttached)
                    {
                        con.Entry(vh).State = EntityState.Detached;
                    }
                }
                return vh;
            }
            public virtual List<AVEHICLE> loadAllVehicle()
            {
                List<AVEHICLE> vhs = null;
                using (DBConnection_EF con = new DBConnection_EF())
                {
                    vhs = vehicleDAO.loadAll(con);
                }
                return vhs;
            }
        }

        public class Cache
        {
            EQObjCacheManager eqObjCacheManager = null;
            public Cache(EQObjCacheManager eqObjCacheManager)
            {
                this.eqObjCacheManager = eqObjCacheManager;
            }
            public void updateVehicleStatus(CMDBLL cmdBLL, string vhID,
                                          string CstID, VHModeStatus mode_status, VHActionStatus act_status,
                                          VhStopSingle block_pause, VhStopSingle cmd_pause, VhStopSingle obs_pause, VhStopSingle safety_pause, VhStopSingle hid_pause, VhStopSingle error_status, VhStopSingle reserve_pause,
                                          bool has_cst,string cmdID)
            {
                var vh = eqObjCacheManager.getVehicletByVHID(vhID);
                vh.MODE_STATUS = mode_status;
                vh.ACT_STATUS = act_status;
                //vh.ChargeStatus = chargeStatus;
                vh.RESERVE_PAUSE = reserve_pause;
                vh.BLOCK_PAUSE = block_pause;
                vh.CMD_PAUSE = cmd_pause;
                vh.OBS_PAUSE = obs_pause;
                vh.HID_PAUSE = hid_pause;
                vh.SAFETY_PAUSE = safety_pause;
                
                vh.ERROR = error_status;
                vh.CST_ID = CstID;
                vh.HAS_CST = has_cst;
                //vh.ShelfStatus_L = left_shelf_status;
                //vh.ShelfStatus_R = right_shelf_status;

                if (!SCUtility.isMatche(vh.CMD_ID, cmdID))
                {
                    vh.CMD_ID = cmdID;
                    vh.TRANSFER_ID = tryGetTranCommandID(cmdBLL, cmdID);
                }

                //if (vh.RESERVE_PAUSE == VhStopSingle.StopSingleOff)
                //{
                //    vh.CurrentFailOverrideTimes = 0;
                //}

                //vh.OP_PAUSE = opPause;
                //vh.PredictSections = willPassSection;
                //vh.WillPassSectionID = willPassSection.ToList();
                vh.onVehicleStatusChange();
            }




            public void setWillPassSectionInfo(string vhID, List<string> willPassSection, List<string> willPassAddresses)
            {
                var vh = eqObjCacheManager.getVehicletByVHID(vhID);
                vh.PredictSections = willPassSection.ToArray();
                vh.WillPassSectionID = willPassSection;
                vh.sWillPassAddressIDs = string.Join(",", willPassAddresses);
                vh.ToAdr = willPassAddresses.Last();
                vh.ToSectionID = willPassSection.Last();
            }
            public void removeAlreadyPassedSection(string vhID, string sectionID)
            {
                var vh = eqObjCacheManager.getVehicletByVHID(vhID);
                if (vh.WillPassSectionID != null && vh.WillPassSectionID.Count > 0)
                    vh.WillPassSectionID.Remove(sectionID);
            }
            public void resetWillPassSectionInfo(string vhID)
            {
                var vh = eqObjCacheManager.getVehicletByVHID(vhID);
                if (vh != null)
                {
                    vh.PredictSections = null;
                    vh.WillPassSectionID = null;
                }
            }


            private string tryGetTranCommandID(CMDBLL cmdBLL, string cmdID)
            {
                var cmd_obj = cmdBLL.cache.getExcuteCmd(cmdID);
                if (cmd_obj == null)
                {
                    return "";
                }
                else
                {
                    return SCUtility.Trim(cmd_obj.TRANSFER_ID, true);
                }
            }

            public void updataVehicleMode(string vhID, VHModeStatus mode_status)
            {
                var vh = eqObjCacheManager.getVehicletByVHID(vhID);
                vh.MODE_STATUS = mode_status;
                vh.onVehicleStatusChange();
            }
            public void updateVehicleExcuteCMD(string vh_id, string cmd_id, string mcs_cmd_id)
            {
                AVEHICLE vh = new AVEHICLE();
                try
                {
                    //vh = vehicleDAO.getByID(con, vh_id);
                    //vh.VEHICLE_ID = vh_id;
                    //con.AVEHICLE.Attach(vh);
                    //vh.OHTC_CMD = cmd_id;
                    //vh.MCS_CMD = mcs_cmd_id;
                    ////vehicleDAO.update(con, vh);
                    //con.Entry(vh).Property(p => p.OHTC_CMD).IsModified = true;
                    //con.Entry(vh).Property(p => p.MCS_CMD).IsModified = true;
                    //vehicleDAO.doUpdate(scApp, con, vh);
                    //con.Entry(vh).State = EntityState.Detached;
                }
                finally
                {
                    //scApp.VehiclPool.PutObject(vh);
                }
            }

            public void updataVehicleLastFullyChargerTime(string vhID)
            {
                var vh = eqObjCacheManager.getVehicletByVHID(vhID);
                vh.LAST_FULLY_CHARGED_TIME = DateTime.Now;
            }
            public void updataVehicleInstall(string vhID)
            {
                var vh = eqObjCacheManager.getVehicletByVHID(vhID);
                vh.IS_INSTALLED = true;
                vh.INSTALLED_TIME = DateTime.Now;
            }

            public void updataVehicleRemove(string vhID)
            {
                var vh = eqObjCacheManager.getVehicletByVHID(vhID);
                vh.IS_INSTALLED = false;
                vh.REMOVED_TIME = DateTime.Now;
                vh.VehicleRemove();
            }

            public void updataVehicleType(string vhID, E_VH_TYPE vhType)
            {
                var vh = eqObjCacheManager.getVehicletByVHID(vhID);
                vh.VEHICLE_TYPE = vhType;
                vh.VehicleRemove();
            }

            public void updateVheiclePosition_CacheManager(string vhID, string adr_id, string sec_id, string seg_id, double sce_dis, DriveDirction driveDirction, double xAxis, double yAxis,double angle)
            {
                var vh = eqObjCacheManager.getVehicletByVHID(vhID);
                vh.CUR_ADR_ID = adr_id;
                vh.CUR_SEC_ID = sec_id;
                vh.CUR_SEG_ID = seg_id;
                vh.ACC_SEC_DIST = sce_dis;
                vh.CurrentDriveDirction = driveDirction;

                vh.X_Axis = xAxis;
                vh.Y_Axis = yAxis;
                //vh.DirctionAngle = dirctionAngle;
                vh.VehicleAngle = angle;
                vh.onVehiclePositionChange();
            }

            public void ResetCanNotReserveInfo(string vhID)
            {
                var vh = eqObjCacheManager.getVehicletByVHID(vhID);
                vh.CanNotReserveInfo = null;
            }
            public void SetUnsuccessReserveInfo(string vhID, AVEHICLE.ReserveUnsuccessInfo reserveUnsuccessInfo)
            {
                var vh = eqObjCacheManager.getVehicletByVHID(vhID);
                vh.CanNotReserveInfo = reserveUnsuccessInfo;
            }
            public void SetReservePause(string vhID, VhStopSingle stopSingle)
            {
                var vh = eqObjCacheManager.getVehicletByVHID(vhID);
                vh.RESERVE_PAUSE = stopSingle;
                //如果stop single是 off則重置計算override fail的次數
                if (stopSingle == VhStopSingle.StopSingleOff)
                {
                    vh.CurrentFailOverrideTimes = 0;
                }
                vh.onVehicleStatusChange();
            }

            public void SetCSTR(string vhID, bool hasCST)
            {
                var vh = eqObjCacheManager.getVehicletByVHID(vhID);
                vh.HAS_CST = hasCST;
                vh.onVehicleStatusChange();
            }
            public void SetCSTL(string vhID, bool hasCST)
            {
                var vh = eqObjCacheManager.getVehicletByVHID(vhID);
                vh.HAS_CST = hasCST;
                vh.onVehicleStatusChange();
            }

            public int getNoExcuteMcsCmdVhCount(E_VH_TYPE vh_type)
            {
                var vhs = eqObjCacheManager.getAllVehicle();
                return vhs.Where(vh => vh.VEHICLE_TYPE == vh_type &&
                               !vh.HasExcuteTransferCommand).Count();
            }

            public int getActVhCount()
            {
                var vhs = eqObjCacheManager.getAllVehicle();
                return vhs.Where(vh => vh.MODE_STATUS == ProtocolFormat.OHTMessage.VHModeStatus.AutoLocal
                        || vh.MODE_STATUS == ProtocolFormat.OHTMessage.VHModeStatus.AutoRemote).Count();
            }

            public int getActVhCount(E_VH_TYPE vh_type)
            {
                var vhs = eqObjCacheManager.getAllVehicle();
                return vhs.Where(vh => (vh.MODE_STATUS == ProtocolFormat.OHTMessage.VHModeStatus.AutoRemote) &&
                               vh.isTcpIpConnect &&
                               //vh.ACT_STATUS != ProtocolFormat.OHTMessage.VHActionStatus.Commanding &&
                               vh.VEHICLE_TYPE == vh_type).Count();
            }

            public int getIdleVhCount()
            {
                var vhs = eqObjCacheManager.getAllVehicle();
                return vhs.Where(vh => IsIdle(vh)).Count();
            }
            private bool IsIdle(AVEHICLE vh)
            {
                bool is_idle = true;
                //1.一定要是Auto的狀態
                is_idle &= vh.MODE_STATUS == ProtocolFormat.OHTMessage.VHModeStatus.AutoLocal ||
                           vh.MODE_STATUS == ProtocolFormat.OHTMessage.VHModeStatus.AutoRemote;
                //2.是處於沒有命令的狀態
                is_idle &= vh.ACT_STATUS == ProtocolFormat.OHTMessage.VHActionStatus.NoCommand;
                //3.不是處於沒有命令的狀態的話就是正在執行Move_Park或Move
                is_idle |= vh.ACT_STATUS == ProtocolFormat.OHTMessage.VHActionStatus.Commanding &&
                           (vh.CmdType == E_CMD_TYPE.Move_Park || vh.CmdType == E_CMD_TYPE.Move);
                return is_idle;
            }

            public List<AVEHICLE> loadVehicleBySEC_ID(string sec_id)
            {
                var vhs = eqObjCacheManager.getAllVehicle();
                return vhs.Where(vh => vh.CUR_SEC_ID.Trim() == sec_id.Trim()).ToList();
            }

            public List<AVEHICLE> loadAllErrorVehicle()
            {
                var vhs = eqObjCacheManager.getAllVehicle();

                return vhs.Where(vh => vh.ERROR == ProtocolFormat.OHTMessage.VhStopSingle.StopSingleOn).ToList();
            }
            public (bool isExist, AVEHICLE vh) IsVehicleExist(string vhID)
            {
                var vhs = eqObjCacheManager.getAllVehicle();
                var vh = vhs.Where(v => SCUtility.isMatche(v.VEHICLE_ID, vhID)).FirstOrDefault();
                return (vh != null, vh);
            }

            public bool IsVehicleExistByRealID(string vhRealID)
            {
                var vhs = eqObjCacheManager.getAllVehicle();
                int count = vhs.Where(vh => SCUtility.isMatche(vh.Real_ID, vhRealID)).Count();
                return count != 0;
            }
            public bool IsVehicleLocationExistByLocationRealID(string vhLocationRealID)
            {
                var vhs = eqObjCacheManager.getAllVehicle();
                int count = vhs.Where(vh => SCUtility.isMatche(vh.Real_ID, vhLocationRealID)).Count();
                return count != 0;
            }
            public AVEHICLE getVehicle(string vhID)
            {
                var vh = eqObjCacheManager.getVehicletByVHID(vhID);
                return vh;
            }
            public AVEHICLE getVehicleByRealID(string vhRealID)
            {
                var vh = eqObjCacheManager.getVehicletByRealID(vhRealID);
                return vh;
            }
            public AVEHICLE getVehicleByLocationRealID(string vhLocationRealID)
            {
                var vhs = eqObjCacheManager.getAllVehicle();
                var vh = vhs.Where(v => SCUtility.isMatche(v.Real_ID, vhLocationRealID)).FirstOrDefault();
                return vh;
            }
            public AVEHICLE getVehicleByMCSCmdID(string trnaCmdID)
            {
                var vhs = eqObjCacheManager.getAllVehicle();
                AVEHICLE vh = vhs.Where(v => SCUtility.isMatche(v.TRANSFER_ID, trnaCmdID)).
                    FirstOrDefault();
                return vh;
            }
            public AVEHICLE getVhByNum(int vhNum)
            {
                var vh = eqObjCacheManager.getAllVehicle().Where(v => v.Num == vhNum).FirstOrDefault();
                return vh;
            }

            public AVEHICLE getVehicleByCSTID(string cstID)
            {
                var vhs = eqObjCacheManager.getAllVehicle();
                AVEHICLE vh = vhs.Where(v => SCUtility.isMatche(v.CST_ID, cstID)).
                    FirstOrDefault();
                return vh;
            }
            public AVEHICLE findBestSuitableVhStepByStepFromAdr(GuideBLL GuideBLL, CMDBLL cmdBLL, string source, E_VH_TYPE vh_type)
            {
                AVEHICLE best_vh = null;

                //List<AVEHICLE> vhs = cache.loadAllVh();
                List<AVEHICLE> vhs = eqObjCacheManager.getAllVehicle().ToList();
                //1.過濾掉狀態不符的
                filterCanNotExcuteTranVh(ref vhs, cmdBLL, vh_type);
                //2.尋找距離Source最近的車子
                int minimum_cost = int.MaxValue;
                foreach (var vh in vhs)
                {
                    //var result = GuideBLL.getGuideInfo(vh.CUR_ADR_ID, source);
                    var result = GuideBLL.IsRoadWalkable(vh.CUR_ADR_ID, source, out int totalCost);
                    if (totalCost < minimum_cost)
                    {
                        best_vh = vh;
                        minimum_cost = totalCost;
                    }
                }
                return best_vh;
            }
            public void filterCanNotExcuteTranVh(ref List<AVEHICLE> vhs, CMDBLL cmdBLL, E_VH_TYPE vh_type)
            {
                if (vh_type != E_VH_TYPE.None)
                {
                    foreach (AVEHICLE vh in vhs.ToList())
                    {
                        if (vh.VEHICLE_TYPE != E_VH_TYPE.None
                            && vh.VEHICLE_TYPE != vh_type)
                        {
                            vhs.Remove(vh);
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleBLL), Device: "OHxC",
                               Data: $"vh id:{vh.VEHICLE_ID} vh type:{vh.VEHICLE_TYPE}, vehicle type not match current find vh type:{vh_type}," +
                                     $"so filter it out",
                               VehicleID: vh.VEHICLE_ID,
                               CarrierID: vh.CST_ID);
                        }
                    }
                }

                foreach (AVEHICLE vh in vhs.ToList())
                {
                    if (!vh.isTcpIpConnect)
                    {
                        vhs.Remove(vh);
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleBLL), Device: "OHTC",
                           Data: $"vh id:{vh.VEHICLE_ID} of tcp ip connection is :{vh.isTcpIpConnect}" +
                                 $"so filter it out",
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                    }
                }

                foreach (AVEHICLE vh in vhs.ToList())
                {
                    if (vh.IsError)
                    {
                        vhs.Remove(vh);
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleBLL), Device: "OHTC",
                           Data: $"vh id:{vh.VEHICLE_ID} of error flag is :{vh.IsError}" +
                                 $"so filter it out",
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                    }
                }

                foreach (AVEHICLE vh in vhs.ToList())// add by Mark 20210609 發現OHTC沒有檢查是否為OHT ActionStatus NoCommand
                {
                    if (vh.ACT_STATUS != VHActionStatus.NoCommand)
                    {
                        vhs.Remove(vh);
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleBLL), Device: "OHTC",
                       Data: $"vh id:{vh.VEHICLE_ID} current action  status is {vh.ACT_STATUS}," +
                             $"so filter it out",
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                    }
                }

                foreach (AVEHICLE vh in vhs.ToList())
                {
                    if (vh.MODE_STATUS != VHModeStatus.AutoRemote)
                    {
                        vhs.Remove(vh);
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleBLL), Device: "OHTC",
                           Data: $"vh id:{vh.VEHICLE_ID} current mode status is {vh.MODE_STATUS}," +
                                 $"so filter it out",
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                    }
                }
                foreach (AVEHICLE vh in vhs.ToList())
                {
                    if (SCUtility.isEmpty(vh.CUR_ADR_ID))
                    {
                        vhs.Remove(vh);
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleBLL), Device: "OHTC",
                           Data: $"vh id:{vh.VEHICLE_ID} current address is empty," +
                                 $"so filter it out",
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                    }
                }
                foreach (AVEHICLE vh in vhs.ToList())
                {
                    //if (!SCUtility.isEmpty(vh.TRANSFER_ID_1) &&
                    //    !SCUtility.isEmpty(vh.TRANSFER_ID_2))
                    //var chack_can_assign_cmd_result = cmdBLL.canAssignCmdNew(vh.VEHICLE_ID, E_CMD_TYPE.LoadUnload);
                    //var chack_can_assign_cmd_result = cmdBLL.canAssignCmdNew(vh, E_CMD_TYPE.LoadUnload);

                    //只要有在執行命令就代表這不能服務新的命令
                    var has_assign_cmd = cmdBLL.hasAssignCmd(vh);
                    if (has_assign_cmd.hasAssign)
                    {
                        vhs.Remove(vh);
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleBLL), Device: "OHTC",
                           Data: $"vh id:{vh.VEHICLE_ID} has assign command cmd ids:{string.Join(",", has_assign_cmd.assignCmdIDs)}," +
                                 $"so filter it out",
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                    }
                }
                //A202105120-01
                foreach (AVEHICLE vh in vhs.ToList())
                {
                    if (vh.HAS_CST)
                    {
                        vhs.Remove(vh);
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleBLL), Device: "AGVC",
                           Data: $"vh id:{vh.VEHICLE_ID} has carry cst,carrier id:{SCUtility.Trim(vh.CST_ID, true)}" +
                                 $"so filter it out",
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                    }
                }

            }

            public bool canAssignTransferCmd(CMDBLL cmdBLL, AVEHICLE vh, BLL.CMDBLL.CommandTranDir transferDir = CMDBLL.CommandTranDir.None)
            {
                if (!vh.isTcpIpConnect)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleBLL), Device: "OHTC",
                       Data: $"vh id:{vh.VEHICLE_ID} of tcp ip connection is :{vh.isTcpIpConnect}" +
                             $"so filter it out",
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                    return false;
                }
                if (vh.IsError)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleBLL), Device: "OHTC",
                       Data: $"vh id:{vh.VEHICLE_ID} of error flag is :{vh.IsError}" +
                             $"so filter it out",
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                    return false;
                }

                if (vh.MODE_STATUS != VHModeStatus.AutoRemote)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleBLL), Device: "OHTC",
                       Data: $"vh id:{vh.VEHICLE_ID} current mode status is {vh.MODE_STATUS}," +
                             $"so filter it out",
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                    return false;
                }
                if (SCUtility.isEmpty(vh.CUR_ADR_ID))
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleBLL), Device: "OHTC",
                       Data: $"vh id:{vh.VEHICLE_ID} current address is empty," +
                             $"so filter it out",
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                    return false;
                }
                if (vh.ACT_STATUS!= VHActionStatus.NoCommand) // add by Mark 20210430
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleBLL), Device: "OHTC",
                       Data: $"vh id:{vh.VEHICLE_ID} current action  status is {vh.ACT_STATUS}," +
                             $"so filter it out",
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                    return false;
                }
                //var chack_can_assign_cmd_result = cmdBLL.canAssignCmdNew(vh, E_CMD_TYPE.LoadUnload);
                //var chack_can_assign_cmd_result = cmdBLL.ICanAssignCmd(vh, E_CMD_TYPE.LoadUnload, transferDir);
                var chack_can_assign_cmd_result = cmdBLL.CanAssignCmd(vh, E_CMD_TYPE.LoadUnload, transferDir);
                if (!chack_can_assign_cmd_result.canAssign)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleBLL), Device: "OHTC",
                       Data: chack_can_assign_cmd_result.result,
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                    return false;
                }
                return true;
            }

            public AVEHICLE getVehicleByLocationID(string locationID)
            {
                AVEHICLE vh = null;
                var vhs = eqObjCacheManager.getAllVehicle().ToList();
                vh = vhs.Where(v => SCUtility.isMatche(v.Real_ID, locationID)).FirstOrDefault();
                return vh;
            }

            public List<AVEHICLE> loadAllVh()
            {
                var vhs = eqObjCacheManager.getAllVehicle().ToList();
                return vhs;
            }
            /// <summary>
            /// 取得目前有車子所在的section
            /// </summary>
            /// <param name="byPassVhID"></param>
            /// <returns></returns>
            public List<string> LoadVehicleCurrentSection(string byPassVhID)
            {
                var vhs = eqObjCacheManager.getAllVehicle().
                          Where(vh => !SCUtility.isMatche(vh.VEHICLE_ID, byPassVhID)).
                          Select(vh => vh.CUR_SEC_ID).
                          ToList();
                return vhs;
            }

            public List<AVEHICLE> loadVhBySegmentID(string segID)
            {
                var vhs = eqObjCacheManager.getAllVehicle();
                return vhs.
                       Where(vh => vh.CUR_SEG_ID.Trim() == segID.Trim()).
                       ToList();
            }
            public List<AVEHICLE> loadVhBySectionIDs(List<string> segIDs)
            {
                var vhs = eqObjCacheManager.getAllVehicle();
                return vhs.
                       Where(vh => segIDs.Contains(SCUtility.Trim(vh.CUR_SEC_ID, true))).
                       ToList();
            }

            public List<AVEHICLE> loadVhByZoneID(string zoneID)
            {
                var vhs = eqObjCacheManager.getAllVehicle();
                return vhs.
                       Where(vh => SCUtility.isMatche(vh.CUR_ZONE_ID, zoneID) &&
                                   vh.IS_INSTALLED).
                       ToList();
            }

            public UInt16 getVhCurrentModeInAutoRemoteCount()
            {
                var vhs = eqObjCacheManager.getAllVehicle();
                return (UInt16)vhs.
                       Where(vh => vh.MODE_STATUS == VHModeStatus.AutoRemote).
                       Count();
            }
            public UInt16 getVhCurrentModeInAutoLocalCount()
            {
                var vhs = eqObjCacheManager.getAllVehicle();
                return (UInt16)vhs.
                       Where(vh => vh.MODE_STATUS == VHModeStatus.AutoLocal).
                       Count();
            }
            public UInt16 getVhCurrentStatusInIdleCount()
            {
                var vhs = eqObjCacheManager.getAllVehicle();
                return (UInt16)vhs.
                       Where(vh => vh.MODE_STATUS == VHModeStatus.AutoRemote &&
                                   vh.ACT_STATUS == VHActionStatus.NoCommand &&
                                   !vh.IsError).
                       Count();
            }

            public UInt16 getVhCurrentStatusInErrorCount()
            {
                var vhs = eqObjCacheManager.getAllVehicle();
                return (UInt16)vhs.
                       Where(vh => vh.ERROR == VhStopSingle.StopSingleOn).
                       Count();
            }


            public AVEHICLE getVhOnAddress(string adrID)
            {
                List<AVEHICLE> vhs = eqObjCacheManager.getAllVehicle();
                return vhs.Where(vh => vh.ACT_STATUS == VHActionStatus.NoCommand &&
                                       vh.CUR_ADR_ID.Trim() == adrID.Trim()).
                           SingleOrDefault();
            }
            public bool hasVhOnAddress(string adrID)
            {
                List<AVEHICLE> vhs = eqObjCacheManager.getAllVehicle();
                return vhs.Where(vh => vh.ACT_STATUS == VHActionStatus.NoCommand &&
                                       vh.CUR_ADR_ID.Trim() == adrID.Trim()).
                           Count() != 0;
            }
            public bool hasChargingVhOnAddress(string adrID)
            {
                List<AVEHICLE> vhs = eqObjCacheManager.getAllVehicle();
                return vhs.Where(vh => vh.CUR_ADR_ID.Trim() == adrID.Trim()).
                           Count() != 0;
            }
            public bool hasVhGoingAdr(string adrID)
            {
                List<AVEHICLE> vhs = eqObjCacheManager.getAllVehicle();
                return vhs.Where(vh => vh.ACT_STATUS == VHActionStatus.Commanding &&
                                       vh.CmdType == E_CMD_TYPE.Move_Charger &&
                                       vh.ToAdr.Trim() == adrID.Trim()).
                           Count() != 0;
            }

            public bool hasVhOnAddresses(List<string> addresses)
            {
                List<AVEHICLE> vhs = eqObjCacheManager.getAllVehicle();
                return vhs.Where(vh => vh.ACT_STATUS == VHActionStatus.NoCommand &&
                                       addresses.Contains(SCUtility.Trim(vh.ToAdr, true))).
                           Count() != 0;
            }

            public bool IsCarryCstByCstID(string vhID, string cstID)
            {
                List<AVEHICLE> vhs = eqObjCacheManager.getAllVehicle();
                return vhs.Where(vh => SCUtility.isMatche(vh.VEHICLE_ID, vhID) &&
                                       SCUtility.isMatche(vh.CST_ID, cstID)).
                           Count() > 0;
            }


        }

        public class Redis
        {
            RedisCacheManager redisCache = null;
            public Redis(RedisCacheManager _redisCache)
            {
                redisCache = _redisCache;
            }
            TimeSpan POSITION_TIMEOUT = new TimeSpan(0, 5, 0);
            public void setPositionReportInfo2Redis(string vh_id, ID_134_TRANS_EVENT_REP report_obj)
            {
                string key_word_position = $"{SCAppConstants.REDIS_KEY_WORD_POSITION_REPORT}_{vh_id}";
                byte[] arrayByte = new byte[report_obj.CalculateSize()];
                report_obj.WriteTo(new Google.Protobuf.CodedOutputStream(arrayByte));
                redisCache.Obj2ByteArraySetAsync(key_word_position, arrayByte, POSITION_TIMEOUT);
            }
        }

        public class Web
        {
            WebClientManager webClientManager = null;
            //List<string> notify_urls = new List<string>()
            //{
            //    //"http://stk01.asek21.mirle.com.tw:15000",
            //     "http://agvc.asek21.mirle.com.tw:15000"
            //};
            List<string> notify_urls = new List<string>();
            const string ERROR_HAPPEND_CONST = "99";

            public Web(WebClientManager _webClient)
            {
                webClientManager = _webClient;
            }
            public void errorHappendNotify()
            {
                try
                {
                    string[] action_targets = new string[]
                    {
                    "weatherforecast"
                    };
                    string[] param = new string[]
                    {
                    ERROR_HAPPEND_CONST,
                    };
                    foreach (string notify_url in notify_urls)
                    {
                        string result = webClientManager.GetInfoFromServer(notify_url, action_targets, param);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                }
            }
            public void commandSendCompleteNotify(string vhID)
            {
                try
                {
                    string assign_vh_id = vhID;
                    string vh_no = vhID.Length > 0 ? vhID.Last().ToString() : "";
                    string[] action_targets = new string[]
                    {
                    "weatherforecast"
                    };
                    string[] param = new string[]
                    {
                    vh_no,
                    };
                    foreach (string notify_url in notify_urls)
                    {
                        string result = webClientManager.GetInfoFromServer(notify_url, action_targets, param);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                }
            }

            public void vehicleDisconnection()
            {
                vehicleDisconnection("agv");
            }
            public void vehicleDisconnection(string lineName)
            {
                try
                {
                    lineName = lineName.ToLower();
                    string notify_name = $"{lineName}_dis";
                    string[] action_targets = new string[]
                    {
                    "weatherforecast"
                    };
                    string[] param = new string[]
                    {
                        notify_name,
                    };
                    foreach (string notify_url in notify_urls)
                    {
                        string result = webClientManager.GetInfoFromServer(notify_url, action_targets, param);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                }
            }

        }

    }


}
