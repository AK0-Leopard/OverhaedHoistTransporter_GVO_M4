//*********************************************************************************
//      MTLValueDefMapAction.cs
//*********************************************************************************
// File Name: MTLValueDefMapAction.cs
// Description: 
//
//(c) Copyright 2018, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
//**********************************************************************************
using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.bcf.Common.MPLC;
using com.mirle.ibg3k0.bcf.Controller;
using com.mirle.ibg3k0.bcf.Data.ValueDefMapAction;
using com.mirle.ibg3k0.bcf.Data.VO;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.PLC_Functions;
using com.mirle.ibg3k0.sc.Data.VO;
using KingAOP;
using NLog;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.ValueDefMapAction
{
    public class MTLValueDefMapAction : MTxValueDefMapActionBase
    {
        Logger logger = NLog.LogManager.GetCurrentClassLogger();
        Logger MTLPauserHandlerInfoLogger = LogManager.GetLogger("MTLPauserHandlerInfo");
        MaintainLift MTL { get { return this.eqpt as MaintainLift; } }
                string Handler ="MTL_HANDLER";
        public MTLValueDefMapAction()
            : base()
        {

        }

        private void registerEvent()
        {
            MTL.addEventHandler(Handler, BCFUtility.getPropertyName(() => MTL.Is_Eq_Alive),
            (s1, e1) => scApp.VehicleService.checkThenSetVehiclePauseByMTLStatus("Is_Eq_Alive"));
            //MTL.addEventHandler(Handler, BCFUtility.getPropertyName(() => MTL.MTxMode),
            //(s1, e1) => checkThenSetVehiclePauseByMTLStatus("MTxMode")); //20210602 客戶要求不要看mode狀態決定是否要暫停全線。
            MTL.addEventHandler(Handler, BCFUtility.getPropertyName(() => MTL.StopSignal),
            (s1, e1) => scApp.VehicleService.checkThenSetVehiclePauseByMTLStatus("StopSignal"));
            MTL.addEventHandler(Handler, BCFUtility.getPropertyName(() => MTL.MTLLocation),
            (s1, e1) => scApp.VehicleService.checkThenSetVehiclePauseByMTLStatus("MTLLocation"));

 //           commonInfo.addEventHandler(nameof(LineService), BCFUtility.getPropertyName(() => commonInfo.MPCTipMsgList),
 //PublishTipMessageInfo);
        }
        public override void doShareMemoryInit(BCFAppConstants.RUN_LEVEL runLevel)
        {
            try
            {
                switch (runLevel)
                {
                    case BCFAppConstants.RUN_LEVEL.ZERO:
                        eqpt.Is_Eq_Alive = true;
                        MTL_Alive(null, null);
                        CarOutSafetyChcek(null, null);
                        CarOutInterlock(null, null);
                        CarInSafetyChcek(null, null);
                        CarInInterlock(null, null);
                        MTL_LFTStatus(null, null);
                        MTL_Current_ID(null, null);

                        registerEvent();
                        scApp.VehicleService.checkThenSetVehiclePauseByMTLStatus("Init");
                        //如果該機台為MTS1或者是MTL時，需要將兩台設定為互相doking的機台
                        //if (eqpt is MaintainSpace &&
                        //    SCUtility.isMatche((eqpt as MaintainSpace).EQPT_ID, "MTS"))
                        //{
                        //    (eqpt as MaintainSpace).DokingMaintainDevice = scApp.EquipmentBLL.cache.GetMaintainLift();
                        //}
                        //else if (eqpt is MaintainLift &&
                        //    SCUtility.isMatche((eqpt as MaintainLift).EQPT_ID, "MTL"))
                        //{
                        //    (eqpt as MaintainLift).DokingMaintainDevice = scApp.EquipmentBLL.cache.GetDockingMTLOfMaintainSpace();
                        //}
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

        object dateTimeSyneObj = new object();
        uint dateTimeIndex = 0;
        public override void DateTimeSyncCommand(DateTime dateTime)
        {

            OHxCToMtl_DateTimeSync send_function =
               scApp.getFunBaseObj<OHxCToMtl_DateTimeSync>(MTL.EQPT_ID) as OHxCToMtl_DateTimeSync;
            try
            {
                lock (dateTimeSyneObj)
                {
                    //1.準備要發送的資料
                    send_function.Year = Convert.ToUInt16(dateTime.Year.ToString(), 10);
                    send_function.Month = Convert.ToUInt16(dateTime.Month.ToString(), 10);
                    send_function.Day = Convert.ToUInt16(dateTime.Day.ToString(), 10);
                    send_function.Hour = Convert.ToUInt16(dateTime.Hour.ToString(), 10);
                    send_function.Min = Convert.ToUInt16(dateTime.Minute.ToString(), 10);
                    send_function.Sec = Convert.ToUInt16(dateTime.Second.ToString(), 10);
                    if (dateTimeIndex >= 9999)
                        dateTimeIndex = 0;
                    send_function.Index = ++dateTimeIndex;
                    //2.紀錄發送資料的Log
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                             Data: send_function.ToString(),
                             XID: MTL.EQPT_ID);
                    //3.發送訊息
                    send_function.Write(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<OHxCToMtl_DateTimeSync>(send_function);
            }
        }
        uint message_index = 0;
        public override void OHxCMessageDownload(string msg)
        {
            OHxCToMtl_MessageDownload send_function =
                scApp.getFunBaseObj<OHxCToMtl_MessageDownload>(MTL.EQPT_ID) as OHxCToMtl_MessageDownload;
            try
            {
                //1.建立各個Function物件
                send_function.Message = msg;
                if (message_index > 9999)
                { message_index = 0; }
                send_function.Index = ++message_index;
                //2.紀錄發送資料的Log
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: send_function.ToString(),
                         XID: MTL.EQPT_ID);
                //3.發送訊息
                send_function.Write(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<OHxCToMtl_MessageDownload>(send_function);
            }
        }
        UInt16 car_realtime_info = 0;
        public override void CarRealtimeInfo(UInt16 car_id, UInt16 action_mode, UInt16 cst_exist, UInt16 current_section_id, UInt32 current_address_id,
                                            UInt32 buffer_distance, UInt16 speed)
        {
            OHxCToMtl_CarRealtimeInfo send_function =
                scApp.getFunBaseObj<OHxCToMtl_CarRealtimeInfo>(MTL.EQPT_ID) as OHxCToMtl_CarRealtimeInfo;
            try
            {
                //1.建立各個Function物件
                send_function.CarID = car_id;
                send_function.ActionMode = action_mode;
                send_function.CSTExist = cst_exist;
                send_function.CurrentSectionID = current_section_id;
                send_function.CurrentAddressID = current_address_id;
                send_function.BufferDistance = buffer_distance;
                send_function.Speed = speed;
                if (car_realtime_info > 9999)
                { car_realtime_info = 0; }
                send_function.Index = ++car_realtime_info;
                //2.紀錄發送資料的Log
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: send_function.ToString(),
                         XID: MTL.EQPT_ID);
                //3.發送訊息
                send_function.Write(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<OHxCToMtl_CarRealtimeInfo>(send_function);
            }
        }

        public override (bool isSendSuccess, UInt16 returnCode) OHxC_CarOutNotify(UInt16 car_id, UInt16 action_type)
        {
            bool isSendSuccess = false;
            var send_function =
                scApp.getFunBaseObj<OHxCToMtl_CarOutNotify>(MTL.EQPT_ID) as OHxCToMtl_CarOutNotify;
            var receive_function =
                scApp.getFunBaseObj<MtlToOHxC_ReplyCarOutNotify>(MTL.EQPT_ID) as MtlToOHxC_ReplyCarOutNotify;
            try
            {
                //1.準備要發送的資料
                send_function.CarID = car_id;
                //send_function.ActionType = action_type;
                ValueRead vr_reply = receive_function.getValueReadHandshake
                    (bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);
                //2.紀錄發送資料的Log
                send_function.Handshake = 1;
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: send_function.ToString(),
                         XID: MTL.EQPT_ID);
                //3.等待回復
                TrxMPLC.ReturnCode returnCode =
                    send_function.SendRecv(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID, vr_reply);
                //4.取得回復的結果
                if (returnCode == TrxMPLC.ReturnCode.Normal)
                {
                    receive_function.Read(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                             Data: receive_function.ToString(),
                             XID: MTL.EQPT_ID);
                    isSendSuccess = true;
                }
                send_function.Handshake = 0;
                send_function.resetHandshake(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: send_function.ToString(),
                         XID: MTL.EQPT_ID);
                return (isSendSuccess, receive_function.ReturnCode);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<OHxCToMtl_CarOutNotify>(send_function);
                scApp.putFunBaseObj<MtlToOHxC_ReplyCarOutNotify>(receive_function);
            }
            return (isSendSuccess, 0);
        }
        public override void OHxC2MTL_CarOutInterface(bool carOutInterlock, bool carOutReady, bool carMoving, bool carMoveComplete)
        {
            try
            {
                ValueWrite vm_carOutInterlock = bcfApp.getWriteValueEvent(MTL.EqptObjectCate, MTL.EQPT_ID, "OHXC_TO_MTL_CAR_OUT_INTERLOCK");
                ValueWrite vm_carOutReady = bcfApp.getWriteValueEvent(MTL.EqptObjectCate, MTL.EQPT_ID, "OHXC_TO_MTL_CAR_OUT_READY");
                ValueWrite vm_carMoving = bcfApp.getWriteValueEvent(MTL.EqptObjectCate, MTL.EQPT_ID, "OHXC_TO_MTL_CAR_OUT_MOVING");
                ValueWrite vm_carMoveCmp = bcfApp.getWriteValueEvent(MTL.EqptObjectCate, MTL.EQPT_ID, "OHXC_TO_MTL_CAR_OUT_MOVE_COMPLETE");






                string setValue = carOutInterlock ? "1" : "0";
                vm_carOutInterlock.setWriteValue(carOutInterlock ? "1" : "0");
                vm_carOutReady.setWriteValue(carOutReady ? "1" : "0");
                vm_carMoving.setWriteValue(carMoving ? "1" : "0");
                vm_carMoveCmp.setWriteValue(carMoveComplete ? "1" : "0");
                bool result = ISMControl.writeDeviceBlock(bcfApp, vm_carOutInterlock);
                ISMControl.writeDeviceBlock(bcfApp, vm_carOutReady);
                ISMControl.writeDeviceBlock(bcfApp, vm_carMoving);
                ISMControl.writeDeviceBlock(bcfApp, vm_carMoveCmp);
                if (result) eqpt.Interlock = setValue == "1" ? true : false;
                MTL.OHTCCarOutInterlock = carOutInterlock;
                MTL.OHTCCarOutReady = carOutReady;
                MTL.OHTCCarOutMoving = carMoving;
                MTL.OHTCCarOutMoveComplete = carMoveComplete;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
        }

        public override void OHxC2MTL_CarInInterface(bool carMoving, bool car_in_interlock, bool car_in_move_complete)
        {
            try
            {
                ValueWrite vm_carMoving = bcfApp.getWriteValueEvent(MTL.EqptObjectCate, MTL.EQPT_ID, "OHXC_TO_MTL_CAR_IN_MOVING");
                ValueWrite vm_carInInterlock = bcfApp.getWriteValueEvent(MTL.EqptObjectCate, MTL.EQPT_ID, "OHXC_TO_MTL_CAR_IN_INTERLOCK");
                ValueWrite vm_carInMoveComplete = bcfApp.getWriteValueEvent(MTL.EqptObjectCate, MTL.EQPT_ID, "OHXC_TO_MTL_CAR_IN_MOVE_COMPLETE");
                vm_carMoving.setWriteValue(carMoving ? "1" : "0");
                vm_carInInterlock.setWriteValue(car_in_interlock ? "1" : "0");
                vm_carInMoveComplete.setWriteValue(car_in_move_complete ? "1" : "0");
                ISMControl.writeDeviceBlock(bcfApp, vm_carMoving);
                ISMControl.writeDeviceBlock(bcfApp, vm_carInInterlock);
                ISMControl.writeDeviceBlock(bcfApp, vm_carInMoveComplete);


            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
        }




        public override bool setOHxC2MTL_CarOutInterlock(bool carOutInterlock)
        {
            try
            {
                ValueWrite vm_carOutInterlock = bcfApp.getWriteValueEvent(MTL.EqptObjectCate, MTL.EQPT_ID, "OHXC_TO_MTL_CAR_OUT_INTERLOCK");
                string setValue = carOutInterlock ? "1" : "0";
                vm_carOutInterlock.setWriteValue(setValue);
                bool result = ISMControl.writeDeviceBlock(bcfApp, vm_carOutInterlock);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: $"Set Car Out Interlock:{carOutInterlock},result:{result}",
                         XID: MTL.EQPT_ID);
                if (result) eqpt.Interlock = setValue == "1" ? true : false;
                MTL.OHTCCarOutInterlock = carOutInterlock;
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return false;
            }
        }


        public override bool setOHxC2MTL_CarOutReady(bool carOutReady)
        {
            try
            {
                ValueWrite vm_carOutReady = bcfApp.getWriteValueEvent(MTL.EqptObjectCate, MTL.EQPT_ID, "OHXC_TO_MTL_CAR_OUT_READY");
                string setValue = carOutReady ? "1" : "0";
                vm_carOutReady.setWriteValue(setValue);
                bool result = ISMControl.writeDeviceBlock(bcfApp, vm_carOutReady);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: $"Set Car Out Ready:{carOutReady},result:{result}",
                         XID: MTL.EQPT_ID);
                //if (result) MTL.OHTCCarOutReady = setValue == "1" ? true : false;
                MTL.OHTCCarOutReady = carOutReady;
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return false;
            }
        }


        public override bool setOHxC2MTL_CarOutMoving(bool carMoving)
        {
            try
            {
                ValueWrite vm_carMoving = bcfApp.getWriteValueEvent(MTL.EqptObjectCate, MTL.EQPT_ID, "OHXC_TO_MTL_CAR_OUT_MOVING");
                vm_carMoving.setWriteValue(carMoving ? "1" : "0");
                bool result = ISMControl.writeDeviceBlock(bcfApp, vm_carMoving);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: $"Set Car Out Moving:{carMoving},result:{result}",
                         XID: MTL.EQPT_ID);
                MTL.OHTCCarOutMoving = carMoving;
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return false;
            }
            return true;
        }
        public override bool setOHxC2MTL_CarOutMoveComplete(bool carOutMoveComplete)
        {
            try
            {
                ValueWrite vm_carOutMoveComplete = bcfApp.getWriteValueEvent(MTL.EqptObjectCate, MTL.EQPT_ID, "OHXC_TO_MTL_CAR_OUT_MOVE_COMPLETE");
                string setValue = carOutMoveComplete ? "1" : "0";
                vm_carOutMoveComplete.setWriteValue(setValue);
                bool result = ISMControl.writeDeviceBlock(bcfApp, vm_carOutMoveComplete);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: $"Set Car Out Move Complete:{carOutMoveComplete},result:{result}",
                         XID: MTL.EQPT_ID);
                //if (result) eqpt.Interlock = setValue == "1" ? true : false;
                MTL.OHTCCarOutMoveComplete = carOutMoveComplete;
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return false;
            }
        }


        public override bool setOHxC2MTL_CarInMoving(bool carMoving)
        {
            try
            {
                ValueWrite vm_carMoving = bcfApp.getWriteValueEvent(MTL.EqptObjectCate, MTL.EQPT_ID, "OHXC_TO_MTL_CAR_IN_MOVING");
                vm_carMoving.setWriteValue(carMoving ? "1" : "0");
                bool result = ISMControl.writeDeviceBlock(bcfApp, vm_carMoving);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: $"Set Car In Moving:{carMoving},result:{result}",
                         XID: MTL.EQPT_ID);
                MTL.OHTCCarInMoving = carMoving;
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return false;
            }
            return true;
        }


        public override bool setOHxC2MTL_CarInInterlock(bool interlock)
        {
            try
            {
                ValueWrite vm_car_in_interlock = bcfApp.getWriteValueEvent(MTL.EqptObjectCate, MTL.EQPT_ID, "OHXC_TO_MTL_CAR_IN_INTERLOCK");
                vm_car_in_interlock.setWriteValue(interlock ? "1" : "0");
                bool result = ISMControl.writeDeviceBlock(bcfApp, vm_car_in_interlock);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: $"Set Car In Interlock:{interlock},result:{result}",
                         XID: MTL.EQPT_ID);
                MTL.OHTCCarInInterlock = interlock;
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return false;
            }
        }

        public override bool setOHxC2MTL_CarInMoveComplete(bool move_complete)
        {
            try
            {
                ValueWrite vm_car_in_move_complete = bcfApp.getWriteValueEvent(MTL.EqptObjectCate, MTL.EQPT_ID, "OHXC_TO_MTL_CAR_IN_MOVE_COMPLETE");
                vm_car_in_move_complete.setWriteValue(move_complete ? "1" : "0");
                bool result = ISMControl.writeDeviceBlock(bcfApp, vm_car_in_move_complete);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: $"Set Car In Move Complete:{move_complete},result:{result}",
                         XID: MTL.EQPT_ID);
                MTL.OHTCCarInMoveComplete = move_complete;
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return false;
            }
        }


        public override void GetMTL2OHxC_CarOutInterface(out bool carOutSafelyCheck, out bool carMoveComplete)
        {
            try
            {
                ValueRead vr_safety_check = bcfApp.getReadValueEvent(MTL.EqptObjectCate, MTL.EQPT_ID, "MTL_TO_OHXC_U2D_SAFETY_CHECK");
                // ValueRead vr_move_cmp = bcfApp.getReadValueEvent(eqpt.EqptObjectCate, eqpt.EQPT_ID, "MTL_TO_OHXC_U2D_MOVE_COMPLETE");
                carOutSafelyCheck = (bool)vr_safety_check.getText();
                // carMoveComplete = (bool)vr_move_cmp.getText();
                carMoveComplete = false;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                carOutSafelyCheck = false;
                carMoveComplete = false;
            }
        }
        public override void GetMTL2OHxC_CarInInterface(out bool carOutSafelyCheck, out bool carInInterlock)
        {
            try
            {
                ValueRead vr_safety_check = bcfApp.getReadValueEvent(MTL.EqptObjectCate, MTL.EQPT_ID, "MTL_TO_OHXC_D2U_SAFETY_CHECK");
                ValueRead vr_car_in = bcfApp.getReadValueEvent(MTL.EqptObjectCate, MTL.EQPT_ID, "MTL_TO_OHXC_D2U_CAR_IN_INTERLOCK");
                carOutSafelyCheck = (bool)vr_safety_check.getText();
                carInInterlock = (bool)vr_car_in.getText();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                carOutSafelyCheck = false;
                carInInterlock = false;
            }
        }


        public override void OHxCResetAllhandshake()
        {
            try
            {
                ValueWrite vw_ReplyAlarmReport = bcfApp.getWriteValueEvent(MTL.EqptObjectCate, MTL.EQPT_ID, "OHXC_TO_MTL_REPLY_ALARM_REPORT_HS");
                ValueWrite vw_MTLAlarmResetRequest = bcfApp.getWriteValueEvent(MTL.EqptObjectCate, MTL.EQPT_ID, "OHXC_TO_MTL_ALARM_RESET_REQUEST_HS");
                ValueWrite vr_CarOutReply = bcfApp.getWriteValueEvent(MTL.EqptObjectCate, MTL.EQPT_ID, "OHXC_TO_MTL_CAR_OUT_REPLY_HS");
                ValueWrite vr_CarOutNotify = bcfApp.getWriteValueEvent(MTL.EqptObjectCate, MTL.EQPT_ID, "OHXC_TO_MTL_CAR_OUT_NOTIFY_HS");
                ValueWrite vr_ReplyCatInDataCheck = bcfApp.getWriteValueEvent(MTL.EqptObjectCate, MTL.EQPT_ID, "OHXC_TO_MTL_REPLY_CAR_IN_DATA_CHECK_HS");
                vw_ReplyAlarmReport.initWriteValue();
                vw_MTLAlarmResetRequest.initWriteValue();
                vr_CarOutReply.initWriteValue();
                vr_CarOutNotify.initWriteValue();
                vr_ReplyCatInDataCheck.initWriteValue();
                ISMControl.writeDeviceBlock(scApp.getBCFApplication(), vw_ReplyAlarmReport);
                ISMControl.writeDeviceBlock(scApp.getBCFApplication(), vw_MTLAlarmResetRequest);
                ISMControl.writeDeviceBlock(scApp.getBCFApplication(), vr_CarOutReply);
                ISMControl.writeDeviceBlock(scApp.getBCFApplication(), vr_CarOutNotify);
                ISMControl.writeDeviceBlock(scApp.getBCFApplication(), vr_ReplyCatInDataCheck);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
         Data: $"Reset All Handshake.",
         XID: MTL.EQPT_ID);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
        }


        public override void CarOutSafetyChcek(object sender, ValueChangedEventArgs args)
        {
            var recevie_function =
                scApp.getFunBaseObj<MtlToOHxC_CarOutSafetyCheck>(MTL.EQPT_ID) as MtlToOHxC_CarOutSafetyCheck;
            try
            {
                recevie_function.Read(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: recevie_function.ToString(),
                         XID: MTL.EQPT_ID);
                MTL.MTLCarOutSafetyCheck = recevie_function.CarOutSafetyCheck;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<MtlToOHxC_CarOutSafetyCheck>(recevie_function);

            }
        }
        public override void CarOutInterlock(object sender, ValueChangedEventArgs args)
        {
            var recevie_function =
                scApp.getFunBaseObj<MtlToOHxC_CarOutInterlock>(MTL.EQPT_ID) as MtlToOHxC_CarOutInterlock;
            try
            {
                recevie_function.Read(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: recevie_function.ToString(),
                         XID: MTL.EQPT_ID);
                eqpt.MTLCarOutInterlock = recevie_function.CarOutInterlock;
                eqpt.SynchronizeTime = DateTime.Now;
                if (!recevie_function.CarOutInterlock)
                {
                    scApp.MTLService.carOutAllComplete(MTL);
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<MtlToOHxC_CarOutInterlock>(recevie_function);

            }
        }

    public override void MTL_LFTStatus(object sender, ValueChangedEventArgs args)
        {
            var recevie_function =
                scApp.getFunBaseObj<MtlToOHxC_LFTStatus>(MTL.EQPT_ID) as MtlToOHxC_LFTStatus;
            try
            {
                recevie_function.Read(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: recevie_function.ToString(),
                         XID: MTL.EQPT_ID);
                MTL.HasVehicle = recevie_function.HasVehicle;
                MTL.StopSignal = recevie_function.StopSingle;
                MTL.MTxMode = (ProtocolFormat.OHTMessage.MTxMode)recevie_function.Mode;
                MTL.MTLLocation = (ProtocolFormat.OHTMessage.MTLLocation)recevie_function.LFTLocation;
                MTL.MTLMovingStatus = (ProtocolFormat.OHTMessage.MTLMovingStatus)recevie_function.LFTMovingStatus;
                MTL.Encoder = recevie_function.LFTEncoder;
                MTL.MTLRailStatus = (ProtocolFormat.OHTMessage.MTLRailStatus)recevie_function.RailStatus;
                //MTL.VhInPosition = (ProtocolFormat.OHTMessage.VhInPosition)recevie_function.VhInPosition;
                MTL.SynchronizeTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<MtlToOHxC_LFTStatus>(recevie_function);

            }
        }

        public override void MTLSafetyStatusChanged(object sender, ValueChangedEventArgs args)
        {
            var recevie_function =
                scApp.getFunBaseObj<MtlToOHxC_SafetyCheck>(MTL.EQPT_ID) as MtlToOHxC_SafetyCheck;
            try
            {
                recevie_function.Read(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: recevie_function.ToString(),
                         XID: MTL.EQPT_ID);
                MTL.SafetySignal = recevie_function.MTLSafety != 0 ? false : true;
                MTL.SynchronizeTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<MtlToOHxC_LFTStatus>(recevie_function);

            }
        }

        public override void CarInSafetyChcek(object sender, ValueChangedEventArgs args)
        {
            var recevie_function =
                scApp.getFunBaseObj<MtlToOHxC_CarInSafetyCheck>(MTL.EQPT_ID) as MtlToOHxC_CarInSafetyCheck;
            try
            {
                recevie_function.Read(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: recevie_function.ToString(),
                         XID: MTL.EQPT_ID);
                MTL.MTLCarInSafetyCheck = recevie_function.CarInSafetyCheck;
                MTL.SynchronizeTime = DateTime.Now;

                //if (MTL.MTLCarInSafetyCheck)
                //{
                //    scApp.MTLService.carInSafetyAndVehicleStatusCheck(MTL);
                //}
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<MtlToOHxC_CarInSafetyCheck>(recevie_function);
            }
        }
        public override void MTL_CarOutRequest(object sender, ValueChangedEventArgs args)
        {
            var recevie_function =
               scApp.getFunBaseObj<MtlToOHxC_MtlCarOutRepuest>(MTL.EQPT_ID) as MtlToOHxC_MtlCarOutRepuest;
            var send_function =
                scApp.getFunBaseObj<OHxCToMtl_MtlCarOutReply>(MTL.EQPT_ID) as OHxCToMtl_MtlCarOutReply;
            try
            {
                recevie_function.Read(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: recevie_function.ToString(),
                         XID: MTL.EQPT_ID);
                int pre_car_out_vh_num = recevie_function.CarID;
                ushort hand_shake = recevie_function.Handshake;
                if (hand_shake == 1)
                {
                    send_function.ReturnCode = 1;
                    if (recevie_function.Canacel == 1)
                    {
                        scApp.MTLService.carOutRequestCancel(MTL);
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                                 Data: $"Process MTL car out cancel",
                                 XID: MTL.EQPT_ID);
                    }
                    //else if (recevie_function.MTLCarOutActionType != 2)
                    //{
                    //    send_function.ReturnCode = 2;
                    //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                    //             Data: $"Process MTL car out request, is success:{false},result:car out action type:{recevie_function.MTLCarOutActionType} is wrong",
                    //             XID: MTL.EQPT_ID);
                    //}
                    else
                    {
                        AVEHICLE pre_car_out_vh = scApp.VehicleBLL.cache.getVhByNum(pre_car_out_vh_num);
                        //MaintainSpace dockingMTS = scApp.EquipmentBLL.cache.GetDockingMTLOfMaintainSpace();//todo 之後會有兩個MTS 要知道是哪個MTS
                        var car_out_check_result = scApp.MTLService.checkVhAndMTxCarOutStatus(this.MTL, null, pre_car_out_vh);
                        send_function.ReturnCode = car_out_check_result.isSuccess ? (ushort)1 : (ushort)2;
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                                 Data: $"Process MTL car out request, is success:{car_out_check_result.isSuccess},result:{car_out_check_result.result}",
                                 XID: MTL.EQPT_ID);
                    }
                }
                else
                {
                    send_function.ReturnCode = 0;
                }
                send_function.Handshake = hand_shake == 0 ? (ushort)0 : (ushort)1;
                send_function.Write(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);

                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: send_function.ToString(),
                         XID: MTL.EQPT_ID);
                MTL.SynchronizeTime = DateTime.Now;
                //if (send_function.Handshake == 1 && send_function.ReturnCode == 1)
                if (send_function.Handshake == 1 && send_function.ReturnCode == 1 && recevie_function.Canacel != 1)
                {
                    AVEHICLE pre_car_out_vh = scApp.VehicleBLL.cache.getVhByNum(pre_car_out_vh_num);
                    scApp.MTLService.processCarOutScenario(MTL, pre_car_out_vh);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<MtlToOHxC_MtlCarOutRepuest>(recevie_function);
                scApp.putFunBaseObj<OHxCToMtl_MtlCarOutReply>(send_function);
            }
        }

        public override void MTL_CarInRequest(object sender, ValueChangedEventArgs args)
        {
            var recevie_function =
                  scApp.getFunBaseObj<MtlToOHxC_RequestCarInDataCheck>(MTL.EQPT_ID) as MtlToOHxC_RequestCarInDataCheck;
            var send_function =
                scApp.getFunBaseObj<OHxCToMtl_ReplyCarInDataCheck>(MTL.EQPT_ID) as OHxCToMtl_ReplyCarInDataCheck;
            try
            {
                recevie_function.Read(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: recevie_function.ToString(),
                         XID: MTL.EQPT_ID);
                ushort vh_num = recevie_function.CarID;
                ushort hand_shake = recevie_function.Handshake;

                AVEHICLE pre_car_in_vh = scApp.VehicleBLL.cache.getVhByNum(vh_num);
                if (hand_shake == 1)
                {
                    if(pre_car_in_vh != null)
                    {
                        var check_result = scApp.MTLService.checkVhAndMTxCarInStatus(MTL, null, pre_car_in_vh);
                        send_function.ReturnCode = check_result.isSuccess ? (UInt16)1 : (UInt16)3;
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                                 Data: $"check mtl car in result, is success:{check_result.isSuccess},result:{check_result.result}",
                                 XID: MTL.EQPT_ID);
                    }
                    else
                    {
                        send_function.ReturnCode = 2;
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                                 Data: $"check mts cal in result, vehicle num:{vh_num} not exist.",
                                 XID: MTL.EQPT_ID);
                    }
                }
                else
                {
                    send_function.ReturnCode = 0;
                }
                send_function.Handshake = hand_shake;
                send_function.Write(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);

                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: send_function.ToString(),
                         XID: MTL.EQPT_ID);
                MTL.SynchronizeTime = DateTime.Now;

                if (hand_shake == 1&& send_function.ReturnCode == 1)
                {
                    scApp.MTLService.carInSafetyAndVehicleStatusCheck(MTL);
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<MtlToOHxC_RequestCarInDataCheck>(recevie_function);
                scApp.putFunBaseObj<OHxCToMtl_ReplyCarInDataCheck>(send_function);

            }
        }

        public override void MTL_CarInRequestCancel(object sender, ValueChangedEventArgs args)
        {
            var recevie_function = 
                scApp.getFunBaseObj<MtlToOHxC_MtlCarInRequestCancel>(MTL.EQPT_ID) as MtlToOHxC_MtlCarInRequestCancel;
            var send_function =
                scApp.getFunBaseObj<OHxCToMtl_MtlCarInRequestCancelReply>(MTL.EQPT_ID) as OHxCToMtl_MtlCarInRequestCancelReply;
            try
            {
                recevie_function.Read(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: recevie_function.ToString(),
                         XID: MTL.EQPT_ID);
                ushort vh_num = recevie_function.CarID;
                ushort hand_shake = recevie_function.Handshake;

                AVEHICLE pre_car_in_vh = scApp.VehicleBLL.cache.getVhByNum(vh_num);
                if (hand_shake == 1)
                {
                    if (pre_car_in_vh != null)
                    {
                        var check_result = scApp.MTLService.checkVhAndMTxCarInStatus(MTL, null, pre_car_in_vh);
                        send_function.ReturnCode = check_result.isSuccess ? (UInt16)1 : (UInt16)3;
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                                 Data: $"check mtl car in cancel result, is success:{check_result.isSuccess},result:{check_result.result}",
                                 XID: MTL.EQPT_ID);
                    }
                    else
                    {
                        send_function.ReturnCode = 2;
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                                 Data: $"check mts cal in cancel result, vehicle num:{vh_num} not exist.",
                                 XID: MTL.EQPT_ID);
                    }
                }
                else
                {
                    send_function.ReturnCode = 0;
                }
                send_function.Handshake = hand_shake;
                send_function.Write(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);

                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: send_function.ToString(),
                         XID: MTL.EQPT_ID);
                MTL.SynchronizeTime = DateTime.Now;

                //if (hand_shake == 1 && send_function.ReturnCode == 1)
                //{
                //    scApp.MTLService.carInSafetyAndVehicleStatusCheck(MTL);
                //}
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<MtlToOHxC_MtlCarInRequestCancel>(recevie_function);
                scApp.putFunBaseObj<OHxCToMtl_MtlCarInRequestCancelReply>(send_function);
            }
        }

        public override void MTL_Alarm_Report(object sender, ValueChangedEventArgs args)
        {
            var recevie_function =
                scApp.getFunBaseObj<MtlToOHxC_AlarmReport>(eqpt.EQPT_ID) as MtlToOHxC_AlarmReport;
            var send_function =
                scApp.getFunBaseObj<MtlToOHxC_ReplyAlarmReport>(eqpt.EQPT_ID) as MtlToOHxC_ReplyAlarmReport;
            try
            {
                recevie_function.Read(bcfApp, eqpt.EqptObjectCate, eqpt.EQPT_ID);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: recevie_function.ToString(),
                         XID: eqpt.EQPT_ID);
                UInt16 error_code = recevie_function.ErrorCode;
                ProtocolFormat.OHTMessage.ErrorStatus status = (ProtocolFormat.OHTMessage.ErrorStatus)recevie_function.ErrorStatus;
                ushort hand_shake = recevie_function.Handshake;

                send_function.Handshake = hand_shake;
                send_function.Write(bcfApp, eqpt.EqptObjectCate, eqpt.EQPT_ID);
                eqpt.SynchronizeTime = DateTime.Now;
                if (hand_shake == 1)
                {
                    //scApp.LineService.ProcessAlarmReport(eqpt.NODE_ID, eqpt.EQPT_ID, eqpt.Real_ID, "", error_code.ToString(), status);
                    scApp.LineService.ProcessAlarmReport("", eqpt.EQPT_ID,  error_code.ToString(), status,"");
                }

                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: send_function.ToString(),
                         XID: eqpt.EQPT_ID);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<MtlToOHxC_AlarmReport>(recevie_function);
                scApp.putFunBaseObj<MtlToOHxC_ReplyAlarmReport>(send_function);

            }
        }


        public override void MTL_Alive(object sender, ValueChangedEventArgs args)
        {
            var recevie_function =
                scApp.getFunBaseObj<MtlToOHxC_Alive>(MTL.EQPT_ID) as MtlToOHxC_Alive;
            try
            {
                recevie_function.Read(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: recevie_function.ToString(),
                         XID: eqpt.EQPT_ID);

                eqpt.Eq_Alive_Index = recevie_function.AliveIndex;
                eqpt.Eq_Alive_Last_Change_time = DateTime.Now;
                eqpt.SynchronizeTime = DateTime.Now;

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<MtlToOHxC_Alive>(recevie_function);

            }
        }
        public override void CarInInterlock(object sender, ValueChangedEventArgs args)
        {
            var recevie_function =
                scApp.getFunBaseObj<MtlToOHxC_CarInInterlock>(MTL.EQPT_ID) as MtlToOHxC_CarInInterlock;
            try
            {
                recevie_function.Read(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: recevie_function.ToString(),
                         XID: MTL.EQPT_ID);
                MTL.MTLCarInInterlock = recevie_function.CarInInterlock;
                MTL.SynchronizeTime = DateTime.Now;

                if (!recevie_function.CarInInterlock)
                {
                    MTL.SetCarInMoveComplete(false);
                    MTL.SetCarInInterlock(false);
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<MtlToOHxC_CarInInterlock>(recevie_function);

            }
        }
        public override void MTL_Current_ID(object sender, ValueChangedEventArgs args)
        {
            var recevie_function =
                scApp.getFunBaseObj<MtlToOHxC_CurrentCarID>(MTL.EQPT_ID) as MtlToOHxC_CurrentCarID;
            try
            {
                recevie_function.Read(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: recevie_function.ToString(),
                         XID: MTL.EQPT_ID);
                MTL.CurrentCarID = recevie_function.CarID.ToString();
                MTL.SynchronizeTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<MtlToOHxC_CurrentCarID>(recevie_function);

            }
        }

        public override bool OHxC_AlarmResetRequest()
        {
            bool isSendSuccess = false;
            var send_function =
                scApp.getFunBaseObj<OHxCToMtl_AlarmResetRequest>(eqpt.EQPT_ID) as OHxCToMtl_AlarmResetRequest;
            var receive_function =
                scApp.getFunBaseObj<MtlToOHxC_AlarmResetReply>(eqpt.EQPT_ID) as MtlToOHxC_AlarmResetReply;
            try
            {
                //1.準備要發送的資料
                ValueRead vr_reply = receive_function.getValueReadHandshake
                    (bcfApp, eqpt.EqptObjectCate, eqpt.EQPT_ID);
                //2.紀錄發送資料的Log
                send_function.Handshake = 1;
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: send_function.ToString(),
                         XID: eqpt.EQPT_ID);
                //3.等待回復
                TrxMPLC.ReturnCode returnCode =
                    send_function.SendRecv(bcfApp, eqpt.EqptObjectCate, eqpt.EQPT_ID, vr_reply);
                //4.取得回復的結果
                if (returnCode == TrxMPLC.ReturnCode.Normal)
                {
                    receive_function.Read(bcfApp, eqpt.EqptObjectCate, eqpt.EQPT_ID);
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                             Data: receive_function.ToString(),
                             XID: eqpt.EQPT_ID);
                    isSendSuccess = true;
                }
                send_function.Handshake = 0;
                send_function.resetHandshake(bcfApp, eqpt.EqptObjectCate, eqpt.EQPT_ID);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: send_function.ToString(),
                         XID: eqpt.EQPT_ID);

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<OHxCToMtl_AlarmResetRequest>(send_function);
                scApp.putFunBaseObj<MtlToOHxC_AlarmResetReply>(receive_function);
            }
            return (isSendSuccess);
        }

        //object pauseByMTLStatusObj = new object();
        //        bool pauseSet = false;
        //        private long syncPoint = 0;
        //        public void checkThenSetVehiclePauseByMTLStatus(string caller)
        //        {
        //            MTLPauserHandlerInfoLogger.Info($"Entry checkThenSetVehiclePauseByMTLStatus method, caller:[{caller}], before into lock. Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
        //            if (System.Threading.Interlocked.Exchange(ref syncPoint, 1) == 0)
        //            {
        //                //lock (pauseByMTLStatusObj)
        //                //{
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
        //                            //Task.Run(() => scApp.VehicleService.ResumeAllVehicleBySafetyPause());
        //                            scApp.VehicleService.ResumeAllVehicleBySafetyPause();

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
        //                                scApp.VehicleService.PauseAllVehicleBySafetyPause();
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
        //                                //Task.Run(() => scApp.VehicleService.ResumeAllVehicleBySafetyPause());
        //                                scApp.VehicleService.ResumeAllVehicleBySafetyPause();
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
        //                                scApp.VehicleService.PauseAllVehicleBySafetyPause();
        //                                MTLPauserHandlerInfoLogger.Info($"In checkThenSetVehiclePauseByMTLStatus method, caller:[{caller}], In Normal process after Stop OHT." +
        //    $" Alive:[{MTL.Is_Eq_Alive}] Mode:[{MTL.MTxMode}] Location:[{MTL.MTLLocation}] RailStatus:[{MTL.MTLRailStatus}] Error:[{MTL.StopSignal}] needPause:[{need_pause}] " +
        //    $"ForcePass:[{DebugParameter.isForceBypassMTLPauseCheck}] pauseSet:[{pauseSet}] Current Thread:[{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
        //                            }
        //                            else
        //                            {
        //                                //Task.Run(() => scApp.VehicleService.ResumeAllVehicleBySafetyPause());
        //                                scApp.VehicleService.ResumeAllVehicleBySafetyPause();
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

        public override (bool isSendSuccess, UInt16 returnCode) OHxC_CarOutRequestCancelNotify()
        {
            bool isSendSuccess = false;
            var send_function =
                scApp.getFunBaseObj<OHxCToMtl_CarOutRequestCancel>(MTL.EQPT_ID) as OHxCToMtl_CarOutRequestCancel;
            var receive_function =
                scApp.getFunBaseObj<MtlToOHxC_CarOutRequestCancelReply>(MTL.EQPT_ID) as MtlToOHxC_CarOutRequestCancelReply;
            try
            {
                //1.準備要發送的資料
                string preCarOutVhId = MTL.PreCarOutVhID;
                AVEHICLE preCarOutVh = scApp.VehicleBLL.cache.getVehicle(preCarOutVhId);
                send_function.CarID = (UInt16)preCarOutVh.Num;
                //send_function.ActionType = action_type;
                ValueRead vr_reply = receive_function.getValueReadHandshake
                    (bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);
                //2.紀錄發送資料的Log
                send_function.Handshake = 1;
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: send_function.ToString(),
                         XID: MTL.EQPT_ID);
                //3.等待回復
                TrxMPLC.ReturnCode returnCode =
                    send_function.SendRecv(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID, vr_reply);
                //4.取得回復的結果
                if (returnCode == TrxMPLC.ReturnCode.Normal)
                {
                    receive_function.Read(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                             Data: receive_function.ToString(),
                             XID: MTL.EQPT_ID);
                    isSendSuccess = true;
                }
                send_function.Handshake = 0;
                send_function.Write(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: send_function.ToString(),
                         XID: MTL.EQPT_ID);
                return (isSendSuccess, receive_function.ReturnCode);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return (isSendSuccess, 0);
            }
            finally
            {
                scApp.putFunBaseObj<OHxCToMtl_CarOutRequestCancel>(send_function);
                scApp.putFunBaseObj<MtlToOHxC_CarOutRequestCancelReply>(receive_function);
            }
        }

        public override void MTL_ManualCarOutRequest(object sender, ValueChangedEventArgs args)
        {
            //20210811
            var recevie_function =
                scApp.getFunBaseObj<MtlToOHxC_ManualCarOutRequest>(MTL.EQPT_ID) as MtlToOHxC_ManualCarOutRequest;
            var send_function =
                scApp.getFunBaseObj<OHxCToMtl_ManualCarOutRequestReply>(MTL.EQPT_ID) as OHxCToMtl_ManualCarOutRequestReply;
            try
            {
                recevie_function.Read(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: recevie_function.ToString(),
                         XID: MTL.EQPT_ID);
                ushort vh_num = recevie_function.CarID;
                ushort hand_shake = recevie_function.Handshake;

                AVEHICLE pre_car_in_vh = scApp.VehicleBLL.cache.getVhByNum(vh_num);
                if (hand_shake == 1)
                {
                    if (pre_car_in_vh != null)
                    {
                        bool checkResult = !pre_car_in_vh.isTcpIpConnect && pre_car_in_vh.IS_INSTALLED;
                        send_function.ReturnCode = checkResult ? (UInt16)1 : (UInt16)3;
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                                 Data: $"check mtl manual car out result, is success:{checkResult}",
                                 XID: MTL.EQPT_ID);
                        scApp.MTLService.readyToCarOutStart(true);
                    }
                    else
                    {
                        send_function.ReturnCode = 2;
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                                 Data: $"check mtl manual car out result, vehicle num:{vh_num} not exist.",
                                 XID: MTL.EQPT_ID);
                    }
                }
                else
                {
                    send_function.ReturnCode = 0;
                }
                send_function.Handshake = hand_shake;
                send_function.Write(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);

                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: send_function.ToString(),
                         XID: MTL.EQPT_ID);
                MTL.SynchronizeTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<MtlToOHxC_ManualCarOutRequest>(recevie_function);
                scApp.putFunBaseObj<OHxCToMtl_ManualCarOutRequestReply>(send_function);
            }
        }
        public override void MTL_ManualCarOutFinish(object sender, ValueChangedEventArgs args)
        {
            var recevie_function =
                scApp.getFunBaseObj<MtlToOHxC_ManualCarOuFinish>(MTL.EQPT_ID) as MtlToOHxC_ManualCarOuFinish;
            var send_function =
                scApp.getFunBaseObj<OHxCToMtl_ManualCarOutFinishReply>(MTL.EQPT_ID) as OHxCToMtl_ManualCarOutFinishReply;
            try
            {
                recevie_function.Read(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: recevie_function.ToString(),
                         XID: MTL.EQPT_ID);
                ushort vh_num = recevie_function.CarID;
                ushort hand_shake = recevie_function.Handshake;
                send_function.Handshake = hand_shake;
                send_function.Write(bcfApp, MTL.EqptObjectCate, MTL.EQPT_ID);

                //20210819
                AVEHICLE pre_car_in_vh = scApp.VehicleBLL.cache.getVhByNum(vh_num);
                if (pre_car_in_vh != null)
                {
                    scApp.VehicleService.Remove(pre_car_in_vh.VEHICLE_ID);
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                        Data: $"mtl manual car out finished, vh id:{pre_car_in_vh.VEHICLE_ID}",
                        XID: MTL.EQPT_ID);
                }

                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(MTLValueDefMapAction), Device: SCAppConstants.DeviceName.DEVICE_NAME_MTx,
                         Data: send_function.ToString(),
                         XID: MTL.EQPT_ID);
                MTL.SynchronizeTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<MtlToOHxC_ManualCarOuFinish>(recevie_function);
                scApp.putFunBaseObj<OHxCToMtl_ManualCarOutFinishReply>(send_function);
            }
        }
    }
}
