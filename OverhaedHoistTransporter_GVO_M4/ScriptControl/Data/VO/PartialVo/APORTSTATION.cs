using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.bcf.Data.ValueDefMapAction;
using com.mirle.ibg3k0.bcf.Data.VO;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.BLL;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.SECS;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.Data.VO.Interface;
using com.mirle.ibg3k0.sc.ObjectRelay;
using com.mirle.ibg3k0.sc.ProtocolFormat.SystemClass.PortInfo;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc
{
    public partial class APORTSTATION : BaseEQObject
    {
        public string CST_ID { get; set; }
        public string ZONE_ID { get; set; }
        public string EQPT_ID { get; set; }
        public string NODE_ID { get; set; }
        public int PortNum { get; set; }

        public bool IncludeCycleTest { get; set; }
        public int TestTimes { get; set; }

        public (bool isSuccess, double x, double y) getAxis(BLL.IReserveBLL reserveBLL)
        {
            var result = reserveBLL.GetHltMapAddress(this.ADR_ID);
            return (result.isSuccess, result.x, result.y);
        }

        public override void doShareMemoryInit(BCFAppConstants.RUN_LEVEL runLevel)
        {
            foreach (IValueDefMapAction action in valueDefMapActionDic.Values)
            {
                action.doShareMemoryInit(runLevel);
            }
        }
        public AEQPT GetEqpt(EqptBLL eqptBLL)
        {
            return eqptBLL.OperateCatch.GetEqpt(EQPT_ID);
        }
        public SCAppConstants.EqptType GetEqptType(EqptBLL eqptBLL)
        {
            return eqptBLL.OperateCatch.GetEqptType(EQPT_ID);
        }
        public string GroupID
        {
            get
            {
                string[] temp = PORT_ID.Contains(":") ? PORT_ID.Split(':') : null;
                if (temp != null)
                {
                    return temp[0];
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public override string ToString()
        {
            return $"{PORT_ID} ({ADR_ID})";
        }
        public string PortAdrInfo
        {
            get { return $"{PORT_ID} ({ADR_ID})"; }
        }

    }

    public partial class APORTSTATION
    {
        //PORT_INFO PortInfo = new PORT_INFO();
        public DateTime Timestamp = DateTime.MinValue;
        //{
        //    get
        //    {
        //        DateTime dateTime = DateTime.MinValue;
        //        DateTime.TryParseExact(PortInfo.Timestamp, SCAppConstants.TimestampFormat_17, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
        //        return dateTime;
        //    }
        //}
        public int CSTCount //20210521 markchou 根據在席訊號推斷有多少CST
        {
            get
            {
                int count = 0;
                if (IsCSTPresenceLoc1) count++;
                if (IsCSTPresenceLoc2) count++;
                if (IsCSTPresenceLoc3) count++;
                if (IsCSTPresenceLoc4) count++;
                if (IsCSTPresenceLoc5) count++;
                return count;
            }
        }

        public enum PortStatus
        {
            None = 0,
            Run = 1,
            Down = 2,
            Fault = 3
        }

        //public bool IsInPutMode { get { return PortInfo.IsInputMode; } }
        public PortStatus PLCPortStatus = PortStatus.None;
        public bool IsInPutMode = false;
        public bool IsOutPutMode = false;
        public bool IsModeChangeable = false;
        public bool PortWaitIn = false;
        public bool PortWaitOut = false;
        public bool PortReady = false;
        public bool IsAutoMode = false;
        public bool IsError = false;
        public int ErrorCode = 0;

        public bool IsReadyToLoad = false;
        public bool IsReadyToUnload = false;


        public bool IsCSTPresenceLoc1 = false;
        public bool IsCSTPresenceLoc2 = false;
        public bool IsCSTPresenceLoc3 = false;
        public bool IsCSTPresenceLoc4 = false;
        public bool IsCSTPresenceLoc5 = false;

        public string CSTPresenceID1 = string.Empty;
        public string CSTPresenceID2 = string.Empty;
        public string CSTPresenceID3 = string.Empty;
        public string CSTPresenceID4 = string.Empty;
        public string CSTPresenceID5 = string.Empty;
        //public bool CSTPresenceMismatch { get { return PortInfo.CSTPresenceMismatch; } }
        public string CassetteID = null;
        public string AssignCSTID = string.Empty;

        public int stageCount = 0;
        //public void SetPortInfo(PORT_INFO newPortInfo)
        //{
        //    //PortInfo.Timestamp = newPortInfo.Timestamp;
        //    PortInfo.Timestamp = DateTime.Now.ToString(SCAppConstants.TimestampFormat_17);
        //    PortInfo.IsAutoMode = newPortInfo.IsAutoMode;
        //    PortInfo.IsInputMode = newPortInfo.IsInputMode;
        //    PortInfo.IsOutputMode = newPortInfo.IsOutputMode;
        //    PortInfo.AGVPortReady = newPortInfo.AGVPortReady;
        //    PortInfo.PortWaitOut = newPortInfo.PortWaitOut;
        //    PortInfo.PortWaitIn = newPortInfo.PortWaitIn;
        //    PortInfo.IsCSTPresence = newPortInfo.IsCSTPresence;
        //    PortInfo.CSTPresenceMismatch = newPortInfo.CSTPresenceMismatch;
        //    PortInfo.CassetteID = newPortInfo.CassetteID;
        //}
        //public void ResetPortInfo()
        //{
        //    PortInfo.IsAutoMode = false;
        //    PortInfo.IsInputMode = false;
        //    PortInfo.IsOutputMode = false;
        //    PortInfo.AGVPortReady = false;
        //    PortInfo.PortWaitOut = false;
        //    PortInfo.PortWaitIn = false;
        //    PortInfo.IsCSTPresence = false;
        //    PortInfo.CassetteID = "";
        //}
    }



}
