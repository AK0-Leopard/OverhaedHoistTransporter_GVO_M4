using com.mirle.ibg3k0.sc.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Service
{
    public class ZoneService
    {
        ZoneBLL zoneBLL;
        VehicleBLL vehicleBLL;
        public void AdjustmentVehicleCount()
        {
            //1.確認所有Zone的水位狀況找出有低於目前水位的
            var zones = zoneBLL.cache.LoadZones();
            var no_enough_zones = zones.Where(zone => !IsVehicleEnoughIncludeOnWay(zone.ZONE_ID));
            if (no_enough_zones.Count() > 0)
            {
                var enough_zones = zones.
                    Where(zone => zone.getCurrentVhs(vehicleBLL).Count > zone.VehicleCountLowerLimit).ToList();
                List<AVEHICLE> can_dispatch_vhs = new List<AVEHICLE>();
                enough_zones.ForEach(zone => can_dispatch_vhs.AddRange(zone.getCurrentVhs(vehicleBLL)));
                foreach (var zone in no_enough_zones)
                {
                    var vh_in_zone = IsVehicleEnoughIncludeOnWay(zone.ZONE_ID);
                    //if (vh_in_zone.Count < zone.VehicleCountLowerLimit)
                    //{
                    //    //當該Zone的數量小於下水位時，要開始從其他的滿足低水位的車(且不能拉走以後就變低水位)
                    //    //因此要找出目前比低水位還要多一台的Zone來支援

                    //}
                }
            }
        }



        //private List<AZONE> LoadCurrentVehicleEnoughZones()
        //{
        //    var zones = zoneBLL.cache.LoadZones();

        //    foreach (var zone in zones)
        //    {
        //        var vh_in_zone = zone.getCurrentVhs(vehicleBLL);
        //        if (vh_in_zone.Count > zone.VehicleCountLowerLimit)//如果比目前的低水位還高的話即可以前往協助其他區
        //        {

        //        }
        //    }
        //}

        private bool IsVehicleEnoughIncludeOnWay(string zoneID)
        {
            AZONE zone = zoneBLL.cache.GetZone(zoneID);
            List<AVEHICLE> in_zone_and_on_way = new List<AVEHICLE>();
            //1.取得目前在該Zone的Vehicle
            var in_current_zone_vh = zone.getCurrentVhs(vehicleBLL);
            if (in_current_zone_vh.Count > 0)
            {
                in_zone_and_on_way.AddRange(in_current_zone_vh);
            }
            //2.要找出在途的Vh，確保不會再多派一台過來
            //todo by kevin
            return in_zone_and_on_way.Count >= zone.VehicleCountLowerLimit;
        }
    }
}
