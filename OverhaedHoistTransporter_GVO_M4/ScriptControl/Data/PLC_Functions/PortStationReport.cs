using com.mirle.ibg3k0.sc.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.sc.App;
using Newtonsoft.Json.Converters;

namespace com.mirle.ibg3k0.sc.Data.PLC_Functions
{
    class PortStationReport : PLC_FunBase
    {
        public DateTime Timestamp;


        [PLCElement(ValueName = "REQUEST")]
        public bool portStationLoadRequest;
        [PLCElement(ValueName = "READY")]
        public bool portStationReady;

        [JsonConverter(typeof(StringEnumConverter))]
        public ProtocolFormat.OHTMessage.PortStationStatus portStationStatus;
        [JsonConverter(typeof(StringEnumConverter))]
        public ProtocolFormat.OHTMessage.PortStationServiceStatus portServiceState;

        public override string ToString()
        {
            //string sJson = Newtonsoft.Json.JsonConvert.SerializeObject(this, JsHelper.jsBooleanConverter, JsHelper.jsTimeConverter);
            string sJson = Newtonsoft.Json.JsonConvert.SerializeObject(this, JsHelper.jsTimeConverter);
            sJson = sJson.Replace(nameof(Timestamp), "@timestamp");
            return sJson;
        }
    }
}
