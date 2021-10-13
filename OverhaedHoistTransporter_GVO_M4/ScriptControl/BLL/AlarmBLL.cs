//*********************************************************************************
//      AlarmBLL.cs
//*********************************************************************************
// File Name: AlarmBLL.cs
// Description: 業務邏輯：Alarm
//
//(c) Copyright 2014, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
//**********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.DAO;
using com.mirle.ibg3k0.sc.Data.SECS;
using com.mirle.ibg3k0.sc.Data.VO;
using NLog;
using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.sc.Data;
using Newtonsoft.Json;

namespace com.mirle.ibg3k0.sc.BLL
{
    /// <summary>
    /// Class AlarmBLL.
    /// </summary>
    public class AlarmBLL
    {

        public const string VEHICLE_ALARM_HAPPEND = "00000";
        public const string VEHICLE_LONG_TIME_INACTION_0 = "10000";
        public const string VEHICLE_CAN_NOT_SERVICE = "10001";
        public const string VEHICLE_BATTERY_LEVEL_IS_LOW = "10002";
        public const string VEHICLE_CAN_NOT_FIND_THE_COUPLER_TO_CHARGING = "10003";
        public const string SEND_TRAN_CMD_TO_VEHICLE_FAIL_0_1 = "10004";

        public const string OHTC_AGVSTATION_RESERVED_TIME_OUT_LINE1_ST01 = "10101";
        public const string OHTC_AGVSTATION_RESERVED_TIME_OUT_LINE1_ST02 = "10102";
        public const string OHTC_AGVSTATION_RESERVED_TIME_OUT_LINE2_ST01 = "10103";
        public const string OHTC_AGVSTATION_RESERVED_TIME_OUT_LINE2_ST02 = "10104";
        public const string OHTC_AGVSTATION_RESERVED_TIME_OUT_LINE2_ST03 = "10105";
        public const string OHTC_AGVSTATION_RESERVED_TIME_OUT_LOOP_ST01 = "10106";
        public const string OHTC_AGVSTATION_RESERVED_TIME_OUT_STOCK_ST01 = "10107";
        public const string OHTC_AGVSTATION_RESERVED_TIME_OUT_LINE3_ST01 = "10108";
        public const string OHTC_AGVSTATION_RESERVED_TIME_OUT_LINE3_ST02 = "10109";
        public const string OHTC_AGVSTATION_RESERVED_TIME_OUT_LINE3_ST03 = "10110";

        public const string OHTC_OUT_OF_STOCK_TIME_OUT_LINE1_ST01 = "10111";
        public const string OHTC_OUT_OF_STOCK_TIME_OUT_LINE1_ST02 = "10112";
        public const string OHTC_OUT_OF_STOCK_TIME_OUT_LINE2_ST01 = "10113";
        public const string OHTC_OUT_OF_STOCK_TIME_OUT_LINE2_ST02 = "10114";
        public const string OHTC_OUT_OF_STOCK_TIME_OUT_LINE2_ST03 = "10115";
        public const string OHTC_OUT_OF_STOCK_TIME_OUT_LOOP_ST01 = "10116";
        public const string OHTC_OUT_OF_STOCK_TIME_OUT_STOCK_ST01 = "10117";
        public const string OHTC_OUT_OF_STOCK_TIME_OUT_LINE3_ST01 = "10118";
        public const string OHTC_OUT_OF_STOCK_TIME_OUT_LINE3_ST02 = "10119";
        public const string OHTC_OUT_OF_STOCK_TIME_OUT_LINE3_ST03 = "10120";

        public const string OHTC_TRAN_COMMAND_IN_QUEUE_TIME_OUT = "10121";

        public const string OHTC_CHARGER_HP_NOT_SAFETY = "10201";
        public const string OHTC_VEHICLE_DISCONNECTED = "65534";
        public const string OHTC_VEHICLE_MANUAL = "65535";

        public const string PORT_STATUS_DOWN = "20000";
        public const string PORT_WAITOUT_ID_EMPTY = "20001";
        public const string INTERLOCK_ERROR_OCCURED = "30000";

        /// <summary>
        /// The sc application
        /// </summary>
        private SCApplication scApp = null;
        /// <summary>
        /// The logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The alarm DAO
        /// </summary>
        private AlarmDao alarmDao = null;
        /// <summary>
        /// The line DAO
        /// </summary>
        private LineDao lineDao = null;
        /// <summary>
        /// The alarm RPT cond DAO
        /// </summary>
        private AlarmRptCondDao alarmRptCondDao = null;
        /// <summary>
        /// The alarm map DAO
        /// </summary>
        private AlarmMapDao alarmMapDao = null;
        private MainAlarmDao mainAlarmDao = null;
        /// <summary>
        /// Initializes a new instance of the <see cref="AlarmBLL"/> class.
        /// </summary>
        public AlarmBLL()
        {

        }

