using com.mirle.ibg3k0.sc;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com.mirle.ibg3k0.bc.winform.UI
{
    public partial class CarrierLocationChooseForm : Form
    {
        SCApplication scApp;

        public CarrierLocationChooseForm(SCApplication _scApp, ATRANSFER transfer)
        {
            InitializeComponent();
            scApp = _scApp;
            initialVhLocationCmd();
            initialForceFinishStatus();
            initialCarrierInfo(transfer);
        }

        private void initialForceFinishStatus()
        {
            cmd_force_finish_status.DataSource = Enum.GetValues(typeof(CompleteStatus)).Cast<CompleteStatus>()
                                      .Where(e => e == CompleteStatus.ForceAbnormalFinishByOp ||
                                                  e == CompleteStatus.ForceNormalFinishByOp
                                                  ).ToList();
        }

        private void initialVhLocationCmd()
        {
            List<string> lstVh = new List<string>();
            lstVh.Add(string.Empty);
            lstVh.AddRange(scApp.VehicleBLL.cache.loadAllVh().Select(vh => vh.VEHICLE_ID).ToList());
            Common.BCUtility.setComboboxDataSource(cmb_vhIDs, lstVh.ToArray());
            cmb_vhLocations.DisplayMember = "ID";

        }

        private void initialCarrierInfo(ATRANSFER transfer)
        {
            string source = SCUtility.Trim(transfer.HOSTSOURCE, true);
            string dest = SCUtility.Trim(transfer.HOSTDESTINATION, true);
            string carrier_id = SCUtility.Trim(transfer.CARRIER_ID, true);
            string cmd_id = SCUtility.Trim(transfer.ID, true);
            m_CMDIDTxb.Text = cmd_id;
            m_CSTIDTxb.Text = carrier_id;
            m_sourceTxb.Text = source;
            m_destTxb.Text = dest;
        }

        public string GetChooseLocation()
        {
            if (radioBtn_Source.Checked)
                return m_sourceTxb.Text;
            else if (radioBtn_dest.Checked)
                return m_destTxb.Text;
            else if (radioBtn_InVehicle.Checked)
                return cmb_vhLocations.Text;
            else if (radioBtn_manual.Checked)
                return m_destTxb.Text;
            else
                return "";
        }
        public CompleteStatus GetCompleteStatus()
        {
            CompleteStatus complete_status;
            Enum.TryParse(cmd_force_finish_status.SelectedValue.ToString(), out complete_status);
            return complete_status;
        }

        private void CarrierLocationChooseForm_Load(object sender, EventArgs e)
        {
            m_confirmBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void cmb_vhIDs_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected_vh_id = cmb_vhIDs.Text;
            AVEHICLE vh = scApp.VehicleBLL.cache.getVehicle(selected_vh_id);
            if (vh == null) return;
            List<string> location_ids = new List<string>();
            location_ids.Add(vh.Real_ID);
            //List<string> location_ids = vh.CarrierLocation.Select(loc => loc.ID).ToList();
            cmb_vhLocations.DataSource = location_ids;
        }

        private void m_cancelBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void radioBtn_Click(object sender, EventArgs e)
        {
            if ((sender is RadioButton))
            {
                RadioButton radio = sender as RadioButton;
                if (SCUtility.isMatche(radio.Name, "radioBtn_Source") ||
                    SCUtility.isMatche(radio.Name, "radioBtn_InVehicle"))
                {
                    cmd_force_finish_status.SelectedIndex = 0;
                    cmd_force_finish_status.Enabled = false;
                }
                else
                {
                    cmd_force_finish_status.SelectedIndex = 0;
                    cmd_force_finish_status.Enabled = true;
                }
            }
        }
    }
}
