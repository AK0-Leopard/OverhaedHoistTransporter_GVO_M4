using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.ObjectRelay;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace com.mirle.ibg3k0.sc.Data.DAO
{
    public class VehicleDao
    {
        List<AVEHICLE> lstVh = null;

        public void start(List<AVEHICLE> _lstVh)
        {
            lstVh = _lstVh;

        }

        public void add(DBConnection_EF con, AVEHICLE vh_id)
        {
            con.AVEHICLE.Add(vh_id);
            con.SaveChanges();
        }

        public void doUpdate( DBConnection_EF con, AVEHICLE vh)
        {
            vh.UPD_TIME = DateTime.Now;
            update(con, vh);
        }

        private void updateCatchManager(SCApplication app, DBConnection_EF con, AVEHICLE vh)
        {
            var changedEntity = con.Entry(vh);
            AVEHICLE vh_vo = app.getEQObjCacheManager().getVehicletByVHID(vh.VEHICLE_ID);
            //VehicleObjToShow showObj = SCApplication.getInstance().getEQObjCacheManager().CommonInfo.ObjectToShow_list.
            //    Where(o => o.VEHICLE_ID == vh.VEHICLE_ID).SingleOrDefault();
            foreach (string propertyName in changedEntity.CurrentValues.PropertyNames)
            {
                if (changedEntity.Property(propertyName).IsModified)
                {
                    #region 更新CatchManager的資料
                    string setPropertyName = string.Empty;
                    switch (propertyName)
                    {
                        case nameof(vh_vo.OBS_PAUSE):
                            setPropertyName = nameof(vh_vo.ObstacleStatus);
                            break;
                        case nameof(vh_vo.BLOCK_PAUSE):
                            setPropertyName = nameof(vh_vo.BlockingStatus);
                            break;
                        case nameof(vh_vo.CMD_PAUSE):
                            setPropertyName = nameof(vh_vo.PauseStatus);
                            break;
                        //case nameof(vh_vo.BATTERYCAPACITY):
                        //    setPropertyName = nameof(vh_vo.BatteryCapacity);
                        //    break;
                        default:
                            setPropertyName = propertyName;
                            break;
                    }
                    var prop = typeof(AVEHICLE).GetProperty(setPropertyName);
                    if (prop != null)
                    {
                        prop.SetValue(vh_vo, changedEntity.Property(propertyName).CurrentValue);
                    }
                    #endregion 更新CatchManager的資料

                    #region 更新Show在畫面上 DGV的資料

                    var prop_for_showObj = typeof(VehicleObjToShow).GetProperty(propertyName);
                    if (prop_for_showObj != null)
                    {
                        //prop_for_showObj.SetValue(showObj, changedEntity.Property(propertyName).CurrentValue);
                        //showObj.NotifyPropertyChanged(propertyName);
                    }
                    #endregion 更新Show在畫面上 DGV的資料
                }
            }
        }

        private void update(DBConnection_EF con, AVEHICLE vh)
        {
            //vh_id.UPD_TIME = DateTime.Now.ToString(SCAppConstants.DateTimeFormat_22);
            //bool isDetached = con.Entry(vh_id).State == EntityState.Modified;
            //if (isDetached)
            {
                con.SaveChanges();
            }
        }


        public List<AVEHICLE> loadAll(DBConnection_EF con)
        {
            var query = from vh in con.AVEHICLE
                        orderby vh.VEHICLE_ID
                        select vh;
            return query.ToList();
        }

        public AVEHICLE getByID(DBConnection_EF con, String cmd_id)
        {
            var query = from vh in con.AVEHICLE
                        where vh.VEHICLE_ID == cmd_id.Trim()
                        select vh;
            return query.FirstOrDefault();
        }

    }
}
