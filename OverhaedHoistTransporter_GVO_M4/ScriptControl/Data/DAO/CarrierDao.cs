using com.mirle.ibg3k0.sc.Data.SECS;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace com.mirle.ibg3k0.sc.Data.DAO
{
    public class CarrierDao
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        #region Inser
        public void add(DBConnection_EF con, ACARRIER carrier)
        {
            con.ACARRIER.Add(carrier);
            con.SaveChanges();
        }
        #endregion Inser
        #region Query
        public ACARRIER getByID(DBConnection_EF con, String carrierID)
        {
            var query = from db_obj in con.ACARRIER
                        where db_obj.ID == carrierID.Trim()
                        select db_obj;
            return query.FirstOrDefault();
        }
        public List<ACARRIER> LoadCassetteDataByOHCV(DBConnection_EF conn, string portName)
        {
            try
            {
                var result = conn.ACARRIER.Where(x => x.LOCATION.Contains(portName.Trim())).ToList();

                return result;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }
        //public List<ACARRIER> LoadCassetteDataByNotCompleted(DBConnection_EF conn)  //原本打算是要取得除了在 shelf 的所有帳
        //{
        //    try
        //    {
        //        var port = from a in conn.ACARRIER
        //                   where a.CSTState != E_CSTState.Completed
        //                   select a;
        //        return port.ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Warn(ex);
        //        throw;
        //    }
        //}

        public List<ACARRIER> loadCurrentInLineCarrier(DBConnection_EF con)
        {
            var query = from db_obj in con.ACARRIER
                        where db_obj.STATE == ProtocolFormat.OHTMessage.E_CARRIER_STATE.WaitIn ||
                              db_obj.STATE == ProtocolFormat.OHTMessage.E_CARRIER_STATE.Installed||
                              db_obj.STATE == ProtocolFormat.OHTMessage.E_CARRIER_STATE.Complete ||
                              db_obj.STATE == ProtocolFormat.OHTMessage.E_CARRIER_STATE.MoveError
                        select db_obj;
            return query.ToList();
        }
        public ACARRIER getInLineCarrierByLocationID(DBConnection_EF con, string location)
        {
            var query = from db_obj in con.ACARRIER
                        where (db_obj.STATE == ProtocolFormat.OHTMessage.E_CARRIER_STATE.WaitIn ||
                              db_obj.STATE == ProtocolFormat.OHTMessage.E_CARRIER_STATE.Installed) &&
                              db_obj.LOCATION == location
                        select db_obj;
            return query.FirstOrDefault();
        }
        public ACARRIER getInLineCarrierByCarrierID(DBConnection_EF con, string carrierID)
        {
            var query = from db_obj in con.ACARRIER
                        where (db_obj.STATE == ProtocolFormat.OHTMessage.E_CARRIER_STATE.WaitIn ||
                              db_obj.STATE == ProtocolFormat.OHTMessage.E_CARRIER_STATE.Installed) &&
                              db_obj.ID == carrierID
                        select db_obj;
            return query.FirstOrDefault();
        }

        public ACARRIER LoadCassetteDataByLoc(DBConnection_EF conn, string portName)
        {
            try
            {
                var result = conn.ACARRIER.Where(x => x.LOCATION.Trim() == portName.Trim()).FirstOrDefault();

                return result;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }
        public List<ACARRIER> LoadCassetteListByLoc(DBConnection_EF conn, string portName)
        {
            try
            {
                var result = conn.ACARRIER.Where(x => x.LOCATION.Trim() == portName.Trim()).ToList();

                return result;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }
        public ACARRIER LoadCassetteDataStartWithAndLoc(DBConnection_EF conn, string cst_id,string loc)
        {
            try
            {
                var result = conn.ACARRIER.Where(x => x.ID.Trim().StartsWith(cst_id.Trim())&&x.LOCATION.Trim() == loc.Trim()).FirstOrDefault();

                return result;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public List<ACARRIER> LoadCassetteData(DBConnection_EF conn)
        {
            try
            {
                var port = from a in conn.ACARRIER
                           select a;
                return port.ToList();
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        #endregion Query
        #region update
        public void update(DBConnection_EF con, ACARRIER carrier)
        {
            con.SaveChanges();
        }
        #endregion update
        #region Delete
        public void delete(DBConnection_EF con, ACARRIER carrier)
        {
            con.ACARRIER.Remove(carrier);
            con.SaveChanges();
        }


        #endregion Delete

        public IQueryable getQueryAllSQL(DBConnection_EF conn)
        {
            try
            {
                return conn.ACARRIER.Select((ACARRIER a) => a);
            }
            catch (Exception value)
            {
                logger.Warn(value);
                throw;
            }
        }

    }
}