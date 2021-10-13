//*********************************************************************************
//      SCApplication.cs
//*********************************************************************************
// File Name: SCApplication.cs
// Description: SCApplication
//
//(c) Copyright 2014, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
//**********************************************************************************

using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.bcf.ConfigHandler;
using com.mirle.ibg3k0.bcf.Controller;
using com.mirle.ibg3k0.bcf.Data.InitAction;
using com.mirle.ibg3k0.bcf.Data.ValueDefMapAction;
using com.mirle.ibg3k0.bcf.Data.VO;
using com.mirle.ibg3k0.bcf.Schedule;
using com.mirle.ibg3k0.sc.BLL;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.ConfigHandler;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.Data.DAO;
using com.mirle.ibg3k0.sc.Data.PLC_Functions;
using com.mirle.ibg3k0.sc.Data.SECS;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.Module;
using com.mirle.ibg3k0.sc.MQTT;
using com.mirle.ibg3k0.sc.RouteKit;
using com.mirle.ibg3k0.sc.Scheduler;
using com.mirle.ibg3k0.sc.Service;
using com.mirle.ibg3k0.sc.WIF;
using com.mirle.ibg3k0.stc.Common.SECS;
using ExcelDataReader;
using GenericParsing;
using Nancy;
using Nancy.Hosting.Self;
using NLog;
using Predes.ZabbixSender;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace com.mirle.ibg3k0.sc.App
{
    public class SCApplication
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private EQPTConfigSections eqptCss = null;
        private NodeFlowRelConfigSections nodeFlowRelCss = null;

        private DataCollectionConfigSections dataCollectionCss = null;
        private string eapSecsAgentName;
        public string EAPSecsAgentName { get { return eapSecsAgentName; } }
        public string BC_ID { get; private set; }
        public static string ServerName { get; private set; }

        public int MaxCommandCount { get; private set; }
        public int MaxCommandCountForSwap { get; private set; }

        private static Object _lock = new Object();
        private static SCApplication application;
        private static BCFApplication bcfApplication;


        private EQObjCacheManager eqObjCacheManager;
        private CommObjCacheManager commObjCacheManager;
        private RedisCacheManager redisCacheManager;
        private NatsManager natsManager;
        //private Mirle.Hlts.ReserveSection.Map.MapAPI _reserveSectionAPI { get; set; }
        //private Mirle.Hlts.ReserveSection.Map.ViewModels.HltMapViewModel reserveSectionAPI { get; set; }

        public HAProxyConnectionTest hAProxyConnectionTest { get; private set; }
        public NancyHost NancyHost { get; private set; }
        public WebClientManager webClientManager { get; private set; }



        //DAO
        private LineDao lineDao = null;
        public LineDao LineDao { get { return lineDao; } }
        private ZoneDao zoneDao = null;
        public ZoneDao ZoneDao { get { return zoneDao; } }
        private NodeDao nodeDao = null;
        public NodeDao NodeDao { get { return nodeDao; } }
        private EqptDao eqptDao = null;
        public EqptDao EqptDao { get { return eqptDao; } }
        private FlowRelDao flowRelDao = null;
        public FlowRelDao FlowRelDao { get { return flowRelDao; } }
        private UnitDao unitDao = null;
        public UnitDao UnitDao { get { return unitDao; } }
        private PortDao portDao = null;
        public PortDao PortDao { get { return portDao; } }
        private PortStationDao portStationDao = null;
        public PortStationDao PortStationDao { get { return portStationDao; } }
        private BufferPortDao bufferPortDao = null;
        public BufferPortDao BufferPortDao { get { return bufferPortDao; } }
        private UserDao userDao = null;
        public UserDao UserDao { get { return userDao; } }
        private FunctionCodeDao functionCodeDao = null;
        public FunctionCodeDao FunctionCodeDao { get { return functionCodeDao; } }
        private UserFuncDao userFuncDao = null;
        public UserFuncDao UserFuncDao { get { return userFuncDao; } }
        private AlarmDao alarmDao = null;
        public AlarmDao AlarmDao { get { return alarmDao; } }

        private MainAlarmDao mainalarmDao = null;
        public MainAlarmDao MainAlarmDao { get { return mainalarmDao; } }



        private CassetteDao cassetteDao = null;
        public CassetteDao CassetteDao { get { return cassetteDao; } }
        private BCStatusDao bcStatusDao = null;
        public BCStatusDao BCStatusDao { get { return bcStatusDao; } }
        private SequenceDao sequenceDao = null;
        public SequenceDao SequenceDao { get { return sequenceDao; } }
        private EventRptCondDao eventRptCondDao = null;
        public EventRptCondDao EventRptCondDao { get { return eventRptCondDao; } }
        private CrateDao crateDao = null;
        public CrateDao CrateDao { get { return crateDao; } }
        private AlarmRptCondDao alarmRptCondDao = null;
        public AlarmRptCondDao AlarmRptCondDao { get { return alarmRptCondDao; } }
        private TraceSetDao traceSetDao = null;
        public TraceSetDao TraceSetDao { get { return traceSetDao; } }
        public OperationHisDao OperationHisDao { get; private set; } = null;


        public ReturnCodeMapDao ReturnCodeMapDao { get; private set; } = null;





        private AlarmMapDao alarmMapDao = null;
        public AlarmMapDao AlarmMapDao { get { return alarmMapDao; } }
        private UserGroupDao userGroupDao = null;
        public UserGroupDao UserGroupDao { get { return userGroupDao; } }
        private ECDataMapDao ecDataMapDao = null;
        public ECDataMapDao ECDataMapDao { get { return ecDataMapDao; } }
        private CEIDDao ceidDao = null;
        public CEIDDao CEIDDao { get { return ceidDao; } }
        private RPTIDDao rptidDao = null;
        public RPTIDDao RPTIDDao { get { return rptidDao; } }



        private RAILDao railDao = null;
        public RAILDao RailDao { get { return railDao; } }
        private ADDRESSDao addressDao = null;
        public ADDRESSDao AddressDao { get { return addressDao; } }
        private PortIconDao porticonDao = null;
        public PortIconDao PortIconDao { get { return porticonDao; } }

        private POINTDao pointDao = null;
        public POINTDao PointDao { get { return pointDao; } }
        private GROUPRAILSDao groupRailDao = null;
        public GROUPRAILSDao GroupRailDao { get { return groupRailDao; } }

        private SectionDao sectionDao = null;
        public SectionDao SectionDao { get { return sectionDao; } }
        private SegmentDao segmentDao = null;
        public SegmentDao SegmentDao { get { return segmentDao; } }
        private VehicleDao vehicleDao = null;
        public VehicleDao VehicleDao { get { return vehicleDao; } }

        private CMDDao cmdDao = null;
        public CMDDao CMDCDao { get { return cmdDao; } }
        private CMD_DetailDao cmd_detailDao = null;
        public CMD_DetailDao CMD_DetailDao { get { return cmd_detailDao; } }


        private BlockZoneDetailDao bolckZoneDetaiDao = null;
        public BlockZoneDetailDao BolckZoneDetaiDao { get { return bolckZoneDetaiDao; } }
        private BlockZoneMasterDao blockZoneMasterDao = null;
        public BlockZoneMasterDao BlockZoneMasterDao { get { return blockZoneMasterDao; } }
        private BlockZoneQueueDao blockZoneQueueDao = null;
        public BlockZoneQueueDao BlockZoneQueueDao { get { return blockZoneQueueDao; } }


        private ParkZoneDetailDao parkZoneDetailDao = null;
        public ParkZoneDetailDao ParkZoneDetailDao { get { return parkZoneDetailDao; } }
        private ParkZoneMasterDao parkZoneMasterDao = null;
        public ParkZoneMasterDao ParkZoneMasterDao { get { return parkZoneMasterDao; } }
        private ParkZoneTypeDao parkZoneTypeDao = null;
        public ParkZoneTypeDao ParkZoneTypeDao { get { return parkZoneTypeDao; } }


        private CycleZoneDetailDao cyclezoneDdetailDao = null;
        public CycleZoneDetailDao CycleZoneDetailDao { get { return cyclezoneDdetailDao; } }
        private CycleZoneMasterDao cyclezonemasterDao = null;
        public CycleZoneMasterDao CycleZoneMasterDao { get { return cyclezonemasterDao; } }
        private CycleZoneTypeDao cyclezonetypeDao = null;
        public CycleZoneTypeDao CycleZoneTypeDao { get { return cyclezonetypeDao; } }
        private TransferDao transferDao = null;
        public TransferDao TransferDao { get { return transferDao; } }
        private VTransferDao vtransferDao = null;
        public VTransferDao VTransferDao { get { return vtransferDao; } }
        private HCMDDao hcmdDao = null;
        public HCMDDao HCMDDao { get { return hcmdDao; } }
        private HTransferDao htransferDao = null;
        public HTransferDao HTransferDao { get { return htransferDao; } }
        private NetworkQualityDao networkqualityDao = null;
        public NetworkQualityDao NetworkQualityDao { get { return networkqualityDao; } }
        private APSettingDao apsettiongDao = null;
        public APSettingDao APSettingDao { get { return apsettiongDao; } }
        private SysExcuteQualityDao sysexcutequalityDao = null;
        public SysExcuteQualityDao SysExcuteQualityDao { get { return sysexcutequalityDao; } }
        private MCSReportQueueDao mcsreportqueueDao = null;
        public MCSReportQueueDao MCSReportQueueDao { get { return mcsreportqueueDao; } }
        private FlexsimCommandDao flexsimcommandDao = null;
        public FlexsimCommandDao FlexsimCommandDao { get { return flexsimcommandDao; } }
        private VehicleMapDao vehicleMapDao = null;
        public VehicleMapDao VehicleMapDao { get { return vehicleMapDao; } }


        private AddressDataDao addressDataDao = null;
        public AddressDataDao AddressDataDao { get { return addressDataDao; } }
        private ScaleBaseDataDao scaleBaseDataDao = null;
        public ScaleBaseDataDao ScaleBaseDataDao { get { return scaleBaseDataDao; } }
        private ControlDataDao controlDataDao = null;
        public ControlDataDao ControlDataDao { get { return controlDataDao; } }
        private VehicleControlDao vehicleControlDao = null;
        public VehicleControlDao VehicleControlDao { get { return vehicleControlDao; } }
        private DataCollectionDao dataCollectionDao = null;
        public DataCollectionDao DataCollectionDao { get { return dataCollectionDao; } }

        private HIDZoneMasterDao hidzonemasterDao = null;
        public HIDZoneMasterDao HIDZoneMasterDao { get { return hidzonemasterDao; } }
        private HIDZoneDetailDao hidzonedetailDao = null;
        public HIDZoneDetailDao HIDZoneDetailDao { get { return hidzonedetailDao; } }
        private HIDZoneQueueDao hidzonequeueDao = null;
        public HIDZoneQueueDao HIDZoneQueueDao { get { return hidzonequeueDao; } }

        private TestTranTaskDao testtrantaskDao = null;
        public TestTranTaskDao TestTranTaskDao { get { return testtrantaskDao; } }

        private CouplerInfoDao couplerInfoDao = null;
        public CouplerInfoDao CouplerInfoDao { get { return couplerInfoDao; } }

        private FireDoorDao fireDoorDao = null;
        public FireDoorDao FireDoorDao { get { return fireDoorDao; } }

        private MTLInfoDao mtlInfoDao = null;
        public MTLInfoDao MTLInfoDao { get { return mtlInfoDao; } }

        private ReserveEnhanceInfoDao reserveEnhanceInfoDao = null;
        public ReserveEnhanceInfoDao ReserveEnhanceInfoDao { get { return reserveEnhanceInfoDao; } }
        private TrafficControlInfoDao trafficControlInfoDao = null;
        public TrafficControlInfoDao TrafficControlInfoDao { get { return trafficControlInfoDao; } }
        private CarrierDao carrierDao = null;
        public CarrierDao CarrierDao { get { return carrierDao; } }
        private GroupPortStationDao groupPortStationDao = null;
        public GroupPortStationDao GroupPortStationDao { get { return groupPortStationDao; } }
        private ZoneDefDao zonedefDao = null;
        public ZoneDefDao ZoneDefDao { get { return zonedefDao; } }
        private ShelfDefDao shelfdefDao = null;
        public ShelfDefDao ShelfDefDao { get { return shelfdefDao; } }
        //BLL
        private UserBLL userBLL = null;
        public UserBLL UserBLL { get { return userBLL; } }
        private BCSystemBLL bcSystemBLL = null;
        public BCSystemBLL BCSystemBLL { get { return bcSystemBLL; } }
        public LineBLL LineBLL { get { return lineBLL; } }
        private LineBLL lineBLL = null;
        public AlarmBLL AlarmBLL { get { return alarmBLL; } }
        private AlarmBLL alarmBLL = null;
        private SequenceBLL sequenceBLL = null;
        public SequenceBLL SequenceBLL { get { return sequenceBLL; } }
        private EventBLL eventBLL = null;
        public EventBLL EventBLL { get { return eventBLL; } }

        private ReportBLL reportBLL = null;
        public ReportBLL ReportBLL { get { return reportBLL; } }

        private MapBLL mapBLL = null;
        public MapBLL MapBLL { get { return mapBLL; } }

        public AddressesBLL AddressesBLL { get; private set; }
        public SectionBLL SectionBLL { get; private set; }
        public SegmentBLL SegmentBLL { get; private set; }
        public UnitBLL UnitBLL { get; private set; }


        private VehicleBLL vehicleBLL = null;
        public VehicleBLL VehicleBLL { get { return vehicleBLL; } }
        private CMDBLL cmdBLL = null;
        public CMDBLL CMDBLL { get { return cmdBLL; } }
        private CycleRunBLL cycleBLL = null;
        public CycleRunBLL CycleBLL { get { return cycleBLL; } }
        private CEIDBLL ceidBLL = null;
        public CEIDBLL CEIDBLL { get { return ceidBLL; } }
        private NetworkQualityBLL networkqualityBLL = null;
        public NetworkQualityBLL NetWorkQualityBLL { get { return networkqualityBLL; } }
        private SysExcuteQualityBLL sysexcutequalityBLL = null;
        public SysExcuteQualityBLL SysExcuteQualityBLL { get { return sysexcutequalityBLL; } }
        private BlockControlBLL blockcontrolBLL = null;
        public BlockControlBLL BlockControlBLL { get { return blockcontrolBLL; } }
        private TransferBLL transferBLL = null;
        public TransferBLL TransferBLL { get { return transferBLL; } }



        private VehicleService vehicleService = null;
        public VehicleService VehicleService { get { return vehicleService; } }
        private LineService lineService = null;
        public LineService LineService { get { return lineService; } }
        private PortStationService portStationService = null;
        public PortStationService PortStationService { get { return portStationService; } }
        private ConnectionInfoService connectionInfoService = null;
        public ConnectionInfoService ConnectionInfoService { get { return connectionInfoService; } }
        private UserControlService userControlService = null;
        public UserControlService UserControlService { get { return userControlService; } }
        private TransferService transferService = null;
        public TransferService TransferService { get { return transferService; } }
        private FailOverService failOverService = null;
        public FailOverService FailOverService { get { return failOverService; } }
        private MTLService mtlService = null;
        public MTLService MTLService { get { return mtlService; } }
        private DataSyncBLL datasynBLL = null;
        public DataSyncBLL DataSyncBLL { get { return datasynBLL; } }


        private HIDBLL hidBLL = null;
        public HIDBLL HIDBLL { get { return hidBLL; } }
        private GuideBLL guideBLL = null;
        public GuideBLL GuideBLL { get { return guideBLL; } }


        public CheckSystemEventHandler CheckSystemEventHandler { get; private set; } = null;

        public PortBLL PortBLL { get; private set; } = null;
        public PortStationBLL PortStationBLL { get; private set; } = null;

        public TrafficControlBLL TrafficControlBLL { get; private set; } = null;
        //public ReserveBLLEmpty ReserveBLL { get; private set; } = null;
        public IReserveBLL ReserveBLL { get; private set; } = null;
        public CarrierBLL CarrierBLL { get; private set; } = null;
        public EqptBLL EqptBLL { get; private set; } = null;
        public ZoneBLL ZoneBLL { get; private set; } = null;
        public GroupPortStationBLL GroupPortStationBLL { get; private set; } = null;
        public ZoneDefBLL ZoneDefBLL { get; private set; } = null;
        public ShelfDefBLL ShelfDefBLL { get; private set; } = null;
        //Module
        private AvoidVehicleModule avoidVehicleModule = null;
        public AvoidVehicleModule AvoidVehicleModule { get { return avoidVehicleModule; } }
        //public VehicleChargerModule VehicleChargerModule { get; private set; } = null;

        //WIF
        private BCSystemWIF bcSystemWIF = null;
        public BCSystemWIF BCSystemWIF { get { return bcSystemWIF; } }
        private LineWIF lineWIF = null;
        public LineWIF LineWIF { get { return lineWIF; } }

        public IRouteGuide NewRouteGuide { get; private set; } = null;

        public FloydAlgorithmRouteGuide.TimeWindow TimeWindow { get; private set; } = null;



        //config
        private DataSet ohxcConfig = null;
        public DataSet OHxCConfig { get { return ohxcConfig; } }
        public DataSet TranCmdPeriodicDataSet;

        public List<DataCollectionSetting> DataCollectionList { get; private set; }


        //BackgroundPLCWorkDriver
        public BackgroundWorkDriver BackgroundWorkSample { get; private set; }              //A0.03


        public IScheduler Scheduler { get; private set; }


        //pool
        public ObjectPool<List<ValueRead>> vEventListPool = new ObjectPool<List<ValueRead>>(() => new List<ValueRead>());
        public ObjectPool<List<ValueWrite>> vWriteList = new ObjectPool<List<ValueWrite>>(() => new List<ValueWrite>());
        public ObjectPool<Dictionary<string, string>> convertValue = new ObjectPool<Dictionary<string, string>>(() => new Dictionary<string, string>());

        public ObjectPool<Stopwatch> StopwatchPool = new ObjectPool<Stopwatch>(() => new Stopwatch());
        public ObjectPool<AVEHICLE> VehiclPool = new ObjectPool<AVEHICLE>(() => new AVEHICLE());

        private ConcurrentDictionary<string, ObjectPool<PLC_FunBase>> dicPLC_FunBasePool =
         new ConcurrentDictionary<string, ObjectPool<PLC_FunBase>>();

        public PLC_FunBase getFunBaseObj<T>(string _id)
        {
            string type_name = typeof(T).Name;

            ObjectPool<PLC_FunBase> plc_funbase = dicPLC_FunBasePool.GetOrAdd(type_name, new ObjectPool<PLC_FunBase>(() => (PLC_FunBase)Activator.CreateInstance(typeof(T))));

            PLC_FunBase fun_base = plc_funbase.GetObject();
            fun_base.initial(_id);
            return fun_base;
        }
        public void putFunBaseObj<T>(PLC_FunBase put_obj)
        {
            if (put_obj == null) return;
            string type_name = typeof(T).Name;
            if (!dicPLC_FunBasePool.Keys.Contains(type_name))
            {
                return;
            }
            dicPLC_FunBasePool[type_name].PutObject(put_obj);
        }



        public SenderService ZabbixService { get; private set; }
        public ObjectPool<StringBuilder> stringBuilder = new ObjectPool<StringBuilder>(() => new StringBuilder(""));
        public string mqttTopic;
        public string mqttMsg;
        public MQTTControl mqttControl;
        public object park_lock_obj = new object();
        private SCApplication()
        {
            init();
        }
        private static BCFApplication.BuildValueEventDelegate buildValueFunc;

        public static SCApplication getInstance(string server_name, BCFApplication.BuildValueEventDelegate _buildValueFunc)
        {
            ServerName = server_name;
            buildValueFunc = _buildValueFunc;
            return getInstance();
        }

        public static SCApplication getInstance()
        {
            if (application == null)
            {
                lock (_lock)
                {
                    if (application == null)
                    {
                        application = new SCApplication();
                    }
                }
            }
            return application;
        }


        private string[] ForceBanSections = new string[]
        {
            "20212",
            "31101"
        };
        private void init()
        {
            //mqttControl = new MQTTControl();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(new string('=', 80));
            sb.AppendLine("Do SCApplication Initialize...");
            sb.AppendLine(string.Format("Version: {0}", SCAppConstants.getMainFormVersion("")));
            sb.AppendLine(string.Format("Build Date: {0}", SCAppConstants.GetBuildDateTime().ToString()));
            sb.AppendLine(new string('=', 80));
            logger.Info(sb.ToString());
            sb.Clear();
            sb = null;



            bcfApplication = BCFApplication.getInstance(buildValueFunc);



            eapSecsAgentName = getString("EAPSecsAgentName", "");
            BC_ID = getString("BC_ID", "BC_ID");

            SystemParameter.SECSConversactionTimeout = getInt("SECSConversactionTimeout", 60);

            bool include_reserve_module = getBoolean("IncludeReserveModule", true);

            initDao();      //Initial DAO
            initBLL(include_reserve_module);      //Initial BLL
            initModule();
            initServer();
            initConfig();   //Initial Config
            initialTransferCommandPeriodicDataSet();




            eqptCss = bcfApplication.getEQPTConfigSections();
            //            mapActionCss = bcfApplication.getMapActionConfigSections();
            nodeFlowRelCss = bcfApplication.getNodeFlowRelConfigSections();
            eqObjCacheManager = EQObjCacheManager.getInstance();
            eqObjCacheManager.setContext(eqptCss, nodeFlowRelCss);
            commObjCacheManager = CommObjCacheManager.getInstance();
            redisCacheManager = new RedisCacheManager(this, BC_ID);
            natsManager = new NatsManager(BC_ID, "nats-cluster", SCApplication.ServerName);


            dataCollectionCss = (DataCollectionConfigSections)ConfigurationManager.GetSection(SCAppConstants.CONFIG_DATA_COLLECTION_SETTING);
            webClientManager = WebClientManager.getInstance();

            //initialReserveSectionAPI();

            startBLL();
            initWIF();      //Initial WIF   //A0.01
            initialCatchDataFromDB();
            commObjCacheManager.start(this);

            var IpAndPort = ReportBLL.getZabbixServerIPAndPort();
            ZabbixService = new SenderService(IpAndPort.Item1, IpAndPort.Item2);
            //ReportBLL.ZabbixPush
            //                (SCAppConstants.ZabbixServerInfo.ZABBIX_OHXC_ALIVE, SCAppConstants.ZabbixOHxCAlive.ZABBIX_OHXC_ALIVE_INITIAL);

            MaxCommandCount = (int)getDouble("MaxCommandCount", 1);
            //MaxCommandCountForSwap = (int)getDouble("MaxCommandCountForSwap", 1);

            double moveCostForward = getDouble("MoveCostForward", 1);
            double moveCostReverse = getDouble("MoveCostReverse", 1);
            string algorithm = getString("ShortestPathAlgorithm", "DIJKSTRA");
            FloydAlgorithmRouteGuide routeGuide =
                new FloydAlgorithmRouteGuide(commObjCacheManager.getSections(), commObjCacheManager.getAddresses(),
                                             moveCostForward, moveCostReverse, BC_ID, algorithm);
            NewRouteGuide = routeGuide;
            TimeWindow = routeGuide.timewindow;

            //var segments = mapBLL.loadAllSegments();
            var segments = SegmentBLL.cache.GetSegments();
            NewRouteGuide.resetBanRoute();
            foreach (ASEGMENT seg in segments)
            {
                //if (only_one_way_segment.Contains(seg.SEG_ID.Trim()))
                //{
                //    if (int.TryParse(seg.RealFromAddress, out int from_address) && int.TryParse(seg.RealToAddress, out int to_address))
                //        NewRouteGuide.banRouteOneDirect(to_address, from_address);
                //}
                if (seg.STATUS == E_SEG_STATUS.Closed)
                {

                    //NewRouteGuide.banRouteTwoDirect(seg.SEG_ID);//由於目前AGV的圖資資料Section=Segment，之後會將牠們分開，
                    foreach (var sec in seg.Sections)
                        NewRouteGuide.banRouteTwoDirect(sec.SEC_ID);//由於目前AGV的圖資資料Section=Segment，之後會將牠們分開，
                                                                    //因此先將Segment作為Sectiong使用
                }
            }
            foreach (string ban_sec in ForceBanSections)
            {
                NewRouteGuide.banRouteTwoDirect(ban_sec);
            }

            //            startBLL();

            initBackgroundWork();               //A0.03
            initScheduler();
            bcfApplication.injectDataIllegalCheck(com.mirle.ibg3k0.sc.Data.SECS.SECSConst.checkDataValue);
            bcfApplication.injectSFTypeCheck(com.mirle.ibg3k0.sc.Data.SECS.SECSConst.checkSFType);
            //bcfApplication.injectUnPackWrapperMsg(com.mirle.ibg3k0.sc.Data.ValueDefMapAction.EQTcpIpMapAction.unPackWrapperMsg);
            bcfApplication.injectTcpIpDecoder(new com.mirle.iibg3k0.ttc.Common.TCPIP.DecodRawData.DecodeRawData_Google(
            com.mirle.ibg3k0.sc.Data.ValueDefMapAction.EQTcpIpMapAction.unPackWrapperMsg));
            hAProxyConnectionTest = new HAProxyConnectionTest(this);

            initialRestfulServer();

            //
            //loadECDataToSystem();
            //bdTableWatcher = new DBTableWatcher(this);
            SystemParameter.setCstMaxWaitTime(getInt("CSTMaxWaitTime", 0));
            SystemParameter.setLongestFullyChargedIntervalTime(getInt("LongestFullyChargedIntervalTime", 15));
        }

        //private void initialReserveSectionAPI()
        //{
        //    _reserveSectionAPI = new Mirle.Hlts.ReserveSection.Map.MapAPI();
        //    reserveSectionAPI = _reserveSectionAPI.MapVM;

        //    setHltVehicleInfo();

        //    LoadMapFiles();
        //}

        //private void setHltVehicleInfo()
        //{
        //    int vh_highi = getInt("VehicleHeight", 1800);
        //    int vh_width = getInt("VehicleWidth", 3200);
        //    int vh_sensor_wlength = getInt("SensorWLength", 1200);
        //    reserveSectionAPI.VehicleHeight = vh_highi;
        //    reserveSectionAPI.VehicleWidth = vh_width;
        //    reserveSectionAPI.SensorLength = vh_sensor_wlength;
        //}

        //private void LoadMapFiles(string addressPath = null, string sectionPath = null)
        //{
        //    try
        //    {
        //        string map_info_path = Environment.CurrentDirectory + this.getString("CsvConfig", "");
        //        if (addressPath == null) addressPath = $@"{map_info_path}\MapInfo\AADDRESS.csv";
        //        reserveSectionAPI.LoadMapAddresses(addressPath);

        //        if (sectionPath == null) sectionPath = $@"{map_info_path}\MapInfo\ASECTION.csv";
        //        reserveSectionAPI.LoadMapSections(sectionPath);
        //    }
        //    finally { }
        //}



        public void startModule()
        {
            avoidVehicleModule.start(this);
            //VehicleChargerModule.start(this);
        }

        private void initModule()
        {
            avoidVehicleModule = new AvoidVehicleModule();

            //VehicleChargerModule = new VehicleChargerModule();
        }

        private void initialRestfulServer()
        {
            HostConfiguration hostConfigs = new HostConfiguration()
            {
                UrlReservations = new UrlReservations() { CreateAutomatically = true }
            };
            NancyHost = new NancyHost(new Uri("http://localhost:3280"), new DefaultNancyBootstrapper(), hostConfigs);
            //TODO Hsinyu Chang 2010201 為了多開先拔掉，port應改成config設置
            //NancyHost = new NancyHost(new Uri("http://localhost:3281"), new DefaultNancyBootstrapper(), hostConfigs);

            //NancyHost = new NancyHost(new Uri("http://localhost:9527"), hostConfigs);
        }

        //DBTableWatcher bdTableWatcher = null;


        private void initialCatchDataFromDB()
        {
            DataCollectionList = loadDataCollectionSetting();
        }

        private List<DataCollectionSetting> loadDataCollectionSetting()
        {
            List<DataCollectionSetting> DataCollectionList = new List<DataCollectionSetting>();
            foreach (DataCollectionConfigSection section in dataCollectionCss.DataCollectionConfigSectionList)
            {
                foreach (DataCollectionConfigItem item in section.DataCollectionItemList)
                {
                    DataCollectionSetting setting = new DataCollectionSetting()
                    {
                        Method = section.Name,
                        IP = section.IP,
                        Port = section.Port,
                        ItemName = item.Name,
                        Period = item.Period,
                        IsReport = item.IsReport
                    };
                    DataCollectionList.Add(setting);
                }
            }
            return DataCollectionList;
        }

        public void loadECDataToSystem()
        {
            //not implement
        }

        public void loadEQPTParametersToSystem()
        {
            //not implement
        }


        #region SECS Agent Parameter Change
        public void setSECSAgentDeviceID(int deviceID, Boolean refresh)
        {
            SECSAgent agent = bcfApplication.getSECSAgent(eapSecsAgentName);
            agent.setDeviceID(deviceID);
            if (refresh)
            {
                agent.refreshConnection();
            }
        }

        public void setSECSAgentT3Timeout(int t3Timeout, Boolean refresh)
        {
            SECSAgent agent = bcfApplication.getSECSAgent(eapSecsAgentName);
            agent.setT3(t3Timeout);
            if (refresh)
            {
                agent.refreshConnection();
            }
        }

        public void setSECSAgentT5Timeout(int t5Timeout, Boolean refresh)
        {
            SECSAgent agent = bcfApplication.getSECSAgent(eapSecsAgentName);
            agent.setT5(t5Timeout);
            if (refresh)
            {
                agent.refreshConnection();
            }
        }

        public void setSECSAgentT6Timeout(int t6Timeout, Boolean refresh)
        {
            SECSAgent agent = bcfApplication.getSECSAgent(eapSecsAgentName);
            agent.setT6(t6Timeout);
            if (refresh)
            {
                agent.refreshConnection();
            }
        }

        public void setSECSAgentT7Timeout(int t7Timeout, Boolean refresh)
        {
            SECSAgent agent = bcfApplication.getSECSAgent(eapSecsAgentName);
            agent.setT7(t7Timeout);
            if (refresh)
            {
                agent.refreshConnection();
            }
        }

        public void setSECSAgentT8Timeout(int t8Timeout, Boolean refresh)
        {
            SECSAgent agent = bcfApplication.getSECSAgent(eapSecsAgentName);
            agent.setT8(t8Timeout);
            if (refresh)
            {
                agent.refreshConnection();
            }
        }
        #endregion SECS Agent Parameter Change

        private void initBackgroundWork()
        {
            BackgroundWorkSample = new BackgroundWorkDriver(new BackgroundWorkSample());            //A0.03
        }
        private void initScheduler()
        {
            Scheduler = StdSchedulerFactory.GetDefaultScheduler();

            IJobDetail tran_command_backup_scheduler = JobBuilder.Create<TransferCommandDataBackupScheduler>().Build();
            IJobDetail mttf_mtbf_scheduler = JobBuilder.Create<MTTFAndMTBFScheduler>().Build();
            ITrigger one_min_trigger = TriggerBuilder.Create()
                   .WithIdentity("news", "TelegramGroup")
                   .WithCronSchedule("0 0/2 * * * ? ")//even 2 min
                   .StartAt(DateTime.UtcNow)
                   .WithPriority(1)
                   .Build();
            ITrigger fives_min_trigger = TriggerBuilder.Create()
                   .WithIdentity("news1", "TelegramGroup")
                   .WithCronSchedule("0 0/1 * * * ? ")//even 5 min
                   .StartAt(DateTime.UtcNow)
                   .WithPriority(1)
                   .Build();

            Scheduler.ScheduleJob(tran_command_backup_scheduler, one_min_trigger);
            Scheduler.ScheduleJob(mttf_mtbf_scheduler, fives_min_trigger);
        }

        private void initDao()
        {
            lineDao = new LineDao();
            zoneDao = new ZoneDao();
            nodeDao = new NodeDao();
            eqptDao = new EqptDao();
            unitDao = new UnitDao();
            portDao = new PortDao();
            portStationDao = new PortStationDao();
            bufferPortDao = new BufferPortDao();
            flowRelDao = new FlowRelDao();
            userDao = new UserDao();
            functionCodeDao = new FunctionCodeDao();
            userFuncDao = new UserFuncDao();
            alarmDao = new AlarmDao();
            mainalarmDao = new MainAlarmDao();
            cassetteDao = new CassetteDao();
            bcStatusDao = new BCStatusDao();
            sequenceDao = new SequenceDao();
            eventRptCondDao = new EventRptCondDao();
            crateDao = new CrateDao();
            alarmRptCondDao = new AlarmRptCondDao();
            traceSetDao = new TraceSetDao();
            OperationHisDao = new OperationHisDao();    //A0.02
            userGroupDao = new UserGroupDao();
            alarmMapDao = new AlarmMapDao();
            ecDataMapDao = new ECDataMapDao();
            rptidDao = new RPTIDDao();
            ceidDao = new CEIDDao();
            ReturnCodeMapDao = new ReturnCodeMapDao();

            railDao = new RAILDao();
            addressDao = new ADDRESSDao();
            porticonDao = new PortIconDao();
            pointDao = new POINTDao();
            groupRailDao = new GROUPRAILSDao();
            sectionDao = new SectionDao(this);
            segmentDao = new SegmentDao();
            vehicleDao = new VehicleDao();

            cmdDao = new CMDDao();
            cmd_detailDao = new CMD_DetailDao();

            blockZoneMasterDao = new BlockZoneMasterDao();
            bolckZoneDetaiDao = new BlockZoneDetailDao();
            blockZoneQueueDao = new BlockZoneQueueDao();

            parkZoneDetailDao = new ParkZoneDetailDao();
            parkZoneMasterDao = new ParkZoneMasterDao();
            parkZoneTypeDao = new ParkZoneTypeDao();

            cyclezoneDdetailDao = new CycleZoneDetailDao();
            cyclezonemasterDao = new CycleZoneMasterDao();
            cyclezonetypeDao = new CycleZoneTypeDao();
            transferDao = new TransferDao();
            vtransferDao = new VTransferDao();
            hcmdDao = new HCMDDao();
            htransferDao = new HTransferDao();
            networkqualityDao = new NetworkQualityDao();
            sysexcutequalityDao = new SysExcuteQualityDao();
            mcsreportqueueDao = new MCSReportQueueDao();

            addressDataDao = new AddressDataDao();
            scaleBaseDataDao = new ScaleBaseDataDao();
            controlDataDao = new ControlDataDao();
            vehicleControlDao = new VehicleControlDao();
            dataCollectionDao = new DataCollectionDao();
            apsettiongDao = new APSettingDao();

            hidzonemasterDao = new HIDZoneMasterDao();
            hidzonedetailDao = new HIDZoneDetailDao();
            hidzonequeueDao = new HIDZoneQueueDao();

            testtrantaskDao = new TestTranTaskDao();
            couplerInfoDao = new CouplerInfoDao();
            reserveEnhanceInfoDao = new ReserveEnhanceInfoDao();
            trafficControlInfoDao = new TrafficControlInfoDao();
            carrierDao = new CarrierDao();
            groupPortStationDao = new GroupPortStationDao();
            fireDoorDao = new FireDoorDao();
            flexsimcommandDao = new FlexsimCommandDao();
            vehicleMapDao = new VehicleMapDao();
            zonedefDao = new ZoneDefDao();
            shelfdefDao = new ShelfDefDao();
            mtlInfoDao = new MTLInfoDao();
        }

        private void initConfig()
        {
            logger.Info("init bc_Config");
            if (ohxcConfig == null)
            {
                ohxcConfig = new DataSet();
                logger.Info("new bc_Config");
                loadCSVToDataset(ohxcConfig, "ALARMMAP");
                loadCSVToDataset(ohxcConfig, "MAINALARM");
                loadCSVToDataset(ohxcConfig, "APSETTING");
                loadCSVToDataset(ohxcConfig, "RETURNCODEMAP");
                loadCSVToDataset(ohxcConfig, "VEHICLEMAP");
                loadCSVToDataset(ohxcConfig, "COUPLERINFO");
                loadCSVToDataset(ohxcConfig, "FIREDOORSEGMENT");
                loadCSVToDataset(ohxcConfig, "MTLINFO");
                loadCSVToDataset(ohxcConfig, "RESERVEENHANCEINFO");
                loadCSVToDataset(ohxcConfig, "TRAFFICCONTROLINFO");
                loadCSVToDataset(ohxcConfig, "APORTSTATIONSETTING");
                logger.Info("init bc_Config success");
            }
            else
            {
                logger.Info("already init bc_Config");
            }
        }

        private void initialTransferCommandPeriodicDataSet()
        {
            string excelPath = Environment.CurrentDirectory + Path.Combine("\\config", BC_ID, "CSTTranSchedule.xlsx");

            loadExcel2DataTable(ref TranCmdPeriodicDataSet, excelPath);
        }

        public string getCsvConfigPath()
        {
            return this.getString("CsvConfig", "");
        }

        private void loadCSVToDataset(DataSet ds, string tableName)
        {
            using (GenericParser parser = new GenericParser())
            {
                if (SCUtility.isMatche(tableName, "MAINALARM"))
                {
                    parser.SetDataSource(Environment.CurrentDirectory + @"\Config\" + tableName + ".csv", System.Text.Encoding.Default);
                }
                else
                {
                    parser.SetDataSource(Environment.CurrentDirectory + this.getString("CsvConfig", "") + tableName + ".csv", System.Text.Encoding.Default);
                }
                parser.ColumnDelimiter = ',';
                parser.FirstRowHasHeader = true;
                //parser.SkipStartingDataRows = 1;
                parser.MaxBufferSize = 1024;
                //parser.MaxRows = 500;
                //parser.TextQualifier = '\"';


                DataTable dt = new System.Data.DataTable(tableName);

                bool isfirst = true;
                while (parser.Read())
                {

                    int cs = parser.ColumnCount;
                    if (isfirst)
                    {

                        for (int i = 0; i < cs; i++)
                        {
                            dt.Columns.Add(parser.GetColumnName(i), typeof(string));
                        }
                        isfirst = false;
                    }


                    DataRow dr = dt.NewRow();

                    for (int i = 0; i < cs; i++)
                    {
                        string val = parser[i];
                        //ALARM 要可以接受 16進制的 2015.02.23 by Kevin Wei
                        //if (dt.Columns[i] != null && BCFUtility.isMatche(dt.Columns[i].ColumnName, "ALARM_ID"))
                        //{
                        //    int valInt = Convert.ToInt32(val);
                        //    val = val;
                        //}
                        dr[i] = val;
                        //                        dr[i] = parser[i];
                    }
                    dt.Rows.Add(dr);
                }
                ds.Tables.Add(dt);
            }
        }
        private void loadExcel2DataTable(ref DataSet dt, string filePath)
        {
            if (!File.Exists(filePath)) return;
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {

                // Auto-detect format, supports:
                //  - Binary Excel files (2.0-2003 format; *.xls)
                //  - OpenXml Excel files (2007 format; *.xlsx)
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {

                    // Choose one of either 1 or 2:

                    // 1. Use the reader methods
                    //do
                    //{
                    //    while (reader.Read())
                    //    {
                    //        // reader.GetDouble(0);
                    //    }
                    //} while (reader.NextResult());

                    // 2. Use the AsDataSet extension method
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {

                        // Gets or sets a value indicating whether to set the DataColumn.DataType 
                        // property in a second pass.
                        UseColumnDataType = false,

                        // Gets or sets a callback to obtain configuration options for a DataTable. 
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {

                            // Gets or sets a value indicating the prefix of generated column names.
                            EmptyColumnNamePrefix = "Column",

                            // Gets or sets a value indicating whether to use a row from the 
                            // data as column names.
                            UseHeaderRow = true,

                            // Gets or sets a callback to determine which row is the header row. 
                            // Only called when UseHeaderRow = true.
                            //ReadHeaderRow = (rowReader) =>
                            //{
                            //    // F.ex skip the first row and use the 2nd row as column headers:
                            //    rowReader.Read();
                            //}
                        }
                    });
                    dt = result;
                    // The result of each spreadsheet is in result.Tables
                }
            }
            //FileStream stream = System.IO.File.Open(@".\zn01.xlsx", FileMode.Open, FileAccess.Read);

            //IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            //DataSet ds = excelReader.AsDataSet();
            //excelReader.Close();
        }
        private void initBLL(bool is_include_reserve_module)
        {
            bcSystemBLL = new BCSystemBLL();
            lineBLL = new LineBLL();
            alarmBLL = new AlarmBLL();
            sequenceBLL = new SequenceBLL();
            eventBLL = new EventBLL();
            reportBLL = new ReportBLL();
            userBLL = new UserBLL();

            mapBLL = new MapBLL();
            AddressesBLL = new AddressesBLL();
            SectionBLL = new SectionBLL();
            SegmentBLL = new SegmentBLL();
            cmdBLL = new CMDBLL();
            cycleBLL = new CycleRunBLL();
            ceidBLL = new CEIDBLL();
            vehicleBLL = new VehicleBLL();
            networkqualityBLL = new NetworkQualityBLL();
            sysexcutequalityBLL = new SysExcuteQualityBLL();
            blockcontrolBLL = new BlockControlBLL();
            transferBLL = new TransferBLL();
            datasynBLL = new DataSyncBLL();

            hidBLL = new HIDBLL();
            guideBLL = new GuideBLL();

            CheckSystemEventHandler = new CheckSystemEventHandler();

            PortBLL = new PortBLL();
            PortStationBLL = new PortStationBLL();
            TrafficControlBLL = new TrafficControlBLL();
            UnitBLL = new UnitBLL();
            ReserveBLL = is_include_reserve_module ? 
                         new ReserveBLL() as IReserveBLL : new ReserveBLLEmpty() as IReserveBLL;
            CarrierBLL = new CarrierBLL();
            EqptBLL = new EqptBLL();
            ZoneBLL = new ZoneBLL();
            GroupPortStationBLL = new GroupPortStationBLL();
            ZoneDefBLL = new ZoneDefBLL();
            ShelfDefBLL = new ShelfDefBLL();
        }

        public void initServer()
        {
            vehicleService = new VehicleService();
            lineService = new LineService();
            portStationService = new PortStationService();
            failOverService = new FailOverService();
            mtlService = new MTLService();
            connectionInfoService = new ConnectionInfoService();
            userControlService = new UserControlService();
            transferService = new TransferService();
        }

        private void startBLL()
        {
            bcSystemBLL.start(this);
            lineBLL.start(this);
            alarmBLL.start(this);
            sequenceBLL.start(this);
            eventBLL.start(this);
            reportBLL.start(this);
            userBLL.start(this);

            mapBLL.start(this);
            AddressesBLL.start(this);
            SectionBLL.start(this);
            SegmentBLL.start(this);
            cmdBLL.start(this);
            cycleBLL.start(this);
            ceidBLL.start(this);
            vehicleBLL.start(this);
            networkqualityBLL.start(this);
            sysexcutequalityBLL.start(this);
            blockcontrolBLL.start(this);
            transferBLL.start(this);
            datasynBLL.start(this);

            hidBLL.start(this);
            guideBLL.start(this);

            CheckSystemEventHandler.Start(this);

            PortBLL.start(this);
            PortStationBLL.start(this);
            TrafficControlBLL.start(this);

            UnitBLL.start(this);
            ReserveBLL.start(this);
            CarrierBLL.start(this);
            EqptBLL.start(this);
            ZoneBLL.start(this);
            GroupPortStationBLL.start(this);
            ZoneDefBLL.start(this);
            ShelfDefBLL.start(this);
        }

        private void startService()
        {
            vehicleService.Start(this);
            lineService.start(this);
            failOverService.start(this);
            mtlService.start(this);
            portStationService.start(this);
            connectionInfoService.start(this);
            userControlService.start(this);
            transferService.start(this);
        }

        private void initWIF()
        {
            bcSystemWIF = new BCSystemWIF(this);
            lineWIF = new LineWIF(this);
        }

        public BCFApplication getBCFApplication()
        {
            return bcfApplication;
        }

        public void sendMQTTMessage()
        {
            mqttControl.MQTTPub(mqttTopic, mqttMsg);
        }

        public Boolean getBoolean(string key, Boolean defaultValue)
        {
            Boolean rtn = defaultValue;
            try
            {
                string val = ConfigurationManager.AppSettings.Get(key);
                if (val != null)  //A0.09
                {
                    if (BCFUtility.isMatche(val, "Y"))
                    {
                        rtn = true;
                    }
                    else
                    {
                        rtn = false;
                    }
                }
                else                        //A0.09
                {                           //A0.09
                    return defaultValue;    //A0.09
                }                           //A0.09
            }
            catch (Exception e)
            {
                logger.Warn("Get Config error[key:{0}][Exception:{1}]", key, e);
            }
            return rtn;
        }

        public int getInt(string key, int defaultValue)
        {
            int rtn = defaultValue;
            try
            {
                string setting_value = ConfigurationManager.AppSettings.Get(key);
                if (!SCUtility.isEmpty(setting_value))
                {
                    rtn = Convert.ToInt32(setting_value);
                }
            }
            catch (Exception e)
            {
                logger.Warn("Get Config error[key:{0}][Exception:{1}]", key, e);
            }
            return rtn;
        }

        private double getDouble(string key, double defaultValue)
        {
            double rtn = defaultValue;
            try
            {
                rtn = Convert.ToDouble(ConfigurationManager.AppSettings.Get(key));
            }
            catch (Exception e)
            {
                logger.Warn("Get Config error[key:{0}][Exception:{1}]", key, e);
            }
            return rtn;
        }

        private long getLong(string key, long defaultValue)
        {
            long rtn = defaultValue;
            try
            {
                rtn = long.Parse(ConfigurationManager.AppSettings.Get(key));
            }
            catch (Exception e)
            {
                logger.Warn("Get Config error[key:{0}][Exception:{1}]", key, e);
            }
            return rtn;
        }

        public string getString(string key, string defaultValue)
        {
            string rtn = defaultValue;
            try
            {
                rtn = ConfigurationManager.AppSettings.Get(key);
                if (SCUtility.isEmpty(rtn))
                {
                    rtn = defaultValue;
                }
            }
            catch (Exception e)
            {
                logger.Warn("Get Config error[key:{0}][Exception:{1}]", key, e);
            }
            return rtn;
        }


        public DBConnection getDBConnection()
        {
            return getBCFApplication().getDBConnection();
        }

        public DBConnection getDBStatelessConnection()
        {
            return getBCFApplication().getDBStatelessConnection();
        }

        private IShareMemoryInitProcess shareMemInitProc;

        private void injectValueDefMapAction(BaseMapActionConfigElement config, ref BaseEQObject baseEQObject)
        {
            List<string> subMapActs = config.ValueDefMapActionClasses.Split(
                            new string[] { ";" },
                            StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (string mapAct in subMapActs)
            {
                try
                {
                    Type mapActType = Type.GetType(mapAct.Trim(), true);

                    IValueDefMapAction valMapAct =
                        (IValueDefMapAction)Activator.CreateInstance(mapActType);
                    baseEQObject.injectValueDefMapAction(valMapAct);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, String.Format("Not Found ValueDefMapAction Class : {0}", mapAct));
                    throw new Exception(String.Format("Not Found ValueDefMapAction Class : {0}", mapAct));
                }
            }
        }

        public Boolean canSelRevertSystem()
        {
            SCAppConstants.BCSystemInitialRtnCode rtnCode = application.bcSystemBLL.initialBCSystem();
            if (rtnCode == SCAppConstants.BCSystemInitialRtnCode.Error)
            {
                throw new Exception("Initial BC System Occur Error !!");
            }
            if (rtnCode == SCAppConstants.BCSystemInitialRtnCode.NonNormalShutdown
                /* && eqObjCacheManager.hasLineDataExist()*/)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 必須呼叫此method進行建立Equipment
        /// </summary>
        /// <param name="recoverFromDB">The recover from database.</param>
        public void startBuildEqpts(Boolean recoverFromDB)
        {

            eqObjCacheManager.start(/*eqptCss, nodeFlowRelCss, */recoverFromDB);      //啟動EQ Object Cache.. 將從DB取得Line資訊建立EQ Object
            string shareMemoryInitClass = eqptCss.ShareMemoryInitClass;
            try
            {
                Type shareMemoryInitType = Type.GetType(shareMemoryInitClass.Trim(), true);
                shareMemInitProc =
                    (IShareMemoryInitProcess)Activator.CreateInstance(shareMemoryInitType);
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "Not Found ShareMemoryInitProcess Class : {0}", shareMemoryInitClass);
            }

            foreach (var config in eqptCss.ConfigSections)
            {
                string line_id = config.Line_ID;
                BaseEQObject line = eqObjCacheManager.getLine();
                if (line == null || !BCFUtility.isMatche(line_id, (line as ALINE).LINE_ID))
                {
                    logger.Warn("MapActionDefs occur error: System Not Define Line[{0}]", line_id);
                    break;
                }
                injectValueDefMapAction(config, ref line);
                foreach (ZoneConfigSection zoneConfig in config.ZoneConfigList)
                {
                    string zone_id = zoneConfig.Zone_ID;
                    BaseEQObject zone = eqObjCacheManager.getZoneByZoneID(zone_id);
                    injectValueDefMapAction(zoneConfig, ref zone);
                    foreach (NodeConfigSection nodeConfig in zoneConfig.NodeConfigList)
                    {
                        string node_id = nodeConfig.Node_ID;
                        BaseEQObject node = eqObjCacheManager.getNodeByNodeID(node_id);
                        injectValueDefMapAction(nodeConfig, ref node);
                        foreach (EQPTConfigSection eqptConfig in nodeConfig.EQPTConfigList)
                        {
                            string eqpt_id = eqptConfig.EQPT_ID;
                            BaseEQObject eqpt = eqObjCacheManager.getEquipmentByEQPTID(eqpt_id);
                            injectValueDefMapAction(eqptConfig, ref eqpt);
                            foreach (UnitConfigSection unitConfig in eqptConfig.UnitConfigList)
                            {
                                string unit_id = unitConfig.Unit_ID;
                                BaseEQObject unit = eqObjCacheManager.getUnitByUnitID(unit_id);
                                injectValueDefMapAction(unitConfig, ref unit);
                            }
                            foreach (PortConfigSection portConfig in eqptConfig.PortConfigList)
                            {
                                string port_id = portConfig.Port_ID;
                                BaseEQObject port = eqObjCacheManager.getPortByPortID(port_id);
                                injectValueDefMapAction(portConfig, ref port);
                            }
                            foreach (BufferConfigSection buffConfig in eqptConfig.BuffConfigList)
                            {
                                string buff_id = buffConfig.Buff_ID;
                                BaseEQObject buff = eqObjCacheManager.getBuffByBuffID(buff_id);
                                injectValueDefMapAction(buffConfig, ref buff);
                            }
                            foreach (PortStationConfigSection portStationConfig in eqptConfig.PortStationConfigList)
                            {
                                string port_station_id = portStationConfig.Port_ID;
                                BaseEQObject port_station = eqObjCacheManager.getPortStation(port_station_id);
                                injectValueDefMapAction(portStationConfig, ref port_station);
                            }
                            //BaseEQObject vh = eqObjCacheManager.getVehicletByVHID(eqpt_id);
                            //injectValueDefMapAction(eqptConfig, ref vh);
                        }
                        foreach (VehicleConfigSection vhConfig in nodeConfig.VehilceConfigList)
                        {
                            string vh_id = vhConfig.Vh_ID;
                            BaseEQObject eqpt = eqObjCacheManager.getVehicletByVHID(vh_id);
                            injectValueDefMapAction(vhConfig, ref eqpt);
                        }
                    }
                }
            }

            VehicleBLL.startMapAction();
            var l = eqObjCacheManager.getLine();
            reportBLL.startMapAction(l.getUsingMapAction());

            vehicleDao.start(eqObjCacheManager.getAllVehicle());
        }

        public EQObjCacheManager getEQObjCacheManager()
        {
            return eqObjCacheManager;
        }
        public CommObjCacheManager getCommObjCacheManager()
        {
            return commObjCacheManager;
        }
        public RedisCacheManager getRedisCacheManager()
        {
            return redisCacheManager;
        }

        public NatsManager getNatsManager()
        {
            return natsManager;
        }

        //public Mirle.Hlts.ReserveSection.Map.ViewModels.HltMapViewModel getReserveSectionAPI()
        //{
        //    return reserveSectionAPI;
        //}

        //A0.07 Begin
        /// <summary>
        /// 根據Equipment的腳本進行初始化
        /// </summary>
        private void initScriptForEquipment()
        {
            try
            {
                if (shareMemInitProc != null)
                {
                    shareMemInitProc.doInit();
                }
                ALINE line = eqObjCacheManager.getLine();
                line.Redis_Link_Stat = redisCacheManager.IsConnection ? SCAppConstants.LinkStatus.LinkOK : SCAppConstants.LinkStatus.LinkFail;

                //foreach (Line line in lineDic.Values) 
                //{
                foreach (BCFAppConstants.RUN_LEVEL runLevel in Enum.GetValues(typeof(BCFAppConstants.RUN_LEVEL)))
                {
                    //mainLine.doShareMemoryInit(runLevel);
                    line.doShareMemoryInit(runLevel);
                }
                //}
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
                throw;
            }
        }


        /// <summary>
        /// The started
        /// </summary>
        private Boolean started = false;
        /// <summary>
        /// 是否已啟動SC Timer
        /// </summary>
        /// <value>The started.</value>
        public Boolean Started { get { return started; } }

        /// <summary>
        /// Starts the process.
        /// </summary>
        private void startProcess()
        {
            startService();
            initScriptForEquipment();
            Scheduler.Start();
        }

        /// <summary>
        /// 啟動Share Memory，與指定MPLC通訊
        /// </summary>
        /// <param name="mplcName">Name of the MPLC.</param>
        public void startShareMemory(string mplcName)
        {
            lock (_lock)
            {
                if (started == false)
                    return;
                bcfApplication.startShareMemory(mplcName);
                logger.Info("Start Share Memory");
            }
        }

        /// <summary>
        /// 啟動Share Memory，與所有MPLC通訊
        /// </summary>
        public void startShareMemory()
        {
            lock (_lock)
            {
                if (started == false)
                    return;
                bcfApplication.startShareMemory();
                logger.Info("Start Share Memory");
            }
        }

        /// <summary>
        /// 啟動SECS Agent
        /// </summary>
        public void startSECSAgent()
        {
            lock (_lock)
            {
                if (started == false)
                    return;
                bcfApplication.startAllSECSAgent();
                logger.Info("Start SECS Agent");
            }
        }



        /// <summary>
        /// 啟動TcpIp Agent
        /// </summary>
        public void startTcpIpSecverListen()
        {
            lock (_lock)
            {
                if (started == false)
                    return;
                hAProxyConnectionTest.listen();
                //bcfApplication.startTcpIpSecverListen();
                //logger.Info("Start TcpIp Agent");

                //if (FailOverService.isActive())
                //{
                //    hAProxyConnectionTest.listen();
                //}
                //else
                //{
                //    hAProxyConnectionTest.shutDown();
                //}
            }
        }
        /// <summary>
        /// 啟動指定的 TcpIpServer
        /// </summary>
        public void startTcpIpServerListen(int portNum)
        {
            lock (_lock)
            {
                if (started == false)
                    return;
                bcfApplication.startTcpIpSecverListen(portNum);
                logger.Info($"Start TcpIp Agent,Port Num:{portNum}");
            }
        }


        /// <summary>
        /// 啟動TcpIp Agent
        /// </summary>
        public void startTcpIpAgent()
        {
            lock (_lock)
            {
                if (started == false)
                    return;
                bcfApplication.startAllTcpIpAgent();
                logger.Info("Start TcpIp Agent");
            }
        }
        /// <summary>
        /// 開始執行
        /// </summary>
        public void start()
        {
            lock (_lock)
            {
                if (started == true)
                    return;
                bcfApplication.start(startProcess);

                //TODO Hsinyu Chang 2010201 為了多開先拔掉，port應改成config設置
                NancyHost.Start();


                logger.Info("Start Application");
                started = true;
            }
        }

        /// <summary>
        /// 停止Share Memory，與指定MPLC停止通訊
        /// </summary>
        /// <param name="mplcName">Name of the MPLC.</param>
        public void stopShareMemory(string mplcName)
        {
            lock (_lock)
            {
                bcfApplication.stopShareMemory(mplcName);
                logger.Info("Stop Share Memory");
            }
        }

        /// <summary>
        /// 停止Share Memory，與所有MPLC停止通訊
        /// </summary>
        public void stopShareMemory()
        {
            lock (_lock)
            {
                bcfApplication.stopShareMemory();
                logger.Info("Stop Share Memory");
            }
        }

        /// <summary>
        /// 停止SECS Agent
        /// </summary>
        public void stopSECSAgent()
        {
            lock (_lock)
            {
                bcfApplication.stopAllSECSAgent();
                logger.Info("Stop SECS Agent");
            }
        }

        /// <summary>
        /// 停止TcpIp Agent
        /// </summary>
        public void stopTcpIpServer()
        {
            lock (_lock)
            {
                bcfApplication.ShutdownTcpIpSecverListen();
                logger.Info("Stop TcpIp Agent");
            }
        }
        /// <summary>
        /// 停止指定TcpIp Server
        /// </summary>
        public void stopTcpIpServer(int portNum)
        {
            lock (_lock)
            {
                bcfApplication.ShutdownTcpIpSecverListen(portNum);
                logger.Info($"Stop TcpIp Agent,Port Num:{portNum}");
            }
        }


        /// <summary>
        /// Stops the process.
        /// </summary>
        private void stopProcess()
        {
            //not implement
            Scheduler.Shutdown(false);
        }

        /// <summary>
        /// 停止執行
        /// </summary>
        public void stop()
        {
            lock (_lock)
            {
                if (started == false)
                    return;
                bcfApplication.stop(stopProcess);
                //RedisConnection.Close();
                //TODO Hsinyu Chang 2010201 為了多開先拔掉，port應改成config設置
                NancyHost.Stop();
                natsManager.close();
                logger.Info("Stop Application");
                started = false;
            }
        }
        public void CloseRedisConnection()
        {
            redisCacheManager.CloseRedisConnection();
        }
        /// <summary>
        /// Gets the message string.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>System.String.</returns>
        public static string getMessageString(string key, params object[] args)
        {
            return BCFApplication.getMessageString(key, args);
        }
    }

    /// <summary>
    /// Class SystemParameter.
    /// </summary>
    public class SystemParameter
    {

        public static event EventHandler<bool> AutoOverrideChange;

        //System EC Data 
        public static int SECSConversactionTimeout = 60;
        public static string InitialControlState = SECSConst.HostCrtMode_EQ_Off_line;
        public static int ControlStateKeepTimeSec = 0;
        public static int HeartBeatSec = 0;


        public static bool AutoTeching = false;
        public static int CSTMaxWaitTime = 0;
        public static int TheLongestFullyChargedIntervalTime_Mim = 15;

        public static bool AutoOverride = true;
        public static int PortMaxPriority = 99;

        public static int OpenAGVStationCoverDistance_mm = 5000;
        public static bool IsByPassAGVShelfStatus { get; private set; } = true;

        public static int TransferCommandQueueTimeOut_mSec = 600000;
        public static double PassAxisDistance = 250;
        
        public static int AllowVhIdleTime_ms = 300000;


        public static void setSECSConversactionTimeout(int timeout)
        {
            SECSConversactionTimeout = timeout;
        }

        public static void setInitialHostMode(string hostMode)
        {
            InitialControlState = hostMode;
        }

        public static void setControlStateKeepTime(int keepTimeSec)
        {
            ControlStateKeepTimeSec = keepTimeSec;
        }

        public static void setHeartBeatSec(int heartBeatSec)
        {
            HeartBeatSec = heartBeatSec;
        }

        public static void setCstMaxWaitTime(int cSTMaxWaitTime)
        {
            CSTMaxWaitTime = cSTMaxWaitTime;
        }

        public static void setAutoOverride(bool autoOverride)
        {
            if (AutoOverride != autoOverride)
            {
                AutoOverride = autoOverride;
                AutoOverrideChange?.Invoke(null, AutoOverride);
            }
        }
        public static void setLongestFullyChargedIntervalTime(int longestFullyChargedIntervalTime)
        {
            TheLongestFullyChargedIntervalTime_Mim = longestFullyChargedIntervalTime;
        }
        public static void setPortMaxPriority(int potyMaxPriority)
        {
            PortMaxPriority = potyMaxPriority;
        }
        public static void setOpenAGVStationCoverDistance(int openAGVStationCoverDistance_mm)
        {
            OpenAGVStationCoverDistance_mm = openAGVStationCoverDistance_mm;
        }
        public static void setIsByPassAGVShelfStatus(bool isByPassAGVShelfStatus)
        {
            IsByPassAGVShelfStatus = isByPassAGVShelfStatus;
        }
        public static void setTransferCommandQueueTimeOut_mSec(int transferCommandQueueTimeOut_mSec)
        {
            TransferCommandQueueTimeOut_mSec = transferCommandQueueTimeOut_mSec;
        }
        public static void setAllowIdleTime_ms(int allowIdleTime_ms)
        {
            AllowVhIdleTime_ms = allowIdleTime_ms;
        }
        public static void setPassAxisDistance(double passDistance)
        {
            PassAxisDistance = passDistance;
        }
    }

    public class HAProxyConnectionTest
    {
        iibg3k0.ttc.Common.TCPIP.TcpIpServer tcpIpServer;
        Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public HAProxyConnectionTest(SCApplication app)
        {
            tcpIpServer = new iibg3k0.ttc.Common.TCPIP.TcpIpServer(5001, true, iibg3k0.ttc.Common.AppConstants.FrameBuilderType.PC_TYPE_MIRLE);
            tcpIpServer.SessionCreat += TcpIpServer_SessionCreat;
        }

        private void TcpIpServer_SessionCreat(object sender, object e)
        {
            try
            {
                string testString = "OK";
                Byte[] byteArray = new byte[testString.Length];
                for (int i = 0; i < testString.Length; i++)
                {
                    byteArray[i] = Convert.ToByte(testString[i]);
                }
                tcpIpServer.SendRawData(sender, byteArray);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                tcpIpServer.CloseSession(sender);
            }
        }

        public void shutDown()
        {
            tcpIpServer?.Shutdown();
        }
        public void listen()
        {
            tcpIpServer.Listen();
        }

    }

    public class DebugParameter
    {
        public enum CycleRunTestType
        {
            Normal,
            IgnorePortStatus,
            OnlyMove,
            MoveBySelectPort
        }
        public enum TransferModeType
        {
            Normal,
            Double
        }

        public static Boolean IsTestEAPMode = false;
        public static Boolean DisableSyncTime = false;
        public static Boolean IsDebugMode = false;
        public static Boolean RejectEQCimOnReq = false;
        public static Boolean RejectReplyEQS1F1 = false;
        public static Boolean RejectEAPOnline = false;

        public static Boolean CanAutoRandomGeneratesCommand = false;
        public static CycleRunTestType CycleRunType = CycleRunTestType.Normal;

        public static Boolean IsCycleRun = false;
        public static int CycleRunIntervalTime = 0;

        public static uint BatteryCapacity = 0;

        private static Boolean isforcedpassblockcontrol = false;
        public static Boolean isForcedPassBlockControl
        {
            set { isforcedpassblockcontrol = value; }
            get { return isforcedpassblockcontrol; }
        }
        public static Boolean isForcedRejectBlockControl = false;
        public static Boolean isTestCarrierInterfaceError = false;
        public static Boolean isReportEQPortCarrierRemove = false; //20210510 改為預設False
        public static Boolean isForceBypassMTLPauseCheck= false;
        public static Boolean isForceBypassVehclePauseCheck = false;

        //public static Boolean isForcedPassReserve = false;
        public static Boolean isForcedPassReserve = false;
        public static Boolean isForcedPassReserve_AGV0609 = false;
        public static Boolean isForcedRejectReserve = false;

        public static Boolean isContinueByIDReadFail = false;
        public static Boolean testRetryReserveReq = false;
        public static Boolean testRetryLoadArrivals = false;
        public static Boolean testRetryLoadComplete = false;
        public static Boolean testRetryUnloadArrivals = false;
        public static Boolean testRetryUnloadComplete = false;
        public static Boolean testRetryVhloading = false;
        public static Boolean testRetryVhunloading = false;
        public static Boolean testRetryBcrread = false;
        public static Boolean CommandCheckForcePass = false;
        public static Boolean CanUnloadToAGVStationTest = false;
        public static int CommandCompleteWaitTime = 0;

        public static int NumberOfAvoidanceSegment = 3;
        public static TransferModeType TransferMode = TransferModeType.Normal;

        public static Boolean isNeedCheckPortReady = true;
        public static Boolean isPassCouplerHPSafetySignal = false;
        public static Boolean isPassCouplerStatus = false;
        public static Boolean isNeedCheckPortUpDateTime = true;
        public static Boolean isDefaultOHTModeAutoRemote = true;

        public static int GuideSearchTimes = 0;
        public static int GuideQuickSearchTimes = 0;

    }
}
