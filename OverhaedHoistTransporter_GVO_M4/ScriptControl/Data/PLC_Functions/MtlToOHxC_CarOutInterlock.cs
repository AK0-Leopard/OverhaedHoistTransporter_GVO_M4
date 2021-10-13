using com.mirle.ibg3k0.sc.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.PLC_Functions
{
    class MtlToOHxC_CarOutInterlock : PLC_FunBase
    {
        [PLCElement(ValueName = "MTL_TO_OHXC_CAR_OUT_INTERLOCK")]
        public bool CarOutInterlock;
    }
}
