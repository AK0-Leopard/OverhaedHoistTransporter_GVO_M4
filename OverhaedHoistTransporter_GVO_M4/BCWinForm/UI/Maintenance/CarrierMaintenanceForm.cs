using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.sc;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using NLog;
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

namespace com.mirle.ibg3k0.bc.winform.UI
{
    public partial class CarrierMaintenanceForm : Form
    {
        BCMainForm MainForm;
        SCApplication scApp;
        const int DGV_CURRENT_IN_LINE_CARRIER_INFO_INDEX_ID = 0;
        const int DGV_CURRENT_IN_LINE_CARRIER_INFO_INDEX_LOCATION = 1;
        const int DGV_CURRENT_IN_LINE_CARRIER_INFO_INDEX_DEST = 2;
        const int DGV_CURRENT_IN_LINE_CARRIER_INFO_INDEX_STATE = 3;


        public CarrierMaintenanceForm(BCMainForm _mainForm)
        {
            InitializeComponent();
            MainForm = _mainForm;
            scApp = _mainForm.BCApp.SCApplication;

            initialCombobox();
            initialDataGridView();
        }

        private void initialDataGridView()
        {
            dgv_current_in_line_carrier.AutoGenerateColumns = false;
            refreshCurrentInLineCarrierInfoAsync();
        }

        private void initialCombobox()
        {
            List<string> lstVh = new List<string>();
            lstVh.Add(string.Empty);
            lstVh.AddRange(scApp.VehicleBLL.cache.loadAllVh().Select(vh => vh.VEHICLE_ID).ToList());
            Common.BCUtility.setComboboxDataSource(cmb_InstalledVhID, lstVh.ToArray());
            cmb_location_ids.DisplayMember = "ID";
            cmb_readyAgvSt.DisplayMember = "PortAdrInfo";
            cmb_readyAgvSt.ValueMember = "PORT_ID";

        }

        private async void btn_installed_Click(object sender, EventArgs e)
        {
            try
            {
                string vh_id = cmb_InstalledVhID.Text;
                string location_id = cmb_location_ids.Text;
                string cst_id = txt_InstalledCSTID.Text;
                btn_installed.Enabled = false;
                (bool isSuccess, string result) check_result = (false, "");
                await Task.Run(() =>
                {
                    check_result = scApp.TransferService.ForceInstallCarrierInVehicle(vh_id, location_id, cst_id);
                });
                MessageBox.Show(check_result.result, "Carrier installed info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await Task.Run(() =>
                {
                    SpinWait.SpinUntil(() => false, 2000);
                });
            }
            catch { }
            finally
            {
                btn_installed.Enabled = true;
            }
        }

