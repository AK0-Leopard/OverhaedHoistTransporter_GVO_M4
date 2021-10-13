using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.PLC_Functions
{
    class MtlToOHxC_ManualCarOutRequest : PLC_FunBase
    {
        public DateTime Timestamp;
        [PLCElement(ValueName = "MTL_TO_OHXC_MANUAL_CAR_OUT_REQUEST_CAR_ID")]
        public UInt16 CarID;
        [PLCElement(ValueName = "MTL_TO_OHXC_MANUAL_CAR_OUT_REQUEST_HS")]
        public UInt16 Handshake;
    }

    class MtlToOHxC_ManualCarOuFinish : PLC_FunBase
    {
        public DateTime Timestamp;
        [PLCElement(ValueName = "MTL_TO_OHXC_MANUAL_CAR_OUT_FINISH_CAR_ID")]
        public UInt16 CarID;
        [PLCElement(ValueName = "MTL_TO_OHXC_MANUAL_CAR_OUT_FINISH_HS")]
        public UInt16 Handshake;
    }

    class OHxCToMtl_ManualCarOutRequestReply : PLC_FunBase
    {
        public DateTime Timestamp;
        [PLCElement(ValueName = "OHXC_TO_MTL_MANUAL_CAR_OUT_REQUEST_REPLY_RETURN_CODE")]
        public UInt16 ReturnCode;
        [PLCElement(ValueName = "OHXC_TO_MTL_MANUAL_CAR_OUT_REQUEST_REPLY_HS")]
        public UInt16 Handshake;
    }

    class OHxCToMtl_ManualCarOutFinishReply : PLC_FunBase
    {
        public DateTime Timestamp;
        [PLCElement(ValueName = "OHXC_TO_MTL_MANUAL_CAR_OUT_FINISH_REPLY_HS")]
        public UInt16 Handshake;
    }
}
