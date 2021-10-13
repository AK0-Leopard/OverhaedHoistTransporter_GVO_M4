using com.mirle.ibg3k0.sc;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.ObjectRelay;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com.mirle.ibg3k0.bc.winform.UI
{
    public partial class HistoryAlarmsForm : Form
    {
        BCMainForm mainform;
        BindingSource cmsMCS_bindingSource = new BindingSource();
        List<ALARMObjToShow> alarmShowList = null;
#pragma warning disable CS0414 // 已指派欄位 'HistoryAlarmsForm.selection_index'，但從未使用過其值。
        int selection_index = -1;
#pragma warning restore CS0414 // 已指派欄位 'HistoryAlarmsForm.selection_index'，但從未使用過其值。
        public HistoryAlarmsForm(BCMainForm _mainForm)
        {
            InitializeComponent();
            dgv_TransferCommand.AutoGenerateColumns = false;
            mainform = _mainForm;

            dgv_TransferCommand.DataSource = cmsMCS_bindingSource;

            m_StartDTCbx.Value = DateTime.Today;
            m_EndDTCbx.Value = DateTime.Now;
        }

        private void updateAlarms()
        {
            DateTime start_time = m_StartDTCbx.Value;
            DateTime end_time = m_EndDTCbx.Value;
            var alarms = mainform.BCApp.SCApplication.AlarmBLL.GetAlarms(start_time, end_time);
            if (alarms != null && alarms.Count > 0)
            {
                string alarm_code = m_AlarmCodeTbl.Text;
                string device_id = m_EqptIDCbx.Text;
                if (!SCUtility.isEmpty(alarm_code))
                {
                    alarms = alarms.Where(alarm => SCUtility.isMatche(alarm.ALAM_CODE, alarm_code)).ToList();
                }
                if (!SCUtility.isEmpty(device_id))
                {
                    alarms = alarms.Where(alarm => SCUtility.isMatche(alarm.EQPT_ID, device_id)).ToList();
                }
                alarmShowList = alarms.Select(alarm => new ALARMObjToShow(alarm)).ToList();
                cmsMCS_bindingSource.DataSource = alarmShowList;
                dgv_TransferCommand.Refresh();
            }
        }

        private void TransferCommandQureyListForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            mainform.removeForm(this.Name);
        }

        private void btnlSearch_Click(object sender, EventArgs e)
        {
            selection_index = -1;
            updateAlarms();
        }

        private async void m_exportBtn_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "Alarm files (*.xlsx)|*.xlsx";
                if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK || bcf.Common.BCFUtility.isEmpty(dlg.FileName))
                {
                    return;
                }
                string filename = dlg.FileName;
                //建立 xlxs 轉換物件
                Common.XSLXHelper helper = new Common.XSLXHelper();
                //取得轉為 xlsx 的物件
                ClosedXML.Excel.XLWorkbook xlsx = null;
                await Task.Run(() => xlsx = helper.Export(alarmShowList));
                if (xlsx != null)
                    xlsx.SaveAs(filename);
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Warn(ex, "Exception");
            }
        }
    }
}
