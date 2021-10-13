// ***********************************************************************
// Assembly         : ScriptControl
// Author           : 
// Created          : 03-31-2016
//
// Last Modified By : 
// Last Modified On : 03-24-2016
// ***********************************************************************
// <copyright file="LinkStatusCheck.cs" company="">
//     Copyright ©  2014
// </copyright>
// <summary></summary>
// ***********************************************************************
using com.mirle.ibg3k0.bcf.Controller;
using com.mirle.ibg3k0.bcf.Data.TimerAction;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.BLL;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.stc.Common.SECS;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.TimerAction
{
    /// <summary>
    /// Class LinkStatusCheck.
    /// </summary>
    /// <seealso cref="com.mirle.ibg3k0.bcf.Data.TimerAction.ITimerAction" />
    public class LinkStatusCheck : ITimerAction
    {
        /// <summary>
        /// The logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// The sc application
        /// </summary>
        protected SCApplication scApp = null;
        /// <summary>
        /// The sm control
        /// </summary>
        protected MPLCSMControl smControl;
        Dictionary<string, CommuncationInfo> dicCommInfo = null;
        ValueWrite isAliveIndexVW = null;
        ValueRead isAliveIndexVR = null;
        List<AEQPT> eqpts = null;
        double eqAlive_Min_Change_Interval_sec = 20;
        ALINE line = null;
        /// <summary>
        /// Initializes a new instance of the <see cref="LinkStatusCheck"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="intervalMilliSec">The interval milli sec.</param>
        public LinkStatusCheck(string name, long intervalMilliSec)
            : base(name, intervalMilliSec)
        {

        }

        /// <summary>
        /// Initializes the start.
        /// </summary>
        public override void initStart()
        {
            //not implement
            scApp = SCApplication.getInstance();
            //smControl = scApp.getBCFApplication().getMPLCSMControl("EQ") as MPLCSMControl;
            //isAliveIndexVW = scApp.getBCFApplication().getWriteValueEvent(SCAppConstants.EQPT_OBJECT_CATE_LINE, "VH_LINE", "OHxC_Alive_W");
            //isAliveIndexVR = scApp.getBCFApplication().getReadValueEvent(SCAppConstants.EQPT_OBJECT_CATE_LINE, "VH_LINE", "OHxC_Alive_R");
            dicCommInfo = scApp.getEQObjCacheManager().CommonInfo.dicCommunactionInfo;
            line = scApp.getEQObjCacheManager().getLine();
            eqpts = scApp.getEQObjCacheManager().getAllEquipment();
        }


        private long syncPoint = 0;
        /// <summary>
        /// Timer Action的執行動作
        /// </summary>
        /// <param name="obj">The object.</param>
        public override void doProcess(object obj)
        {
            if (System.Threading.Interlocked.Exchange(ref syncPoint, 1) == 0)
            {

                try
                {
                    doCheckIPLinkStatus();
                    doCheckEQAliveStatus();
                    scApp.CheckSystemEventHandler.CheckCheckSystemIsExist();

                    InlineEfficiencyMonitor();
                    if (SCUtility.getCallContext<bool>(ALINE.CONTEXT_KEY_WORD_LINE_STATUS_HAS_CHANGE))
                    {
                        line.NotifyLineStatusChange();
                        SCUtility.setCallContext(ALINE.CONTEXT_KEY_WORD_LINE_STATUS_HAS_CHANGE, null);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                }
                finally
                {
                    System.Threading.Interlocked.Exchange(ref syncPoint, 0);
                }
            }
            //if (++excute_count % 2 == 0)
            //    Task.Run(() => doChcekPLCLinkStatus());
            //Task.Run(() => doReadPLCAlive());
        }
        private long syncCheckEQAlive_Point = 0;
        private void doCheckEQAliveStatus()
        {
            if (System.Threading.Interlocked.Exchange(ref syncCheckEQAlive_Point, 1) == 0)
            {
                try
                {
                    foreach (AEQPT eqpt in eqpts)
                    {
                        if (eqpt.Eq_Alive_Last_Change_time.AddSeconds(eqAlive_Min_Change_Interval_sec) < DateTime.Now)
                        {
                            eqpt.Is_Eq_Alive = false;
                        }
                        else
                        {
                            eqpt.Is_Eq_Alive = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                }
                finally
                {
                    System.Threading.Interlocked.Exchange(ref syncCheckEQAlive_Point, 0);
                }
            }
        }
        private void InlineEfficiencyMonitor()
        {
            ALINE line = scApp.getEQObjCacheManager().getLine();
            VehicleBLL vehicleBLL = scApp.VehicleBLL;
            CMDBLL cmdBLL = scApp.CMDBLL;
            line.CurrntVehicleModeAutoRemoteCount = vehicleBLL.cache.getVhCurrentModeInAutoRemoteCount();
            line.CurrntVehicleModeAutoLoaclCount = vehicleBLL.cache.getVhCurrentModeInAutoLocalCount();
            line.CurrntVehicleStatusIdelCount = vehicleBLL.cache.getVhCurrentStatusInIdleCount();
            line.CurrntVehicleStatusErrorCount = vehicleBLL.cache.getVhCurrentStatusInErrorCount();
            var host_cmds = cmdBLL.loadUnfinishedTransfer().Where(cmd=>!cmd.ID.StartsWith("PortTypeChange"));
            UInt16 carrier_transferring_count = (UInt16)host_cmds.
                                        Where(cmd => cmd.TRANSFERSTATE == E_TRAN_STATUS.Transferring).
                                        Count();
            UInt16 carrier_watting_count = (UInt16)host_cmds.
                                        Where(cmd => cmd.TRANSFERSTATE < E_TRAN_STATUS.Transferring).
                                        Count();
            UInt16 host_cmd_assigned_count = (UInt16)host_cmds.
                                        Where(cmd => cmd.TRANSFERSTATE >= E_TRAN_STATUS.Initial).
                                        Count();
            UInt16 host_cmd_watting_count = (UInt16)host_cmds.
                                        Where(cmd => cmd.TRANSFERSTATE < E_TRAN_STATUS.Initial).
                                        Count();

            line.CurrntCSTStatueTransferCount = carrier_transferring_count;
            line.CurrntCSTStatueWaitingCount = carrier_watting_count;
            line.CurrntHostCommandTransferStatueAssignedCount = host_cmd_assigned_count;
            line.CurrntHostCommandTransferStatueWaitingCounr = host_cmd_watting_count;
        }
        private long syncCheckPLC_Point = 0;
        private void doChcekPLCLinkStatus()
        {
            if (System.Threading.Interlocked.Exchange(ref syncCheckPLC_Point, 1) == 0)
            {

                try
                {
                    UInt16 isAliveIndex = (UInt16)isAliveIndexVW.getText();
                    int x = isAliveIndex + 1;
                    if (x > 9999) { x = 1; }
                    isAliveIndexVW.setWriteValue((UInt16)x);
                    bool isWriteSucess = smControl.writeDeviceBlock(isAliveIndexVW);
                    logger.Trace($"Alive Index:{x}");
                    dicCommInfo["PLC"].IsCommunactionSuccess = isWriteSucess;
                    dicCommInfo["PLC"].IsConnectinoSuccess = isWriteSucess;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                }
                finally
                {
                    System.Threading.Interlocked.Exchange(ref syncCheckPLC_Point, 0);
                }
            }
        }

        private long syncReadPLCAlive = 0;
        private void doReadPLCAlive()
        {
            if (System.Threading.Interlocked.Exchange(ref syncReadPLCAlive, 1) == 0)
            {
                try
                {
                    UInt16 alaveIndex = (UInt16)isAliveIndexVR.getText();
                    logger.Trace($"Alive Index      :{alaveIndex}");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                }
                finally
                {
                    System.Threading.Interlocked.Exchange(ref syncReadPLCAlive, 0);
                }
            }
        }

        private long syncCheckIP_Point = 0;
        private void doCheckIPLinkStatus()
        {
            if (System.Threading.Interlocked.Exchange(ref syncCheckIP_Point, 1) == 0)
            {
                try
                {
                    foreach (KeyValuePair<string, CommuncationInfo> keyPair in dicCommInfo)
                    {
                        CommuncationInfo Info = keyPair.Value;
                        if (!SCUtility.isEmpty(Info.Getway_IP))
                        {
                            Info.IsCommunactionSuccess = PingIt(Info.Getway_IP);
                        }
                        if (!SCUtility.isEmpty(Info.Remote_IP))
                        {
                            Info.IsConnectinoSuccess = PingIt(Info.Remote_IP);
                        }
                    }
                    line.setConnectionInfo(dicCommInfo);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                }
                finally
                {
                    System.Threading.Interlocked.Exchange(ref syncCheckIP_Point, 0);
                }
            }
        }
        /// <summary>
        /// Pings it.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool PingIt(String ip)
        {
            PingReply r;
            try
            {
                Ping p = new Ping();
                r = p.Send(ip);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
                return false;
            }
            return r.Status == IPStatus.Success;
        }
    }
}