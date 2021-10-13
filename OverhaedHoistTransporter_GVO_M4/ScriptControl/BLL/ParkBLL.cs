using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.Data.DAO;
using com.mirle.ibg3k0.sc.Data.VO;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class ParkBLL
    {
        ParkZoneTypeDao parkZoneTypeDao = null;
        ParkZoneMasterDao parkZoneMasterDao = null;
        ParkZoneDetailDao parkZoneDetailDao = null;
        protected Logger logger_ParkBllLog = LogManager.GetLogger("ParkBLL");
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private SCApplication scApp = null;
        public ParkBLL()
        {

        }
        public void start(SCApplication app)
        {
            scApp = app;
            parkZoneTypeDao = scApp.ParkZoneTypeDao;
            parkZoneMasterDao = scApp.ParkZoneMasterDao;
            parkZoneDetailDao = scApp.ParkZoneDetailDao;
        }

        public List<string> loadAllParkZoneType()
        {
            List<string> parkZoneTypes = null;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                parkZoneTypes = parkZoneTypeDao.loadAll(con).Select(zone_type => zone_type.PARK_TYPE_ID.Trim()).ToList();
            }

            return parkZoneTypes;
        }

        

        public Boolean setCurrentParkType()
        {
            bool isSuccess = false;
            APARKZONETYPE parkZoneType = null;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                parkZoneType = parkZoneTypeDao.getUsingParkType(con);
            }

            if (parkZoneType != null)
            {
                scApp.getEQObjCacheManager().getLine().
                    Currnet_Park_Type = parkZoneType.PARK_TYPE_ID.Trim();
            }
            return isSuccess;
        }
        public APARKZONEDETAIL getParkDetailByAdr(string adr)
        {
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                return parkZoneDetailDao.getByAdrID(con, adr);
            }
        }
        public APARKZONEDETAIL getParkDetailByZoneIDAndPRIO(string zone_id, int prio)
        {
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                return parkZoneDetailDao.getByZoneIDAndPRIO(con, zone_id, prio);
            }
        }
        public APARKZONEDETAIL getParkDetailByParkZoneIDPrioAscAndCanParkingAdr(string parkZoneID)
        {
            using (DBConnection_EF con = new DBConnection_EF())
            {
                return parkZoneDetailDao.getByParkZoneIDPrioAscAndCanParkingAdr(con, parkZoneID);
            }
        }
        public APARKZONEDETAIL getParkDetailByParkZoneIDPrioDes(string parkZoneID)
        {
            using (DBConnection_EF con = new DBConnection_EF())
            {
                return parkZoneDetailDao.getByParkZoneIDPrioDes(con, parkZoneID);
            }
        }
        public APARKZONEMASTER getParkZoneMasterByParkZoneID(string park_zone_id)
        {
            APARKZONEMASTER park_master = null;
            using (DBConnection_EF con = new DBConnection_EF())
            {
                park_master = parkZoneMasterDao.getByID(con, park_zone_id);
            }
            return park_master;
        }

        public APARKZONEMASTER getParkZoneMasterByAdrID(string adr_id)
        {
            APARKZONEMASTER park_master = null;
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                park_master = parkZoneMasterDao.getByParkingAdr(con, adr_id);
            }
            return park_master;
        }

        public enum FindParkResult
        {
            Success,
            HasParkZoneNoFindRoute,
            NoParkZone,
            OrtherError
        }
      

        public bool EnableParkZoneMaster(string park_zone_id, bool enable)
        {
            bool iiSuccess = false;
            APARKZONEMASTER park_master = null;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                park_master = parkZoneMasterDao.getByID(con, park_zone_id);
                if (park_master != null)
                {
                    park_master.IS_ACTIVE = enable;
                    parkZoneMasterDao.upadate(con, park_master);
                    iiSuccess = true;
                }
            }
            return iiSuccess;
        }


        /// <summary>
        /// //找出目前所有Park Adr停車的 跟 欲前往的方便得知目前為何沒有停車位
        /// </summary>
        /// <param name="con"></param>
        private void ParkAdrInfoTarce()
        {
            //DBConnection_EF con = DBConnection_EF.GetContext();
            List<APARKZONEDETAIL> lstAllParkDetail = null;
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                lstAllParkDetail = parkZoneDetailDao.loadAllParkAdrByParkTypeID(con,
                    scApp.getEQObjCacheManager().getLine().Currnet_Park_Type);
            }
            if (lstAllParkDetail == null)
                return;
            foreach (APARKZONEDETAIL detail in lstAllParkDetail)
            {
                string park_adr = detail.ADR_ID;
                string parkingVhID = detail.CAR_ID ?? string.Empty;
                string onWayVhID = string.Empty;
                AVEHICLE onWayVh = null;
                if (scApp.VehicleBLL.hasVhReserveParkAdr(park_adr, out onWayVh))
                {
                    onWayVhID = onWayVh.VEHICLE_ID;
                }
                string park_warn_info =
                    string.Format("Park adr:[{0}] ,Parking vh id:[{1}] ,On way vh id:[{2}]"
                                    , park_adr
                                    , parkingVhID
                                    , onWayVhID);
                logger_ParkBllLog.Info(park_warn_info);
            }
        }

        public List<APARKZONEMASTER> loadByParkTypeIDAndHasParkSpaceByCount(String _park_type_id, E_VH_TYPE vh_type)
        {
            List<APARKZONEMASTER> rtnParkZoneMaster = new List<APARKZONEMASTER>();
            //DBConnection_EF con = DBConnection_EF.GetContext();
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                List<APARKZONEMASTER> lstParkZoneMasterTemp = parkZoneMasterDao.loadByParkTypeID(con
                         , scApp.getEQObjCacheManager().getLine().Currnet_Park_Type, vh_type);
                foreach (APARKZONEMASTER master in lstParkZoneMasterTemp)
                {

                    int parkCount = parkZoneDetailDao.getCountByParkZoneIDAndVhOnAdrIncludeOnWay(con, master.PARK_ZONE_ID);
                    //int readyComeToVhCountByCMD = 0;
                    //if (scApp.CMDBLL.hasExcuteCMDFromToAdrIsParkInSpecifyPackZoneID
                    //    (master.PACK_ZONE_ID, out readyComeToVhCountByCMD))
                    //{
                    //    parkCount = parkCount + readyComeToVhCountByCMD;
                    //}
                    if (parkCount < master.TOTAL_BORDER)
                        rtnParkZoneMaster.Add(master);
                }
            }
            return rtnParkZoneMaster;
        }

        public List<APARKZONEMASTER> loadByParkTypeIDAndHasParkSpaceByCount(String _park_type_id)
        {
            List<APARKZONEMASTER> rtnParkZoneMaster = new List<APARKZONEMASTER>();
            //DBConnection_EF con = DBConnection_EF.GetContext();
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                List<APARKZONEMASTER> lstParkZoneMasterTemp = parkZoneMasterDao.loadByParkTypeID(con
                         , scApp.getEQObjCacheManager().getLine().Currnet_Park_Type);
                foreach (APARKZONEMASTER master in lstParkZoneMasterTemp)
                {

                    int parkCount = parkZoneDetailDao.getCountByParkZoneIDAndVhOnAdrIncludeOnWay(con, master.PARK_ZONE_ID);
                    //int readyComeToVhCountByCMD = 0;
                    //if (scApp.CMDBLL.hasExcuteCMDFromToAdrIsParkInSpecifyPackZoneID
                    //    (master.PACK_ZONE_ID, out readyComeToVhCountByCMD))
                    //{
                    //    parkCount = parkCount + readyComeToVhCountByCMD;
                    //}
                    if (parkCount < master.TOTAL_BORDER)
                        rtnParkZoneMaster.Add(master);
                }
            }
            return rtnParkZoneMaster;
        }


        public APARKZONEDETAIL findFitParkZoneDetailInParkMater(string zone_master_id)
        {
            //DBConnection_EF con = DBConnection_EF.GetContext();
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                APARKZONEMASTER parkZoneMaster = parkZoneMasterDao.getByID(con, zone_master_id);
                return findFitParkZoneDetailInParkMater(parkZoneMaster);
            }
        }

        private APARKZONEDETAIL findFitParkZoneDetailInParkMater(APARKZONEMASTER zone_master_temp)
        {
            //DBConnection_EF con = DBConnection_EF.GetContext();
            APARKZONEDETAIL bestParkDetailTemp = null;
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {

                switch (zone_master_temp.PARK_TYPE)
                {
                    case E_PARK_TYPE.OrderByAsc:
                        //bestPackDetailTemp = packZoneDetailDao.getByPackZoneIDPrioAscAndCanPackingAdr
                        bestParkDetailTemp = parkZoneDetailDao.getByParkZoneIDPrioDes
                            (con, zone_master_temp.PARK_ZONE_ID);
                        break;
                    case E_PARK_TYPE.OrderByDes:
                        bestParkDetailTemp = parkZoneDetailDao.getByParkZoneIDPrioDes
                            (con, zone_master_temp.PARK_ZONE_ID);
                        break;
                }
            }
            return bestParkDetailTemp;
        }


        public void updateVhEntryParkingAdr(string vh_id, APARKZONEDETAIL parkZoneDetail)
        {
            ALINE line = scApp.getEQObjCacheManager().getLine();
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                updateParkAdrEmpty(vh_id);
                con.APARKZONEDETAIL.Attach(parkZoneDetail);
                parkZoneDetail.CAR_ID = vh_id;
                con.Entry(parkZoneDetail).Property(p => p.CAR_ID).IsModified = true;
                parkZoneDetailDao.update(con, parkZoneDetail);
            }
        }

        //TODO 要改成直接用SQL下達Command
        private void updateParkAdrEmpty(string vh_id)
        {
            //DBConnection_EF con = DBConnection_EF.GetContext();
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                List<APARKZONEDETAIL> lstParkDetail = parkZoneDetailDao.loadAllParkDetailByVeID(con, vh_id);
                if (lstParkDetail != null && lstParkDetail.Count > 0)
                {
                    foreach (APARKZONEDETAIL detail in lstParkDetail)
                    {
                        detail.CAR_ID = string.Empty;
                        parkZoneDetailDao.update(con, detail);
                    }
                }
            }
        }


        public void resetParkAdr(string park_adr)
        {
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                APARKZONEDETAIL parkZoneDetail = parkZoneDetailDao.getByAdrID(con, park_adr);
                if (parkZoneDetail != null)
                {
                    parkZoneDetail.CAR_ID = string.Empty;
                    parkZoneDetailDao.update(con, parkZoneDetail);
                }
            }
        }



        public bool checkParkZoneLowerBorder(out List<APARKZONEMASTER> park_zone_masters)
        {
            bool isEnough = true;
            park_zone_masters = new List<APARKZONEMASTER>();
            List<APARKZONEMASTER> lstMaster = null;
            using (DBConnection_EF con = new DBConnection_EF())
            {
                lstMaster = parkZoneMasterDao.loadByParkTypeID(con
                    , scApp.getEQObjCacheManager().getLine().Currnet_Park_Type);
                if (lstMaster != null)
                {
                    foreach (APARKZONEMASTER master in lstMaster)
                    {
                        //List<APARKZONEDETAIL> lstVhOnAdr = parkZoneDetailDao.
                        //    loadByParkZoneIDAndVhOnAdrIncludeOnWay(con, master.PARK_ZONE_ID);
                        int parkCount = parkZoneDetailDao.
                         getCountByParkZoneIDAndVhOnAdrIncludeOnWay(con, master.PARK_ZONE_ID);
                        if (parkCount < master.LOWER_BORDER)
                        {
                            park_zone_masters.Add(master);
                            isEnough = false;
                            //break;
                        }
                    }
                }
            }
            return isEnough;
        }

        public APARKZONEDETAIL getParkAddress(string adr, E_VH_TYPE park_type)
        {
            APARKZONEDETAIL park_detail = null;
            //DBConnection_EF con = DBConnection_EF.GetContext();
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                park_detail = parkZoneDetailDao.getParkAdrCountByParkTypeAndAdr
                    (con,
                     scApp.getEQObjCacheManager().getLine().Currnet_Park_Type,
                     adr,
                     park_type);
            }
            return park_detail;
        }

        

        public bool tryAdjustTheVhParkingPositionByParkZoneAndPrio()
        {
            bool hasAdjust = false;
            List<APARKZONEMASTER> park_masters = null;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                park_masters = parkZoneMasterDao.loadByParkTypeIDAndHasVh(con,
                    scApp.getEQObjCacheManager().getLine().Currnet_Park_Type);
            }
            if (park_masters != null)
            {
                foreach (APARKZONEMASTER park_master in park_masters)
                {
                    //if (pack_master.PACK_TYPE == E_PACK_TYPE.OrderByDes)
                    //    continue;
                    bool isSuccess = false;
                    isSuccess = tryAdjustTheVhParkingPositionByParkZoneAndPrio(park_master);
                    hasAdjust = hasAdjust | isSuccess;
                }
            }
            return hasAdjust;
        }
        public bool tryAdjustTheVhParkingPositionByParkZoneAndPrio(string want2AdjustParkAdr)
        {
            APARKZONEMASTER park_master = null;
            using (DBConnection_EF con = new DBConnection_EF())
            {
                park_master = parkZoneMasterDao.getByParkingAdr(con, want2AdjustParkAdr);
            }
            return tryAdjustTheVhParkingPositionByParkZoneAndPrio(park_master);
        }
        public bool tryAdjustTheVhParkingPositionByParkZoneAndPrio(APARKZONEDETAIL want2AdjustZoneDetail)
        {
            APARKZONEMASTER park_master = null;
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                park_master = parkZoneMasterDao.getByID(con, want2AdjustZoneDetail.PARK_ZONE_ID);
            }
            return tryAdjustTheVhParkingPositionByParkZoneAndPrio(park_master);
        }
        public bool tryAdjustTheVhParkingPositionByParkZoneAndPrio(APARKZONEMASTER want2AdjustZoneMater)
        {
            if (want2AdjustZoneMater == null)
                return false;
            bool isSuccess = true;
            int first_Prio = 11;
            List<APARKZONEDETAIL> hasParkingDetail = null;
            //DBConnection_EF con = DBConnection_EF.GetContext();
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                hasParkingDetail = parkZoneDetailDao.loadByParkZoneIDAndVhOnAdr(con, want2AdjustZoneMater.PARK_ZONE_ID);
                if (hasParkingDetail == null || hasParkingDetail.Count == 0)
                    return false;
                APARKZONEDETAIL nextParkZoneDetail = null;

                switch (want2AdjustZoneMater.PARK_TYPE)
                {
                    case E_PARK_TYPE.OrderByAsc:
                        foreach (APARKZONEDETAIL parkDetail in hasParkingDetail)
                        {
                            if (parkDetail.PRIO > first_Prio)
                            {
                                nextParkZoneDetail = parkZoneDetailDao.
                                    getByZoneIDAndPRIO(con, parkDetail.PARK_ZONE_ID, parkDetail.PRIO - 1);
                                if (nextParkZoneDetail != null &&
                                    SCUtility.isEmpty(nextParkZoneDetail.CAR_ID))
                                {
                                    //目前的寫法應該不用在檢查是否有人預約了下一個車位
                                    bool hasReserve = scApp.VehicleBLL.hasVhReserveParkAdr(nextParkZoneDetail.ADR_ID);
                                    if (!hasReserve)
                                    {
                                        //scApp.CMDBLL.creatCommand_OHTC(packDetail.CAR_ID, string.Empty, string.Empty, E_CMD_TYPE.Move_Pack
                                        //     , packDetail.ADR_ID, nextPackZoneDetail.ADR_ID, 0, 0);
                                        //if (!scApp.CMDBLL.generateCmd_OHTC_Details())
                                        //{
                                        //    break;
                                        //}
                                        scApp.CMDBLL.doCreatCommand(parkDetail.CAR_ID, string.Empty, string.Empty, E_CMD_TYPE.Move_Park
                                         , parkDetail.ADR_ID, nextParkZoneDetail.ADR_ID, 0, 0);
                                    }
                                }
                            }
                        }
                        break;
                    case E_PARK_TYPE.OrderByDes:
                        //1確認是否有其他車輛已經預約這個位置
                        //   是-找下一個位置是否有空位
                        //      是-往前移
                        //      否-先記住命令，再找下一台，直到找到最後一個可以往前移的空位時就一次下達
                        //hasPackingDetail.Reverse();
                        //APACKZONEDETAIL firstPackDetail = hasPackingDetail.First();
                        //APACKZONEDETAIL currentPackZoneDetail = null;

                        //bool hasOrtherVhReserve = scApp.VehicleBLL.hasVhReservePackAdr(firstPackDetail.ADR_ID);
                        //if (hasOrtherVhReserve)
                        //{
                        //    List<ACMD_OHTC> ACMD_OHTCs = new List<ACMD_OHTC>();
                        //    for (int next_prio = firstPackDetail.PRIO - 1; next_prio <= first_Prio; next_prio--)
                        //    {
                        //        currentPackZoneDetail = packZoneDetailDao.
                        //             getByZoneIDAndPRIO(con, firstPackDetail.PACK_ZONE_ID, next_prio + 1);
                        //        nextPackZoneDetail = packZoneDetailDao.
                        //             getByZoneIDAndPRIO(con, firstPackDetail.PACK_ZONE_ID, next_prio);
                        //        if (nextPackZoneDetail != null)
                        //        {
                        //            ACMD_OHTC prebuildCMD = scApp.CMDBLL.buildCommand_OHTC(currentPackZoneDetail.CAR_ID, string.Empty, E_CMD_TYPE.Move_Pack
                        //                , currentPackZoneDetail.ADR_ID, nextPackZoneDetail.ADR_ID, 0, 0);
                        //            ACMD_OHTCs.Add(prebuildCMD);
                        //            if (SCUtility.isEmpty(nextPackZoneDetail.CAR_ID))
                        //            {
                        //                foreach (ACMD_OHTC cmd in ACMD_OHTCs)
                        //                {
                        //                    scApp.CMDBLL.creatCommand_OHTC(cmd);
                        //                    if (!scApp.CMDBLL.generateCmd_OHTC_Details())
                        //                    {
                        //                        return false;
                        //                    }
                        //                    isSuccess = true;
                        //                }
                        //                return true;
                        //            }
                        //        }
                        //        else
                        //        {
                        //            return false;
                        //        }
                        //    }
                        //}
                        break;
                }

            }
            return isSuccess;
        }


    }
}
