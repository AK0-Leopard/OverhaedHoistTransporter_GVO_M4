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
    public class CMDObjToShow
    {
        //public static App.SCApplication app = App.SCApplication.getInstance();
        public BLL.PortStationBLL PortStationBLL = null;
        public ACMD cmd = null;

        public CMDObjToShow()
        {
        }
        public CMDObjToShow( ACMD _cmd)
        {
            Common.SCUtility.TrimAllParameter(_cmd);
            cmd = _cmd;
        }
        public string ID { get { return cmd.ID; } }
        public string VH_ID { get { return cmd.VH_ID; } }
        public string CARRIER_ID { get { return cmd.CARRIER_ID; } }
        public string TRANSFER_ID { get { return cmd.TRANSFER_ID; } }
        public E_CMD_TYPE CMD_TYPE { get { return cmd.CMD_TYPE; } }
        public string SOURCE
        {
            get
            {
                if (Common.SCUtility.isEmpty(cmd.SOURCE_PORT))
                    return cmd.SOURCE;
                else
                    return $"{cmd.SOURCE}({cmd.SOURCE_PORT})";
            }
        }
        public string DESTINATION
        {
            get
            {
                if (Common.SCUtility.isEmpty(cmd.DESTINATION_PORT))
                    return cmd.DESTINATION;
                else
                    return $"{cmd.DESTINATION}({cmd.DESTINATION_PORT})";
            }
        }
        public System.DateTime CMD_INSER_TIME { get { return cmd.CMD_INSER_TIME; } }
        public Nullable<System.DateTime> CMD_START_TIME { get { return cmd.CMD_START_TIME; } }
        public Nullable<System.DateTime> CMD_FINISH_TIME { get { return cmd.CMD_END_TIME; } }

    }
}
