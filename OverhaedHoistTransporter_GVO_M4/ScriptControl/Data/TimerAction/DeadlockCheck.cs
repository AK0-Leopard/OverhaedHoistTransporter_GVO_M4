//*********************************************************************************
//      ZoneBlockCheck.cs
//*********************************************************************************
// File Name: ZoneBlockCheck.cs
// Description: 
//
//(c) Copyright 2014, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
//**********************************************************************************
using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Controller;
using com.mirle.ibg3k0.bcf.Data.TimerAction;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.TimerAction
{
    /// <summary>
    /// Class ZoneBlockCheck.
    /// </summary>
    /// <seealso cref="com.mirle.ibg3k0.bcf.Data.TimerAction.ITimerAction" />
    class DeadlockCheck : ITimerAction
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
        /// Initializes a new instance of the <see cref="DeadlockCheck"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="intervalMilliSec">The interval milli sec.</param>
        public DeadlockCheck(string name, long intervalMilliSec)
            : base(name, intervalMilliSec)
        {

        }

        /// <summary>
        /// Initializes the start.
        /// </summary>
        public override void initStart()
        {
            //do nothing
            scApp = SCApplication.getInstance();

        }

        private long checkSyncPoint = 0;
        public void doProcessOld(object obj)
        {
            if (!SystemParameter.AutoOverride)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(DeadlockCheck), Device: "OHTC",
                   Data: $"auto override is:{SystemParameter.AutoOverride}.");
                return;
            }
            if (System.Threading.Interlocked.Exchange(ref checkSyncPoint, 1) == 0)
            {
                try
                {
                    //1.找出發生Reserve要不到而停止的車子，找到以後在找是否有下一台車子也是要不到Reserve的
                    //發現以後再透過兩台車所要不到的Address找出互卡的是哪段Section，接著將其section所屬的Segment Banned後在執行override
                    var vhs = scApp.VehicleBLL.cache.loadAllVh();
                    var vhs_ReserveStop = vhs.Where(v => v.IsReservePause)
                                          .OrderBy(v => v.VEHICLE_ID)
                                          .ToList();
                    foreach (var vh_active in vhs_ReserveStop)
                    {
                        foreach (var vh_passive in vhs_ReserveStop)
                        {
                            if (vh_active == vh_passive) continue;
                            if (!vh_active.IsReservePause || !vh_active.IsReservePause) continue;
                            if ((vh_active.CanNotReserveInfo != null && vh_passive.CanNotReserveInfo != null))
                            {
                                List<AVEHICLE> sort_vhs = new List<AVEHICLE>() { vh_active, vh_passive };
                                sort_vhs.Sort(SortOverrideOfVehicle);
                                foreach (AVEHICLE avoid_vh in sort_vhs)
                                {
                                    if (avoid_vh.CurrentFailOverrideTimes >= AVEHICLE.MAX_FAIL_OVERRIDE_TIMES_IN_ONE_CASE)
                                    {
                                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(DeadlockCheck), Device: "OHTC",
                                           Data: $"dead lock happend ,but vh:{avoid_vh.VEHICLE_ID} has been override more than {AVEHICLE.MAX_FAIL_OVERRIDE_TIMES_IN_ONE_CASE} times, continue next vh.",
                                           VehicleID: avoid_vh.VEHICLE_ID);
                                        continue;
                                    }

                                    if (avoid_vh.VhAvoidInfo != null)
                                    {
                                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(DeadlockCheck), Device: "OHTC",
                                           Data: $"dead lock happend ,but vh:{avoid_vh.VEHICLE_ID} has been avoid command , continue next vh." +
                                                 $"blocked section:{avoid_vh.VhAvoidInfo.BlockedSectionID} blocked vh id:{avoid_vh.VhAvoidInfo.BlockedVehicleID}",
                                           VehicleID: avoid_vh.VEHICLE_ID);
                                        continue;
                                    }
                                    AVEHICLE blocked = avoid_vh == vh_active ? vh_passive : vh_active;

                                    var key_blocked_vh = findTheKeyBlockVhID(avoid_vh, blocked);
                                    if (key_blocked_vh == null) continue;
                                    if (avoid_vh.isTcpIpConnect)
                                    {

                                        bool is_override_success = scApp.VehicleService.Avoid.trydoAvoidCommandToVh(avoid_vh, key_blocked_vh);
                                        if (is_override_success)
                                        {
                                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(DeadlockCheck), Device: "OHTC",
                                               Data: $"dead lock happend ,ask vh:{avoid_vh.VEHICLE_ID} chnage path success.",
                                               VehicleID: avoid_vh.VEHICLE_ID);
                                            System.Threading.SpinWait.SpinUntil(() => false, 15000);
                                            return;
                                        }
                                        else
                                        {
                                            avoid_vh.CurrentFailOverrideTimes++;
                                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(DeadlockCheck), Device: "OHTC",
                                               Data: $"dead lock happend ,ask vh:{avoid_vh.VEHICLE_ID} chnage path fail, fail times:{avoid_vh.CurrentFailOverrideTimes}.",
                                               VehicleID: avoid_vh.VEHICLE_ID);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exection:");
                }
                finally
                {

                    System.Threading.Interlocked.Exchange(ref checkSyncPoint, 0);

                }
            }
        }
        private AVEHICLE findTheKeyBlockVhID(AVEHICLE avoidVh, AVEHICLE blockedVh)
        {

            if (SCUtility.isMatche(avoidVh.CanNotReserveInfo.ReservedVhID, blockedVh.VEHICLE_ID) &&
                SCUtility.isMatche(blockedVh.CanNotReserveInfo.ReservedVhID, avoidVh.VEHICLE_ID))
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(DeadlockCheck), Device: "OHTC",
                   Data: $"dead lock happend ,find key blocked vh .avoid vh:{avoidVh.VEHICLE_ID} ,blocked vh:{blockedVh.VEHICLE_ID}",
                   VehicleID: avoidVh.VEHICLE_ID);
                return blockedVh;
            }
            else
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(DeadlockCheck), Device: "OHTC",
                   Data: $"dead lock happend ,can't find key blocked vh .avoid vh:{avoidVh.VEHICLE_ID} ,blocked vh:{blockedVh.VEHICLE_ID}. start find orther block vh",
                   VehicleID: avoidVh.VEHICLE_ID);

                AVEHICLE orther_reserved_vh = scApp.VehicleBLL.cache.getVehicle(blockedVh.CanNotReserveInfo.ReservedVhID);
                int find_count = 0;
                return findTheOrtherKeyBlockVhID(avoidVh, orther_reserved_vh, ref find_count);
            }
        }

        const int MAX_FIND_COUNT = 4;
        private AVEHICLE findTheOrtherKeyBlockVhID(AVEHICLE avoidVh, AVEHICLE reservedVh, ref int findCount)
        {
            if (findCount > MAX_FIND_COUNT)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(DeadlockCheck), Device: "OHTC",
                   Data: $"dead lock happend ,find key block fail. over find times:{findCount}",
                   VehicleID: avoidVh.VEHICLE_ID);
                return null;
            }
            if (SCUtility.isMatche(avoidVh.VEHICLE_ID, reservedVh.VEHICLE_ID))
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(DeadlockCheck), Device: "OHTC",
                   Data: $"dead lock happend ,find key block fail. avoid and blocked vh is same.vh id:{avoidVh.VEHICLE_ID}",
                   VehicleID: avoidVh.VEHICLE_ID);
                return null;
            }
            if (reservedVh.CanNotReserveInfo == null) return null;
            if (SCUtility.isMatche(avoidVh.VEHICLE_ID, reservedVh.CanNotReserveInfo.ReservedVhID))
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(DeadlockCheck), Device: "OHTC",
                   Data: $"dead lock happend ,find key blocked vh(orther).avoid vh:{avoidVh.VEHICLE_ID} ,blocked vh:{reservedVh.VEHICLE_ID}",
                   VehicleID: avoidVh.VEHICLE_ID);
                return reservedVh;
            }
            else
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(DeadlockCheck), Device: "OHTC",
                   Data: $"dead lock happend ,no find key blocked vh ," +
                         $"find next block vh id:{reservedVh.CanNotReserveInfo.ReservedVhID} blocking vh:{reservedVh.VEHICLE_ID}",
                   VehicleID: avoidVh.VEHICLE_ID);
                AVEHICLE orther_reserved_vh = scApp.VehicleBLL.cache.getVehicle(reservedVh.CanNotReserveInfo.ReservedVhID);
                findCount++;
                return findTheOrtherKeyBlockVhID(avoidVh, orther_reserved_vh, ref findCount);
            }
        }

        private int SortOverrideOfVehicle(AVEHICLE vh1, AVEHICLE vh2)
        {
            int result;
            if (vh1.VhAvoidInfo != null)
            {
                return 1;
            }
            var vhs = scApp.VehicleBLL.cache.loadAllVh();

            var blocked_vhs_by_vh1 = vhs.
                   Where(v => v.isTcpIpConnect &&
                              v.IsReservePause &&
                              v.CanNotReserveInfo != null &&
                              SCUtility.isMatche(v.CanNotReserveInfo.ReservedVhID, vh1.VEHICLE_ID)).
                   ToList();

            if (blocked_vhs_by_vh1.Count > 1)
            {
                return -1;
            }

            //if (vh2.VhAvoidInfo != null)
            //{
            //    return 1;
            //}


            if (vh1.HasExcuteTransferCommand && vh2.HasExcuteTransferCommand)
            {
                if (vh1.HasCarryCST && vh2.HasCarryCST)
                {
                    result = 0;
                }
                else if (vh1.HasCarryCST)
                {
                    result = 1;
                }
                //else if (vh2.HasCarryCST)
                //{
                //    result = -1;
                //}
                else
                {
                    result = 0;
                }
            }
            else if (vh1.HasExcuteTransferCommand)
            {
                result = 1;
            }
            //else if (vh2.HasExcuteTransferCommand)
            //{
            //    result = -1;
            //}
            else
            {
                result = 0;
            }
            return result;
        }

        public override void doProcess(object obj)
        {
            if (!SystemParameter.AutoOverride)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(DeadlockCheck), Device: "OHTC",
                   Data: $"auto override is:{SystemParameter.AutoOverride}.");
                return;
            }
            if (System.Threading.Interlocked.Exchange(ref checkSyncPoint, 1) == 0)
            {
                try
                {
                    //1.找出發生Reserve要不到而停止的車子，找到以後在找是否有下一台車子也是要不到Reserve的
                    //發現以後再透過兩台車所要不到的Address找出互卡的是哪段Section，接著將其section所屬的Segment Banned後在執行override
                    var vhs = scApp.VehicleBLL.cache.loadAllVh();
                    var vhs_ReserveStop = vhs.Where(v => v.isTcpIpConnect &&
                                                         v.IsReservePause &&
                                                         v.CanNotReserveInfo != null)
                                          .ToList();
                    vhs_ReserveStop.Sort(SortOverrideOfVehicle);
                    //if (vhs_ReserveStop.Count == 2)
                    //{
                    //    if (SCUtility.isMatche(vhs_ReserveStop[0].CanNotReserveInfo.ReservedVhID, vhs_ReserveStop[1].VEHICLE_ID) &&
                    //        SCUtility.isMatche(vhs_ReserveStop[1].CanNotReserveInfo.ReservedVhID, vhs_ReserveStop[0].VEHICLE_ID))
                    //    {
                    //        //not thing...
                    //    }
                    //    else
                    //    {
                    //        return;
                    //    }
                    //    AVEHICLE avoid_vh = null;
                    //    AVEHICLE wait_pass_vh = null;
                    //    if (vhs_ReserveStop[0].VhAvoidInfo == null)
                    //    {
                    //        avoid_vh = vhs_ReserveStop[0];
                    //        wait_pass_vh = vhs_ReserveStop[1];
                    //    }
                    //    else if (vhs_ReserveStop[1].VhAvoidInfo == null)
                    //    {
                    //        avoid_vh = vhs_ReserveStop[1];
                    //        wait_pass_vh = vhs_ReserveStop[0];
                    //    }
                    //    else
                    //    {
                    //        //todo log
                    //        return;
                    //    }
                    //    bool is_override_success = scApp.VehicleService.Avoid.trydoAvoidCommandToVh(avoid_vh, wait_pass_vh);
                    //    if (is_override_success)
                    //    {
                    //        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(DeadlockCheck), Device: "AGVC",
                    //           Data: $"dead lock happend ,ask vh:{avoid_vh.VEHICLE_ID} chnage path success.",
                    //           VehicleID: avoid_vh.VEHICLE_ID);
                    //        System.Threading.SpinWait.SpinUntil(() => false, 15000);
                    //        return;
                    //    }
                    //    else
                    //    {
                    //        avoid_vh.CurrentFailOverrideTimes++;
                    //        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(DeadlockCheck), Device: "AGVC",
                    //           Data: $"dead lock happend ,ask vh:{avoid_vh.VEHICLE_ID} chnage path fail, fail times:{avoid_vh.CurrentFailOverrideTimes}.",
                    //           VehicleID: avoid_vh.VEHICLE_ID);
                    //    }
                    //}
                    /*else */
                    if (vhs_ReserveStop.Count >= 2)
                    {
                        foreach (var avoid_vh in vhs_ReserveStop)
                        {
                            //如果已經有在執行避車的車子就不再次下達避車指令
                            //if (avoid_vh.VhAvoidInfo != null)
                            //{
                            //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(DeadlockCheck), Device: "AGVC",
                            //       Data: $"dead lock happend ,but avoid vh:{avoid_vh.VEHICLE_ID} has been avoid command , continue next vh." +
                            //             $"blocked section:{avoid_vh.VhAvoidInfo.BlockedSectionID} blocked vh id:{avoid_vh.VhAvoidInfo.BlockedVehicleID}",
                            //       VehicleID: avoid_vh.VEHICLE_ID);
                            //    continue;
                            //}
                            var wait_pass_vhs = vhs_ReserveStop.
                                               Where(v => SCUtility.isMatche(v.CanNotReserveInfo.ReservedVhID, avoid_vh.VEHICLE_ID)).
                                               ToList();
                            foreach (var wait_pass_vh in wait_pass_vhs)
                            {
                                //如果擋住的那台車，是正在執行避車指令且是為了躲目前選中的Avoid Vh的話，
                                //就先等他避完再來確認。
                                if (wait_pass_vh.VhAvoidInfo != null &&
                                    SCUtility.isMatche(wait_pass_vh.VhAvoidInfo.BlockedVehicleID, avoid_vh.VEHICLE_ID))
                                {
                                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(DeadlockCheck), Device: "OHTC",
                                       Data: $"dead lock happend ,but wait pass vh:{wait_pass_vh.VEHICLE_ID} has been avoid command , continue next vh." +
                                             $"blocked section:{wait_pass_vh.VhAvoidInfo.BlockedSectionID} blocked vh id:{wait_pass_vh.VhAvoidInfo.BlockedVehicleID}",
                                       VehicleID: avoid_vh.VEHICLE_ID);
                                    continue;
                                }
                                bool is_override_success = scApp.VehicleService.Avoid.trydoAvoidCommandToVh(avoid_vh, wait_pass_vh);
                                if (is_override_success)
                                {
                                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(DeadlockCheck), Device: "OHTC",
                                       Data: $"dead lock happend ,ask vh:{avoid_vh.VEHICLE_ID} chnage path success.",
                                       VehicleID: avoid_vh.VEHICLE_ID);
                                    System.Threading.SpinWait.SpinUntil(() => false, 15000);
                                    return;
                                }
                                else
                                {
                                    avoid_vh.CurrentFailOverrideTimes++;
                                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(DeadlockCheck), Device: "OHTC",
                                       Data: $"dead lock happend ,ask vh:{avoid_vh.VEHICLE_ID} chnage path fail, fail times:{avoid_vh.CurrentFailOverrideTimes}.",
                                       VehicleID: avoid_vh.VEHICLE_ID);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exection:");
                }
                finally
                {

                    System.Threading.Interlocked.Exchange(ref checkSyncPoint, 0);

                }
            }
        }


    }
}