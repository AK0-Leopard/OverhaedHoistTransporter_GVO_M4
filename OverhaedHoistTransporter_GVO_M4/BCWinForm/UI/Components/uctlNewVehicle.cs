﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using com.mirle.ibg3k0.bc.winform.Properties;
using com.mirle.ibg3k0.sc;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using com.mirle.ibg3k0.bcf.Common;
using NLog;
using System.Diagnostics;
using com.mirle.ibg3k0.sc.Common;
using System.Collections.Concurrent;

namespace com.mirle.ibg3k0.bc.winform.UI.Components
{


    public partial class uctlNewVehicle : UserControl
    {
        #region Enum
        public enum E_VEHICLE_STATUS
        {
            UNCONNECTIED,
            POWERON,
            MANUAL,
            AUTOLOCAL,
            AUTOMTL,
            AUTOCHARGING,
            AUTOLONGCHARGING,
            AUTOREMOTE
        }

        public enum E_ALERT_STATUS
        {
            NOTHING,
            WARNING,
            ALERT,
            ERROR,
            OBS,
            HID,
            PAUSE,
            BLOCK,
            PAUSE_SAFETY,
            PAUSE_EARTHQUAKE,
            RESERVE,
            OP_PAUSE
        }
        public enum E_ACTION_STATUS
        {
            NOTHING,
            ONLYMOVE,
            PARKED,
            PARKING,
            TRAINING,
            MAINTENANCE
        }
        public enum E_SPEED_STATUS
        {
            SLOW,
            MEDIUM,
            FAST
        }
        public enum E_CST_LOAD_STATUS
        {
            NOTHING,
            GOLOAD,
            LOADED,
            LOADING,
            UNLOADING
        }
        #endregion Enum

        #region Const
        const int SPEED_INTERVAL_MM_SLOW = 20;
        const int SPEED_INTERVAL_MM_MEDIUM = 40;
        #endregion Const

        #region Parameter
        private static Logger logger = LogManager.GetCurrentClassLogger();


        private uctl_Map Uctl_Map;
        private AVEHICLE vh;
        private PictureBox PicAlarmStatus;
        private PictureBox PicCSTLoadStatus_R;
        private PictureBox PicCSTLoadStatus_L;


        private Image ImgVehicleStatus = Resources.Vehicle__Unconnected_;
        private E_VEHICLE_STATUS vehicleStatus;
        public E_VEHICLE_STATUS VehicleStatus
        {
            get { return vehicleStatus; }
            private set
            {
                if (vehicleStatus != value)
                {
                    vehicleStatus = value;
                    switch (value)
                    {
                        case E_VEHICLE_STATUS.UNCONNECTIED: ImgVehicleStatus = Resources.Vehicle__Unconnected_; break;
                        case E_VEHICLE_STATUS.POWERON: ImgVehicleStatus = Resources.Vehicle__Power_on_; break;
                        case E_VEHICLE_STATUS.MANUAL: ImgVehicleStatus = Resources.Vehicle__Manual_; break;
                        case E_VEHICLE_STATUS.AUTOLOCAL: ImgVehicleStatus = Resources.Vehicle__Auto_Local_; break;
                        case E_VEHICLE_STATUS.AUTOMTL: ImgVehicleStatus = Resources.Vehicle__Auto_Local_; break;
                        case E_VEHICLE_STATUS.AUTOCHARGING: ImgVehicleStatus = Resources.Vehicle__Auto_Local_; break;
                        case E_VEHICLE_STATUS.AUTOLONGCHARGING: ImgVehicleStatus = Resources.Vehicle__Power_on_; break;//暫時用Power On的圖示，代表長充
                        case E_VEHICLE_STATUS.AUTOREMOTE: ImgVehicleStatus = Resources.Vehicle__Auto_Remote_; break;
                    }
                    pic_VhStatus.Image = ImgVehicleStatus;
                }
            }
        }

