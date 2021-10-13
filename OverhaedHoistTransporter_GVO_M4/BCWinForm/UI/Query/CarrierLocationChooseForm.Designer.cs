namespace com.mirle.ibg3k0.bc.winform.UI
{
    partial class CarrierLocationChooseForm
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
            this.m_tableLayoutPnl = new System.Windows.Forms.TableLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.m_CMDIDTxb = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.m_sourceTxb = new System.Windows.Forms.TextBox();
            this.m_idLbl = new System.Windows.Forms.Label();
            this.m_pwdLbl = new System.Windows.Forms.Label();
            this.m_pwdVerifyLbl = new System.Windows.Forms.Label();
            this.m_CSTIDTxb = new System.Windows.Forms.TextBox();
            this.m_oldpwdLbl = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.cmb_vhLocations = new System.Windows.Forms.ComboBox();
            this.cmb_vhIDs = new System.Windows.Forms.ComboBox();
            this.m_destTxb = new System.Windows.Forms.TextBox();
            this.radioBtn_Source = new System.Windows.Forms.RadioButton();
            this.radioBtn_InVehicle = new System.Windows.Forms.RadioButton();
            this.radioBtn_dest = new System.Windows.Forms.RadioButton();
            this.radioBtn_manual = new System.Windows.Forms.RadioButton();
            this.m_confirmBtn = new CCWin.SkinControl.SkinButton();
            this.m_cancelBtn = new CCWin.SkinControl.SkinButton();
            this.label1 = new System.Windows.Forms.Label();
            this.cmd_force_finish_status = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.m_tableLayoutPnl.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_tableLayoutPnl
            // 
            this.m_tableLayoutPnl.BackColor = System.Drawing.Color.Transparent;
            this.m_tableLayoutPnl.ColumnCount = 3;
            this.m_tableLayoutPnl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.00883F));
            this.m_tableLayoutPnl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.m_tableLayoutPnl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 67.99117F));
            this.m_tableLayoutPnl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.m_tableLayoutPnl.Controls.Add(this.label4, 0, 5);
            this.m_tableLayoutPnl.Controls.Add(this.m_CMDIDTxb, 2, 0);
            this.m_tableLayoutPnl.Controls.Add(this.label2, 0, 0);
            this.m_tableLayoutPnl.Controls.Add(this.m_sourceTxb, 2, 2);
            this.m_tableLayoutPnl.Controls.Add(this.m_idLbl, 0, 1);
            this.m_tableLayoutPnl.Controls.Add(this.m_pwdLbl, 0, 3);
            this.m_tableLayoutPnl.Controls.Add(this.m_pwdVerifyLbl, 0, 4);
            this.m_tableLayoutPnl.Controls.Add(this.m_CSTIDTxb, 2, 1);
            this.m_tableLayoutPnl.Controls.Add(this.m_oldpwdLbl, 0, 2);
            this.m_tableLayoutPnl.Controls.Add(this.tableLayoutPanel1, 2, 3);
            this.m_tableLayoutPnl.Controls.Add(this.m_destTxb, 2, 4);
            this.m_tableLayoutPnl.Controls.Add(this.radioBtn_Source, 1, 2);
            this.m_tableLayoutPnl.Controls.Add(this.radioBtn_InVehicle, 1, 3);
            this.m_tableLayoutPnl.Controls.Add(this.radioBtn_dest, 1, 4);
            this.m_tableLayoutPnl.Controls.Add(this.radioBtn_manual, 1, 5);
            this.m_tableLayoutPnl.Font = new System.Drawing.Font("Arial", 11.25F);
            this.m_tableLayoutPnl.Location = new System.Drawing.Point(19, 59);
            this.m_tableLayoutPnl.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.m_tableLayoutPnl.Name = "m_tableLayoutPnl";
            this.m_tableLayoutPnl.RowCount = 6;
            this.m_tableLayoutPnl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.m_tableLayoutPnl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.m_tableLayoutPnl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.m_tableLayoutPnl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.m_tableLayoutPnl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.m_tableLayoutPnl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.m_tableLayoutPnl.Size = new System.Drawing.Size(605, 270);
            this.m_tableLayoutPnl.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 15.75F);
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(6, 233);
            this.label4.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(166, 24);
            this.label4.TabIndex = 13;
            this.label4.Text = "Manual Take Out";
            // 
            // m_CMDIDTxb
            // 
            this.m_CMDIDTxb.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.m_CMDIDTxb.Font = new System.Drawing.Font("Arial", 15.75F);
            this.m_CMDIDTxb.Location = new System.Drawing.Point(230, 7);
            this.m_CMDIDTxb.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.m_CMDIDTxb.Name = "m_CMDIDTxb";
            this.m_CMDIDTxb.ReadOnly = true;
            this.m_CMDIDTxb.Size = new System.Drawing.Size(369, 32);
            this.m_CMDIDTxb.TabIndex = 12;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 15.75F);
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(6, 10);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(130, 24);
            this.label2.TabIndex = 10;
            this.label2.Text = "Command ID";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // m_sourceTxb
            // 
            this.m_sourceTxb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.m_sourceTxb.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_sourceTxb.Location = new System.Drawing.Point(227, 94);
            this.m_sourceTxb.Name = "m_sourceTxb";
            this.m_sourceTxb.ReadOnly = true;
            this.m_sourceTxb.Size = new System.Drawing.Size(375, 32);
            this.m_sourceTxb.TabIndex = 10;
            // 
            // m_idLbl
            // 
            this.m_idLbl.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.m_idLbl.AutoSize = true;
            this.m_idLbl.Font = new System.Drawing.Font("Arial", 15.75F);
            this.m_idLbl.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_idLbl.Location = new System.Drawing.Point(6, 54);
            this.m_idLbl.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.m_idLbl.Name = "m_idLbl";
            this.m_idLbl.Size = new System.Drawing.Size(102, 24);
            this.m_idLbl.TabIndex = 0;
            this.m_idLbl.Text = "Carrier ID";
            this.m_idLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // m_pwdLbl
            // 
            this.m_pwdLbl.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.m_pwdLbl.AutoSize = true;
            this.m_pwdLbl.Font = new System.Drawing.Font("Arial", 15.75F);
            this.m_pwdLbl.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_pwdLbl.Location = new System.Drawing.Point(6, 142);
            this.m_pwdLbl.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.m_pwdLbl.Name = "m_pwdLbl";
            this.m_pwdLbl.Size = new System.Drawing.Size(100, 24);
            this.m_pwdLbl.TabIndex = 1;
            this.m_pwdLbl.Text = "In Vehicle";
            this.m_pwdLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // m_pwdVerifyLbl
            // 
            this.m_pwdVerifyLbl.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.m_pwdVerifyLbl.AutoSize = true;
            this.m_pwdVerifyLbl.Font = new System.Drawing.Font("Arial", 15.75F);
            this.m_pwdVerifyLbl.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_pwdVerifyLbl.Location = new System.Drawing.Point(6, 186);
            this.m_pwdVerifyLbl.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.m_pwdVerifyLbl.Name = "m_pwdVerifyLbl";
            this.m_pwdVerifyLbl.Size = new System.Drawing.Size(116, 24);
            this.m_pwdVerifyLbl.TabIndex = 2;
            this.m_pwdVerifyLbl.Text = "Destination";
            // 
            // m_CSTIDTxb
            // 
            this.m_CSTIDTxb.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.m_CSTIDTxb.Font = new System.Drawing.Font("Arial", 15.75F);
            this.m_CSTIDTxb.Location = new System.Drawing.Point(230, 51);
            this.m_CSTIDTxb.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.m_CSTIDTxb.Name = "m_CSTIDTxb";
            this.m_CSTIDTxb.ReadOnly = true;
            this.m_CSTIDTxb.Size = new System.Drawing.Size(369, 32);
            this.m_CSTIDTxb.TabIndex = 3;
            // 
            // m_oldpwdLbl
            // 
            this.m_oldpwdLbl.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.m_oldpwdLbl.AutoSize = true;
            this.m_oldpwdLbl.Font = new System.Drawing.Font("Arial", 15.75F);
            this.m_oldpwdLbl.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_oldpwdLbl.Location = new System.Drawing.Point(6, 98);
            this.m_oldpwdLbl.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.m_oldpwdLbl.Name = "m_oldpwdLbl";
            this.m_oldpwdLbl.Size = new System.Drawing.Size(77, 24);
            this.m_oldpwdLbl.TabIndex = 1;
            this.m_oldpwdLbl.Text = "Source";
            this.m_oldpwdLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.cmb_vhLocations, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.cmb_vhIDs, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(227, 135);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(375, 38);
            this.tableLayoutPanel1.TabIndex = 9;
            // 
            // cmb_vhLocations
            // 
            this.cmb_vhLocations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmb_vhLocations.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmb_vhLocations.FormattingEnabled = true;
            this.cmb_vhLocations.Location = new System.Drawing.Point(190, 3);
            this.cmb_vhLocations.Name = "cmb_vhLocations";
            this.cmb_vhLocations.Size = new System.Drawing.Size(182, 32);
            this.cmb_vhLocations.TabIndex = 10;
            // 
            // cmb_vhIDs
            // 
            this.cmb_vhIDs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmb_vhIDs.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmb_vhIDs.FormattingEnabled = true;
            this.cmb_vhIDs.Location = new System.Drawing.Point(3, 3);
            this.cmb_vhIDs.Name = "cmb_vhIDs";
            this.cmb_vhIDs.Size = new System.Drawing.Size(181, 32);
            this.cmb_vhIDs.TabIndex = 9;
            this.cmb_vhIDs.SelectedIndexChanged += new System.EventHandler(this.cmb_vhIDs_SelectedIndexChanged);
            // 
            // m_destTxb
            // 
            this.m_destTxb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.m_destTxb.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_destTxb.Location = new System.Drawing.Point(227, 182);
            this.m_destTxb.Name = "m_destTxb";
            this.m_destTxb.ReadOnly = true;
            this.m_destTxb.Size = new System.Drawing.Size(375, 32);
            this.m_destTxb.TabIndex = 11;
            // 
            // radioBtn_Source
            // 
            this.radioBtn_Source.AutoSize = true;
            this.radioBtn_Source.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioBtn_Source.Location = new System.Drawing.Point(182, 91);
            this.radioBtn_Source.Name = "radioBtn_Source";
            this.radioBtn_Source.Size = new System.Drawing.Size(39, 38);
            this.radioBtn_Source.TabIndex = 7;
            this.radioBtn_Source.TabStop = true;
            this.radioBtn_Source.UseVisualStyleBackColor = true;
            this.radioBtn_Source.Click += new System.EventHandler(this.radioBtn_Click);
            // 
            // radioBtn_InVehicle
            // 
            this.radioBtn_InVehicle.AutoSize = true;
            this.radioBtn_InVehicle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioBtn_InVehicle.Location = new System.Drawing.Point(182, 135);
            this.radioBtn_InVehicle.Name = "radioBtn_InVehicle";
            this.radioBtn_InVehicle.Size = new System.Drawing.Size(39, 38);
            this.radioBtn_InVehicle.TabIndex = 8;
            this.radioBtn_InVehicle.TabStop = true;
            this.radioBtn_InVehicle.UseVisualStyleBackColor = true;
            this.radioBtn_InVehicle.Click += new System.EventHandler(this.radioBtn_Click);
            // 
            // radioBtn_dest
            // 
            this.radioBtn_dest.AutoSize = true;
            this.radioBtn_dest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioBtn_dest.Location = new System.Drawing.Point(182, 179);
            this.radioBtn_dest.Name = "radioBtn_dest";
            this.radioBtn_dest.Size = new System.Drawing.Size(39, 38);
            this.radioBtn_dest.TabIndex = 8;
            this.radioBtn_dest.TabStop = true;
            this.radioBtn_dest.UseVisualStyleBackColor = true;
            this.radioBtn_dest.Click += new System.EventHandler(this.radioBtn_Click);
            // 
            // radioBtn_manual
            // 
            this.radioBtn_manual.AutoSize = true;
            this.radioBtn_manual.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioBtn_manual.Location = new System.Drawing.Point(182, 223);
            this.radioBtn_manual.Name = "radioBtn_manual";
            this.radioBtn_manual.Size = new System.Drawing.Size(39, 44);
            this.radioBtn_manual.TabIndex = 14;
            this.radioBtn_manual.TabStop = true;
            this.radioBtn_manual.UseVisualStyleBackColor = true;
            // 
            // m_confirmBtn
            // 
            this.m_confirmBtn.BackColor = System.Drawing.Color.Transparent;
            this.m_confirmBtn.BaseColor = System.Drawing.Color.Silver;
            this.m_confirmBtn.BorderColor = System.Drawing.Color.Black;
            this.m_confirmBtn.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.m_confirmBtn.DownBack = null;
            this.m_confirmBtn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.m_confirmBtn.Font = new System.Drawing.Font("Arial", 14.25F);
            this.m_confirmBtn.ForeColor = System.Drawing.Color.Black;
            this.m_confirmBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.m_confirmBtn.ImageKey = "confirm.png";
            this.m_confirmBtn.ImageSize = new System.Drawing.Size(24, 24);
            this.m_confirmBtn.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_confirmBtn.Location = new System.Drawing.Point(126, 433);
            this.m_confirmBtn.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.m_confirmBtn.MouseBack = null;
            this.m_confirmBtn.Name = "m_confirmBtn";
            this.m_confirmBtn.NormlBack = null;
            this.m_confirmBtn.RoundStyle = CCWin.SkinClass.RoundStyle.All;
            this.m_confirmBtn.Size = new System.Drawing.Size(115, 40);
            this.m_confirmBtn.TabIndex = 8;
            this.m_confirmBtn.Text = "Confirm";
            this.m_confirmBtn.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.m_confirmBtn.UseVisualStyleBackColor = false;
            // 
            // m_cancelBtn
            // 
            this.m_cancelBtn.BackColor = System.Drawing.Color.Transparent;
            this.m_cancelBtn.BaseColor = System.Drawing.Color.Silver;
            this.m_cancelBtn.BorderColor = System.Drawing.Color.Black;
            this.m_cancelBtn.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.m_cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_cancelBtn.DownBack = null;
            this.m_cancelBtn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.m_cancelBtn.Font = new System.Drawing.Font("Arial", 14.25F);
            this.m_cancelBtn.ForeColor = System.Drawing.Color.Black;
            this.m_cancelBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.m_cancelBtn.ImageKey = "cancel.png";
            this.m_cancelBtn.ImageSize = new System.Drawing.Size(24, 24);
            this.m_cancelBtn.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.m_cancelBtn.Location = new System.Drawing.Point(352, 433);
            this.m_cancelBtn.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.m_cancelBtn.MouseBack = null;
            this.m_cancelBtn.Name = "m_cancelBtn";
            this.m_cancelBtn.NormlBack = null;
            this.m_cancelBtn.RoundStyle = CCWin.SkinClass.RoundStyle.All;
            this.m_cancelBtn.Size = new System.Drawing.Size(115, 40);
            this.m_cancelBtn.TabIndex = 9;
            this.m_cancelBtn.Text = "Cancel";
            this.m_cancelBtn.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.m_cancelBtn.UseVisualStyleBackColor = false;
            this.m_cancelBtn.Click += new System.EventHandler(this.m_cancelBtn_Click);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(149, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(308, 32);
            this.label1.TabIndex = 8;
            this.label1.Text = "Carrier Current Location";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmd_force_finish_status
            // 
            this.cmd_force_finish_status.Enabled = false;
            this.cmd_force_finish_status.FormattingEnabled = true;
            this.cmd_force_finish_status.Location = new System.Drawing.Point(207, 339);
            this.cmd_force_finish_status.Name = "cmd_force_finish_status";
            this.cmd_force_finish_status.Size = new System.Drawing.Size(302, 29);
            this.cmd_force_finish_status.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 15.75F);
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(15, 340);
            this.label3.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(183, 24);
            this.label3.TabIndex = 13;
            this.label3.Text = "Force finish status";
            // 
            // CarrierLocationChooseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(643, 486);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmd_force_finish_status);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.m_cancelBtn);
            this.Controls.Add(this.m_confirmBtn);
            this.Controls.Add(this.m_tableLayoutPnl);
            this.Font = new System.Drawing.Font("新細明體", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.Name = "CarrierLocationChooseForm";
            this.Text = "CarrierLocationChooseForm";
            this.Load += new System.EventHandler(this.CarrierLocationChooseForm_Load);
            this.m_tableLayoutPnl.ResumeLayout(false);
            this.m_tableLayoutPnl.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel m_tableLayoutPnl;
        private System.Windows.Forms.Label m_idLbl;
        private System.Windows.Forms.Label m_pwdLbl;
        private System.Windows.Forms.Label m_pwdVerifyLbl;
        private System.Windows.Forms.TextBox m_CSTIDTxb;
        private System.Windows.Forms.Label m_oldpwdLbl;
        private CCWin.SkinControl.SkinButton m_confirmBtn;
        private CCWin.SkinControl.SkinButton m_cancelBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton radioBtn_Source;
        private System.Windows.Forms.RadioButton radioBtn_dest;
        private System.Windows.Forms.RadioButton radioBtn_InVehicle;
        private System.Windows.Forms.ComboBox cmb_vhIDs;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ComboBox cmb_vhLocations;
        private System.Windows.Forms.TextBox m_sourceTxb;
        private System.Windows.Forms.TextBox m_destTxb;
        private System.Windows.Forms.TextBox m_CMDIDTxb;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmd_force_finish_status;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RadioButton radioBtn_manual;
    }
}