namespace com.mirle.ibg3k0.bc.winform.UI
{
    partial class CarrierMaintenanceForm
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
            this.skinGroupBox1 = new CCWin.SkinControl.SkinGroupBox();
            this.cmb_location_ids = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txt_InstalledCSTID = new System.Windows.Forms.TextBox();
            this.cmb_InstalledVhID = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lbl_installed_vh_id = new System.Windows.Forms.Label();
            this.btn_installed = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label7 = new System.Windows.Forms.Label();
            this.btn_remove = new System.Windows.Forms.Button();
            this.skinGroupBox2 = new CCWin.SkinControl.SkinGroupBox();
            this.cmb_readyAgvSt = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btn_toAGVSt = new System.Windows.Forms.Button();
            this.btn_toDest = new System.Windows.Forms.Button();
            this.btn_refresh = new System.Windows.Forms.Button();
            this.dgv_current_in_line_carrier = new System.Windows.Forms.DataGridView();
            this.dgv_carrier_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgv_location_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgv_host_dest = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgv_State = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.skinGroupBox1.SuspendLayout();
            this.skinGroupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_current_in_line_carrier)).BeginInit();
            this.SuspendLayout();
            // 
            // skinGroupBox1
            // 
            this.skinGroupBox1.BackColor = System.Drawing.Color.Transparent;
            this.skinGroupBox1.BorderColor = System.Drawing.Color.Black;
            this.skinGroupBox1.Controls.Add(this.cmb_location_ids);
            this.skinGroupBox1.Controls.Add(this.label3);
            this.skinGroupBox1.Controls.Add(this.txt_InstalledCSTID);
            this.skinGroupBox1.Controls.Add(this.cmb_InstalledVhID);
            this.skinGroupBox1.Controls.Add(this.label1);
            this.skinGroupBox1.Controls.Add(this.lbl_installed_vh_id);
            this.skinGroupBox1.Controls.Add(this.btn_installed);
            this.skinGroupBox1.Controls.Add(this.label2);
            this.skinGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.skinGroupBox1.Font = new System.Drawing.Font("Arial", 15.75F);
            this.skinGroupBox1.ForeColor = System.Drawing.Color.Black;
            this.skinGroupBox1.Location = new System.Drawing.Point(9, 12);
            this.skinGroupBox1.Name = "skinGroupBox1";
            this.skinGroupBox1.Radius = 20;
            this.skinGroupBox1.RectBackColor = System.Drawing.SystemColors.Control;
            this.skinGroupBox1.RoundStyle = CCWin.SkinClass.RoundStyle.All;
            this.skinGroupBox1.Size = new System.Drawing.Size(395, 469);
            this.skinGroupBox1.TabIndex = 71;
            this.skinGroupBox1.TabStop = false;
            this.skinGroupBox1.Text = "Carrier Installed";
            this.skinGroupBox1.TitleBorderColor = System.Drawing.Color.Black;
            this.skinGroupBox1.TitleRadius = 10;
            this.skinGroupBox1.TitleRectBackColor = System.Drawing.Color.LightSkyBlue;
            this.skinGroupBox1.TitleRoundStyle = CCWin.SkinClass.RoundStyle.All;
            // 
            // cmb_location_ids
            // 
            this.cmb_location_ids.FormattingEnabled = true;
            this.cmb_location_ids.Location = new System.Drawing.Point(10, 136);
            this.cmb_location_ids.Name = "cmb_location_ids";
            this.cmb_location_ids.Size = new System.Drawing.Size(180, 32);
            this.cmb_location_ids.TabIndex = 85;
            this.cmb_location_ids.SelectedIndexChanged += new System.EventHandler(this.cmb_location_ids_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(6, 110);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(132, 23);
            this.label3.TabIndex = 84;
            this.label3.Text = "Location ID:";
            // 
            // txt_InstalledCSTID
            // 
            this.txt_InstalledCSTID.Location = new System.Drawing.Point(10, 213);
            this.txt_InstalledCSTID.Multiline = true;
            this.txt_InstalledCSTID.Name = "txt_InstalledCSTID";
            this.txt_InstalledCSTID.Size = new System.Drawing.Size(349, 33);
            this.txt_InstalledCSTID.TabIndex = 82;
            // 
            // cmb_InstalledVhID
            // 
            this.cmb_InstalledVhID.FormattingEnabled = true;
            this.cmb_InstalledVhID.Location = new System.Drawing.Point(10, 59);
            this.cmb_InstalledVhID.Name = "cmb_InstalledVhID";
            this.cmb_InstalledVhID.Size = new System.Drawing.Size(180, 32);
            this.cmb_InstalledVhID.TabIndex = 81;
            this.cmb_InstalledVhID.SelectedIndexChanged += new System.EventHandler(this.cmb_InstalledVhID_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(6, 187);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(156, 23);
            this.label1.TabIndex = 78;
            this.label1.Text = "CST ID:";
            // 
            // lbl_installed_vh_id
            // 
            this.lbl_installed_vh_id.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lbl_installed_vh_id.Location = new System.Drawing.Point(6, 35);
            this.lbl_installed_vh_id.Name = "lbl_installed_vh_id";
            this.lbl_installed_vh_id.Size = new System.Drawing.Size(70, 23);
            this.lbl_installed_vh_id.TabIndex = 77;
            this.lbl_installed_vh_id.Text = "Vh ID:";
            // 
            // btn_installed
            // 
            this.btn_installed.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_installed.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold);
            this.btn_installed.ForeColor = System.Drawing.Color.Black;
            this.btn_installed.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_installed.Location = new System.Drawing.Point(108, 393);
            this.btn_installed.Name = "btn_installed";
            this.btn_installed.Size = new System.Drawing.Size(168, 35);
            this.btn_installed.TabIndex = 20;
            this.btn_installed.Text = "Installed";
            this.btn_installed.UseVisualStyleBackColor = true;
            this.btn_installed.Click += new System.EventHandler(this.btn_installed_Click);
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.DimGray;
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(-1, 313);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(490, 1);
            this.label2.TabIndex = 69;
            this.label2.Text = "label2";
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            // 
            // label7
            // 
            this.label7.BackColor = System.Drawing.Color.DimGray;
            this.label7.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label7.Location = new System.Drawing.Point(2, 313);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(721, 1);
            this.label7.TabIndex = 69;
            this.label7.Text = "label7";
            // 
            // btn_remove
            // 
            this.btn_remove.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_remove.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold);
            this.btn_remove.ForeColor = System.Drawing.Color.Black;
            this.btn_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_remove.Location = new System.Drawing.Point(595, 393);
            this.btn_remove.Name = "btn_remove";
            this.btn_remove.Size = new System.Drawing.Size(116, 35);
            this.btn_remove.TabIndex = 20;
            this.btn_remove.Text = "Remove";
            this.btn_remove.UseVisualStyleBackColor = true;
            this.btn_remove.Click += new System.EventHandler(this.btn_remove_Click);
            // 
            // skinGroupBox2
            // 
            this.skinGroupBox2.BackColor = System.Drawing.Color.Transparent;
            this.skinGroupBox2.BorderColor = System.Drawing.Color.Black;
            this.skinGroupBox2.Controls.Add(this.cmb_readyAgvSt);
            this.skinGroupBox2.Controls.Add(this.label4);
            this.skinGroupBox2.Controls.Add(this.btn_toAGVSt);
            this.skinGroupBox2.Controls.Add(this.btn_toDest);
            this.skinGroupBox2.Controls.Add(this.btn_refresh);
            this.skinGroupBox2.Controls.Add(this.dgv_current_in_line_carrier);
            this.skinGroupBox2.Controls.Add(this.btn_remove);
            this.skinGroupBox2.Controls.Add(this.label7);
            this.skinGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.skinGroupBox2.Font = new System.Drawing.Font("Arial", 15.75F);
            this.skinGroupBox2.ForeColor = System.Drawing.Color.Black;
            this.skinGroupBox2.Location = new System.Drawing.Point(410, 12);
            this.skinGroupBox2.Name = "skinGroupBox2";
            this.skinGroupBox2.Radius = 20;
            this.skinGroupBox2.RectBackColor = System.Drawing.SystemColors.Control;
            this.skinGroupBox2.RoundStyle = CCWin.SkinClass.RoundStyle.All;
            this.skinGroupBox2.Size = new System.Drawing.Size(725, 469);
            this.skinGroupBox2.TabIndex = 83;
            this.skinGroupBox2.TabStop = false;
            this.skinGroupBox2.Text = "Carrier Remove";
            this.skinGroupBox2.TitleBorderColor = System.Drawing.Color.Black;
            this.skinGroupBox2.TitleRadius = 10;
            this.skinGroupBox2.TitleRectBackColor = System.Drawing.Color.LightSkyBlue;
            this.skinGroupBox2.TitleRoundStyle = CCWin.SkinClass.RoundStyle.All;
            // 
            // cmb_readyAgvSt
            // 
            this.cmb_readyAgvSt.FormattingEnabled = true;
            this.cmb_readyAgvSt.Location = new System.Drawing.Point(13, 351);
            this.cmb_readyAgvSt.Name = "cmb_readyAgvSt";
            this.cmb_readyAgvSt.Size = new System.Drawing.Size(361, 32);
            this.cmb_readyAgvSt.TabIndex = 87;
            this.cmb_readyAgvSt.Visible = false;
            // 
            // label4
            // 
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(9, 325);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(162, 23);
            this.label4.TabIndex = 86;
            this.label4.Text = "Ready AGV St.:";
            this.label4.Visible = false;
            // 
            // btn_toAGVSt
            // 
            this.btn_toAGVSt.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_toAGVSt.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold);
            this.btn_toAGVSt.ForeColor = System.Drawing.Color.Black;
            this.btn_toAGVSt.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_toAGVSt.Location = new System.Drawing.Point(210, 393);
            this.btn_toAGVSt.Name = "btn_toAGVSt";
            this.btn_toAGVSt.Size = new System.Drawing.Size(164, 35);
            this.btn_toAGVSt.TabIndex = 88;
            this.btn_toAGVSt.Text = "To AGV St.";
            this.btn_toAGVSt.UseVisualStyleBackColor = true;
            this.btn_toAGVSt.Visible = false;
            this.btn_toAGVSt.Click += new System.EventHandler(this.btn_toAGVSt_Click);
            // 
            // btn_toDest
            // 
            this.btn_toDest.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_toDest.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold);
            this.btn_toDest.ForeColor = System.Drawing.Color.Black;
            this.btn_toDest.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_toDest.Location = new System.Drawing.Point(408, 393);
            this.btn_toDest.Name = "btn_toDest";
            this.btn_toDest.Size = new System.Drawing.Size(154, 35);
            this.btn_toDest.TabIndex = 87;
            this.btn_toDest.Text = "To Destination";
            this.btn_toDest.UseVisualStyleBackColor = true;
            this.btn_toDest.Click += new System.EventHandler(this.btn_toDest_Click);
            // 
            // btn_refresh
            // 
            this.btn_refresh.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btn_refresh.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold);
            this.btn_refresh.ForeColor = System.Drawing.Color.Black;
            this.btn_refresh.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_refresh.Location = new System.Drawing.Point(13, 35);
            this.btn_refresh.Name = "btn_refresh";
            this.btn_refresh.Size = new System.Drawing.Size(168, 35);
            this.btn_refresh.TabIndex = 86;
            this.btn_refresh.Text = "Refresh";
            this.btn_refresh.UseVisualStyleBackColor = true;
            this.btn_refresh.Click += new System.EventHandler(this.btn_refresh_Click);
            // 
            // dgv_current_in_line_carrier
            // 
            this.dgv_current_in_line_carrier.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_current_in_line_carrier.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgv_carrier_id,
            this.dgv_location_id,
            this.dgv_host_dest,
            this.dgv_State});
            this.dgv_current_in_line_carrier.Location = new System.Drawing.Point(12, 76);
            this.dgv_current_in_line_carrier.MultiSelect = false;
            this.dgv_current_in_line_carrier.Name = "dgv_current_in_line_carrier";
            this.dgv_current_in_line_carrier.ReadOnly = true;
            this.dgv_current_in_line_carrier.RowTemplate.Height = 24;
            this.dgv_current_in_line_carrier.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_current_in_line_carrier.Size = new System.Drawing.Size(699, 218);
            this.dgv_current_in_line_carrier.TabIndex = 85;
            // 
            // dgv_carrier_id
            // 
            this.dgv_carrier_id.DataPropertyName = "ID";
            this.dgv_carrier_id.HeaderText = "ID";
            this.dgv_carrier_id.Name = "dgv_carrier_id";
            this.dgv_carrier_id.ReadOnly = true;
            this.dgv_carrier_id.Width = 150;
            // 
            // dgv_location_id
            // 
            this.dgv_location_id.DataPropertyName = "LOCATION";
            this.dgv_location_id.HeaderText = "Location";
            this.dgv_location_id.Name = "dgv_location_id";
            this.dgv_location_id.ReadOnly = true;
            this.dgv_location_id.Width = 200;
            // 
            // dgv_host_dest
            // 
            this.dgv_host_dest.DataPropertyName = "HOSTDESTINATION";
            this.dgv_host_dest.HeaderText = "Dest.";
            this.dgv_host_dest.Name = "dgv_host_dest";
            this.dgv_host_dest.ReadOnly = true;
            this.dgv_host_dest.Width = 200;
            // 
            // dgv_State
            // 
            this.dgv_State.DataPropertyName = "STATE";
            this.dgv_State.HeaderText = "State";
            this.dgv_State.Name = "dgv_State";
            this.dgv_State.ReadOnly = true;
            // 
            // CarrierMaintenanceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1138, 492);
            this.Controls.Add(this.skinGroupBox2);
            this.Controls.Add(this.skinGroupBox1);
            this.Name = "CarrierMaintenanceForm";
            this.Text = "Carrier Installed / Remove";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CarrierMaintenanceForm_FormClosed);
            this.skinGroupBox1.ResumeLayout(false);
            this.skinGroupBox1.PerformLayout();
            this.skinGroupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_current_in_line_carrier)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private CCWin.SkinControl.SkinGroupBox skinGroupBox1;
        private System.Windows.Forms.Label lbl_installed_vh_id;
        private System.Windows.Forms.Button btn_installed;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmb_InstalledVhID;
        private System.Windows.Forms.TextBox txt_InstalledCSTID;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btn_remove;
        private CCWin.SkinControl.SkinGroupBox skinGroupBox2;
        private System.Windows.Forms.ComboBox cmb_location_ids;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridView dgv_current_in_line_carrier;
        private System.Windows.Forms.Button btn_refresh;
        private System.Windows.Forms.Button btn_toAGVSt;
        private System.Windows.Forms.Button btn_toDest;
        private System.Windows.Forms.ComboBox cmb_readyAgvSt;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgv_carrier_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgv_location_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgv_host_dest;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgv_State;
    }
}