        /// <summary>
        /// Starts the specified sc application.
        /// </summary>
        /// <param name="scApp">The sc application.</param>
        public void start(SCApplication scApp)
        {
            this.scApp = scApp;
            alarmDao = scApp.AlarmDao;
            lineDao = scApp.LineDao;
            alarmRptCondDao = scApp.AlarmRptCondDao;
            alarmMapDao = scApp.AlarmMapDao;
            mainAlarmDao = scApp.MainAlarmDao;
        }

        #region Alarm Map
        //public AlarmMap getAlarmMap(string eqpt_real_id, string alarm_id)
        //{
        //    DBConnection conn = null;
        //    AlarmMap alarmMap = null;
        //    try
        //    {
        //        conn = scApp.getDBConnection();
        //        conn.BeginTransaction();

        //        alarmMap = alarmMapDao.getAlarmMap(conn, eqpt_real_id, alarm_id);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Warn("getAlarmMap Exception!", ex);
        //    }
        //    return alarmMap;
        //}
        public List<AlarmMap> loadAlarmMaps()
        {
            List<AlarmMap> alarmMaps = alarmMapDao.loadAlarmMaps();
            return alarmMaps;
        }
        #endregion Alarm Map

        public (bool isExist, AlarmMap map) tryGetChargerAlarmMap(string errorCode)
        {
            var alarm_maps = alarmMapDao.loadAlarmMaps();
            var alarm_map = alarm_maps.Where(map => map.EQPT_REAL_ID.Contains("Charger") && SCUtility.isMatche(map.ALARM_ID, errorCode)).FirstOrDefault();
            return (alarm_map != null, alarm_map);
        }

        public (bool isExist, AlarmMap map) tryGetConveyerAlarmMap(string errorCode)
        {
            var alarm_maps = alarmMapDao.loadAlarmMaps();
            var alarm_map = alarm_maps.Where(map => map.EQPT_REAL_ID.Contains("Conveyer") && SCUtility.isMatche(map.ALARM_ID, errorCode)).FirstOrDefault();
            return (alarm_map != null, alarm_map);
        }


        object lock_obj_alarm = new object();
        public ALARM setAlarmReport(string node_id, string eq_id, string error_code, string errorDesc)
        {
            return setAlarmReport(node_id, eq_id, error_code, errorDesc, "");
        }
        public ALARM setAlarmReport(string nodeID, string eq_id, string error_code, string errorDesc, string cmd_id)
        {
            lock (lock_obj_alarm)
            {
                if (IsAlarmExist(eq_id, error_code)) return null;
                //AlarmMap alarmMap = alarmMapDao.getAlarmMap(eq_id, error_code);
                AlarmMap alarmMap = null;
                if (!SCUtility.isEmpty(nodeID))
                    alarmMap = alarmMapDao.getAlarmMap(nodeID, error_code);
                else
                    alarmMap = alarmMapDao.getAlarmMap(eq_id, error_code);

                string strNow = BCFUtility.formatDateTime(DateTime.Now, SCAppConstants.TimestampFormat_19);
                ALARM alarm = new ALARM()
                {
                    EQPT_ID = eq_id,
                    RPT_DATE_TIME = DateTime.Now,
                    ALAM_CODE = error_code,
                    ALAM_LVL = alarmMap == null ? E_ALARM_LVL.None : alarmMap.ALARM_LVL,
                    ALAM_STAT = ProtocolFormat.OHTMessage.ErrorStatus.ErrSet,
                    ALAM_DESC = alarmMap == null ? errorDesc : alarmMap.ALARM_DESC,
                    CMD_ID_1 = cmd_id,
                };
                if (SCUtility.isEmpty(alarm.ALAM_DESC))
                {
                    alarm.ALAM_DESC = $"Unknow error:{error_code}";
                }
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    alarmDao.insertAlarm(con, alarm);
                    CheckSetAlarm();
                }

                return alarm;
            }
        }

        public void setAlarmReport2Redis(ALARM alarm)
        {
            if (alarm == null) return;
            string hash_field = $"{alarm.EQPT_ID}_{alarm.ALAM_CODE}";
            scApp.getRedisCacheManager().AddTransactionCondition(StackExchange.Redis.Condition.HashNotExists(SCAppConstants.REDIS_KEY_CURRENT_ALARM, hash_field));
            scApp.getRedisCacheManager().HashSetAsync(SCAppConstants.REDIS_KEY_CURRENT_ALARM, hash_field, JsonConvert.SerializeObject(alarm));
        }

