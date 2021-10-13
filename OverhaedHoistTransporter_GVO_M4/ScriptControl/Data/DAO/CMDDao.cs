
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace com.mirle.ibg3k0.sc.Data.DAO
{
    public class CMDDao
    {
        public void add(DBConnection_EF con, ACMD blockObj)
        {
            con.ACMD.Add(blockObj);
            con.SaveChanges();
        }
        public void RemoteByBatch(DBConnection_EF con, List<ACMD> cmd)
        {
            cmd.ForEach(entity => con.Entry(entity).State = EntityState.Deleted);
            con.ACMD.RemoveRange(cmd);
            con.SaveChanges();
        }

        public void Update(DBConnection_EF con, ACMD cmd)
        {
            con.SaveChanges();
        }
        public void Update(DBConnection_EF con, List<ACMD> cmds)
        {
            con.SaveChanges();
        }

        public IQueryable getQueryAllSQL(DBConnection_EF con)
        {
            var query = from cmd_mcs in con.ACMD
                        select cmd_mcs;
            return query;
        }

        public List<ACMD> loadAll(DBConnection_EF con)
        {
            var query = from block in con.ACMD
                        orderby block.CMD_START_TIME
                        select block;
            return query.ToList();
        }

        public List<ACMD> loadAllQueue_Auto(DBConnection_EF con)
        {
            string sGen_type = ((int)(App.SCAppConstants.GenOHxCCommandType.Auto)).ToString();
            var query = from cmd in con.ACMD.AsNoTracking()
                        where (cmd.CMD_STATUS == E_CMD_STATUS.Queue) &&
                              cmd.ID.StartsWith(sGen_type)
                        //orderby cmd.CMD_START_TIME
                        orderby cmd.ID
                        select cmd;
            return query.ToList();
        }


        public List<ACMD> loadUnfinishCmd(DBConnection_EF con)
        {
            var query = from cmd in con.ACMD.AsNoTracking()
                        where cmd.CMD_STATUS < E_CMD_STATUS.NormalEnd
                        orderby cmd.CMD_START_TIME
                        select cmd;
            return query.ToList();
        }
        public List<ACMD> loadUnfinishCmd(DBConnection_EF con, string vh_id)
        {
            var query = from cmd in con.ACMD
                        where cmd.CMD_STATUS < E_CMD_STATUS.NormalEnd &&
                              cmd.VH_ID.Trim() == vh_id
                        orderby cmd.CMD_START_TIME
                        select cmd;
            return query.ToList();
        }
        public List<ACMD> loadfinishCmd(DBConnection_EF con)
        {
            var query = from cmd in con.ACMD
                        where cmd.CMD_STATUS >= E_CMD_STATUS.NormalEnd
                        select cmd;
            return query.ToList();
        }

        public List<ACMD> loadAssignCmd(DBConnection_EF con, string vh_id)
        {
            var query = from cmd in con.ACMD
                        where cmd.VH_ID == vh_id.Trim() &&
                              cmd.CMD_STATUS < E_CMD_STATUS.NormalEnd
                        orderby cmd.CMD_INSER_TIME
                        select cmd;
            return query.ToList();
        }

        public List<string> loadAssignCmdID(DBConnection_EF con, string vh_id)
        {
            var query = from cmd in con.ACMD
                        where cmd.VH_ID == vh_id.Trim() &&
                              cmd.CMD_STATUS < E_CMD_STATUS.NormalEnd
                        orderby cmd.CMD_INSER_TIME
                        select cmd.ID;
            return query.ToList();
        }
        public int loadAssignCmdIDIgnoreMove(DBConnection_EF con, string vh_id)
        {
            var query = from cmd in con.ACMD
                        where cmd.VH_ID == vh_id.Trim() &&
                              cmd.CMD_STATUS < E_CMD_STATUS.NormalEnd &&
                              cmd.CMD_TYPE != E_CMD_TYPE.Move &&
                              cmd.CMD_TYPE != E_CMD_TYPE.Move_Charger
                        select cmd;
            return query.Count();
        }

        public ACMD getByID(DBConnection_EF con, String cmd_id)
        {
            var query = from cmd in con.ACMD
                        where cmd.ID.Trim() == cmd_id.Trim()
                        select cmd;
            return query.FirstOrDefault();
        }
        public ACMD getByTransferCmdID(DBConnection_EF con, String cmd_id)
        {
            var query = from cmd in con.ACMD
                        where cmd.TRANSFER_ID.Trim() == cmd_id.Trim() &&
                              cmd.CMD_STATUS < E_CMD_STATUS.NormalEnd
                        select cmd;
            return query.FirstOrDefault();
        }

        public string getTransferCmdIDByCmdID(DBConnection_EF con, String cmd_id)
        {
            var query = from cmd in con.ACMD
                        where cmd.ID.Trim() == cmd_id.Trim()
                        select cmd.TRANSFER_ID;
            return query.FirstOrDefault();
        }


        public ACMD getExecuteByVhID(DBConnection_EF con, String vh_id)
        {
            var query = from cmd in con.ACMD
                        where cmd.VH_ID == vh_id.Trim()
                        && cmd.CMD_STATUS >= E_CMD_STATUS.Sending
                        && cmd.CMD_STATUS < E_CMD_STATUS.NormalEnd
                        select cmd;
            return query.FirstOrDefault();
        }
        public ACMD getCMD_OHTCByStatusSending(DBConnection_EF con)
        {
            var query = from cmd in con.ACMD
                        where cmd.CMD_STATUS == E_CMD_STATUS.Sending
                        orderby cmd.CMD_START_TIME
                        select cmd;
            return query.FirstOrDefault();

        }
        public ACMD getCMD_OHTCByVehicleID(DBConnection_EF con, string vh_id)
        {
            var query = from cmd in con.ACMD
                        where cmd.VH_ID == vh_id.Trim()
                        orderby cmd.CMD_START_TIME
                        select cmd;
            return query.FirstOrDefault();
        }
        public ACMD getExcuteCMD_OHTCByCmdID(DBConnection_EF con, string cmd_id)
        {
            var query = from cmd in con.ACMD
                        where cmd.ID == cmd_id.Trim()
                        && cmd.CMD_STATUS < E_CMD_STATUS.NormalEnd
                        select cmd;
            return query.FirstOrDefault();
        }

        public int getVhQueueCMDConut(DBConnection_EF con, string vh_id)
        {
            var query = from cmd in con.ACMD.AsNoTracking()
                        where cmd.VH_ID == vh_id.Trim() &&
                        cmd.CMD_STATUS == E_CMD_STATUS.Queue
                        select cmd;
            return query.Count();
        }


        public int getVhAssignCMDConut(DBConnection_EF con, string vh_id)
        {
            var query = from cmd in con.ACMD
                        where cmd.VH_ID == vh_id.Trim() &&
                        //cmd.CMD_STATUS > E_CMD_STATUS.Queue &&
                        cmd.CMD_STATUS < E_CMD_STATUS.NormalEnd
                        select cmd;
            return query.Count();
        }
        public int getVhExcuteCMDConut(DBConnection_EF con, string vh_id)
        {
            var query = from cmd in con.ACMD
                        where cmd.VH_ID == vh_id.Trim() &&
                        cmd.CMD_STATUS > E_CMD_STATUS.Queue &&
                        cmd.CMD_STATUS < E_CMD_STATUS.NormalEnd
                        select cmd;
            return query.Count();
        }
        public int getSourcePortExcuteCMDConut(DBConnection_EF con, string sourcePort)
        {
            var query = from cmd in con.ACMD
                        where cmd.SOURCE_PORT == sourcePort.Trim() &&
                              cmd.CMD_STATUS < E_CMD_STATUS.NormalEnd
                        select cmd;
            return query.Count();
        }
        public int getDestinationPortExcuteCMDConut(DBConnection_EF con, string destinationPort)
        {
            var query = from cmd in con.ACMD
                        where cmd.DESTINATION_PORT == destinationPort.Trim() &&
                              cmd.CMD_STATUS < E_CMD_STATUS.NormalEnd
                        select cmd;
            return query.Count();
        }

        public int getAddressesExcuteCMDConut(DBConnection_EF con, List<string> addresses)
        {
            var query = from cmd in con.ACMD
                        where (addresses.Contains(cmd.SOURCE.Trim()) || addresses.Contains(cmd.DESTINATION.Trim())) &&
                              cmd.CMD_STATUS < E_CMD_STATUS.NormalEnd

                        select cmd;
            return query.Count();
        }

        public List<ACMD> getCMD_OHTCByConditionss(DBConnection_EF conn, DateTime startDatetime, DateTime endDatetime,
      string CSTID = null, string VhID = null)
        {
            var query = conn.ACMD.Where(x => x.CMD_START_TIME > startDatetime
                && x.CMD_START_TIME < endDatetime
                && x.CMD_STATUS == E_CMD_STATUS.NormalEnd);

            if (!string.IsNullOrEmpty(CSTID))
            {
                query = query.Where(x => x.CARRIER_ID == CSTID);
            }
            if (!string.IsNullOrEmpty(VhID))
            {
                query = query.Where(x => x.VH_ID == VhID);
            }
            return query.ToList();
        }


    }
}