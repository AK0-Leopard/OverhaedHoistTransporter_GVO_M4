using com.mirle.ibg3k0.bc.winform.App;
using com.mirle.ibg3k0.bc.winform.Common;
using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.sc;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction;
using com.mirle.ibg3k0.sc.Data.VO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static com.mirle.ibg3k0.sc.App.SCAppConstants;

namespace com.mirle.ibg3k0.bc.winform.UI
{
    public partial class DebugForm : Form
    {

        BCMainForm mainForm;
        BCApplication bcApp;
        List<RadioButton> radioButtons = new List<RadioButton>();
#pragma warning disable CS0414 // 已指派欄位 'DebugForm.blocked_queues'，但從未使用過其值。
        List<BLOCKZONEQUEUE> blocked_queues = null;
#pragma warning restore CS0414 // 已指派欄位 'DebugForm.blocked_queues'，但從未使用過其值。
        protected sc.Data.SECSDriver.IBSEMDriver mcsMapAction = null;
        APORTSTATION P11 = null;
        APORTSTATION P12 = null;
        APORTSTATION P13 = null;
        APORTSTATION P14 = null;
        List<APORTSTATION> agvPortStation = null;
        List<AGVStation> agvStations = null;
        AEQPT mCharger = null;
        MaintainLift MTL = null;
        public DebugForm(BCMainForm _mainForm)
        {
            InitializeComponent();
            mainForm = _mainForm;
            bcApp = mainForm.BCApp;

            cb_StartGenAntoCmd.Checked = DebugParameter.CanAutoRandomGeneratesCommand;
            numer_num_of_avoid_seg.Value = DebugParameter.NumberOfAvoidanceSegment;
            ck_check_port_is_ready.Checked = DebugParameter.isNeedCheckPortReady;
            cb_reserve_pass.Checked = DebugParameter.isForcedPassReserve;
            cb_reserve_pass_agv0609.Checked = DebugParameter.isForcedPassReserve_AGV0609;

            cb_passCouplerStatus.Checked = DebugParameter.isPassCouplerStatus;
            cb_passCouplerHPSafetySingnal.Checked = DebugParameter.isPassCouplerHPSafetySignal;

            cb_needCheckPortUpdateTime.Checked = DebugParameter.isNeedCheckPortUpDateTime;
            EQReportRemoveCB.Checked = DebugParameter.isReportEQPortCarrierRemove;
            ForceBypassMTLPauseCheckCB.Checked = DebugParameter.isForceBypassMTLPauseCheck;
            ForceBypassVehicleConnectionPauseCheckCB.Checked = DebugParameter.isForceBypassVehclePauseCheck;
            if (DebugParameter.isDefaultOHTModeAutoRemote)
            {
                autoremote_rbn.Checked = true;
            }
            else
            {
                autolocal_rbn.Checked = true;
            }
            List<string> lstVh = new List<string>();
            lstVh.Add(string.Empty);
            lstVh.AddRange(bcApp.SCApplication.VehicleBLL.cache.loadAllVh().Select(vh => vh.VEHICLE_ID).ToList());
            string[] allVh = lstVh.ToArray();
            BCUtility.setComboboxDataSource(cmb_tcpipctr_Vehicle, allVh);
            BCUtility.setComboboxDataSource(cmb_mcsReportTestVHID, allVh);

            List<AADDRESS> allAddress_obj = bcApp.SCApplication.MapBLL.loadAllAddress();
            string[] allAdr_ID = allAddress_obj.Select(adr => adr.ADR_ID).ToArray();
            BCUtility.setComboboxDataSource(cmb_teach_from_adr, allAdr_ID);
            BCUtility.setComboboxDataSource(cmb_teach_to_adr, allAdr_ID.ToArray());

            List<ASECTION> sections = bcApp.SCApplication.SectionBLL.cache.GetSections();
            string[] allSec_ID = sections.Select(sec => sec.SEC_ID).ToArray();
            BCUtility.setComboboxDataSource(cmb_reserve_section, allSec_ID.ToArray());
            BCUtility.setComboboxDataSource(cmb_reserve_section1, allSec_ID.ToArray());
            BCUtility.setComboboxDataSource(cmb_reserve_section2, allSec_ID.ToArray());

            cb_OperMode.DataSource = Enum.GetValues(typeof(sc.ProtocolFormat.OHTMessage.OperatingVHMode));
            cb_PwrMode.DataSource = Enum.GetValues(typeof(sc.ProtocolFormat.OHTMessage.OperatingPowerMode));
            cmb_pauseEvent.DataSource = Enum.GetValues(typeof(sc.ProtocolFormat.OHTMessage.PauseEvent));
            cmb_pauseType.DataSource = Enum.GetValues(typeof(sc.ProtocolFormat.OHTMessage.PauseType));
            cb_Abort_Type.DataSource = Enum.GetValues(typeof(sc.ProtocolFormat.OHTMessage.CMDCancelType));
            cmb_cycle_run_type.DataSource = Enum.GetValues(typeof(DebugParameter.CycleRunTestType));
            cbTranMode.DataSource = Enum.GetValues(typeof(DebugParameter.TransferModeType));
            cmbVhType.DataSource = Enum.GetValues(typeof(E_VH_TYPE));
            cmbUnloadVhType.DataSource = Enum.GetValues(typeof(E_VH_TYPE));

            mcsMapAction = SCApplication.getInstance().getEQObjCacheManager().getLine().getMapActionByIdentityKey(typeof(MCSDefaultMapAction).Name) as MCSDefaultMapAction;

            cb_Cache_data_Name.Items.Add("");
            cb_Cache_data_Name.Items.Add("ASECTION");
            dgv_cache_object_data.AutoGenerateColumns = false;
            dgv_cache_object_data_portstation.AutoGenerateColumns = false;
            dgv_AGVStationInfo.AutoGenerateColumns = false;
            P11 = bcApp.SCApplication.getEQObjCacheManager().getPortStation("AASTK250P01");
            P12 = bcApp.SCApplication.getEQObjCacheManager().getPortStation("AASTK250P02");
            P13 = bcApp.SCApplication.getEQObjCacheManager().getPortStation("CASTK010P01");
            P14 = bcApp.SCApplication.getEQObjCacheManager().getPortStation("CASTK010P02");

            if (P11?.PORT_SERVICE_STATUS == sc.ProtocolFormat.OHTMessage.PortStationServiceStatus.InService)
            {
                comboBox_port11.SelectedIndex = 0;
            }
            else
            {
                comboBox_port11.SelectedIndex = 1;
            }
            if (P12?.PORT_SERVICE_STATUS == sc.ProtocolFormat.OHTMessage.PortStationServiceStatus.InService)
            {
                comboBox_port12.SelectedIndex = 0;
            }
            else
            {
                comboBox_port12.SelectedIndex = 1;
            }
            if (P13?.PORT_SERVICE_STATUS == sc.ProtocolFormat.OHTMessage.PortStationServiceStatus.InService)
            {
                comboBox_port13.SelectedIndex = 0;
            }
            else
            {
                comboBox_port13.SelectedIndex = 1;
            }
            if (P14?.PORT_SERVICE_STATUS == sc.ProtocolFormat.OHTMessage.PortStationServiceStatus.InService)
            {
                comboBox_port14.SelectedIndex = 0;
            }
            else
            {
                comboBox_port14.SelectedIndex = 1;
            }

            cmb_cycle_run_type.SelectedItem = DebugParameter.CycleRunType;
            cbTranMode.SelectedItem = DebugParameter.TransferMode;
            num_BatteryLowBoundaryValue.Value = AVEHICLE.BATTERYLEVELVALUE_LOW;
            num_BatteryHighBoundaryValue.Value = AVEHICLE.BATTERYLEVELVALUE_HIGH;
            numer_pre_open_agv_station_distance.Value = sc.App.SystemParameter.OpenAGVStationCoverDistance_mm;
            num_tran_cmd_queue_time_out_ms.Value = sc.App.SystemParameter.TransferCommandQueueTimeOut_mSec;
            //cb_by_pass_shelf_status.Checked = sc.App.SystemParameter.IsByPassAGVShelfStatus;
            num_vh_idle_time.Value = sc.App.SystemParameter.AllowVhIdleTime_ms;

            agvPortStation = bcApp.SCApplication.PortStationBLL.OperateCatch.loadAllPortStation();
            dgv_cache_object_data_portstation.DataSource = agvPortStation;
            //cmb_mcsReportTestPortID.DataSource = agvPortStation;
            foreach (APORTSTATION p in agvPortStation)
            {
                cmb_mcsReportTestPortID.Items.Add(p.PORT_ID.Trim());
            }

            agvStations = bcApp.SCApplication.EqptBLL.OperateCatch.loadAllAGVStation();
            dgv_AGVStationInfo.DataSource = agvStations;
            string[] agv_station_ids = agvStations.Select(station => station.EQPT_ID).ToArray();
            BCUtility.setComboboxDataSource(cmbStationPortID, agv_station_ids.ToArray());

            tabControl1.TabPages.RemoveAt(1);

            mCharger = bcApp.SCApplication.getEQObjCacheManager().getEquipmentByEQPTID("MCharger");
            if (mCharger != null)
            {
                foreach (AUNIT unit in mCharger.UnitList)
                {
                    cb_Charger.Items.Add(unit.UNIT_ID);
                    cb_ChargerID.Items.Add(unit.UNIT_ID);
                }
            }
            for (int i = 0; i < 15; i++)
            {
                cb_PIOCoupler.Items.Add(i + 1);
            }
            initChargerValue();
            registerEvent();
            foreach (var v in agvPortStation)
            {
                cb_PortID.Items.Add(v.PORT_ID);
            }
            AEQPT eqpt = bcApp.SCApplication.getEQObjCacheManager().getEquipmentByEQPTID("MTL");
            if (eqpt != null)
            {
                MTL = eqpt as MaintainLift;
            }

            registerMTLEvent();
            refreshMTLInfo();

        }



