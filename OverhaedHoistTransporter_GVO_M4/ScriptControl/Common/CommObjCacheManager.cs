//*********************************************************************************
//      EQObjCacheManager.cs
//*********************************************************************************
// File Name: EQObjCacheManager.cs
// Description: Equipment Cache Manager
//
//(c) Copyright 2014, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
//**********************************************************************************
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.bcf.ConfigHandler;
using com.mirle.ibg3k0.bcf.Data.FlowRule;
using com.mirle.ibg3k0.bcf.Data.VO;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Data.VO;
using NLog;
using com.mirle.ibg3k0.sc.ConfigHandler;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.ProtocolFormat.SystemClass.PortInfo;

namespace com.mirle.ibg3k0.sc.Common
{

    public class CommObjCacheManager
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static CommObjCacheManager instance = null;
        private static Object _lock = new Object();
        private SCApplication scApp = null;
        private List<ReserveEnhanceInfoSection> ReserveEnhanceInfosSections = null;
        private List<TrafficControlInfo> TrafficControlInfos = null;
        //Cache Object
        //Section
        private List<ASECTION> Sections;
        //Segment
        private List<ASEGMENT> Segments;
        //Address
        private List<AADDRESS> Addresses;
        //GroupPortStation
        private List<AGROUPPORTSTATION> GroupPortStations;

        //PORT_INFO
        private List<PORT_INFO> PortInfos;
        private List<string> EnhanceSubAddresses = new List<string>();
        private CommonInfo CommonInfo;
        //


        private CommObjCacheManager() { }
        public static CommObjCacheManager getInstance()
        {
            lock (_lock)
            {
                if (instance == null)
                {
                    instance = new CommObjCacheManager();
                }
                return instance;
            }
        }


        public void setContext()
        {
        }

        public void start(SCApplication _app)
        {
            scApp = _app;

            Segments = scApp.MapBLL.loadAllSegments();
            Sections = scApp.MapBLL.loadAllSection();
            Addresses = scApp.MapBLL.loadAllAddress();
            GroupPortStations = scApp.GroupPortStationBLL.OperateDB.loadAllGroupPortStation();

            ReserveEnhanceInfosSections = scApp.ReserveEnhanceInfoDao.getReserveEnhanceInfoSections(scApp);
            TrafficControlInfos = scApp.TrafficControlInfoDao.getTrafficControlInfos(scApp);

            for (int i = 0; i < Addresses.Count; i++)
            {
                AADDRESS address = Addresses[i];
                address.initialAddressType();
                //address.initialSegmentID(scApp.SectionBLL);

                bool isReserveEnhanceAddress = GetReserveEnhanceInfo(address.ADR_ID) != null;
                if (isReserveEnhanceAddress && address.IsCoupler)
                {
                    AADDRESS couplerAndReserveEnhanceAddress = new CouplerAndReserveEnhanceAddress();
                    BCFUtility.setValueToPropety(ref address, ref couplerAndReserveEnhanceAddress);
                    setReserveEnhanceAddressInfo(couplerAndReserveEnhanceAddress);
                    setCouplerTypeAddressInfo(couplerAndReserveEnhanceAddress);
                    Addresses[i] = couplerAndReserveEnhanceAddress;
                }
                else if (address.IsCoupler)
                {
                    AADDRESS couplerAddress = new CouplerAddress();
                    BCFUtility.setValueToPropety(ref address, ref couplerAddress);
                    setCouplerTypeAddressInfo(couplerAddress);
                    Addresses[i] = couplerAddress;
                }
                else if (isReserveEnhanceAddress)
                {
                    AADDRESS couplerAndReserveEnhanceAddress = new ReserveEnhanceAddress();
                    BCFUtility.setValueToPropety(ref address, ref couplerAndReserveEnhanceAddress);
                    setReserveEnhanceAddressInfo(couplerAndReserveEnhanceAddress);
                    Addresses[i] = couplerAndReserveEnhanceAddress;
                }
            }

            foreach (ASECTION section in Sections)
            {
                section.setOnSectionAddress(scApp.AddressesBLL);
            }

            foreach (ASEGMENT segment in Segments)
            {
                segment.SetSectionList(scApp.SectionBLL);
                segment.checkNodeAddressesIsEnhance(scApp.getCommObjCacheManager().EnhanceSubAddresses);
            }

            foreach (TrafficControlInfo info in TrafficControlInfos)
            {
                info.registeredControlSectionLeaveEvent(scApp.SectionBLL);
            }


            CommonInfo = new CommonInfo();
            initialCommunationInfo();

        }

