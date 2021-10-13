using com.mirle.ibg3k0.sc.Data.SECS;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.DAO
{
    public class HCMDDao
    {
        public void AddByBatch(DBConnection_EF con, List<HCMD> hcmds)
        {
            con.HCMD.AddRange(hcmds);
            con.SaveChanges();
        }

        public IQueryable getQueryAllSQL(DBConnection_EF con)
        {
            var query = from cmd in con.HCMD
                        select cmd;
            return query;
        }

        public List<HCMD> loadByInsertPeriod(DBConnection_EF con, DateTime dtStart, DateTime dtEnd)
        {
            var query = from cmd in con.HCMD
                        where (cmd.CMD_INSER_TIME > dtStart && cmd.CMD_INSER_TIME < dtEnd)
                           || (cmd.CMD_INSER_TIME > dtEnd && cmd.CMD_INSER_TIME < dtStart)
                        orderby cmd.CMD_INSER_TIME descending
                        select cmd;
            return query.ToList();
        }

        public IQueryable getTopNQueryCarInOutSQL(DBConnection_EF con, int num)
        {
            var query = (from cmd in con.HCMD
                         where cmd.COMPLETE_STATUS == ProtocolFormat.OHTMessage.CompleteStatus.CmpStatusSystemIn
                            || cmd.COMPLETE_STATUS == ProtocolFormat.OHTMessage.CompleteStatus.CmpStatusSystemOut
                         select cmd).Take(num);
            return query;
        }

        public List<HCMD> loadTopNCarInOutByCompleteStatus(DBConnection_EF con, int num)
        {
            var query = (from cmd in con.HCMD
                         where cmd.COMPLETE_STATUS == ProtocolFormat.OHTMessage.CompleteStatus.CmpStatusSystemIn
                            || cmd.COMPLETE_STATUS == ProtocolFormat.OHTMessage.CompleteStatus.CmpStatusSystemOut
                         orderby cmd.CMD_END_TIME descending
                         select cmd).Take(num);
            return query.ToList();
        }

        public List<HCMD> loadTopNCarInOutByCmdType(DBConnection_EF con, int num)
        {
            var query = (from cmd in con.HCMD
                         where cmd.CMD_TYPE == E_CMD_TYPE.SystemIn
                            || cmd.CMD_TYPE == E_CMD_TYPE.SystemOut
                         orderby cmd.CMD_INSER_TIME descending
                         select cmd).Take(num);
            return query.ToList();
        }

        public List<HCMD> loadCarInOutByInsertPeriod(DBConnection_EF con, DateTime dtStart, DateTime dtEnd)
        {
            var query = from cmd in con.HCMD
                        where (cmd.CMD_TYPE == E_CMD_TYPE.SystemIn
                            || cmd.CMD_TYPE == E_CMD_TYPE.SystemOut
                            || cmd.CMD_TYPE == E_CMD_TYPE.MTLHome
                            || cmd.CMD_TYPE == E_CMD_TYPE.Move_MTL)
                          && 
                              ((cmd.CMD_INSER_TIME > dtStart && cmd.CMD_INSER_TIME < dtEnd)
                            || (cmd.CMD_INSER_TIME > dtEnd && cmd.CMD_INSER_TIME < dtStart))
                        orderby cmd.CMD_INSER_TIME descending
                        select cmd;
            return query.ToList();
        }
    }

}
