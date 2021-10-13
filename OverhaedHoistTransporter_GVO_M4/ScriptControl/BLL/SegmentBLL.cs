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
    public class SegmentBLL
    {
        public SCApplication scApp;
        public Database dataBase { get; private set; }
        public Cache cache { get; private set; }
        public SegmentBLL()
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
            public Database(SectionDao dao)
            {

            }

        }
        public class Cache
        {
            CommObjCacheManager CommObjCacheManager = null;
            public Cache(CommObjCacheManager commObjCacheManager)
            {
                CommObjCacheManager = commObjCacheManager;
            }
            public List<ASEGMENT> GetSegments()
            {
                return CommObjCacheManager.getSegments();
            }
            public ASEGMENT GetSegment(string id)
            {
                if (SCUtility.isEmpty(id)) return null;
                return CommObjCacheManager.getSegments().
                    Where(seg => seg.SEG_ID.Trim() == id.Trim()).
                    SingleOrDefault();
            }
            public List<ASEGMENT> GetSegments(string addressID)
            {
                if (SCUtility.isEmpty(addressID)) return null;
                return CommObjCacheManager.getSegments().
                    Where(seg => seg.RealFromAddress.Trim() == addressID.Trim()).
                    ToList();
            }
            public ASEGMENT GetSegment(string adr1, string adr2)
            {
                return CommObjCacheManager.getSegments().
                       Where(s => (s.FROM_ADR_ID.Trim() == adr1.Trim() && s.TO_ADR_ID.Trim() == adr2.Trim())
                                    || (s.FROM_ADR_ID.Trim() == adr2.Trim() && s.TO_ADR_ID.Trim() == adr1.Trim())).FirstOrDefault();
            }

            public bool IsSegmentAddress(string adr)
            {
                return CommObjCacheManager.getSegments().
                    Where(seg => SCUtility.isMatche(seg.FROM_ADR_ID, adr) || SCUtility.isMatche(seg.TO_ADR_ID, adr)).
                    Count() > 0;
            }

            public void EnableSegment(string segID)
            {
                ASEGMENT seg = CommObjCacheManager.getSegments().Where(s => s.SEG_ID.Trim() == segID.Trim())
                                                  .FirstOrDefault();
                seg.PRE_DISABLE_FLAG = false;
                seg.PRE_DISABLE_TIME = null;
                seg.DISABLE_TIME = null;
                seg.STATUS = E_SEG_STATUS.Active;
            }

            public void DisableSegment(string segID)
            {
                ASEGMENT seg = CommObjCacheManager.getSegments().Where(s => s.SEG_ID.Trim() == segID.Trim())
                                                  .FirstOrDefault();
                seg.PRE_DISABLE_FLAG = false;
                seg.PRE_DISABLE_TIME = null;
                seg.DISABLE_TIME = DateTime.Now;
                seg.STATUS = E_SEG_STATUS.Closed;
            }


        }
    }
}
