using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.BLL;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using System.Collections.Generic;
using System.Linq;

namespace com.mirle.ibg3k0.sc.Module
{
    public class AvoidVehicleModule
    {
        private VehicleBLL vehicleBLL = null;
        private SectionBLL sectionBLL = null;
        public AvoidVehicleModule()
        {

        }
        public void start(SCApplication app)
        {
            vehicleBLL = app.VehicleBLL;
            sectionBLL = app.SectionBLL;
        }

        private string findAvoidAddress(string startSection, List<string> startSearchAddresses, List<string> willPassSections, List<string> alreaySearchedAddresses = null)
        {
            List<string> next_search_addresses = new List<string>();
            List<string> alreay_searched_addresses = alreaySearchedAddresses == null ?
                                                        new List<string>() : alreaySearchedAddresses;
            alreay_searched_addresses.AddRange(startSearchAddresses);
            foreach (string adr in startSearchAddresses)
            {
                //2.2-找出所有下一個有連接的Section，並濾掉自己所在的Section
                List<ASECTION> next_sections = sectionBLL.cache.GetSectionsByAddress(adr);
                next_sections = next_sections.OrderBy(sec => sec.SEC_DIS).ToList();
                foreach (ASECTION next_sec in next_sections)
                {
                    if (SCUtility.isMatche(next_sec.SEC_ID, startSection)) continue;

                    if (willPassSections.Where(sec_id => SCUtility.isMatche(sec_id, next_sec.SEC_ID)).Count() > 0)
                    {
                        if (SCUtility.isMatche(adr, next_sec.FROM_ADR_ID) && !alreay_searched_addresses.Contains(next_sec.TO_ADR_ID.Trim()))
                            next_search_addresses.Add(next_sec.TO_ADR_ID.Trim());
                        if (SCUtility.isMatche(adr, next_sec.TO_ADR_ID) && !alreay_searched_addresses.Contains(next_sec.FROM_ADR_ID.Trim()))
                            next_search_addresses.Add(next_sec.FROM_ADR_ID.Trim());
                        continue;
                    }

                    //if (SCUtility.isMatche(next_sec.FROM_ADR_ID, adr))
                    //{
                    //    if (!SCUtility.isEmpty(next_sec.FROM_ADR_AVOID_ADR))
                    //    {
                    //        return next_sec.FROM_ADR_AVOID_ADR;
                    //    }
                    //    if (!alreay_searched_addresses.Contains(next_sec.TO_ADR_ID.Trim()))
                    //        next_search_addresses.Add(next_sec.TO_ADR_ID.Trim());
                    //}
                    //else if (SCUtility.isMatche(next_sec.TO_ADR_ID, adr))
                    //{
                    //    if (!SCUtility.isEmpty(next_sec.TO_ADR_AVOID_ADR))
                    //    {
                    //        return next_sec.TO_ADR_AVOID_ADR;
                    //    }
                    //    if (!alreay_searched_addresses.Contains(next_sec.FROM_ADR_ID.Trim()))
                    //        next_search_addresses.Add(next_sec.FROM_ADR_ID.Trim());
                    //}
                }
            }
            if (next_search_addresses.Count != 0)
            {
                return findAvoidAddress(startSection, next_search_addresses, willPassSections, alreay_searched_addresses);
            }
            return string.Empty;
        }

        private (bool hasVh, List<AVEHICLE> avoidVh) HasIdleVehicleOnTheWay(List<ReserveInfo> reserveInfos)
        {
            bool has_idle_vh_on_the_way = false;
            List<AVEHICLE> vhs_on_the_way = new List<AVEHICLE>();
            foreach (ReserveInfo reserve in reserveInfos)
            {
                List<AVEHICLE> vhs = vehicleBLL.cache.loadVehicleBySEC_ID(reserve.ReserveSectionID);
                if (vhs != null && vhs.Count > 0)
                    vhs_on_the_way.AddRange(vhs);
            }
            foreach (AVEHICLE vh in vhs_on_the_way.ToList())
            {
                if (vh.ACT_STATUS != VHActionStatus.NoCommand)
                {
                    vhs_on_the_way.Remove(vh);
                }
            }
            if (vhs_on_the_way.Count != 0) has_idle_vh_on_the_way = true;
            return (has_idle_vh_on_the_way, vhs_on_the_way);
        }


        public class VehicleAvoidInfo
        {
            public VehicleAvoidInfo(AVEHICLE obstructedVehicle, string avoidAddress)
            {
                ObstructedVehicle = obstructedVehicle;
                AvoidAddress = avoidAddress;
            }
            public AVEHICLE ObstructedVehicle;
            public string AvoidAddress;
        }

    }
}
