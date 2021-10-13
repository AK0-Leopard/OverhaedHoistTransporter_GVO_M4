using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.bcf.Controller;
using com.mirle.ibg3k0.bcf.Data.ValueDefMapAction;
using com.mirle.ibg3k0.bcf.Data.VO;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.BLL;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.PLC_Functions;
using com.mirle.ibg3k0.sc.Service;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.ValueDefMapAction
{
    public class PortValueDefMapAction : IValueDefMapAction
    {
        public const string DEVICE_NAME_PORT = "PORT";
        public bool IsInService { get; private set; } = false;
        public bool IsAGVMode { get; private set; } = true;
        private Logger logger = NLog.LogManager.GetCurrentClassLogger();

        //protected APORT port = null;
        protected APORTSTATION PortStation = null;

        protected SCApplication scApp = null;
        protected BCFApplication bcfApp = null;

        public PortValueDefMapAction() : base()
        {
            scApp = SCApplication.getInstance();
            bcfApp = scApp.getBCFApplication();
        }

        public virtual void doInit()
        {
            try
            {
                ValueRead vr = null;

                if (bcfApp.tryGetReadValueEventstring(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "NOW_INPUT_MODE", out vr))
                {
                    vr.afterValueChange += (_sender, _e) => Port_onDirectionStatusChangeToInput(_sender, _e);
                }
                if (bcfApp.tryGetReadValueEventstring(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "NOW_OUTPUT_MODE", out vr))
                {
                    vr.afterValueChange += (_sender, _e) => Port_onDirectionStatusChangeToOutput(_sender, _e);
                }
                if (bcfApp.tryGetReadValueEventstring(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "BARCODE_READ_DONE", out vr))
                {
                    vr.afterValueChange += (_sender, _e) => Port_onBarcodeReadDone(_sender, _e);
                }
                if (bcfApp.tryGetReadValueEventstring(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "CST_TRANSFER_COMPLETE", out vr))
                {
                    vr.afterValueChange += (_sender, _e) => Port_onTransferComplete(_sender, _e);
                }
                if (bcfApp.tryGetReadValueEventstring(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "CST_REMOVE_CHECK", out vr))
                {
                    vr.afterValueChange += (_sender, _e) => Port_onCSTRemoveCheck(_sender, _e);
                }




                if (bcfApp.tryGetReadValueEventstring(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "OP_RUN", out vr))
                {
                    vr.afterValueChange += (_sender, _e) => Port_onInService(_sender, _e);
                }
                if (bcfApp.tryGetReadValueEventstring(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "OP_DOWN", out vr))
                {
                    vr.afterValueChange += (_sender, _e) => Port_onOutOfService(_sender, _e);
                }

                if (bcfApp.tryGetReadValueEventstring(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "OP_ERROR", out vr))
                {
                    vr.afterValueChange += (_sender, _e) => Port_onAlarmHppened(_sender, _e);
                }

                if (bcfApp.tryGetReadValueEventstring(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "ERROR_CODE", out vr))
                {
                    vr.afterValueChange += (_sender, _e) => Port_ErrorCodeChanged(_sender, _e);
                }

                if (bcfApp.tryGetReadValueEventstring(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "PORTALLINFO", out vr))
                {
                    vr.afterValueChange += (_sender, _e) => PortInfoChange(_sender, _e);
                }
                if (bcfApp.tryGetReadValueEventstring(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "WAIT_IN", out vr))
                {
                    vr.afterValueChange += (_sender, _e) => Port_onWaitIn(_sender, _e);
                }
                if (bcfApp.tryGetReadValueEventstring(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "WAIT_OUT", out vr))
                {
                    vr.afterValueChange += (_sender, _e) => Port_onWaitOut(_sender, _e);
                }
                if (bcfApp.tryGetReadValueEventstring(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "LOAD_POSITION_1", out vr))
                {
                    vr.afterValueChange += (_sender, _e) => Port_onLoadPosition(_sender, _e);
                }
                if (bcfApp.tryGetReadValueEventstring(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "LOAD_POSITION_2", out vr))
                {
                    vr.afterValueChange += (_sender, _e) => Port_onLoadPosition2(_sender, _e);
                }
                if (bcfApp.tryGetReadValueEventstring(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "LOAD_POSITION_3", out vr))
                {
                    vr.afterValueChange += (_sender, _e) => Port_onLoadPosition3(_sender, _e);
                }
                if (bcfApp.tryGetReadValueEventstring(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "LOAD_POSITION_4", out vr))
                {
                    vr.afterValueChange += (_sender, _e) => Port_onLoadPosition4(_sender, _e);
                }
                if (bcfApp.tryGetReadValueEventstring(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "LOAD_POSITION_5", out vr))
                {
                    vr.afterValueChange += (_sender, _e) => Port_onLoadPosition5(_sender, _e);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
        }

        //private void Port_onFireAlarm(object sender, ValueChangedEventArgs e)
        //{
        //    var function = scApp.getFunBaseObj<PortPLCInfo>(PortStation.PORT_ID) as PortPLCInfo;
        //    try
        //    {
        //        //1.建立各個Function物件
        //        function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
        //        //2.read log
        //        //function.Timestamp = DateTime.Now;
        //        //LogManager.GetLogger("com.mirle.ibg3k0.sc.Common.LogHelper").Info(function.ToString());
        //        NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());
        //        //LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(EQStatusReport), Device: DEVICE_NAME_MTL,
        //        //    XID: eqpt.EQPT_ID, Data: function.ToString());
        //        //3.logical (include db save)
        //        if (function.FireAlarm == true)
        //        {
        //            //scApp.TransferService.OHBC_AlarmSet(port.PORT_ID, SCAppConstants.SystemAlarmCode.PLC_Issue.FireAlarm);
        //            scApp.LineService.ProcessAlarmReport(PortStation.PORT_ID, SCAppConstants.SystemAlarmCode.PLC_Issue.FireAlarm, ProtocolFormat.OHTMessage.ErrorStatus.ErrSet, "Fire Alarm");
        //        }
        //        else
        //        {
        //            //scApp.TransferService.OHBC_AlarmAllCleared(port.PORT_ID);
        //            scApp.LineService.ProcessAlarmReport(PortStation.PORT_ID, SCAppConstants.SystemAlarmCode.PLC_Issue.FireAlarm, ProtocolFormat.OHTMessage.ErrorStatus.ErrReset, "Fire Alarm");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "Exception");
        //    }
        //    finally
        //    {
        //        scApp.putFunBaseObj<PortPLCInfo>(function);
        //    }
        //}

        private void Port_onCSTPresenceMismatch(object sender, ValueChangedEventArgs e)
        {
            var function = scApp.getFunBaseObj<PortPLCInfo>(PortStation.PORT_ID) as PortPLCInfo;
            try
            {
                //1.建立各個Function物件
                function.Timestamp = DateTime.Now;
                function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                //if (function.CSTPresenceMismatch)
                //{
                //    //TODO
                //    if (function.IsInputMode)
                //    {
                //        scApp.TransferService.OpenAGV_Station(function.EQ_ID.Trim(), true, "CSTPresenceMismatch");
                //    }

                //    scApp.TransferService.PLC_AGV_Station(function, "CSTPresenceMismatch");
                //}
                //else
                //{
                //    //0506，士偉、冠皚討論說，若 Mismatch OFF，如果有在執行的退補BOX，就將其把命取消 註：防止 Mismatch 上報後再報 WaitIn 的機制
                //    scApp.TransferService.PLC_AGV_CancelCmd(function.EQ_ID);
                //}
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCInfo>(function);
            }
        }



        private void Port_onAlarmHppened(object sender, ValueChangedEventArgs e)
        {
            var function = scApp.getFunBaseObj<PortPLCInfo>(PortStation.PORT_ID) as PortPLCInfo;
            try
            {
                function.Timestamp = DateTime.Now;
                function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                PortStation.IsError = function.OpError;


                //if (function.OpError == true)
                //{
                //    //pass function.ErrorCode
                //    scApp.TransferService.OHBC_AlarmSet(function.EQ_ID, function.ErrorCode.ToString());
                //    //PLC_AlarmSet(port.PORT_ID, function.ErrorCode);
                //}
                //else
                //{
                //    scApp.TransferService.OHBC_AlarmAllCleared(function.EQ_ID);
                //    //PLC_AlarmAllCleared(port.PORT_ID);
                //    scApp.TransferService.PortCIM_ON(function, "Port_Error_OFF");
                //}
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCInfo>(function);
            }
        }



        private void Port_ErrorCodeChanged(object sender, ValueChangedEventArgs e)
        {
            var function = scApp.getFunBaseObj<PortPLCInfo>(PortStation.PORT_ID) as PortPLCInfo;
            try
            {
                function.Timestamp = DateTime.Now;
                function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());
                PortStation.ErrorCode = function.ErrorCode;

                bool isClearAll = (function.ErrorCode == 0);
                if (isClearAll)
                {
                    scApp.LineService.ProcessAlarmReport(PortStation.NODE_ID, PortStation.PORT_ID, "0", ProtocolFormat.OHTMessage.ErrorStatus.ErrReset, "");

                }
                else
                {
                    scApp.LineService.ProcessAlarmReport(PortStation.NODE_ID, PortStation.PORT_ID, function.ErrorCode.ToString(), ProtocolFormat.OHTMessage.ErrorStatus.ErrSet, "");
                }



                //if (function.OpError == true)
                //{
                //    //pass function.ErrorCode
                //    scApp.TransferService.OHBC_AlarmSet(function.EQ_ID, function.ErrorCode.ToString());
                //    //PLC_AlarmSet(port.PORT_ID, function.ErrorCode);
                //}
                //else
                //{
                //    scApp.TransferService.OHBC_AlarmAllCleared(function.EQ_ID);
                //    //PLC_AlarmAllCleared(port.PORT_ID);
                //    scApp.TransferService.PortCIM_ON(function, "Port_Error_OFF");
                //}
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCInfo>(function);
            }
        }


        //private void Port_onCstPresenceStatusChange(object sender, ValueChangedEventArgs e)
        //{
        //    var function = scApp.getFunBaseObj<PortPLCInfo>(PortStation.PORT_ID) as PortPLCInfo;
        //    try
        //    {
        //        //1.建立各個Function物件
        //        function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
        //        //2.read log
        //        //function.Timestamp = DateTime.Now;
        //        //LogManager.GetLogger("com.mirle.ibg3k0.sc.Common.LogHelper").Info(function.ToString());
        //        NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());
        //        //LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(EQStatusReport), Device: DEVICE_NAME_MTL,
        //        //    XID: eqpt.EQPT_ID, Data: function.ToString());
        //        //3.logical (include db save)

        //        scApp.TransferService.TransferServiceLogger.Info
        //        (
        //            DateTime.Now.ToString("HH:mm:ss.fff ") +
        //            "PLC >> PLC|AGV卡匣在席變化  PortName: " + function.EQ_ID
        //            + " IsReadyToLoad: " + function.IsReadyToLoad
        //            + " IsReadyToUnload:" + function.IsReadyToUnload
        //            + " IsCSTPresence:" + function.IsCSTPresence
        //            + " CstRemoveCheck:" + function.CstRemoveCheck
        //            + " IsInputMode:" + function.IsInputMode
        //            + " IsOutputMode:" + function.IsOutputMode
        //            + " PortWaitIn:" + function.PortWaitIn
        //            + " PortWaitOut:" + function.PortWaitOut
        //        );

        //        if (function.IsCSTPresence == false)
        //        {
        //            //scApp.TransferService.PortCstPositionOFF(function);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "Exception");
        //    }
        //    finally
        //    {
        //        scApp.putFunBaseObj<PortPLCInfo>(function);
        //    }
        //}

        //private void Port_onAGVPortReadyStatusChange(object sender, ValueChangedEventArgs e)
        //{
        //    var function = scApp.getFunBaseObj<PortPLCInfo>(PortStation.PORT_ID) as PortPLCInfo;
        //    try
        //    {
        //        //1.建立各個Function物件
        //        function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
        //        NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

        //        //if (function.AGVPortReady == true)
        //        //{
        //        //    //TODO
        //        //    scApp.TransferService.PLC_AGV_Station(function, "AGVPortReady");
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "Exception");
        //    }
        //    finally
        //    {
        //        scApp.putFunBaseObj<PortPLCInfo>(function);
        //    }
        //}
        private void Port_onCIM_ON(object sender, ValueChangedEventArgs e)
        {
            var function = scApp.getFunBaseObj<PortPLCInfo>(PortStation.PORT_ID) as PortPLCInfo;
            try
            {
                //1.建立各個Function物件
                function.Timestamp = DateTime.Now;
                function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                //if (scApp.TransferService.GetIgnoreModeChange(function))
                //{
                //    return;
                //}

                //scApp.TransferService.PortCIM_ON(function, "Port_CIM_ON 訊號 ON_OFF");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCInfo>(function);
            }
        }
        private void Port_onPreLoadOK(object sender, ValueChangedEventArgs e)
        {
            var function = scApp.getFunBaseObj<PortPLCInfo>(PortStation.PORT_ID) as PortPLCInfo;
            try
            {
                //1.建立各個Function物件
                function.Timestamp = DateTime.Now;
                function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                //if (scApp.TransferService.GetIgnoreModeChange(function))
                //{
                //    return;
                //}
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCInfo>(function);
            }
        }
        private void Port_onOutOfService(object sender, ValueChangedEventArgs e)
        {
            var function = scApp.getFunBaseObj<PortPLCInfo>(PortStation.PORT_ID) as PortPLCInfo;
            try
            {
                //1.建立各個Function物件
                function.Timestamp = DateTime.Now;
                function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCInfo>(function);
            }
        }

        private void Port_onInService(object sender, ValueChangedEventArgs e)
        {
            var function = scApp.getFunBaseObj<PortPLCInfo>(PortStation.PORT_ID) as PortPLCInfo;
            try
            {
                //1.建立各個Function物件
                function.Timestamp = DateTime.Now;
                function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());


                if (function.OpRun)
                {
                    scApp.TransferService.portInServeice(PortStation.PORT_ID);
                    scApp.TransferService.PortAlarrmReset(PortStation.PORT_ID);
                }
                else
                {
                    scApp.TransferService.portOutServeice(PortStation.PORT_ID);
                }

                //if (scApp.TransferService.GetIgnoreModeChange(function))
                //{
                //    return;
                //}

                //scApp.TransferService.PLC_ReportRunDwon(function, "PLC_RUN:" + function.OpAutoMode);

                //if (function.OpAutoMode)
                //{
                //    IsInService = true;
                //    scApp.TransferService.OHBC_AlarmCleared(function.EQ_ID, ((int)AlarmLst.PORT_DOWN).ToString());
                //}
                //else
                //{
                //    scApp.TransferService.OHBC_AlarmSet(function.EQ_ID, ((int)AlarmLst.PORT_DOWN).ToString());
                //}
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCInfo>(function);
            }
        }

        private void PortInfoChange(object sender, ValueChangedEventArgs e)
        {
            var function = scApp.getFunBaseObj<PortPLCInfo>(PortStation.PORT_ID) as PortPLCInfo;
            try
            {
                //1.建立各個Function物件
                function.Timestamp = DateTime.Now;
                function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                function.Port_ID = PortStation.PORT_ID;
                logger.Info(function.ToString());
                APORTSTATION port = scApp.getEQObjCacheManager().getPortStation(PortStation.PORT_ID);
                if (function.OpError)
                {
                    port.PLCPortStatus = APORTSTATION.PortStatus.Fault;
                    scApp.LineService.ProcessAlarmReport("", PortStation.PORT_ID, AlarmBLL.PORT_STATUS_DOWN, ProtocolFormat.OHTMessage.ErrorStatus.ErrSet,
                    $"Port Status:[{PortStation.PORT_ID}] is Down.");
                }
                else if (function.OpDown)
                {
                    port.PLCPortStatus = APORTSTATION.PortStatus.Down;
                    scApp.LineService.ProcessAlarmReport("", PortStation.PORT_ID, AlarmBLL.PORT_STATUS_DOWN, ProtocolFormat.OHTMessage.ErrorStatus.ErrSet,
                    $"Port Status:[{PortStation.PORT_ID}] is Down.");
                }
                else if (function.OpRun)
                {
                    port.PLCPortStatus = APORTSTATION.PortStatus.Run;
                    scApp.LineService.ProcessAlarmReport("", PortStation.PORT_ID, AlarmBLL.PORT_STATUS_DOWN, ProtocolFormat.OHTMessage.ErrorStatus.ErrReset,
                    $"Port Status:[{PortStation.PORT_ID}] is Down.");
                }
                else
                {
                    port.PLCPortStatus = APORTSTATION.PortStatus.None;
                    scApp.LineService.ProcessAlarmReport("", PortStation.PORT_ID, AlarmBLL.PORT_STATUS_DOWN, ProtocolFormat.OHTMessage.ErrorStatus.ErrSet,
                    $"Port Status:[{PortStation.PORT_ID}] is Down.");
                }

                port.IsInPutMode = function.IsInputMode;
                port.IsOutPutMode = function.IsOutputMode;
                port.IsModeChangeable = function.IsModeChangable;

                port.PortWaitIn = function.PortWaitIn;
                port.PortWaitOut = function.PortWaitOut;
                port.IsAutoMode = function.IsAutoMode;
                port.IsReadyToLoad = function.IsReadyToLoad;
                port.IsReadyToUnload = function.IsReadyToUnload;
                port.IsCSTPresenceLoc1 = function.LoadPosition1;
                port.IsCSTPresenceLoc2 = function.LoadPosition2;
                port.IsCSTPresenceLoc3 = function.LoadPosition3;
                port.IsCSTPresenceLoc4 = function.LoadPosition4;
                port.IsCSTPresenceLoc5 = function.LoadPosition5;

                port.CST_ID = function.RFIDCassetteID;

                //發佈到Redis
                //scApp.PortBLL.redis.SetPortInfo(port.PORT_ID, function);
                byte[] port_Serialize = PortStationBLL.Catch.convert2PortstationInfo(port.PORT_ID, port);
                scApp.getNatsManager().PublishAsync
                   (string.Format(SCAppConstants.NATS_SUBJECT_PORTSTATION_INFO_0, port.PORT_ID.Trim()), port_Serialize);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCInfo>(function);
            }
        }

        private void PublishVhInfo(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                //string vh_id = e.PropertyValue as string;
                //AVEHICLE vh = scApp.VehicleBLL.getVehicleByID(vh_id);
                //if (sender == null) return;
                //byte[] vh_Serialize = BLL.VehicleBLL.Convert2GPB_VehicleInfo(vh);
                ////RecoderVehicleObjInfoLog(vh_id, vh_Serialize);

                //scApp.getNatsManager().PublishAsync
                //    (string.Format(SCAppConstants.NATS_SUBJECT_VH_INFO_0, vh.VEHICLE_ID.Trim()), vh_Serialize);

                //scApp.getRedisCacheManager().ListSetByIndexAsync
                //    (SCAppConstants.REDIS_LIST_KEY_VEHICLES, vh.VEHICLE_ID, vh.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
            //});
        }

        public virtual void doShareMemoryInit(BCFAppConstants.RUN_LEVEL runLevel)
        {
            try
            {
                switch (runLevel)
                {
                    case BCFAppConstants.RUN_LEVEL.ZERO:
                        initStageCount();
                        initialValueRead();
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
        private void initStageCount()
        {
            scApp.getEQObjCacheManager().getPortStation(PortStation.PORT_ID).stageCount 
                = scApp.PortStationBLL.OperateDB.getAPortstationStageCountByID(PortStation.PORT_ID);
        }
        private void initialValueRead()
        {
            

            ValueRead vr = null;

            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "WAIT_IN", out vr))
            //{
            //    Port_onWaitIn(vr, null);
            //}
            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "WAIT_OUT", out vr))
            //{
            //    Port_onWaitOut(vr, null);
            //}
            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "NOW_INPUT_MODE", out vr))
            //{
            //    Port_onDirectionStatusChangeToInput(vr, null);
            //}
            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "NOW_OUTPUT_MODE", out vr))
            //{
            //    Port_onDirectionStatusChangeToOutput(vr, null);
            //}
            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "BARCODE_READ_DONE", out vr))
            //{
            //    Port_onBarcodeReadDone(vr, null);
            //}
            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "CST_TRANSFER_COMPLETE", out vr))
            //{
            //    Port_onTransferComplete(vr, null);
            //}
            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "CST_REMOVE_CHECK", out vr))
            //{
            //    Port_onCSTRemoveCheck(vr, null);
            //}
            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "LOAD_POSITION_1", out vr))
            //{
            //    Port_onLoadPosition(vr, null);
            //}
            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "LOAD_POSITION_2", out vr))
            //{
            //    Port_onLoadPosition2(vr, null);
            //}
            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "LOAD_POSITION_3", out vr))
            //{
            //    Port_onLoadPosition3(vr, null);
            //}
            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "LOAD_POSITION_4", out vr))
            //{
            //    Port_onLoadPosition4(vr, null);
            //}
            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "LOAD_POSITION_5", out vr))
            //{
            //    Port_onLoadPosition5(vr, null);
            //}
            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "LOAD_POSITION_6", out vr))
            //{
            //    Port_onLoadPosition6(vr, null);
            //}
            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "LOAD_POSITION_7", out vr))
            //{
            //    Port_onLoadPosition7(vr, null);
            //}
            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "OP_RUN", out vr))
            //{
            //    Port_onInService(vr, null);
            //}
            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "OP_DOWN", out vr))
            //{
            //    Port_onOutOfService(vr, null);
            //}
            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "AGV_PORT_READY", out vr))
            //{
            //    Port_onAGVPortReadyStatusChange(vr, null);
            //}
            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "IS_CST_PRESENCE", out vr))
            //{
            //    Port_onCstPresenceStatusChange(vr, null);
            //}
            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "OP_ERROR", out vr))
            //{
            //    Port_onAlarmHppened(vr, null);
            //}
            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "IS_AGV_MODE", out vr))
            //{
            //    Port_onChangeToAGVMode(vr, null);
            //}
            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "IS_MGV_MODE", out vr))
            //{
            //    Port_onChangeToMGVMode(vr, null);
            //}
            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "CST_PRRESENCE_MISMATCH", out vr))
            //{
            //    Port_onCSTPresenceMismatch(vr, null);
            //}
            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "FIRE_ALARM", out vr))
            //{
            //    Port_onFireAlarm(vr, null);
            //}

            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "PORTALLINFO", out vr))
            //{
            //    PortInfoChange(vr, null);
            //}
            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "CIM_ON", out vr))
            //{
            //    Port_onCIM_ON(vr, null);
            //}
            //if (bcfApp.tryGetReadValueEventstring(port.EqptObjectCate, port.PORT_ID, "PreLoadOK", out vr))
            //{
            //    Port_onPreLoadOK(vr, null);
            //}

            if (bcfApp.tryGetReadValueEventstring(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "PORTALLINFO", out vr))
            {
                PortInfoChange(vr, null);
            }
            if (bcfApp.tryGetReadValueEventstring(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "NOW_INPUT_MODE", out vr))
            {
                Port_onDirectionStatusChangeToInput(vr, null);
            }
            if (bcfApp.tryGetReadValueEventstring(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "NOW_OUTPUT_MODE", out vr))
            {
                Port_onDirectionStatusChangeToOutput(vr, null);
            }
            if (bcfApp.tryGetReadValueEventstring(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "OP_RUN", out vr))
            {
                Port_onInService(vr, null);
            }
            if (bcfApp.tryGetReadValueEventstring(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "WAIT_IN", out vr))
            {
                Port_onWaitIn(vr, null);
            }
            if (bcfApp.tryGetReadValueEventstring(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "ERROR_CODE", out vr))
            {
                Port_ErrorCodeChanged(vr, null);
            }
        }

        public virtual string getIdentityKey()
        {
            return this.GetType().Name;
        }

        public virtual void setContext(BaseEQObject baseEQ)
        {
            this.PortStation = baseEQ as APORTSTATION;
        }

        public virtual void unRegisterEvent()
        {
            //not implement
        }
        public virtual void Port_onModeChangable(object sender, ValueChangedEventArgs args)
        {
            var function = scApp.getFunBaseObj<PortPLCInfo>(PortStation.PORT_ID) as PortPLCInfo;
            try
            {
                //1.建立各個Function物件
                function.Timestamp = DateTime.Now;
                function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);

                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());


                //scApp.TransferService.TransferServiceLogger.Info
                //(
                //    DateTime.Now.ToString("HH:mm:ss.fff ") +
                //    "PLC >> OHB|Port_onWaitInOut"
                //    + " PORT_ID:" + port.PORT_ID
                //    + " Port_onModeChangable:" + function.IsModeChangable
                //);

                //if (scApp.TransferService.GetIgnoreModeChange(function))
                //{
                //    return;
                //}

                //if (function.IsModeChangable)
                //{
                //    Console.WriteLine("IsModeChangable");
                //    //TODO: wait in

                //    if (function.OpAutoMode)
                //    {
                //        scApp.TransferService.PLC_ReportPortIsModeChangable(function, "PLC");
                //    }
                //    else
                //    {
                //        scApp.TransferService.TransferServiceLogger.Info
                //        (
                //            DateTime.Now.ToString("HH:mm:ss.fff ") +
                //            "PLC >> OHB|Port 狀態錯誤，不能報 IsModeChangable "
                //            + " PORT_ID:" + port.PORT_ID
                //            + " Run:" + function.OpAutoMode
                //        );
                //    }
                //}

            }
            catch (Exception ex)
            {
                scApp.TransferService.TransferServiceLogger.Error(ex, "Port_onModeChangable");
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCInfo>(function);
            }
        }
        public virtual void Port_onWaitIn(object sender, ValueChangedEventArgs args)
        {
            var function = scApp.getFunBaseObj<PortPLCInfo>(PortStation.PORT_ID) as PortPLCInfo;
            try
            {
                //1.建立各個Function物件
                function.Timestamp = DateTime.Now;
                function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                scApp.TransferService.TransferServiceLogger.Info
                (
                    DateTime.Now.ToString("HH:mm:ss.fff ") +
                    "PLC >> OHB|Port_onWaitInOut"
                    + " PORT_ID:" + PortStation.PORT_ID
                    + " PortWaitIn:" + function.PortWaitIn
                );

                if (function.PortWaitIn)
                {
                    if (function.OpRun)
                    {
                        //if(string.IsNullOrEmpty(function.RFIDCassetteID))//如果RFID為空
                        //{
                        //    Thread.SpinWait(200);
                        //    function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);//重新抓值


                        //}
                        scApp.TransferService.PLC_ReportPortWaitIn(function, "PLC");
                    }
                    else
                    {
                        scApp.TransferService.TransferServiceLogger.Info
                        (
                            DateTime.Now.ToString("HH:mm:ss.fff ") +
                            "PLC >> OHB|Port 狀態錯誤，不能報 WaitIn "
                            + " PORT_ID:" + PortStation.PORT_ID
                            + " Run:" + function.OpRun
                        );
                    }
                }

            }
            catch (Exception ex)
            {
                scApp.TransferService.TransferServiceLogger.Error(ex, "Port_onWaitIn");
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCInfo>(function);
            }
        }
        public virtual void Port_onWaitOut(object sender, ValueChangedEventArgs args)
        {
            var function = scApp.getFunBaseObj<PortPLCInfo>(PortStation.PORT_ID) as PortPLCInfo;
            try
            {
                //1.建立各個Function物件
                function.Timestamp = DateTime.Now;
                function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                scApp.TransferService.TransferServiceLogger.Info
                (
                    DateTime.Now.ToString("HH:mm:ss.fff ") +
                    "PLC >> OHB|Port_onWaitInOut"
                    + " PORT_ID:" + PortStation.PORT_ID
                    + " PortWaitOut:" + function.PortWaitOut
                );

                //AT&S不在上報Waitout時報Wait out給MCS，改在在席訊號on時上報。
                //if (function.PortWaitOut && function.OpRun && function.IsInputMode)
                //{
                //    ACARRIER datainfo = new ACARRIER();
                //    datainfo.ID = function.RFIDCassetteID;        //填CSTID
                //    datainfo.LOCATION = PortStation.PORT_ID.Trim();  //填Port 名稱

                //    scApp.TransferService.PortPositionWaitOut(datainfo, 1);
                //}
                //else
                //{

                //}
            }
            catch (Exception ex)
            {
                scApp.TransferService.TransferServiceLogger.Error(ex, "Port_onWaitOut");
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCInfo>(function);
            }
        }
        public virtual void Port_onDirectionStatusChangeToInput(object sender, ValueChangedEventArgs args)
        {
            var function = scApp.getFunBaseObj<PortPLCInfo>(PortStation.PORT_ID) as PortPLCInfo;
            try
            {
                //1.建立各個Function物件
                function.Timestamp = DateTime.Now;
                function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());


                //if (scApp.TransferService.GetIgnoreModeChange(function))
                //{
                //    return;
                //}
                PortStation.IsInPutMode = function.IsInputMode;
                if (function.IsInputMode)
                {
                    Port_ChangeToInput(false);
                    scApp.PortStationBLL.OperateDB.updatePortDir(PortStation.PORT_ID, 0);
                    scApp.TransferService.portInMode(PortStation.PORT_ID);
                    //scApp.TransferService.ReportPortType(function.EQ_ID, E_PortType.In, "PLC");

                    //bool cstDelete = scApp.TransferService.portTypeChangeOK_CVPort_CstRemove;
                    string log = "PLC IsInputMode: " + function.IsInputMode.ToString();
                    scApp.TransferService.DeleteOHCVPortCst(function.EQ_ID, log);


                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCInfo>(function);
            }
        }

        public virtual void Port_onDirectionStatusChangeToOutput(object sender, ValueChangedEventArgs args)
        {
            var function = scApp.getFunBaseObj<PortPLCInfo>(PortStation.PORT_ID) as PortPLCInfo;
            try
            {
                //1.建立各個Function物件
                function.Timestamp = DateTime.Now;
                function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                //if (scApp.TransferService.GetIgnoreModeChange(function))
                //{
                //    return;
                //}
                PortStation.IsOutPutMode = function.IsOutputMode;

                if (function.IsOutputMode)
                {
                    Port_ChangeToOutput(false);
                    scApp.PortStationBLL.OperateDB.updatePortDir(PortStation.PORT_ID, 1);
                    //scApp.TransferService.portInMode(PortStation.PORT_ID);
                    scApp.TransferService.portOutMode(PortStation.PORT_ID);

                    //scApp.TransferService.ReportPortType(function.EQ_ID, E_PortType.Out, "PLC");

                    //bool cstDelete = scApp.TransferService.portTypeChangeOK_CVPort_CstRemove;
                    //string log = "PLC IsOutputMode:" + function.IsOutputMode.ToString();

                    //if (cstDelete)
                    //{
                    string log = "PLC IsOutputMode: " + function.IsOutputMode.ToString();
                    scApp.TransferService.DeleteOHCVPortCst(function.EQ_ID, log);
                    //}

                    //if (scApp.TransferService.isUnitType(function.EQ_ID, Service.UnitType.AGV))
                    //{
                    //    scApp.TransferService.PLC_AGV_Station(function, log);
                    //}
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCInfo>(function);
            }
        }

        public PortPLCInfo GetPortValue()
        {

            PortPLCInfo portData = scApp.getFunBaseObj<PortPLCInfo>(PortStation.PORT_ID) as PortPLCInfo;
            portData.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
            return portData;
        }

        public void Port_onBarcodeReadDone(object sender, ValueChangedEventArgs e)
        {
            var function = scApp.getFunBaseObj<PortPLCInfo>(PortStation.PORT_ID) as PortPLCInfo;
            try
            {
                //1.建立各個Function物件
                function.Timestamp = DateTime.Now;
                function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                //if (scApp.TransferService.GetIgnoreModeChange(function))
                //{
                //    return;
                //}

                //if (function.BCRReadDone)
                //{
                //    //get box ID here
                //    //box ID is stored in function.BoxID
                //    if (scApp.TransferService.isUnitType(function.EQ_ID, Service.UnitType.AGV))
                //    {
                //        scApp.TransferService.TransferServiceLogger.Info
                //        (
                //            DateTime.Now.ToString("HH:mm:ss.fff ") +
                //            "PLC >> PLC|Port_onBarcodeReadDone  PortName: " + function.EQ_ID
                //            + " BCRReadDone: " + function.BCRReadDone
                //            + " CassetteID: " + function.CassetteID
                //            + " BoxID: " + function.BoxID
                //            + " IsReadyToLoad: " + function.IsReadyToLoad
                //            + " IsReadyToUnload:" + function.IsReadyToUnload
                //            + " IsCSTPresence:" + function.IsCSTPresence
                //            + " PortWaitOut:" + function.PortWaitOut
                //        );
                //    }
                //}
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCInfo>(function);
            }
        }

        private void Port_onCSTRemoveCheck(object sender, ValueChangedEventArgs e)
        {
            var function = scApp.getFunBaseObj<PortPLCInfo>(PortStation.PORT_ID) as PortPLCInfo;
            try
            {
                //1.建立各個Function物件
                function.Timestamp = DateTime.Now;
                function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                //if (scApp.TransferService.GetIgnoreModeChange(function))
                //{
                //    return;
                //}

                //if (function.CstRemoveCheck)
                //{
                //    if (scApp.TransferService.isUnitType(function.EQ_ID, Service.UnitType.AGV))
                //    {
                //        CassetteData datainfo = new CassetteData();
                //        datainfo.CSTID = function.CassetteID.Trim();        //填CSTID
                //        datainfo.BOXID = function.BoxID.Trim();        //填BOXID
                //        datainfo.Carrier_LOC = function.EQ_ID.Trim();  //填Port 名稱
                //        scApp.TransferService.PortCarrierRemoved(datainfo, function.IsAGVMode, "PortRemove");
                //    }
                //}
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCInfo>(function);
            }
        }

        private void Port_onTransferComplete(object sender, ValueChangedEventArgs e)
        {
            var function = scApp.getFunBaseObj<PortPLCInfo>(PortStation.PORT_ID) as PortPLCInfo;
            try
            {
                //1.建立各個Function物件
                function.Timestamp = DateTime.Now;
                function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);

                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                //if (function.IsTransferComplete)
                //{
                //    //TODO
                //    Console.WriteLine("wait out LP");
                //    //TODO: wait out

                //    //CassetteData datainfo = new CassetteData();
                //    //datainfo.CSTID = function.CassetteID;        //填CSTID
                //    //datainfo.BOXID = function.BoxID.Trim();        //填BOXID
                //    //datainfo.Carrier_LOC = port.PORT_ID.Trim();  //填Port 名稱

                //    //scApp.TransferService.PortTransferCompleted(datainfo);
                //}
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCInfo>(function);
            }
        }
        #region LoadPosition
        private void Port_onLoadPosition(object sender, ValueChangedEventArgs e)
        {
            var function = scApp.getFunBaseObj<PortPLCInfo>(PortStation.PORT_ID) as PortPLCInfo;
            try
            {
                //1.建立各個Function物件
                function.Timestamp = DateTime.Now;
                function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                scApp.TransferService.TransferServiceLogger.Info
                (
                    DateTime.Now.ToString("HH:mm:ss.fff ") +
                    "PLC >> PLC|PortName: " + function.EQ_ID
                    + " LoadPosition1: " + function.LoadPosition1
                    + " LoadPositionCST1:" + function.LoadPositionCSTID1
                );

                #region retry retrive data
                //由於PLC在席亮時有可能CST ID還沒被填上去。 所以遇到該情況需要延時再次取得一遍
                if (function.LoadPosition1 && string.IsNullOrWhiteSpace(function.LoadPositionCSTID1))
                {
                    scApp.TransferService.TransferServiceLogger.Warn
                    (
                        DateTime.Now.ToString("HH:mm:ss.fff ") +
                        "PLC >> PLC|PortName: " + function.EQ_ID + " LoadPosition1 signal is on,but CST ID is Empty. wait 400ms then retrival Data again."
                    );
                    Thread.Sleep(400);//等待400毫秒

                    function.Timestamp = DateTime.Now;
                    function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                    NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                    scApp.TransferService.TransferServiceLogger.Info
                    (
                        DateTime.Now.ToString("HH:mm:ss.fff ") +
                        "PLC >> PLC|PortName: " + function.EQ_ID
                        + "Retrival Data Second Time"
                        + " LoadPosition1: " + function.LoadPosition1
                        + " LoadPositionCST1:" + function.LoadPositionCSTID1
                    );
                    if (function.LoadPosition1 && string.IsNullOrWhiteSpace(function.LoadPositionCSTID1))
                    {
                        scApp.TransferService.TransferServiceLogger.Warn
                        (
                            DateTime.Now.ToString("HH:mm:ss.fff ") +
                            "PLC >> PLC|PortName: " + function.EQ_ID + "Retrival Data Second Time LoadPosition1 signal is on,but CST ID still Empty. wait 400ms again then retrival Data again."
                        );

                        Thread.Sleep(400);//等待400毫秒

                        function.Timestamp = DateTime.Now;
                        function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                        NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());
                        if (function.LoadPosition1 && string.IsNullOrWhiteSpace(function.LoadPositionCSTID1))
                        {
                            scApp.TransferService.TransferServiceLogger.Info
                            (
                                DateTime.Now.ToString("HH:mm:ss.fff ") +
                                "PLC >> PLC|PortName: " + function.EQ_ID
                                + "Retrival Data Third Time"
                                + " LoadPosition1: " + function.LoadPosition1
                                + " LoadPositionCST1:" + function.LoadPositionCSTID1
                            );

                            //return;
//                            scApp.LineService.ProcessAlarmReport(PortStation.PORT_ID, AlarmBLL.PORT_WAITOUT_ID_EMPTY, ProtocolFormat.OHTMessage.ErrorStatus.ErrSet,
//$"Port Status:[{PortStation.PORT_ID}] is Down.");
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        scApp.TransferService.TransferServiceLogger.Info
                        (
                            DateTime.Now.ToString("HH:mm:ss.fff ") +
                            "PLC >> PLC|PortName: " + function.EQ_ID
                            + " Retry Retrival Data End"
                            + " LoadPosition1: " + function.LoadPosition1
                            + " LoadPositionCST1:" + function.LoadPositionCSTID1
                        );
                    }

                }
                #endregion retry retrive data

                APORTSTATION port = scApp.getEQObjCacheManager().getPortStation(PortStation.PORT_ID);
                port.IsCSTPresenceLoc1 = function.LoadPosition1;
                if (function.LoadPosition1)
                {
                    port.CSTPresenceID1 = function.LoadPositionCSTID1;
                    if (function.IsOutputMode && function.OpRun)
                    {
                        if (port.stageCount == 1)//該Port只有一節，所以第一節就是最後一節，要上報Waitout在該PortID
                        {
                            ACARRIER datainfo = new ACARRIER();
                            if (string.IsNullOrWhiteSpace(function.LoadPositionCSTID1)) //取不到ID但以是最後一節Stage就找出最先放入Port的CST
                            {
                                List<ACARRIER> carrier_list = scApp.CarrierBLL.db.loadCassetteListByLoc( port.PORT_ID);
                                ACARRIER first_carrier = carrier_list.Where(cst => cst.WaitOutLPDT == null)
                                       .OrderBy(cst => cst.StoreDT).FirstOrDefault();
                                if (first_carrier != null)
                                {
                                    port.CSTPresenceID1 = first_carrier.ID.Trim();
                                    datainfo.ID = first_carrier.ID.Trim();
                                    datainfo.LOCATION = port.PORT_ID.Trim();  //填Port 名稱
                                    scApp.TransferService.PortPositionWaitOut(datainfo, 1);
                                }
                                else
                                {
                                    scApp.TransferService.TransferServiceLogger.Info
                                    (
                                        DateTime.Now.ToString("HH:mm:ss.fff ") +
                                        "PLC >> PLC|PortName: " + function.EQ_ID
                                        + "Search DB could not found Carrier Data for report."
                                        + " LoadPosition1: " + function.LoadPosition1
                                        + " LoadPositionCST1:" + function.LoadPositionCSTID1
                                    );
                                }
                                return;
                            }
                            ACARRIER unk_carrier = scApp.CarrierBLL.db.loadCassetteDataStartWithAndLoc(function.LoadPositionCSTID1, port.PORT_ID);
                            if (unk_carrier != null)
                            {
                                datainfo.ID = unk_carrier.ID.Trim();        //填CSTID
                            }
                            else if (port.AssignCSTID.StartsWith(function.LoadPositionCSTID1.Trim()))//只有在第一節使用
                            {
                                datainfo.ID = port.AssignCSTID.Trim();        //填CSTID
                            }
                            else
                            {
                                datainfo.ID = function.LoadPositionCSTID1;        //填CSTID
                            }

                            datainfo.LOCATION = port.PORT_ID.Trim();  //填Port 名稱
                            scApp.TransferService.PortPositionWaitOut(datainfo, 1);
                        }
                        else //不是最後一節，上報在OP側
                        {
                            if (string.IsNullOrWhiteSpace(function.LoadPositionCSTID1)) return;//找不到ID且不是最後一節Stage就不予處理
                            ACARRIER datainfo = new ACARRIER();
                            ACARRIER unk_carrier = scApp.CarrierBLL.db.loadCassetteDataStartWithAndLoc(function.LoadPositionCSTID1, port.PORT_ID);
                            if (unk_carrier != null)
                            {
                                datainfo.ID = unk_carrier.ID.Trim();        //填CSTID
                            }
                            else if (port.AssignCSTID.StartsWith(function.LoadPositionCSTID1.Trim()))//只有在第一節使用
                            {
                                datainfo.ID = port.AssignCSTID.Trim();        //填CSTID
                            }
                            else
                            {
                                datainfo.ID = function.LoadPositionCSTID1;        //填CSTID
                            }
                            datainfo.LOCATION = port.PORT_ID.Trim() + "OP";  //填Port 名稱 +"OP"
                            scApp.TransferService.PortPositionWaitOut(datainfo, 1);
                        }
                    }
                }
                else
                {
                    string pre_cst_id = port.CSTPresenceID1;
                    port.CSTPresenceID1 = string.Empty;
                    if (string.IsNullOrWhiteSpace(pre_cst_id)) return;//找不到ID就不予處理
                    if (function.IsOutputMode && function.OpRun)
                    {
                        if (port.stageCount == 1)//該Port只有一節，所以第一節就是最後一節，要上報CarrierRemove
                        {
                            ACARRIER datainfo = new ACARRIER();
                            ACARRIER unk_carrier = scApp.CarrierBLL.db.loadCassetteDataStartWithAndLoc(pre_cst_id, port.PORT_ID);
                            if (unk_carrier != null)
                            {
                                datainfo.ID = unk_carrier.ID.Trim();        //填CSTID
                            }
                            else if (port.AssignCSTID.StartsWith(function.LoadPositionCSTID1.Trim()))//只有在第一節使用
                            {
                                datainfo.ID = port.AssignCSTID.Trim();        //填CSTID
                            }
                            else
                            {
                                datainfo.ID = pre_cst_id;        //填CSTID
                            }
                            datainfo.LOCATION = port.PORT_ID.Trim();  //填Port 名稱
                            scApp.ReportBLL.ReportCarrierRemovedFromPort(datainfo, "2", null);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCInfo>(function);
            }
        }
        private void Port_onLoadPosition2(object sender, ValueChangedEventArgs e)
        {
            var function = scApp.getFunBaseObj<PortPLCInfo>(PortStation.PORT_ID) as PortPLCInfo;
            try
            {
                //1.建立各個Function物件
                function.Timestamp = DateTime.Now;
                function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                scApp.TransferService.TransferServiceLogger.Info
                (
                    DateTime.Now.ToString("HH:mm:ss.fff ") +
                    "PLC >> PLC|PortName: " + function.EQ_ID
                    + " LoadPosition2: " + function.LoadPosition2
                    + " LoadPositionCST2:" + function.LoadPositionCSTID2
                );
                #region retry retrive data
                //由於PLC在席亮時有可能CST ID還沒被填上去。 所以遇到該情況需要延時再次取得一遍
                if (function.LoadPosition2 && string.IsNullOrWhiteSpace(function.LoadPositionCSTID2))
                {
                    scApp.TransferService.TransferServiceLogger.Warn
                    (
                        DateTime.Now.ToString("HH:mm:ss.fff ") +
                        "PLC >> PLC|PortName: " + function.EQ_ID + " LoadPosition2 signal is on,but CST ID is Empty. wait 400ms then retrival Data again."
                    );
                    Thread.Sleep(400);//等待400毫秒

                    function.Timestamp = DateTime.Now;
                    function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                    NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                    scApp.TransferService.TransferServiceLogger.Info
                    (
                        DateTime.Now.ToString("HH:mm:ss.fff ") +
                        "PLC >> PLC|PortName: " + function.EQ_ID
                        + "Retrival Data Second Time"
                        + " LoadPosition2: " + function.LoadPosition2
                        + " LoadPositionCST2:" + function.LoadPositionCSTID2
                    );
                    if (function.LoadPosition2 && string.IsNullOrWhiteSpace(function.LoadPositionCSTID2))
                    {
                        scApp.TransferService.TransferServiceLogger.Warn
                        (
                            DateTime.Now.ToString("HH:mm:ss.fff ") +
                            "PLC >> PLC|PortName: " + function.EQ_ID + "Retrival Data Second Time LoadPosition2 signal is on,but CST ID still Empty. wait 400ms again then retrival Data again."
                        );

                        Thread.Sleep(400);//等待400毫秒

                        function.Timestamp = DateTime.Now;
                        function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                        NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());
                        if (function.LoadPosition2 && string.IsNullOrWhiteSpace(function.LoadPositionCSTID2))
                        {
                            scApp.TransferService.TransferServiceLogger.Info
                            (
                                DateTime.Now.ToString("HH:mm:ss.fff ") +
                                "PLC >> PLC|PortName: " + function.EQ_ID
                                + "Retrival Data Third Time"
                                + " LoadPosition2: " + function.LoadPosition2
                                + " LoadPositionCST2:" + function.LoadPositionCSTID2
                            );

                            //return;

                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        scApp.TransferService.TransferServiceLogger.Info
                        (
                            DateTime.Now.ToString("HH:mm:ss.fff ") +
                            "PLC >> PLC|PortName: " + function.EQ_ID
                            + " Retry Retrival Data End"
                            + " LoadPosition2 " + function.LoadPosition2
                            + " LoadPositionCST2:" + function.LoadPositionCSTID2
                        );
                    }

                }
                #endregion retry retrive data

                APORTSTATION port = scApp.getEQObjCacheManager().getPortStation(PortStation.PORT_ID);
                port.IsCSTPresenceLoc2 = function.LoadPosition2;
                if (function.LoadPosition2)
                {
                    port.CSTPresenceID2 = function.LoadPositionCSTID2;
                    if (function.IsOutputMode && function.OpRun)
                    {
                        if (port.stageCount == 2)//該Port只有兩節，所以第二節就是最後一節，要上報Waitout在該PortID
                        {
                            ACARRIER datainfo = new ACARRIER();
                            if (string.IsNullOrWhiteSpace(function.LoadPositionCSTID2)) //取不到ID但以是最後一節Stage就找出最先放入Port的CST
                            {
                                List<ACARRIER> carrier_list = scApp.CarrierBLL.db.loadCassetteListByLoc(port.PORT_ID);
                                ACARRIER first_carrier = carrier_list.Where(cst => cst.WaitOutLPDT == null)
                                       .OrderBy(cst => cst.StoreDT).FirstOrDefault();
                                if (first_carrier != null)
                                {
                                    port.CSTPresenceID2 = first_carrier.ID.Trim();
                                    datainfo.ID = first_carrier.ID.Trim();
                                    datainfo.LOCATION = port.PORT_ID.Trim();  //填Port 名稱
                                    scApp.TransferService.PortPositionWaitOut(datainfo, 2);
                                }
                                else
                                {
                                    scApp.TransferService.TransferServiceLogger.Info
                                    (
                                        DateTime.Now.ToString("HH:mm:ss.fff ") +
                                        "PLC >> PLC|PortName: " + function.EQ_ID
                                        + "Search DB could not found Carrier Data for report."
                                        + " LoadPosition2: " + function.LoadPosition2
                                        + " LoadPositionCST2:" + function.LoadPositionCSTID2
                                    );
                                }
                                return;
                            }
                            ACARRIER unk_carrier = scApp.CarrierBLL.db.loadCassetteDataStartWithAndLoc(function.LoadPositionCSTID2, port.PORT_ID);
                            if (unk_carrier != null)
                            {
                                datainfo.ID = unk_carrier.ID.Trim();        //填CSTID
                            }
                            else
                            {
                                datainfo.ID = function.LoadPositionCSTID2;        //填CSTID
                            }
                            datainfo.LOCATION = port.PORT_ID.Trim();  //填Port 名稱
                            scApp.TransferService.PortPositionWaitOut(datainfo, 2);
                        }
                        else //不是最後一節，上報在Port 名稱 +"BP1"
                        {
                            if (string.IsNullOrWhiteSpace(function.LoadPositionCSTID2)) return;//找不到ID且不是最後一節Stage就不予處理
                            ACARRIER datainfo = new ACARRIER();
                            ACARRIER unk_carrier = scApp.CarrierBLL.db.loadCassetteDataStartWithAndLoc(function.LoadPositionCSTID2, port.PORT_ID);
                            if (unk_carrier != null)
                            {
                                datainfo.ID = unk_carrier.ID.Trim();        //填CSTID
                            }
                            else
                            {
                                datainfo.ID = function.LoadPositionCSTID2;        //填CSTID
                            }
                            datainfo.LOCATION = port.PORT_ID.Trim() + "BP1";  //填Port 名稱 +"BP1"
                            scApp.TransferService.PortPositionWaitOut(datainfo, 2);
                        }
                    }
                }
                else
                {
                    string pre_cst_id = port.CSTPresenceID2;
                    port.CSTPresenceID2 = string.Empty;
                    if (string.IsNullOrWhiteSpace(pre_cst_id)) return;//找不到ID就不予處理
                    if (function.IsOutputMode && function.OpRun)
                    {
                        if (port.stageCount == 2)//該Port只有兩節，所以第二節就是最後一節，要上報CarrierRemove
                        {
                            ACARRIER datainfo = new ACARRIER();
                            ACARRIER unk_carrier = scApp.CarrierBLL.db.loadCassetteDataStartWithAndLoc(pre_cst_id, port.PORT_ID);
                            if (unk_carrier != null)
                            {
                                datainfo.ID = unk_carrier.ID.Trim();        //填CSTID
                            }
                            else
                            {
                                datainfo.ID = pre_cst_id;        //填CSTID
                            }
                            datainfo.LOCATION = port.PORT_ID.Trim();  //填Port 名稱
                            scApp.ReportBLL.ReportCarrierRemovedFromPort(datainfo, "2", null);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCInfo>(function);
            }
        }
        private void Port_onLoadPosition3(object sender, ValueChangedEventArgs e)
        {
            var function = scApp.getFunBaseObj<PortPLCInfo>(PortStation.PORT_ID) as PortPLCInfo;
            try
            {
                //1.建立各個Function物件
                function.Timestamp = DateTime.Now;
                function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                scApp.TransferService.TransferServiceLogger.Info
                (
                    DateTime.Now.ToString("HH:mm:ss.fff ") +
                    "PLC >> PLC|PortName: " + function.EQ_ID
                    + " LoadPosition3: " + function.LoadPosition3
                    + " LoadPositionCST3:" + function.LoadPositionCSTID3
                );

                #region retry retrive data
                //由於PLC在席亮時有可能CST ID還沒被填上去。 所以遇到該情況需要延時再次取得一遍
                if (function.LoadPosition3 && string.IsNullOrWhiteSpace(function.LoadPositionCSTID3))
                {
                    scApp.TransferService.TransferServiceLogger.Warn
                    (
                        DateTime.Now.ToString("HH:mm:ss.fff ") +
                        "PLC >> PLC|PortName: " + function.EQ_ID + " LoadPosition3 signal is on,but CST ID is Empty. wait 400ms then retrival Data again."
                    );
                    Thread.Sleep(400);//等待400毫秒

                    function.Timestamp = DateTime.Now;
                    function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                    NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                    scApp.TransferService.TransferServiceLogger.Info
                    (
                        DateTime.Now.ToString("HH:mm:ss.fff ") +
                        "PLC >> PLC|PortName: " + function.EQ_ID
                        + "Retrival Data Second Time"
                        + " LoadPosition3: " + function.LoadPosition3
                        + " LoadPositionCST3:" + function.LoadPositionCSTID3
                    );
                    if (function.LoadPosition3 && string.IsNullOrWhiteSpace(function.LoadPositionCSTID3))
                    {
                        scApp.TransferService.TransferServiceLogger.Warn
                        (
                            DateTime.Now.ToString("HH:mm:ss.fff ") +
                            "PLC >> PLC|PortName: " + function.EQ_ID + "Retrival Data Second Time LoadPosition3 signal is on,but CST ID still Empty. wait 400ms again then retrival Data again."
                        );

                        Thread.Sleep(400);//等待400毫秒

                        function.Timestamp = DateTime.Now;
                        function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                        NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());
                        if (function.LoadPosition3 && string.IsNullOrWhiteSpace(function.LoadPositionCSTID3))
                        {
                            scApp.TransferService.TransferServiceLogger.Info
                            (
                                DateTime.Now.ToString("HH:mm:ss.fff ") +
                                "PLC >> PLC|PortName: " + function.EQ_ID
                                + "Retrival Data Third Time"
                                + " LoadPosition3: " + function.LoadPosition3
                                + " LoadPositionCST3:" + function.LoadPositionCSTID3
                            );

                            //return;

                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        scApp.TransferService.TransferServiceLogger.Info
                        (
                            DateTime.Now.ToString("HH:mm:ss.fff ") +
                            "PLC >> PLC|PortName: " + function.EQ_ID
                            + " Retry Retrival Data End"
                            + " LoadPosition3 " + function.LoadPosition3
                            + " LoadPositionCST3:" + function.LoadPositionCSTID3
                        );
                    }

                }
                #endregion retry retrive data

                APORTSTATION port = scApp.getEQObjCacheManager().getPortStation(PortStation.PORT_ID);
                port.IsCSTPresenceLoc3 = function.LoadPosition3;
                if (function.LoadPosition3)
                {
                    port.CSTPresenceID3 = function.LoadPositionCSTID3;
                    if (function.IsOutputMode && function.OpRun)
                    {
                        if (port.stageCount == 3)//該Port只有三節，所以第三節就是最後一節，要上報Waitout在該PortID
                        {
                            ACARRIER datainfo = new ACARRIER();
                            if (string.IsNullOrWhiteSpace(function.LoadPositionCSTID3)) //取不到ID但以是最後一節Stage就找出最先放入Port的CST
                            {
                                List<ACARRIER> carrier_list = scApp.CarrierBLL.db.loadCassetteListByLoc(port.PORT_ID);
                                ACARRIER first_carrier = carrier_list.Where(cst => cst.WaitOutLPDT == null)
                                       .OrderBy(cst => cst.StoreDT).FirstOrDefault();
                                if (first_carrier != null)
                                {
                                    port.CSTPresenceID3 = first_carrier.ID.Trim();
                                    datainfo.ID = first_carrier.ID.Trim();
                                    datainfo.LOCATION = port.PORT_ID.Trim();  //填Port 名稱
                                    scApp.TransferService.PortPositionWaitOut(datainfo, 3);
                                }
                                else
                                {
                                    scApp.TransferService.TransferServiceLogger.Info
                                    (
                                        DateTime.Now.ToString("HH:mm:ss.fff ") +
                                        "PLC >> PLC|PortName: " + function.EQ_ID
                                        + "Search DB could not found Carrier Data for report."
                                        + " LoadPosition3: " + function.LoadPosition3
                                        + " LoadPositionCST3:" + function.LoadPositionCSTID3
                                    );
                                }
                                return;
                            }
                            ACARRIER unk_carrier = scApp.CarrierBLL.db.loadCassetteDataStartWithAndLoc(function.LoadPositionCSTID3, port.PORT_ID);
                            if (unk_carrier != null)
                            {
                                datainfo.ID = unk_carrier.ID.Trim();        //填CSTID
                            }
                            else
                            {
                                datainfo.ID = function.LoadPositionCSTID3;        //填CSTID
                            }
                            datainfo.LOCATION = port.PORT_ID.Trim();  //填Port 名稱
                            scApp.TransferService.PortPositionWaitOut(datainfo, 3);
                        }
                        else //不是最後一節，上報在Port 名稱 +"BP2"
                        {
                            if (string.IsNullOrWhiteSpace(function.LoadPositionCSTID3)) return;//找不到ID且不是最後一節Stage就不予處理
                            ACARRIER datainfo = new ACARRIER();
                            ACARRIER unk_carrier = scApp.CarrierBLL.db.loadCassetteDataStartWithAndLoc(function.LoadPositionCSTID3, port.PORT_ID);
                            if (unk_carrier != null)
                            {
                                datainfo.ID = unk_carrier.ID.Trim();        //填CSTID
                            }
                            else
                            {
                                datainfo.ID = function.LoadPositionCSTID3;        //填CSTID
                            }
                            datainfo.LOCATION = port.PORT_ID.Trim() + "BP2";  //填Port 名稱 +"BP2"
                            scApp.TransferService.PortPositionWaitOut(datainfo, 3);
                        }
                    }
                }
                else
                {
                    string pre_cst_id = port.CSTPresenceID3;
                    port.CSTPresenceID3 = string.Empty;
                    if (string.IsNullOrWhiteSpace(pre_cst_id)) return;//找不到ID就不予處理
                    if (function.IsOutputMode && function.OpRun)
                    {
                        if (port.stageCount == 3)//該Port只有三節，所以第三節就是最後一節，要上報CarrierRemove
                        {
                            ACARRIER datainfo = new ACARRIER();
                            ACARRIER unk_carrier = scApp.CarrierBLL.db.loadCassetteDataStartWithAndLoc(pre_cst_id, port.PORT_ID);
                            if (unk_carrier != null)
                            {
                                datainfo.ID = unk_carrier.ID.Trim();        //填CSTID
                            }
                            else
                            {
                                datainfo.ID = pre_cst_id;        //填CSTID
                            }
                            datainfo.LOCATION = port.PORT_ID.Trim();  //填Port 名稱
                            scApp.ReportBLL.ReportCarrierRemovedFromPort(datainfo, "2", null);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCInfo>(function);
            }
        }
        private void Port_onLoadPosition4(object sender, ValueChangedEventArgs e)
        {
            var function = scApp.getFunBaseObj<PortPLCInfo>(PortStation.PORT_ID) as PortPLCInfo;
            try
            {
                //1.建立各個Function物件
                function.Timestamp = DateTime.Now;
                function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                scApp.TransferService.TransferServiceLogger.Info
                (
                    DateTime.Now.ToString("HH:mm:ss.fff ") +
                    "PLC >> PLC|PortName: " + function.EQ_ID
                    + " LoadPosition4: " + function.LoadPosition4
                    + " LoadPositionCST4:" + function.LoadPositionCSTID4
                );

                #region retry retrive data
                //由於PLC在席亮時有可能CST ID還沒被填上去。 所以遇到該情況需要延時再次取得一遍
                if (function.LoadPosition4 && string.IsNullOrWhiteSpace(function.LoadPositionCSTID4))
                {
                    scApp.TransferService.TransferServiceLogger.Warn
                    (
                        DateTime.Now.ToString("HH:mm:ss.fff ") +
                        "PLC >> PLC|PortName: " + function.EQ_ID + " LoadPosition4 signal is on,but CST ID is Empty. wait 400ms then retrival Data again."
                    );
                    Thread.Sleep(400);//等待400毫秒

                    function.Timestamp = DateTime.Now;
                    function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                    NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                    scApp.TransferService.TransferServiceLogger.Info
                    (
                        DateTime.Now.ToString("HH:mm:ss.fff ") +
                        "PLC >> PLC|PortName: " + function.EQ_ID
                        + "Retrival Data Second Time"
                        + " LoadPosition4: " + function.LoadPosition4
                        + " LoadPositionCST4:" + function.LoadPositionCSTID4
                    );
                    if (function.LoadPosition4 && string.IsNullOrWhiteSpace(function.LoadPositionCSTID4))
                    {
                        scApp.TransferService.TransferServiceLogger.Warn
                        (
                            DateTime.Now.ToString("HH:mm:ss.fff ") +
                            "PLC >> PLC|PortName: " + function.EQ_ID + "Retrival Data Second Time LoadPosition4 signal is on,but CST ID still Empty. wait 400ms again then retrival Data again."
                        );

                        Thread.Sleep(400);//等待400毫秒

                        function.Timestamp = DateTime.Now;
                        function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                        NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());
                        if (function.LoadPosition4 && string.IsNullOrWhiteSpace(function.LoadPositionCSTID4))
                        {
                            scApp.TransferService.TransferServiceLogger.Info
                            (
                                DateTime.Now.ToString("HH:mm:ss.fff ") +
                                "PLC >> PLC|PortName: " + function.EQ_ID
                                + "Retrival Data Third Time"
                                + " LoadPosition4: " + function.LoadPosition4
                                + " LoadPositionCST4:" + function.LoadPositionCSTID4
                            );

                            //return;

                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        scApp.TransferService.TransferServiceLogger.Info
                        (
                            DateTime.Now.ToString("HH:mm:ss.fff ") +
                            "PLC >> PLC|PortName: " + function.EQ_ID
                            + " Retry Retrival Data End"
                            + " LoadPosition4 " + function.LoadPosition4
                            + " LoadPositionCST4:" + function.LoadPositionCSTID4
                        );
                    }

                }
                #endregion retry retrive data
                APORTSTATION port = scApp.getEQObjCacheManager().getPortStation(PortStation.PORT_ID);
                port.IsCSTPresenceLoc4 = function.LoadPosition4;
                if (function.LoadPosition4)
                {
                    port.CSTPresenceID4 = function.LoadPositionCSTID4;
                    if (function.IsOutputMode && function.OpRun)
                    {
                        if (port.stageCount == 4)//該Port只有四節，所以第四節就是最後一節，要上報Waitout在該PortID
                        {
                            ACARRIER datainfo = new ACARRIER();
                            if (string.IsNullOrWhiteSpace(function.LoadPositionCSTID4)) //取不到ID但以是最後一節Stage就找出最先放入Port的CST
                            {
                                List<ACARRIER> carrier_list = scApp.CarrierBLL.db.loadCassetteListByLoc(port.PORT_ID);
                                ACARRIER first_carrier = carrier_list.Where(cst => cst.WaitOutLPDT == null)
                                       .OrderBy(cst => cst.StoreDT).FirstOrDefault();
                                if (first_carrier != null)
                                {
                                    port.CSTPresenceID4 = first_carrier.ID.Trim();
                                    datainfo.ID = first_carrier.ID.Trim();
                                    datainfo.LOCATION = port.PORT_ID.Trim();  //填Port 名稱
                                    scApp.TransferService.PortPositionWaitOut(datainfo, 4);
                                }
                                else
                                {
                                    scApp.TransferService.TransferServiceLogger.Info
                                    (
                                        DateTime.Now.ToString("HH:mm:ss.fff ") +
                                        "PLC >> PLC|PortName: " + function.EQ_ID
                                        + "Search DB could not found Carrier Data for report."
                                        + " LoadPosition4: " + function.LoadPosition4
                                        + " LoadPositionCST4:" + function.LoadPositionCSTID4
                                    );
                                }
                                return;
                            }
                            ACARRIER unk_carrier = scApp.CarrierBLL.db.loadCassetteDataStartWithAndLoc(function.LoadPositionCSTID4, port.PORT_ID);
                            if (unk_carrier != null)
                            {
                                datainfo.ID = unk_carrier.ID.Trim();        //填CSTID
                            }
                            else
                            {
                                datainfo.ID = function.LoadPositionCSTID4;        //填CSTID
                            }
                            datainfo.LOCATION = port.PORT_ID.Trim();  //填Port 名稱
                            scApp.TransferService.PortPositionWaitOut(datainfo, 4);
                        }
                        else //不是最後一節，上報在Port 名稱 +"BP3"
                        {
                            if (string.IsNullOrWhiteSpace(function.LoadPositionCSTID4)) return;//找不到ID且不是最後一節Stage就不予處理
                            ACARRIER datainfo = new ACARRIER();
                            ACARRIER unk_carrier = scApp.CarrierBLL.db.loadCassetteDataStartWithAndLoc(function.LoadPositionCSTID4, port.PORT_ID);
                            if (unk_carrier != null)
                            {
                                datainfo.ID = unk_carrier.ID.Trim();        //填CSTID
                            }
                            else
                            {
                                datainfo.ID = function.LoadPositionCSTID4;        //填CSTID
                            }
                            datainfo.LOCATION = port.PORT_ID.Trim() + "BP3";  //填Port 名稱 +"BP3"
                            scApp.TransferService.PortPositionWaitOut(datainfo, 4);
                        }
                    }
                }
                else
                {
                    string pre_cst_id = port.CSTPresenceID4;
                    port.CSTPresenceID4 = string.Empty;
                    if (string.IsNullOrWhiteSpace(pre_cst_id)) return;//找不到ID就不予處理
                    if (function.IsOutputMode && function.OpRun)
                    {
                        if (port.stageCount == 4)//該Port只有四節，所以第四節就是最後一節，要上報CarrierRemove
                        {
                            ACARRIER datainfo = new ACARRIER();
                            ACARRIER unk_carrier = scApp.CarrierBLL.db.loadCassetteDataStartWithAndLoc(pre_cst_id, port.PORT_ID);
                            if (unk_carrier != null)
                            {
                                datainfo.ID = unk_carrier.ID.Trim();        //填CSTID
                            }
                            else
                            {
                                datainfo.ID = pre_cst_id;        //填CSTID
                            }
                            datainfo.LOCATION = port.PORT_ID.Trim();  //填Port 名稱
                            scApp.ReportBLL.ReportCarrierRemovedFromPort(datainfo, "2", null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCInfo>(function);
            }
        }
        private void Port_onLoadPosition5(object sender, ValueChangedEventArgs e)
        {
            var function = scApp.getFunBaseObj<PortPLCInfo>(PortStation.PORT_ID) as PortPLCInfo;
            try
            {
                //1.建立各個Function物件
                function.Timestamp = DateTime.Now;
                function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                scApp.TransferService.TransferServiceLogger.Info
                (
                    DateTime.Now.ToString("HH:mm:ss.fff ") +
                    "PLC >> PLC|PortName: " + function.EQ_ID
                    + " LoadPosition5: " + function.LoadPosition5
                    + " LoadPositionCST5:" + function.LoadPositionCSTID5
                );

                #region retry retrive data
                //由於PLC在席亮時有可能CST ID還沒被填上去。 所以遇到該情況需要延時再次取得一遍
                if (function.LoadPosition5 && string.IsNullOrWhiteSpace(function.LoadPositionCSTID5))
                {
                    scApp.TransferService.TransferServiceLogger.Warn
                    (
                        DateTime.Now.ToString("HH:mm:ss.fff ") +
                        "PLC >> PLC|PortName: " + function.EQ_ID + " LoadPosition5 signal is on,but CST ID is Empty. wait 400ms then retrival Data again."
                    );
                    Thread.Sleep(400);//等待400毫秒

                    function.Timestamp = DateTime.Now;
                    function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                    NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                    scApp.TransferService.TransferServiceLogger.Info
                    (
                        DateTime.Now.ToString("HH:mm:ss.fff ") +
                        "PLC >> PLC|PortName: " + function.EQ_ID
                        + "Retrival Data Second Time"
                        + " LoadPosition5: " + function.LoadPosition5
                        + " LoadPositionCST5:" + function.LoadPositionCSTID5
                    );
                    if (function.LoadPosition5 && string.IsNullOrWhiteSpace(function.LoadPositionCSTID5))
                    {
                        scApp.TransferService.TransferServiceLogger.Warn
                        (
                            DateTime.Now.ToString("HH:mm:ss.fff ") +
                            "PLC >> PLC|PortName: " + function.EQ_ID + "Retrival Data Second Time LoadPosition5 signal is on,but CST ID still Empty. wait 400ms again then retrival Data again."
                        );

                        Thread.Sleep(400);//等待400毫秒

                        function.Timestamp = DateTime.Now;
                        function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                        NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());
                        if (function.LoadPosition5 && string.IsNullOrWhiteSpace(function.LoadPositionCSTID5))
                        {
                            scApp.TransferService.TransferServiceLogger.Info
                            (
                                DateTime.Now.ToString("HH:mm:ss.fff ") +
                                "PLC >> PLC|PortName: " + function.EQ_ID
                                + "Retrival Data Third Time"
                                + " LoadPosition5: " + function.LoadPosition5
                                + " LoadPositionCST5:" + function.LoadPositionCSTID5
                            );

                            //return;

                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        scApp.TransferService.TransferServiceLogger.Info
                        (
                            DateTime.Now.ToString("HH:mm:ss.fff ") +
                            "PLC >> PLC|PortName: " + function.EQ_ID
                            + " Retry Retrival Data End"
                            + " LoadPosition5 " + function.LoadPosition5
                            + " LoadPositionCST5:" + function.LoadPositionCSTID5
                        );
                    }

                }
                #endregion retry retrive data

                APORTSTATION port = scApp.getEQObjCacheManager().getPortStation(PortStation.PORT_ID);
                port.IsCSTPresenceLoc5 = function.LoadPosition5;
                if (function.LoadPosition5)
                {
                    port.CSTPresenceID5 = function.LoadPositionCSTID5;
                    if (function.IsOutputMode && function.OpRun)
                    {
                        if (port.stageCount == 5)//該Port只有五節，所以第五節就是最後一節，要上報Waitout在該PortID
                        {
                            ACARRIER datainfo = new ACARRIER();
                            if (string.IsNullOrWhiteSpace(function.LoadPositionCSTID5)) //取不到ID但以是最後一節Stage就找出最先放入Port的CST
                            {
                                List<ACARRIER> carrier_list = scApp.CarrierBLL.db.loadCassetteListByLoc(port.PORT_ID);
                                ACARRIER first_carrier = carrier_list.Where(cst => cst.WaitOutLPDT == null)
                                       .OrderBy(cst => cst.StoreDT).FirstOrDefault();
                                if (first_carrier != null)
                                {
                                    port.CSTPresenceID5 = first_carrier.ID.Trim();
                                    datainfo.ID = first_carrier.ID.Trim();
                                    datainfo.LOCATION = port.PORT_ID.Trim();  //填Port 名稱
                                    scApp.TransferService.PortPositionWaitOut(datainfo, 5);
                                }
                                else
                                {
                                    scApp.TransferService.TransferServiceLogger.Info
                                    (
                                        DateTime.Now.ToString("HH:mm:ss.fff ") +
                                        "PLC >> PLC|PortName: " + function.EQ_ID
                                        + "Search DB could not found Carrier Data for report."
                                        + " LoadPosition5: " + function.LoadPosition5
                                        + " LoadPositionCST5:" + function.LoadPositionCSTID5
                                    );
                                }
                                return;
                            }
                            ACARRIER unk_carrier = scApp.CarrierBLL.db.loadCassetteDataStartWithAndLoc(function.LoadPositionCSTID5, port.PORT_ID);
                            if (unk_carrier != null)
                            {
                                datainfo.ID = unk_carrier.ID.Trim();        //填CSTID
                            }
                            else
                            {
                                datainfo.ID = function.LoadPositionCSTID5;        //填CSTID
                            }
                            datainfo.LOCATION = port.PORT_ID.Trim();  //填Port 名稱
                            scApp.TransferService.PortPositionWaitOut(datainfo, 5);
                        }
                        else //不是最後一節，上報在Port 名稱 +"BP3"
                        {
                            if (string.IsNullOrWhiteSpace(function.LoadPositionCSTID5)) return;//找不到ID且不是最後一節Stage就不予處理
                            ACARRIER datainfo = new ACARRIER();
                            ACARRIER unk_carrier = scApp.CarrierBLL.db.loadCassetteDataStartWithAndLoc(function.LoadPositionCSTID5, port.PORT_ID);
                            if (unk_carrier != null)
                            {
                                datainfo.ID = unk_carrier.ID.Trim();        //填CSTID
                            }
                            else
                            {
                                datainfo.ID = function.LoadPositionCSTID5;        //填CSTID
                            }
                            datainfo.LOCATION = port.PORT_ID.Trim() + "BP4";  //填Port 名稱 +"BP4"
                            scApp.TransferService.PortPositionWaitOut(datainfo, 5);
                        }
                    }
                }
                else
                {
                    string pre_cst_id = port.CSTPresenceID5;
                    port.CSTPresenceID5 = string.Empty;
                    if (string.IsNullOrWhiteSpace(pre_cst_id)) return;//找不到ID就不予處理
                    if (function.IsOutputMode && function.OpRun)
                    {
                        if (port.stageCount == 5)//該Port只有五節，所以第五節就是最後一節，要上報CarrierRemove
                        {
                            ACARRIER datainfo = new ACARRIER();
                            ACARRIER unk_carrier = scApp.CarrierBLL.db.loadCassetteDataStartWithAndLoc(pre_cst_id, port.PORT_ID);
                            if (unk_carrier != null)
                            {
                                datainfo.ID = unk_carrier.ID.Trim();        //填CSTID
                            }
                            else
                            {
                                datainfo.ID = pre_cst_id;        //填CSTID
                            }
                            datainfo.LOCATION = port.PORT_ID.Trim();  //填Port 名稱
                            scApp.ReportBLL.ReportCarrierRemovedFromPort(datainfo, "2", null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCInfo>(function);
            }
        }

        #endregion

        //public void Port_WriteCstID(ACARRIER cassetteData)
        //{
        //    var function = scApp.getFunBaseObj<PortPLCControl_CSTID_BOXID>(PortStation.PORT_ID) as PortPLCControl_CSTID_BOXID;
        //    try
        //    {
        //        //1.建立各個Function物件

        //        function.AssignCassetteID = SCUtility.Trim(cassetteData.ID, true);

        //        if (function.AssignCassetteID.Contains("UNK"))
        //        {
        //            function.AssignCassetteID = "ERROR1";
        //        }

        //        function.Write(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
        //        function.Timestamp = DateTime.Now;

        //        //2.write log
        //        //LogManager.GetLogger("com.mirle.ibg3k0.sc.Common.LogHelper").Info(function.ToString());
        //        NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

        //        //3.logical (include db save)
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "Exception");
        //    }
        //    finally
        //    {
        //        scApp.putFunBaseObj<PortPLCControl_CSTID_BOXID>(function);
        //    }
        //}

        #region OHB >> PLC
        public void Port_ChangeToInput(bool isInput)
        {
            //var function = scApp.getFunBaseObj<PortPLCControl>(port.PORT_ID) as PortPLCControl;
            var function = scApp.getFunBaseObj<PortPLCControl_PortInOutModeChange>(PortStation.PORT_ID) as PortPLCControl_PortInOutModeChange;
            try
            {
                //1.建立各個Function物件
                function.PortToInputMode = isInput;
                function.Write(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                function.Timestamp = DateTime.Now;

                //2.write log
                //LogManager.GetLogger("com.mirle.ibg3k0.sc.Common.LogHelper").Info(function.ToString());
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                //3.logical (include db save)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                //scApp.putFunBaseObj<PortPLCControl>(function);
                scApp.putFunBaseObj<PortPLCControl_PortInOutModeChange>(function);
            }
        }

        public void Port_ChangeToOutput(bool isOutput)
        {
            //var function = scApp.getFunBaseObj<PortPLCControl>(port.PORT_ID) as PortPLCControl;
            var function = scApp.getFunBaseObj<PortPLCControl_PortInOutModeChange>(PortStation.PORT_ID) as PortPLCControl_PortInOutModeChange;
            try
            {
                //1.建立各個Function物件
                function.PortToOutputMode = isOutput;
                function.Write(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                function.Timestamp = DateTime.Now;

                //2.write log
                //LogManager.GetLogger("com.mirle.ibg3k0.sc.Common.LogHelper").Info(function.ToString());
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                //3.logical (include db save)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                //scApp.putFunBaseObj<PortPLCControl>(function);
                scApp.putFunBaseObj<PortPLCControl_PortInOutModeChange>(function);
            }
        }
        public void Port_ChangeToOutputTEST(bool isOutput)  //PLC單點控制，參考
        {
            try
            {
                ValueWrite out_mode = bcfApp.getWriteValueEvent(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "CHANGE_TO_OUTPUT_MODE");
                out_mode.setWriteValue(isOutput ? "1" : "0");
                ISMControl.writeDeviceBlock(bcfApp, out_mode);

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
        }

        public void Port_OHCV_Commanding(bool isCmd)    //D6401(?) OHCV Commanding，通知有卡匣要搬過去，流向不能變更意思
        {
            //var function = scApp.getFunBaseObj<PortPLCControl>(port.PORT_ID) as PortPLCControl;
            var function = scApp.getFunBaseObj<PortPLCControl_VehicleCommanding1>(PortStation.PORT_ID) as PortPLCControl_VehicleCommanding1;
            try
            {
                //1.建立各個Function物件
                function.VehicleCommanding1 = isCmd;
                function.Write(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                function.Timestamp = DateTime.Now;

                //2.write log
                //LogManager.GetLogger("com.mirle.ibg3k0.sc.Common.LogHelper").Info(function.ToString());
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                //3.logical (include db save)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                //scApp.putFunBaseObj<PortPLCControl>(function);
                scApp.putFunBaseObj<PortPLCControl_VehicleCommanding1>(function);
            }
        }
        #region AGV 專有
        //public void Port_ToggleBoxCover(bool open)
        //{
        //    var function = scApp.getFunBaseObj<PortPLCControl_AGV_OpenBOX>(port.PORT_ID) as PortPLCControl_AGV_OpenBOX;
        //    try
        //    {
        //        //1.建立各個Function物件
        //        function.ToggleBoxCover = open;
        //        function.Write(bcfApp, port.EqptObjectCate, port.PORT_ID);
        //        function.Timestamp = DateTime.Now;

        //        //2.write log
        //        //LogManager.GetLogger("com.mirle.ibg3k0.sc.Common.LogHelper").Info(function.ToString());
        //        NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

        //        //3.logical (include db save)
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "Exception");
        //    }
        //    finally
        //    {
        //        scApp.putFunBaseObj<PortPLCControl_AGV_OpenBOX>(function);
        //    }
        //}
        public void Port_BCR_Read(bool read)
        {
            var function = scApp.getFunBaseObj<PortPLCControl_AGV_BCR_Read>(PortStation.PORT_ID) as PortPLCControl_AGV_BCR_Read;
            try
            {
                //1.建立各個Function物件
                function.PortIDRead = read;

                function.Write(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                function.Timestamp = DateTime.Now;

                //2.write log
                //LogManager.GetLogger("com.mirle.ibg3k0.sc.Common.LogHelper").Info(function.ToString());
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());
                //3.logical (include db save)
                //Port_RstBCR_Read();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCControl_AGV_BCR_Read>(function);
            }
        }
        public void Port_BCR_Enable(bool enable)
        {
            var function = scApp.getFunBaseObj<PortPLCControl_AGV_BCR_Enable>(PortStation.PORT_ID) as PortPLCControl_AGV_BCR_Enable;
            try
            {
                //1.建立各個Function物件
                function.PortBCR_Enable = enable;

                function.Write(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                function.Timestamp = DateTime.Now;

                //2.write log
                //LogManager.GetLogger("com.mirle.ibg3k0.sc.Common.LogHelper").Info(function.ToString());
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());
                //3.logical (include db save)
                //Port_RstBCR_Read();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCControl_AGV_BCR_Enable>(function);
            }
        }
        public void Port_BCR_Disable(bool disable)
        {
            var function = scApp.getFunBaseObj<PortPLCControl_AGV_BCR_Enable>(PortStation.PORT_ID) as PortPLCControl_AGV_BCR_Enable;
            try
            {
                //1.建立各個Function物件
                function.PortBCR_Enable = disable;

                function.Write(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                function.Timestamp = DateTime.Now;

                //2.write log
                //LogManager.GetLogger("com.mirle.ibg3k0.sc.Common.LogHelper").Info(function.ToString());
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());
                //3.logical (include db save)
                //Port_RstBCR_Read();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCControl_AGV_BCR_Enable>(function);
            }
        }


        public void Port_ChangeToAGVMode()
        {
            var function = scApp.getFunBaseObj<PortPLCControl_AGV_AGVmode>(PortStation.PORT_ID) as PortPLCControl_AGV_AGVmode;
            try
            {
                //1.建立各個Function物件
                function.ToMGVMode = false;
                function.ToAGVMode = true;
                function.Write(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                function.Timestamp = DateTime.Now;

                //2.write log
                //LogManager.GetLogger("com.mirle.ibg3k0.sc.Common.LogHelper").Info(function.ToString());
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                //3.logical (include db save)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCControl_AGV_AGVmode>(function);
            }
        }
        public void Port_ChangeToMGVMode()
        {
            var function = scApp.getFunBaseObj<PortPLCControl_AGV_AGVmode>(PortStation.PORT_ID) as PortPLCControl_AGV_AGVmode;
            try
            {
                //1.建立各個Function物件
                function.ToAGVMode = false;
                function.ToMGVMode = true; ;
                function.Write(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                function.Timestamp = DateTime.Now;

                //2.write log
                //LogManager.GetLogger("com.mirle.ibg3k0.sc.Common.LogHelper").Info(function.ToString());
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                //3.logical (include db save)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCControl_AGV_AGVmode>(function);
            }
        }
        #endregion

        public void Port_RUN()
        {
            var function = scApp.getFunBaseObj<PortPLCControl_PortRunStop>(PortStation.PORT_ID) as PortPLCControl_PortRunStop;
            try
            {
                //1.建立各個Function物件
                function.PortManual = false;
                function.PortAuto = true;

                function.Write(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                function.Timestamp = DateTime.Now;

                //2.write log
                //LogManager.GetLogger("com.mirle.ibg3k0.sc.Common.LogHelper").Info(function.ToString());
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                //3.logical (include db save)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCControl_PortRunStop>(function);
            }
        }
        public void Port_STOP()
        {
            var function = scApp.getFunBaseObj<PortPLCControl_PortRunStop>(PortStation.PORT_ID) as PortPLCControl_PortRunStop;
            try
            {
                //1.建立各個Function物件
                function.PortAuto = false;
                function.PortManual = true;

                function.Write(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                function.Timestamp = DateTime.Now;

                //2.write log
                //LogManager.GetLogger("com.mirle.ibg3k0.sc.Common.LogHelper").Info(function.ToString());
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                //3.logical (include db save)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCControl_PortRunStop>(function);
            }
        }
        public void Port_PortAlarrmReset(bool reset)
        {
            var function = scApp.getFunBaseObj<PortPLCControl_AlarmReset>(PortStation.PORT_ID) as PortPLCControl_AlarmReset;
            try
            {
                //1.建立各個Function物件
                function.PortReset = reset;

                function.Write(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                function.Timestamp = DateTime.Now;

                //2.write log
                //LogManager.GetLogger("com.mirle.ibg3k0.sc.Common.LogHelper").Info(function.ToString());
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                //3.logical (include db save)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCControl_AlarmReset>(function);
            }
        }

        public void Port_AssignCSTID(string cst_id)
        {
            var function = scApp.getFunBaseObj<PortPLCControl_AssignCSTID>(PortStation.PORT_ID) as PortPLCControl_AssignCSTID;
            try
            {
                //1.建立各個Function物件
                APORTSTATION port = scApp.getEQObjCacheManager().getPortStation(PortStation.PORT_ID);
                port.AssignCSTID = cst_id;
                function.AssignCSTID = cst_id;

                function.Write(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                function.Timestamp = DateTime.Now;

                //2.write log
                //LogManager.GetLogger("com.mirle.ibg3k0.sc.Common.LogHelper").Info(function.ToString());
                NLog.LogManager.GetCurrentClassLogger().Info(function.ToString());

                //3.logical (include db save)
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortPLCControl_AssignCSTID>(function);
            }
        }

        #endregion
    }
}
