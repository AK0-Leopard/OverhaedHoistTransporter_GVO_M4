using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.Data.DAO;
using com.mirle.ibg3k0.sc.Data.SECS.OHTC.AT_S;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using NLog;
using Quartz.Impl.Triggers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class CMDBLL
    {
        protected static Logger logger = LogManager.GetCurrentClassLogger();
        NLog.Logger TransferServiceLogger = NLog.LogManager.GetLogger("TransferServiceLogger");
        protected CMDDao cmd_ohtcDAO = null;
        CMD_DetailDao cmd_ohtc_detailDAO = null;
        TransferDao cmd_mcsDao = null;
        VTransferDao vcmd_mcsDao = null;
        HCMDDao hcmdDao = null;
        HTransferDao hTransferDao = null;
        TestTranTaskDao testTranTaskDao = null;
        ReturnCodeMapDao return_code_mapDao = null;
        ICommandChecker commandCheckerNormal = null;
        ICommandChecker commandCheckerSwap = null;


        public Cache cache { get; private set; }


        protected static Logger logger_VhRouteLog = LogManager.GetLogger("VhRoute");
        private string[] ByPassSegment = null;
        ParkZoneTypeDao parkZoneTypeDao = null;
        protected static SCApplication scApp = null;



        public CMDBLL()
        {

        }
        public void start(SCApplication app)
        {
            scApp = app;
            cmd_ohtcDAO = scApp.CMDCDao;
            cmd_ohtc_detailDAO = scApp.CMD_DetailDao;
            cmd_mcsDao = scApp.TransferDao;
            vcmd_mcsDao = scApp.VTransferDao;
            hTransferDao = scApp.HTransferDao;
            hcmdDao = scApp.HCMDDao;
            parkZoneTypeDao = scApp.ParkZoneTypeDao;
            testTranTaskDao = scApp.TestTranTaskDao;
            return_code_mapDao = scApp.ReturnCodeMapDao;

            cache = new Cache(app);
            commandCheckerNormal = new CommandCheckerNormal(scApp);
            commandCheckerSwap = new CommandCheckerSwap(scApp);

        }



        #region CMD_MCS

        public void DeleteLog(int deleteMonths)
        {
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                cmd_mcsDao.DeleteLOG_ByACMD_MCS(con, deleteMonths);
                cmd_mcsDao.DeleteLOG_ByACMD_OHTC(con, deleteMonths);
                //cmd_mcsDao.DeleteLOG_ByACMD_AMCSREPORTQUEUE(con, deleteMonths);
            }
        }



        public bool updateTransferCmd_TranStatus(string cmd_id, E_TRAN_STATUS status)
        {
            bool isSuccess = true;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                ATRANSFER cmd = cmd_mcsDao.getByID(con, cmd_id);
                if (cmd.TRANSFERSTATE <= status)
                {
                    cmd.TRANSFERSTATE = status;
                }
                cmd_mcsDao.update(con, cmd);
            }
            return isSuccess;
        }

        public bool updateTransferCmd_TranStatus2Initial(string cmd_id)
        {
            bool isSuccess = true;
            //using (DBConnection_EF con = new DBConnection_EF())
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    ATRANSFER cmd = cmd_mcsDao.getByID(con, cmd_id);
                    if (cmd.TRANSFERSTATE <= E_TRAN_STATUS.Initial)
                    {
                        cmd.TRANSFERSTATE = E_TRAN_STATUS.Initial;
                    }
                    cmd.CMD_START_TIME = DateTime.Now;
                    cmd_mcsDao.update(con, cmd);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exection:");
                isSuccess = false;
            }
            return isSuccess;
        }

        public bool updateTransferCmd_TranStatus2PreInitial(string cmd_id)
        {
            bool isSuccess = true;
            //using (DBConnection_EF con = new DBConnection_EF())
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    ATRANSFER cmd = cmd_mcsDao.getByID(con, cmd_id);
                    if (cmd.TRANSFERSTATE <= E_TRAN_STATUS.PreInitial)
                    {
                        cmd.TRANSFERSTATE = E_TRAN_STATUS.PreInitial;
                    }
                    cmd_mcsDao.update(con, cmd);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exection:");
                isSuccess = false;
            }
            return isSuccess;
        }



        public bool updateCMD_MCS_TranStatus2Transferring(string cmd_id)
        {
            bool isSuccess = true;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                ATRANSFER cmd = cmd_mcsDao.getByID(con, cmd_id);
                if (cmd.TRANSFERSTATE <= E_TRAN_STATUS.Transferring)
                {
                    cmd.TRANSFERSTATE = E_TRAN_STATUS.Transferring;
                }
                cmd_mcsDao.update(con, cmd);
            }
            return isSuccess;
        }
        public bool updateTransferCmd_TranStatus2Canceling(string cmd_id)
        {
            bool isSuccess = true;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                ATRANSFER cmd = cmd_mcsDao.getByID(con, cmd_id);
                if (cmd.TRANSFERSTATE <= E_TRAN_STATUS.Canceling)
                {
                    cmd.TRANSFERSTATE = E_TRAN_STATUS.Canceling;
                }
                cmd_mcsDao.update(con, cmd);
            }
            return isSuccess;
        }
        public bool updateTransferCmd_TranStatus2Canceled(string cmd_id)
        {
            bool isSuccess = true;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                ATRANSFER cmd = cmd_mcsDao.getByID(con, cmd_id);
                if (cmd.TRANSFERSTATE <= E_TRAN_STATUS.Canceled)
                {
                    cmd.TRANSFERSTATE = E_TRAN_STATUS.Canceled;
                }
                cmd.CMD_FINISH_TIME = DateTime.Now;
                cmd_mcsDao.update(con, cmd);
            }
            return isSuccess;
        }


        public bool updateCMD_MCS_TranStatus2Aborting(string cmd_id)
        {
            bool isSuccess = true;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                ATRANSFER cmd = cmd_mcsDao.getByID(con, cmd_id);
                if (cmd.TRANSFERSTATE <= E_TRAN_STATUS.Aborting)
                {
                    cmd.TRANSFERSTATE = E_TRAN_STATUS.Aborting;
                }
                cmd_mcsDao.update(con, cmd);
            }
            return isSuccess;
        }


        //public bool updateCMD_MCS_TranStatus2Queue(string cmd_id)
        //{
        //    bool isSuccess = true;
        //    using (DBConnection_EF con = DBConnection_EF.GetUContext())
        //    {
        //        ATRANSFER cmd = cmd_mcsDao.getByID(con, cmd_id);
        //        cmd.TRANSFERSTATE = E_TRAN_STATUS.Queue;
        //        cmd_mcsDao.update(con, cmd);
        //    }
        //    return isSuccess;
        //}

        public bool updateCMD_MCS_TranStatus2Complete(string cmd_id, CompleteStatus completeStatu)
        {
            E_TRAN_STATUS tran_status = CompleteStatusToETransferStatus(completeStatu);
            return updateCMD_MCS_TranStatus2Complete(cmd_id, tran_status);
        }

        public void moveCMD_MCSToHistory(string command_id)
        {
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                ATRANSFER mcs_cmd = cmd_mcsDao.getByID(con, command_id);
                HTRANSFER hmcs_cmd = mcs_cmd.ToHCMD_MCS();
                cmd_mcsDao.DeleteCmdData(con, mcs_cmd);
                hTransferDao.Add(con, hmcs_cmd);
            }
        }


        private E_TRAN_STATUS CompleteStatusToETransferStatus(CompleteStatus completeStatus)
        {
            switch (completeStatus)
            {
                case CompleteStatus.CmpStatusCancel:
                    return E_TRAN_STATUS.Canceled;
                case CompleteStatus.CmpStatusAbort:
                case CompleteStatus.CmpStatusVehicleAbort:
                case CompleteStatus.CmpStatusIdmisMatch:
                case CompleteStatus.CmpStatusIdreadFailed:
                case CompleteStatus.CmpStatusInterlockError:
                case CompleteStatus.LongTimeInaction:
                //case CompleteStatus.ForceFinishByOp:
                case CompleteStatus.ForceAbnormalFinishByOp:
                case CompleteStatus.CommandInitailFail:
                    return E_TRAN_STATUS.Aborted;
                default:
                    return E_TRAN_STATUS.Complete;
            }
        }

        public bool updateCMD_MCS_TranStatus2Complete(string cmd_id, E_TRAN_STATUS tran_status)
        {
            bool isSuccess = true;
            //DBConnection_EF con = DBConnection_EF.GetContext();
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                ATRANSFER cmd = cmd_mcsDao.getByID(con, cmd_id);
                if (cmd != null)
                {
                    if (cmd.TRANSFERSTATE <= tran_status)
                    {
                        cmd.TRANSFERSTATE = tran_status;
                    }
                    cmd.CMD_FINISH_TIME = DateTime.Now;
                    cmd.COMMANDSTATE = cmd.COMMANDSTATE | ATRANSFER.COMMAND_STATUS_BIT_INDEX_COMMNAD_FINISH;
                    cmd_mcsDao.update(con, cmd);
                }
                else
                {
                    //isSuccess = false;
                }
            }
            return isSuccess;
        }

        public bool updateCMD_MCS_Priority(ATRANSFER mcs_cmd, int priority)
        {
            bool isSuccess = true;
            //using (DBConnection_EF con = new DBConnection_EF())
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    con.ATRANSFER.Attach(mcs_cmd);
                    mcs_cmd.PRIORITY = priority;
                    cmd_mcsDao.update(con, mcs_cmd);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exection:");
                isSuccess = false;
            }
            return isSuccess;
        }
        public bool updateCMD_MCS_CheckCode(string cmd_id, string checkCode)
        {
            bool isSuccess = true;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                ATRANSFER cmd = cmd_mcsDao.getByID(con, cmd_id);
                if (cmd != null)
                {
                    cmd.CHECKCODE = checkCode;
                    cmd_mcsDao.update(con, cmd);
                }
                else
                {
                    //isSuccess = false;
                }
            }
            return isSuccess;

        }
        public bool updateCMD_MCS_TimePriority(string tranID, int timePriority, int sumPriority)
        {
            bool isSuccess = false;
            ATRANSFER tran = new ATRANSFER();
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    tran.ID = tranID;
                    con.ATRANSFER.Attach(tran);
                    tran.TIME_PRIORITY = timePriority;
                    tran.PRIORITY_SUM = sumPriority;
                    con.Entry(tran).Property(p => p.TIME_PRIORITY).IsModified = true;
                    con.Entry(tran).Property(p => p.PRIORITY_SUM).IsModified = true;
                    cmd_mcsDao.update(con, tran);
                    con.Entry(tran).State = EntityState.Detached;
                    isSuccess = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exection:");
                isSuccess = false;
            }

            return isSuccess;
        }
        public bool updateCMD_MCS_TimePriority(ATRANSFER mcs_cmd, int time_priority)
        {
            bool isSuccess = true;
            //using (DBConnection_EF con = new DBConnection_EF())
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    con.ATRANSFER.Attach(mcs_cmd);
                    mcs_cmd.TIME_PRIORITY = time_priority;
                    cmd_mcsDao.update(con, mcs_cmd);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exection:");
                isSuccess = false;
            }
            return isSuccess;
        }

        public bool updateCMD_MCS_PrioritySUM(ATRANSFER mcs_cmd, int priority_sum)
        {
            bool isSuccess = true;
            //using (DBConnection_EF con = new DBConnection_EF())
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    con.ATRANSFER.Attach(mcs_cmd);
                    mcs_cmd.PRIORITY_SUM = priority_sum;
                    cmd_mcsDao.update(con, mcs_cmd);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exection:");
                isSuccess = false;
            }
            return isSuccess;
        }


        public void remoteCMD_MCSByBatch(List<ATRANSFER> mcs_cmds)
        {
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {

                cmd_mcsDao.RemoteByBatch(con, mcs_cmds);
            }
        }


        public ATRANSFER GetTransferByID(string cmd_id)
        {
            ATRANSFER cmd_mcs = null;
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                cmd_mcs = cmd_mcsDao.getByID(con, cmd_id);
            }
            return cmd_mcs;
        }

        public VTRANSFER getVCMD_MCSByID(string cmd_id)
        {
            VTRANSFER cmd_mcs = null;
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                cmd_mcs = vcmd_mcsDao.getVTransferByTransferID(con, cmd_id);
            }
            return cmd_mcs;
        }

        public ATRANSFER getExcuteCMD_MCSByCarrierID(string carrierID)
        {
            ATRANSFER cmd_mcs = null;
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                cmd_mcs = cmd_mcsDao.getExcuteCMDByCSTID(con, carrierID);
            }
            return cmd_mcs;
        }

        public ATRANSFER getWatingCMDByFromTo(string hostSource, string hostDestination)
        {
            ATRANSFER cmd_mcs = null;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                cmd_mcs = cmd_mcsDao.getWatingCMDByFromTo(con, hostSource, hostDestination);
            }
            return cmd_mcs;
        }
        public ATRANSFER getWatingCMDByFrom(string hostSource)
        {
            ATRANSFER cmd_mcs = null;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                cmd_mcs = cmd_mcsDao.getWatingCMDByFrom(con, hostSource);
            }
            return cmd_mcs;
        }

        public bool HasCMD_MCSInQueue()
        {
            return getCMD_MCSIsQueueCount() > 0;
        }
        public int getCMD_MCSIsQueueCount()
        {
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                return cmd_mcsDao.getCMD_MCSIsQueueCount(con);
            }
        }


        public int getCMD_MCSIsRunningCount()
        {
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                return cmd_mcsDao.getCMD_MCSIsExcuteCount(con);
            }
        }

        public int getCMD_MCSIsRunningCount(DateTime befor_time)
        {
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                return cmd_mcsDao.getCMD_MCSIsExcuteCount(con, befor_time);
            }
        }

        public int getCMD_MCSIsUnfinishedCount()
        {
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                return cmd_mcsDao.getCMD_MCSIsUnfinishedCount(con);
            }
        }

        public int getCMD_MCSIsUnfinishedCountByHostSource(List<string> port_ids)
        {
            if (port_ids == null) return 0;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                return cmd_mcsDao.getCMD_MCSIsUnfinishedCountByHostSource(con, port_ids);
            }
        }
        public int getCMD_MCSIsUnfinishedCountByHostDsetination(string port_id)
        {
            if (SCUtility.isEmpty(port_id)) return 0;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                return cmd_mcsDao.getCMD_MCSIsUnfinishedCountByHostDsetination(con, port_id);
            }
        }
        public int getCMD_MCSIsUnfinishedCountByCarrierID(string carrier_id)
        {
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                return cmd_mcsDao.getCMD_MCSIsUnfinishedCountByCarrierID(con, carrier_id);
            }
        }
        public List<ATRANSFER> loadUnfinishedTransfer()
        {
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                return cmd_mcsDao.loadACMD_MCSIsUnfinished(con);
            }
        }

        public List<ATRANSFER> loadFinishCMD_MCS()
        {
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                return cmd_mcsDao.loadFinishCMD_MCS(con);
            }
        }
        public List<VTRANSFER> loadVTransferIsUnfinished()
        {
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                return vcmd_mcsDao.loadVTransferIsUnfinished(con);
            }
        }

        public List<VTRANSFER> loadAllVTransfer()
        {
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                return vcmd_mcsDao.loadAllVTransfer(con);
            }
        }

        public List<ATRANSFER> loadACMD_MCSIsUnfinished(DBConnection_EF con)
        {
            var query = from cmd in con.ATRANSFER.AsNoTracking()
                        where cmd.TRANSFERSTATE >= E_TRAN_STATUS.Queue && cmd.TRANSFERSTATE <= E_TRAN_STATUS.Aborting
                        && cmd.CHECKCODE.Trim() == SECSConst.HCACK_Confirm
                        select cmd;

            return query.ToList();
        }

        public List<ATRANSFER> loadMCS_Command_Queue()
        {
            List<ATRANSFER> ACMD_MCSs = list();
            return ACMD_MCSs;
        }

        public List<ATRANSFER> loadMCS_Command_Executing()
        {
            List<ATRANSFER> ACMD_MCSs = null;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                ACMD_MCSs = cmd_mcsDao.loadACMD_MCSIsExecuting(con);
            }
            return ACMD_MCSs;
        }

        private List<ATRANSFER> list()
        {
            List<ATRANSFER> ACMD_MCSs = null;
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                ACMD_MCSs = cmd_mcsDao.loadACMD_MCSIsQueue(con);
            }
            return ACMD_MCSs;
        }

        private List<ATRANSFER> Sort(List<ATRANSFER> list_cmd_mcs)
        {
            list_cmd_mcs = list_cmd_mcs.OrderBy(cmd => cmd.CMD_INSER_TIME).ToList();
            return list_cmd_mcs;

        }

        public List<ATRANSFER> GetCmdDataByDest(string portName)
        {
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    //A20.06.15.0
                    return cmd_mcsDao.LoadCmdData_WithoutComplete(con).Where(cmdData =>
                                                                  cmdData.HOSTDESTINATION.Trim() == portName.Trim()).ToList();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return null;
            }
        }

        public int getCMD_MCSInserCountLastHour(int hours)
        {
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                return cmd_mcsDao.getCMD_MCSInserCountLastHour(con, hours);
            }
        }
        public int getCMD_MCSFinishCountLastHour(int hours)
        {
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                return cmd_mcsDao.getCMD_MCSFinishCountLastHours(con, hours);
            }
        }
        private long syncTranCmdPoint = 0;
        public bool assignCommnadToVehicle(string mcs_id, string vh_id, out string result)
        {
            try
            {
                ATRANSFER ACMD_MCS = scApp.CMDBLL.GetTransferByID(mcs_id);
                if (ACMD_MCS != null)
                {
                    bool check_result = true;
                    result = "OK";
                    //ACMD_MCS excute_cmd = ACMD_MCSs[0];
                    string hostsource = ACMD_MCS.HOSTSOURCE;
                    string hostdest = ACMD_MCS.HOSTDESTINATION;
                    string from_adr = string.Empty;
                    string to_adr = string.Empty;
                    AVEHICLE vh = null;
                    E_VH_TYPE vh_type = E_VH_TYPE.None;
                    E_CMD_TYPE cmd_type = default(E_CMD_TYPE);

                    //確認 source 是否為Port
                    bool source_is_a_port = scApp.PortStationBLL.OperateCatch.IsExist(hostsource);
                    if (source_is_a_port)
                    {
                        scApp.MapBLL.getAddressID(hostsource, out from_adr, out vh_type);
                        vh = scApp.VehicleBLL.cache.getVehicle(vh_id);
                        cmd_type = E_CMD_TYPE.LoadUnload;
                    }
                    else
                    {
                        result = "Source must be a port.";
                        return false;
                    }
                    scApp.MapBLL.getAddressID(hostdest, out to_adr);
                    if (vh != null)
                    {
                        if (vh.ACT_STATUS != VHActionStatus.Commanding)
                        {
                            bool btemp = AssignMCSCommand2Vehicle(ACMD_MCS, cmd_type, vh);
                            if (!btemp)
                            {
                                result = "Assign command to vehicle failed.";
                                return false;
                            }
                        }
                        else
                        {
                            result = "Vehicle already have command.";
                            return false;

                        }

                    }
                    else
                    {
                        result = $"Can not find vehicle:{vh_id}.";
                        return false;
                    }
                    return true;
                }
                else
                {
                    result = $"Can not find command:{mcs_id}.";
                    return false;
                }
            }
            finally
            {
                System.Threading.Interlocked.Exchange(ref syncTranCmdPoint, 0);
            }
        }



        const int HIGHT_PRIORITY_VALUE = 99;



        private bool AssignMCSCommand2Vehicle(ATRANSFER waittingExcuteMcsCmd, E_CMD_TYPE cmdType, AVEHICLE bestSuitableVh)
        {
            string hostsource = waittingExcuteMcsCmd.HOSTSOURCE;
            string hostdest = waittingExcuteMcsCmd.HOSTDESTINATION;
            var source_port = scApp.PortStationBLL.OperateCatch.getPortStation(hostsource);
            var dest_port = scApp.PortStationBLL.OperateCatch.getPortStation(hostdest);
            string from_adr = source_port == null ? string.Empty : source_port.ADR_ID;
            string to_adr = dest_port == null ? string.Empty : dest_port.ADR_ID;

            return AssignMCSCommand2Vehicle(waittingExcuteMcsCmd, from_adr, to_adr, cmdType, bestSuitableVh);
        }
        private bool AssignMCSCommand2Vehicle(ATRANSFER waittingExcuteMcsCmd, string fromAdr, string toAdr, E_CMD_TYPE cmdType, AVEHICLE bestSuitableVh)
        {
            bool isSuccess = true;
            string best_suitable_vh_id = bestSuitableVh.VEHICLE_ID;
            string mcs_cmd_id = waittingExcuteMcsCmd.ID;
            string carrier_id = waittingExcuteMcsCmd.CARRIER_ID;
            List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
            using (TransactionScope tx = SCUtility.getTransactionScope())
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {

                    isSuccess &= scApp.CMDBLL.doCreatCommand(best_suitable_vh_id, mcs_cmd_id, carrier_id,
                                        cmdType,
                                        fromAdr,
                                        toAdr, 0, 0);
                    if (isSuccess)
                    {
                        isSuccess &= scApp.CMDBLL.updateTransferCmd_TranStatus2PreInitial(waittingExcuteMcsCmd.ID);
                    }
                    scApp.ReportBLL.insertMCSReport(reportqueues);
                    if (isSuccess)
                    {
                        tx.Complete();
                    }
                    else
                    {
                        return isSuccess;
                    }
                }

            }
            scApp.ReportBLL.newSendMCSMessage(reportqueues);
            scApp.VehicleService.Command.Scan();
            return isSuccess;
        }

        public List<TranTask> loadTranTasks()
        {
            return testTranTaskDao.loadTransferTasks_ACycle(scApp.TranCmdPeriodicDataSet.Tables[0]);
        }

        public Dictionary<int, List<TranTask>> loadTranTaskSchedule_24Hour()
        {
            List<TranTask> allTranTaskType = testTranTaskDao.loadTransferTasks_24Hour(scApp.TranCmdPeriodicDataSet.Tables[0]);
            Dictionary<int, List<TranTask>> dicTranTaskSchedule = new Dictionary<int, List<TranTask>>();
            var query = from tranTask in allTranTaskType
                        group tranTask by tranTask.Sec;

            dicTranTaskSchedule = query.OrderBy(item => item.Key).ToDictionary(item => item.Key, item => item.ToList());

            return dicTranTaskSchedule;
        }
        public Dictionary<string, List<TranTask>> loadTranTaskSchedule_Clear_Dirty()
        {
            List<TranTask> allTranTaskType = testTranTaskDao.loadTransferTasks_24Hour(scApp.TranCmdPeriodicDataSet.Tables[1]);
            Dictionary<string, List<TranTask>> dicTranTaskSchedule = new Dictionary<string, List<TranTask>>();
            var query = from tranTask in allTranTaskType
                        group tranTask by tranTask.CarType;

            dicTranTaskSchedule = query.OrderBy(item => item.Key).ToDictionary(item => item.Key, item => item.ToList());

            return dicTranTaskSchedule;
        }

        #endregion CMD_MCS

        #region CMD_OHTC
        public const string CALL_CONTEXT_KEY_WORD_OHTC_CMD_CHECK_RESULT = "OHTC_CMD_CHECK_RESULT";
        public class CommandCheckResult
        {
            public CommandCheckResult()
            {
                Num = DateTime.Now.ToString(SCAppConstants.TimestampFormat_19);
                IsSuccess = true;
            }
            public string Num { get; private set; }
            public bool IsSuccess = false;
            public StringBuilder Result = new StringBuilder();
            public override string ToString()
            {
                string message = "Alarm No.:" + Num + Environment.NewLine + Environment.NewLine + Result.ToString();
                return message;
            }
        }
        public bool checkCmd(ACMD cmd)
        {
            string vh_id = cmd.VH_ID;
            E_CMD_TYPE cmd_type = cmd.CMD_TYPE;
            string source = cmd.SOURCE;
            string destination = cmd.DESTINATION;

            CommandCheckResult check_result = getOrSetCallContext<CommandCheckResult>(CALL_CONTEXT_KEY_WORD_OHTC_CMD_CHECK_RESULT);
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            string vh_current_adr = vh.CUR_ADR_ID;

            if (SCUtility.isEmpty(vh_id) || vh == null)
            {
                check_result.Result.AppendLine($" vh id is empty");
                check_result.IsSuccess = false;
            }

            if (!vh.isTcpIpConnect)
            {
                check_result.Result.AppendLine($" vh:{vh_id} no connection");
                check_result.Result.AppendLine($" please check IPC.");
                check_result.IsSuccess = false;
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(CMDBLL), Device: string.Empty,
                              Data: check_result.Result.ToString(),
                              XID: check_result.Num);
                return check_result.IsSuccess;
            }

            if (vh.MODE_STATUS == VHModeStatus.Manual)
            {
                check_result.Result.AppendLine($" vh:{vh_id} is manual");
                check_result.Result.AppendLine($" please change to auto mode.");
                check_result.IsSuccess = false;
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(CMDBLL), Device: string.Empty,
                              Data: check_result.Result.ToString(),
                              XID: check_result.Num);
                return check_result.IsSuccess;
            }

            if (SCUtility.isEmpty(vh_current_adr))
            {
                check_result.Result.AppendLine($" vh:{vh_id} current address is empty");
                check_result.Result.AppendLine($" please excute home command.");
                check_result.Result.AppendLine();
                check_result.IsSuccess = false;
            }
            else
            {
                string result = "";
                if (!IsCommandWalkable(vh_id, cmd_type, vh_current_adr, source, destination, out result))
                {
                    check_result.Result.AppendLine(result);
                    check_result.Result.AppendLine($" please check the road traffic status.");
                    check_result.Result.AppendLine();
                    check_result.IsSuccess = false;
                }
            }
            //if (!canAssignCmd(vh_id))
            //var checkAssignCmdResult = canAssignCmdNew(vh_id, cmd.CMD_TYPE);
            //var checkAssignCmdResult = canAssignCmdNew(vh, cmd.CMD_TYPE);
            CommandTranDir transferDir = GetTransferDir(cmd.SOURCE_PORT, cmd.DESTINATION_PORT);
            //var checkAssignCmdResult = ICanAssignCmd(vh, cmd.CMD_TYPE, transferDir);
            var checkAssignCmdResult = CanAssignCmd(vh, cmd.CMD_TYPE, transferDir);
            if (!checkAssignCmdResult.canAssign)
            {
                check_result.Result.AppendLine(checkAssignCmdResult.result);
                check_result.IsSuccess = false;
            }
            return check_result.IsSuccess;
        }

        public static CommandTranDir GetTransferDir(VTRANSFER vTran)
        {
            string source_port = SCUtility.Trim(vTran.HOSTSOURCE, true);
            string dest_port = SCUtility.Trim(vTran.HOSTDESTINATION, true);
            bool is_agv_st_source_port = scApp.PortStationBLL.OperateCatch.IsAGVStationPort(scApp.EqptBLL, source_port);
            if (is_agv_st_source_port)
            {
                return CommandTranDir.OutAGVStation;
            }

            bool is_agv_st_destination_port = scApp.PortStationBLL.OperateCatch.IsAGVStationPort(scApp.EqptBLL, dest_port);
            if (is_agv_st_destination_port)
            {
                return CommandTranDir.InAGVStation;
            }
            return CommandTranDir.OutAGVStation; //當 source port並非st 且 dest 也非st時將它作為出庫的命令
        }

        public static CommandTranDir GetTransferDir(string sourcePort, string destPort)
        {
            bool is_agv_st_source_port = scApp.PortStationBLL.OperateCatch.IsAGVStationPort(scApp.EqptBLL, sourcePort);
            if (is_agv_st_source_port)
            {
                return CommandTranDir.OutAGVStation;
            }

            bool is_agv_st_destination_port = scApp.PortStationBLL.OperateCatch.IsAGVStationPort(scApp.EqptBLL, destPort);
            if (is_agv_st_destination_port)
            {
                return CommandTranDir.InAGVStation;
            }
            return CommandTranDir.None;
        }

        public bool doCreatCommand(string vh_id, string cmd_id_mcs = "", string carrier_id = "", E_CMD_TYPE cmd_type = E_CMD_TYPE.Move,
                                   string source = "", string destination = "", int priority = 0, int estimated_time = 0, SCAppConstants.GenOHxCCommandType gen_cmd_type = SCAppConstants.GenOHxCCommandType.Auto,
                                   string sourcePort = "", string destinationPort = "")
        {
            ACMD cmd_obj = null;

            return doCreatCommand(vh_id, out cmd_obj, cmd_id_mcs, carrier_id, cmd_type,
                                    source, destination, priority, estimated_time,
                                    gen_cmd_type, sourcePort, destinationPort);
        }

        public bool doCreatCommand(string vh_id, out ACMD cmd_obj, string cmd_id_mcs = "", string carrier_id = "", E_CMD_TYPE cmd_type = E_CMD_TYPE.Move,
                                    string source = "", string destination = "", int priority = 0, int estimated_time = 0, SCAppConstants.GenOHxCCommandType gen_cmd_type = SCAppConstants.GenOHxCCommandType.Auto,
                                    string sourcePort = "", string destinationPort = "")
        {
            CommandCheckResult check_result = getOrSetCallContext<CommandCheckResult>(CALL_CONTEXT_KEY_WORD_OHTC_CMD_CHECK_RESULT);
            cmd_obj = null;
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            string vh_current_adr = vh.CUR_ADR_ID;
            //不是MCS Cmd，要檢查檢查有沒有在執行中的，有則不能Creat

            lock (vh.creatCmdAsyncObj)
            {
                if (SCUtility.isEmpty(vh_id) || vh == null)
                {
                    check_result.Result.AppendLine($" vh id is empty");
                    check_result.IsSuccess = false;
                }

                if (!vh.isTcpIpConnect)
                {
                    check_result.Result.AppendLine($" vh:{vh_id} no connection");
                    check_result.Result.AppendLine($" please check IPC.");
                    check_result.IsSuccess = false;
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(CMDBLL), Device: string.Empty,
                                  Data: check_result.Result.ToString(),
                                  XID: check_result.Num);
                    return check_result.IsSuccess;
                }

                if (vh.ACT_STATUS  != VHActionStatus.NoCommand)
                {
                    check_result.Result.AppendLine($" vh:{vh_id} action status is not NoCommnad, ACT_STATUS:{vh.ACT_STATUS}");
                    check_result.Result.AppendLine($" please wait vehicle command status.");
                    check_result.IsSuccess = false;
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(CMDBLL), Device: string.Empty,
                                  Data: check_result.Result.ToString(),
                                  XID: check_result.Num);
                    return check_result.IsSuccess;
                }


                if (vh.MODE_STATUS == VHModeStatus.Manual)
                {
                    check_result.Result.AppendLine($" vh:{vh_id} is manual");
                    check_result.Result.AppendLine($" please change to auto mode.");
                    check_result.IsSuccess = false;
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(CMDBLL), Device: string.Empty,
                                  Data: check_result.Result.ToString(),
                                  XID: check_result.Num);
                    return check_result.IsSuccess;
                }

                if (SCUtility.isEmpty(vh_current_adr))
                {
                    check_result.Result.AppendLine($" vh:{vh_id} current address is empty");
                    check_result.Result.AppendLine($" please excute home command.");
                    check_result.Result.AppendLine();
                    check_result.IsSuccess = false;
                }
                else
                {
                    string result = "";
                    if (!IsCommandWalkable(vh_id, cmd_type, vh_current_adr, source, destination, out result))
                    {
                        check_result.Result.AppendLine(result);
                        check_result.Result.AppendLine($" please check the road traffic status.");
                        check_result.Result.AppendLine();
                        check_result.IsSuccess = false;
                    }
                }

                //var check_can_assign_result = canAssignCmdNew(vh_id, cmd_type);
                //var check_can_assign_result = canAssignCmdNew(vh, cmd_type);
                CommandTranDir transferDir = GetTransferDir(sourcePort, destinationPort);
                //var check_can_assign_result = ICanAssignCmd(vh, cmd_type, transferDir);
                var check_can_assign_result = CanAssignCmd(vh, cmd_type, transferDir);

                if (!check_can_assign_result.canAssign)
                {
                    check_result.Result.AppendLine(check_can_assign_result.result);
                    check_result.IsSuccess = false;
                }

                if (check_result.IsSuccess)
                {
                    check_result.IsSuccess = creatCommand_OHTC
                                             (vh_id, cmd_id_mcs, carrier_id, cmd_type, source, destination, priority, estimated_time, gen_cmd_type, out cmd_obj,
                                             sourcePort: sourcePort, destinationPort: destinationPort);

                    if (!check_result.IsSuccess)
                    {
                        check_result.Result.AppendLine($" vh:{vh_id} creat command to db unsuccess.");
                    }
                }


                if (!check_result.IsSuccess)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(CMDBLL), Device: string.Empty,
                                  Data: check_result.Result.ToString(),
                                  XID: check_result.Num);
                }
            }
            setCallContext(CALL_CONTEXT_KEY_WORD_OHTC_CMD_CHECK_RESULT, check_result);
            return check_result.IsSuccess;
        }

        public static T getCallContext<T>(string key)
        {
            object obj = System.Runtime.Remoting.Messaging.CallContext.GetData(key);
            if (obj == null)
            {
                return default(T);
            }
            return (T)obj;
        }
        public static T getOrSetCallContext<T>(string key)
        {
            //object obj = System.Runtime.Remoting.Messaging.CallContext.GetData(key);
            //if (obj == null)
            //{
            //    obj = Activator.CreateInstance(typeof(T));
            //    System.Runtime.Remoting.Messaging.CallContext.SetData(key, obj);
            //}
            object obj = Activator.CreateInstance(typeof(T));
            System.Runtime.Remoting.Messaging.CallContext.SetData(key, obj);
            return (T)obj;
        }
        public static void setCallContext<T>(string key, T obj)
        {
            if (obj != null)
            {
                System.Runtime.Remoting.Messaging.CallContext.SetData(key, obj);
            }
        }

        public void remoteCMDByBatch(List<ACMD> cmds)
        {
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                cmd_ohtcDAO.RemoteByBatch(con, cmds);
            }
        }



        protected bool IsCommandWalkable(string vh_id, E_CMD_TYPE cmd_type, string vh_current_adr, string source, string destination, out string result)
        {
            bool is_walk_able = true;
            switch (cmd_type)
            {
                case E_CMD_TYPE.Move:
                case E_CMD_TYPE.Unload:
                case E_CMD_TYPE.Move_Park:
                case E_CMD_TYPE.Move_Charger:
                case E_CMD_TYPE.Move_MTL:
                case E_CMD_TYPE.SystemIn:
                case E_CMD_TYPE.SystemOut:
                case E_CMD_TYPE.MTLHome:
                    if (!scApp.GuideBLL.IsRoadWalkable(vh_current_adr, destination))
                    {
                        result = $" vh:{vh_id} current address:[{vh_current_adr}] to destination address:[{destination}] no traffic allowed";
                        is_walk_able = false;
                    }
                    else
                    {
                        result = "";
                    }
                    break;
                case E_CMD_TYPE.Load:
                    if (!scApp.GuideBLL.IsRoadWalkable(vh_current_adr, source))
                    {
                        result = $" vh:{vh_id} current address:[{vh_current_adr}] to destination address:[{source}] no traffic allowed";
                        is_walk_able = false;
                    }
                    else
                    {
                        result = "";
                    }
                    break;
                case E_CMD_TYPE.LoadUnload:
                    if (!scApp.GuideBLL.IsRoadWalkable(vh_current_adr, source))
                    {
                        result = $" vh:{vh_id} current address:{vh_current_adr} to source address:{source} no traffic allowed";
                        is_walk_able = false;
                    }
                    else if (!scApp.GuideBLL.IsRoadWalkable(source, destination))
                    {
                        result = $" vh:{vh_id} source address:{source} to destination address:{destination} no traffic allowed";
                        is_walk_able = false;
                    }
                    else
                    {
                        result = "";
                    }
                    break;
                default:
                    result = $"Incorrect of command type:{cmd_type}";
                    is_walk_able = false;
                    break;
            }

            return is_walk_able;
        }



        protected bool creatCommand_OHTC(string vh_id, string cmd_id_mcs, string carrier_id, E_CMD_TYPE cmd_type,
                                              string source, string destination, int priority, int estimated_time, SCAppConstants.GenOHxCCommandType gen_cmd_type,
                                              out ACMD cmd_ohtc,
                                              string sourcePort = "", string destinationPort = "")

        {
            try
            {
                cmd_ohtc = buildCommand_OHTC(vh_id, cmd_id_mcs, carrier_id, cmd_type, source, destination, priority, estimated_time, gen_cmd_type,
                                             sourcePortID: sourcePort, destinationPortID: destinationPort);

                addCmd(cmd_ohtc);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(CMDBLL), Device: "OHTC",
                   Data: ex,
                   VehicleID: vh_id,
                   CarrierID: carrier_id);
                cmd_ohtc = null;
                return false;
            }
            return true;
        }



        public bool addCmd(ACMD cmd)
        {
            cmd.COMPLETE_STATUS = CompleteStatus.CmpStatusMove;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                cmd_ohtcDAO.add(con, cmd);
            }
            return true;
        }

        private ACMD buildCommand_OHTC(string vh_id, string cmd_id_mcs, string carrier_id, E_CMD_TYPE cmd_type,
                                            string source, string destination, int priority, int estimated_time,
                                            SCAppConstants.GenOHxCCommandType gen_cmd_type, bool is_generate_cmd_id = true,
                                            string sourcePortID = "", string destinationPortID = "")
        {
            string _source = string.Empty;
            string commandID = string.Empty;
            if (is_generate_cmd_id)
            {
                commandID = scApp.SequenceBLL.getCommandID(gen_cmd_type);
            }
            if (cmd_type == E_CMD_TYPE.LoadUnload
                || cmd_type == E_CMD_TYPE.Load)
            {
                _source = source;
            }

            if(cmd_type == E_CMD_TYPE.Move)//20210421 mark趕車需求要記錄命令起點為何
            {
                _source = source;
            }

            ACMD cmd = new ACMD
            {
                ID = commandID,
                CMD_INSER_TIME = DateTime.Now,
                VH_ID = vh_id,
                CARRIER_ID = carrier_id,
                TRANSFER_ID = cmd_id_mcs,
                CMD_TYPE = cmd_type,
                SOURCE = _source,
                DESTINATION = destination,
                PRIORITY = priority,
                CMD_STATUS = E_CMD_STATUS.Queue,
                CMD_PROGRESS = 0,
                ESTIMATED_TIME = estimated_time,
                ESTIMATED_EXCESS_TIME = estimated_time,
                SOURCE_PORT = sourcePortID,
                DESTINATION_PORT = destinationPortID,
                COMPLETE_STATUS = CompleteStatus.CmpStatusMove
            };
            return cmd;
        }

        /// <summary>
        /// 根據Command ID更新OHTC的Command狀態
        /// </summary>
        /// <param name="cmd_id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool updateCommand_OHTC_StatusByCmdID(string vhID, string cmd_id, E_CMD_STATUS status)
        {
            bool isSuccess = false;
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                ACMD cmd = cmd_ohtcDAO.getByID(con, cmd_id);
                if (cmd != null)
                {
                    if (status == E_CMD_STATUS.Execution)
                    {
                        cmd.CMD_START_TIME = DateTime.Now;
                    }
                    else if (status >= E_CMD_STATUS.NormalEnd)
                    {
                        cmd.CMD_END_TIME = DateTime.Now;
                        //cmd_ohtc_detailDAO.DeleteByBatch(con, cmd.ID);
                    }
                    cmd.CMD_STATUS = status;
                    cmd_ohtcDAO.Update(con, cmd);

                    if (status >= E_CMD_STATUS.NormalEnd)
                    {
                        scApp.VehicleBLL.db.updateVehicleExcuteCMD(vhID, string.Empty, string.Empty);
                    }

                }
                isSuccess = true;
            }
            return isSuccess;
        }
        public bool updateCommand_OHTC_StatusToFinish(string cmd_id, CompleteStatus completeStatus)
        {
            E_CMD_STATUS cmd_status = CompleteStatusToECmdStatus(completeStatus);
            return updateCommand_OHTC_StatusToFinish(cmd_id, cmd_status, completeStatus);
        }
        public static E_CMD_STATUS CompleteStatusToECmdStatus(CompleteStatus completeStatus)
        {
            switch (completeStatus)
            {
                case CompleteStatus.CmpStatusCancel:
                    return E_CMD_STATUS.CancelEndByOHTC;
                case CompleteStatus.CmpStatusAbort:
                    return E_CMD_STATUS.AbnormalEndByOHTC;
                case CompleteStatus.CmpStatusVehicleAbort:
                case CompleteStatus.CmpStatusIdmisMatch:
                case CompleteStatus.CmpStatusIdreadFailed:
                case CompleteStatus.CmpStatusInterlockError:
                case CompleteStatus.LongTimeInaction:
                    return E_CMD_STATUS.AbnormalEndByOHT;
                //case CompleteStatus.ForceFinishByOp:
                case CompleteStatus.ForceAbnormalFinishByOp:
                    return E_CMD_STATUS.AbnormalEndByOHTC;
                default:
                    return E_CMD_STATUS.NormalEnd;
            }
        }
        public bool updateCommand_OHTC_StatusToFinish(string cmd_id, E_CMD_STATUS status, CompleteStatus completeStatus)
        {
            bool isSuccess = false;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                ACMD cmd = cmd_ohtcDAO.getByID(con, cmd_id);
                if (cmd != null)
                {
                    cmd.CMD_END_TIME = DateTime.Now;
                    cmd.CMD_STATUS = status;
                    cmd.COMPLETE_STATUS = completeStatus;
                    cmd_ohtcDAO.Update(con, cmd);
                }
                isSuccess = true;
            }
            return isSuccess;
        }
        public bool updateCMD_OHxC_Status2ReadyToReWirte(string cmd_id, out ACMD cmd_ohtc)
        {
            bool isSuccess = false;
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                cmd_ohtc = cmd_ohtcDAO.getByID(con, cmd_id);
                //if (cmd != null)
                //{
                //    cmd_ohtc = cmd;
                //    //cmd.CMD_STAUS = E_CMD_STATUS.Queue;
                //    //cmd.CMD_TPYE = E_CMD_TYPE.Override;
                //    //cmd.CMD_TPYE = E_CMD_TYPE.
                //}
                //else
                isSuccess = true;
            }
            return isSuccess;
        }


        public List<ACMD> loadCMD_OHTCMDStatusIsQueue()
        {
            List<ACMD> acmd_ohtcs = null;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                acmd_ohtcs = cmd_ohtcDAO.loadAllQueue_Auto(con);
            }
            return acmd_ohtcs;
        }
        public List<ACMD> loadUnfinishCmd()
        {
            List<ACMD> acmd_ohtcs = null;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                acmd_ohtcs = cmd_ohtcDAO.loadUnfinishCmd(con);
            }
            return acmd_ohtcs;
        }

        public List<ACMD> loadfinishCmd()
        {
            List<ACMD> acmd_ohtcs = null;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                acmd_ohtcs = cmd_ohtcDAO.loadfinishCmd(con);
            }
            return acmd_ohtcs;
        }

        public ACMD getExcuteCMD_OHTCByCmdID(string cmd_id)
        {
            ACMD cmd_ohtc = null;
            using (DBConnection_EF con = new DBConnection_EF())
            {
                cmd_ohtc = cmd_ohtcDAO.getExcuteCMD_OHTCByCmdID(con, cmd_id);
            }
            return cmd_ohtc;
        }

        public ACMD GetCMD_OHTCByID(string cmdID)
        {
            ACMD cmd_ohtc = null;
            using (DBConnection_EF con = new DBConnection_EF())
            {
                cmd_ohtc = cmd_ohtcDAO.getByID(con, cmdID);
            }
            return cmd_ohtc;
        }
        public ACMD GetCommandByTransferCmdID(string cmdID)
        {
            ACMD cmd_ohtc = null;
            using (DBConnection_EF con = new DBConnection_EF())
            {
                cmd_ohtc = cmd_ohtcDAO.getByTransferCmdID(con, cmdID);
            }
            return cmd_ohtc;
        }
        public string getTransferCmdIDByCmdID(string cmdID)
        {
            string transfer_cmd_id = null;
            using (DBConnection_EF con = new DBConnection_EF())
            {
                transfer_cmd_id = cmd_ohtcDAO.getTransferCmdIDByCmdID(con, cmdID);
            }
            return transfer_cmd_id;
        }
        public bool isCMD_OHTCQueueByVh(string vh_id)
        {
            int count = 0;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                count = cmd_ohtcDAO.getVhQueueCMDConut(con, vh_id);
            }
            return count != 0;
        }


        public bool HasCmdIsGoing(List<string> addresses)
        {
            int count = 0;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                count = cmd_ohtcDAO.getAddressesExcuteCMDConut(con, addresses);
            }
            return count > 0;
        }

        public (bool hasAssign, List<string> assignCmdIDs) hasAssignCmd(AVEHICLE vh)
        {
            string vh_id = vh.VEHICLE_ID;
            List<string> assign_cmd_ids = null;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                assign_cmd_ids = cmd_ohtcDAO.loadAssignCmdID(con, vh_id);
            }
            return (assign_cmd_ids != null && assign_cmd_ids.Count > 0, assign_cmd_ids);
        }
        public bool hasAssignCmdIgnoreMove(AVEHICLE vh)
        {
            string vh_id = vh.VEHICLE_ID;
            int assign_cmd_count = 0;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                assign_cmd_count = cmd_ohtcDAO.loadAssignCmdIDIgnoreMove(con, vh_id);
            }
            return assign_cmd_count > 0;
        }
        //public const int MAX_ASSIGN_CMD_COUNT = 2;
        //public const int MAX_ASSIGN_CMD_COUNT_FOR_SWAP = 4;
        //private int CurrentCanAssignMAXCount = 2;
        //public void setCurrentCanAssignCmdCount(ShelfStatus shelfStatusL, ShelfStatus shelfStatusR)
        //{
        //    int can_assign_coun = 0;
        //    if (shelfStatusR == ShelfStatus.Enable)
        //    {
        //        can_assign_coun++;
        //    }
        //    if (shelfStatusL == ShelfStatus.Enable)
        //    {
        //        can_assign_coun++;
        //    }
        //    CurrentCanAssignMAXCount = can_assign_coun;
        //}

        public (bool canAssign, string result) canAssignCmdNew(AVEHICLE vh, E_CMD_TYPE cmdType)
        {
            try
            {
                string vh_id = vh.VEHICLE_ID;
                List<ACMD> assign_cmds = null;
                //1.如果是Move則需要在沒有執行任何命令下才可以指派
                //2.如果是Trasfer命令，則需要在
                //  a.沒有Move命令下才可以指派
                //  b.已經指派的命令要小於2
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    assign_cmds = cmd_ohtcDAO.loadAssignCmd(con, vh_id);
                }
                //int current_vh_carrier_count = 0;
                int current_can_assign_command_count = 1;
                if (vh.HAS_CST)
                {
                    current_can_assign_command_count = 0;
                }


                //if (assign_cmds.Count == 0)
                //    return (true, "");
                //else
                //{
                switch (cmdType)
                {
                    case E_CMD_TYPE.Move:
                    case E_CMD_TYPE.Move_Charger:
                        if (assign_cmds.Count == 0)
                        {
                            return (true, "");
                        }
                        else
                        {
                            return (false, "has command excute, can't assign move/move to change commmand");
                        }
                    case E_CMD_TYPE.Unload:
                        if (assign_cmds.Count == 0)
                        {
                            return (true, "");
                        }
                        else
                        {
                            bool has_move_command = assign_cmds.Where(cmd => cmd.IsMoveCommand).Count() != 0;
                            if (has_move_command)
                            {
                                return (false, "has move command excute, can't assign transfer commmand");
                            }
                            else
                            {
                                return (true, "");
                            }
                        }
                    //return (true, "");//Unload一律都回覆OK，不然遇到身上已經載2個CST的，會無法下命令
                    default:
                        if (assign_cmds.Count == 0)
                        {
                            if (current_can_assign_command_count == 0)
                            {
                                return (false, $"currrent can assign cmd count:{current_can_assign_command_count}");
                            }
                            else
                            {
                                return (true, "");
                            }
                        }
                        else
                        {
                            bool has_move_command = assign_cmds.Where(cmd => cmd.IsMoveCommand).Count() != 0;
                            if (has_move_command)
                            {
                                return (false, "has move command excute, can't assign transfer commmand");
                            }
                            else
                            {
                                //if (assign_cmds.Count < MAX_ASSIGN_COM_COUNT)
                                if (assign_cmds.Count < current_can_assign_command_count)
                                {
                                    return (true, "");
                                }
                                else
                                {
                                    return (false, $"vh:{vh_id} can assign count:[{current_can_assign_command_count}], has [{assign_cmds.Count}] commands excute and current carrier [{1}] cst," +
                                                   $" can't assign transfer commmand");
                                }
                            }
                        }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return (false, ex.ToString());
            }
            //}
        }


        public bool canSendCmd(AVEHICLE vh)
        {
            int count = 0;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                count = cmd_ohtcDAO.getVhExcuteCMDConut(con, vh.VEHICLE_ID);
            }
            //int current_can_assign_command_count = scApp.MaxCommandCount;
            //if (SystemParameter.IsByPassAGVShelfStatus)
            //{
            //    current_can_assign_command_count = scApp.MaxCommandCount;
            //}
            //else
            //{
            //    current_can_assign_command_count = vh.CurrentAvailableShelf;
            //}
            return count == 0;
        }

        public bool forceUpdataCmdStatus2FnishByVhID(string vh_id)
        {
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                List<ACMD> cmds = cmd_ohtcDAO.loadUnfinishCmd(con, vh_id);
                if (cmds != null && cmds.Count > 0)
                {
                    foreach (ACMD cmd in cmds)
                    {
                        //updateCommand_OHTC_StatusByCmdID(cmd.CMD_ID, E_CMD_STATUS.AbnormalEndByOHTC);
                        updateCommand_OHTC_StatusByCmdID(vh_id, cmd.ID, E_CMD_STATUS.AbnormalEndByOHTC);
                        if (!SCUtility.isEmpty(cmd.TRANSFER_ID))
                        {
                            scApp.CMDBLL.updateCMD_MCS_TranStatus2Complete(cmd.TRANSFER_ID, E_TRAN_STATUS.Aborted);
                        }
                    }
                    cmd_ohtcDAO.Update(con, cmds);
                }
            }
            return true;
        }

        public bool hasExcuteCMDBySourcePort(string sourcePort)
        {
            int count = 0;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                count = cmd_ohtcDAO.getSourcePortExcuteCMDConut(con, sourcePort);
            }
            return count != 0;
        }

        public bool hasExcuteCMDByDestinationPort(string destinationPort)
        {
            int count = 0;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                count = cmd_ohtcDAO.getDestinationPortExcuteCMDConut(con, destinationPort);
            }
            return count != 0;
        }

        public List<ACMD> loadUnfinishCmd(string vh_id)
        {
            List<ACMD> cmds = null;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                cmds = cmd_ohtcDAO.loadUnfinishCmd(con, vh_id);
            }
            return cmds;
        }

        public (bool canAssign, string result) CanAssignCmd(AVEHICLE vh, E_CMD_TYPE cmdType, CommandTranDir transferDir)
        {
            var command_checker = GetCommandChecker(vh);
            return command_checker.CanAssignCommand(vh, cmdType, transferDir);
        }
        public ICommandChecker GetCommandChecker(AVEHICLE vh)
        {
            switch (vh.VEHICLE_TYPE)
            {
                default:
                    return commandCheckerNormal;
            }
        }

        public bool canSendCmdNew(AVEHICLE vh)
        {
            var command_checker = GetCommandChecker(vh);
            return command_checker.canSendCmdNew(vh);
        }





        #endregion CMD_OHTC

        #region CMD_OHTC_DETAIL

        public bool tryGenerateCmd_OHTC_Details(ACMD acmd_ohtc, out ActiveType active_type, out string[] route_sections, out string[] cycle_run_sections
                                                                                    , out string[] minRouteSec_Vh2From, out string[] minRouteSec_From2To
                                                                                    , out string[] minRouteAdr_Vh2From, out string[] minRouteAdr_From2To)
        {
            active_type = default(ActiveType);
            route_sections = null;
            cycle_run_sections = null;
            minRouteSec_Vh2From = null;
            minRouteSec_From2To = null;
            minRouteAdr_Vh2From = null;
            minRouteAdr_From2To = null;
            try
            {
                if (acmd_ohtc == null)
                {
                    return false;
                }

                string Reason = string.Empty;
                SCUtility.TrimAllParameter(acmd_ohtc);
                AVEHICLE vehicle = scApp.VehicleBLL.cache.getVehicle(acmd_ohtc.VH_ID);
                SCUtility.TrimAllParameter(vehicle);

                List<string> minRouteSeg = new List<string>();
                string[] minRouteSec_Vh2FromTemp = null;
                string[] minRouteSec_From2ToTemp = null;
                string[] minRouteAdr_Vh2FromTemp = null;
                string[] minRouteAdr_From2ToTemp = null;

                string cmd_id = acmd_ohtc.ID;
                string vh_current_adr = vehicle.CUR_ADR_ID;
                string source_adr = acmd_ohtc.SOURCE;
                string dest_adr = acmd_ohtc.DESTINATION;
                ActiveType activeType = ActiveType.Move;
                activeType = convert_E_CMD_TYPE2ActiveType(acmd_ohtc);
                var guide_info = FindGuideInfo(vh_current_adr, source_adr, dest_adr, activeType);

                if (guide_info.isSuccess)
                {
                    if (guide_info.guide_start_to_from_section_ids != null)
                    {
                        minRouteSec_Vh2FromTemp = guide_info.guide_start_to_from_section_ids.ToArray();
                        minRouteAdr_Vh2FromTemp = guide_info.guide_start_to_from_address_ids.ToArray();
                        minRouteSeg.AddRange(guide_info.guide_start_to_from_section_ids);
                    }
                    if (guide_info.guide_to_dest_section_ids != null)
                    {
                        minRouteSec_From2ToTemp = guide_info.guide_to_dest_section_ids.ToArray();
                        minRouteAdr_From2ToTemp = guide_info.guide_to_dest_address_ids.ToArray();
                        minRouteSeg.AddRange(guide_info.guide_to_dest_section_ids);
                    }
                }
                else
                {
                    throw new Exception(string.Format("can't find from to of route.cmd id:{0}", acmd_ohtc.ID));
                }

                //找出路徑後，則將該路徑所有經過的Section存入資料庫中，並且將ACMD_OHTC的命令改為發送中
                //if (creatCmd_OHTC_DetailByBatch(acmd_ohtc.CMD_ID, minRouteSeg))
                //{
                scApp.CMDBLL.updateCommand_OHTC_StatusByCmdID(acmd_ohtc.VH_ID, acmd_ohtc.ID, E_CMD_STATUS.Sending);
                //}
                active_type = activeType;
                route_sections = minRouteSeg.ToArray();
                minRouteSec_Vh2From = minRouteSec_Vh2FromTemp;
                minRouteSec_From2To = minRouteSec_From2ToTemp;
                minRouteAdr_Vh2From = minRouteAdr_Vh2FromTemp;
                minRouteAdr_From2To = minRouteAdr_From2ToTemp;
                return true;
            }
            catch (VehicleBLL.BlockedByTheErrorVehicleException blockedException)
            {
                throw blockedException;
            }
            catch (Exception ex)
            {
                updateCommand_OHTC_StatusByCmdID(acmd_ohtc.VH_ID, acmd_ohtc.ID, E_CMD_STATUS.AbnormalEndByOHTC);
                logger_VhRouteLog.Error(ex, "generateCmd_OHTC_Details happend");
                return false;
            }
        }

        private static ActiveType convert_E_CMD_TYPE2ActiveType(ACMD acmd_ohtc)
        {
            ActiveType activeType;
            switch (acmd_ohtc.CMD_TYPE)
            {
                case E_CMD_TYPE.Move:
                case E_CMD_TYPE.Move_Park:
                    //case E_CMD_TYPE.Move_MTPort:
                    activeType = ActiveType.Move;
                    break;
                case E_CMD_TYPE.Move_MTL:
                    activeType = ActiveType.Movetomtl;
                    break;
                case E_CMD_TYPE.SystemIn:
                    activeType = ActiveType.Systemin;
                    break;
                case E_CMD_TYPE.SystemOut:
                    activeType = ActiveType.Systemout;
                    break;
                //case E_CMD_TYPE.Scan:
                //    activeType = ActiveType.Scan;
                //    break;
                case E_CMD_TYPE.Load:
                    activeType = ActiveType.Load;
                    break;
                case E_CMD_TYPE.Unload:
                    activeType = ActiveType.Unload;
                    break;
                case E_CMD_TYPE.LoadUnload:
                    activeType = ActiveType.Loadunload;
                    break;
                case E_CMD_TYPE.Teaching:
                    activeType = ActiveType.Home;
                    break;
                case E_CMD_TYPE.MTLHome:
                    activeType = ActiveType.Mtlhome;
                    break;
                //case E_CMD_TYPE.Round:
                //    activeType = ActiveType.Round;
                //    break;
                case E_CMD_TYPE.Override:
                    activeType = ActiveType.Override;
                    break;

                default:
                    throw new Exception(string.Format("OHT Command type:{0} , not in the definition"
                                                     , acmd_ohtc.CMD_TYPE.ToString()));
            }
            return activeType;
        }

        public (bool isSuccess, int total_code,
    List<string> guide_start_to_from_section_ids,
    List<string> guide_start_to_from_address_ids,
     List<string> guide_to_dest_section_ids,
     List<string> guide_to_dest_address_ids)
    FindGuideInfo(string vh_current_address, string source_adr, string dest_adr, ActiveType active_type, bool has_carray = false, List<string> byPassSectionIDs = null)
        {
            bool isSuccess = true;
            List<string> guide_start_to_from_segment_ids = null;
            List<string> guide_start_to_from_section_ids = null;
            List<string> guide_start_to_from_address_ids = null;
            List<string> guide_to_dest_segment_ids = null;
            List<string> guide_to_dest_section_ids = null;
            List<string> guide_to_dest_address_ids = null;
            int total_cost = 0;
            //1.取得行走路徑的詳細資料
            try
            {
                switch (active_type)
                {
                    case ActiveType.Loadunload:
                        if (has_carray)
                        {
                            if (!SCUtility.isMatche(vh_current_address, dest_adr))
                            {
                                (isSuccess, guide_to_dest_segment_ids, guide_to_dest_section_ids, guide_to_dest_address_ids, total_cost)
                                    = scApp.GuideBLL.getGuideInfo(vh_current_address, dest_adr, byPassSectionIDs);
                            }
                        }
                        else
                        {
                            if (!SCUtility.isMatche(vh_current_address, source_adr))
                            {
                                (isSuccess, guide_start_to_from_segment_ids, guide_start_to_from_section_ids, guide_start_to_from_address_ids, total_cost)
                                    = scApp.GuideBLL.getGuideInfo(vh_current_address, source_adr, byPassSectionIDs);
                            }
                            if (isSuccess && !SCUtility.isMatche(source_adr, dest_adr))
                            {
                                (isSuccess, guide_to_dest_segment_ids, guide_to_dest_section_ids, guide_to_dest_address_ids, total_cost)
                                    = scApp.GuideBLL.getGuideInfo(source_adr, dest_adr, null);
                            }
                        }
                        break;
                    case ActiveType.Load:
                        if (!SCUtility.isMatche(vh_current_address, source_adr))
                        {
                            (isSuccess, guide_start_to_from_segment_ids, guide_start_to_from_section_ids, guide_start_to_from_address_ids, total_cost)
                                = scApp.GuideBLL.getGuideInfo(vh_current_address, source_adr, byPassSectionIDs);
                        }
                        else
                        {
                            isSuccess = true; //如果相同 代表是在同一個點上
                        }
                        break;
                    case ActiveType.Scan:
                        if (!SCUtility.isMatche(vh_current_address, source_adr))
                        {
                            (isSuccess, guide_start_to_from_segment_ids, guide_start_to_from_section_ids, guide_start_to_from_address_ids, total_cost)
                                = scApp.GuideBLL.getGuideInfo(vh_current_address, source_adr, byPassSectionIDs);
                        }
                        else
                        {
                            isSuccess = true; //如果相同 代表是在同一個點上
                        }
                        break;
                    case ActiveType.Unload:
                        if (!SCUtility.isMatche(vh_current_address, dest_adr))
                        {
                            (isSuccess, guide_to_dest_segment_ids, guide_to_dest_section_ids, guide_to_dest_address_ids, total_cost)
                                = scApp.GuideBLL.getGuideInfo(vh_current_address, dest_adr, byPassSectionIDs);
                        }
                        else
                        {
                            isSuccess = true;//如果相同 代表是在同一個點上
                        }
                        break;
                    case ActiveType.Move:
                    case ActiveType.Systemin:
                    case ActiveType.Systemout:
                    case ActiveType.Movetomtl:
                    case ActiveType.Mtlhome:
                        if (!SCUtility.isMatche(vh_current_address, dest_adr))
                        {
                            (isSuccess, guide_to_dest_segment_ids, guide_to_dest_section_ids, guide_to_dest_address_ids, total_cost)
                                = scApp.GuideBLL.getGuideInfo(vh_current_address, dest_adr, byPassSectionIDs);
                        }
                        else
                        {
                            isSuccess = false;
                        }
                        break;
                }
                return (isSuccess, total_cost, guide_start_to_from_section_ids, guide_start_to_from_address_ids, guide_to_dest_section_ids, guide_to_dest_address_ids);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return (isSuccess, total_cost, guide_start_to_from_section_ids, guide_start_to_from_address_ids, guide_to_dest_section_ids, guide_to_dest_address_ids);
            }


        }

        public string[] findBestFitRoute(string vh_crt_sec, string[] AllRouteInfo, string targetAdr)
        {
            string[] FitRouteSec = null;
            //try
            //{
            List<string> crtByPassSeg = ByPassSegment.ToList();
            filterByPassSec_VhAlreadyOnSec(vh_crt_sec, crtByPassSeg);
            filterByPassSec_TargetAdrOnSec(targetAdr, crtByPassSeg);
            string[] AllRoute = AllRouteInfo[1].Split(';');
            List<KeyValuePair<string[], double>> routeDetailAndDistance = PaserRoute2SectionsAndDistance(AllRoute);
            //if (scApp.getEQObjCacheManager().getLine().SegmentPreDisableExcuting)
            //{
            //    List<string> nonActiveSeg = scApp.MapBLL.loadNonActiveSegmentNum();
            //filterByPassSec_VhAlreadyOnSec(vh_crt_sec, nonActiveSeg);
            //filterByPassSec_TargetAdrOnSec(targetAdr, nonActiveSeg);
            foreach (var routeDetial in routeDetailAndDistance.ToList())
            {
                List<ASECTION> lstSec = scApp.MapBLL.loadSectionBySecIDs(routeDetial.Key.ToList());
                if (scApp.getEQObjCacheManager().getLine().SegmentPreDisableExcuting)
                {
                    List<string> nonActiveSeg = scApp.MapBLL.loadNonActiveSegmentNum();
                    string[] secOfSegments = lstSec.Select(s => s.SEG_NUM).Distinct().ToArray();
                    bool isIncludePassSeg = secOfSegments.Where(seg => nonActiveSeg.Contains(seg)).Count() != 0;
                    if (isIncludePassSeg)
                    {
                        routeDetailAndDistance.Remove(routeDetial);
                    }
                }
            }
            foreach (var routeDetial in routeDetailAndDistance.ToList())
            {
                List<ASECTION> lstSec = scApp.MapBLL.loadSectionBySecIDs(routeDetial.Key.ToList());
                List<AVEHICLE> vhs = scApp.VehicleBLL.cache.loadAllErrorVehicle();
                foreach (AVEHICLE vh in vhs)
                {
                    bool IsErrorVhOnPassSection = lstSec.Where(sec => sec.SEC_ID.Trim() == vh.CUR_SEC_ID.Trim()).Count() > 0;
                    if (IsErrorVhOnPassSection)
                    {
                        routeDetailAndDistance.Remove(routeDetial);
                        if (routeDetailAndDistance.Count == 0)
                        {
                            throw new VehicleBLL.BlockedByTheErrorVehicleException
                                ($"Can't find the way to transfer.Because block by error vehicle [{vh.VEHICLE_ID}] on sec [{vh.CUR_SEC_ID}]");
                        }
                    }
                }
            }
            //}

            if (routeDetailAndDistance.Count == 0)
            {
                return null;
            }

            foreach (var routeDetial in routeDetailAndDistance)
            {
                List<ASECTION> lstSec = scApp.MapBLL.loadSectionBySecIDs(routeDetial.Key.ToList());
                string[] secOfSegments = lstSec.Select(s => s.SEG_NUM).Distinct().ToArray();
                bool isIncludePassSeg = secOfSegments.Where(seg => crtByPassSeg.Contains(seg)).Count() != 0;
                if (isIncludePassSeg)
                {
                    continue;
                }
                else
                {
                    FitRouteSec = routeDetial.Key;
                    break;
                }
            }
            if (FitRouteSec == null)
            {
                routeDetailAndDistance = routeDetailAndDistance.OrderBy(o => o.Value).ToList();
                FitRouteSec = routeDetailAndDistance.First().Key;
            }
            //}
            //catch (Exception ex)
            //{
            //    logger_VhRouteLog.Error(ex, "Exception");
            //}
            return FitRouteSec;
        }

        private void filterByPassSec_TargetAdrOnSec(string targetAdr, List<string> crtByPassSeg)
        {
            if (SCUtility.isEmpty(targetAdr)) return;
            List<ASECTION> adrOfSecs = scApp.MapBLL.loadSectionsByFromOrToAdr(targetAdr);
            string[] adrSecOfSegments = adrOfSecs.Select(s => s.SEG_NUM).Distinct().ToArray();
            if (adrSecOfSegments != null && adrSecOfSegments.Count() > 0)
            {
                foreach (string seg in adrSecOfSegments)
                {
                    if (crtByPassSeg.Contains(seg))
                    {
                        crtByPassSeg.Remove(seg);
                    }
                }
            }
        }

        private void filterByPassSec_VhAlreadyOnSec(string vh_crt_sec, List<string> crtByPassSeg)
        {
            ASECTION vh_current_sec = scApp.MapBLL.getSectiontByID(vh_crt_sec);
            if (vh_current_sec != null)
            {
                if (crtByPassSeg.Contains(vh_current_sec.SEG_NUM))
                {
                    crtByPassSeg.Remove(vh_current_sec.SEG_NUM);
                }
            }
        }

        private List<KeyValuePair<string[], double>> PaserRoute2SectionsAndDistance(string[] AllRoute)
        {
            List<KeyValuePair<string[], double>> routeDetailAndDistance = new List<KeyValuePair<string[], double>>();
            foreach (string routeDetial in AllRoute)
            {
                string route = routeDetial.Split('=')[0];
                string[] routeSection = route.Split(',');
                string distance = routeDetial.Split('=')[1];
                double idistance = double.MaxValue;
                if (!double.TryParse(distance, out idistance))
                {
                    logger.Warn($"fun:{nameof(PaserRoute2SectionsAndDistance)},parse distance fail.Route:{route},distance:{distance}");
                }
                routeDetailAndDistance.Add(new KeyValuePair<string[], double>(routeSection, idistance));
            }
            return routeDetailAndDistance;
        }


        private string[] GetGuideSections(List<RouteKit.Section> sections)
        {
            return sections.Select(sec => sec.section_id).ToArray();
        }
        private string[] GetGuideAddresses(List<RouteKit.Section> sections, string lastAddress)
        {
            List<string> addresses = new List<string>();
            //跳過第一個Section(VH自己所在的Section)不使用，
            //拿接下來要經過的Section組合出Guide Addresses。
            var filter_sections = sections.Skip(1);
            var first_section = filter_sections.First();
            string first_address = first_section.direct == 1 ?
                first_section.address_1.ToString() : first_section.address_2.ToString();
            addresses.Add(first_address);
            addresses.AddRange(filter_sections.Select(section_address_selector).ToList());
            //移掉最後一個Address加入最後要到達的Address即可
            addresses.Add(lastAddress);
            return addresses.ToArray();
        }

        string section_address_selector(RouteKit.Section sec)
        {
            if (sec.direct == 1)
            {
                return sec.address_2.ToString("0000");
            }
            else if (sec.direct == 2)
            {
                return sec.address_1.ToString("0000");
            }
            else
            {
                throw new Exception();
            }
        }


        public ActiveType convertECmdType2ActiveType(E_CMD_TYPE cmdType)
        {
            ActiveType activeType;
            switch (cmdType)
            {
                case E_CMD_TYPE.Move:
                case E_CMD_TYPE.Move_Park:
                    activeType = ActiveType.Move;
                    break;
                case E_CMD_TYPE.Load:
                    activeType = ActiveType.Load;
                    break;
                case E_CMD_TYPE.Unload:
                    activeType = ActiveType.Unload;
                    break;
                case E_CMD_TYPE.LoadUnload:
                    activeType = ActiveType.Loadunload;
                    break;
                case E_CMD_TYPE.Teaching:
                    activeType = ActiveType.Home;
                    break;
                case E_CMD_TYPE.Override:
                    activeType = ActiveType.Override;
                    break;
                case E_CMD_TYPE.Move_MTL:
                    activeType = ActiveType.Movetomtl;
                    break;
                case E_CMD_TYPE.SystemOut:
                    activeType = ActiveType.Systemout;
                    break;
                case E_CMD_TYPE.MTLHome:
                    activeType = ActiveType.Mtlhome;
                    break;
                case E_CMD_TYPE.SystemIn:
                    activeType = ActiveType.Systemin;
                    break;


                default:
                    throw new Exception(string.Format("OHT Command type:{0} , not in the definition"
                                                     , cmdType.ToString()));
            }
            return activeType;
        }



        public bool creatCmd_OHTC_Details(string cmd_id, List<string> sec_ids)
        {
            bool isSuccess = false;
            ASECTION section = null;
            try
            {
                //List<ASECTION> lstSce = scApp.MapBLL.loadSectionBySecIDs(sec_ids);
                for (int i = 0; i < sec_ids.Count; i++)
                {
                    section = scApp.MapBLL.getSectiontByID(sec_ids[i]);
                    creatCommand_OHTC_Detail(cmd_id, i + 1, section.FROM_ADR_ID, section.SEC_ID, section.SEG_NUM, 0);
                }
                isSuccess = true;
            }
            catch (Exception ex)
            {
                logger_VhRouteLog.Error(ex, "Exception");
                throw ex;
            }
            return isSuccess;
        }

        public bool creatCommand_OHTC_Detail(string cmd_id, int seq_no, string add_id,
                                      string sec_id, string seg_num, int estimated_time)
        {
            bool isSuccess = false;
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                ACMD_DETAIL cmd = new ACMD_DETAIL
                {
                    CMD_ID = cmd_id,
                    SEQ_NO = seq_no,
                    ADD_ID = add_id,
                    SEC_ID = sec_id,
                    SEG_NUM = seg_num,
                    ESTIMATED_TIME = estimated_time
                };
                cmd_ohtc_detailDAO.add(con, cmd);
            }
            return isSuccess;
        }

        public void CeratCmdDerails(string cmdID, string[] secIDs)
        {
            //using (DBConnection_EF con = new DBConnection_EF())
            int start_seq_no = 0;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                ACMD_DETAIL last_cmd_detail = cmd_ohtc_detailDAO.getLastByID(con, cmdID);
                if (last_cmd_detail != null)
                {
                    start_seq_no = last_cmd_detail.SEQ_NO;
                }
            }
            List<ACMD_DETAIL> cmd_details = new List<ACMD_DETAIL>();
            foreach (string sec_id in secIDs)
            {
                ASECTION section = scApp.MapBLL.getSectiontByID(sec_id);
                ACMD_DETAIL cmd_detail = new ACMD_DETAIL()
                {
                    CMD_ID = cmdID,
                    SEQ_NO = ++start_seq_no,
                    ADD_ID = section.FROM_ADR_ID,
                    SEC_ID = section.SEC_ID,
                    SEG_NUM = section.SEG_NUM,
                    ESTIMATED_TIME = 0
                };
                cmd_details.Add(cmd_detail);
            }
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                cmd_ohtc_detailDAO.AddByBatch(con, cmd_details);
            }
        }

        public bool update_CMD_DetailEntryTime(string cmd_id,
                                               string add_id,
                                               string sec_id)
        {
            bool isSuccess = false;
            //DBConnection_EF con = DBConnection_EF.GetContext();
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                //ACMD_OHTC cmd_oht = cmd_ohtcDAO.getExecuteByVhID(con, vh_id);
                ACMD_DETAIL cmd_detail = cmd_ohtc_detailDAO.
                    getByCmdIDSECIDAndEntryTimeEmpty(con, cmd_id, sec_id);
                if (cmd_detail != null)
                {
                    DateTime nowTime = DateTime.Now;
                    cmd_detail.ADD_ENTRY_TIME = nowTime;
                    cmd_detail.SEC_ENTRY_TIME = nowTime;
                    cmd_ohtc_detailDAO.Update(con, cmd_detail);
                    isSuccess = true;
                }
            }
            return isSuccess;
        }
        public bool update_CMD_DetailLeaveTime(string cmd_id,
                                              string add_id,
                                              string sec_id)
        {
            bool isSuccess = false;
            //DBConnection_EF con = DBConnection_EF.GetContext();
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                //ACMD_OHTC cmd_oht = cmd_ohtcDAO.getExecuteByVhID(con, vh_id);
                //if (cmd_oht != null)
                //{
                ACMD_DETAIL cmd_detail = cmd_ohtc_detailDAO.
                    getByCmdIDSECIDAndLeaveTimeEmpty(con, cmd_id, sec_id);
                if (cmd_detail == null)
                {
                    return false;
                }
                DateTime nowTime = DateTime.Now;
                cmd_detail.SEC_LEAVE_TIME = nowTime;

                cmd_ohtc_detailDAO.Update(con, cmd_detail);
                cmd_ohtc_detailDAO.UpdateIsPassFlag(con, cmd_detail.CMD_ID, cmd_detail.SEQ_NO);
                isSuccess = true;
                //}
            }
            return isSuccess;
        }

        public bool update_CMD_Detail_LoadStartTime(string vh_id,
                                                   string add_id,
                                                   string sec_id)
        {
            bool isSuccess = true;
            //DBConnection_EF con = DBConnection_EF.GetContext();
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                //ACMD_OHTC cmd_oht = cmd_ohtcDAO.getExecuteByVhID(con, vh_id);
                AVEHICLE vh = scApp.VehicleBLL.cache.getVehicle(vh_id);
                //if (!SCUtility.isEmpty(vh.OHTC_CMD)) //todo kevin 需確認是否要使用?
                //{
                //    //ACMD_OHTC_DETAL cmd_detal = cmd_ohtc_detalDAO.getByCmdIDAndAdrID(con, cmd_oht.CMD_ID, add_id);
                //    //ACMD_OHTC_DETAIL cmd_detail = cmd_ohtc_detailDAO.getByCmdIDAndSecID(con, cmd_oht.CMD_ID, sec_id);
                //    ACMD_DETAIL cmd_detail = cmd_ohtc_detailDAO.getByCmdIDAndSecID(con, vh.OHTC_CMD, sec_id);
                //    if (cmd_detail == null)
                //        return false;
                //    DateTime nowTime = DateTime.Now;
                //    cmd_detail.LOAD_START_TIME = nowTime;
                //    cmd_ohtc_detailDAO.Update(con, cmd_detail);
                //}
                //else
                //{
                //    isSuccess = false;
                //}
            }
            return isSuccess;
        }
        public bool update_CMD_Detail_LoadEndTime(string vh_id,
                                         string add_id,
                                         string sec_id)
        {
            bool isSuccess = true;
            //DBConnection_EF con = DBConnection_EF.GetContext();
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                //ACMD_OHTC cmd_oht = cmd_ohtcDAO.getExecuteByVhID(con, vh_id);
                AVEHICLE vh = scApp.VehicleBLL.cache.getVehicle(vh_id);
                //if (!SCUtility.isEmpty(vh.OHTC_CMD)) //todo kevin 需確認是否要使用?
                //{
                //    //ACMD_OHTC_DETAL cmd_detal = cmd_ohtc_detalDAO.getByCmdIDAndAdrID(con, cmd_oht.CMD_ID, add_id);
                //    //ACMD_OHTC_DETAIL cmd_detail = cmd_ohtc_detailDAO.getByCmdIDAndSecID(con, cmd_oht.CMD_ID, sec_id);
                //    ACMD_DETAIL cmd_detail = cmd_ohtc_detailDAO.getByCmdIDAndSecID(con, vh.OHTC_CMD, sec_id);
                //    if (cmd_detail == null)
                //        return false;
                //    DateTime nowTime = DateTime.Now;
                //    cmd_detail.LOAD_END_TIME = nowTime;
                //    cmd_ohtc_detailDAO.Update(con, cmd_detail);
                //    //}
                //}
                //else
                //{
                //    isSuccess = false;
                //}
            }
            return isSuccess;
        }
        public bool update_CMD_Detail_UnloadStartTime(string vh_id,
                                       string add_id,
                                       string sec_id)
        {
            bool isSuccess = true;
            //DBConnection_EF con = DBConnection_EF.GetContext();
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                //ACMD_OHTC cmd_oht = cmd_ohtcDAO.getExecuteByVhID(con, vh_id);
                AVEHICLE vh = scApp.VehicleBLL.cache.getVehicle(vh_id);
                //if (!SCUtility.isEmpty(vh.OHTC_CMD)) //todo kevin 需確認是否要使用?
                //{
                //    ACMD_DETAIL cmd_detail = cmd_ohtc_detailDAO.getByCmdIDAndSecID(con, vh.OHTC_CMD, sec_id);
                //    if (cmd_detail == null)
                //        return false;
                //    DateTime nowTime = DateTime.Now;
                //    cmd_detail.UNLOAD_START_TIME = nowTime;
                //    cmd_ohtc_detailDAO.Update(con, cmd_detail);
                //}
                //else
                //{
                //    isSuccess = false;
                //}
            }
            return isSuccess;
        }


        public bool update_CMD_Detail_UnloadEndTime(string vh_id)
        {
            bool isSuccess = true;
            //DBConnection_EF con = DBConnection_EF.GetContext();
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                //ACMD_OHTC cmd_oht = cmd_ohtcDAO.getExecuteByVhID(con, vh_id);
                AVEHICLE vh = scApp.VehicleBLL.cache.getVehicle(vh_id);
                //if (!SCUtility.isEmpty(vh.OHTC_CMD)) //todo kevin 需確認是否要使用?
                //{
                //    ACMD_DETAIL cmd_detail = cmd_ohtc_detailDAO.getLastByID(con, vh.OHTC_CMD);
                //    if (cmd_detail != null)
                //    {
                //        cmd_detail.UNLOAD_END_TIME = DateTime.Now;
                //        cmd_ohtc_detailDAO.Update(con, cmd_detail);
                //    }
                //    else
                //    {
                //        isSuccess = false;
                //    }
                //}
                //else
                //{
                //    isSuccess = false;
                //}
            }
            return isSuccess;
        }

        //public bool update_CMD_Detail_2AbnormalFinsh(string cmd_id, List<string> sec_ids)
        //{
        //    bool isSuccess = false;
        //    using (DBConnection_EF con = DBConnection_EF.GetUContext())
        //    {
        //        foreach (string sec_id in sec_ids)
        //        {
        //            ACMD_OHTC_DETAIL cmd_detail = new ACMD_OHTC_DETAIL();
        //            cmd_detail.CMD_ID = cmd_id;
        //            con.ACMD_OHTC_DETAIL.Attach(cmd_detail);
        //            cmd_detail.SEC_ID = sec_id;
        //            cmd_detail.SEC_ENTRY_TIME = DateTime.MaxValue;
        //            cmd_detail.SEC_LEAVE_TIME = DateTime.MaxValue;
        //            cmd_detail.ADD_ID = "";
        //            cmd_detail.SEG_NUM = "";

        //            //con.Entry(cmd_detail).Property(p => p.CMD_ID).IsModified = true;
        //            //con.Entry(cmd_detail).Property(p => p.SEC_ID).IsModified = true;
        //            con.Entry(cmd_detail).Property(p => p.SEC_ENTRY_TIME).IsModified = true;
        //            con.Entry(cmd_detail).Property(p => p.SEC_LEAVE_TIME).IsModified = true;
        //            con.Entry(cmd_detail).Property(p => p.ADD_ID).IsModified = false;
        //            con.Entry(cmd_detail).Property(p => p.SEG_NUM).IsModified = false;
        //            cmd_ohtc_detailDAO.Update(con, cmd_detail);
        //            con.Entry(cmd_detail).State = EntityState.Detached;
        //        }
        //        isSuccess = true;
        //    }
        //    return isSuccess;
        //}
        public bool update_CMD_Detail_2AbnormalFinsh(string cmd_id, List<string> sec_ids)
        {
            bool isSuccess = false;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                foreach (string sec_id in sec_ids)
                {
                    ACMD_DETAIL cmd_detail = cmd_ohtc_detailDAO.getByCmdIDSECIDAndEntryTimeEmpty(con, cmd_id, sec_id);
                    if (cmd_detail != null)
                    {
                        cmd_detail.SEC_ENTRY_TIME = DateTime.MaxValue;
                        cmd_detail.SEC_LEAVE_TIME = DateTime.MaxValue;
                        cmd_detail.IS_PASS = true;

                        cmd_ohtc_detailDAO.Update(con, cmd_detail);
                    }
                }
                isSuccess = true;
            }
            return isSuccess;
        }
        public int getAndUpdateVhCMDProgress(string vh_id, out List<string> willPassSecID)
        {
            int procProgress_percen = 0;
            willPassSecID = null;
            //DBConnection_EF con = DBConnection_EF.GetContext();
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                AVEHICLE vh = scApp.VehicleBLL.cache.getVehicle(vh_id);
                //if (!SCUtility.isEmpty(vh.OHTC_CMD)) //todo kevin 需確認是否要使用?
                //{
                //    ACMD cmd_oht = cmd_ohtcDAO.getByID(con, vh.OHTC_CMD);
                //    if (cmd_oht == null) return 0;
                //    double totalDetailCount = 0;
                //    double procDetailCount = 0;
                //    //List<ACMD_OHTC_DETAIL> lstcmd_detail = cmd_ohtc_detailDAO.loadAllByCmdID(con, cmd_oht.CMD_ID);
                //    //totalDetalCount = lstcmd_detail.Count();
                //    //procDetalCount = lstcmd_detail.Where(cmd => cmd.ADD_ENTRY_TIME != null).Count();
                //    totalDetailCount = cmd_ohtc_detailDAO.getAllDetailCountByCmdID(con, cmd_oht.CMD_ID);
                //    procDetailCount = cmd_ohtc_detailDAO.getAllPassDetailCountByCmdID(con, cmd_oht.CMD_ID);
                //    willPassSecID = cmd_ohtc_detailDAO.loadAllNonPassDetailSecIDByCmdID(con, cmd_oht.CMD_ID);
                //    procProgress_percen = (int)((procDetailCount / totalDetailCount) * 100);
                //    cmd_oht.CMD_PROGRESS = procProgress_percen;
                //    cmd_ohtcDAO.Update(con, cmd_oht);
                //}
            }
            return procProgress_percen;
        }

        public bool HasCmdWillPassSegment(string segment_num, out List<string> will_pass_cmd_id)
        {
            bool hasCmd = false;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                will_pass_cmd_id = cmd_ohtc_detailDAO.loadAllWillPassDetailCmdID(con, segment_num);
            }
            hasCmd = will_pass_cmd_id != null && will_pass_cmd_id.Count > 0;
            return hasCmd;
        }

        public string[] loadPassSectionByCMDID(string cmd_id)
        {
            string[] sections = null;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                sections = cmd_ohtc_detailDAO.loadAllSecIDByCmdID(con, cmd_id);
            }
            return sections;
        }

        #endregion CMD_OHTC_DETAIL

        #region Return Code Map
        public ReturnCodeMap getReturnCodeMap(string eq_id, string return_code)
        {
            return return_code_mapDao.getReturnCodeMap(scApp, eq_id, return_code);
        }
        #endregion Return Code Map


        #region HCMD_MCS
        public void CreatHCMD_MCSs(List<HTRANSFER> HCMD_MCS)
        {

            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                hTransferDao.AddByBatch(con, HCMD_MCS);
            }
        }
        public List<ObjectRelay.HCMD_MCSObjToShow> loadHCMD_MCSs()
        {
            List<ObjectRelay.HCMD_MCSObjToShow> hcmds = null;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                hcmds = hTransferDao.loadLast24Hours(con);
            }
            return hcmds;
        }
        #endregion HCMD_MCS

        #region HCMD
        public void CreatHCMD(List<HCMD> HCMD)
        {

            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                hcmdDao.AddByBatch(con, HCMD);
            }
        }
        #endregion HCMD

        public class Cache
        {
            SCApplication app = null;
            //public Cache(ALINE _line)
            public Cache(SCApplication _app)
            {
                app = _app;
            }
            public bool hasCmdExcute(string vhID)
            {
                try
                {
                    ALINE line = app.getEQObjCacheManager().getLine();
                    var current_unfinish_cmd = line.CurrentExcuteCommand;
                    if (current_unfinish_cmd == null || current_unfinish_cmd.Count == 0)
                        return false;
                    var has_unfinish_cmd_by_vh = current_unfinish_cmd.Where(cmd => SCUtility.isMatche(cmd.VH_ID, vhID))
                                                                         .Count() > 0;
                    return has_unfinish_cmd_by_vh;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                    return false;
                }
            }

            public ACMD getExcuteCmd(string cmdID)
            {
                try
                {
                    ALINE line = app.getEQObjCacheManager().getLine();
                    var current_unfinish_cmd = line.CurrentExcuteCommand;
                    if (current_unfinish_cmd == null || current_unfinish_cmd.Count == 0)
                        return null;
                    var has_unfinish_cmd_by_vh = current_unfinish_cmd.Where(cmd => SCUtility.isMatche(cmd.ID, cmdID)
                                                                                && cmd.CMD_STATUS < E_CMD_STATUS.Aborting).
                                                                                FirstOrDefault();
                    return has_unfinish_cmd_by_vh;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                    return null;
                }
            }
            public bool hasMoveWillGoAdr(string adrID)
            {
                try
                {
                    ALINE line = app.getEQObjCacheManager().getLine();
                    var current_unfinish_cmd = line.CurrentExcuteCommand;
                    if (current_unfinish_cmd == null || current_unfinish_cmd.Count == 0)
                        return false;
                    int will_to_go_cmd_count = current_unfinish_cmd.Where(cmd => SCUtility.isMatche(cmd.DESTINATION, adrID)
                                                                                && cmd.CMD_TYPE == E_CMD_TYPE.Move
                                                                                && cmd.CMD_STATUS < E_CMD_STATUS.Aborting).
                                                                                Count();
                    return will_to_go_cmd_count > 0;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                    return false;
                }
            }

        }

        public enum CommandTranDir
        {
            None,
            InAGVStation,
            OutAGVStation
        }
    }
    public interface ICommandChecker
    {
        (bool canAssign, string result) CanAssignCommand(AVEHICLE vh, E_CMD_TYPE cmdType, CMDBLL.CommandTranDir transferDir);
        bool canSendCmdNew(AVEHICLE vh);
    }

    public class CommandCheckerNormal : ICommandChecker
    {
        protected static Logger logger = LogManager.GetCurrentClassLogger();
        SCApplication scApp = null;
        public CommandCheckerNormal(SCApplication _scApp)
        {
            scApp = _scApp;
        }
        public bool canSendCmdNew(AVEHICLE vh)
        {
            int count = 0;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                count = scApp.CMDCDao.getVhExcuteCMDConut(con, vh.VEHICLE_ID);
            }
            //int current_can_assign_command_count = scApp.MaxCommandCount;
            //if (SystemParameter.IsByPassAGVShelfStatus)
            //{
            //    current_can_assign_command_count = scApp.MaxCommandCount;
            //}
            //else
            //{
            //    current_can_assign_command_count = vh.CurrentAvailableShelf;
            //}
            //return count < current_can_assign_command_count;
            return count == 0;

        }
        public (bool canAssign, string result) CanAssignCommand(AVEHICLE vh, E_CMD_TYPE cmdType, CMDBLL.CommandTranDir transferDir)
        {
            try
            {
                string vh_id = vh.VEHICLE_ID;
                List<ACMD> assign_cmds = null;
                //1.如果是Move則需要在沒有執行任何命令下才可以指派
                //2.如果是Trasfer命令，則需要在
                //  a.沒有Move命令下才可以指派
                //  b.已經指派的命令要小於2
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    assign_cmds = scApp.CMDCDao.loadAssignCmd(con, vh_id);
                }
                int current_vh_carrier_count = 0;
                //if (vh.HAS_CST)
                //{
                //    bool cst_l_is_in_commanding = assign_cmds.Where(cmd => SCUtility.isMatche(cmd.CARRIER_ID, vh.CST_ID)).Count() > 0;
                //    //如果身上的CST 不再執行的命令中，則需要用CST 來佔一個Command的位置
                //    if (!cst_l_is_in_commanding)
                //    {
                //        current_vh_carrier_count++;
                //    }
                //}
                //if (vh.HAS_CST_R)
                //{
                //    bool cst_r_is_in_commanding = assign_cmds.Where(cmd => SCUtility.isMatche(cmd.CARRIER_ID, vh.CST_ID_R)).Count() > 0;
                //    //如果身上的CST 不再執行的命令中，則需要用CST 來佔一個Command的位置
                //    if (!cst_r_is_in_commanding)
                //    {
                //        current_vh_carrier_count++;
                //    }
                //}
                ////int current_can_assign_command_count = CurrentCanAssignMAXCount - current_vh_carrier_count;
                //int current_can_assign_command_count = scApp.MaxCommandCount - current_vh_carrier_count;
                //if (SystemParameter.IsByPassAGVShelfStatus)
                //{
                //    current_can_assign_command_count = scApp.MaxCommandCount - current_vh_carrier_count;
                //}
                //else
                //{
                //    //current_can_assign_command_count = CurrentCanAssignMAXCount - current_vh_carrier_count;
                //    current_can_assign_command_count = vh.CurrentAvailableShelf - current_vh_carrier_count;
                //}

                //if (assign_cmds.Count == 0)
                //    return (true, "");
                //else
                //{
                switch (cmdType)
                {
                    case E_CMD_TYPE.Move:
                    case E_CMD_TYPE.Move_Charger:
                        if (assign_cmds.Count == 0)
                        {
                            return (true, "");
                        }
                        else
                        {
                            return (false, "has command excute, can't assign move/move to change commmand");
                        }
                    case E_CMD_TYPE.Unload:
                        if (assign_cmds.Count == 0 && vh.HAS_CST)
                        {
                            return (true, "");
                        }
                        else
                        {
                            return (false, "has command excute or has no cst on vehicle, can't assign unload transfer commmand");
                        }
                    case E_CMD_TYPE.Load:
                    case E_CMD_TYPE.LoadUnload:
                        if (assign_cmds.Count == 0 && !vh.HAS_CST)
                        {
                            return (true, "");
                        }
                        else
                        {
                            return (false, "has command excute or has cst on vehicle, can't assign unload transfer commmand");
                        }
                    //return (true, "");//Unload一律都回覆OK，不然遇到身上已經載2個CST的，會無法下命令
                    default:
                        if (assign_cmds.Count == 0)
                        {
                            //if (current_can_assign_command_count == 0)
                            //{
                            //    return (false, $"currrent can assign cmd count:{current_can_assign_command_count}");
                            //}
                            //else
                            //{
                            return (true, "");
                            //}
                        }
                        else
                        {
                            return (false, $"vh:{vh_id}  has [{assign_cmds.Count}] commands excute and current carrier [{current_vh_carrier_count}] cst," +
                                           $" can't assign transfer commmand");

                        }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return (false, ex.ToString());
            }
        }
    }


    public class CommandCheckerSwap : ICommandChecker
    {
        protected static Logger logger = LogManager.GetCurrentClassLogger();
        SCApplication scApp = null;
        public CommandCheckerSwap(SCApplication _scApp)
        {
            scApp = _scApp;
        }
        public bool canSendCmdNew(AVEHICLE vh)
        {
            int count = 0;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                count = scApp.CMDCDao.getVhExcuteCMDConut(con, vh.VEHICLE_ID);
            }
            return count == 0;
        }
        public (bool canAssign, string result) CanAssignCommand(AVEHICLE vh, E_CMD_TYPE cmdType, CMDBLL.CommandTranDir transferDir)
        {
            try
            {
                string vh_id = vh.VEHICLE_ID;
                List<ACMD> assign_cmds = null;
                //1.如果是Move則需要在沒有執行任何命令下才可以指派
                //2.如果是Trasfer命令，則需要在
                //  a.沒有Move命令下才可以指派
                //  b.AGV車上需要有足夠的周轉空間
                //    b-1.周轉空間=0，無法指派新命令
                //    b-2.周轉空間=1，僅能指派一筆load from st & 一筆unload to st
                //    b-3.周轉空間=2，可以指派最多兩筆Load from & 兩筆unload to station
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    assign_cmds = scApp.CMDCDao.loadAssignCmd(con, vh_id);
                }

                //if (assign_cmds.Count == 0)
                //    return (true, "");
                //else
                //{
                switch (cmdType)
                {
                    case E_CMD_TYPE.Move:
                    case E_CMD_TYPE.Move_Charger:
                        if (assign_cmds.Count == 0)
                        {
                            return (true, "");
                        }
                        else
                        {
                            return (false, "has command excute, can't assign move/move to change commmand");
                        }
                    case E_CMD_TYPE.Unload:
                        if (assign_cmds.Count == 0 && vh.HAS_CST)
                        {
                            return (true, "");
                        }
                        else
                        {
                            return (false, "has command excute or has no cst on vehicle, can't assign unload transfer commmand");
                        }
                    case E_CMD_TYPE.Load:
                    case E_CMD_TYPE.LoadUnload:
                        if (assign_cmds.Count == 0 && !vh.HAS_CST)
                        {
                            return (true, "");
                        }
                        else
                        {
                            return (false, "has command excute or has cst on vehicle, can't assign unload transfer commmand");
                        }
                    //return (true, "");//Unload一律都回覆OK，不然遇到身上已經載2個CST的，會無法下命令
                    default:
                        if (assign_cmds.Count == 0)
                        {
                            //if (current_can_assign_command_count == 0)
                            //{
                            //    return (false, $"currrent can assign cmd count:{current_can_assign_command_count}");
                            //}
                            //else
                            //{
                            return (true, "");
                            //}
                        }
                        else
                        {
                            return (false, $"vh:{vh_id}  has [{assign_cmds.Count}] commands excute," +
                                           $" can't assign transfer commmand");

                        }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return (false, ex.ToString());
            }
        }
    }

}
