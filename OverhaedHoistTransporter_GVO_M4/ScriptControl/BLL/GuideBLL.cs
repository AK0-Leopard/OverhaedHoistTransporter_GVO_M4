using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using com.mirle.ibg3k0.sc.RouteKit;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class GuideBLL
    {
        SCApplication scApp;
        Logger logger = LogManager.GetCurrentClassLogger();

        public void start(SCApplication _scApp)
        {
            scApp = _scApp;
        }
        //public (RouteInfo stratFromRouteInfo, RouteInfo fromToRouteInfo) getGuideInfo(string startaddress, string sourceAddress, string destAddress)
        //{
        //    int.TryParse(startaddress, out int i_start_address);
        //    int.TryParse(sourceAddress, out int i_source_address);
        //    int.TryParse(destAddress, out int i_dest_address);
        //    (List<RouteInfo> stratFromRouteInfoList, List<RouteInfo> fromToRouteInfoList) =
        //        scApp.NewRouteGuide.getStartFromThenFromToRoutesAddrToAddrToAddr(i_start_address, i_source_address, i_dest_address);
        //    return (stratFromRouteInfoList.First(), fromToRouteInfoList.First());
        //}

        public (bool isSuccess, List<string> guideSegmentIds, List<string> guideSectionIds, List<string> guideAddressIds, int totalCost)
            getGuideInfo(string startAddress, string targetAddress, List<string> byPassSectionIDs = null)
        {
            if (SCUtility.isMatche(startAddress, targetAddress))
            {
                return (true, new List<string>(), new List<string>(), new List<string>(), 0);
            }

            bool is_success = false;
            int.TryParse(startAddress, out int i_start_address);
            int.TryParse(targetAddress, out int i_target_address);

            List<RouteInfo> stratFromRouteInfoList = null;
            if (byPassSectionIDs == null || byPassSectionIDs.Count == 0)
            {
                stratFromRouteInfoList = scApp.NewRouteGuide.getFromToRoutesAddrToAddr(i_start_address, i_target_address);
            }
            else
            {
                stratFromRouteInfoList = scApp.NewRouteGuide.getFromToRoutesAddrToAddr(i_start_address, i_target_address, byPassSectionIDs);
            }
            RouteInfo min_stratFromRouteInfo = null;
            if (stratFromRouteInfoList != null && stratFromRouteInfoList.Count > 0)
            {
                min_stratFromRouteInfo = stratFromRouteInfoList.First();
                is_success = true;
            }
            else
            {
                return (false, null, null, null, int.MaxValue);
            }

            return (is_success, null, min_stratFromRouteInfo.GetSectionIDs(), min_stratFromRouteInfo.GetAddressesIDs(), min_stratFromRouteInfo.total_cost);
        }




        public (bool isSuccess, List<string> guideSegmentIds, List<string> guideSectionIds, List<string> guideAddressIds, int totalCost)
        getGuideInfo_New2(string startAddress, string targetAddress, List<string> byPassAddressIDs = null)
        {
            return getGuideInfo_New2("", startAddress, targetAddress, byPassAddressIDs);
        }
        public (bool isSuccess, List<string> guideSegmentIds, List<string> guideSectionIds, List<string> guideAddressIds, int totalCost)
        getGuideInfo_New2(string startSection, string startAddress, string targetAddress, List<string> byPassAddressIDs = null)
        {
            AADDRESS start_adr = scApp.getCommObjCacheManager().getAddress(startAddress);
            AADDRESS target_adr = scApp.getCommObjCacheManager().getAddress(targetAddress);
            string start_node = startAddress;
            string target_node = targetAddress;
            //bool is_node_adr_start = start_adr.IsSegment;
            //bool is_node_adr_target = target_adr.IsSegment;
            bool is_node_adr_start = scApp.SegmentBLL.cache.IsSegmentAddress(startAddress);
            bool is_node_adr_target = scApp.SegmentBLL.cache.IsSegmentAddress(targetAddress);

            if (SCUtility.isMatche(startAddress, targetAddress))
            {
                return (true, new List<string>(), new List<string>(), new List<string>(), 0);
            }
            if (!is_node_adr_start)
            {
                string start_section_adr_id = null;
                ASECTION start_adr_section = null;

                //if (!start_adr.IsSection)
                if (start_adr.IsControl)
                {
                    var nearSectionAddress = scApp.AddressesBLL.cache.GetAddress(start_adr.NODE_ID);
                    if (nearSectionAddress == null)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(GuideBLL), Device: "OHxC",
                           Data: $"Start address:{startAddress} is a nonsection address,but node id:{start_adr.NODE_ID} not exist.");
                        return (false, null, null, null, int.MaxValue);
                    }
                    else
                    {
                        start_section_adr_id = nearSectionAddress.ADR_ID;
                    }
                    start_adr_section = scApp.SectionBLL.cache.GetSection(start_adr.SEC_ID);
                    if (start_adr_section == null)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(GuideBLL), Device: "OHxC",
                           Data: $"Start address:{startAddress} is a nonsection address,but section id:{start_adr.SEC_ID} not exist.");
                        return (false, null, null, null, int.MaxValue);
                    }
                }
                else
                {
                    start_section_adr_id = start_adr.ADR_ID;
                    start_adr_section = scApp.SectionBLL.cache.GetSectionsByFromAddress(start_section_adr_id).First();
                }
                ASEGMENT start_adr_segment = scApp.SegmentBLL.cache.GetSegment(start_adr_section.SEG_NUM);
                if (start_adr_segment.STATUS != E_SEG_STATUS.Active)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(GuideBLL), Device: "OHxC",
                       Data: $"Segment ID:{start_adr_segment.SEG_ID} is disable, can't find guide info");
                    return (false, null, null, null, int.MaxValue);
                }
                start_node = start_adr_segment.getNearSegmentAddress(start_section_adr_id);
            }
            if (!is_node_adr_target)
            {
                string target_section_adr_id = null;
                ASECTION target_adr_section = null;
                //if (!target_adr.IsSection)
                if (target_adr.IsControl)
                {
                    var nearSectionAddress = scApp.AddressesBLL.cache.GetAddress(target_adr.NODE_ID);
                    if (nearSectionAddress == null)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(GuideBLL), Device: "OHxC",
                           Data: $"Target address:{startAddress} is a nonsection address,but node id:{target_adr.NODE_ID} not exist.");
                        return (false, null, null, null, int.MaxValue);
                    }
                    else
                    {
                        target_section_adr_id = nearSectionAddress.ADR_ID;
                    }
                    target_adr_section = scApp.SectionBLL.cache.GetSection(target_adr.SEC_ID);
                    if (target_adr_section == null)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(GuideBLL), Device: "OHxC",
                           Data: $"Target address:{startAddress} is a nonsection address,but section id:{target_adr.SEC_ID} not exist.");
                        return (false, null, null, null, int.MaxValue);
                    }
                }
                else
                {
                    target_section_adr_id = target_adr.ADR_ID;
                    target_adr_section = scApp.SectionBLL.cache.GetSectionsByFromAddress(target_section_adr_id).First();
                }
                ASEGMENT target_adr_segment = scApp.SegmentBLL.cache.GetSegment(target_adr_section.SEG_NUM);
                if (target_adr_segment.STATUS != E_SEG_STATUS.Active)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(GuideBLL), Device: "OHxC",
                       Data: $"Segment ID:{target_adr_segment.SEG_ID} is disable, can't find guide info");
                    return (false, null, null, null, int.MaxValue);
                }
                target_node = target_adr_segment.getNearSegmentAddress(target_section_adr_id);
            }

            if (SCUtility.isMatche(start_node, target_node))
            {
                HashSet<string> guideSegmentIds = new HashSet<string>();
                List<string> guideSectionIds = new List<string>();
                List<string> guideAddressIds = new List<string>();

                ASECTION start_adr_section = null;
                ASECTION target_adr_section = null;
                if (start_adr.IsControl)
                {
                    start_adr_section = scApp.SectionBLL.cache.GetSection(start_adr.SEC_ID);
                }
                else
                {
                    if (!SCUtility.isEmpty(startSection))
                    {
                        start_adr_section = scApp.SectionBLL.cache.GetSection(startSection);
                    }
                    else
                    {
                        start_adr_section = scApp.SectionBLL.cache.GetSectionsByToAddress(start_adr.ADR_ID).FirstOrDefault();
                        if (start_adr_section == null)
                            start_adr_section = scApp.SectionBLL.cache.GetSectionsByFromAddress(start_adr.ADR_ID).FirstOrDefault();
                    }
                }
                if (target_adr.IsControl)
                {
                    target_adr_section = scApp.SectionBLL.cache.GetSection(target_adr.SEC_ID);
                }
                else
                {
                    target_adr_section = scApp.SectionBLL.cache.GetSectionsByToAddress(target_adr.ADR_ID).FirstOrDefault();
                    if (target_adr_section == null)
                        target_adr_section = scApp.SectionBLL.cache.GetSectionsByFromAddress(target_adr.ADR_ID).FirstOrDefault();
                }

                ASEGMENT start_adr_seg = scApp.SegmentBLL.cache.GetSegment(start_adr_section.SEG_NUM);
                ASEGMENT target_adr_seg = scApp.SegmentBLL.cache.GetSegment(target_adr_section.SEG_NUM);
                if (SCUtility.isMatche(start_adr_seg.SEG_ID, target_adr_seg.SEG_ID))
                {
                    if (SCUtility.isMatche(start_adr_section.SEC_ID, target_adr_section.SEC_ID))
                    {
                        if (SCUtility.isMatche(start_adr_section.FROM_ADR_ID, start_adr.ADR_ID))
                        {
                            guideAddressIds.Add(start_adr_section.FROM_ADR_ID);
                            guideAddressIds.Add(start_adr_section.TO_ADR_ID);
                        }
                        else if (SCUtility.isMatche(start_adr_section.TO_ADR_ID, start_adr.ADR_ID))
                        {

                            guideAddressIds.Add(start_adr_section.TO_ADR_ID);
                            guideAddressIds.Add(start_adr_section.FROM_ADR_ID);
                        }
                        else
                        {
                            if (start_adr.DISTANCE > target_adr.DISTANCE)
                            {
                                guideAddressIds.Add(start_adr_section.TO_ADR_ID);
                                guideAddressIds.Add(start_adr_section.FROM_ADR_ID);
                            }
                            else
                            {
                                guideAddressIds.Add(start_adr_section.FROM_ADR_ID);
                                guideAddressIds.Add(start_adr_section.TO_ADR_ID);
                            }
                        }
                        guideSegmentIds.Add(start_adr_section.SEG_NUM);
                    }
                    else
                    {
                        int start_sec_index = start_adr_seg.GetSectionIndex(start_adr_section);
                        int target_sec_index = start_adr_seg.GetSectionIndex(target_adr_section);
                        if (start_adr.IsControl)
                        {
                            if (start_sec_index < target_sec_index)
                            {
                                guideAddressIds.Add(start_adr_section.FROM_ADR_ID);
                            }
                            else
                            {
                                guideAddressIds.Add(start_adr_section.TO_ADR_ID);
                            }
                            //guideAddressIds.Add(start_adr.NODE_ID);
                        }
                        else
                        {
                            guideAddressIds.Add(start_adr.ADR_ID);
                        }
                        if (target_adr.IsControl)
                        {
                            if (target_sec_index < start_sec_index)
                            {
                                guideAddressIds.Add(target_adr_section.FROM_ADR_ID);
                            }
                            else
                            {
                                guideAddressIds.Add(target_adr_section.TO_ADR_ID);
                            }
                            //guideAddressIds.Add(target_adr.NODE_ID);
                        }
                        else
                        {
                            guideAddressIds.Add(target_adr.ADR_ID);
                        }
                        guideSegmentIds.Add(start_adr_section.SEG_NUM);
                        //int index_start_adr_section = start_adr_seg.Sections.IndexOf(start_adr_section);
                        //int index_target_adr_section = start_adr_seg.Sections.IndexOf(target_adr_section);

                        //if (index_start_adr_section < index_target_adr_section)
                        //{
                        //    guideAddressIds.Add(start_adr_section.FROM_ADR_ID);
                        //    guideAddressIds.Add(target_adr_section.TO_ADR_ID);
                        //}
                        //else
                        //{
                        //    guideAddressIds.Add(start_adr_section.TO_ADR_ID);
                        //    guideAddressIds.Add(target_adr_section.FROM_ADR_ID);
                        //}
                        //guideSegmentIds.Add(start_adr_section.SEG_NUM);
                    }
                }
                else
                {
                    //if (SCUtility.isMatche(start_adr_seg.RealToAddress, target_adr_seg.RealFromAddress))
                    //{
                    //    guideAddressIds.Add(start_adr_section.FROM_ADR_ID);
                    //    guideAddressIds.Add(start_adr_seg.RealToAddress);
                    //    guideAddressIds.Add(target_adr_section.TO_ADR_ID);
                    //}
                    //else
                    //{
                    //    guideAddressIds.Add(start_adr_section.TO_ADR_ID);
                    //    guideAddressIds.Add(start_adr_seg.RealFromAddress);
                    //    guideAddressIds.Add(target_adr_section.FROM_ADR_ID);
                    //}
                    //guideSegmentIds.Add(start_adr_section.SEG_NUM);
                    //guideSegmentIds.Add(target_adr_section.SEG_NUM);
                    if (SCUtility.isMatche(start_adr_seg.RealFromAddress, target_adr_seg.RealFromAddress))
                    {
                        guideAddressIds.Add(start_adr_section.TO_ADR_ID);
                        guideAddressIds.Add(start_adr_seg.RealFromAddress);
                        guideAddressIds.Add(target_adr_section.TO_ADR_ID);
                    }
                    else if (SCUtility.isMatche(start_adr_seg.RealToAddress, target_adr_seg.RealToAddress))
                    {
                        guideAddressIds.Add(start_adr_section.FROM_ADR_ID);
                        guideAddressIds.Add(start_adr_seg.RealToAddress);
                        guideAddressIds.Add(target_adr_section.FROM_ADR_ID);
                    }
                    else if (SCUtility.isMatche(start_adr_seg.RealToAddress, target_adr_seg.RealFromAddress))
                    {
                        guideAddressIds.Add(start_adr_section.FROM_ADR_ID);
                        guideAddressIds.Add(start_adr_seg.RealToAddress);
                        guideAddressIds.Add(target_adr_section.TO_ADR_ID);
                    }
                    else
                    {
                        guideAddressIds.Add(start_adr_section.TO_ADR_ID);
                        guideAddressIds.Add(start_adr_seg.RealFromAddress);
                        guideAddressIds.Add(target_adr_section.FROM_ADR_ID);
                    }
                    guideSegmentIds.Add(start_adr_section.SEG_NUM);
                    guideSegmentIds.Add(target_adr_section.SEG_NUM);
                }


                var guideSectionInfos = RouteInfo.GetSectionInfos(scApp.SegmentBLL, guideSegmentIds.ToList(), guideAddressIds);
                return (true, guideSegmentIds.ToList(), guideSectionInfos.guideSections, guideSectionInfos.guideAddresses, 0);
            }
            else
            {
                int.TryParse(start_node, out int i_start_address);
                int.TryParse(target_node, out int i_target_address);

                List<RouteInfo> stratFromRouteInfoList = scApp.NewRouteGuide.getFromToRoutesAddrToAddr(i_start_address, i_target_address);
                if (stratFromRouteInfoList == null || stratFromRouteInfoList.Count == 0)
                {
                    int[] current_ban_route = scApp.NewRouteGuide.getAllBanDirectArray();
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(GuideBLL), Device: "OHxC",
                       Data: $"start node:{start_node}({i_start_address}) and target node:{target_node}({i_target_address})," +
                             $"can't find the path. Current ban route:{string.Join(",", current_ban_route)}");
                    return (false, null, null, null, int.MaxValue);
                }
                RouteInfo min_stratFromRouteInfo = stratFromRouteInfoList.First();
                List<string> guideSegmentIds = min_stratFromRouteInfo.GetSectionIDs();
                List<string> guideAddressIds = min_stratFromRouteInfo.GetAddressesIDs();
                if (!is_node_adr_start)
                {
                    ASECTION start_adr_section = null;
                    //if (!start_adr.IsSection)
                    if (start_adr.IsControl)
                    {
                        start_adr_section = scApp.SectionBLL.cache.GetSection(start_adr.SEC_ID);
                    }
                    else
                    {
                        start_adr_section = scApp.SectionBLL.cache.GetSectionsByToAddress(start_adr.ADR_ID).FirstOrDefault();
                        if (start_adr_section == null)
                            start_adr_section = scApp.SectionBLL.cache.GetSectionsByFromAddress(start_adr.ADR_ID).FirstOrDefault();
                    }

                    if (SCUtility.isMatche(start_adr_section.SEG_NUM, guideSegmentIds.First()))
                    {
                        //if (!start_adr.IsSection)

                        if (start_adr.IsControl)
                        {
                            ////Not thing
                            ASEGMENT start_adr_segment = scApp.SegmentBLL.cache.GetSegment(start_adr_section.SEG_NUM);
                            if (SCUtility.isMatche(guideAddressIds[0], start_adr_segment.RealFromAddress))
                            {
                                guideAddressIds[0] = start_adr_section.FROM_ADR_ID;
                            }
                            else
                            {
                                guideAddressIds[0] = start_adr_section.TO_ADR_ID;
                            }

                            //guideAddressIds[0] = start_adr.NODE_ID.Trim();
                        }
                        else
                        {
                            guideAddressIds[0] = start_adr.ADR_ID.Trim();
                        }
                    }
                    else
                    {
                        guideSegmentIds.Insert(0, start_adr_section.SEG_NUM.Trim());
                        //   if (!start_adr.IsSection)
                        if (start_adr.IsControl)
                        {
                            ASEGMENT start_adr_segment = scApp.SegmentBLL.cache.GetSegment(start_adr_section.SEG_NUM);
                            if (SCUtility.isMatche(guideAddressIds[0], start_adr_segment.RealFromAddress))
                            {
                                guideAddressIds.Insert(0, start_adr_section.TO_ADR_ID.Trim());
                            }
                            else
                            {
                                guideAddressIds.Insert(0, start_adr_section.FROM_ADR_ID.Trim());
                            }
                        }
                        else
                        {
                            guideAddressIds.Insert(0, start_adr.ADR_ID.Trim());
                        }
                    }
                }
                if (!is_node_adr_target)
                {
                    ASECTION target_adr_section = null;
                    //if (!target_adr.IsSection)
                    if (target_adr.IsControl)
                    {
                        target_adr_section = scApp.SectionBLL.cache.GetSection(target_adr.SEC_ID);
                    }
                    else
                    {
                        target_adr_section = scApp.SectionBLL.cache.GetSectionsByToAddress(target_adr.ADR_ID).FirstOrDefault();
                        if (target_adr_section == null)
                            target_adr_section = scApp.SectionBLL.cache.GetSectionsByFromAddress(target_adr.ADR_ID).FirstOrDefault();
                    }

                    if (SCUtility.isMatche(target_adr_section.SEG_NUM, guideSegmentIds.Last()))
                    {
                        //guideAddressIds[guideAddressIds.Count - 1] = target_adr.ADR_ID.Trim();
                        if (target_adr.IsControl)
                        {
                            ASEGMENT target_adr_segment = scApp.SegmentBLL.cache.GetSegment(target_adr_section.SEG_NUM);
                            if (SCUtility.isMatche(guideAddressIds.Last(), target_adr_segment.RealFromAddress))
                            {
                                guideAddressIds[guideAddressIds.Count - 1] = target_adr_section.FROM_ADR_ID.Trim();
                                //guideAddressIds[0] = target_adr_section.FROM_ADR_ID;
                            }
                            else
                            {
                                guideAddressIds[guideAddressIds.Count - 1] = target_adr_section.TO_ADR_ID.Trim();
                            }

                            //guideAddressIds[guideAddressIds.Count - 1] = target_adr.NODE_ID.Trim();
                        }
                        else
                        {
                            guideAddressIds[guideAddressIds.Count - 1] = target_adr.ADR_ID.Trim();
                        }
                    }
                    else
                    {
                        guideSegmentIds.Add(target_adr_section.SEG_NUM.Trim());
                        ASEGMENT target_adr_on_segment = scApp.SegmentBLL.cache.GetSegment(target_adr_section.SEG_NUM);
                        //if (!target_adr.IsSection)
                        if (target_adr.IsControl)
                        {

                            //if (SCUtility.isMatche(guideAddressIds.Last(), target_adr_section.FROM_ADR_ID))
                            //{
                            //    guideAddressIds.Add(target_adr_section.TO_ADR_ID.Trim());
                            //}
                            //else
                            //{
                            //    guideAddressIds.Add(target_adr_section.FROM_ADR_ID.Trim());
                            //}
                            if (SCUtility.isMatche(guideAddressIds.Last(), target_adr_on_segment.RealFromAddress))
                            {
                                guideAddressIds.Add(target_adr_section.TO_ADR_ID.Trim());
                            }
                            else
                            {
                                guideAddressIds.Add(target_adr_section.FROM_ADR_ID.Trim());
                            }
                        }
                        else
                        {
                            guideAddressIds.Add(target_adr.ADR_ID.Trim());
                        }
                    }
                }
                var guideSectionInfos = RouteInfo.GetSectionInfos(scApp.SegmentBLL, guideSegmentIds, guideAddressIds);

                return (true, guideSegmentIds, guideSectionInfos.guideSections, guideSectionInfos.guideAddresses, min_stratFromRouteInfo.total_cost);
            }
        }

        public (bool isSuccess, List<string> guideSegmentIds, List<string> guideSectionIds, List<string> guideAddressIds, int totalCost)
        getGuideInfo_New3(string startAddress, string targetAddress, List<string> byPassSectionIDs = null)
        {
            return getGuideInfo_New3("", startAddress, targetAddress, byPassSectionIDs);
        }
        public (bool isSuccess, List<string> guideSegmentIds, List<string> guideSectionIds, List<string> guideAddressIds, int totalCost)
        //getGuideInfo_New3(string startAddress, string targetAddress, List<string> byPassAddressIDs = null)
        getGuideInfo_New3(string startSection, string startAddress, string targetAddress, List<string> byPassSectionIDs = null)
        {
            AADDRESS start_adr = scApp.getCommObjCacheManager().getAddress(startAddress);
            AADDRESS target_adr = scApp.getCommObjCacheManager().getAddress(targetAddress);
            string start_node = startAddress;
            string target_node = targetAddress;
            //bool is_node_adr_start = start_adr.IsSegment;
            //bool is_node_adr_target = target_adr.IsSegment;
            bool is_node_adr_start = scApp.SegmentBLL.cache.IsSegmentAddress(startAddress);
            bool is_node_adr_target = scApp.SegmentBLL.cache.IsSegmentAddress(targetAddress);

            if (SCUtility.isMatche(startAddress, targetAddress))
            {
                return (true, new List<string>(), new List<string>(), new List<string>(), 0);
            }
            if (!is_node_adr_start)
            {
                string start_section_adr_id = null;
                ASECTION start_adr_section = null;

                //if (!start_adr.IsSection)
                if (start_adr.IsControl)
                {
                    var nearSectionAddress = scApp.AddressesBLL.cache.GetAddress(start_adr.NODE_ID);
                    if (nearSectionAddress == null)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(GuideBLL), Device: "OHxC",
                           Data: $"Start address:{startAddress} is a nonsection address,but node id:{start_adr.NODE_ID} not exist.");
                        return (false, null, null, null, int.MaxValue);
                    }
                    else
                    {
                        start_section_adr_id = nearSectionAddress.ADR_ID;
                    }
                    start_adr_section = scApp.SectionBLL.cache.GetSection(start_adr.SEC_ID);
                    if (start_adr_section == null)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(GuideBLL), Device: "OHxC",
                           Data: $"Start address:{startAddress} is a nonsection address,but section id:{start_adr.SEC_ID} not exist.");
                        return (false, null, null, null, int.MaxValue);
                    }
                }
                else
                {
                    start_section_adr_id = start_adr.ADR_ID;
                    start_adr_section = scApp.SectionBLL.cache.GetSectionsByFromAddress(start_section_adr_id).First();
                }
                ASEGMENT start_adr_segment = scApp.SegmentBLL.cache.GetSegment(start_adr_section.SEG_NUM);
                if (start_adr_segment.STATUS != E_SEG_STATUS.Active)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(GuideBLL), Device: "OHxC",
                       Data: $"Segment ID:{start_adr_segment.SEG_ID} is disable, can't find guide info");
                    return (false, null, null, null, int.MaxValue);
                }
                start_node = start_adr_segment.getNearSegmentAddress(start_section_adr_id);
            }
            if (!is_node_adr_target)
            {
                string target_section_adr_id = null;
                ASECTION target_adr_section = null;
                //if (!target_adr.IsSection)
                if (target_adr.IsControl)
                {
                    var nearSectionAddress = scApp.AddressesBLL.cache.GetAddress(target_adr.NODE_ID);
                    if (nearSectionAddress == null)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(GuideBLL), Device: "OHxC",
                           Data: $"Target address:{startAddress} is a nonsection address,but node id:{target_adr.NODE_ID} not exist.");
                        return (false, null, null, null, int.MaxValue);
                    }
                    else
                    {
                        target_section_adr_id = nearSectionAddress.ADR_ID;
                    }
                    target_adr_section = scApp.SectionBLL.cache.GetSection(target_adr.SEC_ID);
                    if (target_adr_section == null)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(GuideBLL), Device: "OHxC",
                           Data: $"Target address:{startAddress} is a nonsection address,but section id:{target_adr.SEC_ID} not exist.");
                        return (false, null, null, null, int.MaxValue);
                    }
                }
                else
                {
                    target_section_adr_id = target_adr.ADR_ID;
                    target_adr_section = scApp.SectionBLL.cache.GetSectionsByFromAddress(target_section_adr_id).First();
                }
                ASEGMENT target_adr_segment = scApp.SegmentBLL.cache.GetSegment(target_adr_section.SEG_NUM);
                if (target_adr_segment.STATUS != E_SEG_STATUS.Active)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(GuideBLL), Device: "OHxC",
                       Data: $"Segment ID:{target_adr_segment.SEG_ID} is disable, can't find guide info");
                    return (false, null, null, null, int.MaxValue);
                }
                target_node = target_adr_segment.getNearSegmentAddress(target_section_adr_id);
            }

            if (SCUtility.isMatche(start_node, target_node))
            {
                HashSet<string> guideSegmentIds = new HashSet<string>();
                List<string> guideSectionIds = new List<string>();
                List<string> guideAddressIds = new List<string>();

                ASECTION start_adr_section = null;
                ASECTION target_adr_section = null;
                if (start_adr.IsControl)
                {
                    start_adr_section = scApp.SectionBLL.cache.GetSection(start_adr.SEC_ID);
                }
                else
                {
                    if (!SCUtility.isEmpty(startSection))
                    {
                        start_adr_section = scApp.SectionBLL.cache.GetSection(startSection);
                    }
                    else
                    {
                        start_adr_section = scApp.SectionBLL.cache.GetSectionsByToAddress(start_adr.ADR_ID).FirstOrDefault();
                        if (start_adr_section == null)
                            start_adr_section = scApp.SectionBLL.cache.GetSectionsByFromAddress(start_adr.ADR_ID).FirstOrDefault();
                    }
                }
                if (target_adr.IsControl)
                {
                    target_adr_section = scApp.SectionBLL.cache.GetSection(target_adr.SEC_ID);
                }
                else
                {
                    target_adr_section = scApp.SectionBLL.cache.GetSectionsByToAddress(target_adr.ADR_ID).FirstOrDefault();
                    if (target_adr_section == null)
                        target_adr_section = scApp.SectionBLL.cache.GetSectionsByFromAddress(target_adr.ADR_ID).FirstOrDefault();
                }

                ASEGMENT start_adr_seg = scApp.SegmentBLL.cache.GetSegment(start_adr_section.SEG_NUM);
                ASEGMENT target_adr_seg = scApp.SegmentBLL.cache.GetSegment(target_adr_section.SEG_NUM);
                if (SCUtility.isMatche(start_adr_seg.SEG_ID, target_adr_seg.SEG_ID))
                {
                    if (SCUtility.isMatche(start_adr_section.SEC_ID, target_adr_section.SEC_ID))
                    {
                        if (SCUtility.isMatche(start_adr_section.FROM_ADR_ID, start_adr.ADR_ID))
                        {
                            guideAddressIds.Add(start_adr_section.FROM_ADR_ID);
                            guideAddressIds.Add(start_adr_section.TO_ADR_ID);
                        }
                        else if (SCUtility.isMatche(start_adr_section.TO_ADR_ID, start_adr.ADR_ID))
                        {

                            guideAddressIds.Add(start_adr_section.TO_ADR_ID);
                            guideAddressIds.Add(start_adr_section.FROM_ADR_ID);
                        }
                        else
                        {
                            if (start_adr.DISTANCE > target_adr.DISTANCE)
                            {
                                guideAddressIds.Add(start_adr_section.TO_ADR_ID);
                                guideAddressIds.Add(start_adr_section.FROM_ADR_ID);
                            }
                            else
                            {
                                guideAddressIds.Add(start_adr_section.FROM_ADR_ID);
                                guideAddressIds.Add(start_adr_section.TO_ADR_ID);
                            }
                        }
                        guideSegmentIds.Add(start_adr_section.SEG_NUM);
                    }
                    else
                    {
                        int start_sec_index = start_adr_seg.GetSectionIndex(start_adr_section);
                        int target_sec_index = start_adr_seg.GetSectionIndex(target_adr_section);
                        if (start_adr.IsControl)
                        {
                            if (start_sec_index < target_sec_index)
                            {
                                guideAddressIds.Add(start_adr_section.FROM_ADR_ID);
                            }
                            else
                            {
                                guideAddressIds.Add(start_adr_section.TO_ADR_ID);
                            }
                            //guideAddressIds.Add(start_adr.NODE_ID);
                        }
                        else
                        {
                            guideAddressIds.Add(start_adr.ADR_ID);
                        }
                        if (target_adr.IsControl)
                        {
                            if (target_sec_index < start_sec_index)
                            {
                                guideAddressIds.Add(target_adr_section.FROM_ADR_ID);
                            }
                            else
                            {
                                guideAddressIds.Add(target_adr_section.TO_ADR_ID);
                            }
                            //guideAddressIds.Add(target_adr.NODE_ID);
                        }
                        else
                        {
                            guideAddressIds.Add(target_adr.ADR_ID);
                        }
                        guideSegmentIds.Add(start_adr_section.SEG_NUM);
                        //int index_start_adr_section = start_adr_seg.Sections.IndexOf(start_adr_section);
                        //int index_target_adr_section = start_adr_seg.Sections.IndexOf(target_adr_section);

                        //if (index_start_adr_section < index_target_adr_section)
                        //{
                        //    guideAddressIds.Add(start_adr_section.FROM_ADR_ID);
                        //    guideAddressIds.Add(target_adr_section.TO_ADR_ID);
                        //}
                        //else
                        //{
                        //    guideAddressIds.Add(start_adr_section.TO_ADR_ID);
                        //    guideAddressIds.Add(target_adr_section.FROM_ADR_ID);
                        //}
                        //guideSegmentIds.Add(start_adr_section.SEG_NUM);
                    }
                }
                else
                {

                    if (SCUtility.isMatche(start_adr_seg.RealFromAddress, target_adr_seg.RealFromAddress))
                    {
                        guideAddressIds.Add(start_adr_section.TO_ADR_ID);
                        guideAddressIds.Add(start_adr_seg.RealFromAddress);
                        guideAddressIds.Add(target_adr_section.TO_ADR_ID);
                    }
                    else if (SCUtility.isMatche(start_adr_seg.RealToAddress, target_adr_seg.RealToAddress))
                    {
                        guideAddressIds.Add(start_adr_section.FROM_ADR_ID);
                        guideAddressIds.Add(start_adr_seg.RealToAddress);
                        guideAddressIds.Add(target_adr_section.FROM_ADR_ID);
                    }
                    else if (SCUtility.isMatche(start_adr_seg.RealToAddress, target_adr_seg.RealFromAddress))
                    {
                        guideAddressIds.Add(start_adr_section.FROM_ADR_ID);
                        guideAddressIds.Add(start_adr_seg.RealToAddress);
                        guideAddressIds.Add(target_adr_section.TO_ADR_ID);

                    }
                    else
                    {
                        guideAddressIds.Add(start_adr_section.TO_ADR_ID);
                        guideAddressIds.Add(start_adr_seg.RealFromAddress);
                        guideAddressIds.Add(target_adr_section.FROM_ADR_ID);
                    }
                    guideSegmentIds.Add(start_adr_section.SEG_NUM);
                    guideSegmentIds.Add(target_adr_section.SEG_NUM);
                }
                var guideSectionInfos = RouteInfo.GetSectionInfos(scApp.SegmentBLL, guideSegmentIds.ToList(), guideAddressIds);

                bool is_success = false;
                if (byPassSectionIDs == null || byPassSectionIDs.Count == 0)
                {
                    is_success = true;
                }
                else
                {
                    List<string> guideSections = guideSectionInfos.guideSections;
                    if (guideSections.Count > 1)
                    {
                        if (!byPassSectionIDs.Contains(guideSections[0]) &&
                           !byPassSectionIDs.Contains(guideSections[1]))
                        {
                            is_success = true;
                        }
                    }
                    else if (guideSections.Count > 0)
                    {
                        if (!byPassSectionIDs.Contains(guideSections[0]))
                        {
                            is_success = true;
                        }
                    }
                }

                //如果在找路徑時，發現組出來的路徑還是要By Pass的話，就從另外一邊找
                if (is_success)
                {
                    return (true, guideSegmentIds.ToList(), guideSectionInfos.guideSections, guideSectionInfos.guideAddresses, 0);
                }
                else
                {
                    //取得尋找的StartSection，並得到所在的Segment
                    //找到Segment後改從Segment另一端去找路徑
                    string the_opposite_node_address = SCUtility.isMatche(start_adr_seg.FROM_ADR_ID, start_node) ?
                        start_adr_seg.TO_ADR_ID : start_adr_seg.FROM_ADR_ID;
                    return getGuideInfo_New3(startSection, the_opposite_node_address, targetAddress, byPassSectionIDs);
                }
            }
            else
            {
                int.TryParse(start_node, out int i_start_address);
                int.TryParse(target_node, out int i_target_address);

                bool is_success = false;
                List<RouteInfo> stratFromRouteInfoList = scApp.NewRouteGuide.getFromToRoutesAddrToAddr(i_start_address, i_target_address);
                //RouteInfo min_stratFromRouteInfo = stratFromRouteInfoList.First();
                List<string> guideSegmentIds = null;
                List<string> guideSections = null;
                List<string> guideAddressIds = null;
                int cost = 0;

                foreach (var route_info in stratFromRouteInfoList)
                {
                    guideSegmentIds = route_info.GetSectionIDs();
                    guideSections = null;
                    guideAddressIds = route_info.GetAddressesIDs();
                    cost = route_info.total_cost;
                    if (!is_node_adr_start)
                    {
                        ASECTION start_adr_section = null;
                        //if (!start_adr.IsSection)
                        if (start_adr.IsControl)
                        {
                            start_adr_section = scApp.SectionBLL.cache.GetSection(start_adr.SEC_ID);
                        }
                        else
                        {
                            start_adr_section = scApp.SectionBLL.cache.GetSectionsByToAddress(start_adr.ADR_ID).FirstOrDefault();
                            if (start_adr_section == null)
                                start_adr_section = scApp.SectionBLL.cache.GetSectionsByFromAddress(start_adr.ADR_ID).FirstOrDefault();
                        }

                        if (SCUtility.isMatche(start_adr_section.SEG_NUM, guideSegmentIds.First()))
                        {
                            //if (!start_adr.IsSection)

                            if (start_adr.IsControl)
                            {
                                ////Not thing
                                ASEGMENT start_adr_segment = scApp.SegmentBLL.cache.GetSegment(start_adr_section.SEG_NUM);
                                if (SCUtility.isMatche(guideAddressIds[0], start_adr_segment.RealFromAddress))
                                {
                                    guideAddressIds[0] = start_adr_section.FROM_ADR_ID;
                                }
                                else
                                {
                                    guideAddressIds[0] = start_adr_section.TO_ADR_ID;
                                }

                                //guideAddressIds[0] = start_adr.NODE_ID.Trim();
                            }
                            else
                            {
                                guideAddressIds[0] = start_adr.ADR_ID.Trim();
                            }
                        }
                        else
                        {
                            guideSegmentIds.Insert(0, start_adr_section.SEG_NUM.Trim());
                            //   if (!start_adr.IsSection)
                            if (start_adr.IsControl)
                            {
                                ASEGMENT start_adr_segment = scApp.SegmentBLL.cache.GetSegment(start_adr_section.SEG_NUM);
                                if (SCUtility.isMatche(guideAddressIds[0], start_adr_segment.RealFromAddress))
                                {
                                    guideAddressIds.Insert(0, start_adr_section.TO_ADR_ID.Trim());
                                }
                                else
                                {
                                    guideAddressIds.Insert(0, start_adr_section.FROM_ADR_ID.Trim());
                                }
                            }
                            else
                            {
                                guideAddressIds.Insert(0, start_adr.ADR_ID.Trim());
                            }
                        }
                    }
                    if (!is_node_adr_target)
                    {
                        ASECTION target_adr_section = null;
                        //if (!target_adr.IsSection)
                        if (target_adr.IsControl)
                        {
                            target_adr_section = scApp.SectionBLL.cache.GetSection(target_adr.SEC_ID);
                        }
                        else
                        {
                            target_adr_section = scApp.SectionBLL.cache.GetSectionsByToAddress(target_adr.ADR_ID).FirstOrDefault();
                            if (target_adr_section == null)
                                target_adr_section = scApp.SectionBLL.cache.GetSectionsByFromAddress(target_adr.ADR_ID).FirstOrDefault();
                        }

                        if (SCUtility.isMatche(target_adr_section.SEG_NUM, guideSegmentIds.Last()))
                        {
                            //guideAddressIds[guideAddressIds.Count - 1] = target_adr.ADR_ID.Trim();
                            if (target_adr.IsControl)
                            {
                                ASEGMENT target_adr_segment = scApp.SegmentBLL.cache.GetSegment(target_adr_section.SEG_NUM);
                                if (SCUtility.isMatche(guideAddressIds.Last(), target_adr_segment.RealFromAddress))
                                {
                                    guideAddressIds[guideAddressIds.Count - 1] = target_adr_section.FROM_ADR_ID.Trim();
                                    //guideAddressIds[0] = target_adr_section.FROM_ADR_ID;
                                }
                                else
                                {
                                    guideAddressIds[guideAddressIds.Count - 1] = target_adr_section.TO_ADR_ID.Trim();
                                }

                                //guideAddressIds[guideAddressIds.Count - 1] = target_adr.NODE_ID.Trim();
                            }
                            else
                            {
                                guideAddressIds[guideAddressIds.Count - 1] = target_adr.ADR_ID.Trim();
                            }
                        }
                        else
                        {
                            guideSegmentIds.Add(target_adr_section.SEG_NUM.Trim());
                            ASEGMENT target_adr_on_segment = scApp.SegmentBLL.cache.GetSegment(target_adr_section.SEG_NUM);
                            if (target_adr.IsControl)
                            {
                                if (SCUtility.isMatche(guideAddressIds.Last(), target_adr_on_segment.RealFromAddress))
                                {
                                    guideAddressIds.Add(target_adr_section.TO_ADR_ID.Trim());
                                }
                                else
                                {
                                    guideAddressIds.Add(target_adr_section.FROM_ADR_ID.Trim());
                                }
                            }
                            else
                            {
                                guideAddressIds.Add(target_adr.ADR_ID.Trim());
                            }
                        }
                    }
                    var guideSectionInfos = RouteInfo.GetSectionInfos(scApp.SegmentBLL, guideSegmentIds, guideAddressIds);
                    guideSections = guideSectionInfos.guideSections;
                    guideAddressIds = guideSectionInfos.guideAddresses;
                    if (byPassSectionIDs == null || byPassSectionIDs.Count == 0)
                    {
                        is_success = true;
                        break;
                    }
                    else
                    {
                        //if (guideAddressIds.Intersect(byPassSectionIDs).Count() == 0)
                        //if (guideSections.Intersect(byPassSectionIDs).Count() == 0)
                        //{
                        //    is_success = true;
                        //    break;
                        //}
                        //===========================================
                        if (guideSections.Count > 1)
                        {
                            if (!byPassSectionIDs.Contains(guideSections[0]) &&
                               !byPassSectionIDs.Contains(guideSections[1]))
                            {
                                is_success = true;
                                break;
                            }
                        }
                        else if (guideSections.Count > 0)
                        {
                            if (!byPassSectionIDs.Contains(guideSections[0]))
                            {
                                is_success = true;
                                break;
                            }
                        }

                        //string guide_section = string.Join(",", guideSections);
                        //string by_pass_section = string.Join(",", byPassSectionIDs);
                        //if (!guide_section.Contains(by_pass_section))
                        //{
                        //    is_success = true;
                        //    break;
                        //}

                    }
                }
                return (is_success, guideSegmentIds, guideSections, guideAddressIds, cost);
            }
        }

        private ConcurrentDictionary<string, GuideQuickSearch> dicGuideQuickSearch =
                 new ConcurrentDictionary<string, GuideQuickSearch>();
        public bool IsRoadWalkable(string startAddress, string targetAddress)
        {
            return IsRoadWalkable(startAddress, targetAddress, out int total_cost);
        }
        public bool IsRoadWalkable(string startAddress, string targetAddress, out int totalCost)
        {
            try
            {
                if (SCUtility.isMatche(startAddress, targetAddress))
                {
                    totalCost = 0;
                    return true;
                }
                string key = $"{SCUtility.Trim(startAddress, true)},{SCUtility.Trim(targetAddress, true)}";

                bool is_exist = dicGuideQuickSearch.TryGetValue(key, out GuideQuickSearch guideQuickSearch);
                if (is_exist)
                {
                    totalCost = guideQuickSearch.totalCost;
                    DebugParameter.GuideQuickSearchTimes++;
                    return guideQuickSearch.isWalker;
                }
                else
                {
                    var guide_info = getGuideInfo(startAddress, targetAddress);
                    dicGuideQuickSearch.TryAdd(key, new GuideQuickSearch(guide_info.isSuccess, guide_info.totalCost));
                    //if ((guide_info.guideAddressIds != null && guide_info.guideAddressIds.Count != 0) &&
                    //    ((guide_info.guideSectionIds != null && guide_info.guideSectionIds.Count != 0)))
                    DebugParameter.GuideSearchTimes++;
                    if (guide_info.isSuccess)
                    {
                        totalCost = guide_info.totalCost;
                        return true;
                    }
                    else
                    {
                        totalCost = int.MaxValue;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                totalCost = int.MaxValue;
                return false;
            }
        }
        public void clearAllDirGuideQuickSearchInfo()
        {
            DebugParameter.GuideQuickSearchTimes=0;
            DebugParameter.GuideSearchTimes=0;

            dicGuideQuickSearch.Clear();
        }

        public class GuideQuickSearch
        {
            public bool isWalker;
            public int totalCost;
            public GuideQuickSearch(bool _isWalker, int _totalCost)
            {
                isWalker = _isWalker;
                totalCost = _totalCost;
            }

        }

        public bool IsBanRoute(List<ReserveInfo> reserveInfos)
        {
            int[] all_ban_DirectArray = scApp.NewRouteGuide.getAllBanDirectArray();
            foreach (var reserve_info in reserveInfos)
            {
                ASECTION sec_reserve = scApp.SectionBLL.cache.GetSection(reserve_info.ReserveSectionID);
                int from_adr_id = 0;
                int to_adr_id = 0;
                switch (reserve_info.DriveDirction)
                {
                    case DriveDirction.DriveDirForward:
                        int.TryParse(sec_reserve.REAL_FROM_ADR_ID, out from_adr_id);
                        int.TryParse(sec_reserve.REAL_TO_ADR_ID, out to_adr_id);
                        break;
                    case DriveDirction.DriveDirReverse:
                        int.TryParse(sec_reserve.REAL_TO_ADR_ID, out from_adr_id);
                        int.TryParse(sec_reserve.REAL_FROM_ADR_ID, out to_adr_id);
                        break;
                }
                for (int i = 0; i < all_ban_DirectArray.Length - 1; i++)
                {
                    int adr1 = all_ban_DirectArray[i];
                    int adr2 = all_ban_DirectArray[i + 1];
                    if (from_adr_id == adr1 && from_adr_id == adr2)
                        return true;
                }
            }
            return false;
        }

        public ASEGMENT unbanRouteTwoDirect(string segmentID)
        {
            ASEGMENT segment_do = null;
            ASEGMENT segment_vo = scApp.SegmentBLL.cache.GetSegment(segmentID);
            if (segment_vo != null)
            {
                foreach (var sec in segment_vo.Sections)
                    scApp.NewRouteGuide.unbanRouteTwoDirect(sec.SEC_ID);
            }
            segment_do = scApp.MapBLL.EnableSegment(segmentID);
            return segment_do;
        }
        public ASEGMENT banRouteTwoDirect(string segmentID)
        {
            ASEGMENT segment = null;
            ASEGMENT segment_vo = scApp.SegmentBLL.cache.GetSegment(segmentID);
            if (segment_vo != null)
            {
                foreach (var sec in segment_vo.Sections)
                    scApp.NewRouteGuide.banRouteTwoDirect(sec.SEC_ID);
            }
            segment = scApp.MapBLL.DisableSegment(segmentID);
            return segment;
        }




        public (List<string> replaceSegmentIds, List<string> replaceSectionIds, List<string> replaceAddressIds) getReplaceRoad(string start_node, string target_node, string willReplaceSegment)
        {
            int.TryParse(start_node, out int i_start_address);
            int.TryParse(target_node, out int i_target_address);
            List<RouteInfo> stratFromRouteInfoList = scApp.NewRouteGuide.getFromToRoutesAddrToAddr(i_start_address, i_target_address);
            foreach (var route_info in stratFromRouteInfoList)
            {
                List<string> segment_ids = route_info.GetSectionIDs();
                if (!segment_ids.Contains(willReplaceSegment.Trim()))
                {
                    List<string> seg_address_ids = route_info.GetAddressesIDs();
                    var guideSectionInfos = RouteInfo.GetSectionInfos(scApp.SegmentBLL, segment_ids, seg_address_ids);
                    return (segment_ids, guideSectionInfos.guideSections, guideSectionInfos.guideAddresses);

                }
            }
            return (null, null, null);
        }

    }

}

