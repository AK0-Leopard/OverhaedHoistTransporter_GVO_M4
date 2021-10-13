using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Controller;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.SECS;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using com.mirle.ibg3k0.stc.Common;
using com.mirle.ibg3k0.stc.Data.SecsData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace com.mirle.ibg3k0.sc.Data.SECSDriver
{
    public abstract class IBSEMDriver : SEMDriver
    {
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        #region Receive
        protected abstract void S2F49ReceiveEnhancedRemoteCommandExtension(object sender, SECSEventArgs e);
        protected abstract void S2F41ReceiveHostCommand(object sender, SECSEventArgs e);
        #endregion Receive

        #region Send
        #region Transfer Event

        public abstract bool S6F11SendTransferAbortCompleted(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendTransferAbortFailed(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendTransferAbortInitial(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendTransferCancelCompleted(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendTransferCancelFailed(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendTransferCancelInitial(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendTransferPaused(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendTransferResumed(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);

        public abstract bool S6F11SendTransferInitial(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendTransferring(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendVehicleArrived(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendVehicleAcquireStarted(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendVehicleAcquireCompleted(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendVehicleAssigned(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendVehicleDeparted(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendVehicleDepositStarted(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendVehicleDepositCompleted(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendCarrierIDRead(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendCarrierInstalled(string vhID, string carrierID, string location, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendCarrierInstalled(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendCarrierRemoved(string vhID, string carrierID, string location, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendCarrierRemoved(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendVehicleUnassinged(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendTransferCompleted(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendTransferCompleted(VTRANSFER vtransfer, CompleteStatus completeStatus, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendTransferUpdateCompleted(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendTransferUpdateFailed(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendRunTimeStatus(string vhID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendVehicleInstalled(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendVehicleRemoved(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        #endregion Transfer Event
        #region Port Event
        public abstract bool S6F11PortEventStateChanged(string cmdID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendPortInService(string portID,  List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendPortOutService(string portID,  List<AMCSREPORTQUEUE> reportQueues = null);
        #endregion Port Event

        public abstract bool S6F11SendAlarmCleared(string vhID, string transferID, string alarmID, string alarmTest, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendAlarmSet(string vhID, string transferID, string alarmID, string alarmTest, List<AMCSREPORTQUEUE> reportQueues = null);

        public abstract bool S6F11SendCarrierWaitIn(ACARRIER cst, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendCarrierWaitOut(ACARRIER cst, string portType, List<AMCSREPORTQUEUE> reportQueues = null);

        public abstract bool S6F11SendCarrierIDRead(ACARRIER cst, string IDreadStatus, List<AMCSREPORTQUEUE> reportQueues = null);

        public abstract bool S6F11SendCarrierRemovedCompleted(string cst_id, List<AMCSREPORTQUEUE> reportQueues = null);

        public abstract bool S6F11SendCarrierRemovedCompleted(string cst_id,string location, List<AMCSREPORTQUEUE> reportQueues = null);

        public abstract bool S6F11SendCarrierRemovedFromPort(ACARRIER cst, string Handoff_Type, List<AMCSREPORTQUEUE> reportQueues = null);

        public abstract bool S6F11SendLoadReq(string port_id, List<AMCSREPORTQUEUE> reportQueues = null);

        public abstract bool S6F11SendUnLoadReq(string port_id, List<AMCSREPORTQUEUE> reportQueues = null);

        public abstract bool S6F11SendPortInMode(string portID, List<AMCSREPORTQUEUE> reportQueues = null);

        public abstract bool S6F11SendPortOutMode(string portID, List<AMCSREPORTQUEUE> reportQueues = null);

        public abstract bool S6F11SendPortModeChange(string portID, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendNoReq(string port_id, List<AMCSREPORTQUEUE> reportQueues = null);

        public abstract bool S6F11SendCarrierInstallCompleted(ACARRIER cst, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendUnitAlarmSet(string unitID, string alarmID, string alarmTest, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendUnitAlarmCleared(string unitID, string alarmID, string alarmTest, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendOperatorInitialAction(string command_id, string command_type,string carrier_id, string source,string destination,string priority, List<AMCSREPORTQUEUE> reportQueues = null);
        #region TSC State Transition Event
        public abstract bool S6F11SendTSCAutoCompleted();
        public abstract bool S6F11SendTSCAutoInitiated();
        public abstract bool S6F11SendTSCPauseCompleted();
        public abstract bool S6F11SendTSCPaused();
        public abstract bool S6F11SendTSCPauseInitiated();
        #endregion TSC State Transition Event

        #region Operator Event
        public abstract bool S6F11SendOperatorInitiatedAction(string vhID, List<AMCSREPORTQUEUE> reportQueues = null);
        #endregion Operator Event

        #region Charge Event
        public abstract bool S6F11SendVehicleChargeRequest(string vhId, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendVehicleChargeStarted(string vhId, List<AMCSREPORTQUEUE> reportQueues = null);
        public abstract bool S6F11SendVehicleChargeCompleted(string vhId, List<AMCSREPORTQUEUE> reportQueues = null);
        #endregion Charge Event


        #endregion Send

    }
}