        #region Charger
        private void initChargerValue()
        {
            if (mCharger != null)
            {
                agvcAliveChange(mCharger);
                MChargerAbnormalIndexChange(mCharger);
                foreach (AUNIT unit in mCharger.UnitList)
                {
                    ChargerAliveChange(unit);
                    CurrentSupplyStatusBlockChange(unit);
                    ChargerStatusIndexChange(unit);
                    ChargerCurrentParameterindexChange(unit);
                    CouplerChargeInfoIndexChange(unit);
                    PIOIndexChange(unit);
                }
            }
        }
        private void registerEvent()
        {
            string Handler = this.Name;
            if (mCharger != null)
            {
                mCharger.addEventHandler(Handler, BCFUtility.getPropertyName(() => mCharger.OHTCAliveIndex),
                    (s1, e1) => agvcAliveChange(mCharger));

                mCharger.addEventHandler(Handler, BCFUtility.getPropertyName(() => mCharger.AbnormalReportIndex),
                    (s1, e1) => MChargerAbnormalIndexChange(mCharger));

                foreach (AUNIT unit in mCharger.UnitList)
                {
                    unit.addEventHandler(Handler, BCFUtility.getPropertyName(() => unit.ChargerAlive),
                        (s1, e1) => ChargerAliveChange(unit));
                    unit.addEventHandler(Handler, BCFUtility.getPropertyName(() => unit.CurrentSupplyStatusBlock),
                        (s1, e1) => CurrentSupplyStatusBlockChange(unit));
                    unit.addEventHandler(Handler, BCFUtility.getPropertyName(() => unit.ChargerStatusIndex),
                        (s1, e1) => ChargerStatusIndexChange(unit));
                    unit.addEventHandler(Handler, BCFUtility.getPropertyName(() => unit.ChargerCurrentParameterIndex),
                        (s1, e1) => ChargerCurrentParameterindexChange(unit));
                    unit.addEventHandler(Handler, BCFUtility.getPropertyName(() => unit.CouplerChargeInfoIndex),
                        (s1, e1) => CouplerChargeInfoIndexChange(unit));
                    unit.addEventHandler(Handler, BCFUtility.getPropertyName(() => unit.PIOIndex),
                        (s1, e1) => PIOIndexChange(unit));

                }
            }
        }
        private void unregisterEvent()
        {
            if (mCharger != null)
            {
                mCharger.RemoveAllEvents();

                foreach (AUNIT unit in mCharger.UnitList)
                {
                    unit.RemoveAllEvents();
                }
            }
        }

        private void agvcAliveChange(AEQPT eqpt)
        {


            Adapter.Invoke(new SendOrPostCallback((o1) =>
            {
                tb_agvcAlive.Text = eqpt.OHTCAliveIndex.ToString();
            }), null);

        }
        private void MChargerAbnormalIndexChange(AEQPT eqpt)
        {
            Adapter.Invoke(new SendOrPostCallback((o1) =>
            {
                tb_ErrorReportIndex.Text = eqpt.AbnormalReportIndex.ToString();
                tb_ErrorReportCode1.Text = eqpt.abnormalReportCode01.ToString();
                tb_ErrorReportCode2.Text = eqpt.abnormalReportCode02.ToString();
                tb_ErrorReportCode3.Text = eqpt.abnormalReportCode03.ToString();
                tb_ErrorReportCode4.Text = eqpt.abnormalReportCode04.ToString();
                tb_ErrorReportCode5.Text = eqpt.abnormalReportCode05.ToString();
                tb_ErrorReportCode6.Text = eqpt.abnormalReportCode06.ToString();
                tb_ErrorReportCode7.Text = eqpt.abnormalReportCode07.ToString();
                tb_ErrorReportCode8.Text = eqpt.abnormalReportCode08.ToString();
                tb_ErrorReportCode9.Text = eqpt.abnormalReportCode09.ToString();
                tb_ErrorReportCode10.Text = eqpt.abnormalReportCode10.ToString();
                tb_ErrorReportCode11.Text = eqpt.abnormalReportCode11.ToString();
                tb_ErrorReportCode12.Text = eqpt.abnormalReportCode12.ToString();
                tb_ErrorReportCode13.Text = eqpt.abnormalReportCode13.ToString();
                tb_ErrorReportCode14.Text = eqpt.abnormalReportCode14.ToString();
                tb_ErrorReportCode15.Text = eqpt.abnormalReportCode15.ToString();
                tb_ErrorReportCode16.Text = eqpt.abnormalReportCode16.ToString();
                tb_ErrorReportCode17.Text = eqpt.abnormalReportCode17.ToString();
                tb_ErrorReportCode18.Text = eqpt.abnormalReportCode18.ToString();
                tb_ErrorReportCode19.Text = eqpt.abnormalReportCode19.ToString();
                tb_ErrorReportCode20.Text = eqpt.abnormalReportCode20.ToString();
            }), null);
        }
        private void ChargerAliveChange(AUNIT unit)
        {
            Adapter.Invoke(new SendOrPostCallback((o1) =>
            {
                if (unit.UNIT_ID != cb_ChargerID.Text) return;
                tb_ChargerAlive.Text = unit.ChargerAlive.ToString();
            }), null);
        }
        private void CurrentSupplyStatusBlockChange(AUNIT unit)
        {
            Adapter.Invoke(new SendOrPostCallback((o1) =>
            {
                if (unit.UNIT_ID != cb_ChargerID.Text) return;
                tb_inputVoltage.Text = unit.inputVoltage.ToString();
                tb_chargeVoltage.Text = unit.chargeVoltage.ToString();
                textBox_chargeCurrent.Text = unit.chargeCurrent.ToString();
                textBox_chargePower.Text = unit.chargePower.ToString();
                tb_couplerChargeVoltage.Text = unit.couplerChargeVoltage.ToString();
                tb_couplerChargeCurrent.Text = unit.couplerChargeCurrent.ToString();
                textBox_couplerID.Text = unit.couplerID.ToString();
            }), null);
        }
        private void ChargerStatusIndexChange(AUNIT unit)
        {
            Adapter.Invoke(new SendOrPostCallback((o1) =>
            {
                if (unit.UNIT_ID != cb_ChargerID.Text) return;
                tb_ChargerStatusReportIndex.Text = unit.ChargerStatusIndex.ToString();
                tb_Reserve.Text = unit.chargerReserve.ToString();
                tb_ConstantVoOutput.Text = unit.chargerConstantVoltageOutput.ToString();
                tb_ContantCurrentOutput.Text = unit.chargerConstantCurrentOutput.ToString();
                tb_HighInputVoProtect.Text = unit.chargerHighInputVoltageProtection.ToString();
                tb_LowInputVoProtect.Text = unit.chargerLowInputVoltageProtection.ToString();
                tb_HighOutputVoProtect.Text = unit.chargerHighOutputVoltageProtection.ToString();
                tb_HighOutputCurrentProtect.Text = unit.chargerHighOutputCurrentProtection.ToString();
                tb_OverheatProtect.Text = unit.chargerOverheatProtection.ToString();
                tb_RS485Status.Text = unit.chargerRS485Status.ToString();
                tb_CouplerStatus1.Text = unit.Coupler1Status.ToString();
                tb_CouplerStatus2.Text = unit.Coupler2Status.ToString();
                tb_CouplerStatus3.Text = unit.Coupler3Status.ToString();
                tb_Coupler1HPSafety.Text = unit.coupler1HPSafety.ToString();
                tb_Coupler2HPSafety.Text = unit.coupler2HPSafety.ToString();
                tb_Coupler3HPSafety.Text = unit.coupler3HPSafety.ToString();
            }), null);
        }
        private void ChargerCurrentParameterindexChange(AUNIT unit)
        {
            Adapter.Invoke(new SendOrPostCallback((o1) =>
            {
                if (unit.UNIT_ID != cb_ChargerID.Text) return;
                tb_ChargerCurrentParameterSettingIndex.Text = unit.ChargerCurrentParameterIndex.ToString();
                tb_OutputVo.Text = unit.chargerOutputVoltage.ToString();
                tb_OutputCurrent.Text = unit.chargerOutputCurrent.ToString();
                tb_OverloadVo.Text = unit.chargerOverVoltage.ToString();
                tb_OverloadCurrent.Text = unit.chargerOverCurrent.ToString();
            }), null);
        }
        private void CouplerChargeInfoIndexChange(AUNIT unit)
        {
            Adapter.Invoke(new SendOrPostCallback((o1) =>
            {
                if (unit.UNIT_ID != cb_ChargerID.Text) return;
                tb_CouplerChargeInfoReportIndex.Text = unit.CouplerChargeInfoIndex.ToString();
                tb_CouplerID.Text = unit.chargerCouplerID.ToString();
                tb_ChargeStartTime.Text = unit.chargerChargingStartTime.ToString("yyyy/MM/dd hh:mm:ss");
                tb_ChargeEndTime.Text = unit.chargerChargingEndTime.ToString("yyyy/MM/dd hh:mm:ss");
                tb_InputAh.Text = unit.chargerInputAH.ToString();
                tb_ChargeResult.Text = unit.chargerChargingResult.ToString();
            }), null);
        }
        private void PIOIndexChange(AUNIT unit)
        {
            Adapter.Invoke(new SendOrPostCallback((o1) =>
            {
                tb_PIOIndex.Text = unit.PIOIndex.ToString();
                int x;
                bool result = int.TryParse(cb_PIOCoupler.Text, out x);
                if (result)
                {
                    if (unit.PIOInfos.Count >= x)
                    {
                        tb_PIOCouplerID.Text = unit.PIOInfos[x - 1].CouplerID.ToString();
                        tb_PIOHandshakeTime.Text = unit.PIOInfos[x - 1].Timestamp.ToString("yyyy/MM/dd hh:mm:ss");
                        tb_PIOSignal1.Text = unit.PIOInfos[x - 1].signal1.Replace(",", "").Replace("True", "1").Replace("False", "0");
                        tb_PIOSignal2.Text = unit.PIOInfos[x - 1].signal2.Replace(",", "").Replace("True", "1").Replace("False", "0");
                    }
                }
            }), null);
        }
        private void cb_ChargerID_SelectedIndexChanged(object sender, EventArgs e)
        {
            AUNIT unit = bcApp.SCApplication.getEQObjCacheManager().getUnitByUnitID(cb_ChargerID.Text);
            if (unit != null)
            {
                tb_ChargerAlive.Text = unit.ChargerAlive.ToString();
                tb_inputVoltage.Text = unit.inputVoltage.ToString();
                tb_chargeVoltage.Text = unit.chargeVoltage.ToString();
                textBox_chargeCurrent.Text = unit.chargeCurrent.ToString();
                textBox_chargePower.Text = unit.chargePower.ToString();
                tb_couplerChargeVoltage.Text = unit.couplerChargeVoltage.ToString();
                tb_couplerChargeCurrent.Text = unit.couplerChargeCurrent.ToString();
                textBox_couplerID.Text = unit.couplerID.ToString();

                tb_ChargerStatusReportIndex.Text = unit.ChargerStatusIndex.ToString();
                tb_Reserve.Text = unit.chargerReserve.ToString();
                tb_ConstantVoOutput.Text = unit.chargerConstantVoltageOutput.ToString();
                tb_ContantCurrentOutput.Text = unit.chargerConstantCurrentOutput.ToString();
                tb_HighInputVoProtect.Text = unit.chargerHighInputVoltageProtection.ToString();
                tb_LowInputVoProtect.Text = unit.chargerLowInputVoltageProtection.ToString();
                tb_HighOutputVoProtect.Text = unit.chargerHighOutputVoltageProtection.ToString();
                tb_HighOutputCurrentProtect.Text = unit.chargerHighOutputCurrentProtection.ToString();
                tb_OverheatProtect.Text = unit.chargerOverheatProtection.ToString();
                tb_RS485Status.Text = unit.chargerRS485Status.ToString();
                tb_CouplerStatus1.Text = unit.Coupler1Status.ToString();
                tb_CouplerStatus2.Text = unit.Coupler2Status.ToString();
                tb_CouplerStatus3.Text = unit.Coupler3Status.ToString();
                tb_Coupler1HPSafety.Text = unit.coupler1HPSafety.ToString();
                tb_Coupler2HPSafety.Text = unit.coupler2HPSafety.ToString();
                tb_Coupler3HPSafety.Text = unit.coupler3HPSafety.ToString();

                tb_ChargerCurrentParameterSettingIndex.Text = unit.ChargerCurrentParameterIndex.ToString();
                tb_OutputVo.Text = unit.chargerOutputVoltage.ToString();
                tb_OutputCurrent.Text = unit.chargerOutputCurrent.ToString();
                tb_OverloadVo.Text = unit.chargerOverVoltage.ToString();
                tb_OverloadCurrent.Text = unit.chargerOverCurrent.ToString();

                tb_CouplerChargeInfoReportIndex.Text = unit.CouplerChargeInfoIndex.ToString();
                tb_CouplerID.Text = unit.chargerCouplerID.ToString();
                tb_ChargeStartTime.Text = unit.chargerChargingStartTime.ToString("yyyy/MM/dd hh:mm:ss");
                tb_ChargeEndTime.Text = unit.chargerChargingEndTime.ToString("yyyy/MM/dd hh:mm:ss");
                tb_InputAh.Text = unit.chargerInputAH.ToString();
                tb_ChargeResult.Text = unit.chargerChargingResult;

                tb_PIOIndex.Text = unit.PIOIndex.ToString();
                int x;
                bool result = int.TryParse(cb_PIOCoupler.Text, out x);
                if (result)
                {
                    if (unit.PIOInfos.Count >= x)
                    {
                        tb_PIOCouplerID.Text = unit.PIOInfos[x - 1].CouplerID.ToString();
                        tb_PIOHandshakeTime.Text = unit.PIOInfos[x - 1].Timestamp.ToString("yyyy/MM/dd hh:mm:ss");
                        tb_PIOSignal1.Text = unit.PIOInfos[x - 1].signal1.Replace(",", "").Replace("True", "1").Replace("False", "0");
                        tb_PIOSignal2.Text = unit.PIOInfos[x - 1].signal2.Replace(",", "").Replace("True", "1").Replace("False", "0");
                    }
                }
            }

        }
        private void cb_PIOCoupler_SelectedIndexChanged(object sender, EventArgs e)
        {
            AUNIT unit = bcApp.SCApplication.getEQObjCacheManager().getUnitByUnitID(cb_ChargerID.Text);
            if (unit != null)
            {
                tb_PIOIndex.Text = unit.PIOIndex.ToString();
                int x;
                bool result = int.TryParse(cb_PIOCoupler.Text, out x);
                if (result)
                {
                    if (unit.PIOInfos.Count >= x)
                    {
                        tb_PIOCouplerID.Text = unit.PIOInfos[x - 1].CouplerID.ToString();
                        tb_PIOHandshakeTime.Text = unit.PIOInfos[x - 1].Timestamp.ToString("yyyy/MM/dd hh:mm:ss");
                        tb_PIOSignal1.Text = unit.PIOInfos[x - 1].signal1.Replace(",", "").Replace("True", "1").Replace("False", "0");
                        tb_PIOSignal2.Text = unit.PIOInfos[x - 1].signal2.Replace(",", "").Replace("True", "1").Replace("False", "0");
                    }
                }
            }
        }
        #endregion Charger




