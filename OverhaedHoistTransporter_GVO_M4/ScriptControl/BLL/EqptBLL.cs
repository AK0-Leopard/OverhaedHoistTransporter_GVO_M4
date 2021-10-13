using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.VO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class EqptBLL
    {
        public DB OperateDB { private set; get; }
        public Catch OperateCatch { private set; get; }

        public EqptBLL()
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
            public List<AEQPT> loadMaintainLift()
            {
                var eqpts = CacheManager.getAllEquipment().
                            Where(eq => eq is MaintainLift).
                            ToList();
                return eqpts;
            }
            public List<MaintainLift> loadMTLs()
            {
                List<MaintainLift> MTLs = new List<MaintainLift>();
                var eqpts = CacheManager.getAllEquipment().
                            Where(eq => eq is MaintainLift).
                            ToList();
                foreach (var eqpt in eqpts)
                {
                    MTLs.Add(eqpt as MaintainLift);
                }
                return MTLs;
            }
            public AEQPT GetEqpt(string eqptID)
            {
                var eqpt = CacheManager.getAllEquipment().
                             Where(u => SCUtility.isMatche(u.EQPT_ID, eqptID)).
                             SingleOrDefault();
                return eqpt;
            }
            public List<AGVStation> loadAllAGVStation()
            {
                var eqpts = CacheManager.getAllEquipment().
                             Where(e => e is AGVStation).
                             Select(e => e as AGVStation).
                             ToList();
                return eqpts;
            }
            public AGVStation getAGVStation(string portID)
            {
                var eqpt = CacheManager.getAllEquipment().
                             Where(e => (e is AGVStation) &&
                                        SCUtility.isMatche((e as AGVStation).EQPT_ID, portID)).
                             Select(e => e as AGVStation).
                             FirstOrDefault();
                return eqpt;
            }

            public bool IsAGVStation(string portID)
            {
                var eqpt = CacheManager.getAllEquipment().
                             Where(e => (e is AGVStation) &&
                                        SCUtility.isMatche((e as AGVStation).EQPT_ID, portID)).
                             FirstOrDefault();
                return eqpt != null;
            }

            public SCAppConstants.EqptType GetEqptType(string eqptID)
            {
                var eqpt = CacheManager.getAllEquipment().
                             Where(u => SCUtility.isMatche(u.EQPT_ID, eqptID)).
                             SingleOrDefault();
                return eqpt.Type;
            }
            public MaintainLift GetMaintainLiftBySystemOutAdr(string systemOutAdr)
            {
                var eqpt = CacheManager.getAllEquipment().
                            Where(eq => eq is MaintainLift && (eq as MaintainLift).MTL_SYSTEM_OUT_ADDRESS == systemOutAdr.Trim()).
                            SingleOrDefault();
                return eqpt as MaintainLift;
            }

            public MaintainLift GetMaintainLift()
            {
                var eqpt = CacheManager.getAllEquipment().
                            Where(eq => eq is MaintainLift).
                            SingleOrDefault();
                return eqpt as MaintainLift;
            }
            public MaintainLift GetMaintainLiftByMTLAdr(string mtlAdr)
            {
                var eqpt = CacheManager.getAllEquipment().
                            Where(eq => eq is MaintainLift && (eq as MaintainLift).MTL_ADDRESS == mtlAdr.Trim()).
                            SingleOrDefault();
                return eqpt as MaintainLift;
            }
            public MaintainLift GetMaintainLiftByMTLHomeAdr(string homeAdr)
            {
                var eqpt = CacheManager.getAllEquipment().
                            Where(eq => eq is MaintainLift && (eq as MaintainLift).MTL_CAR_IN_BUFFER_ADDRESS == homeAdr.Trim()).
                            SingleOrDefault();
                return eqpt as MaintainLift;
            }

            public MaintainLift GetMaintainDeviceBySystemInAdr(string systemInAdr)
            {
                var eqpt = CacheManager.getAllEquipment().
                            Where(eq => eq is MaintainLift && (eq as MaintainLift).MTL_SYSTEM_IN_ADDRESS == systemInAdr.Trim()).
                            SingleOrDefault();
                return eqpt as MaintainLift;
            }

            public MaintainLift GetExcuteCarOutMTL(string vhID)
            {
                var eqpt = CacheManager.getAllEquipment().
                                  Where(eq => eq is MaintainLift && (eq as MaintainLift).PreCarOutVhID == vhID.Trim()).
                                  SingleOrDefault();
                return eqpt as MaintainLift;
            }
        }
    }
}