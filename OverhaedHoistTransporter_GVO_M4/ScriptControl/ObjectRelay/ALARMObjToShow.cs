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
    public class ALARMObjToShow
    {
        public ALARM alarm_obj = null;
        public ALARMObjToShow(ALARM alarmObj)
        {
            alarm_obj = alarmObj;
        }
        [Description("Code")]
        public string ALAM_CODE { get { return alarm_obj.ALAM_CODE; } }
        [Description("Device ID")]
        public string EQPT_ID { get { return alarm_obj.EQPT_ID; } }
        [Description("State")]
        public ErrorStatus ALAM_STAT { get { return alarm_obj.ALAM_STAT; } }
        [Description("Level")]
        public E_ALARM_LVL ALAM_LVL { get { return alarm_obj.ALAM_LVL; } }
        [Description("Happend time")]
        public System.DateTime RPT_DATE_TIME { get { return alarm_obj.RPT_DATE_TIME; } }
        [Description("Clear time")]
        public Nullable<System.DateTime> CLEAR_DATE_TIME { get { return alarm_obj.CLEAR_DATE_TIME; } }
        [Description("Description")]
        public string ALAM_DESC { get { return alarm_obj.ALAM_DESC; } }
    }
}
