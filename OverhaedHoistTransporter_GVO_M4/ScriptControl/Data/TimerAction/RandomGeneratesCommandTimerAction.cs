// ***********************************************************************
// Assembly         : ScriptControl
// Author           : 
// Created          : 03-31-2016
//
// Last Modified By : 
// Last Modified On : 03-24-2016
// ***********************************************************************
// <copyright file="BCSystemStatusTimer.cs" company="">
//     Copyright ©  2014
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using com.mirle.ibg3k0.bcf.Data.TimerAction;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.DAO;
using com.mirle.ibg3k0.sc.Data.SECS;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using NLog;

namespace com.mirle.ibg3k0.sc.Data.TimerAction
{
    /// <summary>
    /// Class BCSystemStatusTimer.
    /// </summary>
    /// <seealso cref="com.mirle.ibg3k0.bcf.Data.TimerAction.ITimerAction" />
    public class RandomGeneratesCommandTimerAction : ITimerAction
    {
        /// <summary>
        /// The logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// The sc application
        /// </summary>
        protected SCApplication scApp = null;
        private List<TranTask> tranTasks = null;

        public Dictionary<string, List<TranTask>> dicTranTaskSchedule_Clear_Dirty = null;
        public List<String> SourcePorts_None = null;
        public List<String> SourcePorts_Clear = null;
        public List<String> SourcePorts_Dirty = null;

        public List<String> AllCanExcutePort = null;


        Random rnd_Index = new Random(Guid.NewGuid().GetHashCode());