        private async void btn_remove_Click(object sender, EventArgs e)
        {
            try
            {

                if (dgv_current_in_line_carrier.SelectedRows[0].Index < 0)
                {
                    MessageBox.Show("Carrier Remove info", "No select remove carrier.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string carrier_id = dgv_current_in_line_carrier.SelectedRows[0].Cells[0].Value as string;

                if (MessageBox.Show($"Are you sure to remove carrier:[{carrier_id}]?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                }
                else
                {
                    return;
                }

                btn_remove.Enabled = false;
                (bool isSuccess, string result) check_result = (false, "");
                await Task.Run(() =>
                {
                    check_result = scApp.TransferService.ForceRemoveCarrierInVehicleByOP(carrier_id);
                });
                MessageBox.Show(check_result.result, "Carrier remove info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await Task.Run(() =>
                {
                    SpinWait.SpinUntil(() => false, 2000);
                });
            }
            catch { }
            finally
            {
                btn_remove.Enabled = true;
            }
        }

        private void CarrierMaintenanceForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainForm.removeForm(this.Name);
        }

        private void cmb_InstalledVhID_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected_vh_id = cmb_InstalledVhID.Text;
            AVEHICLE vh = scApp.VehicleBLL.cache.getVehicle(selected_vh_id);
            if (vh == null) return;
            List<string> location_ids = new List<string>();
            location_ids.Add(vh.Real_ID);
            //List<string> location_ids = vh.CarrierLocation.Select(loc => loc.ID).ToList();
            //cmb_location_ids.DataSource = vh.CarrierLocation;
            cmb_location_ids.DataSource = location_ids;
        }

        private void cmb_location_ids_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected_vh_id = cmb_InstalledVhID.Text;
            AVEHICLE vh = scApp.VehicleBLL.cache.getVehicle(selected_vh_id);
            if (vh == null) return;
            //var location = cmb_location_ids.SelectedItem as AVEHICLE.Location;
            txt_InstalledCSTID.Text = SCUtility.Trim(vh.CST_ID, true);
        }

        private async void btn_refresh_Click(object sender, EventArgs e)
        {
            try
            {
                skinGroupBox2.Enabled = false;
                refreshCurrentInLineCarrierInfoAsync();
                await Task.Run(() => scApp.PortStationBLL.updatePortStatusByRedis());
                //refreshCurrentReadyAGVStation();
            }
            catch (Exception ex) { }
            finally
            {
                skinGroupBox2.Enabled = true;
            }
        }

        private void refreshCurrentReadyAGVStation()
        {
            var agv_stations = scApp.PortStationBLL.OperateCatch.loadAGVPortStationCanUnload(scApp.CMDBLL, "EQ_ZONE1");
            cmb_readyAgvSt.DataSource = agv_stations;
        }

        private async void refreshCurrentInLineCarrierInfoAsync()
        {
            List<ACARRIER> carriers = null;
            await Task.Run(() =>
            {
                carriers = scApp.CarrierBLL.db.loadCurrentInLineCarrier();
            });

            carriers.ForEach(carrier => SCUtility.TrimAllParameter(carrier));
            dgv_current_in_line_carrier.DataSource = carriers;
        }

        private async void btn_toAGVSt_Click(object sender, EventArgs e)
        {
            var selected_row = dgv_current_in_line_carrier.SelectedRows[0];
            if (selected_row.Index < 0)
            {
                MessageBox.Show("No select remove carrier.", "Carrier To AGV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string selected_agv_station = cmb_readyAgvSt.SelectedValue as string;
            string carrier_id = selected_row.Cells[DGV_CURRENT_IN_LINE_CARRIER_INFO_INDEX_ID].Value as string;
            string carrier_current_location = selected_row.Cells[DGV_CURRENT_IN_LINE_CARRIER_INFO_INDEX_LOCATION].Value as string;
            //確認Current Location是在車上，並且當前車子也真的有該CST
            var check_is_in_vh = IsCstInVh(carrier_id, carrier_current_location);
            if (!check_is_in_vh.isIn)
            {
                MessageBox.Show(check_is_in_vh.result, "Carrier To AGV St.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var port_station = scApp.PortStationBLL.OperateCatch.getPortStation(selected_agv_station);
            sc.BLL.CMDBLL.CommandCheckResult check_result_info = null;
            await Task.Run(() =>
            {
                scApp.VehicleService.Command.Unload(check_is_in_vh.vh.VEHICLE_ID, carrier_id, port_station.ADR_ID, port_station.PORT_ID);
                check_result_info = sc.BLL.CMDBLL.getCallContext<sc.BLL.CMDBLL.CommandCheckResult>
                                                    (sc.BLL.CMDBLL.CALL_CONTEXT_KEY_WORD_OHTC_CMD_CHECK_RESULT);

            });
            if (check_result_info != null && !check_result_info.IsSuccess)
            {
                MessageBox.Show(check_result_info.ToString(), "Command create fail.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void btn_toDest_Click(object sender, EventArgs e)
        {
            var selected_row = dgv_current_in_line_carrier.SelectedRows[0];
            if (selected_row.Index < 0)
            {
                MessageBox.Show("No select remove carrier.", "Carrier To AGV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string carrier_current_location = selected_row.Cells[DGV_CURRENT_IN_LINE_CARRIER_INFO_INDEX_LOCATION].Value as string;
            string carrier_id = selected_row.Cells[DGV_CURRENT_IN_LINE_CARRIER_INFO_INDEX_ID].Value as string;
            var check_is_in_vh = IsCstInVh(carrier_id, carrier_current_location);
            if (!check_is_in_vh.isIn)
            {
                MessageBox.Show(check_is_in_vh.result, "Carrier To AGV St.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string dest_port_id = selected_row.Cells[DGV_CURRENT_IN_LINE_CARRIER_INFO_INDEX_DEST].Value as string;
            var port_station = scApp.PortStationBLL.OperateCatch.getPortStation(dest_port_id);

            sc.BLL.CMDBLL.CommandCheckResult check_result_info = null;
            await Task.Run(() =>
            {
                scApp.VehicleService.Command.Unload(check_is_in_vh.vh.VEHICLE_ID, carrier_id, port_station.ADR_ID, port_station.PORT_ID);
                check_result_info = sc.BLL.CMDBLL.getCallContext<sc.BLL.CMDBLL.CommandCheckResult>
                                                    (sc.BLL.CMDBLL.CALL_CONTEXT_KEY_WORD_OHTC_CMD_CHECK_RESULT);

            });
            if (check_result_info != null && !check_result_info.IsSuccess)
            {
                MessageBox.Show(check_result_info.ToString(), "Command create fail.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private (bool isIn, AVEHICLE vh, string result) IsCstInVh(string carrier_id, string location)
        {
            var vh = scApp.VehicleBLL.cache.getVehicleByLocationRealID(location);
            if (vh == null)
            {
                return (false, null, $"carrier:{carrier_id}, not in vh.");
            }
            if (SCUtility.isMatche(vh.CST_ID, carrier_id))
            {
                //not thing...
            }
            else
            {
                return (false, null, $"carrier:{carrier_id}, not in vh:{vh.VEHICLE_ID} of location.");
            }
            return (true, vh, "");
        }
    }
}
