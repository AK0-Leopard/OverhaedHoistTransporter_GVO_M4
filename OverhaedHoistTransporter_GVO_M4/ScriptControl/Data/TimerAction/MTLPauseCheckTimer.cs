using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.bcf.Data.TimerAction;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction;
using com.mirle.ibg3k0.sc.Data.VO;

namespace com.mirle.ibg3k0.sc.Data.TimerAction
{
    public class MTLPauseCheckTimer : ITimerAction
    {
        protected SCApplication scApp = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MTLPauseCheckTimer"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="intervalMilliSec">The interval milli sec.</param>
        public MTLPauseCheckTimer(string name, long intervalMilliSec)
            : base(name, intervalMilliSec)
        {
        }

        /// <summary>
        /// The synchronize point
        /// </summary>
        private long syncPoint = 0;
        /// <summary>
        /// Timer Action的執行動作
        /// </summary>
        /// <param name="obj">The object.</param>
        public override void doProcess(object obj)
        {
            if (System.Threading.Interlocked.Exchange(ref syncPoint, 1) == 0)
            {
                try
                {
                    AEQPT eqpt = scApp.getEQObjCacheManager().getEquipmentByEQPTID("MTL");
                    if (eqpt != null)
                    {
                        MaintainLift mtl = eqpt as MaintainLift;
                        if (mtl != null)
                        {
                            MTLValueDefMapAction mtlMapAction = mtl.getMapActionByIdentityKey(typeof(MTLValueDefMapAction).Name) as MTLValueDefMapAction;
                            if (mtlMapAction != null)
                            {
                                Task.Run(() => scApp.VehicleService.checkThenSetVehiclePauseByMTLStatus("Timer"));
                            }
                        }
                    }
                }
                finally
                {
                    System.Threading.Interlocked.Exchange(ref syncPoint, 0);
                }
            }
        }

        /// <summary>
        /// Initializes the start.
        /// </summary>
        public override void initStart()
        {
            scApp = SCApplication.getInstance();
        }
    }
}
