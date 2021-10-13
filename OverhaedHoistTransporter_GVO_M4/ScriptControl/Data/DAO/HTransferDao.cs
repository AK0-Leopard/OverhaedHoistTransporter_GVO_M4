using com.mirle.ibg3k0.sc.Data.SECS;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.DAO
{
    public class HTransferDao
    {

        public void Add(DBConnection_EF con, HTRANSFER cmd_mcs)
        {
            con.HTRANSFER.Add(cmd_mcs);
            con.SaveChanges();
        }
        public void AddByBatch(DBConnection_EF con, List<HTRANSFER> cmd_ohtcs)
        {
            con.HTRANSFER.AddRange(cmd_ohtcs);
            con.SaveChanges();
        }

        public List<ObjectRelay.HCMD_MCSObjToShow> loadLast24Hours(DBConnection_EF con)
        {
            DateTime query_time = DateTime.Now.AddHours(-24);
            var query = from cmd in con.HTRANSFER
                        where cmd.CMD_INSER_TIME > query_time
                        orderby cmd.CMD_INSER_TIME descending
                        select new ObjectRelay.HCMD_MCSObjToShow() { cmd_mcs = cmd };
            return query.ToList();
        }
        public List<HTRANSFER> loadByInsertPeriod(DBConnection_EF con, DateTime dtStart, DateTime dtEnd)
        {
            var query = from cmd in con.HTRANSFER
                        where (cmd.CMD_INSER_TIME > dtStart && cmd.CMD_INSER_TIME < dtEnd)
                           || (cmd.CMD_INSER_TIME > dtEnd && cmd.CMD_INSER_TIME < dtStart)
                        orderby cmd.CMD_INSER_TIME descending
                        select cmd;
            return query.ToList();
        }
        public List<HTRANSFER> loadByInsertTimeEndTime(DBConnection_EF con, DateTime insertTime, DateTime finishTime)
        {
            var query = from cmd in con.HTRANSFER
                        where cmd.CMD_START_TIME > insertTime && (cmd.CMD_FINISH_TIME != null && cmd.CMD_FINISH_TIME < finishTime)
                        orderby cmd.CMD_START_TIME descending
                        select cmd;
            return query.ToList();
        }
        public int GetTodayCount(DBConnection_EF con)
        {
            var query = from cmd in con.HTRANSFER
                        where cmd.CMD_FINISH_TIME != null
                        && cmd.CMD_FINISH_TIME >= DateTime.Today && cmd.CMD_FINISH_TIME < DateTime.Now
                        select cmd;
            return query?.Count() ?? 0;
        }
        public int GetHourlyCount(DBConnection_EF con)
        {
            DateTime dtS = DateTime.Now.AddHours(-1);
            var query = from cmd in con.HTRANSFER
                        where cmd.CMD_FINISH_TIME != null
                        && cmd.CMD_FINISH_TIME > dtS && cmd.CMD_FINISH_TIME <= DateTime.Now
                        select cmd;
            return query?.Count() ?? 0;
        }
    }

}
