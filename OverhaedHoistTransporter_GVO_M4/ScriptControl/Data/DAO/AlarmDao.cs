//*********************************************************************************
//      AlarmDao.cs
//*********************************************************************************
// File Name: AlarmDao.cs
// Description: AlarmDao類別
//
//(c) Copyright 2014, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
// 2014/03/05    Hayes Chen     N/A            N/A     Initial Release
// 2014/04/02    Miles Chen     N/A            A0.01   Modify Functions for UI Use
// 
//**********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.bcf.Data;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Data.VO;
using NLog;

namespace com.mirle.ibg3k0.sc.Data.DAO
{
    /// <summary>
    /// Class AlarmDao.
    /// </summary>
    /// <seealso cref="com.mirle.ibg3k0.bcf.Data.DaoBase" />
    public class AlarmDao : DaoBase
    {
        /// <summary>
        /// The logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ALARM getSetAlarm(DBConnection_EF conn, string eq_id, string code)
        {
            var alarm = from b in conn.ALARM
                        where b.ALAM_CODE == code.Trim() &&
                         b.EQPT_ID == eq_id.Trim() &&
                         b.ALAM_STAT == ProtocolFormat.OHTMessage.ErrorStatus.ErrSet
                        select b;
            return alarm.FirstOrDefault();
        }

        public List<ALARM> getAlarms(DBConnection_EF conn, DateTime startTime, DateTime endTime)
        {
            var alarm = from b in conn.ALARM
                        where b.RPT_DATE_TIME >= startTime &&
                         b.RPT_DATE_TIME <= endTime
                        orderby b.CLEAR_DATE_TIME
                        select b;
            return alarm.ToList();
        }

        /// <summary>
        /// Inserts the alarm.
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <param name="alarm">The alarm.</param>
        public void insertAlarm(DBConnection_EF conn, ALARM alarm)
        {
            try
            {
                conn.ALARM.Add(alarm);
                conn.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        /// <summary>
        /// Updates the alarm.
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <param name="alarm">The alarm.</param>
        public void updateAlarm(DBConnection_EF conn, ALARM alarm)
        {
            try
            {
                conn.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public void UpdateAllAlarmStatus2ClearByVhID(DBConnection_EF con, string vh_id)
        {
            string sql = "Update [ALARM] SET [ALAM_STAT] = {0} WHERE [EQPT_ID] = {1}";
            //con.Database.ExecuteSqlCommand(sql,, vh_id, seq_no);
        }

        public int getSetAlarmCountByEQAndCode(DBConnection_EF conn, string eq_id, string code)
        {
            try
            {
                var alarm = from b in conn.ALARM
                            where b.ALAM_CODE == code.Trim() &&
                                  b.EQPT_ID == eq_id &&
                                  b.ALAM_STAT == ProtocolFormat.OHTMessage.ErrorStatus.ErrSet
                            select b;
                return alarm.Count();
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public List<ALARM> loadSetAlarm(DBConnection_EF conn, string eq_id)
        {
            try
            {
                var alarm = from a in conn.ALARM
                            where a.EQPT_ID == eq_id &&
                                  a.ALAM_STAT == ProtocolFormat.OHTMessage.ErrorStatus.ErrSet
                            select a;
                return alarm.ToList();
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public List<ALARM> loadSetAlarm(DBConnection_EF conn)
        {
            try
            {
                var alarm = from a in conn.ALARM
                            where a.ALAM_STAT == ProtocolFormat.OHTMessage.ErrorStatus.ErrSet
                            select a;
                return alarm.ToList();
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }
        public List<ALARM> loadSetErrorAlarm(DBConnection_EF conn)
        {
            try
            {
                var alarm = from a in conn.ALARM
                            where a.ALAM_STAT == ProtocolFormat.OHTMessage.ErrorStatus.ErrSet
                                  && a.ALAM_LVL == E_ALARM_LVL.Error
                            select a;
                return alarm.ToList();
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public List<ALARM> loadSetMinorAlarm(DBConnection_EF conn)
        {
            try
            {
                var alarm = from a in conn.ALARM
                            where a.ALAM_STAT == ProtocolFormat.OHTMessage.ErrorStatus.ErrSet
                                  && a.ALAM_LVL == E_ALARM_LVL.Warn
                            select a;
                return alarm.ToList();
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }




        public List<ALARM> loadAllAlarmByStartTimeEndTime(DBConnection_EF conn, DateTime set_time, DateTime clear_time)
        {
            try
            {
                var alarm = from a in conn.ALARM
                            where ((a.ALAM_STAT == ProtocolFormat.OHTMessage.ErrorStatus.ErrReset && (a.RPT_DATE_TIME > set_time && a.CLEAR_DATE_TIME < clear_time))
                            || (a.ALAM_STAT == ProtocolFormat.OHTMessage.ErrorStatus.ErrSet && (a.RPT_DATE_TIME > set_time))) && (a.ALAM_LVL == E_ALARM_LVL.Error)
                            select a;
                return alarm.ToList();
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public IQueryable getQueryAllSQL(DBConnection_EF conn)
        {
            try
            {
                return conn.ALARM.Select((ALARM a) => a);
            }
            catch (Exception value)
            {
                logger.Warn(value);
                throw;
            }
        }

    }
}
