//*********************************************************************************
//      DefaultValueDefMapAction.cs
//*********************************************************************************
// File Name: PortDefaultValueDefMapAction.cs
// Description: Port Scenario 
//
//(c) Copyright 2013, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
//**********************************************************************************
using System;
using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.bcf.Controller;
using com.mirle.ibg3k0.bcf.Data.ValueDefMapAction;
using com.mirle.ibg3k0.bcf.Data.VO;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.PLC_Functions;
using com.mirle.ibg3k0.sc.Data.VO;
using NLog;

namespace com.mirle.ibg3k0.sc.Data.ValueDefMapAction
{
    public class PortStationDefaultValueDefMapAction : IValueDefMapAction
    {
        public const string DEVICE_NAME = "EQ";
        protected Logger logger = NLog.LogManager.GetCurrentClassLogger();
        protected APORTSTATION PortStation = null;
        protected SCApplication scApp = null;
        protected BCFApplication bcfApp = null;
        protected String[] recipeIDNodes = null;
        public PortStationDefaultValueDefMapAction()
            : base()
        {
            scApp = SCApplication.getInstance();
            bcfApp = scApp.getBCFApplication();
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
            // not implement
        }

        public virtual void doShareMemoryInit(BCFAppConstants.RUN_LEVEL runLevel)
        {
            try
            {
                switch (runLevel)
                {
                    case BCFAppConstants.RUN_LEVEL.ZERO:
                        initPortStationDataChange();
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
                logger.Error(ex, "Exception:");
            }
        }

        private void initPortStationDataChange()
        {

            var function =
            scApp.getFunBaseObj<PortStationReport>(PortStation.PORT_ID) as PortStationReport;
            try
            {
                //1.建立各個Function物件
                function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                PortStation.PORT_SERVICE_STATUS = ProtocolFormat.OHTMessage.PortStationServiceStatus.InService;//暫時先寫死，以後要從資料庫讀
                //APORTSTATION port =  scApp.MapBLL.getPortByPortID(PortStation.PORT_ID);
                //PortStation.PORT_SERVICE_STATUS = port.PORT_SERVICE_STATUS;
                function.portServiceState = PortStation.PORT_SERVICE_STATUS;

                //2.read log
                function.Timestamp = DateTime.Now;
                LogManager.GetLogger("com.mirle.ibg3k0.sc.Common.LogHelper").Info(function.ToString());
                //LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(PortStationReport), Device: DEVICE_NAME,
                //    XID: PortStation.PORT_ID, Data: function.ToString());

                //3.logical (include db save)

                ProtocolFormat.OHTMessage.PortStationStatus status = default(ProtocolFormat.OHTMessage.PortStationStatus);
                if (function.portStationReady)
                {
                    switch (function.portStationLoadRequest)
                    {
                        case true:
                            //PortStation.PORT_STATUS = ProtocolFormat.OHTMessage.PortStationStatus.LoadRequest;//LoadRequest
                            status = ProtocolFormat.OHTMessage.PortStationStatus.LoadRequest;//LoadRequest
                            break;
                        case false:
                            //PortStation.PORT_STATUS = ProtocolFormat.OHTMessage.PortStationStatus.UnloadRequest;//UnloadRequest
                            status = ProtocolFormat.OHTMessage.PortStationStatus.UnloadRequest;//UnloadRequest
                            break;
                    }
                }
                else
                {
                    switch (function.portStationLoadRequest)
                    {
                        case true:
                            //PortStation.PORT_STATUS = ProtocolFormat.OHTMessage.PortStationStatus.Down;//Down
                            status = ProtocolFormat.OHTMessage.PortStationStatus.Down;//Down
                            break;
                        case false:
                            //PortStation.PORT_STATUS = ProtocolFormat.OHTMessage.PortStationStatus.Wait;//Wait 
                            status = ProtocolFormat.OHTMessage.PortStationStatus.Wait;//Wait
                            break;
                    }
                }
                scApp.PortStationService.doUpdatePortStatus(PortStation.PORT_ID, status);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortStationReport>(function);
            }

        }
        private void PortStationDataChange(object sender, ValueChangedEventArgs e)
        {
            var function =
    scApp.getFunBaseObj<PortStationReport>(PortStation.PORT_ID) as PortStationReport;
            try
            {
                //1.建立各個Function物件
                function.Read(bcfApp, SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID);
                function.portServiceState = PortStation.PORT_SERVICE_STATUS;
                //2.read log
                function.Timestamp = DateTime.Now;
                LogManager.GetLogger("com.mirle.ibg3k0.sc.Common.LogHelper").Info(function.ToString());
                //LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(PortStationReport), Device: DEVICE_NAME,
                //    XID: PortStation.PORT_ID, Data: function.ToString());

                //3.logical (include db save)
                ProtocolFormat.OHTMessage.PortStationStatus status = default(ProtocolFormat.OHTMessage.PortStationStatus);
                if (function.portStationReady)
                {
                    switch (function.portStationLoadRequest)
                    {
                        case true:
                            //PortStation.PORT_STATUS = ProtocolFormat.OHTMessage.PortStationStatus.LoadRequest;//LoadRequest
                            status = ProtocolFormat.OHTMessage.PortStationStatus.LoadRequest;//LoadRequest
                            break;
                        case false:
                            //PortStation.PORT_STATUS = ProtocolFormat.OHTMessage.PortStationStatus.UnloadRequest;//UnloadRequest
                            status = ProtocolFormat.OHTMessage.PortStationStatus.UnloadRequest;//UnloadRequest
                            break;
                    }
                }
                else
                {
                    switch (function.portStationLoadRequest)
                    {
                        case true:
                            PortStation.PORT_STATUS = ProtocolFormat.OHTMessage.PortStationStatus.Down;//Down
                            status = ProtocolFormat.OHTMessage.PortStationStatus.Down;//Down
                            break;
                        case false:
                            PortStation.PORT_STATUS = ProtocolFormat.OHTMessage.PortStationStatus.Wait;//Wait 
                            status = ProtocolFormat.OHTMessage.PortStationStatus.Wait;//Wait
                            break;
                    }
                }
                scApp.PortStationService.doUpdatePortStatus(PortStation.PORT_ID, status);

                AEQPT eqpt = scApp.getEQObjCacheManager().getEquipmentByEQPTID(PortStation.EQPT_ID);
                if (PortStation.PORT_SERVICE_STATUS != ProtocolFormat.OHTMessage.PortStationServiceStatus.OutOfService && !eqpt.EQ_Down)//如果Port沒有disable且所屬EQ並非Down，才需要上報MCS PortEventState
                {
                    //mcsMapAction.sendS6F11_PortEventState(PortStation.PORT_ID, ((int)PortStation.PORT_STATUS).ToString());
                }
                else
                {
                    //do nothing
                }
                //如果port Down

                //scApp.getEQObjCacheManager().getPortStation

                //foreach(AUNIT portstation in eqpt.UnitList )
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<PortStationReport>(function);
            }
        }


        public virtual void doInit()
        {
            try
            {
                //mcsMapAction =
                //SCApplication.getInstance().getEQObjCacheManager().getLine().getMapActionByIdentityKey(typeof(AUOMCSDefaultMapAction).Name) as AUOMCSDefaultMapAction;
                ValueRead PortRequest_vr = null;
                if (bcfApp.tryGetReadValueEventstring(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "REQUEST", out PortRequest_vr))
                {
                    PortRequest_vr.afterValueChange += (_sender, _e) => PortStationDataChange(_sender, _e);
                }
                ValueRead PortReady_vr = null;
                if (bcfApp.tryGetReadValueEventstring(SCAppConstants.EQPT_OBJECT_CATE_PORT, PortStation.PORT_ID, "READY", out PortReady_vr))
                {
                    PortReady_vr.afterValueChange += (_sender, _e) => PortStationDataChange(_sender, _e);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
        }



    }
}
