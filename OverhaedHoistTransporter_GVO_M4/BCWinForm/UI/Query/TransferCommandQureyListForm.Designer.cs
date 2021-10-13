namespace com.mirle.ibg3k0.bc.winform.UI
{
    partial class TransferCommandQureyListForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.cMDMCSObjToShowBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.btn_cancel_abort = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.dgv_TransferCommand = new System.Windows.Forms.DataGridView();
            this.cMDIDDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cARRIERIDDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tRANSFERSTATEDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.hOSTSOURCEDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.hOSTDESTINATIONDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pRIORITYDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cMDINSERTIMEDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cMDSTARTTIMEDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rEPLACEDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pel_button = new System.Windows.Forms.Panel();
            this.lbl_st_port_id = new System.Windows.Forms.Label();
            this.lbl_vhid = new System.Windows.Forms.Label();
            this.cmb_st_port_ids = new System.Windows.Forms.ComboBox();
            this.btn_force_assign = new System.Windows.Forms.Button();
            this.cmb_force_assign = new System.Windows.Forms.ComboBox();
            this.btn_force_finish = new System.Windows.Forms.Button();
            this.btn_refresh = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.cMDMCSObjToShowBindingSource)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_TransferCommand)).BeginInit();
            this.pel_button.SuspendLayout();
            this.SuspendLayout();
            // 
            // cMDMCSObjToShowBindingSource
            // 
            this.cMDMCSObjToShowBindingSource.DataSource = typeof(com.mirle.ibg3k0.sc.ObjectRelay.TRANSFERObjToShow);
            // 
            // btn_cancel_abort
            // 
            this.btn_cancel_abort.Location = new System.Drawing.Point(134, 12);
            this.btn_cancel_abort.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.btn_cancel_abort.Name = "btn_cancel_abort";
            this.btn_cancel_abort.Size = new System.Drawing.Size(200, 42);
            this.btn_cancel_abort.TabIndex = 2;
            this.btn_cancel_abort.Text = "Cancel / Abort";
            this.btn_cancel_abort.UseVisualStyleBackColor = true;
            this.btn_cancel_abort.Click += new System.EventHandler(this.btn_cancel_abort_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.dgv_TransferCommand, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.pel_button, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 143F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1602, 716);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // dgv_TransferCommand
            // 
            this.dgv_TransferCommand.AllowUserToAddRows = false;
            this.dgv_TransferCommand.AutoGenerateColumns = false;
            this.dgv_TransferCommand.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgv_TransferCommand.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dgv_TransferCommand.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_TransferCommand.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.cMDIDDataGridViewTextBoxColumn,
            this.cARRIERIDDataGridViewTextBoxColumn,
            this.tRANSFERSTATEDataGridViewTextBoxColumn,
            this.hOSTSOURCEDataGridViewTextBoxColumn,
            this.hOSTDESTINATIONDataGridViewTextBoxColumn,
            this.pRIORITYDataGridViewTextBoxColumn,
            this.cMDINSERTIMEDataGridViewTextBoxColumn,
            this.cMDSTARTTIMEDataGridViewTextBoxColumn,
            this.rEPLACEDataGridViewTextBoxColumn});
            this.dgv_TransferCommand.DataSource = this.cMDMCSObjToShowBindingSource;
            this.dgv_TransferCommand.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_TransferCommand.GridColor = System.Drawing.SystemColors.ControlDarkDark;
            this.dgv_TransferCommand.Location = new System.Drawing.Point(3, 3);
            this.dgv_TransferCommand.MultiSelect = false;
            this.dgv_TransferCommand.Name = "dgv_TransferCommand";
            this.dgv_TransferCommand.ReadOnly = true;
            this.dgv_TransferCommand.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dgv_TransferCommand.RowTemplate.Height = 24;
            this.dgv_TransferCommand.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_TransferCommand.Size = new System.Drawing.Size(1596, 567);
            this.dgv_TransferCommand.TabIndex = 8;
            this.dgv_TransferCommand.SelectionChanged += new System.EventHandler(this.dgv_TransferCommand_SelectionChanged);
            // 
            // cMDIDDataGridViewTextBoxColumn
            // 
            this.cMDIDDataGridViewTextBoxColumn.DataPropertyName = "CMD_ID";
            this.cMDIDDataGridViewTextBoxColumn.HeaderText = "ID";
            this.cMDIDDataGridViewTextBoxColumn.Name = "cMDIDDataGridViewTextBoxColumn";
            this.cMDIDDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // cARRIERIDDataGridViewTextBoxColumn
            // 
            this.cARRIERIDDataGridViewTextBoxColumn.DataPropertyName = "CARRIER_ID";
            this.cARRIERIDDataGridViewTextBoxColumn.HeaderText = "Carrier ID";
            this.cARRIERIDDataGridViewTextBoxColumn.Name = "cARRIERIDDataGridViewTextBoxColumn";
            this.cARRIERIDDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // tRANSFERSTATEDataGridViewTextBoxColumn
            // 
            this.tRANSFERSTATEDataGridViewTextBoxColumn.DataPropertyName = "TRANSFERSTATE";
            this.tRANSFERSTATEDataGridViewTextBoxColumn.FillWeight = 60F;
            this.tRANSFERSTATEDataGridViewTextBoxColumn.HeaderText = "State";
            this.tRANSFERSTATEDataGridViewTextBoxColumn.Name = "tRANSFERSTATEDataGridViewTextBoxColumn";
            this.tRANSFERSTATEDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // hOSTSOURCEDataGridViewTextBoxColumn
            // 
            this.hOSTSOURCEDataGridViewTextBoxColumn.DataPropertyName = "HOSTSOURCE";
            this.hOSTSOURCEDataGridViewTextBoxColumn.FillWeight = 150F;
            this.hOSTSOURCEDataGridViewTextBoxColumn.HeaderText = "L Port";
            this.hOSTSOURCEDataGridViewTextBoxColumn.Name = "hOSTSOURCEDataGridViewTextBoxColumn";
            this.hOSTSOURCEDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // hOSTDESTINATIONDataGridViewTextBoxColumn
            // 
            this.hOSTDESTINATIONDataGridViewTextBoxColumn.DataPropertyName = "HOSTDESTINATION";
            this.hOSTDESTINATIONDataGridViewTextBoxColumn.FillWeight = 150F;
            this.hOSTDESTINATIONDataGridViewTextBoxColumn.HeaderText = "U Port";
            this.hOSTDESTINATIONDataGridViewTextBoxColumn.Name = "hOSTDESTINATIONDataGridViewTextBoxColumn";
            this.hOSTDESTINATIONDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // pRIORITYDataGridViewTextBoxColumn
            // 
            this.pRIORITYDataGridViewTextBoxColumn.DataPropertyName = "PRIORITY";
            this.pRIORITYDataGridViewTextBoxColumn.FillWeight = 70F;
            this.pRIORITYDataGridViewTextBoxColumn.HeaderText = "Priority";
            this.pRIORITYDataGridViewTextBoxColumn.Name = "pRIORITYDataGridViewTextBoxColumn";
            this.pRIORITYDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // cMDINSERTIMEDataGridViewTextBoxColumn
            // 
            this.cMDINSERTIMEDataGridViewTextBoxColumn.DataPropertyName = "CMD_INSER_TIME";
            this.cMDINSERTIMEDataGridViewTextBoxColumn.FillWeight = 120F;
            this.cMDINSERTIMEDataGridViewTextBoxColumn.HeaderText = "Inser Time";
            this.cMDINSERTIMEDataGridViewTextBoxColumn.Name = "cMDINSERTIMEDataGridViewTextBoxColumn";
            this.cMDINSERTIMEDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // cMDSTARTTIMEDataGridViewTextBoxColumn
            // 
            this.cMDSTARTTIMEDataGridViewTextBoxColumn.DataPropertyName = "CMD_START_TIME";
            this.cMDSTARTTIMEDataGridViewTextBoxColumn.FillWeight = 120F;
            this.cMDSTARTTIMEDataGridViewTextBoxColumn.HeaderText = "Start Time";
            this.cMDSTARTTIMEDataGridViewTextBoxColumn.Name = "cMDSTARTTIMEDataGridViewTextBoxColumn";
            this.cMDSTARTTIMEDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // rEPLACEDataGridViewTextBoxColumn
            // 
            this.rEPLACEDataGridViewTextBoxColumn.DataPropertyName = "REPLACE";
            this.rEPLACEDataGridViewTextBoxColumn.FillWeight = 50F;
            this.rEPLACEDataGridViewTextBoxColumn.HeaderText = "Replace";
            this.rEPLACEDataGridViewTextBoxColumn.Name = "rEPLACEDataGridViewTextBoxColumn";
            this.rEPLACEDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // pel_button
            // 
            this.pel_button.Controls.Add(this.lbl_st_port_id);
            this.pel_button.Controls.Add(this.lbl_vhid);
            this.pel_button.Controls.Add(this.cmb_st_port_ids);
            this.pel_button.Controls.Add(this.btn_force_assign);
            this.pel_button.Controls.Add(this.cmb_force_assign);
            this.pel_button.Controls.Add(this.btn_force_finish);
            this.pel_button.Controls.Add(this.btn_refresh);
            this.pel_button.Controls.Add(this.btn_cancel_abort);
            this.pel_button.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pel_button.Location = new System.Drawing.Point(5, 579);
            this.pel_button.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.pel_button.Name = "pel_button";
            this.pel_button.Size = new System.Drawing.Size(1592, 131);
            this.pel_button.TabIndex = 6;
            // 
            // lbl_st_port_id
            // 
            this.lbl_st_port_id.AutoSize = true;
            this.lbl_st_port_id.Location = new System.Drawing.Point(1261, 50);
            this.lbl_st_port_id.Name = "lbl_st_port_id";
            this.lbl_st_port_id.Size = new System.Drawing.Size(130, 22);
            this.lbl_st_port_id.TabIndex = 9;
            this.lbl_st_port_id.Text = "St. Port ID:";
            // 
            // lbl_vhid
            // 
            this.lbl_vhid.AutoSize = true;
            this.lbl_vhid.Location = new System.Drawing.Point(1322, 6);
            this.lbl_vhid.Name = "lbl_vhid";
            this.lbl_vhid.Size = new System.Drawing.Size(70, 22);
            this.lbl_vhid.TabIndex = 9;
            this.lbl_vhid.Text = "Vh ID:";
            // 
            // cmb_st_port_ids
            // 
            this.cmb_st_port_ids.FormattingEnabled = true;
            this.cmb_st_port_ids.Location = new System.Drawing.Point(1398, 47);
            this.cmb_st_port_ids.Name = "cmb_st_port_ids";
            this.cmb_st_port_ids.Size = new System.Drawing.Size(187, 30);
            this.cmb_st_port_ids.TabIndex = 8;
            // 
            // btn_force_assign
            // 
            this.btn_force_assign.Location = new System.Drawing.Point(1445, 83);
            this.btn_force_assign.Name = "btn_force_assign";
            this.btn_force_assign.Size = new System.Drawing.Size(140, 42);
            this.btn_force_assign.TabIndex = 7;
            this.btn_force_assign.Text = "Force Assign";
            this.btn_force_assign.UseVisualStyleBackColor = true;
            this.btn_force_assign.Click += new System.EventHandler(this.btn_force_assign_Click);
            // 
            // cmb_force_assign
            // 
            this.cmb_force_assign.FormattingEnabled = true;
            this.cmb_force_assign.Location = new System.Drawing.Point(1398, 3);
            this.cmb_force_assign.Name = "cmb_force_assign";
            this.cmb_force_assign.Size = new System.Drawing.Size(187, 30);
            this.cmb_force_assign.TabIndex = 6;
            this.cmb_force_assign.SelectedIndexChanged += new System.EventHandler(this.cmb_force_assign_SelectedIndexChanged);
            // 
            // btn_force_finish
            // 
            this.btn_force_finish.Location = new System.Drawing.Point(342, 12);
            this.btn_force_finish.Name = "btn_force_finish";
            this.btn_force_finish.Size = new System.Drawing.Size(140, 42);
            this.btn_force_finish.TabIndex = 5;
            this.btn_force_finish.Text = "Force finish";
            this.btn_force_finish.UseVisualStyleBackColor = true;
            this.btn_force_finish.Click += new System.EventHandler(this.btn_force_finish_Click);
            // 
            // btn_refresh
            // 
            this.btn_refresh.Location = new System.Drawing.Point(7, 12);
            this.btn_refresh.Name = "btn_refresh";
            this.btn_refresh.Size = new System.Drawing.Size(119, 42);
            this.btn_refresh.TabIndex = 4;
            this.btn_refresh.Text = "Refresh";
            this.btn_refresh.UseVisualStyleBackColor = true;
            this.btn_refresh.Click += new System.EventHandler(this.btn_refresh_Click);
            // 
            // TransferCommandQureyListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1602, 716);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Consolas", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.Name = "TransferCommandQureyListForm";
            this.Text = "TransferCommandQureyListForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TransferCommandQureyListForm_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.cMDMCSObjToShowBindingSource)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_TransferCommand)).EndInit();
            this.pel_button.ResumeLayout(false);
            this.pel_button.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.BindingSource cMDMCSObjToShowBindingSource;
        private System.Windows.Forms.Button btn_cancel_abort;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel pel_button;
        private System.Windows.Forms.DataGridView dgv_TransferCommand;
        private System.Windows.Forms.Button btn_refresh;
        private System.Windows.Forms.DataGridViewTextBoxColumn cMDIDDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn cARRIERIDDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn tRANSFERSTATEDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn hOSTSOURCEDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn hOSTDESTINATIONDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn pRIORITYDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn cMDINSERTIMEDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn cMDSTARTTIMEDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn rEPLACEDataGridViewTextBoxColumn;
        private System.Windows.Forms.Button btn_force_finish;
        private System.Windows.Forms.Button btn_force_assign;
        private System.Windows.Forms.ComboBox cmb_force_assign;
        private System.Windows.Forms.ComboBox cmb_st_port_ids;
        private System.Windows.Forms.Label lbl_st_port_id;
        private System.Windows.Forms.Label lbl_vhid;
    }
}