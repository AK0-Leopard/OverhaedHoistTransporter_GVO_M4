using com.mirle.ibg3k0.sc;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.ObjectRelay;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com.mirle.ibg3k0.bc.winform.UI
{
    public partial class HistoryTransferCommandQureyListForm : Form
    {
        BCMainForm mainform;
        BindingSource cmsMCS_bindingSource = new BindingSource();
        List<HCMD_MCSObjToShow> cmdMCSList = null;
#pragma warning disable CS0414 // 已指派欄位 'HistoryTransferCommandQureyListForm.selection_index'，但從未使用過其值。
        int selection_index = -1;
#pragma warning restore CS0414 // 已指派欄位 'HistoryTransferCommandQureyListForm.selection_index'，但從未使用過其值。
        public HistoryTransferCommandQureyListForm(BCMainForm _mainForm)
        {
            InitializeComponent();
            dgv_TransferCommand.AutoGenerateColumns = false;
            mainform = _mainForm;

            dgv_TransferCommand.DataSource = cmsMCS_bindingSource;
        }

        private void updateTransferCommand()
        {
            cmdMCSList = mainform.BCApp.SCApplication.CMDBLL.loadHCMD_MCSs();
            cmsMCS_bindingSource.DataSource = cmdMCSList;
            dgv_TransferCommand.Refresh();
        }

        private void btn_refresh_Click(object sender, EventArgs e)
        {
            selection_index = -1;
            updateTransferCommand();
        }
        private void TransferCommandQureyListForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            mainform.removeForm(this.Name);
        }
    }
}
