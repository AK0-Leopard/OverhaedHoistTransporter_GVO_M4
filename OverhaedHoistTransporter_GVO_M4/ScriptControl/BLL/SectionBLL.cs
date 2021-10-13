using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.Data.DAO;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class SectionBLL
    {
        public SCApplication scApp;
        public Database dataBase { get; private set; }
        public Cache cache { get; private set; }
        public SectionBLL()
        {
        }
        public void start(SCApplication _app)
        {
            scApp = _app;
            dataBase = new Database(scApp.SectionDao);
            cache = new Cache(scApp.getCommObjCacheManager());
        }
        public class Database
        {
            SectionDao SectionDao = null;
            public Database(SectionDao dao)
            {
                SectionDao = dao;
            }
            public List<ASECTION> loadAllSection()
            {
                List<ASECTION> sections = null;
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    sections = SectionDao.loadAll(con);
                }
                return sections;
            }
        }
        public class Cache
        {
            CommObjCacheManager CommObjCacheManager = null;
            public Cache(CommObjCacheManager commObjCacheManager)
            {
                CommObjCacheManager = commObjCacheManager;
            }
            public ASECTION GetSection(string id)
            {
                return CommObjCacheManager.getSection(id);
            }
            public List<ASECTION> GetSectionBySegmentID(string id)
            {
                return CommObjCacheManager.getSections().
                       Where(sec => sec.SEG_NUM.Trim() == id.Trim()).
                       ToList();
            }

            public ASECTION GetSection(string adr1, string adr2)
            {
                return CommObjCacheManager.getSection(adr1, adr2);
            }
            public List<ASECTION> GetSections()
            {
                return CommObjCacheManager.getSections();
            }
            public List<ASECTION> GetSections(List<string> ids)
            {
                List<ASECTION> result_sections = CommObjCacheManager.getSections().
                                                 Where(sec => ids.Contains(sec.SEC_ID.Trim())).
                                                 ToList();
                return result_sections;
            }
            public List<ASECTION> GetSectionsByFromToAddress(string adr)
            {
                List<ASECTION> result_sections = CommObjCacheManager.getSections().
                                                 //Where(sec => sec.FROM_ADR_ID.Trim() == adr.Trim()).
                                                 Where(sec => SCUtility.isMatche(sec.REAL_FROM_ADR_ID, adr) ||
                                                              SCUtility.isMatche(sec.REAL_TO_ADR_ID, adr)).
                                                 ToList();
                return result_sections;
            }
            public List<ASECTION> GetSectionsByFromAddress(string adr)
            {
                List<ASECTION> result_sections = CommObjCacheManager.getSections().
                                                 //Where(sec => sec.FROM_ADR_ID.Trim() == adr.Trim()).
                                                 Where(sec => sec.REAL_FROM_ADR_ID.Trim() == adr.Trim()).
                                                 ToList();
                return result_sections;
            }

            public List<ASECTION> GetSectionsByToAddress(string adr)
            {
                List<ASECTION> result_sections = CommObjCacheManager.getSections().
                                                 Where(sec => sec.TO_ADR_ID.Trim() == adr.Trim()).
                                                 ToList();
                return result_sections;
            }

            public List<ASECTION> GetSectionsByAddress(string adr_id)
            {
                List<ASECTION> result_sections = CommObjCacheManager.getSections().
                                                 Where(sec => SCUtility.isMatche(sec.FROM_ADR_ID, adr_id) ||
                                                 SCUtility.isMatche(sec.TO_ADR_ID, adr_id)).
                                                 ToList();
                return result_sections;
            }
            public List<ASECTION> GetSectionsByAddresses(List<string> adrIDs)
            {
                List<ASECTION> result_sections = CommObjCacheManager.getSections().
                                                 Where(sec => adrIDs.Contains(SCUtility.Trim(sec.FROM_ADR_ID)) ||
                                                              adrIDs.Contains(SCUtility.Trim(sec.TO_ADR_ID))).
                                                 ToList();
                return result_sections;
            }

            public double GetSectionsDistance(List<string> sectionIDs)
            {
                List<ASECTION> result_sections = CommObjCacheManager.getSections().
                                                 Where(sec => sectionIDs.Contains(sec.SEC_ID.Trim())).
                                                 ToList();
                return result_sections.Sum(sec => sec.SEC_DIS);
            }
        }
    }
}
