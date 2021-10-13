using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.bcf.Data.ValueDefMapAction;
using com.mirle.ibg3k0.bcf.Data.VO;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.Data.VO.Interface;
using com.mirle.ibg3k0.sc.ObjectRelay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc
{
    public partial class VTRANSFER
    {

        public ACMD ConvertToCmd(BLL.PortStationBLL portStationBLL, BLL.SequenceBLL sequenceBLL, AVEHICLE assignVehicle)
        {
            var source_port_station = portStationBLL.OperateCatch.getPortStation(this.HOSTSOURCE);
            var desc_port_station = portStationBLL.OperateCatch.getPortStation(this.HOSTDESTINATION);
            //如果HostSource port是與車子一樣的話，代表是要去進行Unload的動作
            bool source_is_a_port = portStationBLL.OperateCatch.IsExist(this.HOSTSOURCE);

            string host_source = source_is_a_port ? this.HOSTSOURCE : "";
            //E_CMD_TYPE cmd_type = source_is_a_port ? E_CMD_TYPE.LoadUnload : E_CMD_TYPE.Unload; //如果Source不是Port的話，則代表是在車上

            bool dest_is_a_port = portStationBLL.OperateCatch.IsExist(this.HOSTDESTINATION);

            string host_dest = dest_is_a_port ? this.HOSTDESTINATION : "";
            E_CMD_TYPE cmd_type;
            if (source_is_a_port && dest_is_a_port)
            {
                cmd_type = E_CMD_TYPE.LoadUnload;
            }else if (source_is_a_port)
            {
                cmd_type = E_CMD_TYPE.Load;
            }
            else
            {
                cmd_type = E_CMD_TYPE.Unload;
            }
            //E_CMD_TYPE cmd_type = dest_is_a_port ? E_CMD_TYPE.LoadUnload : E_CMD_TYPE.Unload; //如果Source不是Port的話，則代表是在車上


            string from_adr = source_port_station == null ? string.Empty : source_port_station.ADR_ID;
            string to_adr = desc_port_station == null ? string.Empty : desc_port_station.ADR_ID;
            return new ACMD()
            {
                ID = sequenceBLL.getCommandID(SCAppConstants.GenOHxCCommandType.Auto),
                TRANSFER_ID = this.ID,
                VH_ID = assignVehicle.VEHICLE_ID,
                CARRIER_ID = this.CARRIER_ID,
                CMD_TYPE = cmd_type,
                SOURCE = from_adr,
                DESTINATION = to_adr,
                PRIORITY = this.PRIORITY_SUM,
                CMD_INSER_TIME = DateTime.Now,
                CMD_STATUS = E_CMD_STATUS.Queue,
                SOURCE_PORT = host_source,
                DESTINATION_PORT = this.HOSTDESTINATION,
                COMPLETE_STATUS = ProtocolFormat.OHTMessage.CompleteStatus.CmpStatusMove
            };
        }

        public string getSourcePortGroupID(BLL.PortStationBLL portStationBLL)
        {
            var port_station = portStationBLL.OperateCatch.getPortStation(this.HOSTSOURCE);
            if (port_station == null) return "";
            return SCUtility.Trim(port_station.GROUP_ID, true);
        }
        public string getSourcePortEQID(BLL.PortStationBLL portStationBLL)
        {
            var port_station = portStationBLL.OperateCatch.getPortStation(this.HOSTSOURCE);
            if (port_station == null) return "";
            return SCUtility.Trim(port_station.EQPT_ID, true);
        }
        public AEQPT getSourcePortEQ(BLL.EqptBLL eqptBLL)
        {
            var eq = eqptBLL.OperateCatch.GetEqpt(this.HOSTSOURCE);
            if (eq == null) return null;
            return eq;
        }
        public string getSourcePortNodeID(BLL.PortStationBLL portStationBLL, BLL.EqptBLL eqptBLL)
        {
            var port_station = portStationBLL.OperateCatch.getPortStation(this.HOSTSOURCE);
            if (port_station == null) return "";
            var eq = port_station.GetEqpt(eqptBLL);
            if (eq == null) return "";
            return SCUtility.Trim(eq.NODE_ID, true);
        }

        public string getTragetPortEQID(BLL.PortStationBLL portStationBLL)
        {
            var port_station = portStationBLL.OperateCatch.getPortStation(this.HOSTDESTINATION);
            if (port_station == null) return "";
            return SCUtility.Trim(port_station.EQPT_ID, true);
        }

        public string getSourcePortAdrID(BLL.PortStationBLL portStationBLL)
        {
            var port_station = portStationBLL.OperateCatch.getPortStation(this.HOSTSOURCE);
            if (port_station == null) return "";
            return SCUtility.Trim(port_station.ADR_ID, true);
        }
        public string getTragetPortAdrID(BLL.PortStationBLL portStationBLL)
        {
            var port_station = portStationBLL.OperateCatch.getPortStation(this.HOSTDESTINATION);
            if (port_station == null) return "";
            return SCUtility.Trim(port_station.ADR_ID, true);
        }

        public AEQPT getTragetPortEQ(BLL.PortStationBLL portStationBLL, BLL.EqptBLL eqptBLL)
        {
            var port_station = portStationBLL.OperateCatch.getPortStation(this.HOSTDESTINATION);
            if (port_station == null) return null;
            return port_station.GetEqpt(eqptBLL);
        }
        public string getTragetPortNodeID(BLL.PortStationBLL portStationBLL, BLL.EqptBLL eqptBLL)
        {
            var port_station = portStationBLL.OperateCatch.getPortStation(this.HOSTDESTINATION);
            if (port_station == null) return "";
            var eq = port_station.GetEqpt(eqptBLL);
            if (eq == null) return "";
            return SCUtility.Trim(eq.NODE_ID, true);
        }
        public AEQPT getTragetPortEQ(BLL.EqptBLL eqptBLL)
        {
            var eq = eqptBLL.OperateCatch.GetEqpt(this.HOSTDESTINATION);
            if (eq == null) return null;
            return eq;
        }

        public bool IsTargetPortAGVStation(BLL.EqptBLL eqptBLL)
        {
            var eq = eqptBLL.OperateCatch.GetEqpt(this.HOSTDESTINATION);
            if (eq == null) return false;
            return eq is IAGVStationType;
        }

        public bool IsSourceOnVh(BLL.VehicleBLL vehicleBLL)
        {
            var vh = vehicleBLL.cache.getVehicleByLocationRealID(HOSTSOURCE);
            return vh != null;
        }

        public BLL.CMDBLL.CommandTranDir GetTransferDir()
        {
            return BLL.CMDBLL.GetTransferDir(this);
        }

        public bool IsQueueTimeOut
        {
            get
            {
                bool is_timeout = TRANSFERSTATE == E_TRAN_STATUS.Queue &&
                                                   DateTime.Now > CMD_INSER_TIME.AddMilliseconds(SystemParameter.TransferCommandQueueTimeOut_mSec);
                return is_timeout;
            }
        }

        public override string ToString()
        {
            return $"Command:{this.ID},source:{this.HOSTSOURCE},desc:{this.HOSTDESTINATION},inser time:{CMD_INSER_TIME.ToString()}";
        }

    }

}
