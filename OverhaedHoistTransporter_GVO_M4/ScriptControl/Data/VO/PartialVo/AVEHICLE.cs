using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.bcf.Controller;
using com.mirle.ibg3k0.bcf.Data.TimerAction;
using com.mirle.ibg3k0.bcf.Data.ValueDefMapAction;
using com.mirle.ibg3k0.bcf.Data.VO;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.BLL;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using Newtonsoft.Json;
using NLog;
using Stateless;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc
{
    public class PositionChangeEventArgs : EventArgs
    {
        public double Last_X_Axis;
        public double Last_Y_Axis;
        public double Current_X_Axis;
        public double Current_Y_Axis;
        public PositionChangeEventArgs(double last_X_Axis, double last_Y_Axis, double current_X_Axis, double current_Y_Axis)
        {
            Last_X_Axis = last_X_Axis;
            Last_Y_Axis = last_Y_Axis;
            Current_X_Axis = current_X_Axis;
            Current_Y_Axis = current_Y_Axis;
        }
    }
    public class LocationChangeEventArgs : EventArgs
    {
        public string EntrySection;
        public string LeaveSection;
        public LocationChangeEventArgs(string entrySection, string leaveSection)
        {
            EntrySection = entrySection;
            LeaveSection = leaveSection;
        }
    }
    public class SegmentChangeEventArgs : EventArgs
    {
        public string EntrySegment;
        public string LeaveSegment;
        public SegmentChangeEventArgs(string entrySegment, string leaveSegment)
        {
            EntrySegment = entrySegment;
            LeaveSegment = leaveSegment;
        }
    }

    public partial class AVEHICLE : BaseEQObject
    {
        Logger logger = LogManager.GetCurrentClassLogger();
        public const string DEVICE_NAME_AGV = "AGV";

        public const int VEHICLE_CARRIER_LOCATION_R_INDEX = 0;
        public const int VEHICLE_CARRIER_LOCATION_L_INDEX = 1;

        public static UInt16 BATTERYLEVELVALUE_FULL { get; private set; } = 100;
        public static UInt16 BATTERYLEVELVALUE_HIGH { get; private set; } = 80;
        public static UInt16 BATTERYLEVELVALUE_LOW { get; private set; } = 40;
        /// <summary>
        /// 在一次的Reserve要不到的過程中，最多可以Override失敗的次數
        /// </summary>
        public static UInt16 MAX_FAIL_OVERRIDE_TIMES_IN_ONE_CASE { get; private set; } = 3;
        /// <summary>
        /// 最大允許沒有通訊的時間
        /// </summary>
        public static UInt16 MAX_ALLOW_NO_COMMUNICATION_TIME_SECOND { get; private set; } = 30;
        /// <summary>
        /// 單筆命令，最大允許的搬送時間
        /// </summary>
        public static UInt32 MAX_ALLOW_ACTION_TIME_MILLISECOND { get; private set; } = 1200000;
        /// <summary>
        /// 最大允許斷線時間
        /// </summary>
        public static UInt16 MAX_ALLOW_NO_CONNECTION_TIME_SECOND { get; private set; } = 60;
        /// <summary>
        /// 最大允許斷線時間Milliseconds
        /// </summary>
        public static UInt32 MAX_ALLOW_IDLE_TIME_MILLISECOND { get; private set; } = 300000;
        /// <summary>
        /// 當要求狀態失敗N次後，會重新啟動連線的機制
        /// </summary>
        public const int MAX_STATUS_REQUEST_FAIL_TIMES = 3;

        VehicleTimerAction vehicleTimer = null;
        public VehicleStateMachine vhStateMachine;
        private Stopwatch CurrentCommandExcuteTime;
        private Stopwatch IdleTimer;
        private Stopwatch CommandActionTimer;
        public AVEHICLE()
        {
            eqptObjectCate = SCAppConstants.EQPT_OBJECT_CATE_EQPT;
            vhStateMachine = new VehicleStateMachine(() => State, (state) => State = state);
            vhStateMachine.OnTransitioned(TransitionedHandler);
            vhStateMachine.OnUnhandledTrigger(UnhandledTriggerHandler);
            CurrentCommandExcuteTime = new Stopwatch();
            CommandActionTimer = new Stopwatch();
            IdleTimer = new Stopwatch();
        }


        #region Event
        public event EventHandler ExcuteCommandStatusChange;
        public event EventHandler VehicleStatusChange;
        public event EventHandler VehiclePositionChange;
        public event EventHandler<bool> ConnectionStatusChange;

        /// <summary>
        /// 用來通知X、Y改變
        /// </summary>
        public event EventHandler<PositionChangeEventArgs> PositionChange;
        /// <summary>
        /// 用來通知Section改變
        /// </summary>
        public event EventHandler<LocationChangeEventArgs> LocationChange;
        public event EventHandler<SegmentChangeEventArgs> SegmentChange;
        public event EventHandler<CompleteStatus> CommandComplete;
        public event EventHandler LongTimeNoCommuncation;
        public event EventHandler<List<string>> LongTimeInaction;
        public event EventHandler LongTimeDisconnection;
        public event EventHandler<VHModeStatus> ModeStatusChange;
        public event EventHandler<VhStopSingle> ErrorStatusChange;
        public event EventHandler Idling;
        public event EventHandler<string> CurrentExcuteCmdChange;
        public event EventHandler<int> StatusRequestFailOverTimes;
        public event EventHandler CanNotFindTheCharger;

        public void onExcuteCommandStatusChange()
        {
            ExcuteCommandStatusChange?.Invoke(this, EventArgs.Empty);
        }
        public void onVehicleStatusChange()
        {
            VehicleStatusChange?.Invoke(this, EventArgs.Empty);
        }
        public void onVehiclePositionChange()
        {
            VehiclePositionChange?.Invoke(this, EventArgs.Empty);
        }
        public void onConnectionStatusChange(bool isConnection)
        {
            ConnectionStatusChange?.Invoke(this, isConnection);
        }
        public void onCommandComplete(CompleteStatus cmpStatus)
        {
            CommandComplete?.Invoke(this, cmpStatus);
        }
        public void onPositionChange(double last_X_Axis, double last_Y_Axis, double current_X_Axis, double current_Y_Axis)
        {
            PositionChange?.Invoke(this, new PositionChangeEventArgs(last_X_Axis, last_Y_Axis, current_X_Axis, current_Y_Axis));
        }
        public void onLocationChange(string entrySection, string leaveSection)
        {
            LocationChange?.Invoke(this, new LocationChangeEventArgs(entrySection, leaveSection));
        }
        public void onSegmentChange(string entrySegemnt, string leaveSegment)
        {
            SegmentChange?.Invoke(this, new SegmentChangeEventArgs(entrySegemnt, leaveSegment));
        }
        public void onLongTimeNoCommuncation()
        {
            LongTimeNoCommuncation?.Invoke(this, EventArgs.Empty);
        }
        public void onLongTimeInaction(List<string> cmdIDs)
        {
            isLongTimeInaction = true;
            LongTimeInaction?.Invoke(this, cmdIDs);
        }
        public void onLongTimeDisConnection()
        {
            LongTimeDisconnection?.Invoke(this, EventArgs.Empty);
        }
        public void onModeStatusChange(VHModeStatus modeStatus)
        {
            ModeStatusChange?.Invoke(this, modeStatus);
        }
        public void onErrorStatusChange(VhStopSingle vhStopSingle)
        {
            ErrorStatusChange?.Invoke(this, vhStopSingle);
        }
        public void onVehicleIdle()
        {
            isIdling = true;
            Idling?.Invoke(this, EventArgs.Empty);
        }
        public void onCurrentExcuteCmdChange(string currentExcuteCmdID)
        {
            CurrentExcuteCmdChange?.Invoke(this, currentExcuteCmdID);
        }
        public void onVehicleCanNotFindTheCharger()
        {
            if (!isCanNotFindTheCharger)
            {
                isCanNotFindTheCharger = true;
                CanNotFindTheCharger?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion Event

        public void SetupTimerAction()
        {
            vehicleTimer = new VehicleTimerAction(this, "VehicleTimerAction", 1000);
            startVehicleTimer();
        }
        public void startVehicleTimer()
        {
            vehicleTimer.start();
        }
        public void stopVehicleTimer()
        {
            vehicleTimer.stop();
        }

        public override string ToString()
        {
            string json = JsonConvert.SerializeObject(this);
            return json;
        }

        #region Vehicle Parameter
        public string LastLoadCompleteCommandID = "";
        public int PrePositionSeqNum = 0;
        public string CUR_SEG_ID { get; set; }
        public string CUR_SEC_ID { get; set; }
        public string CUR_ADR_ID { get; set; }
        public string CUR_ZONE_ID
        {
            get
            {
                if (SCUtility.isEmpty(CUR_SEC_ID)) return "";
                char first_char = CUR_SEC_ID[0];
                switch (first_char)
                {
                    case '1':
                        return "EQ_ZONE1";
                    case '2':
                        return "EQ_ZONE2";
                    case '3':
                        return "EQ_ZONE3";
                    default:
                        return "";
                }
            }
        }
        public bool IsCloseToAGVStation { get; set; }
        public string getZoneID(SectionBLL sectionBLL)
        {
            string current_adr = SCUtility.Trim(CUR_ADR_ID, true);
            if (SCUtility.isEmpty(current_adr)) return "";
            if (current_adr.StartsWith("9"))
            {
                current_adr = current_adr.Remove(0, 1);
                current_adr = "1" + current_adr;
                //var sections = sectionBLL.cache.GetSectionsByFromAddress(current_adr);
                var sections = sectionBLL.cache.GetSectionsByFromToAddress(current_adr);
                //濾掉9開頭的路段
                sections = sections.
                           Where(sec => !sec.SEC_ID.StartsWith("9")).
                           ToList();
                if (sections.Count > 0)
                {
                    string first_sec_id = sections[0].SEC_ID;
                    char first_char = first_sec_id[0];
                    switch (first_char)
                    {
                        case '1':
                            return "EQ_ZONE1";
                        case '2':
                            return "EQ_ZONE2";
                        case '3':
                            return "EQ_ZONE3";
                        default:
                            return "";
                    }

                }
                else
                {
                    return "";
                }
            }
            else
            {
                if (SCUtility.isEmpty(CUR_SEC_ID)) return "";
                char first_char = CUR_SEC_ID[0];
                switch (first_char)
                {
                    case '1':
                        return "EQ_ZONE1";
                    case '2':
                        return "EQ_ZONE2";
                    case '3':
                        return "EQ_ZONE3";
                    default:
                        return "";
                }
            }

        }
        public double ACC_SEC_DIST { get; set; }
        public com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage.VHModeStatus MODE_STATUS { get; set; }
        public com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage.VHActionStatus ACT_STATUS { get; set; }
        public string TRANSFER_ID { get; set; }
        public string CMD_ID { get; set; }
        public string CurrentExcuteCmdID { get; set; }
        public string PreExcute_Transfer_ID { get; set; }
        //public List<Location> CarrierLocation { get; private set; }

        public bool HAS_CST { get; set; }
        public string CST_ID { get; set; }

        public com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage.VhStopSingle BLOCK_PAUSE { get; set; }
        public com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage.VhStopSingle CMD_PAUSE { get; set; }
        public com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage.VhStopSingle OBS_PAUSE { get; set; }
        public com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage.VhStopSingle HID_PAUSE { get; set; }
        public com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage.VhStopSingle ERROR { get; set; }
        public com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage.VhStopSingle EARTHQUAKE_PAUSE { get; set; }
        public com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage.VhStopSingle SAFETY_PAUSE { get; set; }
        public com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage.VhStopSingle RESERVE_PAUSE { get; set; }
        //public com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage.VhStopSingle OP_PAUSE { get; set; }

        public int OBS_DIST { get; set; }
        public Nullable<System.DateTime> UPD_TIME { get; set; }
        public int BATTERYCAPACITY { get; set; }
        public int STEERINGWHEELANGLE { get; set; }
        public Nullable<System.DateTime> SEC_ENTRY_TIME { get; set; }
        #endregion Vehicle Parameter

        public VehicleState State = VehicleState.REMOVED;
        [JsonIgnore]
        public virtual string[] PredictSections { get; set; }
        [JsonIgnore]
        public virtual string StartAdr { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual string FromAdr { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual string ToAdr { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual string ToSectionID { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual DriveDirction CurrentDriveDirction { get; set; }
        [JsonIgnore]
        public virtual double Speed { get; set; }
        [JsonIgnore]
        public virtual string ObsVehicleID { get; set; }
        [JsonIgnore]
        public virtual int CurrentFailOverrideTimes { get; set; } = 0;
        private int statusRequestFailTimes = 0;
        public virtual int StatusRequestFailTimes
        {
            get { return statusRequestFailTimes; }
            set
            {
                statusRequestFailTimes = value;
                if (statusRequestFailTimes >= MAX_STATUS_REQUEST_FAIL_TIMES)
                {
                    StatusRequestFailOverTimes?.Invoke(this, statusRequestFailTimes);
                }
            }
        }
        [JsonIgnore]
        public virtual double X_Axis { get; set; }
        [JsonIgnore]
        public virtual double Y_Axis { get; set; }
        //[JsonIgnore]
        //public virtual double DirctionAngle { get; set; }
        [JsonIgnore]
        public virtual double VehicleAngle { get; set; }


        [JsonIgnore]
        public virtual int BatteryTemperature { get; set; }
        [JsonIgnore]
        public virtual E_CMD_TYPE CmdType { get; set; } = default(E_CMD_TYPE);
        [JsonIgnore]
        public virtual E_CMD_STATUS vh_CMD_Status { get; set; }
        public virtual bool isIdling { get; private set; }
        public virtual bool isLongTimeInaction { get; private set; }
        public virtual bool isCanNotFindTheCharger { get; private set; }
        public virtual bool isAuto
        {
            get
            {
                return MODE_STATUS == VHModeStatus.AutoLocal ||
                       MODE_STATUS == VHModeStatus.AutoRemote;
            }
        }
        private bool istcpipconnect;
        public virtual bool isTcpIpConnect
        {
            get { return istcpipconnect; }
            set
            {
                if (istcpipconnect != value)
                {
                    istcpipconnect = value;
                    //OnPropertyChanged(BCFUtility.getPropertyName(() => this.isTcpIpConnect), VEHICLE_ID);
                    onConnectionStatusChange(istcpipconnect);
                }
            }
        }
        [JsonIgnore]
        public virtual bool HasExcuteTransferCommand
        {
            get { return !SCUtility.isEmpty(this.TRANSFER_ID); }
        }
        [JsonIgnore]
        public virtual bool HasCarryCST
        {
            get { return HAS_CST; }
        }

        [JsonIgnore]
        public object PositionRefresh_Sync = new object();
        [JsonIgnore]
        public object connection_sync = new object();
        [JsonIgnore]
        public bool isCommandEnding = false;

        [JsonIgnore]
        public virtual ReserveUnsuccessInfo CanNotReserveInfo { get; set; }
        [JsonIgnore]
        public virtual AvoidInfo VhAvoidInfo { get; set; }
        [JsonIgnore]
        public virtual List<string> ReservedSectionID { get; set; }
        [JsonIgnore]
        public virtual List<string> WillPassSectionID { get; set; }
        public virtual string sWillPassAddressIDs { get; set; }

        [JsonIgnore]
        public bool isHPRCar = false;

        private bool _isReadyForCarOut = false;
        [JsonIgnore]
        public bool IsReadyForCarOut
        {
            get
            {
                return _isReadyForCarOut;
            }
            set
            {
                if (_isReadyForCarOut != value)
                {
                    _isReadyForCarOut = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.IsReadyForCarOut));
                }
            }
        }

        #region Lock Object
        public object creatCmdAsyncObj = new object();
        #endregion

        #region Pause Status
        public Stopwatch watchObstacleTime = new Stopwatch();
        [JsonIgnore]
        public virtual bool IsReservePause
        {
            get { return RESERVE_PAUSE == VhStopSingle.StopSingleOn; }
            set { }
        }
        [JsonIgnore]
        [BaseElement(NonChangeFromOtherVO = true)]
        public VhStopSingle ObstacleStatus
        {
            get { return OBS_PAUSE; }
            set
            {
                if (OBS_PAUSE != value)
                {
                    OBS_PAUSE = value;
                    if (OBS_PAUSE == VhStopSingle.StopSingleOn)
                    {
                        watchObstacleTime.Restart();
                    }
                    else
                    {
                        watchObstacleTime.Stop();
                    }
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.ObstacleStatus));
                }
            }
        }
        [JsonIgnore]
        public virtual bool IsObstacle
        {
            get { return OBS_PAUSE == VhStopSingle.StopSingleOn; }
            set { }
        }
        public Stopwatch watchBlockTime = new Stopwatch();
        [JsonIgnore]
        [BaseElement(NonChangeFromOtherVO = true)]
        public VhStopSingle BlockingStatus
        {
            get { return BLOCK_PAUSE; }
            set
            {
                if (BLOCK_PAUSE != value)
                {
                    BLOCK_PAUSE = value;
                    if (BLOCK_PAUSE == VhStopSingle.StopSingleOn)
                    {
                        watchBlockTime.Restart();
                    }
                    else
                    {
                        watchBlockTime.Stop();
                    }
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.BlockingStatus));
                }
            }
        }
        [JsonIgnore]
        public virtual bool IsBlocking
        {
            get { return BLOCK_PAUSE == VhStopSingle.StopSingleOn; }
            set { }
        }
        public Stopwatch watchPauseTime = new Stopwatch();
        [JsonIgnore]
        [BaseElement(NonChangeFromOtherVO = true)]
        public VhStopSingle PauseStatus
        {
            get { return CMD_PAUSE; }
            set
            {
                if (CMD_PAUSE != value)
                {
                    CMD_PAUSE = value;
                    if (CMD_PAUSE == VhStopSingle.StopSingleOn)
                    {
                        watchPauseTime.Restart();
                    }
                    else
                    {
                        watchPauseTime.Stop();
                    }
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.PauseStatus));
                }
            }
        }
        [JsonIgnore]
        public virtual bool IsPause
        {
            get { return CMD_PAUSE == VhStopSingle.StopSingleOn; }
            set { }
        }
        [JsonIgnore]
        public virtual bool IsError
        {
            get { return ERROR == VhStopSingle.StopSingleOn; }
            set { }
        }

        public Stopwatch watchHIDTime = new Stopwatch();
        [JsonIgnore]
        [BaseElement(NonChangeFromOtherVO = true)]
        public VhStopSingle HIDStatus
        {
            get { return HID_PAUSE; }
            set
            {
                if (HID_PAUSE != value)
                {
                    HID_PAUSE = value;
                    if (HID_PAUSE == VhStopSingle.StopSingleOn)
                    {
                        watchHIDTime.Restart();
                    }
                    else
                    {
                        watchHIDTime.Stop();
                    }
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.HIDStatus));
                }
            }
        }
        [JsonIgnore]
        public virtual bool IsHIDPause
        {
            get { return HID_PAUSE == VhStopSingle.StopSingleOn; }
            set { }
        }

        #endregion Pause Status

        public void Action()
        {
            CurrentCommandExcuteTime.Restart();
        }
        public void Stop()
        {
            CurrentCommandExcuteTime.Reset();
        }
        #region Send Message
        public bool send_Str1(ID_1_HOST_BASIC_INFO_VERSION_REP send_gpb, out ID_101_HOST_BASIC_INFO_VERSION_RESPONSE receive_gpp)
        {
            ValueDefMapActionBase mapAction = null;
            mapAction = getExcuteMapAction();
            return mapAction.send_Str1(send_gpb, out receive_gpp);
        }
        public bool send_S13(ID_13_TAVELLING_DATA_REP send_gpp, out ID_113_TAVELLING_DATA_RESPONSE receive_gpp)
        {
            ValueDefMapActionBase mapAction = null;
            mapAction = getExcuteMapAction();
            return mapAction.send_Str13(send_gpp, out receive_gpp);
        }
        public bool send_S15(ID_15_SECTION_DATA_REP send_gpp, out ID_115_SECTION_DATA_RESPONSE receive_gpp)
        {
            ValueDefMapActionBase mapAction = null;
            mapAction = getExcuteMapAction();
            return mapAction.send_Str15(send_gpp, out receive_gpp);
        }
        public bool send_S17(ID_17_ADDRESS_DATA_REP send_gpp, out ID_117_ADDRESS_DATA_RESPONSE receive_gpp)
        {
            ValueDefMapActionBase mapAction = null;
            mapAction = getExcuteMapAction();
            return mapAction.send_Str17(send_gpp, out receive_gpp);
        }
        public bool send_S19(ID_19_SCALE_DATA_REP send_gpp, out ID_119_SCALE_DATA_RESPONSE receive_gpp)
        {
            ValueDefMapActionBase mapAction = null;
            mapAction = getExcuteMapAction();
            return mapAction.send_Str19(send_gpp, out receive_gpp);
        }

        public bool send_S21(ID_21_CONTROL_DATA_REP send_gpp, out ID_121_CONTROL_DATA_RESPONSE receive_gpp)
        {
            ValueDefMapActionBase mapAction = null;
            mapAction = getExcuteMapAction();
            return mapAction.send_Str21(send_gpp, out receive_gpp);
        }
        public bool send_S23(ID_23_GUIDE_DATA_REP send_gpp, out ID_123_GUIDE_DATA_RESPONSE receive_gpp)
        {
            ValueDefMapActionBase mapAction = null;
            mapAction = getExcuteMapAction();
            return mapAction.send_Str23(send_gpp, out receive_gpp);
        }


        public bool send_S61(ID_61_INDIVIDUAL_UPLOAD_REQ send_gpp, out ID_161_INDIVIDUAL_UPLOAD_RESPONSE receive_gpp)
        {
            ValueDefMapActionBase mapAction = null;
            mapAction = getExcuteMapAction();
            return mapAction.send_Str61(send_gpp, out receive_gpp);
        }
        public bool send_S63(ID_63_INDIVIDUAL_CHANGE_REQ send_gpp, out ID_163_INDIVIDUAL_CHANGE_RESPONSE receive_gpp)
        {
            ValueDefMapActionBase mapAction = null;
            mapAction = getExcuteMapAction();
            return mapAction.send_Str63(send_gpp, out receive_gpp);
        }
        public bool send_S41(ID_41_MODE_CHANGE_REQ send_gpp, out ID_141_MODE_CHANGE_RESPONSE receive_gpp)
        {
            ValueDefMapActionBase mapAction = null;
            mapAction = getExcuteMapAction();
            return mapAction.send_Str41(send_gpp, out receive_gpp);
        }
        public bool send_S43(ID_43_STATUS_REQUEST send_gpp, out ID_143_STATUS_RESPONSE receive_gpp)
        {
            ValueDefMapActionBase mapAction = null;
            mapAction = getExcuteMapAction();
            return mapAction.send_Str43(send_gpp, out receive_gpp);
        }
        public bool send_S45(ID_45_POWER_OPE_REQ send_gpp, out ID_145_POWER_OPE_RESPONSE receive_gpp)
        {
            ValueDefMapActionBase mapAction = null;
            mapAction = getExcuteMapAction();
            return mapAction.send_Str45(send_gpp, out receive_gpp);
        }
        public bool send_S91(ID_91_ALARM_RESET_REQUEST send_gpp, out ID_191_ALARM_RESET_RESPONSE receive_gpp)
        {
            ValueDefMapActionBase mapAction = null;
            mapAction = getExcuteMapAction();
            return mapAction.send_Str91(send_gpp, out receive_gpp);
        }


        public void registeredProcEvent()
        {
            getExcuteMapAction().RegisteredTcpIpProcEvent();
        }
        public void unRegisteredProcEvent()
        {
            getExcuteMapAction().UnRgisteredProcEvent();
        }

        public bool send_Str31(ID_31_TRANS_REQUEST send_gpp, out ID_131_TRANS_RESPONSE receive_gpp, out string reason)
        {
            ValueDefMapActionBase mapAction = null;
            mapAction = getExcuteMapAction();
            return mapAction.send_Str31(send_gpp, out receive_gpp, out reason);
        }
        public bool send_Str35(ID_35_CARRIER_ID_RENAME_REQUEST send_gpp, out ID_135_CARRIER_ID_RENAME_RESPONSE receive_gpp)
        {
            ValueDefMapActionBase mapAction = null;
            mapAction = getExcuteMapAction();
            return mapAction.send_Str35(send_gpp, out receive_gpp);
        }

        //public bool send_Str37(ID_37_TRANS_CANCEL_REQUEST send_gpp, out ID_137_TRANS_CANCEL_RESPONSE receive_gpp)
        //{
        //    ValueDefMapActionBase mapAction = null;
        //    mapAction = getExcuteMapAction();
        //    return mapAction.send_Str37(send_gpp, out receive_gpp);
        //}

        public bool sned_Str37(string cmd_id, CMDCancelType actType)
        {
            ValueDefMapActionBase mapAction = null;
            mapAction = getExcuteMapAction();
            return mapAction.send_Str37(cmd_id, actType);
        }

        public bool send_Str39(ID_39_PAUSE_REQUEST send_gpp, out ID_139_PAUSE_RESPONSE receive_gpp)
        {
            ValueDefMapActionBase mapAction = null;
            mapAction = getExcuteMapAction();
            return mapAction.send_Str39(send_gpp, out receive_gpp);
        }
        public void CatchPLCCSTInterfacelog()
        {
            ValueDefMapActionBase mapAction = null;
            mapAction = getExcuteMapAction();
            mapAction.doCatchPLCCSTInterfaceLog();
        }

        public bool send_Str71(ID_71_RANGE_TEACHING_REQUEST send_gpp, out ID_171_RANGE_TEACHING_RESPONSE receive_gpp)
        {
            ValueDefMapActionBase mapAction = null;
            mapAction = getExcuteMapAction();
            return mapAction.send_Str71(send_gpp, out receive_gpp);
        }
        public bool send_Str51(ID_51_AVOID_REQUEST send_gpp, out ID_151_AVOID_RESPONSE receive_gpp)
        {
            ValueDefMapActionBase mapAction = null;
            mapAction = getExcuteMapAction();
            return mapAction.send_Str51(send_gpp, out receive_gpp);
        }
        public bool sendMessage(WrapperMessage wrapper, bool isReply = false)
        {
            ValueDefMapActionBase mapAction = null;
            mapAction = getExcuteMapAction();
            return mapAction.snedMessage(wrapper, isReply);
        }
        #endregion Send Message
        private ValueDefMapActionBase getExcuteMapAction()
        {
            ValueDefMapActionBase mapAction;
            mapAction = this.getMapActionByIdentityKey(typeof(EQTcpIpMapAction).Name) as EQTcpIpMapAction;
            return mapAction;
        }
        public void initialParameter()
        {
            this.VEHICLE_ID = null;
            this.VEHICLE_TYPE = default(E_VH_TYPE);
            this.VEHICLE_ACC_DIST = 0;
            this.MANT_ACC_DIST = 0;
            this.MANT_DATE = null;
            this.GRIP_COUNT = 0;
            this.GRIP_MANT_COUNT = 0;
            this.GRIP_MANT_DATE = null;
            this.LAST_FULLY_CHARGED_TIME = null;
            this.IS_INSTALLED = false;
            this.INSTALLED_TIME = null;
            this.REMOVED_TIME = null;
        }

        //public void setCarrierLocationInfo(string location_id_r, string location_id_l)
        //{
        //    CarrierLocation = new List<Location>()
        //    {
        //        new Location(location_id_r),
        //        new Location(location_id_l),
        //    };
        //}


        #region TcpIpAgentInfo
        int CommunicationInterval_ms = 15000;
        public bool IsCommunication(BCFApplication bcfApp)
        {
            bool is_communication = false;
            Stopwatch fromLastCommTime = ITcpIpControl.StopWatch_FromTheLastCommTime(bcfApp, TcpIpAgentName);
            is_communication = fromLastCommTime.IsRunning ?
                fromLastCommTime.ElapsedMilliseconds < CommunicationInterval_ms : false;
            return is_communication;
        }
        //public bool IsCarreirExist(string carreirID)
        //{
        //    var location = this.CarrierLocation.
        //                        Where(loc => loc.HAS_CST && SCUtility.isMatche(loc.CST_ID, carreirID)).
        //                        FirstOrDefault();
        //    return location != null;
        //}
        //public (bool isExist, AVEHICLE.Location Location) getCarreirLocation(string carreirID)
        //{
        //    var location = this.CarrierLocation.
        //                        Where(loc => loc.HAS_CST && SCUtility.isMatche(loc.CST_ID, carreirID)).
        //                        FirstOrDefault();
        //    return (location != null, location);
        //}

        public void getAgentInfo(BCFApplication bcfApp,
            out bool IsCommunication, out bool IsConnections,
            out DateTime connTime, out TimeSpan accConnTime,
            out DateTime disConnTime, out TimeSpan accDisConnTime,
            out int disconnTimes, out int lostPackets)
        {
            Stopwatch fromLastCommTime = ITcpIpControl.StopWatch_FromTheLastCommTime(bcfApp, TcpIpAgentName);
            IsCommunication = fromLastCommTime.IsRunning ?
                fromLastCommTime.ElapsedMilliseconds < CommunicationInterval_ms : false;
            IsConnections = ITcpIpControl.IsConnection(bcfApp, TcpIpAgentName);
            connTime = ITcpIpControl.ConnectionTime(bcfApp, TcpIpAgentName);
            accConnTime = ITcpIpControl.StopWatch_ConnectionTime(bcfApp, TcpIpAgentName).Elapsed;
            disConnTime = ITcpIpControl.DisconnectionTime(bcfApp, TcpIpAgentName);
            accDisConnTime = ITcpIpControl.StopWatch_DisconnectionTime(bcfApp, TcpIpAgentName).Elapsed;
            disconnTimes = ITcpIpControl.DisconnectionTimes(bcfApp, TcpIpAgentName);
            lostPackets = ITcpIpControl.NumberOfPacketsLost(bcfApp, TcpIpAgentName);
        }

        public bool IsTcpIpListening(BCFApplication bcfApp)
        {
            bool IsListening = false;
            int local_port = ITcpIpControl.getLocalPortNum(bcfApp, TcpIpAgentName);
            if (local_port != 0)
            {
                iibg3k0.ttc.Common.TCPIP.TcpIpServer tcpip_server = bcfApp.getTcpIpServerByPortNum(local_port);
                if (tcpip_server != null)
                {
                    IsListening = tcpip_server.IsListening;
                }
            }
            return IsListening;
        }

        public bool IsStanby(BLL.CMDBLL cmdBLL)
        {
            if (!this.isTcpIpConnect)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleBLL), Device: "OHTC",
                   Data: $"vh id:{this.VEHICLE_ID} of tcp ip connection is :{this.isTcpIpConnect}" +
                         $"so filter it out",
                   VehicleID: this.VEHICLE_ID,
                   CarrierID: this.CST_ID);
                return false;
            }
            if (this.IsError)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleBLL), Device: "OHTC",
                   Data: $"vh id:{this.VEHICLE_ID} of error flag is :{this.IsError}" +
                         $"so filter it out",
                   VehicleID: this.VEHICLE_ID,
                   CarrierID: this.CST_ID);
                return false;
            }
            if (this.MODE_STATUS != VHModeStatus.AutoRemote)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleBLL), Device: "OHTC",
                   Data: $"vh id:{this.VEHICLE_ID} current mode status is {this.MODE_STATUS}," +
                         $"so filter it out",
                   VehicleID: this.VEHICLE_ID,
                   CarrierID: this.CST_ID);
                return false;
            }
            if (SCUtility.isEmpty(this.CUR_ADR_ID))
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleBLL), Device: "OHTC",
                   Data: $"vh id:{this.VEHICLE_ID} current address is empty," +
                         $"so filter it out",
                   VehicleID: this.VEHICLE_ID,
                   CarrierID: this.CST_ID);
                return false;
            }
            //if (isCommandEnding)
            //{
            //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleBLL), Device: "OHTC",
            //       Data: $"vh id:{this.VEHICLE_ID} is command ending," +
            //             $"so filter it out",
            //       VehicleID: this.VEHICLE_ID,
            //       CarrierID: this.CST_ID);
            //    return false;
            //}

            var has_assign_cmd = cmdBLL.hasAssignCmdIgnoreMove(this);
            if (has_assign_cmd)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleBLL), Device: "OHTC",
                   Data: $"vh id:{this.VEHICLE_ID} has assign command cmd," +
                         $"so filter it out",
                   VehicleID: this.VEHICLE_ID,
                   CarrierID: this.CST_ID);
                return false;
            }
            return true;
        }


        public int getPortNum(BCFApplication bcfApp)
        {
            return ITcpIpControl.getLocalPortNum(bcfApp, TcpIpAgentName);
        }
        internal string getIPAddress(BCFApplication bcfApp)
        {
            if (SCUtility.isEmpty(TcpIpAgentName))
            {
                return string.Empty;
            }
            return ITcpIpControl.getRemoteIPAddress(bcfApp, TcpIpAgentName);
        }

        internal double getFromTheLastCommTime(BCFApplication bcfApp)
        {
            return ITcpIpControl.StopWatch_FromTheLastCommTime(bcfApp, TcpIpAgentName).Elapsed.TotalSeconds;

        }
        internal double getConnectionIntervalTime(BCFApplication bcfApp)
        {
            return ITcpIpControl.StopWatch_ConnectionTime(bcfApp, TcpIpAgentName).Elapsed.TotalSeconds;
        }
        internal double getDisconnectionIntervalTime(BCFApplication bcfApp)
        {
            return ITcpIpControl.StopWatch_DisconnectionTime(bcfApp, TcpIpAgentName).Elapsed.TotalSeconds;
        }

        #endregion TcpIpAgentInfo

        public override void doShareMemoryInit(BCFAppConstants.RUN_LEVEL runLevel)
        {
            foreach (IValueDefMapAction action in valueDefMapActionDic.Values)
            {
                action.doShareMemoryInit(runLevel);
            }

        }
        [JsonIgnore]
        [BaseElement(NonChangeFromOtherVO = true)]
        public virtual string NODE_ID { get; set; }
        [JsonIgnore]
        [BaseElement(NonChangeFromOtherVO = true)]
        public override string Version { get { return base.Version; } }
        [JsonIgnore]
        [BaseElement(NonChangeFromOtherVO = true)]
        public override string EqptObjectCate { get { return base.EqptObjectCate; } }
        [JsonIgnore]
        [BaseElement(NonChangeFromOtherVO = true)]
        public override string SECSAgentName { get { return base.SECSAgentName; } set { base.SECSAgentName = value; } }
        [JsonIgnore]
        [BaseElement(NonChangeFromOtherVO = true)]
        public override string TcpIpAgentName { get { return base.TcpIpAgentName; } set { base.TcpIpAgentName = value; } }
        //
        // 摘要:
        //     真實的ID
        [JsonIgnore]
        [BaseElement(NonChangeFromOtherVO = true)]
        public override string Real_ID { get; set; }
        [JsonIgnore]
        [BaseElement(NonChangeFromOtherVO = true)]
        public int Num { get; set; }


        #region Vehicle state machine
        void TransitionedHandler(Stateless.StateMachine<VehicleState, VehicleTrigger>.Transition transition)
        {
            string Destination = transition.Destination.ToString();
            string Source = transition.Source.ToString();
            string Trigger = transition.Trigger.ToString();
            string IsReentry = transition.IsReentry.ToString();

            LogHelper.Log(logger: NLog.LogManager.GetCurrentClassLogger(), LogLevel: NLog.LogLevel.Debug, Class: nameof(AVEHICLE), Device: DEVICE_NAME_AGV,
                           Data: $"Vh:{VEHICLE_ID} message state,From:{Source} to:{Destination} by:{Trigger}.IsReentry:{IsReentry}",
                           VehicleID: VEHICLE_ID,
                   CarrierID: this.CST_ID);
        }
        void UnhandledTriggerHandler(VehicleState state, VehicleTrigger trigger)
        {
            string SourceState = state.ToString();
            string Trigger = trigger.ToString();

            LogHelper.Log(logger: NLog.LogManager.GetCurrentClassLogger(), LogLevel: NLog.LogLevel.Debug, Class: nameof(AVEHICLE), Device: DEVICE_NAME_AGV,
                           Data: $"Vh:{VEHICLE_ID} message state ,unhandled trigger happend ,source state:{SourceState} trigger:{Trigger}",
                           VehicleID: VEHICLE_ID,
                   CarrierID: this.CST_ID);
        }

        public class VehicleStateMachine : StateMachine<VehicleState, VehicleTrigger>
        {
            public VehicleStateMachine(Func<VehicleState> stateAccessor, Action<VehicleState> stateMutator)
                : base(stateAccessor, stateMutator)
            {
                VehicleStateMachineConfigInitial();
            }
            internal IEnumerable<VehicleTrigger> getPermittedTriggers()//回傳當前狀態可以進行的Trigger，且會檢查GaurdClause。
            {
                return this.PermittedTriggers;
            }


            internal VehicleState getCurrentState()//回傳當前的狀態
            {
                return this.State;
            }
            public List<string> getNextStateStrList()
            {
                List<string> nextStateStrList = new List<string>();
                foreach (VehicleTrigger item in this.PermittedTriggers)
                {
                    nextStateStrList.Add(item.ToString());
                }
                return nextStateStrList;
            }
            private void VehicleStateMachineConfigInitial()
            {
                this.Configure(VehicleState.NOT_ASSIGNED)
                    .PermitIf(VehicleTrigger.VehicleAssign, VehicleState.ASSIGNED, () => VehicleAssignGC())//guardClause為真才會執行狀態變化
                    .PermitIf(VehicleTrigger.VechileRemove, VehicleState.REMOVED, () => VechileRemoveGC());//guardClause為真才會執行狀態變化
                this.Configure(VehicleState.ASSIGNED).OnEntry(() => this.Fire(VehicleTrigger.VehicleAssign))
                    .PermitIf(VehicleTrigger.VehicleAssign, VehicleState.ENROUTE)
                    .PermitIf(VehicleTrigger.VehicleUnassign, VehicleState.NOT_ASSIGNED);
                this.Configure(VehicleState.ENROUTE).SubstateOf(VehicleState.ASSIGNED)
                    .PermitIf(VehicleTrigger.VehicleArrive, VehicleState.PARKED, () => VehicleArriveGC());//guardClause為真才會執行狀態變化
                                                                                                          //.PermitIf(VehicleTrigger.VehicleUnassign, VehicleState.NOT_ASSIGNED, () => VehicleUnassignGC());//guardClause為真才會執行狀態變化
                this.Configure(VehicleState.PARKED).SubstateOf(VehicleState.ASSIGNED)
                    .PermitIf(VehicleTrigger.VehicleDepart, VehicleState.ENROUTE, () => VehicleDepartGC())//guardClause為真才會執行狀態變化
                    .PermitIf(VehicleTrigger.VehicleAcquireStart, VehicleState.ACQUIRING, () => VehicleAcquireStartGC())//guardClause為真才會執行狀態變化
                    .PermitIf(VehicleTrigger.VehicleDepositStart, VehicleState.DEPOSITING, () => VehicleDepositStartGC());//guardClause為真才會執行狀態變化
                this.Configure(VehicleState.ACQUIRING).SubstateOf(VehicleState.ASSIGNED)
                    .PermitIf(VehicleTrigger.VehilceAcquireComplete, VehicleState.PARKED, () => VehilceAcquireCompleteGC())//guardClause為真才會執行狀態變化
                    .PermitIf(VehicleTrigger.VehicleDepositStart, VehicleState.DEPOSITING, () => VehicleDepositStartGC());//guardClause為真才會執行狀態變化
                this.Configure(VehicleState.DEPOSITING).SubstateOf(VehicleState.ASSIGNED)
                    .PermitIf(VehicleTrigger.VehicleDepositComplete, VehicleState.PARKED, () => VehicleDepositCompleteGC())//guardClause為真才會執行狀態變化
                    .PermitIf(VehicleTrigger.VehicleAcquireStart, VehicleState.ACQUIRING, () => VehicleAcquireStartGC());//guardClause為真才會執行狀態變化
                this.Configure(VehicleState.REMOVED)
                    .PermitIf(VehicleTrigger.VehicleInstall, VehicleState.NOT_ASSIGNED, () => VehicleInstallGC());//guardClause為真才會執行狀態變化

            }

            private bool VehicleArriveGC()
            {
                return true;
            }
            private bool VehicleUnassignGC()
            {
                return true;
            }
            private bool VehicleDepartGC()
            {
                return true;
            }
            private bool VehicleAcquireStartGC()
            {
                return true;
            }
            private bool VehicleDepositStartGC()
            {
                return true;
            }
            private bool VehilceAcquireCompleteGC()
            {
                return true;
            }
            private bool VehicleDepositCompleteGC()
            {
                return true;
            }
            private bool VehicleAssignGC()
            {
                return true;
            }
            private bool VechileRemoveGC()
            {
                return true;
            }
            private bool VehicleInstallGC()
            {
                return true;
            }
        }

        public enum VehicleState //有哪些State
        {
            REMOVED = 1,
            NOT_ASSIGNED = 2,
            ENROUTE = 3,
            PARKED = 4,
            ACQUIRING = 5,
            DEPOSITING = 6,
            ASSIGNED = 99
        }

        public enum VehicleTrigger //有哪些Trigger
        {
            VehicleArrive,
            VehicleDepart,
            VehicleAcquireStart,
            VehilceAcquireComplete,
            VehicleDepositStart,
            VehicleDepositComplete,
            VehicleUnassign,
            VehicleAssign,
            VechileRemove,
            VehicleInstall
        }
        public bool VehicleArrive()
        {
            try
            {
                if (vhStateMachine.CanFire(VehicleTrigger.VehicleArrive))//檢查當前狀態能否進行這個Trigger
                {
                    vhStateMachine.Fire(VehicleTrigger.VehicleArrive);//進行Trigger

                    //可以在這邊做事情

                    return true;

                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool VehicleDepart()
        {
            try
            {
                if (vhStateMachine.CanFire(VehicleTrigger.VehicleDepart))//檢查當前狀態能否進行這個Trigger
                {
                    vhStateMachine.Fire(VehicleTrigger.VehicleDepart);//進行Trigger

                    //可以在這邊做事情

                    return true;

                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool VehicleAcquireStart()
        {
            try
            {
                if (vhStateMachine.CanFire(VehicleTrigger.VehicleAcquireStart))//檢查當前狀態能否進行這個Trigger
                {
                    vhStateMachine.Fire(VehicleTrigger.VehicleAcquireStart);//進行Trigger

                    //可以在這邊做事情

                    return true;

                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool VehilceAcquireComplete()
        {
            try
            {
                if (vhStateMachine.CanFire(VehicleTrigger.VehilceAcquireComplete))//檢查當前狀態能否進行這個Trigger
                {
                    vhStateMachine.Fire(VehicleTrigger.VehilceAcquireComplete);//進行Trigger

                    //可以在這邊做事情

                    return true;

                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool VehicleDepositStart()
        {
            try
            {
                if (vhStateMachine.CanFire(VehicleTrigger.VehicleDepositStart))//檢查當前狀態能否進行這個Trigger
                {
                    vhStateMachine.Fire(VehicleTrigger.VehicleDepositStart);//進行Trigger

                    //可以在這邊做事情

                    return true;

                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool VehicleDepositComplete()
        {
            try
            {
                if (vhStateMachine.CanFire(VehicleTrigger.VehicleDepositComplete))//檢查當前狀態能否進行這個Trigger
                {
                    vhStateMachine.Fire(VehicleTrigger.VehicleDepositComplete);//進行Trigger

                    //可以在這邊做事情

                    return true;

                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool VehicleUnassign()
        {
            try
            {
                if (vhStateMachine.CanFire(VehicleTrigger.VehicleUnassign))//檢查當前狀態能否進行這個Trigger
                {
                    vhStateMachine.Fire(VehicleTrigger.VehicleUnassign);//進行Trigger

                    //可以在這邊做事情

                    return true;

                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool VehicleAssign()
        {
            try
            {
                if (vhStateMachine.CanFire(VehicleTrigger.VehicleAssign))//檢查當前狀態能否進行這個Trigger
                {
                    vhStateMachine.Fire(VehicleTrigger.VehicleAssign);//進行Trigger

                    //可以在這邊做事情

                    return true;

                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool VehicleRemove()
        {
            try
            {
                if (vhStateMachine.CanFire(VehicleTrigger.VechileRemove))//檢查當前狀態能否進行這個Trigger
                {
                    vhStateMachine.Fire(VehicleTrigger.VechileRemove);//進行Trigger

                    //可以在這邊做事情

                    return true;

                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool VehicleInstall()
        {
            try
            {
                if (vhStateMachine.CanFire(VehicleTrigger.VehicleInstall))//檢查當前狀態能否進行這個Trigger
                {
                    vhStateMachine.Fire(VehicleTrigger.VehicleInstall);//進行Trigger

                    //可以在這邊做事情

                    return true;

                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion Vehicle state machine


        public class VehicleTimerAction : ITimerAction
        {
            private static Logger logger = LogManager.GetCurrentClassLogger();
            AVEHICLE vh = null;
            SCApplication scApp = null;
            public VehicleTimerAction(AVEHICLE _vh, string name, long intervalMilliSec)
                : base(name, intervalMilliSec)
            {
                vh = _vh;
            }

            public override void initStart()
            {
                scApp = SCApplication.getInstance();
            }

            private long syncPoint = 0;
            public override void doProcess(object obj)
            {
                if (System.Threading.Interlocked.Exchange(ref syncPoint, 1) == 0)
                {
                    try
                    {
                        //1.檢查是否已經大於一定時間沒有進行通訊
                        double from_last_comm_time = vh.getFromTheLastCommTime(scApp.getBCFApplication());
                        if (from_last_comm_time > AVEHICLE.MAX_ALLOW_NO_COMMUNICATION_TIME_SECOND)
                        {
                            vh.onLongTimeNoCommuncation();
                        }
                        //檢查斷線時間是否已經大於容許的最大值
                        double disconnection_time = vh.getDisconnectionIntervalTime(scApp.getBCFApplication());
                        if (disconnection_time > AVEHICLE.MAX_ALLOW_NO_CONNECTION_TIME_SECOND)
                        {
                            vh.onLongTimeDisConnection();
                        }
                        if (!vh.isTcpIpConnect) return;
                        //double action_time = vh.CurrentCommandExcuteTime.Elapsed.TotalSeconds; //todo 需確認如何修改成正確的命令Timeout
                        //if (action_time > AVEHICLE.MAX_ALLOW_ACTION_TIME_SECOND)
                        //{
                        //    vh.onLongTimeInaction(vh.OHTC_CMD);
                        //}
                        IdleTimeCheck();
                        //if (!vh.isIdling && vh.IdleTimer.ElapsedMilliseconds > AVEHICLE.MAX_ALLOW_IDLE_TIME_MILLISECOND)
                        if (!vh.isIdling && vh.IdleTimer.ElapsedMilliseconds > SystemParameter.AllowVhIdleTime_ms)
                        {
                            vh.onVehicleIdle();
                        }
                        ALINE line = scApp.getEQObjCacheManager().getLine();
                        CommandActionTimeCheck(line);
                        if (!vh.isLongTimeInaction && vh.CommandActionTimer.ElapsedMilliseconds > AVEHICLE.MAX_ALLOW_ACTION_TIME_MILLISECOND)
                        {
                            var currnet_excute_ids = getVhCurrentExcuteCommandID(line.CurrentExcuteCommand);
                            vh.onLongTimeInaction(currnet_excute_ids);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(AVEHICLE), Device: "OHTC",
                           Data: ex,
                           VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                    }
                    finally
                    {
                        System.Threading.Interlocked.Exchange(ref syncPoint, 0);
                    }

                }
            }

            private void IdleTimeCheck()
            {
                if (vh.ACT_STATUS == VHActionStatus.NoCommand)
                {
                    if (!vh.IdleTimer.IsRunning)
                    {
                        vh.IdleTimer.Restart();
                    }
                }
                else
                {
                    if (vh.IdleTimer.IsRunning)
                    {
                        vh.IdleTimer.Reset();
                    }
                    vh.isIdling = false;
                }
            }

            private void CommandActionTimeCheck(ALINE line)
            {
                if (line == null) return;
                var cmds = line.CurrentExcuteCommand;
                if (cmds == null) return;
                bool has_command_excute = getVhCurrentExcuteCommandCount(cmds);
                if (has_command_excute)
                {
                    if (!vh.CommandActionTimer.IsRunning)
                    {
                        vh.CommandActionTimer.Restart();
                    }
                }
                else
                {
                    if (vh.CommandActionTimer.IsRunning)
                    {
                        vh.CommandActionTimer.Reset();
                    }
                    vh.isLongTimeInaction = false;
                }
            }

            private bool getVhCurrentExcuteCommandCount(List<ACMD> cmds)
            {
                return cmds.Where(cmd => SCUtility.isMatche(cmd.VH_ID, vh.VEHICLE_ID))
                           .Count() > 0;
            }
            private List<string> getVhCurrentExcuteCommandID(List<ACMD> cmds)
            {
                return cmds.Where(cmd => SCUtility.isMatche(cmd.VH_ID, vh.VEHICLE_ID))
                           .Select(cmd => cmd.ID)
                           .ToList();
            }
        }
        public class ReserveUnsuccessInfo
        {
            public ReserveUnsuccessInfo(string vhID, string adrID, string secID)
            {
                ReservedVhID = vhID;
                ReservedAdrID = SCUtility.Trim(adrID);
                ReservedSectionID = SCUtility.Trim(secID);
            }
            public string ReservedVhID { get; }
            public string ReservedAdrID { get; }
            public string ReservedSectionID { get; }
        }
        public class AvoidInfo
        {
            public string BlockedSectionID { get; }
            public string BlockedVehicleID { get; }
            public AvoidInfo(string blockedSectionID, string blockedVehicleID)
            {
                BlockedSectionID = SCUtility.Trim(blockedSectionID, true);
                BlockedVehicleID = SCUtility.Trim(blockedVehicleID, true);
            }
        }

        public class Location
        {
            public string ID { get; set; }
            public bool HAS_CST { get; private set; }
            public string CST_ID { get; private set; }

            public Location(string id)
            {
                ID = id;
            }

            public void setCstID(string cst_id)
            {
                CST_ID = cst_id;
            }
            public void setHasCst(bool has_cst)
            {
                HAS_CST = has_cst;
            }
        }

    }
}
