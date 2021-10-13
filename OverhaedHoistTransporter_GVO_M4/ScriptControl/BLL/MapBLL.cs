//*********************************************************************************
//(c) Copyright 2017, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------

//**********************************************************************************

using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.Data.DAO;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class MapBLL
    {
        private SCApplication scApp = null;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        RAILDao railDAO = null;
        ADDRESSDao adrDAO = null;
        PortDao portDAO = null;
        POINTDao pointDAO = null;
        PortIconDao portIconDAO = null;
        GROUPRAILSDao groupRailDAO = null;
        SectionDao sectionDAO = null;
        SegmentDao segmentDAO = null;
        VehicleDao vehicleDAO = null;
        BlockZoneMasterDao blockZoneMasterDao = null;
        BlockZoneDetailDao blockZoneDetaiDao = null;
        BlockZoneQueueDao blockZoneQueueDao = null;

        CommObjCacheManager commObjCacheManager = null;

        public MapBLL()
        {

        }
        public void start(SCApplication app)
        {
            scApp = app;
            railDAO = scApp.RailDao;
            adrDAO = scApp.AddressDao;
            portDAO = scApp.PortDao;
            portIconDAO = scApp.PortIconDao;
            pointDAO = scApp.PointDao;
            groupRailDAO = scApp.GroupRailDao;
            sectionDAO = scApp.SectionDao;
            segmentDAO = scApp.SegmentDao;
            vehicleDAO = scApp.VehicleDao;
            blockZoneMasterDao = scApp.BlockZoneMasterDao;
            blockZoneDetaiDao = scApp.BolckZoneDetaiDao;
            blockZoneQueueDao = scApp.BlockZoneQueueDao;
            commObjCacheManager = scApp.getCommObjCacheManager();
        }
        #region Rail
        public List<ARAIL> loadAllRail()
        {
            List<ARAIL> Rails = null;
            using (DBConnection_EF con = new DBConnection_EF())
            {
                Rails = railDAO.loadAll(con);
            }
            return Rails;
        }
        #endregion Rail
        #region Point
        public List<APOINT> loadAllPoint()
        {
            List<APOINT> points = null;
            using (DBConnection_EF con = new DBConnection_EF())
            {
                points = pointDAO.loadAll(con);
            }
            return points;
        }

        public APOINT getPointByID(string point_id)
        {
            APOINT point = null;
            using (DBConnection_EF con = new DBConnection_EF())
            {
                point = pointDAO.getByID(con, point_id);
            }
            return point;
        }
        #endregion Point
        #region GROUPRAILS
        public AGROUPRAILS getGroupRailsBySectionID(string sec_id)
        {
            AGROUPRAILS groupRails = null;
            using (DBConnection_EF con = new DBConnection_EF())
            {
                groupRails = groupRailDAO.getByID(con, sec_id);
            }
            return groupRails;
        }
        public List<AGROUPRAILS> loadAllGroupRail()
        {
            List<AGROUPRAILS> lstGroupRail = null;
            using (DBConnection_EF con = new DBConnection_EF())
            {
                lstGroupRail = groupRailDAO.loadAll(con);
            }
            return lstGroupRail;
        }
        public List<string> loadAllSectionID()
        {
            List<string> sec_ids = null;
            using (DBConnection_EF con = new DBConnection_EF())
            {
                sec_ids = groupRailDAO.loadAllSectionID(con);
            }
            return sec_ids;
        }
        public void getFirstAndLastRailBySecID(string sec_id, out AGROUPRAILS first_rail, out AGROUPRAILS last_rail)
        {
            using (DBConnection_EF con = new DBConnection_EF())
            {
                groupRailDAO.getFirstAndLastRailBySecID(con, sec_id, out first_rail, out last_rail);
            }
        }


        #endregion GROUPRAILS

        #region Address
        public List<AADDRESS> loadAllAddress()
        {
            List<AADDRESS> adrs = null;
            using (DBConnection_EF con = new DBConnection_EF())
            {
                adrs = adrDAO.loadAll(con);
            }
            return adrs;
        }
        public AADDRESS getAddressByID(string adr_id)
        {
            AADDRESS adr = null;
            //DBConnection_EF con = DBConnection_EF.GetContext();
            using (DBConnection_EF con = new DBConnection_EF())
            {
                adr = adrDAO.getByID(con, adr_id);
            }
            return adr;
        }
        public AADDRESS getAddressByPortID(string port_id)
        {
            AADDRESS adr = null;
            using (DBConnection_EF con = new DBConnection_EF())
            {
                adr = adrDAO.getByPortID(con, port_id);
            }
            return adr;
        }
        public int getCount_AddressCount()
        {
            int count = 0;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                count = adrDAO.getAdrCount(con);
            }
            return count;
        }
        #endregion Address
        #region Section
        Dictionary<string, List<ASECTION>> dicNextSection = new Dictionary<string, List<ASECTION>>();

        public ASEGMENT getSegmentBySectionID(string id)
        {
            ASEGMENT segment = null;
            using (DBConnection_EF con = new DBConnection_EF())
            {
                //ASECTION section = sectionDAO.getByID(con, id);
                ASECTION section = sectionDAO.getByID(commObjCacheManager, id);
                if (section != null)
                    segment = segmentDAO.getByID(con, section.SEG_NUM);
            }

            return segment;
        }

        public bool updateSegStatus(string id, E_SEG_STATUS status)
        {
            ASEGMENT segment = null;
            try
            {
                using (DBConnection_EF con = new DBConnection_EF())
                {
                    segment = segmentDAO.getByID(con, id);
                    segment.STATUS = status;
                    //bool isDetached = con.Entry(segment).State == EntityState.Modified;
                    //if (isDetached)
                    segmentDAO.Update(con, segment);
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return false;
            }
        }

        public bool updateSecDistance(string from_id, string to_id, double distance, out ASECTION section)
        {
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    section = sectionDAO.getByFromToAdr(con, from_id, to_id);
                    section.SEC_DIS = distance;
                    section.LAST_TECH_TIME = DateTime.Now;
                    sectionDAO.update(con, section);
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                section = null;
                return false;
            }
        }
        public bool updateSecDistance(string id, double distance)
        {
            ASECTION section = null;
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    section = sectionDAO.getByID(con, id);
                    section.SEC_DIS = distance;
                    //bool isDetached = con.Entry(section).State == EntityState.Modified;
                    //if (isDetached)
                    sectionDAO.update(con, section);
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return false;
            }
        }
        public bool resetSecTechingTime(string id)
        {
            ASECTION section = null;
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    section = sectionDAO.getByID(con, id);
                    section.LAST_TECH_TIME = null;
                    //bool isDetached = con.Entry(section).State == EntityState.Modified;
                    //if (isDetached)
                    sectionDAO.update(con, section);
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return false;
            }
        }




        public List<ASECTION> loadSectionBySecIDs(List<string> section_ids)
        {
            if (section_ids == null || section_ids.Count == 0)
            {
                return null;
            }
            List<ASECTION> sections = null;
            //using (DBConnection_EF con = new DBConnection_EF())
            //{
            //    sections = sectionDAO.loadSecBySecIds(con, section_ids);
            //}
            sections = sectionDAO.loadSecBySecIds(commObjCacheManager, section_ids);

            return sections;
        }

        public List<ASECTION> loadSectionsByFromOrToAdr(string adr)
        {
            if (SCUtility.isEmpty(adr))
            {
                return null;
            }
            List<ASECTION> sections = null;
            sections = sectionDAO.loadByFromOrToAdr(commObjCacheManager, adr);

            return sections;
        }



        public ASECTION getSectiontByID(string section_id)
        {
            ASECTION section = null;
            //using (DBConnection_EF con = new DBConnection_EF())
            //{
            //    section = sectionDAO.getByID(con, section_id);
            //}
            section = sectionDAO.getByID(commObjCacheManager, section_id);

            return section;
        }
        public List<ASECTION> loadAllSection()
        {
            List<ASECTION> sections = null;
            using (DBConnection_EF con = new DBConnection_EF())
            {
                sections = sectionDAO.loadAll(con);
            }
            return sections;
        }

        public List<ASECTION> loadSectionByFromAdrs(List<string> from_adrs)
        {
            List<ASECTION> sections = null;
            sections = sectionDAO.loadByFromAdrs(commObjCacheManager, from_adrs);
            return sections;
        }
        public List<ASECTION> loadSectionByToAdrs(List<string> to_adrs)
        {
            List<ASECTION> sections = null;
            //using (DBConnection_EF con = new DBConnection_EF())
            //{
            //    sections = sectionDAO.loadByToAdrs(con, to_adrs);
            //}
            sections = sectionDAO.loadByToAdrs(commObjCacheManager, to_adrs);
            return sections;
        }

        public Dictionary<string, int> loadGroupBySecAndThroughTimes()
        {
            Dictionary<string, int> secAndThroughTimes = null;
            using (DBConnection_EF con = new DBConnection_EF())
            {
                secAndThroughTimes = sectionDAO.loadGroupBySecIDAndThroughTimes(con);
            }
            return secAndThroughTimes;
        }
        public string getFirstSecIDBySegmentID(string seg_id)
        {
            ASECTION sec = sectionDAO.getFirstSecBySegmentID(commObjCacheManager, seg_id);
            return sec == null ? string.Empty : sec.SEC_ID.Trim();
        }
        public List<ASECTION> loadSectionsBySegmentID(string seg_num)
        {
            List<ASECTION> secs = sectionDAO.loadSectionsBySegmentID(commObjCacheManager, seg_num);
            return secs;
        }


        #endregion Section
        #region Segment
        public ASEGMENT getSegmentByID(string segment_id)
        {
            ASEGMENT segment = null;
            using (DBConnection_EF con = new DBConnection_EF())
            {
                segment = segmentDAO.getByID(con, segment_id);
            }
            return segment;
        }
        public List<ASEGMENT> loadAllSegments()
        {
            List<ASEGMENT> lstSeg = null;
            using (DBConnection_EF con = new DBConnection_EF())
            {
                lstSeg = segmentDAO.loadAllSegments(con);
            }
            return lstSeg;
        }
        public List<string> loadAllSegmentIDs()
        {
            List<string> lstSeg = null;
            using (DBConnection_EF con = new DBConnection_EF())
            {
                lstSeg = segmentDAO.loadAllSegmentIDs(con);
            }
            return lstSeg;
        }
        public ASEGMENT PreDisableSegment(string seg_num)
        {
            ASEGMENT seg = null;
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    seg = segmentDAO.getByID(con, seg_num);
                    if (seg != null)
                    {
                        seg.PRE_DISABLE_FLAG = true;
                        seg.PRE_DISABLE_TIME = DateTime.Now;
                        seg.DISABLE_TIME = null;
                    }
                    segmentDAO.Update(con, seg);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            return seg;
        }
        public ASEGMENT DisableSegment(string seg_num)
        {
            ASEGMENT seg = null;
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    seg = segmentDAO.getByID(con, seg_num);
                    if (seg != null)
                    {
                        seg.PRE_DISABLE_FLAG = false;
                        seg.PRE_DISABLE_TIME = null;
                        seg.DISABLE_TIME = DateTime.Now;
                        seg.STATUS = E_SEG_STATUS.Closed;
                        segmentDAO.Update(con, seg);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            return seg;
        }
        public ASEGMENT EnableSegment(string seg_num)
        {
            ASEGMENT seg = null;
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    seg = segmentDAO.getByID(con, seg_num);
                    if (seg != null)
                    {
                        seg.PRE_DISABLE_FLAG = false;
                        seg.PRE_DISABLE_TIME = null;
                        seg.DISABLE_TIME = null;
                        seg.STATUS = E_SEG_STATUS.Active;
                    }
                    segmentDAO.Update(con, seg);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            return seg;
        }
        public List<ASEGMENT> loadPreDisableSegment()
        {
            List<ASEGMENT> preDisableSegments = null;
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    preDisableSegments = segmentDAO.loadPreDisableSegment(con);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            return preDisableSegments;
        }

        public bool IsSegmentActive(string seg_num)
        {
            bool isActive = false;
            try
            {
                ASEGMENT seg = null;
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    seg = segmentDAO.getByID(con, seg_num);
                }
                if (seg != null)
                {
                    isActive = !seg.PRE_DISABLE_FLAG && seg.STATUS == E_SEG_STATUS.Active;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            return isActive;
        }

        public List<string> loadNonActiveSegmentNum()
        {
            List<string> non_active_seg_nums = null;
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    non_active_seg_nums = segmentDAO.loadAllNonActiveSegmentNums(con);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            return non_active_seg_nums;
        }

        public bool CheckSegmentInActiveByPortID(string port_id)
        {
            bool SegmentInActive = true;
            APORTSTATION aPORTSTATION = scApp.MapBLL.getPortByPortID(port_id);
            ASECTION aSECTION = scApp.SectionDao.loadByFromOrToAdr(commObjCacheManager, aPORTSTATION.ADR_ID.Trim()).First();
            SegmentInActive = scApp.MapBLL.IsSegmentActive(aSECTION.SEG_NUM);
            return SegmentInActive;
        }

        #endregion
        #region Port
        public APORTSTATION getPortByPortID(string port_id)
        {
            APORTSTATION portTemp = null;
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                portTemp = portDAO.getByID(con, port_id);
            }
            return portTemp;
        }
        public APORTSTATION getPortByAdrID(string adr_id)
        {
            APORTSTATION portTemp = null;
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                portTemp = portDAO.getByAdrID(con, adr_id);
            }
            return portTemp;
        }
        public List<APORTSTATION> loadAllPort()
        {
            List<APORTSTATION> portTemp = null;
            using (DBConnection_EF con = new DBConnection_EF())
            {
                portTemp = portDAO.loadAll(con);
            }
            return portTemp;
        }
        public List<APORTSTATION> loadAllPortBySegmentID(string segment_id)
        {
            List<APORTSTATION> port_stations = null;
            List<ASECTION> sections = loadSectionsBySegmentID(segment_id);
            List<string> adrs_from = sections.Select(sec => sec.FROM_ADR_ID.Trim()).ToList();
            List<string> adrs_to = sections.Select(sec => sec.TO_ADR_ID.Trim()).ToList();
            List<string> adrs = adrs_from.Concat(adrs_to).Distinct().ToList();

            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                port_stations = portDAO.loadPortStationByAdrs(con, adrs);
            }
            return port_stations;
        }




        public bool getAddressID(string adr_port_id, out string adr)
        {
            E_VH_TYPE vh_type = E_VH_TYPE.None;
            return getAddressID(adr_port_id, out adr, out vh_type);
        }
        public bool getAddressID(string adr_port_id, out string adr, out E_VH_TYPE vh_type)
        {
            APORTSTATION port = scApp.MapBLL.getPortByPortID(adr_port_id);
            if (port != null)
            {
                adr = port.ADR_ID.Trim();
                //vh_type = port.ULD_VH_TYPE;
                vh_type = E_VH_TYPE.None;
                return true;
            }
            else
            {
                adr = adr_port_id;
                vh_type = E_VH_TYPE.None;
                return false;
            }
        }
        public bool getPortID(string adr_id, out string portid)
        {
            APORTSTATION port = scApp.MapBLL.getPortByAdrID(adr_id);
            if (port != null)
            {
                portid = port.PORT_ID.Trim();
                return true;
            }
            else
            {
                portid = adr_id;
                return false;
            }
        }



        #endregion Port
        #region PortIcon
        public List<APORTICON> loadAllPortIcon()
        {
            List<APORTICON> portIcons = null;
            using (DBConnection_EF con = new DBConnection_EF())
            {
                portIcons = portIconDAO.loadAll(con);
            }
            return portIcons;
        }
        #endregion


    }
}