        private void DebugForm_Load(object sender, EventArgs e)
        {
            DebugParameter.IsDebugMode = true;
            //refreshCouplerStatusUI();
        }

        private void DebugForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            DebugParameter.IsDebugMode = false;
            DebugParameter.IsCycleRun = false;
            DebugParameter.CanAutoRandomGeneratesCommand = false;

            //DebugParameter.isForcedPassReserve = false;
            DebugParameter.isForcedRejectReserve = false;
            DebugParameter.isContinueByIDReadFail = false;
            DebugParameter.testRetryReserveReq = false;
            DebugParameter.testRetryLoadArrivals = false;
            DebugParameter.testRetryLoadComplete = false;
            DebugParameter.testRetryUnloadArrivals = false;
            DebugParameter.testRetryUnloadComplete = false;
            DebugParameter.testRetryVhloading = false;
            DebugParameter.testRetryVhunloading = false;
            DebugParameter.testRetryBcrread = false;
            DebugParameter.CommandCheckForcePass = false;
            DebugParameter.CommandCompleteWaitTime = 0;

            mainForm.removeForm(typeof(DebugForm).Name);

            unregisterEvent();

        }




        AVEHICLE noticeCar = null;
        string vh_id = null;
        private async void cmb_Vehicle_SelectedIndexChanged(object sender, EventArgs e)
        {
            vh_id = cmb_tcpipctr_Vehicle.Text.Trim();

            noticeCar = bcApp.SCApplication.getEQObjCacheManager().getVehicletByVHID(vh_id);
            lbl_install_status.Text = noticeCar?.IS_INSTALLED.ToString();

            string[] command_ids = null;
            await Task.Run(() =>
            {
                command_ids = bcApp.SCApplication.CMDBLL.loadUnfinishCmd(vh_id)?.Select(cmd => cmd.ID).ToArray();
            });
            BCUtility.setComboboxDataSource(cmb_command_ids, command_ids);
            cmbVhType.SelectedItem = noticeCar?.VEHICLE_TYPE;
        }

        private void uctl_Btn1_Click(object sender, EventArgs e)
        {
            asyExecuteAction(bcApp.SCApplication.VehicleService.HostBasicVersionReport);
            //asyExecuteAction(noticeCar.sned_S1);
        }
        private void uctl_SendFun11_Click(object sender, EventArgs e)
        {
            //asyExecuteAction(bcApp.SCApplication.VehicleService.CoplerInfosReport);
        }
        private void uctl_SendFun13_Click(object sender, EventArgs e)
        {
            asyExecuteAction(bcApp.SCApplication.VehicleService.TavellingDataReport);
            //asyExecuteAction(noticeCar.sned_S13);
        }
        private void uctl_SendFun15_Click(object sender, EventArgs e)
        {
        }
        private void uctl_SendFun17_Click(object sender, EventArgs e)
        {
            asyExecuteAction(bcApp.SCApplication.VehicleService.AddressDataReport);
            //asyExecuteAction(noticeCar.sned_S17);
        }

        private void uctl_SendFun19_Click(object sender, EventArgs e)
        {
            asyExecuteAction(bcApp.SCApplication.VehicleService.ScaleDataReport);
            //asyExecuteAction(noticeCar.sned_S19);
        }

        private void uctl_SendFun21_Click(object sender, EventArgs e)
        {
            asyExecuteAction(bcApp.SCApplication.VehicleService.ControlDataReport);
            //asyExecuteAction(noticeCar.sned_S21);
        }

        private void uctl_SendFun23_Click(object sender, EventArgs e)
        {
            asyExecuteAction(bcApp.SCApplication.VehicleService.GuideDataReport);
            //asyExecuteAction(noticeCar.sned_S23);
        }

        private void asyExecuteAction(Func<string, bool> act)
        {
            Task.Run(() =>
            {
                act(vh_id);
            });
        }

        private void uctl_SendAllFun_Click(object sender, EventArgs e)
        {
            asyExecuteAction(bcApp.SCApplication.VehicleService.doDataSysc);
            //asyExecuteAction(noticeCar.sned_ALL);
        }

