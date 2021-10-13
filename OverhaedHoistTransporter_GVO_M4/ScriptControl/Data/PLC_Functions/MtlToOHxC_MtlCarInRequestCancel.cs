using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.PLC_Functions
{
    class MtlToOHxC_MtlCarInRequestCancel : PLC_FunBase
    {
        public DateTime Timestamp;
        [PLCElement(ValueName = "MTL_TO_OHXC_CAR_IN_REQUEST_CANCEL_CAR_ID")]
        public UInt16 CarID;
        [PLCElement(ValueName = "MTL_TO_OHXC_CAR_IN_REQUEST_CANCEL_HS")]
        public UInt16 Handshake;
    }

    class OHxCToMtl_MtlCarInRequestCancelReply : PLC_FunBase
    {
        public DateTime Timestamp;
        [PLCElement(ValueName = "OHXC_TO_MTL_CAR_IN_REQUEST_CANCEL_REPLY_RETURN_CODE")]
        public UInt16 ReturnCode;
        [PLCElement(ValueName = "OHXC_TO_MTL_CAR_IN_REQUEST_CANCEL_REPLY_HS")]
        public UInt16 Handshake;
    }
}
