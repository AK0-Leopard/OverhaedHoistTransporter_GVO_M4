using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.BLL;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.Data.VO.Interface;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Service
{
    public class MTLService
    {
        //public readonly string MTL_CAR_OUT_BUFFER_ADDRESS = "20292";
        //public readonly string MTL_CAR_IN_BUFFER_ADDRESS = "24294";
        //public readonly string MTL_ADDRESS = "20293";
        //public readonly string MTL_SYSTEM_IN_ADDRESS = "20198";
        const ushort CAR_ACTION_MODE_NO_ACTION = 0;
        const ushort CAR_ACTION_MODE_ACTION = 1;
        const ushort CAR_ACTION_MODE_ACTION_FOR_MCS_COMMAND = 2;
        VehicleService VehicleService = null;
        VehicleBLL vehicleBLL = null;
        ReportBLL reportBLL = null;
        private SCApplication scApp = null;
        //MaintainLift mtx = null;
        //string carOutVhID = "";
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public MTLService()
        {
        }
        public void start(SCApplication app)
        {
            scApp = app;
            List<AEQPT> eqpts = app.getEQObjCacheManager().getAllEquipment();

            foreach (var eqpt in eqpts)
            {
                if (eqpt is IMaintainDevice)
                {
                    IMaintainDevice maintainDevice = eqpt as IMaintainDevice;
                    if (maintainDevice is MaintainLift)
                    {
                        MaintainLift maintainLift = eqpt as MaintainLift;
                        maintainLift.addEventHandler(nameof(MTLService), nameof(maintainLift.Plc_Link_Stat), PublishMTLInfo);
                        maintainLift.addEventHandler(nameof(MTLService), nameof(maintainLift.Is_Eq_Alive), PublishMTLInfo);
                        maintainLift.addEventHandler(nameof(MTLService), nameof(maintainLift.MTxMode), PublishMTLInfo);
                        maintainLift.addEventHandler(nameof(MTLService), nameof(maintainLift.Interlock), PublishMTLInfo);
                        maintainLift.addEventHandler(nameof(MTLService), nameof(maintainLift.CurrentCarID), PublishMTLInfo);
                        maintainLift.addEventHandler(nameof(MTLService), nameof(maintainLift.HasVehicle), PublishMTLInfo);
                        maintainLift.addEventHandler(nameof(MTLService), nameof(maintainLift.EQ_Error), PublishMTLInfo);
                        maintainLift.addEventHandler(nameof(MTLService), nameof(maintainLift.MTLLocation), PublishMTLInfo);
                        maintainLift.addEventHandler(nameof(MTLService), nameof(maintainLift.MTLMovingStatus), PublishMTLInfo);
                        maintainLift.addEventHandler(nameof(MTLService), nameof(maintainLift.Encoder), PublishMTLInfo);
                        maintainLift.addEventHandler(nameof(MTLService), nameof(maintainLift.MTLRailStatus), PublishMTLInfo);
                        maintainLift.addEventHandler(nameof(MTLService), nameof(maintainLift.CurrentPreCarOurDistance), PublishMTLInfo);
                        maintainLift.addEventHandler(nameof(MTLService), nameof(maintainLift.SynchronizeTime), PublishMTLInfo);
                        maintainLift.addEventHandler(nameof(MTLService), nameof(maintainLift.MTLCarOutSafetyCheck), PublishMTLInfo);
                        maintainLift.addEventHandler(nameof(MTLService), nameof(maintainLift.MTLCarOutInterlock), PublishMTLInfo);
                        maintainLift.addEventHandler(nameof(MTLService), nameof(maintainLift.MTLCarInSafetyCheck), PublishMTLInfo);
                        maintainLift.addEventHandler(nameof(MTLService), nameof(maintainLift.MTLCarInInterlock), PublishMTLInfo);
                        maintainLift.addEventHandler(nameof(MTLService), nameof(maintainLift.OHTCCarOutInterlock), PublishMTLInfo);
                        maintainLift.addEventHandler(nameof(MTLService), nameof(maintainLift.OHTCCarOutReady), PublishMTLInfo);
                        maintainLift.addEventHandler(nameof(MTLService), nameof(maintainLift.OHTCCarOutMoving), PublishMTLInfo);
                        maintainLift.addEventHandler(nameof(MTLService), nameof(maintainLift.OHTCCarOutMoveComplete), PublishMTLInfo);
                        maintainLift.addEventHandler(nameof(MTLService), nameof(maintainLift.OHTCCarInInterlock), PublishMTLInfo);
                        maintainLift.addEventHandler(nameof(MTLService), nameof(maintainLift.OHTCCarInMoving), PublishMTLInfo);
                        maintainLift.addEventHandler(nameof(MTLService), nameof(maintainLift.OHTCCarInMoveComplete), PublishMTLInfo);
                    }
                }
            }

            VehicleService = app.VehicleService;
            vehicleBLL = app.VehicleBLL;
            reportBLL = app.ReportBLL;
            //  mtl = app.getEQObjCacheManager().getEquipmentByEQPTID("MTL") as MaintainLift;
        }


        //bool cancelCarOutRequest = false;
        //bool carOurSuccess = false;

        /// <summary>
        /// 處理人員由MTL執行CAR OUT時的流程
        /// </summary>
        /// <param name="vhNum"></param>
        /// <returns></returns>
        //public (bool isSuccess, string result) carOutRequset(IMaintainDevice mtx, int vhNum)
        //{
        //    AVEHICLE pre_car_out_vh = vehicleBLL.cache.getVhByNum(vhNum);
        //    if (pre_car_out_vh == null)
        //        return (false, $"vh num:{vhNum}, not exist.");
        //    else
        //    {
        //        bool isSuccess = true;
        //        string result = "";
        //        var check_result = checkVhAndMTxCarOutStatus(mtx, null, pre_car_out_vh);
        //        isSuccess = check_result.isSuccess;
        //        result = check_result.result;
        //        if (isSuccess)
        //        {
        //            (bool isSuccess, string result) process_result = default((bool isSuccess, string result));
        //            if (mtx is MaintainLift)
        //            {
        //                process_result = processCarOutScenario(mtx as MaintainLift, pre_car_out_vh);
        //            }
        //            else if (mtx is MaintainSpace)
        //            {
        //                process_result = processCarOutScenario(mtx as MaintainSpace, pre_car_out_vh);
        //            }
        //            else
        //            {
        //                return process_result;
        //            }
        //            isSuccess = process_result.isSuccess;
        //            result = process_result.result;
        //        }
        //        return (isSuccess, result);
        //    }
        //}
        /// <summary>
        /// 處理人員由OHTC執行CAR OUT時的流程
        /// </summary>
        /// <param name="vhID"></param>
        /// <returns></returns>
        //public (bool isSuccess, string result) carOutRequset(IMaintainDevice mtx, string vhID)
        //{
        //    AVEHICLE pre_car_out_vh = vehicleBLL.cache.getVhByID(vhID);
        //    bool isSuccess = true;
        //    string result = "";
        //    var check_result = checkVhAndMTxCarOutStatus(mtx, null, pre_car_out_vh);
        //    isSuccess = check_result.isSuccess;
        //    result = check_result.result;
        //    //2.向MTL發出Car out request
        //    //成功後開始向MTL發送該台Vh的當前狀態，並在裡面判斷是否有收到Cancel的命令，有的話要將資料清空
        //    //Trun on 給MTL的Interlock flag
        //    //將該台Vh變更為AutoToMtl
        //    if (isSuccess)
        //    {
        //        var send_result = mtx.carOutRequest((UInt16)pre_car_out_vh.Num);
        //        if (send_result.isSendSuccess && send_result.returnCode == 1)
        //        {
        //            (bool isSuccess, string result) process_result = default((bool isSuccess, string result));
        //            if (mtx is MaintainLift)
        //            {
        //                process_result = processCarOutScenario(mtx as MaintainLift, pre_car_out_vh);
        //            }
        //            else if (mtx is MaintainSpace)
        //            {
        //                process_result = processCarOutScenario(mtx as MaintainSpace, pre_car_out_vh);
        //            }
        //            else
        //            {
        //                return process_result;
        //            }
        //            isSuccess = process_result.isSuccess;
        //            result = process_result.result;
        //        }
        //        else
        //        {
        //            isSuccess = false;
        //            result = $"Request car fail,Send result:{send_result.isSendSuccess}, return code:{send_result.returnCode}";
        //        }
        //    }
        //    return (isSuccess, result);
        //}

        public (bool isSuccess, string result) checkVhAndMTxCarOutStatus(IMaintainDevice mtx, IMaintainDevice dockingMtx, AVEHICLE car_out_vh)
        {
            bool isSuccess = true;
            string result = "";

            //1.要判斷目前車子的狀態
            if (isSuccess && mtx.OHTCCarOutInterlock)
            {
                isSuccess = false;
                result = $"MTx:{mtx.DeviceID} Current CarOutInterlock:{mtx.OHTCCarOutInterlock}, can't excute car out requset.";
            }

            if (isSuccess && car_out_vh == null)
            {
                isSuccess = false;
                result = $"vh not exist.";
                //如果car_out_vh是Null就直接return回去
                return (isSuccess, result);
            }
            string vh_id = car_out_vh.VEHICLE_ID;
            if (isSuccess && !car_out_vh.isTcpIpConnect)
            {
                isSuccess = false;
                result = $"vh id:{vh_id}, not connection.";
            }
            if (isSuccess && car_out_vh.MODE_STATUS == ProtocolFormat.OHTMessage.VHModeStatus.Manual)
            {
                isSuccess = false;
                result = $"Vehicle:{vh_id}, current mode is:{car_out_vh.MODE_STATUS}, can't excute auto car out";
            }
            if (isSuccess && SCUtility.isEmpty(car_out_vh.CUR_SEC_ID))
            {
                isSuccess = false;
                result = $"vh id:{vh_id}, current section is empty.";
            }
            if (isSuccess && SCUtility.isEmpty(car_out_vh.CUR_ADR_ID))
            {
                isSuccess = false;
                result = $"vh id:{vh_id}, current address is empty.";
            }

            //要判斷目前到車子所在位置到目的地(MTL/MTS)路徑是不是通的
            //KeyValuePair<string[], double> route_distance;
            int route_distance;

            if (isSuccess && !scApp.GuideBLL.IsRoadWalkable(car_out_vh.CUR_ADR_ID, mtx.DeviceAddress, out route_distance))
            {
                isSuccess = false;
                result = $"vh id:{vh_id}, current address:{car_out_vh.CUR_ADR_ID} to device:{mtx.DeviceID} of address id:{mtx.DeviceAddress} not walkable.";
            }

            //2.要判斷MTL的 Safety check是否有On且是否為Auto Mode
            if (isSuccess && !SCUtility.isEmpty(mtx.PreCarOutVhID))
            {
                isSuccess = false;
                result = $"MTL:{mtx.DeviceID} Current process car our vh:{mtx.PreCarOutVhID}, can't excute cat out again.";
            }

            if (isSuccess && !mtx.IsAlive)
            {
                isSuccess = false;
                result = $"MTL:{mtx.DeviceID} Current Alive:{mtx.IsAlive}, can't excute cat out requset.";
            }
            if (isSuccess && mtx.MTxMode == ProtocolFormat.OHTMessage.MTxMode.Manual)
            {
                isSuccess = false;
                result = $"MTL:{mtx.DeviceID} Current Mode:{mtx.MTxMode}, can't excute cat out requset.";
            }
            if (isSuccess && isCarOutProcessing())
            {
                isSuccess = false;
                result = $"MTL:{mtx.DeviceID} is in car out processing, can't excute cat out requset.";
            }
            //if (isSuccess && !mtx.CarOutSafetyCheck)
            //{
            //    isSuccess = false;
            //    result = $"MTx:{mtx.DeviceID} CarOutSafetyCheck:{mtx.CarOutSafetyCheck}, can't excute cat out requset.";
            //}


            return (isSuccess, result);
        }

        public (bool isSuccess, string result) CarOurRequest(IMaintainDevice mtx, AVEHICLE car_out_vh)
        {
            bool is_success = false;
            string result = "";
            var send_result = mtx.carOutRequest((UInt16)car_out_vh.Num);
            is_success = send_result.isSendSuccess && send_result.returnCode == 1;
            if (!is_success)
            {
                result = $"MTL:{mtx.DeviceID} reject car our request. return code:{send_result.returnCode}";
            }
            else
            {
                result = "OK";
            }
            return (is_success, result);
        }


        public bool readyToCarOutStart(bool isManualCarOut)//須將與MCS連線切為Online Local
        {
            //to do 檢查現在是否為AutoLocal狀態，不是的話要切過去
            bool result = true;
            //20210827 Hsinyu Chang只有在online remote時需要切換，online local或offline都可以直接放行
            if (scApp.getEQObjCacheManager().getLine().Host_Control_State == SCAppConstants.LineHostControlState.HostControlState.On_Line_Remote)
            {
                result = false;
                scApp.LineService.OnlineLocalWithHostMTL();
            }

            if (!isManualCarOut)
            {
                if (scApp.CMDBLL.getCMD_MCSIsUnfinishedCount() > 0)
                {
                    return false;
                }

                List<ACMD> unfinish_cmd = scApp.CMDBLL.loadUnfinishCmd();
                if (unfinish_cmd.Count > 0)
                {
                    return false;
                }
            }

            return result;
        }

        private bool readyToCarInStart()//須將與MCS連線切為Online Local
        {
            bool result = true;
            //20210827 Hsinyu Chang只有在online remote時需要切換，online local或offline都可以直接放行
            if (scApp.getEQObjCacheManager().getLine().Host_Control_State == SCAppConstants.LineHostControlState.HostControlState.On_Line_Remote)
            {
                result = false;
                scApp.LineService.OnlineLocalWithHostMTL();
            }

            MaintainLift MTL = scApp.EqptBLL.OperateCatch.GetMaintainLift();
            
            if(MTL == null)
            {
                return false;
            }
            string move_adr = string.Empty;

            if (BCFUtility.isMatche(scApp.BC_ID, SCAppConstants.WorkVersion.VERSION_NAME_AT_S_OHTC_301))
            {
                move_adr = (int.Parse(MTL.MTL_SYSTEM_IN_ADDRESS) + 2).ToString();
            }
            else
            {
                move_adr = (int.Parse(MTL.MTL_SYSTEM_IN_ADDRESS) -2).ToString();
            }

            //to do 檢查現在是否為AutoLocal狀態，不是的話要切過去
            if (scApp.CMDBLL.getCMD_MCSIsUnfinishedCount() > 0)
            {
                return false;
            }

            List<ACMD> unfinish_cmd = scApp.CMDBLL.loadUnfinishCmd();
            if (unfinish_cmd.Count > 0)
            {
                return false;
            }

            List<AVEHICLE> vhs = scApp.VehicleBLL.cache.loadAllVh();
            foreach(AVEHICLE vh in vhs)
            {
                if (vh.isTcpIpConnect||vh.IS_INSTALLED)
                {
                    if(vh.CUR_SEG_ID == MTL.MTL_SEGMENT)
                    {
                        if (string.IsNullOrWhiteSpace( vh.CurrentExcuteCmdID ))
                        {
                            scApp.CMDBLL.doCreatCommand(vh.VEHICLE_ID, cmd_type: E_CMD_TYPE.Move, destination: move_adr);
                        }
                        result = false;
                    }
                }
            }
            return result;
            //return true;
        }


        private bool CarInVhReadyCheck()//檢查CarIn的OHT是否復電上線了
        {
            MaintainLift MTL = scApp.EqptBLL.OperateCatch.GetMaintainLift();
            if (MTL == null)
            {
                return false;
            }

            AVEHICLE car_in_vh = vehicleBLL.cache.getVhOnAddress(MTL.MTL_ADDRESS);
            if (car_in_vh != null && car_in_vh.isTcpIpConnect)
            {
                if (car_in_vh.MODE_STATUS == ProtocolFormat.OHTMessage.VHModeStatus.AutoMtl)
                {
                    return true;
                }
            }
            return false;

        }

        private long syncCarOutPoint = 0;
        public bool isCarOutProcessing()
        {
            return syncCarOutPoint != 0;
        }
        public (bool isSuccess, string result) processCarOutScenario(MaintainLift mtx, AVEHICLE preCarOutVh)
        {
            if (System.Threading.Interlocked.Exchange(ref syncCarOutPoint, 1) == 0)
            {
                try
                {
                    string pre_car_out_vh_id = preCarOutVh.VEHICLE_ID;
                    string pre_car_out_vh_ohtc_cmd_id = preCarOutVh.CMD_ID;
                    string pre_car_out_vh_cur_adr_id = preCarOutVh.CUR_ADR_ID;
                    bool isSuccess;
                    string result = "OK";
                    mtx.CancelCarOutRequest = false;
                    mtx.CarOurSuccess = false;


                    if (!SpinWait.SpinUntil(() => readyToCarOutStart(false) == true, 600000))
                    {
                        isSuccess = false;
                        result = $"Process car out scenario,but ohtc status is not ready " +
                        $",please check host state is autolocal (or host offline) and all mcs cmd is over.";
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                                 Data: result,
                                 XID: mtx.DeviceID);
                        return (isSuccess, result);
                    }

                    //if (!SpinWait.SpinUntil(() => mtx.MTLCarOutSafetyCheck == true , 60000))
                    //{
                    //    isSuccess = false;
                    //    result = $"Process car out scenario,but mtl:{mtx.DeviceID} status not ready " +
                    //    $"{nameof(mtx.MTLCarOutSafetyCheck)}:{mtx.MTLCarOutSafetyCheck}";
                    //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                    //             Data: result,
                    //             XID: mtx.DeviceID);
                    //    return (isSuccess, result);
                    //}

                    CarOutStart(mtx);
                    if (!SpinWait.SpinUntil(() => mtx.MTLCarOutInterlock == true, 60000))
                    {
                        isSuccess = false;
                        result = $"Process car out scenario,but mtl:{mtx.DeviceID} status not ready " +
                        $"{nameof(mtx.MTLCarOutInterlock)}:{mtx.MTLCarOutInterlock}";
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                                 Data: result,
                                 XID: mtx.DeviceID);
                        return (isSuccess, result);
                    }
                    //isSuccess = VehicleService.doReservationVhToMaintainsBufferAddress(pre_car_out_vh_id, MTL_CAR_OUT_BUFFER_ADDRESS);

                    isSuccess = VehicleService.doReservationVhToModeChange(pre_car_out_vh_id);

                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                     Data: $"ProcessCarOutScenario before move car out vehicle, pre_car_out_vh_ohtc_cmd_id:[{pre_car_out_vh_ohtc_cmd_id}]",
                     XID: mtx.DeviceID);
                    if (isSuccess && SCUtility.isEmpty(pre_car_out_vh_ohtc_cmd_id))
                    {
                        //在收到OHT的ID:132-命令結束後或者在變為AutoLocal後此時OHT沒有命令的話則會呼叫此Function來創建一個Transfer command，讓Vh移至移動至System out上
                        if (SCUtility.isMatche(pre_car_out_vh_cur_adr_id, mtx.MTL_SYSTEM_OUT_ADDRESS))
                        {
                            mtx.SetCarOutReady(true);

                            bool create_ok = VehicleService.doAskVhToMaintainsAddress(pre_car_out_vh_id, mtx.MTL_ADDRESS);
                            if (create_ok)
                            {
                                mtx.SetCarOutMoving(true);
                            }
                        }
                        else
                        {

                            VehicleService.doAskVhToSystemOutAddress(pre_car_out_vh_id, mtx.MTL_SYSTEM_OUT_ADDRESS);
                        }
                    }
                    else if (!SCUtility.isEmpty(pre_car_out_vh_ohtc_cmd_id))
                    {
                        //2021.08.27 Hsinyu Chang 被car out的OHT尚有命令，多等1分鐘後再來派MoveToMTL
                        if (!SpinWait.SpinUntil(() => SCUtility.isEmpty(pre_car_out_vh_ohtc_cmd_id), 60000))
                        {
                            isSuccess = false;
                            result = $"ProcessCarOutScenario move car out vehicle fail, pre_car_out_vh_ohtc_cmd_id:[{pre_car_out_vh_ohtc_cmd_id}]";
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                             Data: $"ProcessCarOutScenario move car out vehicle fail, pre_car_out_vh_ohtc_cmd_id:[{pre_car_out_vh_ohtc_cmd_id}]",
                             XID: mtx.DeviceID);
                            return (isSuccess, result);
                        }
                        VehicleService.doAskVhToSystemOutAddress(pre_car_out_vh_id, mtx.MTL_SYSTEM_OUT_ADDRESS);
                    }
                    else
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: $"ProcessCarOutScenario move car out vehicle fail, pre_car_out_vh_ohtc_cmd_id:[{pre_car_out_vh_ohtc_cmd_id}]",
                         XID: mtx.DeviceID);
                    }
                    if (isSuccess)
                    {
                        //carOutVhID = pre_car_out_vh_id;
                        mtx.PreCarOutVhID = pre_car_out_vh_id;
                        Task.Run(() => RegularUpdateRealTimeCarInfo(mtx, preCarOutVh));
                    }
                    else
                    {
                        //mtx.SetCarOutInterlock(false);
                        CarOutFinish(mtx);
                        isSuccess = false;
                        result = $"Reservation vh to mtl fail.";
                    }
                    return (isSuccess, result);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception:");
                    return (false, $"exception happend in processCarOutScenario, ex:[{ex.Message}]");
                }
                finally
                {
                    System.Threading.Interlocked.Exchange(ref syncCarOutPoint, 0);
                }
            }
            else
            {
                return (false, "Already in processCarOutScenario.");
            }
        }

        private void CarOutStart(MaintainLift mtx)
        {
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                     Data: $"Process CarOutStart!",
                     XID: mtx.DeviceID);

            mtx.SetCarOutInterlock(true);
            //if (mtx.DokingMaintainDevice != null &&
            //    mtx.DokingMaintainDevice is MaintainSpace)
            //{
            //    //因為MTL的前方有DockingMTS，因此要觸發CAR IN INTERLOCK，通知將門放下
            //    mtx.DokingMaintainDevice.SetCarInMoving(true);
            //}
        }
        private void CarOutAtMTL(MaintainLift mtx)
        {
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                     Data: $"Process CarOutAtMTL!",
                     XID: mtx.DeviceID);
            mtx.SetCarOutMoving(false);
            mtx.SetCarOutReady(false);

            //if (mtx.DokingMaintainDevice != null &&
            //    mtx.DokingMaintainDevice is MaintainSpace)
            //{
            //    mtx.DokingMaintainDevice.SetCarInMoving(false);
            //}
        }



        private void CarOutFinish(MaintainLift mtx)
        {
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                     Data: $"Process CarOutFinish!",
                     XID: mtx.DeviceID);
            mtx.SetCarOutMoving(false);
            mtx.SetCarOutReady(false);
            mtx.SetCarOutMoveComplete(false);
            mtx.SetCarOutInterlock(false);
            AVEHICLE outCar = scApp.VehicleBLL.cache.getVehicle(mtx.PreCarOutVhID);
            if (outCar != null)
            {
                outCar.IsReadyForCarOut = false;
            }
            //if (mtx.DokingMaintainDevice != null &&
            //    mtx.DokingMaintainDevice is MaintainSpace)
            //{
            //    mtx.DokingMaintainDevice.SetCarInMoving(false);
            //}
        }


        //public (bool isSuccess, string result) processCarOutScenario(MaintainSpace mtx, AVEHICLE preCarOutVh)
        //{
        //    string pre_car_out_vh_id = preCarOutVh.VEHICLE_ID;
        //    string pre_car_out_vh_ohtc_cmd_id = preCarOutVh.OHTC_CMD;
        //    string pre_car_out_vh_cur_adr_id = preCarOutVh.CUR_ADR_ID;
        //    bool isSuccess;
        //    string result = "";
        //    mtx.CancelCarOutRequest = false;
        //    mtx.CarOurSuccess = false;

        //    if (!SpinWait.SpinUntil(() => mtx.CarOutSafetyCheck == true, 30000))
        //    {
        //        isSuccess = false;
        //        result = $"Process car out scenario,but mts:{mtx.DeviceID} status not ready " +
        //        $"{nameof(mtx.CarOutSafetyCheck)}:{mtx.CarOutSafetyCheck}";
        //        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
        //         Data: result,
        //         XID: mtx.DeviceID);
        //        return (false, result);
        //    }

        //    CarOutStart(mtx);
        //    //if (!SpinWait.SpinUntil(() => mtx.MTSBackDoorStatus == MTSDoorStatus.Open, 20000))
        //    //需要判斷MTS的門是否已經開啟
        //    if (!SpinWait.SpinUntil(() => mtx.MTSBackDoorStatus == MTSDoorStatus.Open, MTS_DOOR_OPEN_TIME_OUT_ms))
        //    {
        //        result = $"mts:{mtx.DeviceID}, status not ready  {nameof(mtx.MTSBackDoorStatus)}:{mtx.MTSBackDoorStatus} after interlock on 20 sec, can't excute car out";
        //        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
        //                 Data: result,
        //                 XID: mtx.DeviceID);
        //        CarOutFinish(mtx);
        //        return (false, result);
        //    }
        //    isSuccess = VehicleService.doReservationVhToMaintainsSpace(pre_car_out_vh_id);
        //    if (isSuccess && SCUtility.isEmpty(pre_car_out_vh_ohtc_cmd_id))
        //    {
        //        //在收到OHT的ID:132-命令結束後或者在變為AutoLocal後此時OHT沒有命令的話則會呼叫此Function來創建一個Transfer command，讓Vh移至移動至System out上
        //        isSuccess = VehicleService.doAskVhToSystemOutAddress(pre_car_out_vh_id, mtx.MTS_ADDRESS);
        //    }
        //    if (isSuccess)
        //    {
        //        //carOutVhID = pre_car_out_vh_id;
        //        mtx.PreCarOutVhID = pre_car_out_vh_id;
        //        Task.Run(() => RegularUpdateRealTimeCarInfo(mtx, preCarOutVh));
        //    }
        //    else
        //    {
        //        //mtx.SetCarOutInterlock(false);
        //        CarOutFinish(mtx);
        //        isSuccess = false;
        //        result = $"Reservation vh to mtl fail.";
        //    }
        //    return (isSuccess, result);
        //}

        //private void CarOutStart(MaintainSpace mtx)
        //{
        //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
        //             Data: $"Process CarOutStart!",
        //             XID: mtx.DeviceID);
        //    mtx.SetCarOutInterlock(true);
        //}

        //private void CarOutFinish(MaintainSpace mtx)
        //{
        //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
        //             Data: $"Process CarOutFinish!",
        //             XID: mtx.DeviceID);
        //    mtx.SetCarOutInterlock(false);
        //}

        private void RegularUpdateRealTimeCarInfo(IMaintainDevice mtx, AVEHICLE carOurVh)
        {
            do
            {
                UInt16 car_id = (ushort)carOurVh.Num;
                UInt16 action_mode = 0;
                if (carOurVh.ACT_STATUS == ProtocolFormat.OHTMessage.VHActionStatus.Commanding)
                {
                    //if (!SCUtility.isEmpty(carOurVh.MCS_CMD))
                    //{
                    //    action_mode = CAR_ACTION_MODE_ACTION_FOR_MCS_COMMAND;
                    //}
                    //else
                    //{
                        action_mode = CAR_ACTION_MODE_ACTION;
                    //}
                }
                else
                {
                    action_mode = CAR_ACTION_MODE_NO_ACTION;
                }
                UInt16 cst_exist = (ushort)(carOurVh.HAS_CST ? 1 : 0);
                UInt16 current_section_id = 0;
                UInt16.TryParse(carOurVh.CUR_SEC_ID, out current_section_id);
                UInt16 current_address_id = 0;
                UInt16.TryParse(carOurVh.CUR_ADR_ID, out current_address_id);
                UInt32 buffer_distance = 0;
                UInt16 speed = (ushort)carOurVh.Speed;

                mtx.CurrentPreCarOurID = car_id;
                mtx.CurrentPreCarOurActionMode = action_mode;
                mtx.CurrentPreCarOurCSTExist = cst_exist;
                mtx.CurrentPreCarOurSectionID = current_section_id;
                mtx.CurrentPreCarOurAddressID = current_address_id;
                mtx.CurrentPreCarOurDistance = buffer_distance;
                mtx.CurrentPreCarOurSpeed = speed;

                mtx.setCarRealTimeInfo(car_id, action_mode, cst_exist, current_section_id, current_address_id, buffer_distance, speed);

                //如果在移動過程中，MTx突然變成手動模式的話，則要將原本在移動的車子取消命令
                //if (mtx.MTxMode == MTxMode.Manual || !mtx.MTLCarOutSafetyCheck)
                //{
                //    carOutRequestCancle(mtx, true);
                //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                //             Data: $"Device:{mtx.DeviceID} mtx mode suddenly turned mode:{mtx.MTxMode} or car out safety check change:{mtx.MTLCarOutSafetyCheck}, " +
                //             $"so urgent cancel vh:{mtx.PreCarOutVhID} of command.",
                //             XID: mtx.DeviceID);
                //    break;
                //}

                if (mtx.MTxMode == MTxMode.Manual)
                {
                    carOutRequestCancel(mtx, true);
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                             Data: $"Device:{mtx.DeviceID} mtx mode suddenly turned mode:{mtx.MTxMode}, " +
                             $"so urgent cancel vh:{mtx.PreCarOutVhID} of command.",
                             XID: mtx.DeviceID);
                    break;
                }

                SpinWait.SpinUntil(() => false, 200);
            } while (!mtx.CancelCarOutRequest && !mtx.CarOurSuccess);

            //mtx.setCarRealTimeInfo(0, 0, 0, 0, 0, 0, 0);
        }

        public void carOutAtMTLComplete(IMaintainDevice mtx)
        {
            //mtx.CarOurSuccess = true;
            ////carOutVhID = "";
            //mtx.PreCarOutVhID = "";
            //mtx.SetCarOutInterlock(false);
            if (mtx is MaintainLift)
            {
                (mtx as MaintainLift).SetCarOutMoveComplete(true);

                CarOutAtMTL(mtx as MaintainLift);
            }
            //else if (mtx is MaintainSpace)
            //{
            //    CarOutFinish(mtx as MaintainSpace);
            //}
        }


        public void carOutAllComplete(IMaintainDevice mtx)
        {
            mtx.CarOurSuccess = false;
            //carOutVhID = "";
            mtx.PreCarOutVhID = "";
            //mtx.SetCarOutInterlock(false);
            if (mtx is MaintainLift)
            {

                CarOutFinish(mtx as MaintainLift);
            }
            //else if (mtx is MaintainSpace)
            //{
            //    CarOutFinish(mtx as MaintainSpace);
            //}
        }
        public void carOutCancelComplete(IMaintainDevice mtx)
        {
            mtx.CarOurSuccess = true;
            mtx.PreCarOutVhID = "";
            if (mtx is MaintainLift)
            {

                CarOutFinish(mtx as MaintainLift);
            }
        }

        public (bool isSuccess, string result) AutoCarOut(IMaintainDevice maintainDevice, AVEHICLE car_out_vh)
        {
            var r = default((bool isSuccess, string result));
            try
            {
                if (maintainDevice is sc.Data.VO.MaintainLift)
                {

                    r = checkVhAndMTxCarOutStatus(maintainDevice, null, car_out_vh);

                    if (r.isSuccess)
                    {
                        r = CarOurRequest(maintainDevice, car_out_vh);
                    }
                    //if (!SpinWait.SpinUntil(() => maintainDevice.CarOutSafetyCheck == true &&
                    ////maintainDevice.CarOutActionTypeSystemOutToMTL == true && 
                    //( dockingMTS==null||dockingMTS.CarOutSafetyCheck == true), 60000))
                    //{
                    //    r.isSuccess = false;
                    //    string  result = $"Process car out scenario,but mtl:{maintainDevice.DeviceID} status not ready " +
                    //    $"{nameof(maintainDevice.CarOutSafetyCheck)}:{maintainDevice.CarOutSafetyCheck}";
                    //    MessageBox.Show(result);
                    //}
                    if (r.isSuccess)
                    {
                        r = processCarOutScenario(maintainDevice as sc.Data.VO.MaintainLift, car_out_vh);
                    }
                }
                //else if (maintainDevice is sc.Data.VO.MaintainSpace)
                //{
                //    r = bcApp.SCApplication.MTLService.checkVhAndMTxCarOutStatus(maintainDevice, null, pre_car_out_vh);
                //    if (r.isSuccess)
                //    {
                //        r = bcApp.SCApplication.MTLService.CarOurRequest(maintainDevice, pre_car_out_vh);
                //    }
                //    //if (!SpinWait.SpinUntil(() => maintainDevice.CarOutSafetyCheck == true, 30000))
                //    //{
                //    //    r.isSuccess = false;
                //    //    string result = $"Process car out scenario,but mtl:{maintainDevice.DeviceID} status not ready " +
                //    //    $"{nameof(maintainDevice.CarOutSafetyCheck)}:{maintainDevice.CarOutSafetyCheck}";
                //    //    MessageBox.Show(result);
                //    //}

                //    if (r.isSuccess)
                //    {
                //        r = bcApp.SCApplication.MTLService.processCarOutScenario(maintainDevice as sc.Data.VO.MaintainSpace, pre_car_out_vh);
                //    }
                //}
            }
            catch (Exception ex)
            {
                r = (false, ex.ToString());
            }
            finally
            {
            }
            return r;
        }

        public void carOutRequestCancel(IMaintainDevice mtx)
        {
            //carOutRequestCancle(mtx, false);
            if (mtx is MaintainLift)
            {
                notifyCarOutRequestCancel(mtx as MaintainLift);  //2021/08/10 Hsinyu Chang
            }
        }
        public void carOutRequestCancel(IMaintainDevice mtx, bool isForceFinish)
        {
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                     Data: $"Process car out cancel request. mtx:{mtx.DeviceID}, pre car out vh:{mtx.PreCarOutVhID}, is force finish:{isForceFinish}",
                     XID: mtx.DeviceID);
            //將原本的在等待Carout的Vh改回AutoRemote
            mtx.CancelCarOutRequest = true;
            //if (!SCUtility.isEmpty(carOutVhID))
            //{
            //    VehicleService.doRecoverModeStatusToAutoRemote(carOutVhID);
            //}
            //carOutVhID = "";
            if (!SCUtility.isEmpty(mtx.PreCarOutVhID))
            {
                VehicleService.doRecoverModeStatusToAutoRemote(mtx.PreCarOutVhID);
                AVEHICLE pre_car_out_vh = vehicleBLL.cache.getVehicle(mtx.PreCarOutVhID);
                if (!SCUtility.isEmpty(pre_car_out_vh?.CMD_ID))
                {
                    ACMD cmd = scApp.CMDBLL.GetCMD_OHTCByID(pre_car_out_vh.CMD_ID);
                    if (cmd != null)
                    {
                        if (cmd.CMD_TYPE == E_CMD_TYPE.Move_MTL || cmd.CMD_TYPE == E_CMD_TYPE.SystemOut ||
                            cmd.CMD_TYPE == E_CMD_TYPE.SystemIn || cmd.CMD_TYPE == E_CMD_TYPE.MTLHome)
                        {
                            //如果是強制被取消(Safety check突然關閉)的時候，要先下一次暫停給車子
                            if (isForceFinish)
                            {
                                //VehicleService.PauseRequest
                                //    (pre_car_out_vh.VEHICLE_ID, PauseEvent.Pause, SCAppConstants.OHxCPauseType.Normal);
                            }
                            //VehicleService.abort
                            VehicleService.Send.Cancel
                                (pre_car_out_vh.VEHICLE_ID, pre_car_out_vh.CMD_ID, ProtocolFormat.OHTMessage.CMDCancelType.CmdCancel);
                        }
                    }
                    else
                    {
                        //還沒下給車子則直接切掉訊號
                        mtx.PreCarOutVhID = "";
                        if (mtx is MaintainLift)
                        {
                            CarOutFinish(mtx as MaintainLift);
                        }
                        //如果是強制被取消(Safety check突然關閉)的時候，要先下一次暫停給車子
                        if (isForceFinish)
                            {
                                //VehicleService.PauseRequest
                                //    (pre_car_out_vh.VEHICLE_ID, PauseEvent.Pause, SCAppConstants.OHxCPauseType.Normal);
                            }
                            //VehicleService.doAbortCommand
                            //    (pre_car_out_vh, pre_car_out_vh.OHTC_CMD, ProtocolFormat.OHTMessage.CMDCancelType.CmdCancel);

                    }

                }
                else
                {
                    mtx.PreCarOutVhID = "";
                    if (mtx is MaintainLift)
                    {
                        CarOutFinish(mtx as MaintainLift);
                    }
                }
                //如果OHT已經在MTS/MTL的Segment上時，
                //就不能將他的對應訊號關閉
                //if (SCUtility.isMatche(mtx.DeviceSegment, pre_car_out_vh.CUR_SEG_ID))
                //{
                //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                //             Data: $"Process car out cancel request. mtx:{mtx.DeviceID}, pre car out vh:{mtx.PreCarOutVhID}, is force finish:{isForceFinish}," +
                //                   $"But vh current section is in MTL segment:{mtx.DeviceSegment} .can't trun off car out single ",
                //             XID: mtx.DeviceID);
                //    return;
                //}
            }
            //mtx.PreCarOutVhID = "";
            //if (mtx is MaintainLift)
            //{
            //    CarOutFinish(mtx as MaintainLift);
            //}
            //mtx.SetCarOutInterlock(false);

        }
        private long syncCarInPoint = 0;
        public bool isCarInProcessing()
        {
            return syncCarInPoint != 0;
        }

        public void carInSafetyAndVehicleStatusCheck(MaintainLift mtl)
        {
            if (System.Threading.Interlocked.Exchange(ref syncCarInPoint, 1) == 0)
            {
                try
                {
                    bool isSuccess;
                    string result = "OK";

                    //切換為AutoLocal狀態
                    //20210827 Hsinyu Chang只有在online remote時需要切換，online local或offline都可以直接放行
                    if (scApp.getEQObjCacheManager().getLine().Host_Control_State == SCAppConstants.LineHostControlState.HostControlState.On_Line_Remote)
                    {
                        scApp.LineService.OnlineLocalWithHostMTL();
                    }
                    //結束MCS所有命令
                    //沒有車輛在MTL區域
                    if (!SpinWait.SpinUntil(() => readyToCarInStart() == true, 600000))
                    {
                        isSuccess = false;
                        result = $"Process car in scenario,but ohtc status is not ready " +
                        $",please check host state is autolocal (or host offline) and all mcs cmd is over and all vehicle is not in MTL area.";
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                                 Data: result,
                                 XID: mtl.DeviceID);
                        return;
                    }

                    MTLValueDefMapAction mtlMapAction = mtl.getMapActionByIdentityKey(typeof(MTLValueDefMapAction).Name) as MTLValueDefMapAction;
                    if (mtlMapAction != null)
                    {
                        scApp.VehicleService.checkThenSetVehiclePauseByMTLStatus("Car In");
                    }

                    mtl.SetCarInInterlock(true);


                    if (!SpinWait.SpinUntil(() => CarInVhReadyCheck() == true, 600000))
                    {
                        isSuccess = false;
                        result = $"Process car in scenario,but car in oht is not ready.";
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                                 Data: result,
                                 XID: mtl.DeviceID);
                        return;
                    }
                    if (!mtl.MTLCarInInterlock)
                    {
                        mtl.SetCarInMoveComplete(false);
                        mtl.SetCarInInterlock(false);
                        return;
                    }
                    //if (!mtl.MTLCarInSafetyCheck || mtl.MTxMode != ProtocolFormat.OHTMessage.MTxMode.Auto || mtl.MTLLocation != MTLLocation.Upper)
                    //{
                    //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                    //             Data: $"Device:{mtl.DeviceID} car in safety check in on, but mts mode:{mtl.MTxMode} or Location:{mtl.MTLLocation}, can't excute car in.",
                    //             XID: mtl.DeviceID);
                    //    return;
                    //}


                    CarInStart(mtl);

                    //在收到MTL的 Car in safety check後，就可以叫Vh移動至Car in 的buffer區(MTL Home)
                    //不過要先判斷vh是否已經在Auto模式下如果是則先將它變成AutoLocal的模式

                    //AVEHICLE car_in_vh = vehicleBLL.cache.getVhByAddressID(mtl.MTL_ADDRESS);
                    AVEHICLE car_in_vh = vehicleBLL.cache.getVhOnAddress(mtl.MTL_ADDRESS);
                    if (car_in_vh != null && car_in_vh.isTcpIpConnect)
                    {
                        VehicleService.doReservationVhToModeChange(car_in_vh.VEHICLE_ID);
                        if (car_in_vh.MODE_STATUS == ProtocolFormat.OHTMessage.VHModeStatus.Manual)
                        {
                            VehicleService.Send.ModeChange(car_in_vh.VEHICLE_ID, OperatingVHMode.OperatingAuto);
                            //VehicleService.ModeChangeRequest(car_in_vh.VEHICLE_ID, OperatingVHMode.OperatingAuto);
                            if (SpinWait.SpinUntil(() => (car_in_vh.MODE_STATUS == VHModeStatus.AutoMtl && mtl.MTLCarInSafetyCheck), 10000))
                            {
                                //mtl.SetCarInMoving(true);
                                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                                    Data: $"vh:{car_in_vh.VEHICLE_ID} request car in, OHTC Ready to create system in command.",
                                     XID: mtl.DeviceID,
                                    VehicleID: car_in_vh.VEHICLE_ID);
                                bool create_result = VehicleService.doAskVhToCarInBufferAddress(car_in_vh.VEHICLE_ID, mtl.MTL_CAR_IN_BUFFER_ADDRESS);
                                if (create_result)
                                {
                                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                                    Data: $"vh:{car_in_vh.VEHICLE_ID} request car in, OHTC create system in command  successful.",
                                     XID: mtl.DeviceID,
                                    VehicleID: car_in_vh.VEHICLE_ID);
                                }
                                else
                                {
                                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                                        Data: $"vh:{car_in_vh.VEHICLE_ID} request car in, OHTC create system in command failes.",
                                         XID: mtl.DeviceID,
                                        VehicleID: car_in_vh.VEHICLE_ID);
                                    CarInFinish(mtl);
                                }
                            }
                            else
                            {
                                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                                         Data: $"vh:{car_in_vh.VEHICLE_ID} request car in, but can't change to auto mode.",
                                         XID: mtl.DeviceID,
                                         VehicleID: car_in_vh.VEHICLE_ID);
                                CarInFinish(mtl);
                            }
                        }
                        else if (car_in_vh.MODE_STATUS == ProtocolFormat.OHTMessage.VHModeStatus.AutoMtl && mtl.MTLCarInSafetyCheck)
                        {
                            //mtl.SetCarInMoving(true);
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                                Data: $"vh:{car_in_vh.VEHICLE_ID} request car in, OHTC Ready to create system in command.",
                                 XID: mtl.DeviceID,
                                VehicleID: car_in_vh.VEHICLE_ID);
                            bool create_result = VehicleService.doAskVhToCarInBufferAddress(car_in_vh.VEHICLE_ID, mtl.MTL_CAR_IN_BUFFER_ADDRESS);
                            if (create_result)
                            {
                                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                                Data: $"vh:{car_in_vh.VEHICLE_ID} request car in, OHTC create system in command  successful.",
                                 XID: mtl.DeviceID,
                                VehicleID: car_in_vh.VEHICLE_ID);
                            }
                            else
                            {
                                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                                    Data: $"vh:{car_in_vh.VEHICLE_ID} request car in, OHTC create system in command failes.",
                                     XID: mtl.DeviceID,
                                    VehicleID: car_in_vh.VEHICLE_ID);
                                CarInFinish(mtl);
                            }
                        }
                        else
                        {
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                                     Data: $"vh:{car_in_vh.VEHICLE_ID} request car in, but status is incorrect current status:{car_in_vh.MODE_STATUS}.",
                                     XID: mtl.DeviceID,
                                     VehicleID: car_in_vh.VEHICLE_ID);
                        }
                    }
                    else
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                             Data: $"Request car in, but no vehicle at MTL or vehicle is not connected.",
                            XID: mtl.DeviceID);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception:");
                }
                finally
                {
                    System.Threading.Interlocked.Exchange(ref syncCarInPoint, 0);
                }
            }
        }

        private void CarInStart(MaintainLift mtl)
        {
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                     Data: $"Process CarInStart!",
                     XID: mtl.DeviceID);
            mtl.SetCarInMoving(true);
        }
        private void CarInFinish(MaintainLift mtl)
        {
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                     Data: $"Process CarInFinish!",
                     XID: mtl.DeviceID);
            mtl.SetCarInMoving(false);
            mtl.SetCarInMoveComplete(true);
        }

        public (bool isSuccess, string result) checkVhAndMTxCarInStatus(IMaintainDevice mtx, IMaintainDevice dockingMtx, AVEHICLE car_in_vh)
        {
            bool isSuccess = true;
            string result = "";

            string vh_id = car_in_vh.VEHICLE_ID;
            //1.要判斷目前車子的狀態
            //要判斷目前MTS是否有在處理CAR IN流程
            if (isSuccess && mtx.OHTCCarOutMoving)
            {
                isSuccess = false;
                result = $"MTx:{mtx.DeviceID} Current CarInMoving:{mtx.OHTCCarOutMoving}, can't excute car in requset.";
            }
            //if (isSuccess && !car_in_vh.isTcpIpConnect)
            //{
            //    isSuccess = false;
            //    result = $"vh id:{vh_id}, not connection.";
            //}

            //if (isSuccess && SCUtility.isEmpty(car_in_vh.CUR_SEC_ID))
            //{
            //    isSuccess = false;
            //    result = $"vh id:{vh_id}, current section is empty.";
            //}
            //if (isSuccess && !SCUtility.isMatche(car_in_vh.CUR_ADR_ID, mtx.DeviceAddress))
            //{
            //    isSuccess = false;
            //    result = $"vh id:{vh_id}, current address:{car_in_vh.CUR_ADR_ID} not match mtx device address:{mtx.DeviceAddress}.";
            //}


            //2.要判斷MTL的 Safety check是否有On且是否為Auto Mode
            if (isSuccess && mtx.MTxMode != ProtocolFormat.OHTMessage.MTxMode.Auto)
            {
                isSuccess = false;
                result = $"MTx:{mtx.DeviceID} Current Mode:{mtx.MTxMode}, can't excute car in requset.";
            }

            if (isSuccess && isCarInProcessing())
            {
                isSuccess = false;
                result = $"MTx:{mtx.DeviceID} Current Car In is processing, can't excute car in requset.";
            }


            return (isSuccess, result);
        }

        const int MTS_DOOR_OPEN_TIME_OUT_ms = 20000;
        //public void processCarInScenario(MaintainSpace mts)
        //{
        //    CarInStart(mts);
        //    //在收到MTL的 Car in safety check後，就可以叫Vh移動至Car in 的buffer區(MTL Home)
        //    //不過要先判斷vh是否已經在Auto模式下如果是則先將它變成AutoLocal的模式
        //    if (!SpinWait.SpinUntil(() => mts.CarInSafetyCheck && mts.MTxMode == MTxMode.Auto, 10000))
        //    {
        //        LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
        //                 Data: $"mts:{mts.DeviceID}, status not ready CarInSafetyCheck:{mts.CarInSafetyCheck},Mode:{mts.MTxMode} ,can't excute car in",
        //                 XID: mts.DeviceID);
        //        CarInFinish(mts);
        //        return;
        //    }

        //    //在車子要Car In的時候，要判斷MTS的前門是否已經開啟
        //    if (!SpinWait.SpinUntil(() => mts.MTSFrontDoorStatus == MTSDoorStatus.Open, MTS_DOOR_OPEN_TIME_OUT_ms))
        //    {
        //        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
        //                 Data: $"mts:{mts.DeviceID}, status not ready {nameof(mts.MTSFrontDoorStatus)}:{ mts.MTSFrontDoorStatus} ,can't excute car in",
        //                 XID: mts.DeviceID);
        //        CarInFinish(mts);
        //        return;
        //    }

        //    AVEHICLE car_in_vh = vehicleBLL.cache.getVhByAddressID(mts.MTS_ADDRESS);
        //    //if (car_in_vh != null && car_in_vh.isTcpIpConnect)
        //    if (car_in_vh != null)
        //    {
        //        if (car_in_vh.isTcpIpConnect)
        //        {
        //            if (car_in_vh.MODE_STATUS == ProtocolFormat.OHTMessage.VHModeStatus.Manual)
        //            {
        //                VehicleService.ModeChangeRequest(car_in_vh.VEHICLE_ID, ProtocolFormat.OHTMessage.OperatingVHMode.OperatingAuto);

        //                if (SpinWait.SpinUntil(() => car_in_vh.MODE_STATUS == VHModeStatus.AutoMts, 10000))
        //                {
        //                    //mts.SetCarInMoving(true);
        //                    VehicleService.doAskVhToSystemInAddress(car_in_vh.VEHICLE_ID, mts.MTS_SYSTEM_IN_ADDRESS);
        //                }
        //                else
        //                {
        //                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
        //                             Data: $"Process car in scenario:{mts.DeviceID} fail. ask vh change to auto mode time out",
        //                             XID: mts.DeviceID,
        //                             VehicleID: car_in_vh.VEHICLE_ID);
        //                    CarInFinish(mts);
        //                }
        //            }
        //            else if (car_in_vh.MODE_STATUS == ProtocolFormat.OHTMessage.VHModeStatus.AutoMts)
        //            {
        //                //mts.SetCarInMoving(true);
        //                VehicleService.doAskVhToSystemInAddress(car_in_vh.VEHICLE_ID, mts.MTS_SYSTEM_IN_ADDRESS);
        //            }
        //        }
        //        else
        //        {
        //            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
        //                     Data: $"Process car in scenario fail, mts:{mts.DeviceID}. because on mts of vh:{car_in_vh.VEHICLE_ID} is disconnect ",
        //                     XID: mts.DeviceID,
        //                     VehicleID: car_in_vh.VEHICLE_ID);
        //            //mts.SetCarInMoving(false);
        //            CarInFinish(mts);
        //        }
        //    }
        //    else
        //    {
        //        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
        //                 Data: $"Process car in scenario fail, mts:{mts.DeviceID}. because no vh in mts",
        //                 XID: mts.DeviceID);
        //        //mts.SetCarInMoving(false);
        //        CarInFinish(mts);
        //    }
        //}

        //private void CarInStart(MaintainSpace mts)
        //{
        //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
        //             Data: $"Process CarInStart!",
        //             XID: mts.DeviceID);
        //    mts.SetCarInMoving(true);
        //    if (mts.DokingMaintainDevice != null)
        //    {
        //        if (mts.DokingMaintainDevice is MaintainLift)
        //        {
        //            mts.DokingMaintainDevice.SetCarOutInterlock(true);
        //        }
        //    }
        //}
        //private void CarInFinish(MaintainSpace mts)
        //{
        //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
        //             Data: $"Process CarInFinish!",
        //             XID: mts.DeviceID);
        //    mts.SetCarInMoving(false);
        //    if (mts.DokingMaintainDevice != null)
        //    {
        //        if (mts.DokingMaintainDevice is MaintainLift)
        //        {
        //            mts.DokingMaintainDevice.SetCarOutInterlock(false);
        //        }
        //    }
        //}

        public void carInComplete(IMaintainDevice mtx, string vhID)
        {
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLService), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                     Data: $"Process carInComplete!",
                     XID: mtx.DeviceID,
                     VehicleID: vhID);
            VehicleService.doRecoverModeStatusToAutoRemote(vhID);
            //mtx.SetCarInMoving(false);
            if (mtx is MaintainLift)
            {
                CarInFinish(mtx as MaintainLift);
            }
            if (scApp.getEQObjCacheManager().getLine().Host_Control_State != SCAppConstants.LineHostControlState.HostControlState.On_Line_Remote)
            {
                scApp.LineService.OnlineRemoteWithHost();
            }
            //else if (mtx is MaintainSpace)
            //{
            //    CarInFinish(mtx as MaintainSpace);
            //}
            //如果是MTS機台，因為在準備通過MTL時，會將CAR OUT訊號 TURN ON，因此在結束的時候要將它復歸

            //List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
            //reportBLL.newReportVehicleInstalled(vhID, reportqueues);
        }


        //public void PublishMTSInfo(object sender, PropertyChangedEventArgs e)
        //{
        //    try
        //    {
        //        MaintainSpace eqpt = sender as MaintainSpace;
        //        if (sender == null) return;
        //        byte[] line_serialize = BLL.LineBLL.Convert2GPB_MTLMTSInfo(eqpt);
        //        scApp.getNatsManager().PublishAsync
        //            (SCAppConstants.NATS_SUBJECT_MTLMTS, line_serialize);
        //        //TODO 要改用GPP傳送
        //        //var line_Serialize = ZeroFormatter.ZeroFormatterSerializer.Serialize(line);
        //        //scApp.getNatsManager().PublishAsync
        //        //    (string.Format(SCAppConstants.NATS_SUBJECT_LINE_INFO), line_Serialize);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "Exception:");
        //    }
        //}

        public void PublishMTLInfo(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                MaintainLift eqpt = sender as MaintainLift;
                if (sender == null) return;
                byte[] line_serialize = BLL.LineBLL.Convert2GPB_MTLMTSInfo(eqpt);
                scApp.getNatsManager().PublishAsync
                    (SCAppConstants.NATS_SUBJECT_MTLMTS, line_serialize);
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

        private void notifyCarOutRequestCancel(MaintainLift mtl)
        {
            MTLValueDefMapAction mtlMapAction = mtl.getMapActionByIdentityKey(typeof(MTLValueDefMapAction).Name) as MTLValueDefMapAction;
            if (mtlMapAction != null)
            {
                (bool isSendSuccess, UInt16 returnCode) = mtlMapAction.OHxC_CarOutRequestCancelNotify();
                if (isSendSuccess && returnCode == 1)
                {
                    carOutRequestCancel(mtl, false);
                }
            }
        }
    }
}
