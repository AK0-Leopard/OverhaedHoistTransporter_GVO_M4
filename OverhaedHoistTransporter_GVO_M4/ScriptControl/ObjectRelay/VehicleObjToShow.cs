using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.sc;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.ObjectRelay
{
    public class VehicleObjToShow : INotifyPropertyChanged
    {
        double distance2millimeter = 1000;
        double km2millimeter = 1000000;


        //public static BindingList<VehicleObjToShow> ObjectToShow_list = new BindingList<VehicleObjToShow>();
        AVEHICLE vehicle = null;
        public VehicleObjToShow(AVEHICLE myDatabaseObject, double distance_scale)
        {
            this.vehicle = myDatabaseObject;
            distance2millimeter = distance_scale;
        }
        [DisplayName("Vh ID")]
        public string VEHICLE_ID
        {
            get { return vehicle.VEHICLE_ID; }
            set
            {
                this.vehicle.VEHICLE_ID = value;
                NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.VEHICLE_ID));
            }
        }
        //[DisplayName("Address ID")]
        //public string cUR_ADR_ID
        //{
        //    get { return vehicle.CUR_ADR_ID; }
        //    set
        //    {
        //        this.vehicle.CUR_ADR_ID = value;
        //        NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.cUR_ADR_ID));
        //    }
        //}
        //[DisplayName("Section ID")]
        //public string cUR_SEC_ID
        //{
        //    get { return vehicle.CUR_SEC_ID; }
        //    set
        //    {
        //        this.vehicle.CUR_SEC_ID = value;
        //        NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.cUR_SEC_ID));
        //    }
        //}



        [DisplayName("Mode")]
        public VHModeStatus MODE_STATUS
        {
            get { return vehicle.MODE_STATUS; }
            set
            {
                vehicle.MODE_STATUS = value;
                NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.MODE_STATUS));
            }
        }

        [DisplayName("Action")]
        public VHActionStatus ACT_STATUS
        {
            get { return vehicle.ACT_STATUS; }
            set
            {
                vehicle.ACT_STATUS = value;
                NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.ACT_STATUS));
            }
        }
        [DisplayName("Transfer cmd")]
        public string TRANSFER_ID
        {
            get
            {
                return vehicle.TRANSFER_ID == null ?
                    string.Empty : vehicle.TRANSFER_ID.Trim();
            }
            set
            {
                vehicle.TRANSFER_ID = value;
                NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.TRANSFER_ID));
            }
        }
        //[DisplayName("Transfer cmd 2")]
        //public string TRANSFER_ID_2
        //{
        //    get
        //    {
        //        return vehicle.TRANSFER_ID == null ?
        //            string.Empty : vehicle.TRANSFER_ID.Trim();
        //    }
        //    set
        //    {
        //        vehicle.TRANSFER_ID = value;
        //        NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.TRANSFER_ID_2));
        //    }
        //}
        //[DisplayName("Transfer cmd 3")]
        //public string TRANSFER_ID_3
        //{
        //    get
        //    {
        //        return vehicle.TRANSFER_ID == null ?
        //            string.Empty : vehicle.TRANSFER_ID.Trim();
        //    }
        //    set
        //    {
        //        vehicle.TRANSFER_ID = value;
        //        NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.TRANSFER_ID_3));
        //    }
        //}
        //[DisplayName("Transfer cmd 4")]
        //public string TRANSFER_ID_4
        //{
        //    get
        //    {
        //        return vehicle.TRANSFER_ID == null ?
        //            string.Empty : vehicle.TRANSFER_ID.Trim();
        //    }
        //    set
        //    {
        //        vehicle.TRANSFER_ID = value;
        //        NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.TRANSFER_ID_4));
        //    }
        //}
        [DisplayName("Command ID")]
        public string CMD_ID_1
        {
            get
            {
                return vehicle.CMD_ID == null ?
                    string.Empty : vehicle.CMD_ID.Trim();
            }
            set
            {
                vehicle.CMD_ID = value;
                NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.CMD_ID_1));
            }
        }
        //[DisplayName("Commnad ID 2")]
        //public string CMD_ID_2
        //{
        //    get
        //    {
        //        return vehicle.CMD_ID == null ?
        //            string.Empty : vehicle.CMD_ID.Trim();
        //    }
        //    set
        //    {
        //        vehicle.CMD_ID = value;
        //        NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.CMD_ID_2));
        //    }
        //}
        //[DisplayName("Commnad ID 3")]
        //public string CMD_ID_3
        //{
        //    get
        //    {
        //        return vehicle.CMD_ID == null ?
        //            string.Empty : vehicle.CMD_ID.Trim();
        //    }
        //    set
        //    {
        //        vehicle.CMD_ID = value;
        //        NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.CMD_ID_3));
        //    }
        //}
        //[DisplayName("Commnad ID 4")]
        //public string CMD_ID_4
        //{
        //    get
        //    {
        //        return vehicle.CMD_ID == null ?
        //            string.Empty : vehicle.CMD_ID.Trim();
        //    }
        //    set
        //    {
        //        vehicle.CMD_ID = value;
        //        NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.CMD_ID_4));
        //    }
        //}
        //[DisplayName("CST ID")]
        //public string cST_ID
        //{
        //    get { return vehicle.CST_ID; }
        //    set
        //    {
        //        vehicle.CST_ID = value;
        //        NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.cST_ID));
        //    }
        //}

        [DisplayName("Block pause")]
        public bool bLOCK_PAUSE2Show
        {
            get
            {
                return vehicle.BLOCK_PAUSE == VhStopSingle.StopSingleOn;
            }
        }
        [Browsable(false)]
        public VhStopSingle BLOCK_PAUSE
        {
            set
            {
                vehicle.BLOCK_PAUSE = value;
                NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.bLOCK_PAUSE2Show));
            }
        }

        [DisplayName("CMD pause")]
        public bool cMD_PAUSE2Show
        {
            get
            {
                return vehicle.CMD_PAUSE == VhStopSingle.StopSingleOn;
            }
        }

        [Browsable(false)]
        public VhStopSingle CMD_PAUSE
        {
            set
            {
                vehicle.CMD_PAUSE = value;
                NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.cMD_PAUSE2Show));
            }
        }

        [DisplayName("OBS pause")]
        public bool oBS_PAUSE2Show
        {
            get
            {
                return vehicle.OBS_PAUSE == VhStopSingle.StopSingleOn;
            }
        }
        [Browsable(false)]
        public VhStopSingle OBS_PAUSE
        {
            set
            {
                vehicle.OBS_PAUSE = value;
                NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.oBS_PAUSE2Show));
            }
        }

        [DisplayName("HID pause")]
        public bool hID_PAUSE2Show
        {
            get
            {
                return vehicle.HID_PAUSE == VhStopSingle.StopSingleOn;
            }
        }
        [Browsable(false)]
        public VhStopSingle HID_PAUSE
        {
            set
            {
                vehicle.HID_PAUSE = value;
                NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.hID_PAUSE2Show));
            }
        }


        [DisplayName("OBS DIST(m)")]
        public double oBS_DIST2Show
        {
            get
            {
                return Math.Round((vehicle.OBS_DIST / distance2millimeter), 2, MidpointRounding.AwayFromZero);
            }
        }

        [Browsable(false)]
        public int OBS_DIST
        {
            get { return (int)(vehicle.OBS_DIST / distance2millimeter); }
            set
            {
                vehicle.OBS_DIST = value;
                NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.oBS_DIST2Show));
            }
        }
        //[DisplayName("Has CST")]
        //public int hAS_CST
        //{
        //    get { return vehicle.HAS_CST; }
        //    set
        //    {
        //        vehicle.HAS_CST = value;
        //        NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.hAS_CST));
        //    }
        //}

        [DisplayName("ODO(km)")]
        public double vEHICLE_ACC_DIST2Show
        {
            get
            {
                return Math.Round((vehicle.VEHICLE_ACC_DIST / km2millimeter), 2, MidpointRounding.AwayFromZero);
            }
        }
        //[DisplayName("ODO(km)")]
        [Browsable(false)]
        public int VEHICLE_ACC_DIST
        {
            get
            {
                return (int)(vehicle.VEHICLE_ACC_DIST / km2millimeter);
            }
            set
            {
                vehicle.VEHICLE_ACC_DIST = value;
                NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.vEHICLE_ACC_DIST2Show));
            }
        }
        //[DisplayName("Maintain ODO")]
        //public int mANT_ACC_DIST
        //{
        //    get { return vehicle.MANT_ACC_DIST; }
        //    set
        //    {
        //        vehicle.MANT_ACC_DIST = value;
        //        NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.mANT_ACC_DIST));
        //    }
        //}
        //[DisplayName("Last Maintain Date")]
        //public DateTime? mANT_DATE
        //{
        //    get { return vehicle.MANT_DATE ?? DateTime.MinValue; }
        //    set
        //    {
        //        vehicle.MANT_DATE = value;
        //        NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.mANT_DATE));
        //    }
        //}
        //[DisplayName("GRIP count")]
        //public int gRIP_COUNT
        //{
        //    get { return vehicle.GRIP_COUNT; }
        //    set
        //    {
        //        vehicle.GRIP_COUNT = value;
        //        NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.gRIP_COUNT));
        //    }
        //}
        //[DisplayName("GRIP Mant count")]
        //public int gRIP_MANT_COUNT
        //{
        //    get { return vehicle.GRIP_MANT_COUNT; }
        //    set
        //    {
        //        vehicle.GRIP_MANT_COUNT = value;
        //        NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.gRIP_MANT_COUNT));
        //    }
        //}
        //[DisplayName("Last GRIP Mant date")]
        //public DateTime? gRIP_MANT_DATE
        //{
        //    get { return vehicle.GRIP_MANT_DATE ?? DateTime.MinValue; }
        //    set
        //    {
        //        vehicle.GRIP_MANT_DATE = value;
        //        NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.gRIP_MANT_DATE));
        //    }
        //}
        //[DisplayName("Mode Adr")]
        //public string nODE_ADR
        //{
        //    get { return vehicle.NODE_ADR; }
        //    set
        //    {
        //        vehicle.NODE_ADR = value;
        //        NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.nODE_ADR));
        //    }
        //}


        [DisplayName("Sec DIST(m)")]
        public double ACC_SEC_DIST2Show
        {
            get
            {
                return Math.Round((vehicle.ACC_SEC_DIST / distance2millimeter), 2, MidpointRounding.AwayFromZero);
            }
        }
        [Browsable(false)]
        public double ACC_SEC_DIST
        {
            get { return (vehicle.ACC_SEC_DIST / distance2millimeter); }
            set
            {
                this.vehicle.ACC_SEC_DIST = value;
                NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.ACC_SEC_DIST2Show));
            }
        }
        public int BATTERYCAPACITY
        {
            get { return vehicle.BATTERYCAPACITY; }
            set
            {
                vehicle.BATTERYCAPACITY = value;
                NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.BATTERYCAPACITY));
            }
        }
        [DisplayName("UPD_TIME")]
        public DateTime? UPD_TIME
        {
            get { return vehicle.UPD_TIME ?? DateTime.MinValue; }
            set
            {
                vehicle.UPD_TIME = value;
                NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.UPD_TIME));
            }
        }
        [DisplayName("Type")]
        public E_VH_TYPE VEHICLE_TYPE
        {
            get { return vehicle.VEHICLE_TYPE; }
            set
            {
                vehicle.VEHICLE_TYPE = value;
                NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.VEHICLE_TYPE));
            }
        }

        //[DisplayName("Cycle Zone ID")]
        //public string cYCLERUN_ID
        //{
        //    get { return vehicle.CYCLERUN_ID; }
        //    set
        //    {
        //        vehicle.CYCLERUN_ID = value;
        //        NotifyPropertyChanged(BCFUtility.getPropertyName(() => this.cYCLERUN_ID));
        //    }
        //}
        //This is to notify the changes made to the object directly and not from the control. This refreshes the datagridview.
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(String propertyName)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    Adapter.Invoke((obj) =>
                    {
                        {
                            PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
                        }
                    }, null);
                }
            }
            catch
            {

            }
        }
    }
}
