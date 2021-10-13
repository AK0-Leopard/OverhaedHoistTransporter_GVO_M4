using com.mirle.ibg3k0.sc.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.PLC_Functions
{
    public class PortPLCInfo : PLC_FunBase
    {
        public DateTime Timestamp;

        public string Port_ID;
        [PLCElement(ValueName = "OP_RUN")]
        public bool OpRun;         //D6401.0
        [PLCElement(ValueName = "OP_DOWN")]
        public bool OpDown;       //D6401.1 
        [PLCElement(ValueName = "OP_ERROR")]
        public bool OpError;

        [PLCElement(ValueName = "NOW_INPUT_MODE")]
        public bool IsInputMode;        //D6401.3
        [PLCElement(ValueName = "NOW_OUTPUT_MODE")]
        public bool IsOutputMode;       //D6401.4
        [PLCElement(ValueName = "MODE_CHANGEABLE")]
        public bool IsModeChangable;    //D6401.5

        [PLCElement(ValueName = "WAIT_IN")]
        public bool PortWaitIn;         //D6401.8
        [PLCElement(ValueName = "WAIT_OUT")]
        public bool PortWaitOut;        //D6401.9

        [PLCElement(ValueName = "IS_AUTO_MODE")]
        public bool IsAutoMode;

        [PLCElement(ValueName = "READY_TO_LOAD")]
        public bool IsReadyToLoad;      //D6401.12
        [PLCElement(ValueName = "READY_TO_UNLOAD")]
        public bool IsReadyToUnload;    //D6401.13

        [PLCElement(ValueName = "LOAD_POSITION_1")]
        public bool LoadPosition1;      //D6402.0
        [PLCElement(ValueName = "LOAD_POSITION_2")]
        public bool LoadPosition2;      //D6402.1
        [PLCElement(ValueName = "LOAD_POSITION_3")]
        public bool LoadPosition3;      //D6402.2
        [PLCElement(ValueName = "LOAD_POSITION_4")]
        public bool LoadPosition4;      //D6402.3
        [PLCElement(ValueName = "LOAD_POSITION_5")]
        public bool LoadPosition5;      //D6402.4

        [PLCElement(ValueName = "BARCODE_READ_DONE")]
        public bool BCRReadDone;

        [PLCElement(ValueName = "CST_TRANSFER_COMPLETE")]
        public bool IsTransferComplete;
        [PLCElement(ValueName = "CST_REMOVE_CHECK")]
        public bool CstRemoveCheck;

        [PLCElement(ValueName = "ERROR_CODE")]
        public UInt16 ErrorCode;

 

        [PLCElement(ValueName = "LOAD_POSITION_CST_ID_1")]
        public string LoadPositionCSTID1;

        [PLCElement(ValueName = "LOAD_POSITION_CST_ID_2")]
        public string LoadPositionCSTID2;

        [PLCElement(ValueName = "LOAD_POSITION_CST_ID_3")]
        public string LoadPositionCSTID3;

        [PLCElement(ValueName = "LOAD_POSITION_CST_ID_4")]
        public string LoadPositionCSTID4;

        [PLCElement(ValueName = "LOAD_POSITION_CST_ID_5")]
        public string LoadPositionCSTID5;

        [PLCElement(ValueName = "RFID_READ_CST_ID")]
        public string RFIDCassetteID;

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            //builder.AppendLine();
            if (fieldInfos == null || fieldInfos.Count() == 0) return string.Empty;
            string function_name = fieldInfos[0].DeclaringType.Name;

            
            builder.Append(" -");
            builder.Append(string.Format("{0} : {1}", "Timestamp", Timestamp.ToString(SCAppConstants.DateTimeFormat_23))).AppendLine();
            builder.Append(" -");
            builder.Append(string.Format("{0} : {1}", "Func", function_name)).AppendLine();
            builder.Append(" -");
            builder.Append(string.Format("{0} : {1}", "PortID", Port_ID)).AppendLine();
            foreach (FieldInfo field in fieldInfos)
            {
                string name = field.Name;
                string sValue = string.Empty;
                if (field.FieldType == typeof(char[]))
                {
                    sValue = string.Join("", (char[])field.GetValue(this));
                }
                else if (field.FieldType == typeof(UInt16[]))
                {
                    sValue = string.Join(" ", (UInt16[])field.GetValue(this));
                }
                else
                {
                    object obj = field.GetValue(this);
                    sValue = obj == null ? string.Empty : obj.ToString();
                }
                builder.Append(" -");
                builder.Append(string.Format("{0} : {1}", name, sValue)).AppendLine();
            }
            return builder.ToString();
        }
    }
}