        /// <summary>
        /// Initializes a new instance of the <see cref="BCSystemStatusTimer"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="intervalMilliSec">The interval milli sec.</param>
        public RandomGeneratesCommandTimerAction(string name, long intervalMilliSec)
            : base(name, intervalMilliSec)
        {

        }
        /// <summary>
        /// Initializes the start.
        /// </summary>
        public override void initStart()
        {
            scApp = SCApplication.getInstance();
            tranTasks = scApp.CMDBLL.loadTranTasks();
            dicTranTaskSchedule_Clear_Dirty = new Dictionary<string, List<TranTask>>();

            HashSet<string> all_port = new HashSet<string>();
            foreach (var task in tranTasks)
            {
                all_port.Add(task.SourcePort);
                all_port.Add(task.DestinationPort);
                APORTSTATION port_station = scApp.getEQObjCacheManager().getPortStation(task.SourcePort);
                if (port_station == null) continue;

                if (port_station.ULD_VH_TYPE == E_VH_TYPE.None)
                {
                    if (!dicTranTaskSchedule_Clear_Dirty.ContainsKey("N"))
                    {
                        dicTranTaskSchedule_Clear_Dirty.Add("N", new List<TranTask>());
                    }
                    dicTranTaskSchedule_Clear_Dirty["N"].Add(task);
                }
                else if (port_station.ULD_VH_TYPE == E_VH_TYPE.Clean)
                {
                    if (!dicTranTaskSchedule_Clear_Dirty.ContainsKey("C"))
                    {
                        dicTranTaskSchedule_Clear_Dirty.Add("C", new List<TranTask>());
                    }
                    dicTranTaskSchedule_Clear_Dirty["C"].Add(task);
                }
                else if (port_station.ULD_VH_TYPE == E_VH_TYPE.Dirty)
                {
                    if (!dicTranTaskSchedule_Clear_Dirty.ContainsKey("D"))
                    {
                        dicTranTaskSchedule_Clear_Dirty.Add("D", new List<TranTask>());
                    }
                    dicTranTaskSchedule_Clear_Dirty["D"].Add(task);
                }
            }
            AllCanExcutePort = all_port.ToList();

            if (dicTranTaskSchedule_Clear_Dirty.ContainsKey("N"))
                SourcePorts_None = dicTranTaskSchedule_Clear_Dirty["N"].Select(task => task.SourcePort).Distinct().ToList();
            if (dicTranTaskSchedule_Clear_Dirty.ContainsKey("C"))
                SourcePorts_Clear = dicTranTaskSchedule_Clear_Dirty["C"].Select(task => task.SourcePort).Distinct().ToList();
            if (dicTranTaskSchedule_Clear_Dirty.ContainsKey("D"))
                SourcePorts_Dirty = dicTranTaskSchedule_Clear_Dirty["D"].Select(task => task.SourcePort).Distinct().ToList();

        }
        /// <summary>
        /// Timer Action的執行動作
        /// </summary>
        /// <param name="obj">The object.</param>
        private long syncPoint = 0;
        public override void doProcess(object obj)
        {
            if (!DebugParameter.CanAutoRandomGeneratesCommand) return;
            if (System.Threading.Interlocked.Exchange(ref syncPoint, 1) == 0)
            {
                try
                {
                    Taichung();
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
            //scApp.BCSystemBLL.reWriteBCSystemRunTime();
        }

        private void OHS100()
        {
            if (scApp.VehicleBLL.cache.getNoExcuteMcsCmdVhCount(E_VH_TYPE.Clean) > 0)
                RandomGenerates_TranTask_Clear_Drity("C");
            Thread.Sleep(1000);
            if (scApp.VehicleBLL.cache.getNoExcuteMcsCmdVhCount(E_VH_TYPE.Dirty) > 0)
                RandomGenerates_TranTask_Clear_Drity("D");
        }

        private void RandomGenerates_TranTask_Clear_Drity(string car_type)
        {
            List<TranTask> lstTranTask = dicTranTaskSchedule_Clear_Dirty[car_type];
            int task_RandomIndex = rnd_Index.Next(lstTranTask.Count - 1);
            Console.WriteLine(string.Format("Car Type:{0},Index:{1}", car_type, task_RandomIndex));
            TranTask tranTask = lstTranTask[task_RandomIndex];
            //Task.Run(() => mcsManager.sendTranCmd(tranTask.SourcePort, tranTask.DestinationPort));
            //sendTranCmd("CST02", tranTask.SourcePort, tranTask.DestinationPort);
        }

        private void Taichung()
        {
            //tryCreatMCSCommand(E_VH_TYPE.None, SourcePorts_None);
            tryCreatMoveCommand();
        }

        private void tryCreatMoveCommand()
        {
            List<AVEHICLE> vhs = scApp.VehicleBLL.cache.loadAllVh();
            foreach (AVEHICLE vh in vhs)
            {
                bool is_can_creat_move_cmd = vh.isTcpIpConnect &&
                       (vh.MODE_STATUS == VHModeStatus.AutoRemote) &&
                       vh.ACT_STATUS == VHActionStatus.NoCommand &&
                       !SCUtility.isEmpty(vh.CUR_ADR_ID) &&
                       !scApp.CMDBLL.isCMD_OHTCQueueByVh(vh.VEHICLE_ID);
                if (!is_can_creat_move_cmd) continue;

                string vh_cur_adr = SCUtility.Trim(vh.CUR_ADR_ID);
                List<string> all_can_excute_port_temp = AllCanExcutePort.ToList();
                all_can_excute_port_temp.Remove(vh_cur_adr);
                int task_RandomIndex = rnd_Index.Next(all_can_excute_port_temp.Count - 1);
                string move_target_adr = all_can_excute_port_temp[task_RandomIndex];
                creatMoveCommand(vh.VEHICLE_ID, move_target_adr);
            }
        }
        private void tryCreatMCSCommand(E_VH_TYPE vh_type, List<string> load_port_lst)
        {
            int act_count = scApp.VehicleBLL.cache.getActVhCount(vh_type);
            if (act_count == 0) return;
            int unfinished_cmd_count = scApp.CMDBLL.getCMD_MCSIsUnfinishedCountByHostSource(load_port_lst);
            int task_RandomIndex = 0;
            TranTask tranTask = null;
            APORTSTATION source_port_station = null;
            APORTSTATION destination_port_station = null;
            string carrier_id = null;
            if (unfinished_cmd_count < act_count)
            {
                bool is_find = false;
                var task_list_clean = dicTranTaskSchedule_Clear_Dirty[vh_type.ToString().Substring(0, 1)].ToList();
                do
                {
                    task_RandomIndex = rnd_Index.Next(task_list_clean.Count - 1);
                    tranTask = task_list_clean[task_RandomIndex];
                    source_port_station = scApp.getEQObjCacheManager().getPortStation(tranTask.SourcePort);
                    destination_port_station = scApp.getEQObjCacheManager().getPortStation(tranTask.DestinationPort);
                    if ((source_port_station != null && !SCUtility.isEmpty(source_port_station.CST_ID)) &&
                        scApp.CMDBLL.getCMD_MCSIsUnfinishedCountByCarrierID(source_port_station.CST_ID) == 0 &&
                        (destination_port_station != null && SCUtility.isEmpty(destination_port_station.CST_ID)))
                    {
                        carrier_id = source_port_station.CST_ID;
                        is_find = true;
                    }
                    else
                    {
                        task_list_clean.RemoveAt(task_RandomIndex);
                        if (task_list_clean.Count == 0) return;
                    }
                    SpinWait.SpinUntil(() => false, 1);
                } while (!is_find);

                if (is_find)
                {
                    //sendTranCmd(carrier_id, tranTask.SourcePort, tranTask.DestinationPort);
                    //SpinWait.SpinUntil(() => false, 1000);
                    //sendTranCmd(carrier_id, tranTask.DestinationPort, tranTask.SourcePort);
                }
            }
        }

        public void creatMoveCommand(string vhID, string targetAdr)
        {
            //scApp.CMDBLL.doCreatCommand(vhID, string.Empty, string.Empty,
            //                                    E_CMD_TYPE.Move,
            //                                    string.Empty,
            //                                    targetAdr, 0, 0);
            scApp.VehicleService.Command.Move(vhID, targetAdr);
        }
    }

}

