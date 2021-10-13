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
using KingAOP;
using NLog;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Text;

namespace com.mirle.ibg3k0.sc.Data.ValueDefMapAction
{
    public class EquipmentValueDefMapAction : IValueDefMapAction
    {
        public const string DEVICE_NAME_MTL = "EQ";
        Logger logger = NLog.LogManager.GetCurrentClassLogger();
        AEQPT eqpt = null;
        protected SCApplication scApp = null;
        protected BCFApplication bcfApp = null;
        public EquipmentValueDefMapAction()
            : base()
        {
            scApp = SCApplication.getInstance();
            bcfApp = scApp.getBCFApplication();
        }
        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new AspectWeaver(parameter, this);
        }

        public virtual string getIdentityKey()
        {
            return this.GetType().Name;
        }
        public virtual void setContext(BaseEQObject baseEQ)
        {
            this.eqpt = baseEQ as AEQPT;

        }
        public virtual void unRegisterEvent()
        {
            //not implement
        }
        public virtual void doShareMemoryInit(BCFAppConstants.RUN_LEVEL runLevel)
        {
            try
            {
                switch (runLevel)
                {
                    case BCFAppConstants.RUN_LEVEL.ZERO:
                        initEQStatus();
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

        private void initEQStatus()
        {
            var function =
                scApp.getFunBaseObj<EQStatusReport>(eqpt.EQPT_ID) as EQStatusReport;
            try
            {
                //1.建立各個Function物件
                function.Read(bcfApp, eqpt.EqptObjectCate, eqpt.EQPT_ID);
                //2.read log
                function.Timestamp = DateTime.Now;
                LogManager.GetLogger("com.mirle.ibg3k0.sc.Common.LogHelper").Info(function.ToString());
                //LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(EQStatusReport), Device: DEVICE_NAME_MTL,
                //    XID: eqpt.EQPT_ID, Data: function.ToString());

                //3.logical (include db save)
                eqpt.EQ_Ready = function.EQ_READY;
                eqpt.EQ_Error = function.EQ_ERROR;
                bool isDown = !eqpt.EQ_Ready || eqpt.EQ_Error;
                eqpt.EQ_Down = isDown;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                scApp.putFunBaseObj<EQStatusReport>(function);
            }
        }


        private void EQStatusReport(object sender, ValueChangedEventArgs e)
        {
            var function = scApp.getFunBaseObj<EQStatusReport>(eqpt.EQPT_ID) as EQStatusReport;
            try
            {
                //1.建立各個Function物件
                function.Read(bcfApp, eqpt.EqptObjectCate, eqpt.EQPT_ID);
                //2.read log
                function.Timestamp = DateTime.Now;
                LogManager.GetLogger("com.mirle.ibg3k0.sc.Common.LogHelper").Info(function.ToString());
                //LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(EQStatusReport), Device: DEVICE_NAME_MTL,
                //    XID: eqpt.EQPT_ID, Data: function.ToString());

                //3.logical (include db save)
                eqpt.EQ_Ready = function.EQ_READY;
                eqpt.EQ_Error = function.EQ_ERROR;
                bool isDown = !eqpt.EQ_Ready || eqpt.EQ_Error;
                
                List<APORTSTATION> portStationList = scApp.getEQObjCacheManager().getPortStationByEquipment(eqpt.EQPT_ID);
                if(eqpt.EQ_Down != isDown)
                {
                    eqpt.EQ_Down = isDown;
                    if (isDown)
                    {
                        foreach(APORTSTATION portstation in portStationList)
                        {
                            if(portstation.PORT_SERVICE_STATUS == ProtocolFormat.OHTMessage.PortStationServiceStatus.InService)//如果PortStation有被啟用才需要上報
                            {
                                if(portstation.PORT_STATUS != ProtocolFormat.OHTMessage.PortStationStatus.Down)//如果PortStation狀態並非Down才需要上報
                                {
                                    //mcsMapAction.sendS6F11_PortEventState(portstation.PORT_ID, ((int)ProtocolFormat.OHTMessage.PortStationStatus.Down).ToString());// 上報MCS down狀態
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (APORTSTATION portstation in portStationList)
                        {
                            if (portstation.PORT_SERVICE_STATUS == ProtocolFormat.OHTMessage.PortStationServiceStatus.InService)//如果PortStation有被啟用才需要上報
                            {
                                if (portstation.PORT_STATUS != ProtocolFormat.OHTMessage.PortStationStatus.Down)//如果PortStation狀態並非Down才需要上報
                                {
                                    //mcsMapAction.sendS6F11_PortEventState(portstation.PORT_ID, ((int)portstation.PORT_STATUS).ToString());// 上報MCS當前PortStation 狀態
                                }
                            }
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
                scApp.putFunBaseObj<EQStatusReport>(function);
            }
        }



        string event_id = string.Empty;
        /// <summary>
        /// Does the initialize.
        /// </summary>
        public virtual void doInit()
        {
            try
            {
                //mcsMapAction = SCApplication.getInstance().getEQObjCacheManager().getLine().getMapActionByIdentityKey(typeof(AUOMCSDefaultMapAction).Name) as AUOMCSDefaultMapAction;
                ValueRead EQ_Ready_vr = null;
                if (bcfApp.tryGetReadValueEventstring(eqpt.EqptObjectCate, eqpt.EQPT_ID, "EQ_READY", out EQ_Ready_vr))
                {
                    EQ_Ready_vr.afterValueChange += (_sender, _e) => EQStatusReport(_sender, _e);
                }
                ValueRead EQ_Error_vr = null;
                if (bcfApp.tryGetReadValueEventstring(eqpt.EqptObjectCate, eqpt.EQPT_ID, "EQ_ERROR", out EQ_Error_vr))
                {
                    EQ_Error_vr.afterValueChange += (_sender, _e) => EQStatusReport(_sender, _e);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exection:");
            }

        }

    }
}
