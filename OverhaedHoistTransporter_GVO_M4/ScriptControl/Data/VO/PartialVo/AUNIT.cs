using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.bcf.Data.ValueDefMapAction;
using com.mirle.ibg3k0.bcf.Data.VO;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Data.SECS;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.Data.VO.Interface;
using com.mirle.ibg3k0.sc.ObjectRelay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc
{
    public partial class AUNIT : BaseUnitObject
    {
        private AlarmHisList alarmHisList = new AlarmHisList();
        #region charger 

        #region Event
        public event EventHandler CouplerStatusChanged;
        public event EventHandler<SCAppConstants.CouplerHPSafety> CouplerHPSafetyChaged;
        #endregion Event

        private int chargerAlive;
        public virtual int ChargerAlive
        {
            get { return chargerAlive; }
            set
            {
                if (chargerAlive != value)
                {
                    chargerAlive = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.ChargerAlive));
                }
            }
        }

        private int chargerStatusIndex;
        public virtual int ChargerStatusIndex
        {
            get { return chargerStatusIndex; }
            set
            {
                if (chargerStatusIndex != value)
                {
                    chargerStatusIndex = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.ChargerStatusIndex));
                }
            }
        }

        private int chargerCurrentParameterIndex;
        public virtual int ChargerCurrentParameterIndex
        {
            get { return chargerCurrentParameterIndex; }
            set
            {
                if (chargerCurrentParameterIndex != value)
                {
                    chargerCurrentParameterIndex = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.ChargerCurrentParameterIndex));
                }
            }
        }


        private int couplerChargeInfoIndex;
        public virtual int CouplerChargeInfoIndex
        {
            get { return couplerChargeInfoIndex; }
            set
            {
                if (couplerChargeInfoIndex != value)
                {
                    couplerChargeInfoIndex = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.CouplerChargeInfoIndex));
                }
            }
        }

        private int pIOIndex;
        public virtual int PIOIndex
        {
            get { return pIOIndex; }
            set
            {
                if (pIOIndex != value)
                {
                    pIOIndex = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.PIOIndex));
                }
            }
        }


        private int currentSupplyStatusBlock;
        public virtual int CurrentSupplyStatusBlock
        {
            get { return currentSupplyStatusBlock; }
            set
            {
                OnPropertyChanged(BCFUtility.getPropertyName(() => this.CurrentSupplyStatusBlock));
            }
        }
        public float inputVoltage;
        public float chargeVoltage;
        public float chargeCurrent;
        public float chargePower;
        public float couplerChargeVoltage;
        public float couplerChargeCurrent;
        public UInt16 couplerID;



        public bool chargerReserve;
        public bool chargerConstantVoltageOutput;
        public bool chargerConstantCurrentOutput;
        public bool chargerHighInputVoltageProtection;
        public bool chargerLowInputVoltageProtection;
        public bool chargerHighOutputVoltageProtection;
        public bool chargerHighOutputCurrentProtection;
        public bool chargerOverheatProtection;
        public string chargerRS485Status;

        public SCAppConstants.CouplerStatus Coupler1Status { get; private set; }
        public SCAppConstants.CouplerStatus Coupler2Status { get; private set; }
        public SCAppConstants.CouplerStatus Coupler3Status { get; private set; }
        public void setCouplerStatus(SCAppConstants.CouplerStatus coupler1Status, SCAppConstants.CouplerStatus coupler2Status,
                                     SCAppConstants.CouplerStatus coupler3Status)
        {
            bool has_change = false;
            if (Coupler1Status != coupler1Status)
            {
                has_change = hasCouplerStatusChange(coupler1Status, Coupler1Status);
                Coupler1Status = coupler1Status;
            }
            if (Coupler2Status != coupler2Status)
            {
                has_change = hasCouplerStatusChange(coupler2Status, Coupler2Status);
                Coupler2Status = coupler2Status;
            }
            if (Coupler3Status != coupler3Status)
            {
                has_change = hasCouplerStatusChange(coupler3Status, Coupler3Status);
                Coupler3Status = coupler3Status;
            }
            if (has_change)
            {
                onCouplerStatusChange();
            }
        }
        private bool hasCouplerStatusChange(SCAppConstants.CouplerStatus oldStatus, SCAppConstants.CouplerStatus newStatus)
        {
            switch (oldStatus)
            {
                case SCAppConstants.CouplerStatus.Auto:
                case SCAppConstants.CouplerStatus.Charging:
                    if (newStatus == SCAppConstants.CouplerStatus.None ||
                       newStatus == SCAppConstants.CouplerStatus.Manual ||
                       newStatus == SCAppConstants.CouplerStatus.Error)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case SCAppConstants.CouplerStatus.None:
                case SCAppConstants.CouplerStatus.Manual:
                case SCAppConstants.CouplerStatus.Error:
                    if (newStatus == SCAppConstants.CouplerStatus.Auto ||
                       newStatus == SCAppConstants.CouplerStatus.Charging)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
            }
            return false;
        }


        private SCAppConstants.CouplerHPSafety coupler1hpsafety;
        public SCAppConstants.CouplerHPSafety coupler1HPSafety
        {
            get { return coupler1hpsafety; }
            set
            {
                if (coupler1hpsafety != value)
                {
                    coupler1hpsafety = value;
                    onCouplerHPSafetyChaged(value);
                }
            }
        }
        private SCAppConstants.CouplerHPSafety coupler2hpsafety;
        public SCAppConstants.CouplerHPSafety coupler2HPSafety
        {
            get { return coupler2hpsafety; }
            set
            {
                if (coupler2hpsafety != value)
                {
                    coupler2hpsafety = value;
                    //onCouplerHPSafetyChaged();
                }
            }
        }
        private SCAppConstants.CouplerHPSafety coupler3hpsafety;
        public SCAppConstants.CouplerHPSafety coupler3HPSafety
        {
            get { return coupler3hpsafety; }
            set
            {
                if (coupler3hpsafety != value)
                {
                    coupler3hpsafety = value;
                    //onCouplerHPSafetyChaged();
                }
            }
        }

        public float chargerOutputVoltage;
        public float chargerOutputCurrent;
        public float chargerOverVoltage;
        public float chargerOverCurrent;

        public int chargerCouplerID;
        public DateTime chargerChargingStartTime;
        public DateTime chargerChargingEndTime;
        public float chargerInputAH;
        public string chargerChargingResult;
        public List<PIOInfo> PIOInfos = new List<PIOInfo>();


        public class PIOInfo
        {
            public int CouplerID;
            public DateTime Timestamp;
            public string signal1;
            public string signal2;
        }

        public void onCouplerStatusChange()
        {
            CouplerStatusChanged?.Invoke(this, EventArgs.Empty);
        }
        public void onCouplerHPSafetyChaged(SCAppConstants.CouplerHPSafety couplerHPSafety)
        {
            CouplerHPSafetyChaged?.Invoke(this, couplerHPSafety);
        }
        #endregion charger

        public AUNIT()
        {
            eqptObjectCate = SCAppConstants.EQPT_OBJECT_CATE_UNIT;
        }

        public override void doShareMemoryInit(BCFAppConstants.RUN_LEVEL runLevel)
        {
            foreach (IValueDefMapAction action in valueDefMapActionDic.Values)
            {
                action.doShareMemoryInit(runLevel);
            }
        }


        /// <summary>
        /// Gets the eqpt object key.
        /// </summary>
        /// <returns>System.String.</returns>
        public virtual string getEqptObjectKey()
        {
            return BCFUtility.generateEQObjectKey(eqptObjectCate, UNIT_ID);
        }

        #region FireReport
        //public bool fireReport { get; set; }
        public bool fireDoorOpen { get; set; }
        public bool fireDoorCloseGrant { get; set; }
        public bool fireDoorCrossingSignal { get; set; }

        private List<string> reserve_section_id_List = new List<string>();//在防火門所在區域Segment的Section被Reserve時，會把ID加到這個List。

        Object reserve_section_id_dic_lock = new object();
        public void section_reserved(string section_id)
        {
            lock (reserve_section_id_dic_lock)
            {
                if (!reserve_section_id_List.Contains(section_id))
                {
                    reserve_section_id_List.Add(section_id);
                }
                else
                {

                }
            }
        }
        public void section_unreserved(string section_id)
        {
            lock (reserve_section_id_dic_lock)
            {
                if (reserve_section_id_List.Contains(section_id))
                {
                    reserve_section_id_List.Remove(section_id);
                }
                else
                {

                }
            }
        }
        public bool isOKtoBanRoute()
        {
            return reserve_section_id_List.Count == 0 && !fireDoorCrossingSignal;
        }
        #endregion FireReport
    }

}
