using com.mirle.ibg3k0.bc.winform;
using com.mirle.ibg3k0.bc.winform.App;
using com.mirle.ibg3k0.bc.winform.Common;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.sc;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.Data.VO.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com.mirle.ibg3k0.bc.winform.UI
{
    public partial class MaintainDeviceForm : Form
    {
        BCMainForm mainForm;
        BCApplication bcApp;

        MaintainLift MTL = null;
        //MaintainSpace MTS = null;
        MTxValueDefMapActionBase MTLValueDefMapActionBase = null;
        MTxValueDefMapActionBase MTSValueDefMapActionBase = null;
        AVEHICLE preCarOut = null;

        public MaintainDeviceForm(BCMainForm _mainForm)
        {
            InitializeComponent();
            mainForm = _mainForm;
            bcApp = mainForm.BCApp;
            timer1.Enabled = true;

        }
        private void cmb_mtl_SelectedIndexChanged(object sender, EventArgs e)
        {
            string device_id = (sender as ComboBox).Text;
            MTL = bcApp.SCApplication.getEQObjCacheManager().getEquipmentByEQPTID(device_id) as MaintainLift;
            MTLValueDefMapActionBase = MTL.getMapActionByIdentityKey(nameof(MTLValueDefMapAction)) as MTxValueDefMapActionBase;
            //if (MTLValueDefMapActionBase == null)
            //{
            //    MTLValueDefMapActionBase = MTL.getMapActionByIdentityKey(nameof(MTLValueDefMapAction)) as MTxValueDefMapActionBase;
            //}
        }

        private void cmb_mts_SelectedIndexChanged(object sender, EventArgs e)
        {
            //string device_id = (sender as ComboBox).Text;
            //MTS = bcApp.SCApplication.getEQObjCacheManager().getEquipmentByEQPTID(device_id) as MaintainSpace;
            //MTSValueDefMapActionBase = MTS.getMapActionByIdentityKey(nameof(MTSValueDefMapActionNew)) as MTxValueDefMapActionBase;

            //if (MTSValueDefMapActionBase == null)
            //{
            //    MTSValueDefMapActionBase = MTS.getMapActionByIdentityKey(nameof(MTSValueDefMapActionNewPH2)) as MTxValueDefMapActionBase;
            //}
        }

        private void btn_mtl_dateTimeSync_Click(object sender, EventArgs e)
        {
            DateTimeSyncCommand(MTLValueDefMapActionBase);
        }
        private void btn_mts_dateTimeSync_Click(object sender, EventArgs e)
        {
            DateTimeSyncCommand(MTSValueDefMapActionBase);
        }
        private void DateTimeSyncCommand(MTxValueDefMapActionBase mTxValueDefMapActionBase)
        {
            Task.Run(() =>
            {
                mTxValueDefMapActionBase.DateTimeSyncCommand(DateTime.Now);
            });
        }

        private void btn_mtl_car_out_notify_Click(object sender, EventArgs e)
        {
            UInt16 car_id = UInt16.Parse(txt_mtl_car_out_notify_car_id.Text);
            CarOutNotify(MTLValueDefMapActionBase, car_id, 2);
        }



        private void CarOutNotify(MTxValueDefMapActionBase mTxValueDefMapActionBase, ushort carNum,ushort action_type)
        {
            Task.Run(() =>
            {
                mTxValueDefMapActionBase.OHxC_CarOutNotify(carNum, action_type);
            });
        }

        bool mtlcaroutexcuting = false;
        bool MTLCarOutExcuting
        {
            set
            {
                if (mtlcaroutexcuting != value)
                {
                    mtlcaroutexcuting = value;
                    if (mtlcaroutexcuting)
                    {
                        btn_mtlcarOutTest.Enabled = false;
                        mtl_prepare_car_out_info.Enabled = false;
                        btn_mtl_cauout_cancel.Enabled = true;
                    }
                    else
                    {
                        btn_mtlcarOutTest.Enabled = true;
                        mtl_prepare_car_out_info.Enabled = true;
                        btn_mtl_cauout_cancel.Enabled = false;
                    }
                }
            }
            get
            {
                return mtlcaroutexcuting;
            }
        }

        private async void btn_mtlcarOutTest_Click(object sender, EventArgs e)
        {
            try
            {
                string pre_car_out_vh = cmb_mtl_car_out_vh.Text;
                btn_mtlcarOutTest.Enabled = false;
                var r = default((bool isSuccess, string result));
                await Task.Run(() => r = AutoCarOut(MTL, pre_car_out_vh));

                MessageBox.Show(r.result);
            }
            finally
            {
                btn_mtlcarOutTest.Enabled = true;
            }
        }

        private (bool isSuccess, string result) AutoCarOut(IMaintainDevice maintainDevice, string preCarOutVhID)
        {
            var r = default((bool isSuccess, string result));
            try
            {
                AVEHICLE pre_car_out_vh = bcApp.SCApplication.VehicleBLL.cache.getVehicle(preCarOutVhID);
                string Handler = this.Name;
                preCarOut = bcApp.SCApplication.VehicleBLL.cache.getVehicle(preCarOutVhID);
                preCarOut.addEventHandler(Handler, BCFUtility.getPropertyName(() => preCarOut.IsReadyForCarOut),
                    (s1, e1) => isReadyForCarOutChange(preCarOut));

                if (maintainDevice is sc.Data.VO.MaintainLift)
                {

                    r = bcApp.SCApplication.MTLService.checkVhAndMTxCarOutStatus(maintainDevice, null, pre_car_out_vh);

                    if (r.isSuccess)
                    {
                        r = bcApp.SCApplication.MTLService.CarOurRequest(maintainDevice, pre_car_out_vh);
                    }
                    //if (!SpinWait.SpinUntil(() => maintainDevice.CarOutSafetyCheck == true &&
                    ////maintainDevice.CarOutActionTypeSystemOutToMTL == true && 
                    //( dockingMTS==null||dockingMTS.CarOutSafetyCheck == true), 60000))
                    //{
                    //    r.isSuccess = false;
                    //    string  result = $"Process car out scenario,but mtl:{maintainDevice.DeviceID} status not ready " +
                    //    $"{nameof(maintainDevice.CarOutSafetyCheck)}:{maintainDevice.CarOutSafetyCheck}";
                    //    MessageBox.Show(result);
                    //}
                    if (r.isSuccess)
                    {
                        r = bcApp.SCApplication.MTLService.processCarOutScenario(maintainDevice as sc.Data.VO.MaintainLift, pre_car_out_vh);
                    }
                }
                //else if (maintainDevice is sc.Data.VO.MaintainSpace)
                //{
                //    r = bcApp.SCApplication.MTLService.checkVhAndMTxCarOutStatus(maintainDevice, null, pre_car_out_vh);
                //    if (r.isSuccess)
                //    {
                //        r = bcApp.SCApplication.MTLService.CarOurRequest(maintainDevice, pre_car_out_vh);
                //    }
                //    //if (!SpinWait.SpinUntil(() => maintainDevice.CarOutSafetyCheck == true, 30000))
                //    //{
                //    //    r.isSuccess = false;
                //    //    string result = $"Process car out scenario,but mtl:{maintainDevice.DeviceID} status not ready " +
                //    //    $"{nameof(maintainDevice.CarOutSafetyCheck)}:{maintainDevice.CarOutSafetyCheck}";
                //    //    MessageBox.Show(result);
                //    //}

                //    if (r.isSuccess)
                //    {
                //        r = bcApp.SCApplication.MTLService.processCarOutScenario(maintainDevice as sc.Data.VO.MaintainSpace, pre_car_out_vh);
                //    }
                //}
            }
            catch (Exception ex)
            {
                r = (false, ex.ToString());
            }
            finally
            {
            }
            return r;
        }

        private void isReadyForCarOutChange(AVEHICLE preCarOut)
        {
            try
            {
                if (preCarOut.IsReadyForCarOut)
                {
                    btn_mtl_cauout_cancel.Enabled = false;
                }
                else
                {
                    btn_mtl_cauout_cancel.Enabled = true;
                    this.preCarOut = null;
                }
            }
            finally
            {

            }
        }

        private void btn_mtl_cauout_cancel_Click(object sender, EventArgs e)
        {
            try
            {
                btn_mtl_cauout_cancel.Enabled = false;
                AutoCarOutCancel(MTL);
            }
            finally
            {
                btn_mtl_cauout_cancel.Enabled = true;
            }
        }



        private Task AutoCarOutCancel(IMaintainDevice maintainDevice)
        {
            return Task.Run(() => bcApp.SCApplication.MTLService.carOutRequestCancel(maintainDevice));
        }

        private void btn_mtl_vh_realtime_info_Click(object sender, EventArgs e)
        {
            UInt16 car_id = UInt16.Parse(txt_mtl_car_id.Text);
            UInt16 action_mode = UInt16.Parse(txt_mtl_action_mode.Text);
            UInt16 cst_exist = UInt16.Parse(txt_mtl_cst_exist.Text);
            UInt16 current_section_id = UInt16.Parse(txt_mtl_current_sec_id.Text);
            UInt32 current_address_id = UInt32.Parse(txt_mtl_current_adr_id.Text);
            UInt32 buffer_distance = UInt32.Parse(txt_mtl_buffer_distance.Text);
            UInt16 speed = UInt16.Parse(txt_mtl_speed.Text);
            Task.Run(() =>
            {
                CarRealtimeInfo(MTL, car_id, action_mode, cst_exist, current_section_id, current_address_id, buffer_distance, speed);
            }
            );
        }

        private void btn_mts_vh_realtime_info_Click(object sender, EventArgs e)
        {
            //UInt16 car_id = UInt16.Parse(txt_mts_car_id.Text);
            //UInt16 action_mode = UInt16.Parse(txt_mts_action_mode.Text);
            //UInt16 cst_exist = UInt16.Parse(txt_mts_cst_exist.Text);
            //UInt16 current_section_id = UInt16.Parse(txt_mts_current_sec_id.Text);
            //UInt32 current_address_id = UInt32.Parse(txt_mts_current_adr_id.Text);
            //UInt32 buffer_distance = UInt32.Parse(txt_mts_buffer_distance.Text);
            //UInt16 speed = UInt16.Parse(txt_mts_speed.Text);
            //Task.Run(() =>
            //{
            //    CarRealtimeInfo(MTS, car_id, action_mode, cst_exist, current_section_id, current_address_id, buffer_distance, speed);
            //}
            //);
        }

        private void CarRealtimeInfo(IMaintainDevice maintainDevice, UInt16 car_id, UInt16 action_mode, UInt16 cst_exist, UInt16 current_section_id, UInt32 current_address_id,
                                            UInt32 buffer_distance, UInt16 speed)
        {
            maintainDevice.setCarRealTimeInfo(car_id, action_mode, cst_exist, current_section_id, current_address_id, buffer_distance, speed);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            lbl_mtl_alive.Text = MTL.Is_Eq_Alive.ToString();
            lbl_mtl_current_car_id.Text = MTL.CurrentCarID;
            lbl_mtl_has_vh.Text = MTL.HasVehicle.ToString();
            lbl_mtl_stop_single.Text = MTL.StopSignal.ToString();
            lbl_mtl_mode.Text = MTL.MTxMode.ToString();
            lbl_mtl_location.Text = MTL.MTLLocation.ToString();
            lbl_mtl_moving_status.Text = MTL.MTLMovingStatus.ToString();
            lbl_mtl_encoder.Text = MTL.Encoder.ToString();
            lbl_mtl_rail_status.Text = MTL.MTLRailStatus.ToString();


            //MTL Car Out Interface
            btn_mtl_m2o_u2d_safetycheck.Checked = MTL.MTLCarOutSafetyCheck;
            btn_mtl_car_out_interlock.Checked = MTL.MTLCarOutInterlock;
            //MTL Car In Interface
            btn_mtl_m2o_d2u_safetycheck.Checked = MTL.MTLCarInSafetyCheck;
            btn_mtl_car_in_interlock.Checked = MTL.MTLCarInInterlock;

            //OHTC Car Out Interface
            btn_mtl_o2m_u2d_caroutInterlock.Checked = MTL.OHTCCarOutInterlock;
            btn_mtl_o2m_caroutready.Checked = MTL.OHTCCarOutReady;
            btn_mtl_o2m_caroutMoving.Checked = MTL.OHTCCarOutMoving;
            btn_mtl_o2m_caroutmovecomplete.Checked = MTL.OHTCCarOutMoveComplete;

            //OHTC Car In Interface
            btn_mtl_o2m_car_in_interlock.Checked = MTL.OHTCCarInInterlock;
            btn_mtl_o2m_d2u_moving.Checked = MTL.OHTCCarInMoving;
            btn_mtl_o2m_car_in_move_complete.Checked = MTL.OHTCCarInMoveComplete;



            if (!mtl_prepare_car_out_info.Enabled)
            {
                txt_mtl_car_id.Text = MTL.CurrentPreCarOurID.ToString();
                txt_mtl_action_mode.Text = MTL.CurrentPreCarOurActionMode.ToString();
                txt_mtl_cst_exist.Text = MTL.CurrentPreCarOurCSTExist.ToString();
                txt_mtl_current_sec_id.Text = MTL.CurrentPreCarOurSectionID.ToString();
                txt_mtl_current_adr_id.Text = MTL.CurrentPreCarOurAddressID.ToString();
                txt_mtl_buffer_distance.Text = MTL.CurrentPreCarOurDistance.ToString();
                txt_mtl_speed.Text = MTL.CurrentPreCarOurSpeed.ToString();
                //btn_mtl_o2m_u2d_caroutInterlock.Checked = MTL.CarOutInterlock;
                //btn_mtl_o2m_d2u_moving.Checked = MTL.CarInMoving;
            }
            //if (!mts_prepare_car_out_info.Enabled)
            //{
            //    txt_mts_car_id.Text = MTS.CurrentPreCarOurID.ToString();
            //    txt_mts_action_mode.Text = MTS.CurrentPreCarOurActionMode.ToString();
            //    txt_mts_cst_exist.Text = MTS.CurrentPreCarOurCSTExist.ToString();
            //    txt_mts_current_sec_id.Text = MTS.CurrentPreCarOurSectionID.ToString();
            //    txt_mts_current_adr_id.Text = MTS.CurrentPreCarOurAddressID.ToString();
            //    txt_mts_buffer_distance.Text = MTS.CurrentPreCarOurDistance.ToString();
            //    txt_mts_speed.Text = MTS.CurrentPreCarOurSpeed.ToString();
            //    //btn_mts_o2m_u2d_caroutInterlock.Checked = MTS.CarOutInterlock;
            //    //btn_mts_o2m_d2u_moving.Checked = MTS.CarInMoving;
            //}
            MTLCarOutExcuting = !SCUtility.isEmpty(MTL.PreCarOutVhID);
            //MTSCarOutExcuting = !SCUtility.isEmpty(MTS.PreCarOutVhID);


        }

        private void MaintainDeviceForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            mainForm.removeForm(this.Name);
            timer1.Enabled = false;
        }

        private void MaintainDeviceForm_Load(object sender, EventArgs e)
        {
            List<AEQPT> maintainDevices = bcApp.SCApplication.EqptBLL.OperateCatch.loadMaintainLift();
            string[] maintain_Lift_id = maintainDevices.Select(eq => eq.EQPT_ID).ToArray();
            BCUtility.setComboboxDataSource(cmb_mtl, maintain_Lift_id.ToArray());
            //maintainDevices = bcApp.SCApplication.EquipmentBLL.cache.loadMaintainSpace();
            //string[] maintain_Space_id = maintainDevices.Select(eq => eq.EQPT_ID).ToArray();
            //BCUtility.setComboboxDataSource(cmb_mts, maintain_Space_id.ToArray());

            List<AVEHICLE> vhs = bcApp.SCApplication.VehicleBLL.cache.loadAllVh();
            string[] vh_ids = vhs.Select(vh => vh.VEHICLE_ID).ToArray();
            BCUtility.setComboboxDataSource(cmb_mtl_car_out_vh, vh_ids.ToArray());
        }

        private void btn_mtl_o2m_u2d_caroutInterlock_Click(object sender, EventArgs e)
        {

        }

        private void btn_mts_o2m_u2d_caroutInterlock_Click(object sender, EventArgs e)
        {

        }


        private void btn_mts_o2m_d2u_moving_Click(object sender, EventArgs e)
        {

        }

        private void btn_mtl_o2m_d2u_moving_Click(object sender, EventArgs e)
        {

        }

        private async void btn_mtl_alarm_reset_Click(object sender, EventArgs e)
        {
            await Task.Run(() => MTLValueDefMapActionBase.OHxC_AlarmResetRequest());
        }

        private async void btn_mts_alarm_reset_Click(object sender, EventArgs e)
        {
            await Task.Run(() => MTSValueDefMapActionBase.OHxC_AlarmResetRequest());
        }

        private async void btn_mtl_car_out_interlock_on_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                MTLValueDefMapActionBase.setOHxC2MTL_CarOutInterlock(true);
            });
        }

        private async void btn_mtl_car_out_interlock_off_Click(object sender, EventArgs e)
        {
            await Task.Run(() => MTLValueDefMapActionBase.setOHxC2MTL_CarOutInterlock(false));
        }

        private async void btn_mtl_car_in_interlock_on_Click(object sender, EventArgs e)
        {
            //MTLValueDefMapActionBase.setOHxC2MTL_CarInMoving(set_true_flase);
            await Task.Run(() => MTLValueDefMapActionBase.setOHxC2MTL_CarInMoving(true));

        }

        private async void btn_mtl_car_in_interlock_off_Click(object sender, EventArgs e)
        {
            await Task.Run(() => MTLValueDefMapActionBase.setOHxC2MTL_CarInMoving(false));
        }

        private async void btn_mts_car_out_interlock_on_Click(object sender, EventArgs e)
        {
            await Task.Run(() => MTSValueDefMapActionBase.setOHxC2MTL_CarOutInterlock(true));
        }

        private async void btn_mts_car_out_interlock_off_Click(object sender, EventArgs e)
        {
            await Task.Run(() => MTSValueDefMapActionBase.setOHxC2MTL_CarOutInterlock(false));
        }

        private async void btn_mts_car_in_interlock_on_Click(object sender, EventArgs e)
        {
            await Task.Run(() => MTSValueDefMapActionBase.setOHxC2MTL_CarInMoving(true));
        }

        private async void btn_mts_car_in_interlock_off_Click(object sender, EventArgs e)
        {
            await Task.Run(() => MTSValueDefMapActionBase.setOHxC2MTL_CarInMoving(false));
        }

        private async void btn_mtl_message_download_Click(object sender, EventArgs e)
        {
            string msg = txt_mtlMessage.Text;
            await Task.Run(() => MTLValueDefMapActionBase.OHxCMessageDownload(msg));
        }

        private async void btn_reset_handshake_Click(object sender, EventArgs e)
        {
            string msg = txt_mtlMessage.Text;
            await Task.Run(() => MTLValueDefMapActionBase.OHxCResetAllhandshake());
        }

        private async void btn_mtl_car_out_ready_on_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                MTLValueDefMapActionBase.setOHxC2MTL_CarOutReady(true);
            });
        }

        private async void btn_mtl_car_out_ready_off_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                MTLValueDefMapActionBase.setOHxC2MTL_CarOutReady(false);
            });
        }

        private async void btn_mtl_car_out_moving_on_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                MTLValueDefMapActionBase.setOHxC2MTL_CarOutMoving(true);
            });
        }

        private async void btn_mtl_car_out_moving_off_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                MTLValueDefMapActionBase.setOHxC2MTL_CarOutMoving(false);
            });
        }

        private async void btn_mtl_car_out_move_complete_on_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                MTLValueDefMapActionBase.setOHxC2MTL_CarOutMoveComplete(true);
            });
        }

        private async void btn_mtl_car_out_move_complete_off_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                MTLValueDefMapActionBase.setOHxC2MTL_CarOutMoveComplete(false);
            });
        }

        private async void btn_ohtc_car_in_interlock_on_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                MTLValueDefMapActionBase.setOHxC2MTL_CarInInterlock(true);
            });
        }

        private async void btn_ohtc_car_in_interlock_off_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                MTLValueDefMapActionBase.setOHxC2MTL_CarInInterlock(false);
            });
        }

        private async void btn_ohtc_car_in_move_complete_on_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                MTLValueDefMapActionBase.setOHxC2MTL_CarInMoveComplete(true);
            });
        }

        private async void btn_ohtc_car_in_move_complete_off_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                MTLValueDefMapActionBase.setOHxC2MTL_CarInMoveComplete(false);
            });
        }
    }
}
