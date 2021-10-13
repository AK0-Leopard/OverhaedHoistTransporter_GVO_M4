using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class ZoneBLL
    {
        public Cache cache { get; private set; }

        public void start(SCApplication _app)
        {
            cache = new Cache(_app.getEQObjCacheManager());
        }


        public class Cache
    {
            EQObjCacheManager eqObjCacheManager = null;
            public Cache(EQObjCacheManager eqObjCacheManager)
            {
                this.eqObjCacheManager = eqObjCacheManager;
            }

            public AZONE GetZone(string zoneID)
            {
                var zones = eqObjCacheManager.getZoneListByLine();
                return zones.Where(zone => SCUtility.isMatche(zone.ZONE_ID, zoneID)).
                             FirstOrDefault();
            }

            public List<AZONE> LoadZones()
            {
                return eqObjCacheManager.getZoneListByLine();
            }


        }
    }
}
