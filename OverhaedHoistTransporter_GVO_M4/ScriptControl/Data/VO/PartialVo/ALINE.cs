using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.bcf.Data.TimerAction;
using com.mirle.ibg3k0.bcf.Data.ValueDefMapAction;
using com.mirle.ibg3k0.bcf.Data.VO;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.BLL;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.SECS;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.Data.VO.Interface;
using com.mirle.ibg3k0.sc.ObjectRelay;
using NLog;
using Stateless;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc
{
    public partial class ALINE : BaseEQObject, IAlarmHisList
    {
        public event EventHandler<EventArgs> LineStatusChange;
        public event EventHandler LongTimeNoCommuncation;

        /// <summary>
        /// 最大允許跟MCS沒有通訊的時間
        /// </summary>
        public static UInt16 MAX_ALLOW_NO_COMMUNICATION_TIME_FOR_MCS_SECOND { get; private set; } = 60;
        public Stopwatch CommunicationIntervalWithMCS = new Stopwatch();

        LineTimerAction lineTimer = null;

        public ALINE()
        {
            TSC_state_machine = new TSCStateMachine(TSCState.NONE);
            //lineTimer = new LineTimerAction(this, "", 1000);
            //lineTimer.start();
            //AGVCInitialComplete();
            //StopWatch_mcsConnectionTime = new Stopwatch();
            //StopWatch_mcsDisconnectionTime = new Stopwatch();
        }
        public void TimerActionStart()
        {
            lineTimer = new LineTimerAction(this, "LineTimerAction", 1000);
            lineTimer.start();
        }

        public void onLongTimeNoCommuncation()
        {
            LongTimeNoCommuncation?.Invoke(this, EventArgs.Empty);
        }


        public TSCStateMachine TSC_state_machine;
        private AlarmHisList alarmHisList = new AlarmHisList();
        private string current_park_type;
        public virtual string Currnet_Park_Type
        {
            get { return current_park_type; }
            set
            {
                if (current_park_type != value)
                {
                    current_park_type = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.Currnet_Park_Type));
                }
            }
        }
        private string current_cycle_type;
        public virtual string Currnet_Cycle_Type
        {
            get { return current_cycle_type; }
            set
            {
                if (current_cycle_type != value)
                {
                    current_cycle_type = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.Currnet_Cycle_Type));
                }
            }
        }

        private SCAppConstants.AppServiceMode servicemode;
        public SCAppConstants.AppServiceMode ServiceMode
        {
            get { return servicemode; }
            set
            {
                if (servicemode != value)
                {
                    servicemode = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.ServiceMode));
                }
            }
        }
        private bool serverPreStop = false;
        public bool ServerPreStop
        {
            get { return serverPreStop; }
            set
            {
                if (serverPreStop != value)
                {
                    serverPreStop = value;
                }
            }
        }
        private SCAppConstants.LinkStatus secs_link_stat;
        [BaseElement(NonChangeFromOtherVO = true)]
        public virtual SCAppConstants.LinkStatus Secs_Link_Stat
        {
            get { return secs_link_stat; }
            set
            {
                if (secs_link_stat != value)
                {
                    secs_link_stat = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.Secs_Link_Stat));
                }
            }
        }

        private SCAppConstants.LinkStatus redis_link_stat;
        [BaseElement(NonChangeFromOtherVO = true)]
        public virtual SCAppConstants.LinkStatus Redis_Link_Stat
        {
            get { return redis_link_stat; }
            set
            {
                if (redis_link_stat != value)
                {
                    redis_link_stat = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.Redis_Link_Stat));
                }
            }
        }

        private SCAppConstants.ExistStatus detectionsystemexist = SCAppConstants.ExistStatus.NoExist;
        [BaseElement(NonChangeFromOtherVO = true)]
        public virtual SCAppConstants.ExistStatus DetectionSystemExist
        {
            get { return detectionsystemexist; }
            set
            {
                if (detectionsystemexist != value)
                {
                    detectionsystemexist = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.DetectionSystemExist));
                }
            }
        }

        private bool isearthquakehappend;
        [BaseElement(NonChangeFromOtherVO = true)]
        public virtual bool IsEarthquakeHappend
        {
            get { return isearthquakehappend; }
            set
            {
                if (isearthquakehappend != value)
                {
                    isearthquakehappend = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.IsEarthquakeHappend));
                }
            }
        }


        private bool segmentpredisableexcuting;
        [BaseElement(NonChangeFromOtherVO = true)]
        public virtual bool SegmentPreDisableExcuting
        {
            get { return segmentpredisableexcuting; }
            set
            {
                if (segmentpredisableexcuting != value)
                {
                    segmentpredisableexcuting = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.SegmentPreDisableExcuting));
                }
            }
        }

        public Data.SECSDriver.IBSEMDriver getUsingMapAction()
        {
            if (valueDefMapActionDic.Count > 0)
            {
                string using_map_aaction = valueDefMapActionDic.First().Key;

                var mapAction = this.getMapActionByIdentityKey(using_map_aaction) as Data.SECSDriver.IBSEMDriver;
                return mapAction;
            }
            else
            {
                return null;
            }

        }



        private Boolean establishComm;
        public virtual Boolean EstablishComm
        {
            get { return establishComm; }
            set
            {
                establishComm = value;
                OnPropertyChanged(BCFUtility.getPropertyName(() => this.EstablishComm));
            }
        }
        public DateTime mcsConnectionTime { get; private set; }
        public DateTime mcsDisconnectionTime { get; private set; }
        public Stopwatch StopWatch_mcsConnectionTime { get; set; }
        public Stopwatch StopWatch_mcsDisconnectionTime { get; set; }
        public void connInfoUpdate_Connection()
        {
            mcsConnectionTime = DateTime.Now;
            StopWatch_mcsConnectionTime.Restart();
            StopWatch_mcsDisconnectionTime.Stop();
        }
        public void connInfoUpdate_Disconnection()
        {
            mcsDisconnectionTime = DateTime.Now;
            StopWatch_mcsConnectionTime.Stop();
            StopWatch_mcsDisconnectionTime.Restart();
        }

        private SCAppConstants.LineHostControlState.HostControlState host_control_state = SCAppConstants.LineHostControlState.HostControlState.EQ_Off_line;
        public virtual SCAppConstants.LineHostControlState.HostControlState Host_Control_State
        {
            get
            {
                return host_control_state;
            }
            set
            {
                if (host_control_state != value)
                {
                    host_control_state = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.Host_Control_State));
                }
            }
        }

        public List<VTRANSFER> CurrentExcuteTransferCommand = null;
        public List<ACMD> CurrentExcuteCommand = null;

        private TSCState scstate = TSCState.NONE;
        public virtual TSCState SCStats
        {
            get { return scstate; }
            set
            {
                if (scstate != value)
                {
                    scstate = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.SCStats));
                }
            }
        }
        public const string CONTEXT_KEY_WORD_LINE_STATUS_HAS_CHANGE = "LineStatusChange";
        private UInt16 currntVehicleModeAutoRemoteCount = 0;
        public UInt16 CurrntVehicleModeAutoRemoteCount
        {
            get
            { return currntVehicleModeAutoRemoteCount; }
            set
            {
                if (currntVehicleModeAutoRemoteCount != value)
                {
                    currntVehicleModeAutoRemoteCount = value;
                    SCUtility.setCallContext(CONTEXT_KEY_WORD_LINE_STATUS_HAS_CHANGE, true);
                }
            }
        }
        private UInt16 currntVehicleModeAutoLoaclCount = 0;
        public UInt16 CurrntVehicleModeAutoLoaclCount
        {
            get
            { return currntVehicleModeAutoLoaclCount; }
            set
            {
                if (currntVehicleModeAutoLoaclCount != value)
                {
                    currntVehicleModeAutoLoaclCount = value;
                    SCUtility.setCallContext(CONTEXT_KEY_WORD_LINE_STATUS_HAS_CHANGE, true);
                }
            }
        }
        private UInt16 currntVehicleStatusIdelCounr = 0;
        public UInt16 CurrntVehicleStatusIdelCount
        {
            get
            { return currntVehicleStatusIdelCounr; }
            set
            {
                if (currntVehicleStatusIdelCounr != value)
                {
                    currntVehicleStatusIdelCounr = value;
                    SCUtility.setCallContext(CONTEXT_KEY_WORD_LINE_STATUS_HAS_CHANGE, true);
                }
            }
        }
        private UInt16 currntVehicleStatusErrorCounr = 0;
        public UInt16 CurrntVehicleStatusErrorCount
        {
            get
            { return currntVehicleStatusErrorCounr; }
            set
            {
                if (currntVehicleStatusErrorCounr != value)
                {
                    currntVehicleStatusErrorCounr = value;
                    SCUtility.setCallContext(CONTEXT_KEY_WORD_LINE_STATUS_HAS_CHANGE, true);
                }
            }
        }

        private UInt16 currntCSTStatueTransferCount = 0;
        public UInt16 CurrntCSTStatueTransferCount
        {
            get
            { return currntCSTStatueTransferCount; }
            set
            {
                if (currntCSTStatueTransferCount != value)
                {
                    currntCSTStatueTransferCount = value;
                    SCUtility.setCallContext(CONTEXT_KEY_WORD_LINE_STATUS_HAS_CHANGE, true);
                }
            }
        }
        private UInt16 currntCSTStatueWaitingCount = 0;
        public UInt16 CurrntCSTStatueWaitingCount
        {
            get
            { return currntCSTStatueWaitingCount; }
            set
            {
                if (currntCSTStatueWaitingCount != value)
                {
                    currntCSTStatueWaitingCount = value;
                    SCUtility.setCallContext(CONTEXT_KEY_WORD_LINE_STATUS_HAS_CHANGE, true);
                }
            }
        }

        private UInt16 currntHostCommandTransferStatueAssignedCount = 0;
        public UInt16 CurrntHostCommandTransferStatueAssignedCount
        {
            get
            { return currntHostCommandTransferStatueAssignedCount; }
            set
            {
                if (currntHostCommandTransferStatueAssignedCount != value)
                {
                    currntHostCommandTransferStatueAssignedCount = value;
                    SCUtility.setCallContext(CONTEXT_KEY_WORD_LINE_STATUS_HAS_CHANGE, true);
                }
            }
        }
        private UInt16 currntHostCommandTransferStatueWaitingCounr = 0;
        public UInt16 CurrntHostCommandTransferStatueWaitingCounr
        {
            get
            { return currntHostCommandTransferStatueWaitingCounr; }
            set
            {
                if (currntHostCommandTransferStatueWaitingCounr != value)
                {
                    currntHostCommandTransferStatueWaitingCounr = value;
                    SCUtility.setCallContext(CONTEXT_KEY_WORD_LINE_STATUS_HAS_CHANGE, true);
                }
            }
        }

        public void NotifyLineStatusChange()
        {
            LineStatusChange?.Invoke(this, null);
        }


        public override void doShareMemoryInit(BCFAppConstants.RUN_LEVEL runLevel)
        {
            foreach (IValueDefMapAction action in valueDefMapActionDic.Values)
            {
                action.doShareMemoryInit(runLevel);
            }
            //對sub eqpt進行初始化
            List<AZONE> subZoneList = SCApplication.getInstance().getEQObjCacheManager().getZoneListByLine();
            if (subZoneList != null)
            {
                foreach (AZONE zone in subZoneList)
                {
                    zone.doShareMemoryInit(runLevel);
                }
            }
        }
        public virtual void addAlarmHis(ALARM AlarmHis)
        {
            alarmHisList.addAlarmHis(AlarmHis);
        }
        public virtual void resetAlarmHis(List<ALARM> AlarmHisList)
        {
            alarmHisList.resetAlarmHis(AlarmHisList);
        }
        public override string Version { get { return base.Version; } }
        public override string EqptObjectCate { get { return base.EqptObjectCate; } }
        [BaseElement(NonChangeFromOtherVO = true)]
        public override string SECSAgentName { get { return base.SECSAgentName; } set { base.SECSAgentName = value; } }
        [BaseElement(NonChangeFromOtherVO = true)]
        public override string TcpIpAgentName { get { return base.TcpIpAgentName; } set { base.TcpIpAgentName = value; } }
        [BaseElement(NonChangeFromOtherVO = true)]
        public override string Real_ID { get; set; }

        private bool isAlarmHappened = false;
        public bool IsAlarmHappened
        {
            get { return isAlarmHappened; }
            set
            {
                if (isAlarmHappened != value)
                {
                    isAlarmHappened = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.IsAlarmHappened));
                }
            }
        }


        #region TSC state machine

        public class TSCStateMachine : StateMachine<TSCState, TSCTrigger>
        {
            public TSCStateMachine(TSCState state)
                : base(state)
            {
                TSCStateMachineConfigInitial();
            }
            internal IEnumerable<TSCTrigger> getPermittedTriggers()//回傳當前狀態可以進行的Trigger，且會檢查GaurdClause。
            {
                return this.PermittedTriggers;
            }

            internal TSCState getCurrentState()//回傳當前的狀態
            {
                return this.State;
            }
            public List<string> getNextStateStrList()
            {
                List<string> nextStateStrList = new List<string>();
                foreach (TSCTrigger item in this.PermittedTriggers)
                {
                    nextStateStrList.Add(item.ToString());
                }
                return nextStateStrList;
            }
            private void TSCStateMachineConfigInitial()
            {
                //this.Configure(TSCState.NONE)
                //    .PermitIf(TSCTrigger.AGVCInitial, TSCState.TSC_INIT, () => AGVCInitialGC());//guardClause為真才會執行狀態變化
                this.Configure(TSCState.NONE)
                    .PermitIf(TSCTrigger.OHTCInitial, TSCState.IN_STATUS, () => OHTCInitialGC());//guardClause為真才會執行狀態變化
                this.Configure(TSCState.IN_STATUS).OnEntry(() => this.Fire(TSCTrigger.OHTCInitial))
                    .PermitIf(TSCTrigger.OHTCInitial, TSCState.TSC_INIT, () => OHTCInitialGC());//guardClause為真才會執行狀態變化

                this.Configure(TSCState.TSC_INIT).SubstateOf(TSCState.IN_STATUS)
                    .PermitIf(TSCTrigger.StartUpSuccess, TSCState.PAUSED, () => StartUpSuccessGC());//guardClause為真才會執行狀態變化
                this.Configure(TSCState.PAUSING).SubstateOf(TSCState.IN_STATUS)
                    .PermitIf(TSCTrigger.ResumeAuto, TSCState.AUTO, () => ResumeAutoGC())//guardClause為真才會執行狀態變化
                    .PermitIf(TSCTrigger.PauseComplete, TSCState.PAUSED, () => PauseCompleteGC());//guardClause為真才會執行狀態變化
                this.Configure(TSCState.PAUSED).SubstateOf(TSCState.IN_STATUS)
                    .PermitIf(TSCTrigger.ResumeAuto, TSCState.AUTO, () => ResumeAutoGC());//guardClause為真才會執行狀態變化
                this.Configure(TSCState.AUTO).SubstateOf(TSCState.IN_STATUS)
                    .PermitIf(TSCTrigger.RequestPause, TSCState.PAUSING, () => RequestPauseGC());//guardClause為真才會執行狀態變化

                this.Configure(TSCState.IN_STATUS)
                    .PermitIf(TSCTrigger.OHTCOffLine, TSCState.NONE, () => OHTCOffLineGC());//guardClause為真才會執行狀態變化

            }

            private bool OHTCInitialGC()
            {
                return true;
            }
            private bool OHTCOffLineGC()
            {
                return true;
            }
            private bool StartUpSuccessGC()
            {
                return true;
            }
            private bool ResumeAutoGC()
            {
                return true;
            }
            private bool PauseCompleteGC()
            {
                return true;
            }
            private bool RequestPauseGC()
            {
                return true;
            }
        }

        public enum TSCState //有哪些State
        {
            [System.ComponentModel.DataAnnotations.Display(Name = "None")]
            NONE = 0,
            [System.ComponentModel.DataAnnotations.Display(Name = "Init")]
            TSC_INIT = 1,
            [System.ComponentModel.DataAnnotations.Display(Name = "Paused")]
            PAUSED = 2,
            [System.ComponentModel.DataAnnotations.Display(Name = "Pausing")]
            PAUSING = 3,
            [System.ComponentModel.DataAnnotations.Display(Name = "Auto")]
            AUTO = 4,
            [System.ComponentModel.DataAnnotations.Display(Name = "In Status")]
            IN_STATUS = 99
        }

        public enum TSCTrigger //有哪些Trigger
        {
            OHTCInitial,
            StartUpSuccess,
            ResumeAuto,
            RequestPause,
            PauseComplete,
            OHTCOffLine
        }
        public bool OHTCinitialComplete(ReportBLL reportBLL)
        {
            try
            {
                if (TSC_state_machine.CanFire(TSCTrigger.OHTCInitial))//檢查當前狀態能否進行這個Trigger
                {
                    TSC_state_machine.Fire(TSCTrigger.OHTCInitial);//進行Trigger
                    SCStats = TSCState.TSC_INIT;
                    reportBLL.ReportTSCAutoInitiated();
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
        public bool StartUpSuccessed(ReportBLL reportBLL)
        {
            try
            {
                if (TSC_state_machine.CanFire(TSCTrigger.StartUpSuccess))//檢查當前狀態能否進行這個Trigger
                {
                    TSC_state_machine.Fire(TSCTrigger.StartUpSuccess);//進行Trigger
                    SCStats = TSCState.PAUSED;
                    reportBLL.ReportTSCPaused();
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
        public bool ResumeToAuto(ReportBLL reportBLL)
        {
            try
            {
                if (TSC_state_machine.CanFire(TSCTrigger.ResumeAuto))//檢查當前狀態能否進行這個Trigger
                {
                    //reportBLL.ReportTSCAutoInitiated();
                    TSC_state_machine.Fire(TSCTrigger.ResumeAuto);//進行Trigger
                    SCStats = TSCState.AUTO;
                    reportBLL.ReportTSCAutoCompleted();
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
        public bool RequestToPause(ReportBLL reportBLL)
        {
            try
            {
                if (TSC_state_machine.CanFire(TSCTrigger.RequestPause))//檢查當前狀態能否進行這個Trigger
                {
                    TSC_state_machine.Fire(TSCTrigger.RequestPause);//進行Trigger
                    SCStats = TSCState.PAUSING;
                    reportBLL.ReportTSCPauseInitiated();
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
        public bool PauseCompleted(ReportBLL reportBLL)
        {
            try
            {
                if (TSC_state_machine.CanFire(TSCTrigger.PauseComplete))//檢查當前狀態能否進行這個Trigger
                {
                    TSC_state_machine.Fire(TSCTrigger.PauseComplete);//進行Trigger
                    SCStats = TSCState.PAUSED;
                    reportBLL.ReportTSCPauseCompleted();
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
        public bool ChangeToOffline()
        {
            try
            {
                if (TSC_state_machine.CanFire(TSCTrigger.OHTCOffLine))//檢查當前狀態能否進行這個Trigger
                {
                    TSC_state_machine.Fire(TSCTrigger.OHTCOffLine);//進行Trigger
                    SCStats = TSCState.NONE;
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
        #endregion TSC state machine
        //bool CurrentPortStateChecked = 1;
        //bool CurrentStateChecked = 2;
        //bool EnhancedVehiclesChecked = 3;
        //bool TSCStateChecked = 4;
        //bool UnitAlarmStateListChecked = 5;
        //bool EnhancedTransfersChecked = 6;
        //bool EnhancedCarriersChecked = 7;
        //bool LaneCutListChecked = 8;
        #region MCS Online Check Item
        private bool currentPortStateChecked = false;
        public bool CurrentPortStateChecked
        {
            get
            { return currentPortStateChecked; }
            set
            {
                if (currentPortStateChecked != value)
                {
                    currentPortStateChecked = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.CurrentPortStateChecked));
                }
            }
        }
        private bool currentStateChecked = false;
        public bool CurrentStateChecked
        {
            get
            { return currentStateChecked; }
            set
            {
                if (currentStateChecked != value)
                {
                    currentStateChecked = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.CurrentStateChecked));
                }
            }
        }
        private bool enhancedVehiclesChecked = false;
        public bool EnhancedVehiclesChecked
        {
            get
            { return enhancedVehiclesChecked; }
            set
            {
                if (enhancedVehiclesChecked != value)
                {
                    enhancedVehiclesChecked = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.EnhancedVehiclesChecked));
                }
            }
        }
        private bool tSCStateChecked = false;
        public bool TSCStateChecked
        {
            get
            { return tSCStateChecked; }
            set
            {
                if (tSCStateChecked != value)
                {
                    tSCStateChecked = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.TSCStateChecked));
                }
            }
        }
        //private bool unitAlarmStateListChecked = false;
        //public bool UnitAlarmStateListChecked
        //{
        //    get
        //    { return unitAlarmStateListChecked; }
        //    set
        //    {
        //        if (unitAlarmStateListChecked != value)
        //        {
        //            unitAlarmStateListChecked = value;
        //            OnPropertyChanged(BCFUtility.getPropertyName(() => this.UnitAlarmStateListChecked));
        //        }
        //    }
        //}
        private bool enhancedTransfersChecked = false;
        public bool EnhancedTransfersChecked
        {
            get
            { return enhancedTransfersChecked; }
            set
            {
                if (enhancedTransfersChecked != value)
                {
                    enhancedTransfersChecked = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.EnhancedTransfersChecked));
                }
            }
        }
        private bool enhancedCarriersChecked = false;
        public bool EnhancedCarriersChecked
        {
            get
            { return enhancedCarriersChecked; }
            set
            {
                if (enhancedCarriersChecked != value)
                {
                    enhancedCarriersChecked = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.EnhancedCarriersChecked));
                }
            }
        }
        //private bool laneCutListChecked = false;
        //public bool LaneCutListChecked
        //{
        //    get
        //    { return laneCutListChecked; }
        //    set
        //    {
        //        if (laneCutListChecked != value)
        //        {
        //            laneCutListChecked = value;
        //            OnPropertyChanged(BCFUtility.getPropertyName(() => this.LaneCutListChecked));
        //        }
        //    }
        //}
        public void resetOnlieCheckItem()
        {
            CurrentPortStateChecked = false;
            CurrentStateChecked = false;
            EnhancedVehiclesChecked = false;
            TSCStateChecked = false;
            //UnitAlarmStateListChecked = false;
            EnhancedTransfersChecked = false;
            EnhancedCarriersChecked = false;
            //LaneCutListChecked = false;
        }
        #endregion MCS Online Check Item

        #region Ping Check Item
        private bool mCSConnectionSuccess = false;
        public bool MCSConnectionSuccess
        {
            get
            { return mCSConnectionSuccess; }
            set
            {
                if (mCSConnectionSuccess != value)
                {
                    mCSConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.MCSConnectionSuccess));
                }
            }
        }
        private bool routerConnectionSuccess = false;
        public bool RouterConnectionSuccess
        {
            get
            { return routerConnectionSuccess; }
            set
            {
                if (routerConnectionSuccess != value)
                {
                    routerConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.RouterConnectionSuccess));
                }
            }
        }
        private bool aGV1ConnectionSuccess = false;
        public bool AGV1ConnectionSuccess
        {
            get
            { return aGV1ConnectionSuccess; }
            set
            {
                if (aGV1ConnectionSuccess != value)
                {
                    aGV1ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.AGV1ConnectionSuccess));
                }
            }
        }
        private bool aGV2ConnectionSuccess = false;
        public bool AGV2ConnectionSuccess
        {
            get
            { return aGV2ConnectionSuccess; }
            set
            {
                if (aGV2ConnectionSuccess != value)
                {
                    aGV2ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.AGV2ConnectionSuccess));
                }
            }
        }
        private bool aGV3ConnectionSuccess = false;
        public bool AGV3ConnectionSuccess
        {
            get
            { return aGV3ConnectionSuccess; }
            set
            {
                if (aGV3ConnectionSuccess != value)
                {
                    aGV3ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.AGV3ConnectionSuccess));
                }
            }
        }
        private bool aGV4ConnectionSuccess = false;
        public bool AGV4ConnectionSuccess
        {
            get
            { return aGV4ConnectionSuccess; }
            set
            {
                if (aGV4ConnectionSuccess != value)
                {
                    aGV4ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.AGV4ConnectionSuccess));
                }
            }
        }
        private bool aGV5ConnectionSuccess = false;
        public bool AGV5ConnectionSuccess
        {
            get
            { return aGV5ConnectionSuccess; }
            set
            {
                if (aGV5ConnectionSuccess != value)
                {
                    aGV5ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.AGV5ConnectionSuccess));
                }
            }
        }
        private bool aGV6ConnectionSuccess = false;
        public bool AGV6ConnectionSuccess
        {
            get
            { return aGV6ConnectionSuccess; }
            set
            {
                if (aGV6ConnectionSuccess != value)
                {
                    aGV6ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.AGV6ConnectionSuccess));
                }
            }
        }
        private bool aGV7ConnectionSuccess = false;
        public bool AGV7ConnectionSuccess
        {
            get
            { return aGV7ConnectionSuccess; }
            set
            {
                if (aGV7ConnectionSuccess != value)
                {
                    aGV7ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.AGV7ConnectionSuccess));
                }
            }
        }
        private bool aGV8ConnectionSuccess = false;
        public bool AGV8ConnectionSuccess
        {
            get
            { return aGV8ConnectionSuccess; }
            set
            {
                if (aGV8ConnectionSuccess != value)
                {
                    aGV8ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.AGV8ConnectionSuccess));
                }
            }
        }
        private bool aGV9ConnectionSuccess = false;
        public bool AGV9ConnectionSuccess
        {
            get
            { return aGV9ConnectionSuccess; }
            set
            {
                if (aGV9ConnectionSuccess != value)
                {
                    aGV9ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.AGV9ConnectionSuccess));
                }
            }
        }
        private bool aGV10ConnectionSuccess = false;
        public bool AGV10ConnectionSuccess
        {
            get
            { return aGV10ConnectionSuccess; }
            set
            {
                if (aGV10ConnectionSuccess != value)
                {
                    aGV10ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.AGV10ConnectionSuccess));
                }
            }
        }
        private bool aGV11ConnectionSuccess = false;
        public bool AGV11ConnectionSuccess
        {
            get
            { return aGV11ConnectionSuccess; }
            set
            {
                if (aGV11ConnectionSuccess != value)
                {
                    aGV11ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.AGV11ConnectionSuccess));
                }
            }
        }
        private bool aGV12ConnectionSuccess = false;
        public bool AGV12ConnectionSuccess
        {
            get
            { return aGV12ConnectionSuccess; }
            set
            {
                if (aGV12ConnectionSuccess != value)
                {
                    aGV12ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.AGV12ConnectionSuccess));
                }
            }
        }
        private bool aGV13ConnectionSuccess = false;
        public bool AGV13ConnectionSuccess
        {
            get
            { return aGV13ConnectionSuccess; }
            set
            {
                if (aGV13ConnectionSuccess != value)
                {
                    aGV13ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.AGV13ConnectionSuccess));
                }
            }
        }
        private bool aGV14ConnectionSuccess = false;
        public bool AGV14ConnectionSuccess
        {
            get
            { return aGV14ConnectionSuccess; }
            set
            {
                if (aGV14ConnectionSuccess != value)
                {
                    aGV14ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.AGV14ConnectionSuccess));
                }
            }
        }
        private bool chargePLCConnectionSuccess = false;
        public bool ChargePLCConnectionSuccess
        {
            get
            { return chargePLCConnectionSuccess; }
            set
            {
                if (chargePLCConnectionSuccess != value)
                {
                    chargePLCConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.ChargePLCConnectionSuccess));
                }
            }
        }
        private bool aDAM1ConnectionSuccess = false;
        public bool ADAM1ConnectionSuccess
        {
            get
            { return aDAM1ConnectionSuccess; }
            set
            {
                if (aDAM1ConnectionSuccess != value)
                {
                    aDAM1ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.ADAM1ConnectionSuccess));
                }
            }
        }
        private bool aDAM2ConnectionSuccess = false;
        public bool ADAM2ConnectionSuccess
        {
            get
            { return aDAM2ConnectionSuccess; }
            set
            {
                if (aDAM2ConnectionSuccess != value)
                {
                    aDAM2ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.ADAM2ConnectionSuccess));
                }
            }
        }
        private bool aDAM3ConnectionSuccess = false;
        public bool ADAM3ConnectionSuccess
        {
            get
            { return aDAM3ConnectionSuccess; }
            set
            {
                if (aDAM3ConnectionSuccess != value)
                {
                    aDAM3ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.ADAM3ConnectionSuccess));
                }
            }
        }
        private bool aDAM4ConnectionSuccess = false;
        public bool ADAM4ConnectionSuccess
        {
            get
            { return aDAM4ConnectionSuccess; }
            set
            {
                if (aDAM4ConnectionSuccess != value)
                {
                    aDAM4ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.ADAM4ConnectionSuccess));
                }
            }
        }
        private bool aDAM5ConnectionSuccess = false;
        public bool ADAM5ConnectionSuccess
        {
            get
            { return aDAM5ConnectionSuccess; }
            set
            {
                if (aDAM5ConnectionSuccess != value)
                {
                    aDAM5ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.ADAM5ConnectionSuccess));
                }
            }
        }

        private bool ap1ConnectionSuccess = false;
        public bool AP1ConnectionSuccess
        {
            get
            { return ap1ConnectionSuccess; }
            set
            {
                if (ap1ConnectionSuccess != value)
                {
                    ap1ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.AP1ConnectionSuccess));
                }
            }
        }

        private bool ap2ConnectionSuccess = false;
        public bool AP2ConnectionSuccess
        {
            get
            { return ap2ConnectionSuccess; }
            set
            {
                if (ap2ConnectionSuccess != value)
                {
                    ap2ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.AP2ConnectionSuccess));
                }
            }
        }

        private bool ap3ConnectionSuccess = false;
        public bool AP3ConnectionSuccess
        {
            get
            { return ap3ConnectionSuccess; }
            set
            {
                if (ap3ConnectionSuccess != value)
                {
                    ap3ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.AP3ConnectionSuccess));
                }
            }
        }


        private bool ap4ConnectionSuccess = false;
        public bool AP4ConnectionSuccess
        {
            get
            { return ap4ConnectionSuccess; }
            set
            {
                if (ap4ConnectionSuccess != value)
                {
                    ap4ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.AP4ConnectionSuccess));
                }
            }
        }


        private bool ap5ConnectionSuccess = false;
        public bool AP5ConnectionSuccess
        {
            get
            { return ap5ConnectionSuccess; }
            set
            {
                if (ap5ConnectionSuccess != value)
                {
                    ap5ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.AP5ConnectionSuccess));
                }
            }
        }


        private bool ap6ConnectionSuccess = false;
        public bool AP6ConnectionSuccess
        {
            get
            { return ap6ConnectionSuccess; }
            set
            {
                if (ap6ConnectionSuccess != value)
                {
                    ap6ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.AP6ConnectionSuccess));
                }
            }
        }


        private bool ap7ConnectionSuccess = false;
        public bool AP7ConnectionSuccess
        {
            get
            { return ap7ConnectionSuccess; }
            set
            {
                if (ap7ConnectionSuccess != value)
                {
                    ap7ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.AP7ConnectionSuccess));
                }
            }
        }


        private bool ap8ConnectionSuccess = false;
        public bool AP8ConnectionSuccess
        {
            get
            { return ap8ConnectionSuccess; }
            set
            {
                if (ap8ConnectionSuccess != value)
                {
                    ap8ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.AP8ConnectionSuccess));
                }
            }
        }


        private bool ap9ConnectionSuccess = false;
        public bool AP9ConnectionSuccess
        {
            get
            { return ap9ConnectionSuccess; }
            set
            {
                if (ap9ConnectionSuccess != value)
                {
                    ap9ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.AP9ConnectionSuccess));
                }
            }
        }


        private bool ap10ConnectionSuccess = false;
        public bool AP10ConnectionSuccess
        {
            get
            { return ap10ConnectionSuccess; }
            set
            {
                if (ap10ConnectionSuccess != value)
                {
                    ap10ConnectionSuccess = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.AP10ConnectionSuccess));
                }
            }
        }

        public void setConnectionInfo(Dictionary<string, CommuncationInfo> dicCommInfo)
        {
            foreach (KeyValuePair<string, CommuncationInfo> keyPair in dicCommInfo)
            {
                CommuncationInfo Info = keyPair.Value;

                switch (keyPair.Key)
                {
                    case "MCS":
                        MCSConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "ROUTER":
                        RouterConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "AGV_1":
                        AGV1ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "AGV_2":
                        AGV2ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "AGV_3":
                        AGV3ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "AGV_4":
                        AGV4ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "AGV_5":
                        AGV5ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "AGV_6":
                        AGV6ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "AGV_7":
                        AGV7ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "AGV_8":
                        AGV8ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "AGV_9":
                        AGV9ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "AGV_10":
                        AGV10ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "AGV_11":
                        AGV11ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "AGV_12":
                        AGV12ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "AGV_13":
                        AGV13ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "AGV_14":
                        AGV14ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                        break;
                    case "CHARGER_PLC":
                        ChargePLCConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "ADAM_1":
                        ADAM1ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "ADAM_2":
                        ADAM2ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "ADAM_3":
                        ADAM3ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "ADAM_4":
                        ADAM4ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "ADAM_5":
                        ADAM5ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "AP1":
                        AP1ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "AP2":
                        AP2ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "AP3":
                        AP3ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "AP4":
                        AP4ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "AP5":
                        AP5ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "AP6":
                        AP6ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "AP7":
                        AP7ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "AP8":
                        AP8ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "AP9":
                        AP9ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                    case "AP10":
                        AP10ConnectionSuccess = Info.IsConnectinoSuccess;
                        break;
                }
            }
        }
        #endregion Ping Check Item


        #region Transfer
        private bool mCSCommandAutoAssign = true;
        public bool MCSCommandAutoAssign
        {
            get
            { return mCSCommandAutoAssign; }
            set
            {
                if (mCSCommandAutoAssign != value)
                {
                    mCSCommandAutoAssign = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.MCSCommandAutoAssign));
                }
            }
        }
        #endregion Transfer
        #region DisplayModeParameter
        public bool isDisplayMode;
        public bool isCMDIndiSetChanged;
        public bool isDisplayLastCMD;
        public CMDIndiSettings DisplayLoopSetting;
        public int CMDIndiPriortySetting;
        public int CMDLoopIntervalSetting;
        public enum CMDIndiSettings
        {
            All,
            MCS,
            OHxC,
            Priority
        }
        #endregion
        public class LineTimerAction : ITimerAction
        {
            private static Logger logger = LogManager.GetCurrentClassLogger();
            ALINE line = null;
            SCApplication scApp = null;
            public LineTimerAction(ALINE _line, string name, long intervalMilliSec)
                : base(name, intervalMilliSec)
            {
                line = _line;
            }

            public override void initStart()
            {
                scApp = SCApplication.getInstance();
            }

            private long syncPoint = 0;
            public override void doProcess(object obj)
            {
                //20210127暫時取消S1F1的發送 markchou
                //if (System.Threading.Interlocked.Exchange(ref syncPoint, 1) == 0)
                //{
                //    try
                //    {
                //        if (line.Secs_Link_Stat == SCAppConstants.LinkStatus.LinkFail) return;
                //        //1.檢查是否已經大於一定時間沒有進行通訊
                //        double from_last_comm_time = line.CommunicationIntervalWithMCS.Elapsed.TotalSeconds;
                //        if (from_last_comm_time > ALINE.MAX_ALLOW_NO_COMMUNICATION_TIME_FOR_MCS_SECOND)
                //        {
                //            line.onLongTimeNoCommuncation();
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(AVEHICLE), Device: "AGVC",
                //           Data: ex,
                //           VehicleID: line.LINE_ID);
                //    }
                //    finally
                //    {
                //        System.Threading.Interlocked.Exchange(ref syncPoint, 0);
                //    }

                //}
            }

        }

    }
}
