// ***********************************************************************
// Assembly         : ScriptControl
// Author           : 
// Created          : 03-31-2016
//
// Last Modified By : 
// Last Modified On : 03-24-2016
// ***********************************************************************
// <copyright file="ReportBLL.cs" company="">
//     Copyright ©  2014
// </copyright>
// <summary></summary>
// ***********************************************************************
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.Data.DAO;
using com.mirle.ibg3k0.sc.Data.SECSDriver;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace com.mirle.ibg3k0.sc.BLL
{
    /// <summary>
    /// Class ReportBLL.
    /// </summary>
    public class ReportBLL
    {
        /// <summary>
        /// The sc application
        /// </summary>
        private SCApplication scApp = null;
        /// <summary>
        /// The logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The trace set DAO
        /// </summary>
        private TraceSetDao traceSetDao = null;
        private MCSReportQueueDao mcsReportQueueDao = null;
        private DataCollectionDao dataCollectionDao = null;
        protected IBSEMDriver iBSEMDriver = null;
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportBLL"/> class.
        /// </summary>
        public ReportBLL()
        {

        }

        /// <summary>
        /// Starts the specified sc application.
        /// </summary>
        /// <param name="scApp">The sc application.</param>
        public void start(SCApplication scApp)
        {
            this.scApp = scApp;
            traceSetDao = scApp.TraceSetDao;
            mcsReportQueueDao = scApp.MCSReportQueueDao;
            dataCollectionDao = scApp.DataCollectionDao;
        }
        public void startMapAction(IBSEMDriver iBSEMDriver)
        {
            //var auo_mcsDefaultMapAction = scApp.getEQObjCacheManager().
            //      getLine().getMapActionByIdentityKey(typeof(AUOMCSDefaultMapAction).Name) as AUOMCSDefaultMapAction;
            //iBSEMDriver = auo_mcsDefaultMapAction;
            this.iBSEMDriver = iBSEMDriver;
        }

        /// <summary>
        /// Updates the trace set.
        /// </summary>
        /// <param name="trace_id">The trace_id.</param>
        /// <param name="smp_period">The smp_period.</param>
        /// <param name="total_smp_cnt">The total_smp_cnt.</param>
        /// <param name="svidList">The svid list.</param>
        public void updateTraceSet(string trace_id, string smp_period, int total_smp_cnt, List<string> svidList)
        {
            ATRACESET traceSet = new ATRACESET()
            {
                TRACE_ID = trace_id,
                SMP_PERIOD = smp_period,
                TOTAL_SMP_CNT = total_smp_cnt,
                TraceItemList = new List<ATRACEITEM>()
            };
            traceSet.calcNextSmpTime();
            List<ATRACEITEM> traceItems = new List<ATRACEITEM>();
            foreach (string svid in svidList)
            {
                ATRACEITEM tItem = new ATRACEITEM();
                tItem.TRACE_ID = trace_id;
                tItem.SVID = svid;
                traceItems.Add(tItem);
            }
            updateTraceSet(traceSet, traceItems);
        }

        /// <summary>
        /// Updates the trace set.
        /// </summary>
        /// <param name="traceSet">The trace set.</param>
        /// <param name="traceItems">The trace items.</param>
        public void updateTraceSet(ATRACESET traceSet, List<ATRACEITEM> traceItems)
        {
            DBConnection_EF conn = null;
            try
            {
                conn = DBConnection_EF.GetContext();
                conn.BeginTransaction();
                ATRACESET sv_traceSet = null;
                sv_traceSet = traceSetDao.getTraceSet(conn, true, traceSet.TRACE_ID);
                if (sv_traceSet != null)
                {
                    sv_traceSet.SMP_PERIOD = traceSet.SMP_PERIOD;
                    sv_traceSet.TOTAL_SMP_CNT = traceSet.TOTAL_SMP_CNT;
                    sv_traceSet.SMP_CNT = 0;            //重新開始
                    sv_traceSet.calcNextSmpTime();
                    traceSetDao.updateTraceSet(conn, sv_traceSet);
                }
                else
                {
                    sv_traceSet = traceSet;
                    sv_traceSet.NX_SMP_TIME = DateTime.Now;
                    sv_traceSet.SMP_TIME = DateTime.Now;
                    traceSetDao.insertTraceSet(conn, traceSet);
                }

                deleteTraceItem(traceSet.TRACE_ID);
                foreach (ATRACEITEM item in traceItems)
                {
                    traceSetDao.insertTraceItem(conn, item);
                }
                conn.Commit();
            }
            catch (Exception ex)
            {
                if (conn != null) { try { conn.Rollback(); } catch (Exception ex_rollback) { logger.Error(ex_rollback, "Exception"); } }
                logger.Error(ex, "Exception:");
            }
            finally
            {
                if (conn != null) { try { conn.Close(); } catch (Exception ex_close) { logger.Error(ex_close, "Exception:"); } }
            }
        }

        /// <summary>
        /// Deletes the trace item.
        /// </summary>
        /// <param name="trace_id">The trace_id.</param>
        public void deleteTraceItem(string trace_id)
        {
            DBConnection_EF conn = null;
            try
            {
                conn = DBConnection_EF.GetContext();
                conn.BeginTransaction();
                traceSetDao.deleteTraceItem(conn, trace_id);
                conn.Commit();
            }
            catch (Exception ex)
            {
                if (conn != null) { try { conn.Rollback(); } catch (Exception ex_rollback) { logger.Error(ex_rollback, "Exception"); } }
                logger.Error(ex, "Exception:");
            }
            finally
            {
                if (conn != null) { try { conn.Close(); } catch (Exception ex_close) { logger.Error(ex_close, "Exception:"); } }
            }
        }

        /// <summary>
        /// Updates the trace set.
        /// </summary>
        /// <param name="traceSet">The trace set.</param>
        public void updateTraceSet(ATRACESET traceSet)
        {

            DBConnection_EF conn = null;
            try
            {
                conn = DBConnection_EF.GetContext();
                conn.BeginTransaction();
                traceSetDao.updateTraceSet(conn, traceSet);
                conn.Commit();
            }
            catch (Exception ex)
            {
                if (conn != null) { try { conn.Rollback(); } catch (Exception ex_rollback) { logger.Error(ex_rollback, "Exception"); } }
                logger.Error(ex, "Exception:");
            }
            finally
            {
                if (conn != null) { try { conn.Close(); } catch (Exception ex_close) { logger.Error(ex_close, "Exception:"); } }
            }
        }

        /// <summary>
        /// Loads the active trace set data.
        /// </summary>
        /// <returns>List&lt;TraceSet&gt;.</returns>
        public List<ATRACESET> loadActiveTraceSetData()
        {
            List<ATRACESET> traceSetList = new List<ATRACESET>();
            DBConnection_EF conn = null;
            try
            {
                conn = DBConnection_EF.GetContext();

                traceSetList = traceSetDao.loadActiveTraceSet(conn);
                foreach (ATRACESET set in traceSetList)
                {
                    string trace_id = set.TRACE_ID;
                    List<ATRACEITEM> itemList = traceSetDao.loadTraceItem(conn, trace_id);
                    set.TraceItemList = itemList;
                }
            }
            catch (Exception ex)
            {
                if (conn != null) { try { conn.Rollback(); } catch (Exception ex_rollback) { logger.Error(ex_rollback, "Exception"); } }
                logger.Error(ex, "Exception:");
            }
            finally
            {
                if (conn != null) { try { conn.Close(); } catch (Exception ex_close) { logger.Error(ex_close, "Exception:"); } }
            }
            return traceSetList;
        }

        /// <summary>
        /// Loads the report trace set data.
        /// </summary>
        /// <returns>List&lt;TraceSet&gt;.</returns>
        public List<ATRACESET> loadReportTraceSetData()
        {
            List<ATRACESET> traceSetList = new List<ATRACESET>();
            DBConnection_EF conn = null;
            try
            {
                conn = DBConnection_EF.GetContext();

                List<ATRACESET> tmpTraceSetList = traceSetDao.loadActiveTraceSet(conn);
                DateTime reportNowDate = DateTime.Now;
                //
                foreach (ATRACESET traceSet in tmpTraceSetList)
                {
                    if (traceSet.NX_SMP_TIME.CompareTo(reportNowDate) <= 0 && traceSet.SMP_TIME.CompareTo(reportNowDate) < 0)
                    {
                        traceSetList.Add(traceSet);
                    }
                }
                //
                foreach (ATRACESET set in traceSetList)
                {
                    string trace_id = set.TRACE_ID;
                    List<ATRACEITEM> itemList = traceSetDao.loadTraceItem(conn, trace_id);
                    set.TraceItemList = itemList;
                }
            }
            catch (Exception ex)
            {
                if (conn != null) { try { conn.Rollback(); } catch (Exception ex_rollback) { logger.Error(ex_rollback, "Exception"); } }
                logger.Error(ex, "Exception:");
            }
            finally
            {
                if (conn != null) { try { conn.Close(); } catch (Exception ex_close) { logger.Error(ex_close, "Exception:"); } }
            }
            return traceSetList;
        }

        #region MCS SXFY Report
        public bool AskAreYouThere()
        {
            return iBSEMDriver.S1F1SendAreYouThere();
        }
        public bool AskDateAndTimeRequest()
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S2F17SendDateAndTimeRequest();
            return isSuccsess;
        }

        public bool ReportEquiptmentOffLine()
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendEquiptmentOffLine();
            return isSuccsess;
        }

        public bool ReportControlStateRemote()
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendControlStateRemote();
            return isSuccsess;
        }

        public bool ReportControlStateLocal()
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendControlStateLocal();
            return isSuccsess;
        }

        public bool ReportTSCAutoInitiated()
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendTSCAutoInitiated();
            return isSuccsess;
        }
        public bool ReportTSCPaused()
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendTSCPaused();
            return isSuccsess;
        }
        public bool ReportTSCAutoCompleted()
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendTSCAutoCompleted();
            return isSuccsess;
        }
        public bool ReportTSCPauseInitiated()
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendTSCPauseInitiated();
            return isSuccsess;
        }
        public bool ReportTSCPauseCompleted()
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendTSCPauseCompleted();
            return isSuccsess;
        }
        public bool ReportAlarmSet()
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendAlarmSet();
            return isSuccsess;
        }
        public bool ReportAlarmCleared()
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendAlarmCleared();
            return isSuccsess;
        }

        public bool newReportTransferInitial(string cmdID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendTransferInitial(cmdID, reportqueues);
            return isSuccsess;
        }

        public bool newReportTransferUpdateCompleted(string cmdID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendTransferUpdateCompleted(cmdID, reportqueues);
            return isSuccsess;
        }
        public bool newReportTransferUpdateFailed(string cmdID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendTransferUpdateFailed(cmdID, reportqueues);
            return isSuccsess;
        }

        public bool newReportBeginTransfer(string cmdID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendVehicleAssigned(cmdID, reportqueues);
            //isSuccsess = isSuccsess && iBSEMDriver.S6F11SendTransferInitial(cmdID, reportqueues);
            return isSuccsess;
        }
        public bool ReportCarrierWaitIn(ACARRIER cst, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendCarrierWaitIn(cst, reportQueues);
            return isSuccsess;
        }


        public bool ReportCarrierWaitOut(ACARRIER cst, string portType, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendCarrierWaitOut(cst, portType, reportQueues);
            return isSuccsess;
        }

        public bool ReportCarrierIDRead(ACARRIER cst, string IDreadStatus, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendCarrierIDRead(cst, IDreadStatus, reportQueues);
            return isSuccsess;
        }
        public bool ReportCarrierRemovedCompleted(string cstid, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendCarrierRemovedCompleted(cstid, reportQueues);
            return isSuccsess;
        }

        public bool ReportCarrierRemovedCompleted(string cstid,string location, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendCarrierRemovedCompleted(cstid,location, reportQueues);
            return isSuccsess;
        }

        public bool ReportCarrierRemovedFromPort(ACARRIER cst, string Handoff_Type, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendCarrierRemovedFromPort(cst, Handoff_Type);
            return isSuccsess;
        }

        public bool ReportCarrierInstallCompleted(ACARRIER cst, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendCarrierInstallCompleted(cst, reportQueues);
            return isSuccsess;
        }

        public bool ReportUnitAlarmSet(string unitID, string alarmID, string alarmTest, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendUnitAlarmSet(unitID, alarmID, alarmTest, reportQueues);
            return isSuccsess;
        }

        public bool ReportUnitAlarmClear(string unitID, string alarmID, string alarmTest, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendUnitAlarmCleared(unitID, alarmID, alarmTest, reportQueues);
            return isSuccsess;
        }

        public bool ReportdOperatorInitialAction(string command_id, string command_type, string carrier_id, string source, string destination, string priority, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendOperatorInitialAction(command_id, command_type, carrier_id, source, destination, priority, reportQueues);
            return isSuccsess;
        }
        

        public bool ReportLoadReq(string port_id, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendLoadReq(port_id, reportQueues);
            return isSuccsess;
        }

        public bool ReportUnLoadReq(string port_id, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendUnLoadReq(port_id, reportQueues);
            return isSuccsess;
        }

        public bool ReportNoReq(string port_id, List<AMCSREPORTQUEUE> reportQueues = null)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendNoReq(port_id, reportQueues);
            return isSuccsess;
        }
        public bool newReportLoadArrivals(string cmdID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendVehicleArrived(cmdID, reportqueues);
            return isSuccsess;
        }
        public bool newReportLoadComplete(string cmdID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendVehicleAcquireCompleted(cmdID, reportqueues);
            //if (bcrReadResult == BCRReadResult.BcrNormal) //todo kevin 要將 departed 移至144的離開事件中
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendCarrierInstalled(cmdID, reportqueues);
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendVehicleDeparted(cmdID, reportqueues); 
            return isSuccsess;
        }
        public bool newReportLoadComplete(string vhID, string carrierID, string location, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendCarrierInstalled(vhID, carrierID, location, reportqueues);
            return isSuccsess;
        }
        public bool newReportCarrierIDReadReport(string cmdID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            //isSuccsess = isSuccsess && iBSEMDriver.S6F11SendCarrierInstalled(cmdID, reportqueues);
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendCarrierIDRead(cmdID, reportqueues); // PTI 需要在此上報
            return isSuccsess;
        }
        public bool newReportUnloadArrivals(string cmdID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendVehicleArrived(cmdID, reportqueues);
            return isSuccsess;
        }
        public bool newReportUnloadComplete(string cmdID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            //isSuccsess = isSuccsess && iBSEMDriver.S6F11SendCarrierRemoved(cmdID, reportqueues);
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendVehicleDepositCompleted(cmdID, reportqueues);
            return isSuccsess;
        }
        public bool newReportUnloadComplete(string vhRealID, string carrierID, string location, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendCarrierRemoved(vhRealID, carrierID, location, reportqueues);
            return isSuccsess;
        }
        public bool newReportLoading(string cmdID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendTransferring(cmdID, reportqueues);
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendVehicleAcquireStarted(cmdID, reportqueues);
            return isSuccsess;
        }
        public bool newReportUnloading(string cmdID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendVehicleDepositStarted(cmdID, reportqueues);
            return isSuccsess;
        }
        public bool newReportRunTimetatus(string vhID)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendRunTimeStatus(vhID, null);
            return isSuccsess;
        }

        public bool ReportTransferResult2MCS(string cmdID, CompleteStatus completeStatus)
        {
            bool isSuccess = false;
            List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                switch (completeStatus)
                {
                    case CompleteStatus.CmpStatusCancel:
                        isSuccess = newReportTransferCancelCompleted(cmdID, true, reportqueues);
                        break;
                    case CompleteStatus.CmpStatusAbort:
                        isSuccess = newReportTransferCommandAbortFinish(cmdID, reportqueues);
                        break;
                    case CompleteStatus.CmpStatusLoad:
                    case CompleteStatus.CmpStatusUnload:
                    case CompleteStatus.CmpStatusLoadunload:
                    case CompleteStatus.CmpStatusVehicleAbort:
                    case CompleteStatus.CmpStatusInterlockError:
                    case CompleteStatus.ForceAbnormalFinishByOp:
                    case CompleteStatus.ForceNormalFinishByOp:
                    case CompleteStatus.CmpStatusIdmisMatch:
                    case CompleteStatus.CmpStatusIdreadFailed:
                        isSuccess = newReportTransferCommandFinish(cmdID, reportqueues);
                        break;
                    case CompleteStatus.CmpStatusMove:
                    //case CompleteStatus.MoveToCharger:
                        //Nothing...
                        break;
                    default:
                        logger.Info($"Proc func:CommandCompleteReport, but completeStatus:{completeStatus} notimplemented ");
                        break;
                }
                scApp.ReportBLL.insertMCSReport(reportqueues);
            }
            scApp.ReportBLL.newSendMCSMessage(reportqueues);
            return isSuccess;
        }
        public bool newReportTransferCommandForceFinish(VTRANSFER vTran, CompleteStatus completeStatus, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendTransferCompleted(vTran, completeStatus, reportqueues);
            return isSuccsess;
        }
        public bool newReportTransferCommandFinish(string cmdID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendVehicleUnassinged(cmdID, reportqueues);
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendTransferCompleted(cmdID, reportqueues);
            return isSuccsess;
        }

        public bool newReportTransferCancelCompleted(string cmdID, bool isAssnged, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            if (isAssnged)
                isSuccsess = isSuccsess && iBSEMDriver.S6F11SendVehicleUnassinged(cmdID, reportqueues);
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendTransferCancelCompleted(cmdID, reportqueues);
            return isSuccsess;
        }

        public bool newReportTransferCancelInitial(string cmdID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendTransferCancelInitial(cmdID, reportqueues);
            return isSuccsess;
        }
        public bool newReportTransferCancelFailed(string cmdID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendTransferCancelFailed(cmdID, reportqueues);
            return isSuccsess;
        }
        public bool newReportTransferAbortInitial(string cmdID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendTransferAbortInitial(cmdID, reportqueues);
            return isSuccsess;
        }
        public bool newReportTransferAbortFailed(string cmdID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendTransferAbortFailed(cmdID, reportqueues);
            return isSuccsess;
        }
        public bool newReportTransferCommandAbortFinish(string cmdID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendVehicleUnassinged(cmdID, reportqueues);
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendTransferAbortCompleted(cmdID, reportqueues);
            return isSuccsess;
        }

        public bool newReportVehicleInstalled(string vhID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendVehicleInstalled(vhID, reportqueues);
            return isSuccsess;
        }
        public bool newReportVehicleRemoved(string vhID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendVehicleRemoved(vhID, reportqueues);
            return isSuccsess;
        }
        public bool newReportCarrierInstalled(string vhID, string carrierID, string location, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendCarrierInstalled(vhID, carrierID, location, reportqueues);
            return isSuccsess;
        }
        public bool newReportCarrierRemoved(string vhID, string carrierID, string location, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendCarrierRemoved(vhID, carrierID, location, reportqueues);
            return isSuccsess;
        }


        public bool newReportPortInServeice(string portID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendPortInService(portID, reportqueues);
            return isSuccsess;
        }

        public bool newReportPortOutServeice(string portID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendPortOutService(portID, reportqueues);
            return isSuccsess;
        }


        public bool newReportPortInMode(string portID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendPortInMode(portID, reportqueues);
            return isSuccsess;
        }
        public bool newReportPortOutMode(string portID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendPortOutMode(portID, reportqueues);
            return isSuccsess;
        }
        public bool newReportPortModeChange(string portID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendPortModeChange(portID, reportqueues);
            return isSuccsess;
        }
        public void newSendMCSMessage(List<AMCSREPORTQUEUE> reportqueues)
        {
            foreach (AMCSREPORTQUEUE queue in reportqueues)
                iBSEMDriver.S6F11SendMessage(queue);
        }

        public bool ReportAlarmHappend(string vhID, string transferID, ErrorStatus alarm_status, string error_code, string desc, List<AMCSREPORTQUEUE> reportqueues)
        {
            string alcd = SCAppConstants.AlarmStatus.convert2MCS(alarm_status);
            string alid = error_code;
            //string altx = $"[{SCUtility.Trim(vhID, true) }]{desc}";
            string altx = desc;
            iBSEMDriver.S6F11SendAlarmSet(vhID, transferID, alid, altx);
            iBSEMDriver.S5F1SendAlarmReport(alcd, alid, altx);
            return true;
        }
        public bool ReportAlarmCleared(string vhID, string transferID, ErrorStatus alarm_status, string error_code, string desc, List<AMCSREPORTQUEUE> reportqueues)
        {
            string alcd = SCAppConstants.AlarmStatus.convert2MCS(alarm_status);
            string alid = error_code;
            //string altx = $"[{SCUtility.Trim(vhID, true) }]{desc}";
            string altx = desc;
            iBSEMDriver.S6F11SendAlarmCleared(vhID, transferID, alid, altx);
            iBSEMDriver.S5F1SendAlarmReport(alcd, alid, altx);
            return true;
        }


        public bool ReportUnitAlarmHappend(string vhID, ErrorStatus alarm_status, string error_code, string desc, List<AMCSREPORTQUEUE> reportqueues)
        {
            string alcd = SCAppConstants.AlarmStatus.convert2MCS(alarm_status);
            string alid = error_code;
            //string altx = $"[{SCUtility.Trim(vhID, true) }]{desc}";
            string altx = desc;
            iBSEMDriver.S6F11SendUnitAlarmSet(vhID, alid, altx);
            iBSEMDriver.S5F1SendAlarmReport(alcd, alid, altx);
            return true;
        }
        public bool ReportUnitAlarmCleared(string vhID, ErrorStatus alarm_status, string error_code, string desc, List<AMCSREPORTQUEUE> reportqueues)
        {
            string alcd = SCAppConstants.AlarmStatus.convert2MCS(alarm_status);
            string alid = error_code;
            //string altx = $"[{SCUtility.Trim(vhID, true) }]{desc}";
            string altx = desc;
            iBSEMDriver.S6F11SendUnitAlarmCleared(vhID, alid, altx);
            iBSEMDriver.S5F1SendAlarmReport(alcd, alid, altx);
            return true;
        }


        public bool ReportOperatorInitiatedAction(string cmdID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendOperatorInitiatedAction(cmdID, reportqueues);
            return isSuccsess;
        }

        public bool ReportVehicleChargeRequest(string vhID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendVehicleChargeRequest(vhID, reportqueues);
            return isSuccsess;
        }

        public bool ReportVehicleChargeStarted(string vhID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendVehicleChargeStarted(vhID, reportqueues);
            return isSuccsess;
        }

        public bool ReportVehicleChargeFinished(string vhID, List<AMCSREPORTQUEUE> reportqueues)
        {
            bool isSuccsess = true;
            isSuccsess = isSuccsess && iBSEMDriver.S6F11SendVehicleChargeCompleted(vhID, reportqueues);
            return isSuccsess;
        }

        public void insertMCSReport(List<AMCSREPORTQUEUE> mcsQueues)
        {
            //using (DBConnection_EF con = DBConnection_EF.GetUContext())
            //{
            //    mcsReportQueueDao.AddByBatch(con, mcsQueues);
            //}
        }

        public void insertMCSReport(AMCSREPORTQUEUE mcs_queue)
        {
            //lock (mcs_report_lock_obj)
            //{
            if (mcs_queue == null)
            {
                return;
            }
            SCUtility.LockWithTimeout(mcs_report_lock_obj, SCAppConstants.LOCK_TIMEOUT_MS,
                () =>
                {
                    //DBConnection_EF con = DBConnection_EF.GetContext();
                    //using (DBConnection_EF con = new DBConnection_EF())
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        mcsReportQueueDao.add(con, mcs_queue);
                    }
                });
            //}
        }
        object mcs_report_lock_obj = new object();

        public bool updateMCSReportTime2Empty(AMCSREPORTQUEUE ReportQueue)
        {
            bool isSuccess = false;
            //DBConnection_EF con = DBConnection_EF.GetContext();
            try
            {
                //using (DBConnection_EF con = new DBConnection_EF())
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    //con.BeginTransaction();
                    con.AMCSREPORTQUEUE.Attach(ReportQueue);
                    ReportQueue.REPORT_TIME = null;
                    con.Entry(ReportQueue).Property(p => p.REPORT_TIME).IsModified = true;
                    mcsReportQueueDao.Update(con, ReportQueue);
                    con.Entry(ReportQueue).State = System.Data.Entity.EntityState.Detached;

                    //con.Commit();
                }
                isSuccess = true;
            }
            catch (Exception ex)
            {
                //if (con != null) { try { con.Rollback(); } catch (Exception ex_rollback) { logger.Error(ex_rollback, "Exception"); } }
                logger.Error(ex, "Exception");
                return isSuccess;
            }
            finally
            {
                //if (con != null) { try { con.Close(); } catch (Exception ex_close) { logger.Error(ex_close, "Exception"); } }
            }
            return isSuccess;
        }


        public bool sendMCSMessage(AMCSREPORTQUEUE mcsMessageQueue)
        {
            return iBSEMDriver.S6F11SendMessage(mcsMessageQueue);
        }


        public List<AMCSREPORTQUEUE> loadNonReportEvent()
        {
            List<AMCSREPORTQUEUE> AMCSREPORTQUEUEs;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                AMCSREPORTQUEUEs = mcsReportQueueDao.loadByNonReport(con);
            }

            return AMCSREPORTQUEUEs;
        }
        #endregion MCS SXFY Report


        #region Zabbix Report


        public Tuple<string, int> getZabbixServerIPAndPort()
        {
            DataCollectionSetting setting = dataCollectionDao.getDataCollectionFirstItem(scApp);
            string ip = setting.IP;
            var remoteipAdr = System.Net.Dns.GetHostAddresses(setting.IP);
            if (remoteipAdr != null && remoteipAdr.Count() != 0)
            {
                ip = remoteipAdr[0].ToString();
            }
            return new Tuple<string, int>(ip, setting.Port);
        }
        public string getZabbixHostName()
        {
            //DataCollectionSetting setting = dataCollectionDao.getDataCollectionFirstItem(scApp);
            //return setting.Method;
            return SCApplication.ServerName;
        }

        public bool IsReportZabbixInfo(string item_name)
        {
            DataCollectionSetting setting = dataCollectionDao.getDataCollectionItemByMethodAndItemName(scApp, item_name);
            if (setting == null)
                return false;
            return setting.IsReport;
        }
        public void ZabbixPush(string key, int value)
        {
            ZabbixPush(key, value.ToString());
        }
        //[Conditional("Release")]
        public void ZabbixPush(string key, string value)
        {
            try
            {
                string zabbix_host_name = getZabbixHostName();
                if (!IsReportZabbixInfo(key))
                    return;
                //var response1 = scApp.ZabbixService.Send(zabbix_host_name, key, value);
                //if (response1.Failed != 0)
                //{
                //    logger.Error($"Push zabbix fail,key:{key},value:{value},info:{response1.Info},responsel:{response1.Response}");
                //}
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
        }
        #endregion Zabbix Report





    }
}
