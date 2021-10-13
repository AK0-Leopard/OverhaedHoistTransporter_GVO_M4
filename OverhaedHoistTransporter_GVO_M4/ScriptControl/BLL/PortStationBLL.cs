using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.Data.DAO;
using com.mirle.ibg3k0.sc.ProtocolFormat.SystemClass.PortInfo;
using NLog;
using STAN.Client;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class PortStationBLL
    {
        public DB OperateDB { private set; get; }
        public Catch OperateCatch { private set; get; }
        public Redis redis { private set; get; }
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public PortStationBLL()
        {
        }
        public void start(SCApplication _app)
        {
            OperateDB = new DB(_app.PortStationDao);
            OperateCatch = new Catch(_app.EqptBLL, _app.getEQObjCacheManager());
            redis = new Redis(_app.getRedisCacheManager());
        }

        const double PORT_INFO_TIME_OUT_SEC = 10;
        public void updatePortStatusByRedis()
        {
            var ports_plc_info = redis.getCurrentPortsInfo();
            var sb = new StringBuilder();
            ports_plc_info.ForEach(info => sb.AppendLine(info.ToString()));
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(PortStationBLL), Device: "OHTC",
               Data: $"port infos:{sb}");

            var port_stations = OperateCatch.loadAGVPortStation();
            foreach (var port_station in port_stations)
            {
                var port_plc_info = ports_plc_info.Where(port => SCUtility.isMatche(port.PortID, port_station.PORT_ID)).
                                                   FirstOrDefault();
                if (port_plc_info != null)
                {
                    //port_station.SetPortInfo(port_plc_info);
                }
                else
                {
                    //port_station.ResetPortInfo();
                }

            }
        }
        public class DB
        {
            PortStationDao portStationDao = null;
            public DB(PortStationDao _portStationDao)
            {
                portStationDao = _portStationDao;
            }
            public APORTSTATION get(string _id)
            {
                APORTSTATION rtnPortStation = null;
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    rtnPortStation = portStationDao.getByID(con, _id);
                }
                return rtnPortStation;
            }
            public List<APORTSTATION> loadAll()
            {
                List<APORTSTATION> rtnPortStations = null;
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    rtnPortStations = portStationDao.loadAll(con);
                }
                return rtnPortStations;
            }

            public List<APORTSTATION> loadAllNonEQPort()
            {
                List<APORTSTATION> rtnPortStations = null;
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    rtnPortStations = portStationDao.loadAllNonEQPort(con);
                }
                return rtnPortStations;
            }

            public bool isPortStationEQPort(string id)
            {
                APORTSTATION portStation = null;
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    portStation = portStationDao.getByID(con, id);
                    if (portStation == null)
                    {
                        portStation = portStationDao.getByAdrID(con, id);
                    }
                }
                if (portStation != null)
                {
                    if(portStation.PORT_TYPE == 1)// O: Non EQ Port, 1: EQ Port
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            public bool isPortStation(string id)
            {
                APORTSTATION portStation = null;
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    portStation = portStationDao.getByID(con, id);
                    if (portStation == null)
                    {
                        portStation = portStationDao.getByAdrID(con, id);
                    }
                }
                if (portStation != null)
                {

                    return true;
                }
                else
                {
                    return false;
                }
            }

            public int getAPortstationStageCountByID(string id)
            {
                return portStationDao.getAPortstationStageCountByID(id);
            }


            public bool add(APORTSTATION portStation)
            {
                try
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        portStationDao.add(con, portStation);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                    return false;
                }
                return true;
            }

            public bool updatePriority(string portID, int priority)
            {
                try
                {
                    APORTSTATION port_statino = new APORTSTATION();
                    port_statino.PORT_ID = portID;
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        con.APORTSTATION.Attach(port_statino);
                        port_statino.PRIORITY = priority;

                        con.Entry(port_statino).Property(p => p.PRIORITY).IsModified = true;

                        portStationDao.update(con, port_statino);
                        con.Entry(port_statino).State = EntityState.Detached;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                    return false;
                }
                return true;
            }

            public bool updateServiceStatus(string portID, ProtocolFormat.OHTMessage.PortStationServiceStatus status)
            {
                try
                {
                    APORTSTATION port_statino = new APORTSTATION();
                    port_statino.PORT_ID = portID;
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        con.APORTSTATION.Attach(port_statino);
                        port_statino.PORT_SERVICE_STATUS = status;

                        con.Entry(port_statino).Property(p => p.PORT_SERVICE_STATUS).IsModified = true;

                        portStationDao.update(con, port_statino);
                        con.Entry(port_statino).State = EntityState.Detached;
                    }
                }
                catch
                {
                    return false;
                }
                return true;
            }
            public bool updatePortStatus(string portID, ProtocolFormat.OHTMessage.PortStationStatus status)
            {
                try
                {
                    APORTSTATION port_statino = new APORTSTATION();
                    port_statino.PORT_ID = portID;
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        con.APORTSTATION.Attach(port_statino);
                        port_statino.PORT_STATUS = status;

                        con.Entry(port_statino).Property(p => p.PORT_STATUS).IsModified = true;

                        portStationDao.update(con, port_statino);
                        con.Entry(port_statino).State = EntityState.Detached;
                    }
                }
                catch
                {
                    return false;
                }
                return true;
            }

            public bool updatePortDir(string portID,int dir)
            {
                try
                {
                    APORTSTATION port_statino = new APORTSTATION();
                    port_statino.PORT_ID = portID;
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        con.APORTSTATION.Attach(port_statino);
                        port_statino.PORT_DIR = dir;

                        con.Entry(port_statino).Property(p => p.PORT_DIR).IsModified = true;

                        portStationDao.update(con, port_statino);
                        con.Entry(port_statino).State = EntityState.Detached;
                    }
                }
                catch
                {
                    return false;
                }
                return true;
            }
            public bool updatePortUnloadVhType(string portID, E_VH_TYPE vhType)
            {
                try
                {
                    APORTSTATION port_statino = new APORTSTATION();
                    port_statino.PORT_ID = portID;
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        con.APORTSTATION.Attach(port_statino);
                        port_statino.ULD_VH_TYPE = vhType;

                        con.Entry(port_statino).Property(p => p.ULD_VH_TYPE).IsModified = true;

                        portStationDao.update(con, port_statino);
                        con.Entry(port_statino).State = EntityState.Detached;
                    }
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }
        public class Catch
        {
            private readonly EqptBLL eqptBLL;
            EQObjCacheManager CacheManager;
            public Catch(EqptBLL eqptBLL, EQObjCacheManager _cache_manager)
            {
                this.eqptBLL = eqptBLL;
                CacheManager = _cache_manager;
            }
            public List<APORTSTATION> loadAllPortStation()
            {
                return CacheManager.getALLPortStation();
            }
            public List<APORTSTATION> loadAllEQPortStation()
            {
                return CacheManager.getALLEQPortStation();
            }
            public List<APORTSTATION> loadAllPortStation(string zoneID)
            {
                var port_stations = CacheManager.getALLPortStation();
                return port_stations.Where(port => SCUtility.isMatche(port.ZONE_ID, zoneID)).
                                     ToList();
            }
            public List<APORTSTATION> loadAGVPortStation()
            {
                var port_stations = CacheManager.getALLPortStation();
                return port_stations.Where(port => port.GetEqptType(eqptBLL) == SCAppConstants.EqptType.AGVStation).
                                                        ToList();
            }
            public List<APORTSTATION> loadAGVPortStation(string zoneID)
            {
                var port_stations = CacheManager.getALLPortStation();
                return port_stations.Where(port => port.GetEqptType(eqptBLL) == SCAppConstants.EqptType.AGVStation &&
                                                   SCUtility.isMatche(port.ZONE_ID, zoneID)).
                                     ToList();
            }
            public List<APORTSTATION> loadAGVPortStationCanUnload(CMDBLL cmdBLL, string zoneID)
            {
                var port_stations = loadAGVPortStation();
                return port_stations.Where(port => port.IsInPutMode &&
                                                   port.PortReady &&
                                                   //!port.CSTPresenceMismatch &&
                                                   !port.IsCSTPresenceLoc1 &&
                                                   SCUtility.isMatche(port.ZONE_ID, zoneID) &&
                                                   !cmdBLL.hasExcuteCMDByDestinationPort(port.PORT_ID)).ToList();
            }
            public APORTSTATION getAGVPortStationCanUnloadForCycleRun(CMDBLL cmdBLL, string zoneID)
            {
                var port_stations = loadAGVPortStation();
                return port_stations.Where(port => port.IncludeCycleTest &&
                                                   port.IsInPutMode &&
                                                   port.PortReady &&
                                                   //!port.CSTPresenceMismatch &&
                                                   !port.IsCSTPresenceLoc1 &&
                                                   SCUtility.isMatche(port.ZONE_ID, zoneID) &&
                                                   !cmdBLL.hasExcuteCMDByDestinationPort(port.PORT_ID)).FirstOrDefault();
            }
            public List<APORTSTATION> loadAGVPortStationCanUnload()
            {
                var port_stations = CacheManager.getALLPortStation();
                return port_stations.Where(port => port.IsInPutMode &&
                                                   port.PortReady &&
                                                   //!port.CSTPresenceMismatch &&
                                                   !port.IsCSTPresenceLoc1).ToList();
            }
            public APORTSTATION getAGVPortStationWaitOutOK(CMDBLL cmdBLL, string zoneID)
            {
                var port_stations = loadAGVPortStation();
                return port_stations.Where(port => port.IncludeCycleTest &&
                                                   port.IsOutPutMode &&
                                                   port.PortReady &&
                                                   port.PortWaitOut &&
                                                   SCUtility.isMatche(port.ZONE_ID, zoneID) &&
                                                   !cmdBLL.hasExcuteCMDBySourcePort(port.PORT_ID)).FirstOrDefault();
            }
            public List<APORTSTATION> getHasCSTPortStation()
            {
                var port_stations = CacheManager.getALLPortStation();
                return port_stations.Where(port_station => !SCUtility.isEmpty(port_station.CST_ID)).ToList();
            }
            public APORTSTATION getPortStation(string port_id)
            {
                APORTSTATION portTemp = CacheManager.getPortStation(port_id);
                return portTemp;
            }
            public List<APORTSTATION> getPortStationByAdrID(string adrID)
            {
                if (SCUtility.isEmpty(adrID)) return null;
                List<APORTSTATION> portTemp = CacheManager.getALLPortStation().Where(port_station => port_station.ADR_ID.Trim() == adrID.Trim()).
                                                                         ToList();
                return portTemp;
            }

            public APORTSTATION getPortStationByAdrIDSingle(string adrID)
            {
                if (SCUtility.isEmpty(adrID)) return null;
                APORTSTATION portTemp = CacheManager.getALLPortStation().Where(port_station => port_station.ADR_ID.Trim() == adrID.Trim()).FirstOrDefault();
                return portTemp;
            }

            public void updatePortStationCSTExistStatus(string port_id, string cst_id)
            {
                APORTSTATION port_station = CacheManager.getPortStation(port_id);
                if (port_station != null)
                {
                    port_station.CST_ID = cst_id;
                }
            }
            public bool IsExist(string portID)
            {
                APORTSTATION port_station = CacheManager.getPortStation(portID);
                return port_station != null;
            }
            public bool IsEqPort(EqptBLL eqptBLL, string portID)
            {
                APORTSTATION port_station = CacheManager.getPortStation(portID);
                if (port_station == null) return false;
                return port_station.GetEqptType(eqptBLL) == SCAppConstants.EqptType.Equipment;
            }
            public bool IsAGVStationPort(EqptBLL eqptBLL, string portID)
            {
                APORTSTATION port_station = CacheManager.getPortStation(portID);
                if (port_station == null) return false;
                return port_station.GetEqptType(eqptBLL) == SCAppConstants.EqptType.AGVStation;
            }

            public bool updatePriority(string portID, int priority)
            {
                try
                {
                    APORTSTATION port_station = CacheManager.getPortStation(portID);
                    port_station.PRIORITY = priority;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                    return false;
                }
                return true;
            }

            public bool updateServiceStatus(string portID, ProtocolFormat.OHTMessage.PortStationServiceStatus status)
            {
                try
                {
                    APORTSTATION port_station = CacheManager.getPortStation(portID);
                    port_station.PORT_SERVICE_STATUS = status;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                    return false;
                }
                return true;
            }
            public bool updatePortStatus(string portID, ProtocolFormat.OHTMessage.PortStationStatus status)
            {
                try
                {
                    APORTSTATION port_station = CacheManager.getPortStation(portID);
                    port_station.PORT_STATUS = status;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                    return false;
                }
                return true;
            }
            public bool updatePortUnloadVhType(string portID, E_VH_TYPE vhType)
            {
                try
                {
                    APORTSTATION port_station = CacheManager.getPortStation(portID);
                    port_station.ULD_VH_TYPE = vhType;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                    return false;
                }
                return true;
            }
            public bool updatePortIncludeCycleTest(string portID, bool isIncludeCycleTest)
            {
                try
                {
                    APORTSTATION port_station = CacheManager.getPortStation(portID);
                    port_station.IncludeCycleTest = isIncludeCycleTest;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                    return false;
                }
                return true;
            }


            public static byte[] convert2PortstationInfo(string portID, APORTSTATION por)
            {
                //ProtocolFormat.OHTMessage.PORT_INFO port_info = new PORT_INFO();
                //port_info.IsAutoMode = portPLCInfo.IsAutoMode;
                //port_info.OpAutoMode = portPLCInfo.OpAutoMode;
                //port_info.OpManualMode = portPLCInfo.OpManualMode;
                //port_info.OpError = portPLCInfo.OpError;
                //port_info.IsInputMode = portPLCInfo.IsInputMode;
                //port_info.IsOutputMode = portPLCInfo.IsOutputMode;
                //port_info.IsModeChangable = portPLCInfo.IsModeChangable;
                //port_info.IsAGVMode = portPLCInfo.IsAGVMode;
                //port_info.IsMGVMode = portPLCInfo.IsMGVMode;
                //port_info.PortWaitIn = portPLCInfo.PortWaitIn;
                //port_info.PortWaitOut = portPLCInfo.PortWaitOut;
                //port_info.IsReadyToLoad = portPLCInfo.IsReadyToLoad;
                //port_info.IsReadyToUnload = portPLCInfo.IsReadyToUnload;
                //port_info.LoadPosition1 = portPLCInfo.LoadPosition1;
                //port_info.LoadPosition2 = portPLCInfo.LoadPosition2;
                //port_info.LoadPosition3 = portPLCInfo.LoadPosition3;
                //port_info.LoadPosition4 = portPLCInfo.LoadPosition4;
                //port_info.LoadPosition5 = portPLCInfo.LoadPosition5;
                //port_info.LoadPosition7 = portPLCInfo.LoadPosition7;
                //port_info.LoadPosition6 = portPLCInfo.LoadPosition6;
                //port_info.IsCSTPresence = portPLCInfo.IsCSTPresence;
                //port_info.AGVPortReady = portPLCInfo.AGVPortReady;
                //port_info.CanOpenBox = portPLCInfo.CanOpenBox;
                //port_info.IsBoxOpen = portPLCInfo.IsBoxOpen;
                //port_info.BCRReadDone = portPLCInfo.BCRReadDone;
                //port_info.CSTPresenceMismatch = portPLCInfo.CSTPresenceMismatch;
                //port_info.IsTransferComplete = portPLCInfo.IsTransferComplete;
                //port_info.CstRemoveCheck = portPLCInfo.CstRemoveCheck;
                //port_info.ErrorCode = portPLCInfo.ErrorCode;
                //port_info.BoxID = portPLCInfo.BoxID;
                //port_info.CassetteID = portPLCInfo.CassetteID;
                //port_info.PortID = portID;

                //port_info.LoadPositionBOX1 = portPLCInfo.LoadPositionBOX1;
                //port_info.LoadPositionBOX2 = portPLCInfo.LoadPositionBOX2;
                //port_info.LoadPositionBOX3 = portPLCInfo.LoadPositionBOX3;
                //port_info.LoadPositionBOX4 = portPLCInfo.LoadPositionBOX4;
                //port_info.LoadPositionBOX5 = portPLCInfo.LoadPositionBOX5;
                //port_info.FireAlarm = portPLCInfo.FireAlarm;

                //byte[] arrayByte = new byte[port_info.CalculateSize()];
                byte[] arrayByte = new byte[0];
                //port_info.WriteTo(new Google.Protobuf.CodedOutputStream(arrayByte));
                return arrayByte;
            }

        }

        public class Redis
        {
            private readonly RedisCacheManager redisCacheManager;

            public Redis(RedisCacheManager redisCacheManager)
            {
                this.redisCacheManager = redisCacheManager;
            }

            public List<PORT_INFO> getCurrentPortsInfo()
            {
                var redis_values_ports = redisCacheManager.HashValuesAsync(SCAppConstants.REDIS_KEY_CURRENT_PORTS_INFO).Result;
                var ports_info = redis_values_ports.Select(info_raw_data => Convert2Object_PortInfo(info_raw_data)).ToList();
                return ports_info;
            }

            public static PORT_INFO Convert2Object_PortInfo(byte[] raw_data)
            {
                return ToObject<PORT_INFO>(raw_data);
            }
            private static T ToObject<T>(byte[] buf) where T : Google.Protobuf.IMessage<T>, new()
            {
                if (buf == null)
                    return default(T);
                Google.Protobuf.MessageParser<T> parser = new Google.Protobuf.MessageParser<T>(() => new T());
                return parser.ParseFrom(buf);
            }


        }
    }
}