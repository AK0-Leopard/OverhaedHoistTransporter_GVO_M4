using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.Data.DAO;
using NLog;
using System.Collections.Generic;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class CEIDBLL
    {
        CEIDDao ceidDAO = null;
        private Logger logger = LogManager.GetCurrentClassLogger();
        RPTIDDao rptidDAO = null;
        private SCApplication scApp = null;
        public CEIDBLL()
        {

        }
        public void start(SCApplication app)
        {
            scApp = app;
            ceidDAO = scApp.CEIDDao;
            rptidDAO = scApp.RPTIDDao;
        }


        public bool buildCEIDAndReportID(Dictionary<string, string[]> reportItem)
        {
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                foreach (var item in reportItem)
                {
                    int order = 1;
                    foreach (var report_id in item.Value)
                    {
                        addCEID(con, item.Key, report_id, order++);
                    }
                }
            }
            return true;
        }

        public bool buildCEIDsFromMCS(Data.SECS.S2F35.RPTITEM[] rPTITEMs)
        {
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                foreach (var item in rPTITEMs)
                {
                    int order = 1;
                    foreach (var rpt_item in item.RPTIDS)
                    {
                        addCEID(con, item.CEID.PadLeft(3, '0'), rpt_item, order++);
                    }
                }
            }
            return true;
        }


        public bool addCEID(DBConnection_EF con, string ceid, string rpt_id, int order)
        {
            bool isSuccess = true;
            ACEID aCEID = new ACEID()
            {
                CEID = ceid.Trim(),
                RPTID = rpt_id.Trim(),
                ORDER_NUM = order
            };
            ceidDAO.add(con, aCEID);
            return isSuccess;
        }

        public bool getCEID(string vh_id)
        {
            bool isSuccess = true;

            return isSuccess;
        }
        public void DeleteCEIDInfoByBatch()
        {
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                string sql = "DELETE FROM [ACEID] ";
                con.Database.ExecuteSqlCommand(sql);
            }
        }


        public Dictionary<string, List<string>> loadDicCEIDAndRPTID()
        {
            Dictionary<string, List<string>> dicCeidAndRptid = null;
            using (DBConnection_EF con = new DBConnection_EF())
            {
                dicCeidAndRptid = ceidDAO.loadAllCEIDByGroup(con);
            }
            return dicCeidAndRptid;
        }



        public Dictionary<string, List<ARPTID>> loadDicRPTIDAndVID()
        {
            Dictionary<string, List<ARPTID>> dicRptidAndVid = null;
            using (DBConnection_EF con = new DBConnection_EF())
            {
                dicRptidAndVid = rptidDAO.loadAllRPTIDByGroup(con);
            }
            return dicRptidAndVid;
        }

        public bool buildReportIDAndVid(Dictionary<string, string[]> reportItems)
        {
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                foreach (var item in reportItems)
                {
                    int order = 1;
                    foreach (var rpt_item in item.Value)
                    {
                        addRpt(con, item.Key, rpt_item, order++);
                    }
                }
            }
            return true;
        }
        public bool buildRptsFromMCS(Data.SECS.S2F33.RPTITEM[] rPTITEMs)
        {
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                foreach (var item in rPTITEMs)
                {
                    int order = 1;
                    foreach (var rpt_item in item.VIDS)
                    {
                        if (rpt_item.Length > 3)
                        {
                            logger.Warn($"Build report event of vid over length,report id:{item.REPTID},vid:{rpt_item}");
                            continue;
                        }
                        addRpt(con, item.REPTID, rpt_item, order++);
                    }
                }
            }
            return true;
        }



        public bool addRpt(DBConnection_EF con, string rpt_id, string vid, int order)
        {
            bool isSuccess = true;
            ARPTID rpt = new ARPTID()
            {
                RPTID = rpt_id.Trim(),
                VID = vid.Trim(),
                ORDER_NUM = order
            };
            rptidDAO.add(con, rpt);
            return isSuccess;
        }

        public void DeleteRptInfoByBatch()
        {
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                string sql = "DELETE FROM [ARPTID]";
                con.Database.ExecuteSqlCommand(sql);
            }
        }


    }
}