        private void setCouplerTypeAddressInfo(AADDRESS couplerAddress)
        {
            //  CouplerInfo coupler_info = GetCouplerInfo(couplerAddress.ADR_ID);
            CouplerData coupler_info = scApp.CouplerInfoDao.getCouplerInfo(scApp, couplerAddress.ADR_ID);
            if (coupler_info == null)
            {
                throw new Exception($"CouplerInfo not exist!,Adr id:{couplerAddress.ADR_ID}");
            }
            (couplerAddress as ICpuplerType).ChargerID = coupler_info.ChargerID;
            (couplerAddress as ICpuplerType).CouplerNum = (CouplerNum)coupler_info.CouplerNum;
            (couplerAddress as ICpuplerType).Priority = coupler_info.Priority;
            (couplerAddress as ICpuplerType).Priority = coupler_info.Priority;
            (couplerAddress as ICpuplerType).TrafficControlSegment = coupler_info.TrafficControlSegment.Split('-');
            //if (!SCUtility.isMatche(couplerAddress.ADR_ID, "24002"))
            (couplerAddress as ICpuplerType).IsEnable = true;
        }
        public MTLInfo getMTLInfo(string mtl_id)
        {
            MTLInfo mtlInfo = scApp.MTLInfoDao.getMTLInfo(scApp, mtl_id);
            return mtlInfo;
        }
        public List<ASEGMENT> getFireDoorSegment(string fireDoorID)
        {
            List<string> segment_ids = scApp.FireDoorDao.loadFireDoorSegmentID(scApp, fireDoorID);
            List<ASEGMENT> segments = Segments.Where(seg => segment_ids.Contains(seg.SEG_ID.Trim())).ToList();
            return segments;
        }
        public bool isSectionAtFireDoorArea(string section_id)
        {
            ASECTION section = scApp.SectionBLL.cache.GetSection(section_id);
            string segment_id = section.SEG_NUM;
            return scApp.FireDoorDao.isSegmentIDatFireDoorArea(scApp, segment_id);
        }
        public void sectionReserveAtFireDoorArea(string section_id)
        {
            ASECTION section = scApp.SectionBLL.cache.GetSection(section_id);
            string segment_id = section.SEG_NUM;
            string firedoor_id = scApp.FireDoorDao.getFireDoorIDBySegmentID(scApp, segment_id);
            AUNIT fireDoor = scApp.getEQObjCacheManager().getUnitByUnitID(firedoor_id);
            fireDoor.section_reserved(section_id);
        }

        public void sectionUnreserveAtFireDoorArea(string section_id)
        {
            ASECTION section = scApp.SectionBLL.cache.GetSection(section_id);
            string segment_id = section.SEG_NUM;
            string firedoor_id = scApp.FireDoorDao.getFireDoorIDBySegmentID(scApp, segment_id);
            AUNIT fireDoor = scApp.getEQObjCacheManager().getUnitByUnitID(firedoor_id);
            fireDoor.section_unreserved(section_id);
        }

        private void setReserveEnhanceAddressInfo(AADDRESS reserveEnhanceAddress)
        {
            ReserveEnhanceInfo enhanceInfo = GetReserveEnhanceInfo(reserveEnhanceAddress.ADR_ID);
            //(reserveEnhanceAddress as IReserveEnhance).EnhanceControlAddress = enhanceInfo.EnhanceControlAddress.Split('-');
            (reserveEnhanceAddress as IReserveEnhance).EnhanceControlAddress = enhanceInfo.EnhanceControlAddress;
            (reserveEnhanceAddress as IReserveEnhance).infos = GetReserveEnhanceInfos(reserveEnhanceAddress.ADR_ID);
            EnhanceSubAddresses.AddRange(enhanceInfo.EnhanceControlAddress);
        }

        private ReserveEnhanceInfo GetReserveEnhanceInfo(string adrID)
        {
            return null;
            //return scApp.ReserveEnhanceInfoDao.getReserveInfo(scApp, adrID);
        }
        private List<ReserveEnhanceInfo> GetReserveEnhanceInfos(string adrID)
        {
            return null;
            //return scApp.ReserveEnhanceInfoDao.getReserveInfos(scApp, adrID);
        }

