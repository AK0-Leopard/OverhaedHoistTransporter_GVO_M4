using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.Data.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class GroupPortStationBLL
    {
        public DB OperateDB { private set; get; }
        public Catch OperateCatch { private set; get; }

        public GroupPortStationBLL()
        {
        }
        public void start(SCApplication _app)
        {
            OperateDB = new DB(_app.GroupPortStationDao);
            OperateCatch = new Catch(_app.getCommObjCacheManager());
        }
        public class DB
        {
            GroupPortStationDao groupPortStationDao = null;
            public DB(GroupPortStationDao _groupPortStationDao)
            {
                groupPortStationDao = _groupPortStationDao;
            }

            public List<AGROUPPORTSTATION> loadAllGroupPortStation()
            {
                List<AGROUPPORTSTATION> rtnGroupPortStation = null;
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    rtnGroupPortStation = groupPortStationDao.loadAll(con);
                }
                return rtnGroupPortStation;
            }
        }
        public class Catch
        {
            CommObjCacheManager CacheManager;
            public Catch(CommObjCacheManager _cache_manager)
            {
                CacheManager = _cache_manager;
            }
            public List<AGROUPPORTSTATION> loadAllGroupPortStation()
            {
                return CacheManager.loadGroupPortStations();
            }

        }
    }
}