        private Image ImgAlertStatus;
        private E_ALERT_STATUS alertStatus;
        public E_ALERT_STATUS AlertStatus
        {
            get { return alertStatus; }
            private set
            {
                alertStatus = value;
                switch (value)
                {
                    case E_ALERT_STATUS.NOTHING: ImgAlertStatus = null; break;
                    case E_ALERT_STATUS.ERROR: ImgAlertStatus = Resources.Alarm__Error_; break;
                    case E_ALERT_STATUS.ALERT: ImgAlertStatus = Resources.Alarm__Alert_; break;
                    case E_ALERT_STATUS.WARNING: ImgAlertStatus = Resources.Alarm__Warning_; break;
                    case E_ALERT_STATUS.BLOCK: ImgAlertStatus = Resources.Pause__Block_; break;
                    case E_ALERT_STATUS.OBS: ImgAlertStatus = Resources.Pause__Obstructed_; break;
                    case E_ALERT_STATUS.HID: ImgAlertStatus = Resources.Pause__HID_; break;
                    case E_ALERT_STATUS.PAUSE: ImgAlertStatus = Resources.Pause__Pause_; break;
                    case E_ALERT_STATUS.PAUSE_SAFETY: ImgAlertStatus = Resources.Pause__Safety_; break;
                    case E_ALERT_STATUS.PAUSE_EARTHQUAKE: ImgAlertStatus = Resources.Pause__Earthquake_; break;
                    case E_ALERT_STATUS.RESERVE: ImgAlertStatus = Resources.Pause__Block_; break;
                    case E_ALERT_STATUS.OP_PAUSE: ImgAlertStatus = Resources.Action__Parked_; break;
                }
            }
        }

        private Image ImgActionStatus;
        private E_ACTION_STATUS actionStatus;
        public E_ACTION_STATUS ActionStatus
        {
            get { return actionStatus; }
            private set
            {
                actionStatus = value;
                switch (value)
                {
                    case E_ACTION_STATUS.NOTHING: ImgActionStatus = null; break;
                    case E_ACTION_STATUS.ONLYMOVE: ImgActionStatus = Resources.Action__Moving_; break;
                    case E_ACTION_STATUS.PARKED: ImgActionStatus = Resources.Action__Parked_; break;
                    case E_ACTION_STATUS.PARKING: ImgActionStatus = Resources.Action__Parking_; break;
                    case E_ACTION_STATUS.TRAINING: ImgActionStatus = Resources.Action__Correcting_action_; break;
                    case E_ACTION_STATUS.MAINTENANCE: ImgActionStatus = Resources.Action__Maintenance_; break;
                }
            }
        }

        private Image ImgSpeedStatus = Resources.Speed__Slow_;
        private E_SPEED_STATUS speedStatus;
        public E_SPEED_STATUS SpeedStatus
        {
            get { return speedStatus; }
            private set
            {
                speedStatus = value;
                switch (value)
                {
                    case E_SPEED_STATUS.SLOW: ImgSpeedStatus = Resources.Speed__Slow_; break;
                    case E_SPEED_STATUS.MEDIUM: ImgSpeedStatus = Resources.Speed__Medium_; break;
                    case E_SPEED_STATUS.FAST: ImgSpeedStatus = Resources.Speed__Fast_; break;
                }
            }
        }


        private Image ImgCSTLoadStatus_R = null;
        private E_CST_LOAD_STATUS cstLoadStatus_R;
        public E_CST_LOAD_STATUS CSTLoadStatus_R
        {
            get { return cstLoadStatus_R; }
            private set
            {
                cstLoadStatus_R = value;
                switch (value)
                {
                    case E_CST_LOAD_STATUS.NOTHING: ImgCSTLoadStatus_R = null; break;
                    case E_CST_LOAD_STATUS.LOADED: ImgCSTLoadStatus_R = Resources.Action__Cassette_; break;
                    case E_CST_LOAD_STATUS.LOADING: ImgCSTLoadStatus_R = Resources.Action__Loading_; break;
                    case E_CST_LOAD_STATUS.UNLOADING: ImgCSTLoadStatus_R = Resources.Action__Unloading_; break;
                    case E_CST_LOAD_STATUS.GOLOAD: ImgCSTLoadStatus_R = Resources.Action__Receive_command_; break;
                }
                Adapter.Invoke((obj) =>
                {
                    if (ImgCSTLoadStatus_R != null)
                    {
                        PicCSTLoadStatus_R.Image = ImgCSTLoadStatus_R;
                        PicCSTLoadStatus_R.Visible = true;
                    }
                    else
                    {
                        PicCSTLoadStatus_R.Visible = false;
                    }
                }, null);
            }
        }

