
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class UnitBLL
    {
        public DB OperateDB { private set; get; }
        public Catch OperateCatch { private set; get; }

        public UnitBLL()
        {
        }
        public void start(SCApplication _app)
        {
            OperateDB = new DB();
            OperateCatch = new Catch(_app.getEQObjCacheManager());
        }
        public class DB
        {

        }
        public class Catch
        {
            EQObjCacheManager CacheManager;
            public Catch(EQObjCacheManager _cache_manager)
            {
                CacheManager = _cache_manager;
            }

            public AUNIT getUnit(string unitID)
            {
                AUNIT unit = CacheManager.getAllUnit().
                             Where(u => SCUtility.isMatche(u.UNIT_ID, unitID)).
                             SingleOrDefault();
                return unit;
            }
            public List<AUNIT> loadUnits()
            {
                List<AUNIT> units = CacheManager.getAllUnit().
                             Where(u => u.UNIT_ID.Contains("Charger")).
                             ToList();
                return units;
            }
        }
    }
}