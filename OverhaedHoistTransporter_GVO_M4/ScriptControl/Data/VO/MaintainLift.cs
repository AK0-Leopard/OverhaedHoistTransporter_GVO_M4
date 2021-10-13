using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction;
using com.mirle.ibg3k0.sc.Data.VO.Interface;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.VO
{
    public class MaintainLift : AEQPT, IMaintainDevice
    {
        public string MTL_SYSTEM_OUT_ADDRESS = null;
        public string MTL_CAR_IN_BUFFER_ADDRESS = null;
        public string MTL_SEGMENT = null;
        public string MTL_ADDRESS = null;
        public string MTL_SYSTEM_IN_ADDRESS = null;
        public MaintainLift()
        {
            //MTLInfo info = SCApplication.getInstance().getCommObjCacheManager().getMTLInfo(EQPT_ID);
            //MTL_SYSTEM_OUT_ADDRESS = info.SYSTEM_OUT_ADDRESS;
            //MTL_CAR_IN_BUFFER_ADDRESS = info.CAR_IN_BUFFER_ADDRESS;
            //MTL_SEGMENT = info.SEGMENT;
            //MTL_ADDRESS = info.ADDRESS;
            //MTL_SYSTEM_IN_ADDRESS = info.SYSTEM_IN_ADDRESS;
        }

        public void setMTLInfo()
        {
            MTLInfo info = SCApplication.getInstance().getCommObjCacheManager().getMTLInfo(EQPT_ID);
            MTL_SYSTEM_OUT_ADDRESS = info.SYSTEM_OUT_ADDRESS;
            MTL_CAR_IN_BUFFER_ADDRESS = info.CAR_IN_BUFFER_ADDRESS;
            MTL_SEGMENT = info.SEGMENT;
            MTL_ADDRESS = info.ADDRESS;
            MTL_SYSTEM_IN_ADDRESS = info.SYSTEM_IN_ADDRESS;
        }
        public string DeviceID { get { return EQPT_ID; } set { } }
        public string DeviceSegment { get { return MTL_SEGMENT; } set { } }
        public string DeviceAddress { get { return MTL_ADDRESS; } set { } }
        [JsonIgnore]
        //public IMaintainDevice DokingMaintainDevice = null;

        //public string CurrentCarID { get; set; }
        //public bool HasVehicle { get; set; }
        //public bool StopSingle { get; set; }
        //public MTxMode MTxMode { get; set; }
        //public MTLLocation MTLLocation;
        //public MTLMovingStatus MTLMovingStatus;
        //public UInt32 Encoder;
        //public VhInPosition VhInPosition;
        //public bool CarInSafetyCheck { get; set; }
        //public bool CarOutSafetyCheck { get; set; }
        public string PreCarOutVhID { get; set; }
        public bool CancelCarOutRequest { get; set; }
        public bool CarOurSuccess { get; set; }

        public ushort CurrentPreCarOurID { get; set; }
        public ushort CurrentPreCarOurActionMode { get; set; }
        public ushort CurrentPreCarOurCSTExist { get; set; }
        public ushort CurrentPreCarOurSectionID { get; set; }
        public uint CurrentPreCarOurAddressID { get; set; }
        private uint currentPreCarOurDistance;
        public uint CurrentPreCarOurDistance
        {
            get { return currentPreCarOurDistance; }
            set
            {
                if (currentPreCarOurDistance != value)
                {
                    currentPreCarOurDistance = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.CurrentPreCarOurDistance));
                }
            }
        }

        public ushort CurrentPreCarOurSpeed { get; set; }

        private bool carOutInterlock;
        public bool OHTCCarOutInterlock
        {
            get { return carOutInterlock; }
            set
            {
                if (carOutInterlock != value)
                {
                    carOutInterlock = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.OHTCCarOutInterlock));
                }
            }
        }

        private bool carOutMoving;
        public bool OHTCCarOutMoving
        {
            get { return carOutMoving; }
            set
            {
                if (carOutMoving != value)
                {
                    carOutMoving = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.OHTCCarOutMoving));
                }
            }
        }

        private bool ohtcCarOutReady;
        public bool OHTCCarOutReady
        {
            get { return ohtcCarOutReady; }
            set
            {
                if (ohtcCarOutReady != value)
                {
                    ohtcCarOutReady = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.OHTCCarOutReady));
                }
            }
        }

        private bool ohtcCarOutMoveComplete;
        public bool OHTCCarOutMoveComplete
        {
            get { return ohtcCarOutMoveComplete; }
            set
            {
                if (ohtcCarOutMoveComplete != value)
                {
                    ohtcCarOutMoveComplete = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.OHTCCarOutMoveComplete));
                }
            }
        }


        private bool ohtcCarInMoving;
        public bool OHTCCarInMoving
        {
            get { return ohtcCarInMoving; }
            set
            {
                if (ohtcCarInMoving != value)
                {
                    ohtcCarInMoving = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.OHTCCarInMoving));
                }
            }
        }

        private bool ohtcCarInInterlock;
        public bool OHTCCarInInterlock
        {
            get { return ohtcCarInInterlock; }
            set
            {
                if (ohtcCarInInterlock != value)
                {
                    ohtcCarInInterlock = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.OHTCCarInInterlock));
                }
            }
        }

        private bool ohtcCarInMoveComplete;
        public bool OHTCCarInMoveComplete
        {
            get { return ohtcCarInMoveComplete; }
            set
            {
                if (ohtcCarInMoveComplete != value)
                {
                    ohtcCarInMoveComplete = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.OHTCCarInMoveComplete));
                }
            }
        }



        public bool IsAlive { get { return base.Is_Eq_Alive; } set { } }

        public (bool isSendSuccess, UInt16 returnCode) carOutRequest(UInt16 carNum)
        {

            //return getExcuteMapAction().OHxC_CarOutNotify(carNum,2);
            MTLValueDefMapAction mapAction = getExcuteMapAction();
            if (mapAction != null)
            {
                return mapAction.OHxC_CarOutNotify(carNum, 2);
            }
            else
            {
                return getExcuteMapActionNew().OHxC_CarOutNotify(carNum, 2);
            }

        }
        public bool SetCarOutInterlock(bool onOff)
        {
            //return getExcuteMapAction().setOHxC2MTL_CarOutInterlock(onOff);

            MTLValueDefMapAction mapAction = getExcuteMapAction();
            if (mapAction != null)
            {
                return mapAction.setOHxC2MTL_CarOutInterlock(onOff);
            }
            else
            {
                return getExcuteMapActionNew().setOHxC2MTL_CarOutInterlock(onOff);
            }
        }

        public bool SetCarOutReady(bool onOff)
        {
            MTLValueDefMapAction mapAction = getExcuteMapAction();
            if (mapAction != null)
            {
                return mapAction.setOHxC2MTL_CarOutReady(onOff);
            }
            else
            {
                return getExcuteMapActionNew().setOHxC2MTL_CarOutReady(onOff);
            }
        }
        public bool SetCarOutMoveComplete(bool onOff)
        {
            MTLValueDefMapAction mapAction = getExcuteMapAction();
            if (mapAction != null)
            {
                return mapAction.setOHxC2MTL_CarOutMoveComplete(onOff);
            }
            else
            {
                return getExcuteMapActionNew().setOHxC2MTL_CarOutMoveComplete(onOff);
            }
        }
        public bool SetCarOutMoving(bool onOff)
        {
            //return getExcuteMapAction().setOHxC2MTL_CarOutInterlock(onOff);

            MTLValueDefMapAction mapAction = getExcuteMapAction();
            if (mapAction != null)
            {
                return mapAction.setOHxC2MTL_CarOutMoving(onOff);
            }
            else
            {
                return getExcuteMapActionNew().setOHxC2MTL_CarOutMoving(onOff);
            }
        }

        public bool SetCarInMoving(bool onOff)
        {
            //return getExcuteMapAction().setOHxC2MTL_CarInMoving(onOff);


            MTLValueDefMapAction mapAction = getExcuteMapAction();
            if (mapAction != null)
            {
                return mapAction.setOHxC2MTL_CarInMoving(onOff);
            }
            else
            {
                return getExcuteMapActionNew().setOHxC2MTL_CarInMoving(onOff);
            }
        }
        public bool SetCarInMoveComplete(bool onOff)
        {
            //return getExcuteMapAction().setOHxC2MTL_CarInMoving(onOff);
            MTLValueDefMapAction mapAction = getExcuteMapAction();
            if (mapAction != null)
            {
                return mapAction.setOHxC2MTL_CarInMoveComplete(onOff);
            }
            else
            {
                return getExcuteMapActionNew().setOHxC2MTL_CarInMoveComplete(onOff);
            }
        }

        public bool SetCarInInterlock(bool onOff)
        {
            //return getExcuteMapAction().setOHxC2MTL_CarInMoving(onOff);
            MTLValueDefMapAction mapAction = getExcuteMapAction();
            if (mapAction != null)
            {
                return mapAction.setOHxC2MTL_CarInInterlock(onOff);
            }
            else
            {
                return getExcuteMapActionNew().setOHxC2MTL_CarInInterlock(onOff);
            }
        }

        public void setCarRealTimeInfo(UInt16 car_id, UInt16 action_mode, UInt16 cst_exist, UInt16 current_section_id, UInt32 current_address_id,
                                            UInt32 buffer_distance, UInt16 speed)
        {
            MTLValueDefMapAction mapAction = getExcuteMapAction();
            if (mapAction != null)
            {
                mapAction.CarRealtimeInfo(car_id, action_mode, cst_exist, current_section_id, current_address_id, buffer_distance, speed);
            }
            else
            {
                getExcuteMapActionNew().CarRealtimeInfo(car_id, action_mode, cst_exist, current_section_id, current_address_id, buffer_distance, speed);
            }

        }

        private MTLValueDefMapAction getExcuteMapAction()
        {
            MTLValueDefMapAction mapAction;
            mapAction = this.getMapActionByIdentityKey(typeof(MTLValueDefMapAction).Name) as MTLValueDefMapAction;

            return mapAction;
        }

        private MTLValueDefMapAction getExcuteMapActionNew()
        {
            MTLValueDefMapAction mapAction;
            mapAction = this.getMapActionByIdentityKey(typeof(MTLValueDefMapAction).Name) as MTLValueDefMapAction;

            return mapAction;
        }

        public void setMTLSegment(string adrID)
        {
            MTL_SEGMENT = adrID;
        }
        public void setMTLAddress(string adrID)
        {
            MTL_ADDRESS = adrID;
        }
        public void setMTLSystemInAddress(string adrID)
        {
            MTL_SYSTEM_IN_ADDRESS = adrID;
        }
        public void setMTLCarInBufferAddress(string adrID)
        {
            MTL_CAR_IN_BUFFER_ADDRESS = adrID;
        }
        public void setMTLSystemOutAddress(string adrID)
        {
            MTL_SYSTEM_OUT_ADDRESS = adrID;
        }


    }
}
