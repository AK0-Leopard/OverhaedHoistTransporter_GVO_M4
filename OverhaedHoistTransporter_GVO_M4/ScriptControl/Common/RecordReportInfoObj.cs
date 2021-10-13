using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Common
{
    public class RecordReportInfoObj
    {
        AVEHICLE vh;
        //message info
        public string Index = "RecordReportInfo";
        public DateTime RPT_TIME;
        public string MSG_FROM;
        public string MSG_TO;
        public string FUN_NAME;

        //Vh info
        public string VH_ID { get { return vh?.VEHICLE_ID; } }
        public string CST { get { return vh?.CST_ID; } }
        public string CMD { get { return vh?.CMD_ID; } }

        public string TRAN_CMD_ID { get { return vh?.TRANSFER_ID; } }

        public string ADR_ID { get { return vh?.CUR_ADR_ID; } }
        public string SEC_ID { get { return vh?.CUR_SEC_ID; } }
        public string SEC_DIS { get { return vh?.ACC_SEC_DIST.ToString(); } }
        public string VH_MODE { get { return vh?.MODE_STATUS.ToString(); } }
        public string VH_STATUS { get { return vh?.ACT_STATUS.ToString(); } }

        public string SEQ_NUM;
        //136
        public string EVENT_TYPE;
        public string RESERVE_SEC_ID;
        //36
        public string IS_RESERVE_SUCCESS;

        //public string CMD_ID;
        //public string TRAN_CMD_ID;
        public string MSG_BODY;

        public void set(AVEHICLE vh, int seqNum, ID_134_TRANS_EVENT_REP gpbMessage)
        {
            SEQ_NUM = seqNum.ToString();
            EVENT_TYPE = gpbMessage.EventType.ToString();
        }
        public void set(AVEHICLE vh, int seqNum, ID_136_TRANS_EVENT_REP gpbMessage)
        {
            SEQ_NUM = seqNum.ToString();
            EVENT_TYPE = gpbMessage.EventType.ToString();
            if (gpbMessage.ReserveInfos.Count > 0)
                RESERVE_SEC_ID = gpbMessage.ReserveInfos[0].ReserveSectionID;
        }
        public void set(AVEHICLE vh, int seqNum, ID_36_TRANS_EVENT_RESPONSE gpbMessage)
        {
            SEQ_NUM = seqNum.ToString();
            EVENT_TYPE = gpbMessage.ReplyActiveType.ToString();
            if (gpbMessage.ReserveInfos.Count > 0)
                RESERVE_SEC_ID = gpbMessage.ReserveInfos[0].ReserveSectionID;
            IS_RESERVE_SUCCESS = gpbMessage.IsReserveSuccess.ToString();
        }
        public override string ToString()
        {
            string sJson = Newtonsoft.Json.JsonConvert.SerializeObject(this, JsHelper.jsBooleanConverter, JsHelper.jsTimeConverter, JsHelper.jsLogTypeConverter);
            sJson = sJson.Replace(nameof(RPT_TIME), "@timestamp");
            return sJson;
        }

    }
}