        private Image ImgCSTLoadStatus_L = null;
        private E_CST_LOAD_STATUS cstLoadStatus_L;
        public E_CST_LOAD_STATUS CSTLoadStatus_L
        {
            get { return cstLoadStatus_L; }
            private set
            {
                cstLoadStatus_L = value;
                switch (value)
                {
                    case E_CST_LOAD_STATUS.NOTHING: ImgCSTLoadStatus_L = null; break;
                    case E_CST_LOAD_STATUS.LOADED: ImgCSTLoadStatus_L = Resources.Action__Cassette_; break;
                    case E_CST_LOAD_STATUS.LOADING: ImgCSTLoadStatus_L = Resources.Action__Loading_; break;
                    case E_CST_LOAD_STATUS.UNLOADING: ImgCSTLoadStatus_L = Resources.Action__Unloading_; break;
                    case E_CST_LOAD_STATUS.GOLOAD: ImgCSTLoadStatus_L = Resources.Action__Receive_command_; break;
                }
                Adapter.Invoke((obj) =>
                {
                    if (ImgCSTLoadStatus_L != null)
                    {
                        PicCSTLoadStatus_L.Image = ImgCSTLoadStatus_L;
                        PicCSTLoadStatus_L.Visible = true;
                    }
                    else
                    {
                        PicCSTLoadStatus_L.Visible = false;
                    }
                }, null);
            }
        }

        private int currentSpeed;
        private int CurrentSpeed
        {
            get { return currentSpeed; }
            set
            {
                currentSpeed = value;
                sCurrentSpeed = currentSpeed.ToString("000");
                if (value < SPEED_INTERVAL_MM_SLOW)
                {
                    SpeedStatus = E_SPEED_STATUS.SLOW;
                }
                else if (SPEED_INTERVAL_MM_SLOW < value && value < SPEED_INTERVAL_MM_MEDIUM)
                {
                    SpeedStatus = E_SPEED_STATUS.MEDIUM;
                }
                else
                {
                    SpeedStatus = E_SPEED_STATUS.FAST;
                }
            }
        }
        public string sCurrentSpeed = "000";

        private int num = 0;
        public int Num
        {
            get { return (num); }
            set
            {
                num = value;
                sNum = num.ToString("00");
            }
        }
        public string sNum = "00";

        public string CurrentSecID { get; set; } = "";
        #endregion Parameter