        private void initialCommunationInfo()
        {
            List<APSetting> lstAPSetting = scApp.LineBLL.loadAPSettiong();
            CommonInfo.dicCommunactionInfo = new Dictionary<string, CommuncationInfo>();
            foreach (APSetting ap in lstAPSetting)
            {
                CommonInfo.dicCommunactionInfo.Add(
                    ap.AP_NAME
                    , new CommuncationInfo()
                    {
                        Name = ap.AP_NAME,
                        Getway_IP = ap.GETWAY_IP,
                        Remote_IP = ap.REMOTE_IP
                    });
            }
        }

        public void stop()
        {
            clearCache();
        }


        private void clearCache()
        {
            Sections.Clear();
        }


        private void removeFromDB()
        {
            //not implement yet.
        }

        #region 取得各種EQ Object的方法
        //Section
        public ASECTION getSection(string sec_id)
        {
            return Sections.Where(z => z.SEC_ID.Trim() == sec_id.Trim()).FirstOrDefault();
        }
        public ASECTION getSection(string adr1, string adr2)
        {
            return Sections.Where(s => (s.FROM_ADR_ID.Trim() == adr1.Trim() && s.TO_ADR_ID.Trim() == adr2.Trim())
                                    || (s.FROM_ADR_ID.Trim() == adr2.Trim() && s.TO_ADR_ID.Trim() == adr1.Trim())).FirstOrDefault();
        }
        public List<ASECTION> getSections()
        {
            return Sections;
        }
        //Segment
        public List<ASEGMENT> getSegments()
        {
            return Segments;
        }
        //Address
        public AADDRESS getAddress(string adr_id)
        {
            return Addresses.Where(a => a.ADR_ID.Trim() == adr_id.Trim()).FirstOrDefault();
        }
        public List<AADDRESS> getAddresses()
        {
            return Addresses;
        }
        public List<CouplerAddress> getCouplerAddresses()
        {
            return Addresses.Where(adr => adr is CouplerAddress).
                             Select(adr => adr as CouplerAddress).
                             ToList();
        }
        public List<string> getEnhanceSubAddresses()
        {
            return EnhanceSubAddresses;
        }

        public List<TrafficControlInfo> getTrafficControlInfos()
        {
            return TrafficControlInfos;
        }
        public List<AGROUPPORTSTATION> loadGroupPortStations()
        {
            return GroupPortStations;
        }


        public (bool isBlockControlSec, ReserveEnhanceInfoSection enhanceInfo) IsBlockControlSection(string sectionID)
        {
            var enhance_info = ReserveEnhanceInfosSections.
                Where(i => i.EnhanceControlSections.Contains(Common.SCUtility.Trim(sectionID)))
                .FirstOrDefault();
            return (enhance_info != null, enhance_info);
        }
        public (bool isBlockControlSec, ReserveEnhanceInfoSection enhanceInfo) IsBlockControlSection(string blockID, string sectionID)
        {
            var enhance_info = ReserveEnhanceInfosSections.
                Where(i => SCUtility.isMatche(i.BlockID, blockID) &&
                i.EnhanceControlSections.Contains(Common.SCUtility.Trim(sectionID)))
                .FirstOrDefault();
            return (enhance_info != null, enhance_info);
        }
        public (bool isEnhanceBlock, List<ReserveEnhanceInfoSection> enhanceBlock) IsBlockEntrySection(ProtocolFormat.OHTMessage.ReserveInfo info)
        {
            var enhanceBlock = GetReserveEnhanceInfos(info);
            return (enhanceBlock != null && enhanceBlock.Count > 0, enhanceBlock);
        }
        public List<ReserveEnhanceInfoSection> GetReserveEnhanceInfos(ProtocolFormat.OHTMessage.ReserveInfo info)
        {
            var enhance_info = ReserveEnhanceInfosSections.
                Where(i => i.EntrySectionInfos.
                           Where(entry_section => SCUtility.isMatche(entry_section.ReserveSectionID, info.ReserveSectionID) &&
                                                  entry_section.DriveDirction == info.DriveDirction).Count() > 0)
                .ToList();
            return enhance_info;
        }

        #endregion


        private void setValueToPropety<T>(ref T sourceObj, ref T destinationObj)
        {
            BCFUtility.setValueToPropety(ref sourceObj, ref destinationObj);
        }

        #region 將最新物件資料，放置入Cache的方法
        //NotImplemented
        #endregion


        #region 從DB取得最新EQ Object，並更新Cache
        //NotImplemented
        #endregion



    }
}
