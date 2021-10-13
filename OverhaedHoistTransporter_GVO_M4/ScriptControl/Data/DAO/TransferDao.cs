using com.mirle.ibg3k0.sc.Data.SECS.OHTC.AT_S;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.DAO
{
    public class TransferDao
    {
        public void add(DBConnection_EF con, ATRANSFER rail)
        {
            con.ATRANSFER.Add(rail);
            con.SaveChanges();
        }
        public void RemoteByBatch(DBConnection_EF con, List<ATRANSFER> cmd_mcss)
        {
            cmd_mcss.ForEach(entity => con.Entry(entity).State = EntityState.Deleted);
            con.ATRANSFER.RemoveRange(cmd_mcss);
            con.SaveChanges();
        }


        public void update(DBConnection_EF con, ATRANSFER cmd)
        {
            con.SaveChanges();
        }

        public ATRANSFER getByID(DBConnection_EF con, String cmd_id)
        {
            var query = from cmd in con.ATRANSFER
                        where cmd.ID.Trim() == cmd_id.Trim()
                        select cmd;
            return query.SingleOrDefault();
        }


        public ATRANSFER getExcuteCMDByCSTID(DBConnection_EF con, String carrierID)
        {
            var query = from cmd in con.ATRANSFER
                        where (cmd.TRANSFERSTATE >= E_TRAN_STATUS.Queue && cmd.TRANSFERSTATE <= E_TRAN_STATUS.Aborting) &&
                               cmd.CARRIER_ID.Trim() == carrierID.Trim()
                        select cmd;
            return query.FirstOrDefault();
        }

        public IQueryable getQueryAllSQL(DBConnection_EF con)
        {
            var query = from cmd_mcs in con.ATRANSFER
                        select cmd_mcs;
            return query;
        }
        public void DeleteCmdData(DBConnection_EF conn, ATRANSFER cmddata)
        {
            try
            {
                conn.ATRANSFER.Remove(cmddata);
                conn.SaveChanges();
            }
            catch (Exception ex)
            {
                //logger.Warn(ex);
                throw;
            }
        }
        /// <summary>
        /// 透過hostSource與hostDestination取得尚未搬走(狀態還在Transferring之前)的CMD_MCS
        /// </summary>
        /// <param name="con"></param>
        /// <param name="hostSource"></param>
        /// <param name="hostDestination"></param>
        /// <returns></returns>
        public ATRANSFER getWatingCMDByFromTo(DBConnection_EF con, string hostSource, string hostDestination)
        {
            var query = from cmd in con.ATRANSFER
                        where (cmd.TRANSFERSTATE >= E_TRAN_STATUS.Queue && cmd.TRANSFERSTATE < E_TRAN_STATUS.Transferring) &&
                               cmd.HOSTSOURCE.Trim() == hostSource.Trim() &&
                               cmd.HOSTDESTINATION.Trim() == hostDestination.Trim()
                        select cmd;
            return query.FirstOrDefault();
        }
        public ATRANSFER getWatingCMDByFrom(DBConnection_EF con, string hostSource)
        {
            var query = from cmd in con.ATRANSFER
                        where (cmd.TRANSFERSTATE >= E_TRAN_STATUS.Queue && cmd.TRANSFERSTATE < E_TRAN_STATUS.Transferring) &&
                               cmd.HOSTSOURCE.Trim() == hostSource.Trim()
                        select cmd;
            return query.FirstOrDefault();
        }

        public ATRANSFER getTransferByExecutedCMDID(DBConnection_EF con, string cmd_id)
        {
            var query = from cmd in con.ATRANSFER
                        where (cmd.TRANSFERSTATE >= E_TRAN_STATUS.Queue && cmd.TRANSFERSTATE <= E_TRAN_STATUS.Transferring) &&
                               cmd.EXCUTE_CMD_ID.Trim() == cmd_id.Trim()
                        select cmd;
            return query.FirstOrDefault();
        }

        public List<ATRANSFER> loadACMD_MCSIsQueue(DBConnection_EF con)
        {
            var query = from cmd in con.ATRANSFER.AsNoTracking()
                        where (cmd.TRANSFERSTATE == E_TRAN_STATUS.Queue || cmd.TRANSFERSTATE == E_TRAN_STATUS.RouteChanging)
                        && (cmd.CHECKCODE.Trim() == SECSConst.HCACK_Confirm || cmd.CHECKCODE.Trim() == SECSConst.HCACK_Confirm_Executed)
                        orderby cmd.PRIORITY_SUM descending, cmd.CMD_INSER_TIME
                        select cmd;
            return query.ToList();
        }

        public List<ATRANSFER> loadACMD_MCSIsUnfinished(DBConnection_EF con)
        {
            var query = from cmd in con.ATRANSFER.AsNoTracking()
                        where cmd.TRANSFERSTATE >= E_TRAN_STATUS.Queue && cmd.TRANSFERSTATE <= E_TRAN_STATUS.Aborting
                        && cmd.CHECKCODE.Trim() == SECSConst.HCACK_Confirm
                        select cmd;

            return query.ToList();
        }
        public List<ATRANSFER> LoadCmdData_WithoutComplete(DBConnection_EF conn)
        {
            try
            {
                var query = from a in conn.ATRANSFER
                           where a.TRANSFERSTATE < E_TRAN_STATUS.Complete
                           select a;
                return query.ToList();
            }
            catch (Exception ex)
            {
                //logger.Warn(ex);
                throw;
            }
        }
        public List<ATRANSFER> loadACMD_MCSIsExecuting(DBConnection_EF con)
        {
            var query = from cmd in con.ATRANSFER.AsNoTracking()
                        where cmd.TRANSFERSTATE > E_TRAN_STATUS.Queue && cmd.TRANSFERSTATE <= E_TRAN_STATUS.Transferring
                        && cmd.CHECKCODE.Trim() == SECSConst.HCACK_Confirm
                        select cmd;
            return query.ToList();
        }

        public List<ATRANSFER> loadFinishCMD_MCS(DBConnection_EF con)
        {
            var query = from cmd in con.ATRANSFER
                        where cmd.TRANSFERSTATE >= E_TRAN_STATUS.Canceled
                        select cmd;
            return query.ToList();
        }



        public int getCMD_MCSMaxPrioritySum(DBConnection_EF con)
        {
            var query = from cmd in con.ATRANSFER
                        where cmd.TRANSFERSTATE == E_TRAN_STATUS.Queue
                        orderby cmd.PRIORITY_SUM descending
                        select cmd.PRIORITY_SUM;
            List<int> prorityList = query.ToList();
            if (prorityList.Count == 0)
            {
                return 0;
            }
            else
            {
                return prorityList[0];
            }
        }
        public int getCMD_MCSMinPrioritySum(DBConnection_EF con)
        {
            var query = from cmd in con.ATRANSFER
                        where cmd.TRANSFERSTATE == E_TRAN_STATUS.Queue
                        orderby cmd.PRIORITY_SUM ascending
                        select cmd.PRIORITY_SUM;
            List<int> prorityList = query.ToList();
            if (prorityList.Count == 0)
            {
                return 0;
            }
            else
            {
                return prorityList[0];
            }
        }

        public int getCMD_MCSIsQueueCount(DBConnection_EF con)
        {
            var query = from cmd in con.ATRANSFER
                        where cmd.TRANSFERSTATE == E_TRAN_STATUS.Queue
                        select cmd;
            return query.Count();
        }
        public int getCMD_MCSTotalCount(DBConnection_EF con)
        {
            var query = from cmd in con.ATRANSFER
                        select cmd;
            return query.Count();
        }

        public int getCMD_MCSIsExcuteCount(DBConnection_EF con)
        {
            var query = from cmd in con.ATRANSFER
                        where cmd.TRANSFERSTATE > E_TRAN_STATUS.Queue
                        && cmd.TRANSFERSTATE < E_TRAN_STATUS.Canceled
                        select cmd;
            return query.Count();
        }
        public int getCMD_MCSIsExcuteCount(DBConnection_EF con, DateTime defore_time)
        {
            var query = from cmd in con.ATRANSFER
                        where cmd.TRANSFERSTATE > E_TRAN_STATUS.Queue
                        && cmd.TRANSFERSTATE < E_TRAN_STATUS.Canceled
                        && cmd.CMD_INSER_TIME < defore_time
                        select cmd;
            return query.Count();
        }
        public List<string> loadIsExcuteCMD_MCS_ID(DBConnection_EF con, DateTime defore_time)
        {
            var query = from cmd in con.ATRANSFER
                        where cmd.TRANSFERSTATE > E_TRAN_STATUS.Queue
                        && cmd.TRANSFERSTATE < E_TRAN_STATUS.Canceled
                        && cmd.CMD_INSER_TIME < defore_time
                        select cmd.ID;
            return query.ToList();
        }
        public int getCMD_MCSIsUnfinishedCountByHostSource(DBConnection_EF con, List<string> port_ids)
        {
            var query = from cmd in con.ATRANSFER
                        where port_ids.Contains(cmd.HOSTSOURCE.Trim()) &&
                        cmd.TRANSFERSTATE >= E_TRAN_STATUS.Queue
                        && cmd.TRANSFERSTATE < E_TRAN_STATUS.Canceled
                        select cmd;
            return query.Count();
        }

        public int getCMD_MCSIsUnfinishedCount(DBConnection_EF con)
        {
            var query = from cmd in con.ATRANSFER where
                        cmd.TRANSFERSTATE >= E_TRAN_STATUS.Queue
                        && cmd.TRANSFERSTATE < E_TRAN_STATUS.Canceled
                        select cmd;
            return query.Count();
        }

        public int getCMD_MCSIsUnfinishedCountByHostDsetination(DBConnection_EF con, string port_id)
        {
            var query = from cmd in con.ATRANSFER
                        where cmd.HOSTDESTINATION.Trim() == port_id.Trim() &&
                        cmd.TRANSFERSTATE > E_TRAN_STATUS.Queue
                        && cmd.TRANSFERSTATE < E_TRAN_STATUS.Canceled
                        select cmd;
            return query.Count();
        }

        public List<ATRANSFER> getCMD_MCSByConditionss(DBConnection_EF conn, DateTime startDatetime, DateTime endDatetime,
                                       string CSTID = null, string CmdID = null)
        {
            var query = conn.ATRANSFER.Where(x => x.CMD_START_TIME > startDatetime
                && x.CMD_START_TIME < endDatetime
                && x.TRANSFERSTATE == E_TRAN_STATUS.Complete);

            if (!string.IsNullOrEmpty(CSTID))
            {
                query = query.Where(x => x.CARRIER_ID == CSTID);
            }
            if (!string.IsNullOrEmpty(CmdID))
            {
                query = query.Where(x => x.ID == CmdID);
            }
            return query.ToList();
        }

        public int getCMD_MCSInserCountLastHour(DBConnection_EF con, int hours)
        {
            DateTime nowTime = DateTime.Now;
            DateTime lastTime = nowTime.AddHours(-hours);

            var query = from cmd in con.ATRANSFER
                        where cmd.CMD_INSER_TIME < nowTime &&
                        cmd.CMD_INSER_TIME > lastTime
                        select cmd;
            return query.Count();
        }

        public int getCMD_MCSFinishCountLastHours(DBConnection_EF con, int hours)
        {
            DateTime nowTime = DateTime.Now;
            DateTime lastTime = nowTime.AddHours(-hours);
            var query = from cmd in con.ATRANSFER
                        where cmd.CMD_FINISH_TIME < nowTime &&
                        cmd.CMD_FINISH_TIME > lastTime
                        select cmd;
            return query.Count();
        }

        public int getCMD_MCSIsUnfinishedCountByCarrierID(DBConnection_EF con, string carrier_id)
        {
            var query = from cmd in con.ATRANSFER
                        where cmd.CARRIER_ID.Trim() == carrier_id.Trim() &&
                        cmd.TRANSFERSTATE >= E_TRAN_STATUS.Queue
                        && cmd.TRANSFERSTATE < E_TRAN_STATUS.Canceled
                        select cmd;
            return query.Count();
        }
        public List<ATRANSFER> loadByInsertTimeEndTime(DBConnection_EF con, DateTime insertTime, DateTime finishTime)
        {
            var query = from cmd in con.ATRANSFER
                        where cmd.CMD_START_TIME > insertTime && (cmd.CMD_FINISH_TIME != null && cmd.CMD_FINISH_TIME < finishTime)
                        orderby cmd.CMD_START_TIME descending
                        select cmd;
            return query.ToList();
        }

        public void DeleteLOG_ByACMD_MCS(DBConnection_EF conn, int deleteMonths)
        {
            try
            {
                DateTime dateTime = DateTime.Now.AddMonths(-deleteMonths);
                var deleteData = conn.ATRANSFER.Where(x => x.CMD_INSER_TIME < dateTime);
                conn.ATRANSFER.RemoveRange(deleteData);
                conn.SaveChanges();
            }
            catch (Exception ex)
            {
                //logger.Warn(ex);
                throw;
            }
        }
        public void DeleteLOG_ByACMD_OHTC(DBConnection_EF conn, int deleteMonths)
        {
            try
            {
                DateTime dateTime = DateTime.Now.AddMonths(-deleteMonths);
                var deleteData = conn.ATRANSFER.Where(x => x.CMD_INSER_TIME < dateTime);
                conn.ATRANSFER.RemoveRange(deleteData);
                conn.SaveChanges();
            }
            catch (Exception ex)
            {
                //logger.Warn(ex);
                throw;
            }
        }


    }

}
