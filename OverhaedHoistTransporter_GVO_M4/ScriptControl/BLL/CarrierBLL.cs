using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.Data.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class CarrierBLL
    {
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public DB db { private set; get; }
        public Catch OperateCatch { private set; get; }

        public CarrierBLL()
        {
        }
        public void start(SCApplication _app)
        {
            db = new DB(_app.CarrierDao);
            OperateCatch = new Catch(_app.getEQObjCacheManager());
        }
        public class DB
        {
            CarrierDao CarrierDao = null;
            public DB(CarrierDao _CarrierDao)
            {
                CarrierDao = _CarrierDao;
            }
            #region Inser
            public void addOrUpdate(ACARRIER carrier)
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    var vcarrierTemp = CarrierDao.getByID(con, carrier.ID);
                    if (vcarrierTemp == null)
                    {
                        CarrierDao.add(con, carrier);
                    }
                    else
                    {
                        //vcarrierTemp.INSER_TIME = carrier.INSER_TIME;
                        vcarrierTemp.LOCATION = carrier.LOCATION;
                        vcarrierTemp.INSTALLED_TIME = carrier.INSTALLED_TIME;
                        vcarrierTemp.RENAME_ID = carrier.RENAME_ID;
                        vcarrierTemp.STATE = carrier.STATE;
                        vcarrierTemp.LOT_ID = carrier.LOT_ID;
                        vcarrierTemp.TYPE = carrier.TYPE;
                        vcarrierTemp.READ_STATUS = carrier.READ_STATUS;
                        vcarrierTemp.FINISH_TIME = carrier.FINISH_TIME;
                        vcarrierTemp.HOSTSOURCE = carrier.HOSTSOURCE;
                        vcarrierTemp.HOSTDESTINATION = carrier.HOSTDESTINATION;
                        vcarrierTemp.CSTType = carrier.CSTType;
                        CarrierDao.update(con, vcarrierTemp);
                    }
                }
            }
            #endregion Inser
            #region update
            public bool updateLocationAndState(string carrierID, string location, ProtocolFormat.OHTMessage.E_CARRIER_STATE state)
            {
                bool isSuccess = true;
                try
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        ACARRIER carrier = CarrierDao.getByID(con, carrierID);
                        if (carrier != null)
                        {
                            carrier.LOCATION = location;
                            carrier.STATE = state;
                            switch (state)
                            {
                                case ProtocolFormat.OHTMessage.E_CARRIER_STATE.Installed:
                                    carrier.INSTALLED_TIME = DateTime.Now;
                                    break;
                                case ProtocolFormat.OHTMessage.E_CARRIER_STATE.Complete:
                                case ProtocolFormat.OHTMessage.E_CARRIER_STATE.OpRemove:
                                    carrier.FINISH_TIME = DateTime.Now;
                                    break;
                            }
                            CarrierDao.update(con, carrier);
                        }
                        else
                        {
                            isSuccess = false;
                            //todo kevin Exception log
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                    isSuccess = false;
                }
                return isSuccess;
            }

            public bool updateStoredTime(string carrierID)
            {
                bool isSuccess = true;
                try
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        ACARRIER carrier = CarrierDao.getByID(con, carrierID);
                        if (carrier != null)
                        {
                            carrier.StoreDT = DateTime.Now.ToString("yy/MM/dd HH:mm:ss");
                            CarrierDao.update(con, carrier);
                        }
                        else
                        {
                            isSuccess = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                    isSuccess = false;
                }
                return isSuccess;
            }

            public bool updateWaitOutOPTime(string carrierID)
            {
                bool isSuccess = true;
                try
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        ACARRIER carrier = CarrierDao.getByID(con, carrierID);
                        if (carrier != null)
                        {
                            carrier.WaitOutOPDT = DateTime.Now.ToString("yy/MM/dd HH:mm:ss");
                            CarrierDao.update(con, carrier);
                        }
                        else
                        {
                            isSuccess = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                    isSuccess = false;
                }
                return isSuccess;
            }

            public bool updateWaitOutLPTime(string carrierID)
            {
                bool isSuccess = true;
                try
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        ACARRIER carrier = CarrierDao.getByID(con, carrierID);
                        if (carrier != null)
                        {
                            carrier.WaitOutOPDT = DateTime.Now.ToString("yy/MM/dd HH:mm:ss");
                            CarrierDao.update(con, carrier);
                        }
                        else
                        {
                            isSuccess = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                    isSuccess = false;
                }
                return isSuccess;
            }


            public bool updateRenameID(string carrierID, string renameID)
            {
                bool isSuccess = true;
                try
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        ACARRIER carrier = CarrierDao.getByID(con, carrierID);
                        if (carrier != null)
                        {
                            carrier.RENAME_ID = renameID;
                            CarrierDao.update(con, carrier);
                        }
                        else
                        {
                            isSuccess = false;
                            //todo kevin Exception log
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                    isSuccess = false;
                }
                return isSuccess;
            }
            public bool updateReadStatus(string carrierID, ProtocolFormat.OHTMessage.E_ID_READ_STSTUS readResult,string renameCstID)
            {
                bool isSuccess = true;
                try
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        ACARRIER carrier = CarrierDao.getByID(con, carrierID);
                        if (carrier != null)
                        {
                            carrier.RENAME_ID = renameCstID;
                            carrier.READ_STATUS = readResult;
                            CarrierDao.update(con, carrier);
                        }
                        else
                        {
                            isSuccess = false;
                            //todo kevin Exception log
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                    isSuccess = false;
                }
                return isSuccess;
            }
            public bool updateState(string carrierID, ProtocolFormat.OHTMessage.E_CARRIER_STATE state)
            {
                bool isSuccess = true;
                try
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        ACARRIER carrier = CarrierDao.getByID(con, carrierID);
                        if (carrier != null)
                        {
                            carrier.STATE = state;
                            switch (state)
                            {
                                case ProtocolFormat.OHTMessage.E_CARRIER_STATE.Installed:
                                    carrier.INSTALLED_TIME = DateTime.Now;
                                    break;
                                case ProtocolFormat.OHTMessage.E_CARRIER_STATE.Complete:
                                case ProtocolFormat.OHTMessage.E_CARRIER_STATE.OpRemove:
                                case ProtocolFormat.OHTMessage.E_CARRIER_STATE.MoveError:
                                    carrier.FINISH_TIME = DateTime.Now;
                                    break;
                            }
                            CarrierDao.update(con, carrier);
                        }
                        else
                        {
                            isSuccess = false;
                            //todo kevin Exception log
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                    isSuccess = false;
                }
                return isSuccess;
            }
            #endregion update
            #region Delete
            public bool delete(string carrierID) //todo kevin 要再將他搬至History
            {
                bool isSuccess = true;
                try
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        ACARRIER carrier = CarrierDao.getByID(con, carrierID);
                        if (carrier != null)
                        {
                            CarrierDao.delete(con, carrier);
                        }
                        else
                        {
                            //todo kevin Exception log
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                    isSuccess = false;
                }
                return isSuccess;
            }
            #endregion Delete
            #region Query
            public List<ACARRIER> LoadCassetteDataByOHCV(string portName)   //取得 OHCV Port 上的所有卡匣
            {
                try
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        return CarrierDao.LoadCassetteDataByOHCV(con, portName);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                    return null;
                }
            }
            //public List<ACARRIER> LoadCassetteDataByNotCompleted()
            //{
            //    try
            //    {
            //        using (DBConnection_EF con = DBConnection_EF.GetUContext())
            //        {
            //            return CarrierDao.LoadCassetteDataByNotCompleted(con);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.Error(ex, "Exception");
            //        return null;
            //    }
            //}

            public ACARRIER loadCassetteDataByLoc(string portName)
            {
                try
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        return CarrierDao.LoadCassetteDataByLoc(con, portName);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                    return null;
                }
            }


            public List<ACARRIER> loadCassetteListByLoc(string portName)
            {
                try
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        return CarrierDao.LoadCassetteListByLoc(con, portName);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                    return null;
                }
            }


            public ACARRIER loadCassetteDataStartWithAndLoc(string cst_id,string loc)
            {
                try
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        return CarrierDao.LoadCassetteDataStartWithAndLoc(con, cst_id, loc);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                    return null;
                }
            }

            public List<ACARRIER> loadCurrentInLineCarrier()
            {
                List<ACARRIER> carriers = null;
                try
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        carriers = CarrierDao.loadCurrentInLineCarrier(con);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                }
                return carriers;
            }
            public (bool has, ACARRIER onVhCarrier) hasCarrierOnVhLocation(string locationID)
            {
                ACARRIER carrier = null;
                try
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        carrier = CarrierDao.getInLineCarrierByLocationID(con, locationID);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                }
                return (carrier != null, carrier);
            }
            public (bool has, ACARRIER inLineCarrier) hasCarrierInLine(string carrierID)
            {
                ACARRIER carrier = null;
                try
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        carrier = CarrierDao.getInLineCarrierByCarrierID(con, carrierID);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                }
                return (carrier != null, carrier);
            }
            public ACARRIER getCarrier(string carrierID)
            {
                ACARRIER carrier = null;
                try
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        carrier = CarrierDao.getByID(con, carrierID);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                }
                return carrier;
            }
            #endregion
        }
        public class Catch
        {
            EQObjCacheManager CacheManager;
            public Catch(EQObjCacheManager _cache_manager)
            {
                CacheManager = _cache_manager;
            }
        }
    }
}