        public List<ALARM> getCurrentAlarmsFromRedis()
        {
            List<ALARM> alarms = new List<ALARM>();
            var redis_values_alarms = scApp.getRedisCacheManager().HashValuesProductOnlyAsync(SCAppConstants.REDIS_KEY_CURRENT_ALARM).Result;
            foreach (string redis_value_alarm in redis_values_alarms)
            {
                ALARM alarm_obj = (ALARM)JsonConvert.DeserializeObject(redis_value_alarm, typeof(ALARM));
                alarms.Add(alarm_obj);
            }
            return alarms;
        }
        public List<ALARM> getCurrentChargerAlarmsFromRedis()
        {
            List<ALARM> alarms = new List<ALARM>();
            var redis_values_alarms = scApp.getRedisCacheManager().HashValuesProductOnlyAsync(SCAppConstants.REDIS_KEY_CURRENT_ALARM).Result;
            foreach (string redis_value_alarm in redis_values_alarms)
            {
                ALARM alarm_obj = (ALARM)JsonConvert.DeserializeObject(redis_value_alarm, typeof(ALARM));
                if (alarm_obj.EQPT_ID.Contains("Charger"))
                    alarms.Add(alarm_obj);
            }
            return alarms;
        }

        public List<ALARM> getCurrentConveyerAlarmsFromRedis()
        {
            List<ALARM> alarms = new List<ALARM>();
            var redis_values_alarms = scApp.getRedisCacheManager().HashValuesProductOnlyAsync(SCAppConstants.REDIS_KEY_CURRENT_ALARM).Result;
            foreach (string redis_value_alarm in redis_values_alarms)
            {
                ALARM alarm_obj = (ALARM)JsonConvert.DeserializeObject(redis_value_alarm, typeof(ALARM));
                if (alarm_obj.EQPT_ID.Contains("Charger"))
                    alarms.Add(alarm_obj);
            }
            return alarms;
        }

