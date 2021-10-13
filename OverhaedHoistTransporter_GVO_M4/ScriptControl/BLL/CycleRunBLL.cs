using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.Data.DAO;
using com.mirle.ibg3k0.sc.Data.VO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class CycleRunBLL
    {
        CycleZoneTypeDao cycleZoneTypeDao = null;
        CycleZoneMasterDao cycleZoneMasterDao = null;
        CycleZoneDetailDao cycleZoneDetailDao = null;
        private SCApplication scApp = null;
        public CycleRunBLL()
        {

        }
        public void start(SCApplication app)
        {
            scApp = app;
            cycleZoneTypeDao = scApp.CycleZoneTypeDao;
            cycleZoneMasterDao = scApp.CycleZoneMasterDao;
            cycleZoneDetailDao = scApp.CycleZoneDetailDao;
        }






    }
}