        public uctlNewVehicle()
        {
            InitializeComponent();
            //  this.Size = Resources.Vehicle__Unconnected_.Size;
        }
        public uctlNewVehicle(AVEHICLE _vh, uctl_Map uctl_Map, PictureBox alarmStatus, PictureBox cstloadStatus_R, PictureBox cstloadStatus_L)
        {
            InitializeComponent();
            //  this.Size = Resources.Vehicle__Unconnected_.Size;
            Uctl_Map = uctl_Map;
            vh = _vh;

            this.Width = this.Width / icon_scale;
            this.Height = this.Height / icon_scale;

            //this.Left = this.Width / 2;
            //this.Top = this.Height / 2;


            PicAlarmStatus = alarmStatus;
            PicCSTLoadStatus_R = cstloadStatus_R;
            PicCSTLoadStatus_L = cstloadStatus_L;
            PicAlarmStatus.Size =
            new Size(Resources.Alarm__Error_.Width / icon_scale, Resources.Alarm__Error_.Height / icon_scale);
            PicCSTLoadStatus_R.Size =
                new Size((Resources.Action__Cassette_.Size.Width / icon_scale) / 2, Resources.Action__Cassette_.Size.Height / icon_scale);
            PicCSTLoadStatus_L.Size =
                new Size((Resources.Action__Cassette_.Size.Width / icon_scale) / 2, Resources.Action__Cassette_.Size.Height / icon_scale);

            font = new System.Drawing.Font("Consolas", 20F / icon_scale, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            font_Numbering = new System.Drawing.Font("Arial", 28F / icon_scale, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            PicAlarmStatus.VisibleChanged += PicAlarmStatus_VisibleChanged;
            PicCSTLoadStatus_R.VisibleChanged += PicCSTLoadStatus_VisibleChanged;
            PicCSTLoadStatus_L.VisibleChanged += PicCSTLoadStatus_VisibleChanged;
            this.BackColor = Color.FromArgb(29, 36, 60);
            registerEvent();
            _SetInitialVhToolTip();
            _SetRailToolTip();
        }

        private void PicCSTLoadStatus_VisibleChanged(object sender, EventArgs e)
        {
            refreshCSTIconPosition();
        }


        private void PicAlarmStatus_VisibleChanged(object sender, EventArgs e)
        {
            if ((sender as PictureBox).Visible)
            {
                //PicAlarmStatus.Left = this.Left + (this.Width / 2) - (PicAlarmStatus.Width / 2);
                //PicAlarmStatus.Top = this.Top - PicAlarmStatus.Height - 7;
            }
        }

        public void turnOnMonitorVh()
        {
            if (!SCUtility.isEmpty(vh.CUR_ADR_ID))
            {
                updateVehicleModeStatus();
                updateVehicleActionStatus();
                updateVehiclePosition();
            }
        }

        #region Proc
        string event_id = string.Empty;
        private void registerEvent()
        {
            event_id = this.Name + Num;
            //vh.addEventHandler(event_id
            //                    , nameof(vh.isTcpIpConnect)
            //                    , (s1, e1) =>
            //                    {
            //                        updateVehicleModeStatus();
            //                        updateVehicleActionStatus();
            //                        Adapter.Invoke((obj) =>
            //                        {
            //                            pic_VhStatus.Refresh();
            //                        }, null);
            //                    });
            //vh.addEventHandler(event_id
            //                    , nameof(vh.VhPositionChangeEvent)
            //                    , (s1, e1) =>
            //                    {
            //                        updateVehiclePosition();
            //                    });
            //vh.addEventHandler(event_id
            //                    , nameof(vh.VhStatusChangeEvent)
            //                    , (s1, e1) =>
            //                    {
            //                        updateVehicleModeStatus();
            //                        updateVehicleActionStatus();
            //                        Adapter.Invoke((obj) =>
            //                        {
            //                            pic_VhStatus.Refresh();
            //                        }, null);
            //                    });
            vh.ConnectionStatusChange += Vh_ConnectionStatusChange;
            vh.VehicleStatusChange += Vh_VehicleStatusChange;
            vh.VehiclePositionChange += Vh_VehiclePositionChange;
            //vh.addEventHandler(event_id
            //                    , nameof(vh.procProgress_Percen)
            //                    , (s1, e1) => { updateVhCurrentProcProgress(); });
        }

        private void Vh_ConnectionStatusChange(object sender, bool e)
        {
            updateVehicleModeStatus();
            updateVehicleActionStatus();
            Adapter.Invoke((obj) =>
            {
                pic_VhStatus.Refresh();
            }, null);
        }

        private void Vh_VehiclePositionChange(object sender, EventArgs e)
        {
            updateVehiclePosition();
        }

        private void Vh_VehicleStatusChange(object sender, EventArgs e)
        {
            updateVehicleModeStatus();
            updateVehicleActionStatus();
            Adapter.Invoke((obj) =>
            {
                pic_VhStatus.Refresh();
            }, null);
        }

        private void removeEvent()
        {
            vh.removeEventHandler(event_id);
        }
        #endregion Proc

        private void updateVehicleModeStatus()
        {
            if (!vh.isTcpIpConnect)
            {
                VehicleStatus = E_VEHICLE_STATUS.UNCONNECTIED;
            }
            else
            {
                switch (vh.MODE_STATUS)
                {
                    case VHModeStatus.Manual:
                        VehicleStatus = E_VEHICLE_STATUS.MANUAL;
                        break;
                    case VHModeStatus.AutoLocal:
                        VehicleStatus = E_VEHICLE_STATUS.AUTOLOCAL;
                        break;
                    case VHModeStatus.AutoMtl:
                        VehicleStatus = E_VEHICLE_STATUS.AUTOMTL;
                        break;

                    //case VHModeStatus.AutoCharging:
                    //    if (vh.IsNeedToLongCharge())
                    //    {
                    //        VehicleStatus = E_VEHICLE_STATUS.AUTOLONGCHARGING;
                    //    }
                    //    else
                    //    {
                    //        VehicleStatus = E_VEHICLE_STATUS.AUTOCHARGING;
                    //    }
                    //    break;
                    case VHModeStatus.AutoRemote:
                        VehicleStatus = E_VEHICLE_STATUS.AUTOREMOTE;
                        break;
                    default:
                        VehicleStatus = E_VEHICLE_STATUS.MANUAL;
                        break;
                }
            }
        }

        private void updateVehicleActionStatus()
        {

            RefeshAlertStatus();


            if (!vh.isTcpIpConnect)
            {
                VehicleStatus = E_VEHICLE_STATUS.UNCONNECTIED;
            }
            else
            {
                switch (vh.ACT_STATUS)
                {
                    case VHActionStatus.NoCommand:
                        {
                            //CSTLoadStatus = E_CST_LOAD_STATUS.NOTHING;
                            ActionStatus = E_ACTION_STATUS.NOTHING;
                            if (vh.HAS_CST)
                            {
                                CSTLoadStatus_L = E_CST_LOAD_STATUS.LOADED;
                            }
                            else
                            {
                                CSTLoadStatus_L = E_CST_LOAD_STATUS.NOTHING;
                            }
                            //if (vh.HAS_CST_R) //todo kevin 要再多一個欄位來顯示
                            //{
                            //    CSTLoadStatus_R = E_CST_LOAD_STATUS.LOADED;
                            //}
                            //else
                            //{
                            //    CSTLoadStatus_R = E_CST_LOAD_STATUS.NOTHING;
                            //}
                        }
                        break;
                    case VHActionStatus.Teaching:
                        {
                            CSTLoadStatus_R = E_CST_LOAD_STATUS.NOTHING;
                            ActionStatus = E_ACTION_STATUS.TRAINING;
                        }
                        break;
                    case VHActionStatus.Commanding:


                        bool isDisplayAlertIcon = true;
                        if (ImgAlertStatus != null)
                        {
                            PicAlarmStatus.Image = ImgAlertStatus;
                            isDisplayAlertIcon = true;
                        }
                        else
                        {
                            isDisplayAlertIcon = false;
                        }
                        Adapter.Invoke((obj) =>
                        {
                            PicAlarmStatus.Visible = isDisplayAlertIcon;
                        }, null);

                        if (vh.HAS_CST)
                        {
                            CSTLoadStatus_L = E_CST_LOAD_STATUS.LOADED;
                        }
                        else
                        {
                            CSTLoadStatus_L = E_CST_LOAD_STATUS.NOTHING;
                        }
                        //if (vh.HAS_CST_R) //todo kevin 要再多一個欄位來顯示
                        //{
                        //    CSTLoadStatus_R = E_CST_LOAD_STATUS.LOADED;
                        //}
                        //else
                        //{
                        //    CSTLoadStatus_R = E_CST_LOAD_STATUS.NOTHING;
                        //}

                        if (vh.MODE_STATUS == VHModeStatus.AutoLocal)
                        {
                            ActionStatus = E_ACTION_STATUS.MAINTENANCE;
                        }
                        else
                        {
                            switch (vh.CmdType)
                            {
                                case E_CMD_TYPE.Move:
                                    ActionStatus = E_ACTION_STATUS.ONLYMOVE;
                                    break;
                                default:
                                    ActionStatus = E_ACTION_STATUS.NOTHING;
                                    break;

                            }

                            ActionStatus = E_ACTION_STATUS.NOTHING;
                        }



                        break;
                    case VHActionStatus.GripperTeaching:
                        //TODO 是不是GripperTeaching也要有自己的圖片?
                        break;
                    case VHActionStatus.CycleRun:
                        //TODO 是不是CycleRun也要有自己的圖片?
                        break;
                }
            }
        }

        private void RefeshAlertStatus()
        {
            if (vh.IsError)
            {
                AlertStatus = E_ALERT_STATUS.ERROR;
            }
            else if (vh.IsBlocking)
            {
                AlertStatus = E_ALERT_STATUS.BLOCK;
            }
            else if (vh.IsHIDPause)
            {
                AlertStatus = E_ALERT_STATUS.HID;
            }
            else if (vh.IsObstacle)
            {
                AlertStatus = E_ALERT_STATUS.OBS;
            }
            else if (vh.IsPause)
            {
                AlertStatus = E_ALERT_STATUS.PAUSE;
            }
            else if (vh.IsReservePause)
            {
                AlertStatus = E_ALERT_STATUS.RESERVE;
            }
            //else if (vh.IsOPPause)
            //{
            //    AlertStatus = E_ALERT_STATUS.OP_PAUSE;
            //}
            else
            {
                AlertStatus = E_ALERT_STATUS.NOTHING;
            }

            bool isDisplayAlertIcon = true;
            if (ImgAlertStatus != null)
            {
                PicAlarmStatus.Image = ImgAlertStatus;
                isDisplayAlertIcon = true;
            }
            else
            {
                isDisplayAlertIcon = false;
            }
            Adapter.Invoke((obj) =>
            {
                PicAlarmStatus.Visible = isDisplayAlertIcon;
            }, null);

        }

        public const int UNKNOW_DEFAULT_X_LOCATION_VALUE = 50;
        public const int UNKNOW_DEFAULT_Y_LOCATION_VALUE = 30;
        private void updateVehiclePosition()
        {

            Adapter.Invoke((obj) =>
            {
                try
                {
                    //如果車子目前是位於9開頭的Address 就把他換成用Address的座標來定位
                    if (vh.CUR_ADR_ID != null && vh.CUR_ADR_ID.StartsWith("9"))
                    {
                        string replaced_cur_adr = replaceFirstChar(vh.CUR_ADR_ID);
                        uctlAddress uctlAdr = Uctl_Map.getuctAddressByAdrID(replaced_cur_adr);
                        if (uctlAdr == null) return;
                        PrcSetLocation(uctlAdr.p_LocX, uctlAdr.p_LocY);
                    }
                    else
                    {
                        GroupRails groupRails = Uctl_Map.getGroupRailBySecID(vh.CUR_SEC_ID);
                        if (groupRails != null)
                        {
                            groupRails.VehicleEnterSection(this, vh.CUR_ADR_ID, vh.ACC_SEC_DIST, vh.CurrentDriveDirction);
                        }
                        else
                        {
                            PrcSetLocation((UNKNOW_DEFAULT_X_LOCATION_VALUE * Num) + 2, UNKNOW_DEFAULT_Y_LOCATION_VALUE);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                }
            }, null);
        }

        private string replaceFirstChar(string curAdrID)
        {
            string replaced_cur_adr = curAdrID;
            try
            {
                replaced_cur_adr = $"1{replaced_cur_adr.Substring(1, curAdrID.Length - 1)}";
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            return replaced_cur_adr;
        }

        public void PrcSetLocation(int iLocX, int iLocY)
        {
            this.Left = iLocX - (this.Width / 2);
            this.Top = iLocY - (this.Height + 5);
            refreshCSTIconPosition();
        }

        private void refreshCSTIconPosition()
        {
            //if (PicCSTLoadStatus.Visible)
            //{
            if (PicCSTLoadStatus_L != null)
            {
                PicCSTLoadStatus_L.Left = this.Left + 10;
                PicCSTLoadStatus_L.Top = this.Top - (PicCSTLoadStatus_L.Height / 3);
            }
            if (PicCSTLoadStatus_R != null)
            {
                PicCSTLoadStatus_R.Left = this.Left + 25;
                PicCSTLoadStatus_R.Top = this.Top - (PicCSTLoadStatus_R.Height / 3);
            }
            if (PicAlarmStatus != null)
            {
                PicAlarmStatus.Left = this.Left + (this.Width / 2) - (PicAlarmStatus.Width / 2);
                PicAlarmStatus.Top = this.Top - PicAlarmStatus.Height - 7;
            }
            //}
        }

        #region DrawImage

        static SolidBrush white_objBrush = new SolidBrush(Color.White);
        static SolidBrush black_objBrush = new SolidBrush(Color.Black);
        static Font font = new System.Drawing.Font("Consolas", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        static Font font_Numbering = new System.Drawing.Font("Arial", 28F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        static ConcurrentDictionary<string, RectangleF> dicRectagles = new ConcurrentDictionary<string, RectangleF>();
        string RECTANGLE_KEY_SPEEDSTATUS = "SPEED_STATUS";
        string RECTANGLE_KEY_ACTIONSTATS = "ACTION_STATUS";
#pragma warning disable CS0414 // 已指派欄位 'uctlNewVehicle.RECTANGLE_KEY_CSTLOADSTATIUS'，但從未使用過其值。
        string RECTANGLE_KEY_CSTLOADSTATIUS = "CST_LOAD_STATIUS";
#pragma warning restore CS0414 // 已指派欄位 'uctlNewVehicle.RECTANGLE_KEY_CSTLOADSTATIUS'，但從未使用過其值。
        string RECTANGLE_KEY_CURSPEED = "CUR_SPEED";
        string RECTANGLE_KEY_NUMBERING = "NUMBERING";
        int icon_scale = 3;
        private void pic_VhStatus_Paint(object sender, PaintEventArgs e)
        {

            Graphics g = e.Graphics;
            //pic_VhStatus.Image = ImgVehicleStatus;
            setVehicleNumbering(g);

            g.DrawImage(ImgSpeedStatus,
                GetRectangle(RECTANGLE_KEY_SPEEDSTATUS, new Rectangle(0, this.pic_VhStatus.Height - (ImgSpeedStatus.Size.Height / icon_scale),
                                                                    ImgSpeedStatus.Size.Height / icon_scale,
                                                                    ImgSpeedStatus.Size.Width / icon_scale)));

            setVhCurrentSpeed(g);

            if (ImgActionStatus != null)
                g.DrawImage(ImgActionStatus,
                    GetRectangle(RECTANGLE_KEY_ACTIONSTATS, new Rectangle((this.Width / 2) - ((Resources.Action__Parked_.Width / icon_scale) / 2), 0,
                                                                        Resources.Action__Parked_.Size.Height / icon_scale,
                                                                        Resources.Action__Parked_.Size.Width / icon_scale)));
            //else if (ImgCSTLoadStatus != null)
            //    g.DrawImage(ImgCSTLoadStatus,
            //        GetRectangle(RECTANGLE_KEY_CSTLOADSTATIUS, new Rectangle((this.Width / 2) - 17, 0,
            //                                                                ImgCSTLoadStatus.Size.Height / icon_scale,
            //                                                                ImgCSTLoadStatus.Size.Width / icon_scale)));
        }

        private void setVhCurrentSpeed(Graphics g)
        {
            var font_size = g.MeasureString(sCurrentSpeed, font, int.MaxValue);
            float speed_status_height = this.pic_VhStatus.Height - (ImgSpeedStatus.Size.Height / icon_scale);
            float crt_speed_height = speed_status_height + ((ImgSpeedStatus.Size.Height / icon_scale) / 2) - (font_size.Height / 2);
            g.DrawString(sCurrentSpeed, font, white_objBrush, GetRectangle(RECTANGLE_KEY_CURSPEED, new RectangleF(0, crt_speed_height,
                                                                                                                  font_size.Width, font_size.Height)));
        }
        private void setVehicleNumbering(Graphics g)
        {
            SolidBrush objBrush = null;
            if (VehicleStatus == E_VEHICLE_STATUS.POWERON)
                objBrush = black_objBrush;
            else
                objBrush = white_objBrush;
            var num_font_size = g.MeasureString(sNum, font_Numbering, int.MaxValue);
            g.DrawString(sNum, font_Numbering, objBrush, GetRectangle(RECTANGLE_KEY_NUMBERING, new RectangleF(pic_VhStatus.Width - (num_font_size.Width), pic_VhStatus.Height - num_font_size.Height - 3,
                                                                                                              num_font_size.Width, num_font_size.Height)));
        }

        private RectangleF GetRectangle(string key, RectangleF value)
        {
            return dicRectagles.GetOrAdd(key, value);
        }

        #endregion DrawImage

        private void _SetRailToolTip()
        {
            this.ToolTip.SetToolTip(this.pic_VhStatus,
                        "Current Adr : " + SCUtility.Trim(vh.CUR_ADR_ID) + "\r\n" +
                        "Current SEC : " + SCUtility.Trim(vh.CUR_SEC_ID) + "\r\n" +
                        "Action : " + vh.ACT_STATUS.ToString());
        }
        private void _SetInitialVhToolTip()
        {
            this.ToolTip.AutoPopDelay = 30000;
            this.ToolTip.ForeColor = Color.Black;
            this.ToolTip.BackColor = Color.White;
            this.ToolTip.ShowAlways = true;
            this.ToolTip.UseAnimation = false;
            this.ToolTip.UseFading = false;

            this.ToolTip.InitialDelay = 100;
            this.ToolTip.ReshowDelay = 100;
        }
        private long syncPoint = 0;
        private void ToolTip_Popup(object sender, PopupEventArgs e)
        {
            if (System.Threading.Interlocked.Exchange(ref syncPoint, 1) == 0)
            {
                try
                {
                    _SetRailToolTip();
                }
                finally
                {
                    System.Threading.Interlocked.Exchange(ref syncPoint, 0);
                }
            }
        }

        private void pic_VhStatus_Click(object sender, EventArgs e)
        {
            Uctl_Map.ohtc_Form.setMonitorVehicle(vh.VEHICLE_ID);
        }
    }
}