        public List<ALARM> GetAlarms(DateTime startTime, DateTime endTime)
        {
            List<ALARM> alarm = null;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                alarm = alarmDao.getAlarms(con, startTime, endTime);
            }
            return alarm;
        }

        public List<ALARM> getCurrentAlarms()
        {
            List<ALARM> alarms = new List<ALARM>();
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                alarms = alarmDao.loadSetAlarm(con);
            }
            return alarms;
        }
        public List<ALARM> getCurrentErrorAlarms()
        {
            List<ALARM> alarms = new List<ALARM>();
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                alarms = alarmDao.loadSetErrorAlarm(con);
            }
            return alarms;
        }

        public List<ALARM> getCurrentMinorAlarms()
        {
            List<ALARM> alarms = new List<ALARM>();
            //using (DBConnection_EF con = new DBConnection_EF())
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                alarms = alarmDao.loadSetMinorAlarm(con);
            }
            return alarms;
        }


        public ALARM resetAlarmReport(string eq_id, string error_code)
        {
            lock (lock_obj_alarm)
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    ALARM alarm = alarmDao.getSetAlarm(con, eq_id, error_code);
                    if (alarm != null)
                    {
                        alarm.ALAM_STAT = ProtocolFormat.OHTMessage.ErrorStatus.ErrReset;
                        alarm.CLEAR_DATE_TIME = DateTime.Now;
                        alarmDao.updateAlarm(con, alarm);
                        CheckSetAlarm();
                    }
                    return alarm;
                }
            }
        }

        public void resetAlarmReport2Redis(ALARM alarm)
        {
            if (alarm == null) return;
            string hash_field = $"{alarm.EQPT_ID.Trim()}_{alarm.ALAM_CODE.Trim()}";
            //scApp.getRedisCacheManager().AddTransactionCondition(StackExchange.Redis.Condition.HashExists(SCAppConstants.REDIS_KEY_CURRENT_ALARM, hash_field));
            scApp.getRedisCacheManager().HashDeleteAsync(SCAppConstants.REDIS_KEY_CURRENT_ALARM, hash_field);
        }


        public List<ALARM> resetAllAlarmReport(string eq_id)
        {
            List<ALARM> alarms = null;
            lock (lock_obj_alarm)
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    alarms = alarmDao.loadSetAlarm(con, eq_id);

                    if (alarms != null)
                    {
                        foreach (ALARM alarm in alarms.ToList())
                        {
                            if (!SCUtility.isMatche(alarm.ALAM_CODE, VEHICLE_ALARM_HAPPEND))
                            {
                                alarm.ALAM_STAT = ProtocolFormat.OHTMessage.ErrorStatus.ErrReset;
                                alarm.CLEAR_DATE_TIME = DateTime.Now;
                                alarmDao.updateAlarm(con, alarm);
                            }
                            else
                            {
                                alarms.Remove(alarm);
                            }
                        }
                        CheckSetAlarm();
                    }
                }
            }
            return alarms;
        }



        public void resetAllAlarmReport2Redis(string vh_id)
        {
            var current_all_alarm = scApp.getRedisCacheManager().HashKeys(SCAppConstants.REDIS_KEY_CURRENT_ALARM);
            var vh_all_alarm = current_all_alarm.Where(redisKey => ((string)redisKey).Contains(vh_id)).ToArray();
            scApp.getRedisCacheManager().HashDeleteAsync(SCAppConstants.REDIS_KEY_CURRENT_ALARM, vh_all_alarm);
        }
        private bool IsAlarmExist(string eq_id, string code)
        {
            bool isExist = false;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                isExist = alarmDao.getSetAlarmCountByEQAndCode(con, eq_id, code) > 0;
            }
            return isExist;
        }
        public bool IsReportToHost(string code)
        {
            bool result = true;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                ALARMRPTCOND cond = alarmRptCondDao.getRptCond(con, code);
                if (cond != null)
                {
                    result = SCUtility.isMatche(cond.ENABLE_FLG, SCAppConstants.YES_FLAG);
                }
            }
            return result;
        }

        public AlarmMap GetAlarmMap(string objID, string errorCode)
        {
            AlarmMap alarmMap = alarmMapDao.getAlarmMap(objID, errorCode);
            return alarmMap;
        }

        public bool hasAlarmExist()
        {
            var redis_values_alarms = scApp.getRedisCacheManager().HashValuesProductOnlyAsync(SCAppConstants.REDIS_KEY_CURRENT_ALARM).Result;
            if (redis_values_alarms.Count() > 0)
            {
                return true;
            }
            return false;
        }



        public bool enableAlarmReport(string alarm_id, Boolean isEnable)
        {
            bool isSuccess = true;
            try
            {
                string enable_flag = (isEnable ? SCAppConstants.YES_FLAG : SCAppConstants.NO_FLAG);

                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    ALARMRPTCOND cond = null;
                    cond = alarmRptCondDao.getRptCond(con, alarm_id);
                    if (cond != null)
                    {
                        cond.ENABLE_FLG = enable_flag;
                        alarmRptCondDao.insertRptCond(con, cond);
                    }
                    else
                    {
                        cond = new ALARMRPTCOND()
                        {
                            ALAM_CODE = alarm_id,
                            ENABLE_FLG = enable_flag
                        };
                        alarmRptCondDao.insertRptCond(con, cond);
                    }
                }
            }
            catch (Exception ex)
            {
                isSuccess = true;
                logger.Error(ex, "Exception");
            }
            return isSuccess;
        }

        public string onMainAlarm(string mAlarmCode, params object[] args)
        {
            MainAlarm mainAlarm = mainAlarmDao.getMainAlarmByCode(mAlarmCode);
            bool isAlarm = false;
            string msg = string.Empty;
            try
            {
                if (mainAlarm != null)
                {
                    isAlarm = mainAlarm.CODE.StartsWith("A");
                    msg = string.Format(mainAlarm.DESCRIPTION, args);
                    if (isAlarm)
                    {
                        msg = string.Format("[{0}]{2}", mainAlarm.CODE, Environment.NewLine, msg);
                        BCFApplication.onErrorMsg(msg);
                    }
                    else
                    {
                        msg = string.Format("[{0}]{2}", mainAlarm.CODE, Environment.NewLine, msg);
                        BCFApplication.onWarningMsg(msg);
                    }
                }
                else
                {
                    logger.Warn(string.Format("LFC alarm/warm happen, but no defin remark code:[{0}] !!!", mAlarmCode));
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
            return msg;
        }

        object lock_obj_alarm_happen = new object();
        public void CheckSetAlarm()
        {
            lock (lock_obj_alarm_happen)
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    ALINE line = scApp.getEQObjCacheManager().getLine();
                    List<ALARM> alarmLst = alarmDao.loadSetAlarm(con);

                    if (alarmLst != null && alarmLst.Count > 0)
                    {
                        line.IsAlarmHappened = true;
                    }
                    else
                    {
                        line.IsAlarmHappened = false;
                    }
                }
            }
        }



    }
}