        private void uctl_Send_Fun_71_Click(object sender, EventArgs e)
        {
            string from_adr = cmb_teach_from_adr.Text;
            string to_adr = cmb_teach_to_adr.Text;
            Task.Run(() =>
            {
                bcApp.SCApplication.VehicleService.Send.Teaching(vh_id, from_adr, to_adr);
                //noticeCar.send_Str71(from_adr, to_adr);
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            asyExecuteAction(bcApp.SCApplication.VehicleService.IndividualUploadRequest);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            asyExecuteAction(bcApp.SCApplication.VehicleService.IndividualChangeRequest);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            sc.ProtocolFormat.OHTMessage.OperatingVHMode operatiogMode;
            Enum.TryParse(cb_OperMode.SelectedValue.ToString(), out operatiogMode);

            Task.Run(() =>
            {
                bcApp.SCApplication.VehicleService.Send.ModeChange(vh_id, operatiogMode);
            });
        }

        private void button4_Click(object sender, EventArgs e)
        {
            sc.ProtocolFormat.OHTMessage.OperatingPowerMode operatiogPowerMode;
            Enum.TryParse(cb_PwrMode.SelectedValue.ToString(), out operatiogPowerMode);

            Task.Run(() =>
            {
                bcApp.SCApplication.VehicleService.Send.PowerOperatorChange(vh_id, operatiogPowerMode);
            });
        }

        private void button5_Click(object sender, EventArgs e)
        {
            asyExecuteAction(bcApp.SCApplication.VehicleService.Send.AlarmReset);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                bool isSuccess = bcApp.SCApplication.CMDBLL.forceUpdataCmdStatus2FnishByVhID(vh_id);
                if (isSuccess)
                {
                    var vh = bcApp.SCApplication.VehicleBLL.cache.getVehicle(vh_id);
                    vh.VehicleUnassign();
                    vh.onExcuteCommandStatusChange();
                }

            });
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                //bcApp.SCApplication.VehicleService.forceResetVHStatus(vh_id);
                bcApp.SCApplication.VehicleService.Send.StatusRequest(vh_id, true);
            });
        }

        private void cb_StartGenAntoCmd_CheckedChanged(object sender, EventArgs e)
        {

            CheckBox cb = sender as CheckBox;
            DebugParameter.CanAutoRandomGeneratesCommand = cb.Checked;

            //if (!cb.Checked)
            //{
            //    Task.Run(() =>
            //    {
            //        var mcs_cmds = bcApp.SCApplication.CMDBLL.loadMCS_Command_Queue();
            //        foreach (var cmd in mcs_cmds)
            //            bcApp.SCApplication.CMDBLL.updateCMD_MCS_TranStatus2Complete(cmd.ID, E_TRAN_STATUS.Canceled);
            //    });
            //}
        }


        private void btn_pause_Click(object sender, EventArgs e)
        {
            sc.ProtocolFormat.OHTMessage.PauseEvent pauseEvent;
            sc.ProtocolFormat.OHTMessage.PauseType pauseType;
            Enum.TryParse(cmb_pauseEvent.SelectedValue.ToString(), out pauseEvent);
            Enum.TryParse(cmb_pauseType.SelectedValue.ToString(), out pauseType);
            Task.Run(() =>
            {
                bcApp.SCApplication.VehicleService.Send.Pause(vh_id, pauseEvent, pauseType);
            });

        }




        private void label17_Click(object sender, EventArgs e)
        {

        }



        private void radio_bitX_Click(object sender, EventArgs e)
        {
            (sender as RadioButton).Checked = !(sender as RadioButton).Checked;
        }




        private void button8_Click(object sender, EventArgs e)
        {
        }

        private void button9_Click(object sender, EventArgs e)
        {
            sc.ProtocolFormat.OHTMessage.CMDCancelType type;
            Enum.TryParse(cb_Abort_Type.SelectedValue.ToString(), out type);

            Task.Run(() =>
            {
                ATRANSFER aTRANSFER = bcApp.SCApplication.TransferBLL.db.transfer.getTransferByExecutedCMDID(noticeCar.CMD_ID);
                if (aTRANSFER != null)
                {
                    bcApp.SCApplication.ReportBLL.newReportTransferCancelInitial(aTRANSFER.ID, null);
                }
                bool result = bcApp.SCApplication.VehicleService.Send.Cancel(noticeCar.VEHICLE_ID, noticeCar.CMD_ID, sc.ProtocolFormat.OHTMessage.CMDCancelType.CmdCancel);
                if (result)
                {

                }
                else
                {
                    if (aTRANSFER != null)
                    {
                        bcApp.SCApplication.ReportBLL.newReportTransferCancelFailed(aTRANSFER.ID, null);
                    }
                }
                //noticeCar.sned_Str37(noticeCar.OHTC_CMD, type); //todo kevin 要填入Command id
            });

        }

        private void button10_Click(object sender, EventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {
        }

        private void btn_blocked_sec_refresh_Click(object sender, EventArgs e)
        {
        }


        private void btn_portInServeice_Click(object sender, EventArgs e)
        {
            string port_id = cb_PortID.Text;
            if (!string.IsNullOrWhiteSpace(port_id))
            {
                Task.Run(() =>
                {
                    portInServeice(port_id);
                });
            }

        }
        private void portInServeice(string port_id)
        {
            APORTSTATION aPORTSTATION = bcApp.SCApplication.PortStationBLL.OperateCatch.getPortStation(port_id);
            if (aPORTSTATION != null)
            {
                if (aPORTSTATION.PORT_SERVICE_STATUS == sc.ProtocolFormat.OHTMessage.PortStationServiceStatus.InService) return;
                bool result = bcApp.SCApplication.PortStationBLL.OperateDB.updateServiceStatus(port_id, sc.ProtocolFormat.OHTMessage.PortStationServiceStatus.InService);
                if (result)
                {
                    bcApp.SCApplication.PortStationBLL.OperateCatch.updateServiceStatus(port_id, sc.ProtocolFormat.OHTMessage.PortStationServiceStatus.InService);
                    List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
                    bcApp.SCApplication.ReportBLL.newReportPortInServeice(port_id, reportqueues);
                    bcApp.SCApplication.ReportBLL.newSendMCSMessage(reportqueues);
                }
            }
        }

        private void btn_portOutOfServeice_Click(object sender, EventArgs e)
        {
            string port_id = cb_PortID.Text;
            Task.Run(() =>
            {
                portOutServeice(port_id);
            });

        }

        private void portOutServeice(string port_id)
        {
            APORTSTATION aPORTSTATION = bcApp.SCApplication.PortStationBLL.OperateCatch.getPortStation(port_id);
            if (aPORTSTATION != null)
            {
                if (aPORTSTATION.PORT_SERVICE_STATUS == sc.ProtocolFormat.OHTMessage.PortStationServiceStatus.OutOfService) return;
                bool result = bcApp.SCApplication.PortStationBLL.OperateDB.updateServiceStatus(port_id, sc.ProtocolFormat.OHTMessage.PortStationServiceStatus.OutOfService);
                if (result)
                {
                    bcApp.SCApplication.PortStationBLL.OperateCatch.updateServiceStatus(port_id, sc.ProtocolFormat.OHTMessage.PortStationServiceStatus.OutOfService);
                    List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
                    bcApp.SCApplication.ReportBLL.newReportPortOutServeice(port_id, reportqueues);
                    bcApp.SCApplication.ReportBLL.newSendMCSMessage(reportqueues);
                }
            }
        }

        private void ck_test_carrierinterface_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.isTestCarrierInterfaceError = ck_test_carrierinterface_error.Checked;
        }

        private void button8_Click_1(object sender, EventArgs e)
        {
        }



        private void ck_autoTech_Click(object sender, EventArgs e)
        {

        }

        private void btn_reset_teach_result_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                List<ASECTION> sections = bcApp.SCApplication.MapBLL.loadAllSection();
                foreach (var sec in sections)
                {
                    if (bcApp.SCApplication.MapBLL.resetSecTechingTime(sec.SEC_ID))
                    {
                        sec.LAST_TECH_TIME = null;
                    }

                }
            });
        }

        private void btn_cmd_override_test_Click(object sender, EventArgs e)
        {
            string vh_id = cmb_tcpipctr_Vehicle.Text;
            Task.Run(() =>
            {
            });
        }

        private void uctl_SendFun2_Click(object sender, EventArgs e)
        {
            //asyExecuteAction(bcApp.SCApplication.VehicleService.BasicInfoVersionReport);
            asyExecuteAction(bcApp.SCApplication.VehicleService.HostBasicVersionReport);


        }

        private void cb_Cache_data_Name_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected_name = (sender as ComboBox).SelectedItem as string;

        }



        private void dgv_cache_object_data_EditModeChanged(object sender, EventArgs e)
        {

        }


        #region MTL Test


        private void btn_mtl_dateTimeSync_Click(object sender, EventArgs e)
        {

        }
        #endregion MTL Test

        private void btn_mtl_message_download_Click(object sender, EventArgs e)
        {

        }

        private void btn_mtl_vh_realtime_info_Click(object sender, EventArgs e)
        {

        }

        private void btn_mtl_car_out_notify_Click(object sender, EventArgs e)
        {

        }

        private void btn_func71_Click(object sender, EventArgs e)
        {
            string[] guide_sections = txt_func51_guideSec.Text.Split(',').Select(sec => sec.PadLeft(4, '0')).ToArray();
            string[] guide_addresses = txt_func51_guideAdr.Text.Split(',').Select(adr => adr.PadLeft(4, '0')).ToArray();
            Task.Run(() =>
            {
                //bcApp.SCApplication.VehicleService.AvoidRequest(vh_id, "", guide_sections, guide_addresses);
            });
        }

        private void btn_coulper_enable_Click(object sender, EventArgs e)
        {

        }

        private void btn_ReserveTest_Click(object sender, EventArgs e)
        {
            string sec_id = cmb_reserve_section.Text;
            string sec_id_1 = cmb_reserve_section1.Text;
            string sec_id_2 = cmb_reserve_section2.Text;
            sc.ProtocolFormat.OHTMessage.DriveDirction driveDirction = radio_Forward.Checked ?
                sc.ProtocolFormat.OHTMessage.DriveDirction.DriveDirForward :
                sc.ProtocolFormat.OHTMessage.DriveDirction.DriveDirReverse;
            Task.Run(() =>
            {
                Google.Protobuf.Collections.RepeatedField<sc.ProtocolFormat.OHTMessage.ReserveInfo> reserves = new Google.Protobuf.Collections.RepeatedField<sc.ProtocolFormat.OHTMessage.ReserveInfo>();
                if (!sc.Common.SCUtility.isEmpty(sec_id))
                    reserves.Add(new sc.ProtocolFormat.OHTMessage.ReserveInfo()
                    {
                        ReserveSectionID = sec_id,
                        DriveDirction = driveDirction
                    });
                if (!sc.Common.SCUtility.isEmpty(sec_id_1))
                    reserves.Add(new sc.ProtocolFormat.OHTMessage.ReserveInfo()
                    {
                        ReserveSectionID = sec_id_1,
                        DriveDirction = driveDirction
                    });
                if (!sc.Common.SCUtility.isEmpty(sec_id_2))
                    reserves.Add(new sc.ProtocolFormat.OHTMessage.ReserveInfo()
                    {
                        ReserveSectionID = sec_id_2,
                        DriveDirction = driveDirction
                    });
                bcApp.SCApplication.ReserveBLL.IsMultiReserveSuccess(bcApp.SCApplication, noticeCar.VEHICLE_ID, reserves);
            });

        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            dgv_cache_object_data.Refresh();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            dgv_cache_object_data.Refresh();
        }

        private void btn_reserve_clear_Click(object sender, EventArgs e)
        {
            string sec_id = cmb_reserve_section.Text;
            ASECTION section = bcApp.SCApplication.SectionBLL.cache.GetSection(sec_id);
            sc.ProtocolFormat.OHTMessage.DriveDirction driveDirction = radio_Forward.Checked ?
            sc.ProtocolFormat.OHTMessage.DriveDirction.DriveDirForward :
            sc.ProtocolFormat.OHTMessage.DriveDirction.DriveDirReverse;
            section.ClaerSectionReservation(driveDirction);

        }


        private void num_134_test_dis_ValueChanged(object sender, EventArgs e)
        {
            string vh_id = cmb_tcpipctr_Vehicle.Text;
            string sec_id = txt_134_test_section_id.Text;
            string drive = txt_134_test_section_id.Text;
            uint distance = (uint)num_134_test_dis.Value;
            double x = double.Parse(txt_134_test_X.Text);
            double y = double.Parse(txt_134_test_Y.Text);
            sc.ProtocolFormat.OHTMessage.DriveDirction driveDirction = rad_134_test_f.Checked ?
                sc.ProtocolFormat.OHTMessage.DriveDirction.DriveDirForward : sc.ProtocolFormat.OHTMessage.DriveDirction.DriveDirReverse;
            var section_obj = mainForm.BCApp.SCApplication.SectionBLL.cache.GetSection(sec_id);
            sc.ProtocolFormat.OHTMessage.ID_134_TRANS_EVENT_REP id_134_trans_event_rep = new sc.ProtocolFormat.OHTMessage.ID_134_TRANS_EVENT_REP()
            {
                //CurrentAdrID = section_obj == null ? "" : section_obj.TO_ADR_ID,
                CurrentAdrID = section_obj == null ? "" : section_obj.FROM_ADR_ID,
                CurrentSecID = sec_id,
                EventType = sc.ProtocolFormat.OHTMessage.EventType.AdrPass,
                SecDistance = distance,
                XAxis = x,
                YAxis = y
                //DrivingDirection = driveDirction
            };

            Task.Run(() =>
            {
                mainForm.BCApp.SCApplication.VehicleBLL.setAndPublishPositionReportInfo2Redis(vh_id, id_134_trans_event_rep);
            });
        }

        private void btn_Charger1ForceFinish_Click(object sender, EventArgs e)
        {

        }

        private void btn_Charger1ReadStatus_Click(object sender, EventArgs e)
        {
            //            AUNIT unit = bcApp.SCApplication.getEQObjCacheManager().getUnitByUnitID("Charger1");
            //            Task.Run(() =>
            //            {
            //                //AUNIT unit = bcApp.SCApplication.getEQObjCacheManager().getUnitByUnitID("Charger1");
            //                var mtl_mapaction = unit.
            //                    getMapActionByIdentityKey(nameof(com.mirle.ibg3k0.sc.Data.ValueDefMapAction.SubChargerValueDefMapAction)) as
            //                    com.mirle.ibg3k0.sc.Data.ValueDefMapAction.SubChargerValueDefMapAction;
            //                mtl_mapaction.ChargerCurrentStatus();

            //                Adapter.BeginInvoke(new SendOrPostCallback((o1) =>
            //                {
            //                    textBox_inputVoltage.Text = unit.inputVoltage.ToString();
            //                    textBox_chargeVoltage.Text = unit.chargeVoltage.ToString();
            //                    textBox_chargeCurrent.Text = unit.chargeCurrent.ToString();
            //                    textBox_chargePower.Text = unit.chargePower.ToString();
            //                    textBox_couplerChargeVoltage.Text = unit.couplerChargeVoltage.ToString();
            //                    textBox_couplerChargeCurrent.Text = unit.couplerChargeCurrent.ToString();
            //                    textBox_couplerID.Text = unit.couplerID.ToString();
            //                }), null);

            //            }
            //);

        }


        private void ck_CycleRunTest_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.IsCycleRun = ck_CycleRunTest.Checked;
        }

        //private void btn_CouplerStatusRefresh_Click(object sender, EventArgs e)
        //{
        //    refreshCouplerStatusUI();
        //}

        //private void refreshCouplerStatusUI()
        //{
        //    AUNIT unit1 = bcApp.SCApplication.getEQObjCacheManager().getUnit("MCharger", "Charger1");
        //    AUNIT unit2 = bcApp.SCApplication.getEQObjCacheManager().getUnit("MCharger", "Charger2");
        //    AUNIT unit3 = bcApp.SCApplication.getEQObjCacheManager().getUnit("MCharger", "Charger3");
        //    AUNIT unit4 = bcApp.SCApplication.getEQObjCacheManager().getUnit("MCharger", "Charger4");

        //    textBox_C11_Status.Text = unit1.coupler1Status.ToString();
        //    textBox_C12_Status.Text = unit1.coupler2Status.ToString();
        //    textBox_C13_Status.Text = unit1.coupler3Status.ToString();
        //    textBox_C21_Status.Text = unit2.coupler1Status.ToString();
        //    textBox_C22_Status.Text = unit2.coupler2Status.ToString();
        //    textBox_C23_Status.Text = unit2.coupler3Status.ToString();
        //    textBox_C31_Status.Text = unit3.coupler1Status.ToString();
        //    textBox_C32_Status.Text = unit3.coupler2Status.ToString();
        //    textBox_C33_Status.Text = unit3.coupler3Status.ToString();
        //    textBox_C41_Status.Text = unit4.coupler1Status.ToString();
        //    textBox_C42_Status.Text = unit4.coupler2Status.ToString();
        //    textBox_C43_Status.Text = unit4.coupler3Status.ToString();
        //}

        private void btn_online_Click(object sender, EventArgs e)
        {
            SCApplication scApp = SCApplication.getInstance();
            scApp.LineService.OnlineRemoteWithHost();
        }

        private void btnPortEnableSet_Click(object sender, EventArgs e)
        {

        }

        private void num_batteryCapacity_ValueChanged(object sender, EventArgs e)
        {
            DebugParameter.BatteryCapacity = (uint)num_batteryCapacity.Value;
        }

        private void btn_auto_remote_Click(object sender, EventArgs e)
        {
            asyExecuteAction(bcApp.SCApplication.VehicleService.changeVhStatusToAutoRemote);

        }

        private async void button10_Click_1(object sender, EventArgs e)
        {
            try
            {
                button10.Enabled = false;
            }
            catch { }
            finally
            {
                button10.Enabled = true;
            }
        }

        //private void num_BatteryLowBoundaryValue_ValueChanged(object sender, EventArgs e)
        //{
        //    AVEHICLE.SettingBatteryLevelLowBoundary((UInt16)num_BatteryLowBoundaryValue.Value);

        //}

        //private void num_BatteryHighBoundaryValue_ValueChanged(object sender, EventArgs e)
        //{
        //    AVEHICLE.SettingBatteryLevelHighBoundary((UInt16)num_BatteryHighBoundaryValue.Value);
        //}

        private void btn_refresf_portsation_info_Click(object sender, EventArgs e)
        {
            dgv_cache_object_data_portstation.Refresh();

        }

        private void num_cycle_run_interval_time_ValueChanged(object sender, EventArgs e)
        {
            DebugParameter.CycleRunIntervalTime = (int)num_cycle_run_interval_time.Value;
        }

        private void cb_reserve_reject_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.isForcedRejectReserve = cb_reserve_reject.Checked;
        }

        private async void btn_release_block_Click(object sender, EventArgs e)
        {
            try
            {
                btn_release_block.Enabled = false;
                await Task.Run(() => bcApp.SCApplication.LineService.RefreshCurrentVehicleReserveStatus());
                BCFApplication.onInfoMsg("Fource release current vehicle reserve status success");
            }
#pragma warning disable CS0168 // 已宣告變數 'ex'，但從未使用過它。
            catch (Exception ex)
#pragma warning restore CS0168 // 已宣告變數 'ex'，但從未使用過它。
            {

            }
            finally
            {
                btn_release_block.Enabled = true;
            }
        }

        private void numer_num_of_avoid_seg_ValueChanged(object sender, EventArgs e)
        {
            DebugParameter.NumberOfAvoidanceSegment = (int)numer_num_of_avoid_seg.Value;
        }

        private void cb_reserve_pass_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.isForcedPassReserve = cb_reserve_pass.Checked;
        }

        private void btn_loadArrivals_Click(object sender, EventArgs e)
        {
            var report_event = sc.ProtocolFormat.OHTMessage.EventType.LoadArrivals;
            McsReportEventTest(report_event);
        }

        private void McsReportEventTest(sc.ProtocolFormat.OHTMessage.EventType report_event,
            sc.ProtocolFormat.OHTMessage.BCRReadResult bCRReadResult = sc.ProtocolFormat.OHTMessage.BCRReadResult.BcrNormal)
        {
            string cst_id = txt_mcsReportTestCstID.Text;
            string cmd_id = txt_mcsReportTestCmdID.Text;
            AVEHICLE test_report_vh = bcApp.SCApplication.VehicleBLL.cache.getVehicle(cmb_mcsReportTestVHID.Text);
            var id_136 = new sc.ProtocolFormat.OHTMessage.ID_136_TRANS_EVENT_REP()
            {
                EventType = report_event,
                CSTID = cst_id,
                BCRReadResult = bCRReadResult,
                //CmdID = cmd_id
            };
            var bcfApp = bcApp.SCApplication.getBCFApplication();
            Task.Run(() =>
            {
                dynamic recive_processor = bcApp.SCApplication.VehicleService.Receive;
                //bcApp.SCApplication.VehicleService.Receive.TranEventReport(bcfApp, test_report_vh, id_136, 0);
                recive_processor.TranEventReport(bcfApp, test_report_vh, id_136, 0);
            });
        }

        private void btn_vhloading_Click(object sender, EventArgs e)
        {
            var report_event = sc.ProtocolFormat.OHTMessage.EventType.Vhloading;
            McsReportEventTest(report_event);
        }

        private void btn_loadComplete_Click(object sender, EventArgs e)
        {
            var report_event = sc.ProtocolFormat.OHTMessage.EventType.LoadComplete;
            McsReportEventTest(report_event);
        }

        private void btn_unloadArrivals_Click(object sender, EventArgs e)
        {
            var report_event = sc.ProtocolFormat.OHTMessage.EventType.UnloadArrivals;
            McsReportEventTest(report_event);
        }

        private void btn_vhunloading_Click(object sender, EventArgs e)
        {
            var report_event = sc.ProtocolFormat.OHTMessage.EventType.Vhunloading;
            McsReportEventTest(report_event);
        }

        private void btn_unloadComplete_Click(object sender, EventArgs e)
        {
            var report_event = sc.ProtocolFormat.OHTMessage.EventType.UnloadComplete;
            McsReportEventTest(report_event);
        }




        private void btn_id_bcr_read_Click(object sender, EventArgs e)
        {
            var report_event = sc.ProtocolFormat.OHTMessage.EventType.Bcrread;
            McsReportEventTest(report_event);
        }

        private void btn_bcrReadMismatch_Click(object sender, EventArgs e)
        {
            var report_event = sc.ProtocolFormat.OHTMessage.EventType.Bcrread;
            McsReportEventTest(report_event, sc.ProtocolFormat.OHTMessage.BCRReadResult.BcrMisMatch);

            //var completeStatus = sc.ProtocolFormat.OHTMessage.CompleteStatus.IdmisMatch;
            //McsCommandCompleteTest(completeStatus);
        }

        private void btn_bcrReadError_Click(object sender, EventArgs e)
        {
            var report_event = sc.ProtocolFormat.OHTMessage.EventType.Bcrread;
            McsReportEventTest(report_event, sc.ProtocolFormat.OHTMessage.BCRReadResult.BcrReadFail);
            //var completeStatus = sc.ProtocolFormat.OHTMessage.CompleteStatus.IdreadFailed;
            //McsCommandCompleteTest(completeStatus);
        }

        private void btn_cancel_cmp_Click(object sender, EventArgs e)
        {

        }
        private void btn_cancelFail_Click(object sender, EventArgs e)
        {

        }

        private void btn_interlockError_Click(object sender, EventArgs e)
        {
            var completeStatus = sc.ProtocolFormat.OHTMessage.CompleteStatus.CmpStatusInterlockError;

            McsCommandCompleteTest(completeStatus);
        }

        private void McsCommandCompleteTest(sc.ProtocolFormat.OHTMessage.CompleteStatus completeStatus)
        {
            string cmd_id = txt_mcsReportTestCmdID.Text;
            string cst_id = txt_mcsReportTestCstID.Text;
            AVEHICLE test_report_vh = bcApp.SCApplication.VehicleBLL.cache.getVehicle(cmb_mcsReportTestVHID.Text);
            var id_132 = new sc.ProtocolFormat.OHTMessage.ID_132_TRANS_COMPLETE_REPORT()
            {
                CmdID = cmd_id,
                CSTID = cst_id,
                CmpStatus = completeStatus,
                CurrentAdrID = sc.Common.SCUtility.Trim(test_report_vh.CUR_ADR_ID, true),
                CurrentSecID = sc.Common.SCUtility.Trim(test_report_vh.CUR_SEC_ID, true),
                //XAxis = test_report_vh.X_Axis,
                //YAxis = test_report_vh.Y_Axis
            };
            var bcfApp = bcApp.SCApplication.getBCFApplication();
            Task.Run(() => bcApp.SCApplication.VehicleService.Receive.CommandCompleteReport("", bcfApp, test_report_vh, id_132, 0));
        }

        private void btn_alarmtSet_Click(object sender, EventArgs e)
        {
            string error_code = "111018";
            //string error_code = "109008";
            var error_status = sc.ProtocolFormat.OHTMessage.ErrorStatus.ErrSet;
            AVEHICLE test_report_vh = bcApp.SCApplication.VehicleBLL.cache.getVehicle(cmb_mcsReportTestVHID.Text);
            Task.Run(() => bcApp.SCApplication.LineService.ProcessAlarmReport(test_report_vh, error_code, error_status, ""));
        }

        private void btn_alarmClear_Click(object sender, EventArgs e)
        {
            string error_code = "111018";
            //string error_code = "109008";
            var error_status = sc.ProtocolFormat.OHTMessage.ErrorStatus.ErrReset;
            AVEHICLE test_report_vh = bcApp.SCApplication.VehicleBLL.cache.getVehicle(cmb_mcsReportTestVHID.Text);
            Task.Run(() => bcApp.SCApplication.LineService.ProcessAlarmReport(test_report_vh, error_code, error_status, ""));
        }

        private void btn_cmpIdMismatch_Click(object sender, EventArgs e)
        {
            var completeStatus = sc.ProtocolFormat.OHTMessage.CompleteStatus.CmpStatusIdmisMatch;

            McsCommandCompleteTest(completeStatus);
        }

        private void btn_idReadError_Click(object sender, EventArgs e)
        {
            var completeStatus = sc.ProtocolFormat.OHTMessage.CompleteStatus.CmpStatusIdreadFailed;

            McsCommandCompleteTest(completeStatus);
        }

        private void btn_cmp_vh_abort_Click(object sender, EventArgs e)
        {
            var completeStatus = sc.ProtocolFormat.OHTMessage.CompleteStatus.CmpStatusAbort;

            McsCommandCompleteTest(completeStatus);
        }

        private void ch_reserve_stop_CheckedChanged(object sender, EventArgs e)
        {
            bcApp.SCApplication.VehicleService.Receive.ReserveStopTest(vh_id, ch_reserve_stop.Checked);
        }

        private void btn_auto_charge_Click(object sender, EventArgs e)
        {
            asyExecuteAction(bcApp.SCApplication.VehicleService.changeVhStatusToAutoMTL);
            //asyExecuteAction(bcApp.SCApplication.VehicleService.changeVhStatusToAutoCharging);
        }

        private void btn_auto_local_Click(object sender, EventArgs e)
        {
            asyExecuteAction(bcApp.SCApplication.VehicleService.changeVhStatusToAutoLocal);
        }

        private async void btn_changeToRemove_Click(object sender, EventArgs e)
        {
            try
            {
                //if (!noticeCar.IS_INSTALLED)
                //{
                //    MessageBox.Show($"{vh_id} is removed ready!");
                //    return;
                //}
                if (!noticeCar.IS_INSTALLED)
                {
                    MessageBox.Show($"{vh_id} is removed ready!");
                    //有可能車輛在沒有INSTALL的情況下上線又離線了，所以在已Remove的情況下，還是要做移除車輛的動作
                    await Task.Run(() => bcApp.SCApplication.VehicleService.Remove(vh_id, false));
                    return;
                }
                btn_changeToRemove.Enabled = false;
                (bool isSuccess, string result) check_result = default((bool isSuccess, string result));
                await Task.Run(() => check_result = bcApp.SCApplication.VehicleService.Remove(vh_id));
                if (check_result.isSuccess)
                {
                    MessageBox.Show($"{vh_id} remove ok");
                }
                else
                {
                    MessageBox.Show($"{vh_id} remove fail.{Environment.NewLine}" +
                                    $"result:{check_result.result}");
                }
                lbl_install_status.Text = noticeCar?.IS_INSTALLED.ToString();
            }
            finally
            {
                btn_changeToRemove.Enabled = true;
            }
        }

        private async void btn_changeToInstall_Click(object sender, EventArgs e)
        {
            try
            {
                if (noticeCar.IS_INSTALLED)
                {
                    MessageBox.Show($"{vh_id} is install ready!");
                    return;
                }

                btn_changeToInstall.Enabled = false;
                (bool isSuccess, string result) check_result = default((bool isSuccess, string result));
                await Task.Run(() => check_result = bcApp.SCApplication.VehicleService.Install(vh_id));
                if (check_result.isSuccess)
                {
                    MessageBox.Show($"{vh_id} install ok");
                }
                else
                {
                    MessageBox.Show($"{vh_id} install fail.{Environment.NewLine}" +
                                    $"result:{check_result.result}");
                }
                lbl_install_status.Text = noticeCar?.IS_INSTALLED.ToString();
            }
            finally
            {
                btn_changeToInstall.Enabled = true;
            }
        }

        int COUPLER_TEST_INDEX = 0;
        private void btn_coupler_enable_Click(object sender, EventArgs e)
        {
            COUPLER_TEST_INDEX++;
            if (bcApp.SCApplication.getBCFApplication().tryGetReadValueEventstring("Unit", "Charger1", "CHARGERX_TO_AGVC_CHARGER_STATUS_REPORT_COUPLER1_STATUS", out bcf.Controller.ValueRead vr_status))
            {
                vr_status.Value = new int[] { 1 };
            }
            if (bcApp.SCApplication.getBCFApplication().tryGetReadValueEventstring("Unit", "Charger1", "CHARGERX_TO_AGVC_CHARGER_STATUS_REPORT_INDEX", out bcf.Controller.ValueRead vr))
            {
                vr.Value = new int[] { COUPLER_TEST_INDEX };
            }

        }

        private void btn_coupler_error_Click(object sender, EventArgs e)
        {
            COUPLER_TEST_INDEX++;
            if (bcApp.SCApplication.getBCFApplication().tryGetReadValueEventstring("Unit", "Charger1", "CHARGERX_TO_AGVC_CHARGER_STATUS_REPORT_COUPLER1_STATUS", out bcf.Controller.ValueRead vr_status))
            {
                vr_status.Value = new int[] { 3 };
            }
            if (bcApp.SCApplication.getBCFApplication().tryGetReadValueEventstring("Unit", "Charger1", "CHARGERX_TO_AGVC_CHARGER_STATUS_REPORT_INDEX", out bcf.Controller.ValueRead vr))
            {
                vr.Value = new int[] { COUPLER_TEST_INDEX };
            }
        }

        private void btn_set_adr_Click(object sender, EventArgs e)
        {

        }

        private void btn_cmp_vh_complete_Click(object sender, EventArgs e)
        {
            var completeStatus = sc.ProtocolFormat.OHTMessage.CompleteStatus.CmpStatusLoadunload;

            McsCommandCompleteTest(completeStatus);
        }

        private void ck_CST_Status_Click(object sender, EventArgs e)
        {
            bcApp.SCApplication.VehicleService.Receive.CST_R_DisaplyTest(vh_id, ck_CST_Status_R.Checked);
        }

        private void ck_CST_Status_L_Click(object sender, EventArgs e)
        {
            bcApp.SCApplication.VehicleService.Receive.CST_L_DisaplyTest(vh_id, ck_CST_Status_L.Checked);
        }

        private void cb_cycle_run_type_SelectedIndexChanged(object sender, EventArgs e)
        {
            Enum.TryParse(cmb_cycle_run_type?.SelectedValue.ToString(), out DebugParameter.CycleRunTestType cycleRunType);
            DebugParameter.CycleRunType = cycleRunType;
        }

        private void ck_continue_bcrreadfail_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.isContinueByIDReadFail = ck_continue_bcrreadfail.Checked;
        }

        private void ck_test_retry_ReserveReq_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.testRetryReserveReq = ck_test_retry_ReserveReq.Checked;
        }

        private void cb_test_retry_LoadArrivals_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.testRetryLoadArrivals = cb_test_retry_LoadArrivals.Checked;
        }

        private void cb_test_retry_LoadComplete_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.testRetryLoadComplete = cb_test_retry_LoadComplete.Checked;
        }

        private void cb_test_retry_UnloadArrivals_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.testRetryUnloadArrivals = cb_test_retry_UnloadArrivals.Checked;
        }

        private void cb_test_retry_UnloadComplete_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.testRetryUnloadComplete = cb_test_retry_UnloadComplete.Checked;
        }

        private void cb_test_retry_Vhloading_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.testRetryVhloading = cb_test_retry_Vhloading.Checked;
        }

        private void cb_test_retry_Vhunloading_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.testRetryVhunloading = cb_test_retry_Vhunloading.Checked;
        }

        private void cb_test_retry_Bcrread_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.testRetryBcrread = cb_test_retry_Bcrread.Checked;
        }

        private void cb_test_command_over_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.CommandCheckForcePass = cb_test_command_over.Checked;
        }

        private void numer_commandWaitTime_ValueChanged(object sender, EventArgs e)
        {
            DebugParameter.CommandCompleteWaitTime = (int)numer_commandWaitTime.Value;
        }

        private void cb_canUnloadToAGVStation_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.CanUnloadToAGVStationTest = cb_canUnloadToAGVStation.Checked;
        }

        private void cbTranMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            Enum.TryParse(cbTranMode.SelectedValue.ToString(), out DebugParameter.TransferModeType transferModeType);
            DebugParameter.TransferMode = transferModeType;
        }

        private void btn_refresh_agvstationInfo_Click(object sender, EventArgs e)
        {
            dgv_AGVStationInfo.Refresh();
        }

        private void ck_check_port_is_ready_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.isNeedCheckPortReady = ck_check_port_is_ready.Checked;
        }

        private void numer_pre_open_agv_station_distance_ValueChanged(object sender, EventArgs e)
        {
            sc.App.SystemParameter.setOpenAGVStationCoverDistance((int)numer_pre_open_agv_station_distance.Value);
        }

        //private void cb_by_pass_shelf_status_CheckedChanged(object sender, EventArgs e)
        //{
        //    sc.App.SystemParameter.setIsByPassAGVShelfStatus(cb_by_pass_shelf_status.Checked);
        //}

        private void num_tran_cmd_queue_time_out_ms_ValueChanged(object sender, EventArgs e)
        {
            sc.App.SystemParameter.setTransferCommandQueueTimeOut_mSec((int)num_tran_cmd_queue_time_out_ms.Value);
        }

        private async void button11_Click_1(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                bcApp.SCApplication.LineService.ProcessAlarmReport("", "AGVC", "0", sc.ProtocolFormat.OHTMessage.ErrorStatus.ErrReset,
                            $"");
            });
        }

        private void lbl_listening_status_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && (ModifierKeys & Keys.Control) == Keys.Control)
            {
                btn_open_tcp_port.Visible = true;
                btn_close_tcp_port.Visible = true;
            }
        }

        private async void btn_open_tcp_port_Click(object sender, EventArgs e)
        {
            bool is_success = false;
            await Task.Run(() =>
            {
                is_success = bcApp.SCApplication.VehicleService.startVehicleTcpIpServer(vh_id);
            });
            MessageBox.Show(is_success ? "OK" : "NG");
        }

        private async void btn_close_tcp_port_Click(object sender, EventArgs e)
        {
            bool is_success = false;
            await Task.Run(() =>
            {
                is_success = bcApp.SCApplication.VehicleService.stopVehicleTcpIpServer(vh_id);
            });
            MessageBox.Show(is_success ? "OK" : "NG");
        }

        private void cb_passCouplerStatus_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.isPassCouplerStatus = cb_passCouplerStatus.Checked;
        }

        private void cb_passCouplerHPSafetySingnal_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.isPassCouplerHPSafetySignal = cb_passCouplerHPSafetySingnal.Checked;
        }

        private void num_vh_idle_time_ValueChanged(object sender, EventArgs e)
        {
            sc.App.SystemParameter.setAllowIdleTime_ms((int)num_vh_idle_time.Value);
        }

        private void cb_needCheckPortUpdateTime_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.isNeedCheckPortUpDateTime = cb_needCheckPortUpdateTime.Checked;
        }

        private void cb_reserve_pass_agv0609_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.isForcedPassReserve_AGV0609 = cb_reserve_pass_agv0609.Checked;
        }

        private async void vhType_Update_Click(object sender, EventArgs e)
        {
            try
            {
                E_VH_TYPE vh_type;
                Enum.TryParse(cmbVhType.SelectedValue.ToString(), out vh_type);
                btnVhTypeUpdate.Enabled = false;
                await Task.Run(() => bcApp.SCApplication.VehicleService.updateVhType(vh_id, vh_type));
                cmbVhType.SelectedItem = noticeCar.VEHICLE_TYPE;
            }
            finally
            {
                btnVhTypeUpdate.Enabled = true;
            }
        }

        private async void btnUpdateUnloadVhType_Click(object sender, EventArgs e)
        {
            try
            {
                string port_id = cmbStationPortID.Text;
                E_VH_TYPE vh_type;
                Enum.TryParse(cmbUnloadVhType.SelectedValue.ToString(), out vh_type);
                btnUpdateUnloadVhType.Enabled = false;
                await Task.Run(() => bcApp.SCApplication.PortStationService.doUpdatePortUnloadVhType(port_id, vh_type));
            }
            finally
            {
                btnUpdateUnloadVhType.Enabled = true;
            }
        }

        private void dgv_AGVStationInfo_SelectionChanged(object sender, EventArgs e)
        {

        }

        private void cmbStationPortID_SelectedIndexChanged(object sender, EventArgs e)
        {
            string port_id = cmbStationPortID.Text;
            var port_st_obj = bcApp.SCApplication.PortStationBLL.OperateCatch.getPortStation(port_id);
            cmbUnloadVhType.SelectedItem = port_st_obj.ULD_VH_TYPE;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            lbl_guideTimes.Text = DebugParameter.GuideSearchTimes.ToString();
            lbl_guideQuickTimes.Text = DebugParameter.GuideQuickSearchTimes.ToString();
        }

        private void btn_CarrierRead_Click(object sender, EventArgs e)
        {
            string cst_id = txt_mcsReportTestCstID.Text.Trim();
            string Port_id = cmb_mcsReportTestPortID.Text.Trim();
            ACARRIER cst = new ACARRIER();
            cst.ID = cst_id;
            cst.LOCATION = Port_id;
            Task.Run(() => bcApp.SCApplication.ReportBLL.ReportCarrierIDRead(cst, "0", null));
        }

        private void btn_CarrierRemove_Click(object sender, EventArgs e)
        {
            string cst_id = txt_mcsReportTestCstID.Text.Trim();
            //string Port_id = cmb_mcsReportTestPortID.Text.Trim();
            ACARRIER cst = new ACARRIER();
            cst.ID = cst_id;


            //cst.LOCATION = Port_id;
            Task.Run(() => bcApp.SCApplication.ReportBLL.ReportCarrierRemovedFromPort(cst, "2", null));
        }

        private void btn_WaitIn_Click(object sender, EventArgs e)
        {
            string cst_id = txt_mcsReportTestCstID.Text.Trim();
            string Port_id = cmb_mcsReportTestPortID.Text.Trim();
            ACARRIER cst = new ACARRIER();
            cst.ID = cst_id;
            cst.LOCATION = Port_id;
            Task.Run(() => bcApp.SCApplication.ReportBLL.ReportCarrierWaitIn(cst, null));
        }

        private void btn_WaitOut_Click(object sender, EventArgs e)
        {
            string cst_id = txt_mcsReportTestCstID.Text.Trim();
            string Port_id = cmb_mcsReportTestPortID.Text.Trim();
            ACARRIER cst = new ACARRIER();
            cst.ID = cst_id;
            cst.LOCATION = Port_id;
            Task.Run(() => bcApp.SCApplication.ReportBLL.ReportCarrierWaitOut(cst, null));
        }

        private void btn_CarrierRemoveComplete_Click(object sender, EventArgs e)
        {
            string cst_id = txt_mcsReportTestCstID.Text.Trim();
            //string Port_id = cmb_mcsReportTestPortID.Text.Trim();
            //ACARRIER cst = new ACARRIER();
            //cst.ID = cst_id;
            //cst.LOCATION = Port_id;
            Task.Run(() => bcApp.SCApplication.ReportBLL.ReportCarrierRemovedCompleted(cst_id, null));
        }

        private void btn_LoadRequest_Click(object sender, EventArgs e)
        {
            //string cst_id = txt_mcsReportTestCstID.Text.Trim();
            string Port_id = cmb_mcsReportTestPortID.Text.Trim();
            //ACARRIER cst = new ACARRIER();
            //cst.ID = cst_id;
            //cst.LOCATION = Port_id;
            Task.Run(() => bcApp.SCApplication.ReportBLL.ReportLoadReq(Port_id, null));
        }

        private void btn_UnloadRequest_Click(object sender, EventArgs e)
        {
            //string cst_id = txt_mcsReportTestCstID.Text.Trim();
            string Port_id = cmb_mcsReportTestPortID.Text.Trim();
            //ACARRIER cst = new ACARRIER();
            //cst.ID = cst_id;
            //cst.LOCATION = Port_id;
            Task.Run(() => bcApp.SCApplication.ReportBLL.ReportUnLoadReq(Port_id, null));
        }

        private void button14_Click(object sender, EventArgs e)
        {
            string port_id = cb_PortID.Text;
            Task.Run(() =>
            {
                portOutMode(port_id);
            });
        }

        private void button13_Click(object sender, EventArgs e)
        {
            string port_id = cb_PortID.Text;
            Task.Run(() =>
            {
                portInMode(port_id);
            });
        }


        private void portOutMode(string port_id)
        {

            List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
            bcApp.SCApplication.ReportBLL.newReportPortModeChange(port_id, reportqueues);
            bcApp.SCApplication.ReportBLL.newReportPortOutMode(port_id, reportqueues);
            bcApp.SCApplication.ReportBLL.newSendMCSMessage(reportqueues);

        }
        private void portInMode(string port_id)
        {
            List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
            bcApp.SCApplication.ReportBLL.newReportPortModeChange(port_id, reportqueues);
            bcApp.SCApplication.ReportBLL.newReportPortInMode(port_id, reportqueues);
            bcApp.SCApplication.ReportBLL.newSendMCSMessage(reportqueues);
        }

        private void btn_NoRequest_Click(object sender, EventArgs e)
        {
            //string cst_id = txt_mcsReportTestCstID.Text.Trim();
            string Port_id = cmb_mcsReportTestPortID.Text.Trim();
            //ACARRIER cst = new ACARRIER();
            //cst.ID = cst_id;
            //cst.LOCATION = Port_id;
            Task.Run(() => bcApp.SCApplication.ReportBLL.ReportNoReq(Port_id, null));
        }

        private void btn_CarrierInstallCompleted_Click(object sender, EventArgs e)
        {
            string cst_id = txt_mcsReportTestCstID.Text.Trim();
            ACARRIER cst = new ACARRIER();
            cst.ID = cst_id;
            cst.LOCATION = noticeCar.Real_ID;
            cst.CSTType = "0";
            cst.LOT_ID = "LOT01";
            Task.Run(() => bcApp.SCApplication.ReportBLL.ReportCarrierInstallCompleted(cst, null));

        }

        private void btn_CarrierReadFailed_Click(object sender, EventArgs e)
        {
            string cst_id = txt_mcsReportTestCstID.Text.Trim();
            string Port_id = cmb_mcsReportTestPortID.Text.Trim();
            ACARRIER cst = new ACARRIER();
            cst.ID = cst_id;
            cst.LOCATION = Port_id;
            Task.Run(() => bcApp.SCApplication.ReportBLL.ReportCarrierIDRead(cst, "1", null));
        }

        private void EQReportRemoveCB_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.isReportEQPortCarrierRemove = EQReportRemoveCB.Checked;
        }

        #region MTL
        private void ForceBypassMTLPauseCheckCB_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.isForceBypassMTLPauseCheck = ForceBypassMTLPauseCheckCB.Checked;
            AEQPT eqpt = bcApp.SCApplication.getEQObjCacheManager().getEquipmentByEQPTID("MTL");
            if (eqpt != null)
            {
                MaintainLift mtl = eqpt as MaintainLift;
                if (mtl != null)
                {
                    Task.Run(() => bcApp.SCApplication.VehicleService.checkThenSetVehiclePauseByMTLStatus("Debug"));
                }
            }
        }


        private void registerMTLEvent()
        {
            //AEQPT eqpt = bcApp.SCApplication.getEQObjCacheManager().getEquipmentByEQPTID("MTL");
            //if (eqpt != null)
            //{
            //    MaintainLift MTL = eqpt as MaintainLift;
            if (MTL != null)
            {
                MTL.addEventHandler("MTLhandler", BCFUtility.getPropertyName(() => MTL.Eq_Alive_Index),
                (s1, e1) => refreshMTLInfo());
                MTL.addEventHandler("MTLhandler", BCFUtility.getPropertyName(() => MTL.MTxMode),
                (s1, e1) => refreshMTLInfo());
                MTL.addEventHandler("MTLhandler", BCFUtility.getPropertyName(() => MTL.StopSignal),
                (s1, e1) => refreshMTLInfo());
                MTL.addEventHandler("MTLhandler", BCFUtility.getPropertyName(() => MTL.MTLLocation),
                (s1, e1) => refreshMTLInfo());
            }
            //}
        }

        private void refreshMTLInfo()
        {
            //AEQPT eqpt = bcApp.SCApplication.getEQObjCacheManager().getEquipmentByEQPTID("MTL");
            //if (eqpt != null)
            //{
            //    MaintainLift MTL = eqpt as MaintainLift;
            Adapter.BeginInvoke(new SendOrPostCallback((o1) =>
            {
                if (MTL != null)
                {
                    MTL_Alive_tb.Text = MTL.Eq_Alive_Index.ToString();
                    MTL_Mode_tb.Text = MTL.MTxMode.ToString();
                    MTL_LFT_LOC_tb.Text = MTL.MTLLocation.ToString();
                    MTL_Error_tb.Text = MTL.StopSignal.ToString();
                }
            }), null);

            //}
        }
        #endregion MTL

        private void button15_Click(object sender, EventArgs e)
        {
            bcApp.SCApplication.VehicleService.Avoid.findNotConflictAvoidAdr(textBox2.Text, textBox1.Text);
        }

        private void button16_Click(object sender, EventArgs e)
        {
            sc.App.SystemParameter.setPassAxisDistance(double.Parse(textBox3.Text));

        }

        private void onlineremote_rbn_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.isDefaultOHTModeAutoRemote = true;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.isDefaultOHTModeAutoRemote = false;
        }

        private void ForceBypassVehicleConnectionPauseCheckCB_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.isForceBypassVehclePauseCheck = ForceBypassVehicleConnectionPauseCheckCB.Checked;
            //AEQPT eqpt = bcApp.SCApplication.getEQObjCacheManager().getEquipmentByEQPTID("MTL");
            //if (eqpt != null)
            //{
            //    MaintainLift mtl = eqpt as MaintainLift;
            //    if (mtl != null)
            //    {
            //        MTLValueDefMapAction mtlMapAction = mtl.getMapActionByIdentityKey(typeof(MTLValueDefMapAction).Name) as MTLValueDefMapAction;
            //        if (mtlMapAction != null)
            //        {
            //            Task.Run(() => mtlMapAction.checkThenSetVehiclePauseByMTLStatus("Debug"));
            //        }
            //    }
            //}
        }
    }
}
