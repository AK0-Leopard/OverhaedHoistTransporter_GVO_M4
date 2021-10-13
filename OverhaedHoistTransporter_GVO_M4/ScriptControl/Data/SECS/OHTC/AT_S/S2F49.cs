using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.stc.Common;
using com.mirle.ibg3k0.stc.Data.SecsData;

namespace com.mirle.ibg3k0.sc.Data.SECS.OHTC.AT_S
{
    public class S2F49 : SXFY
    {
        [SecsElement(Index = 1, ListSpreadOut = true, ListElementType = SecsElement.SecsElementType.TYPE_4_BYTE_UNSIGNED_INTEGER, Length = 1)]
        public string DATAID;
        [SecsElement(Index = 2, ListSpreadOut = true, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
        public string OBJSPEC;
        //[SecsElement(Index = 3, ListSpreadOut = true, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
        [SecsElement(Index = 3, ListSpreadOut = true, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 20)]//修改長度為20 MarkChou 20190329
        public string RCMD;
        [SecsElement(Index = 4)]
        public REPITEM[] REPITEMS;
        public class REPITEM : SXFY
        {
            //[SecsElement(Index = 1, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
            //public string CPNAME;
            //[SecsElement(Index = 2, ListElementType = SecsElement.SecsElementType.TYPE_UNKNOWN, Length = 64)]
            //public string CPVAL;
        }

        public S2F49()
        {
            StreamFunction = "S2F49";
            StreamFunctionName = "Enhanced Remote Command Extension";
            W_Bit = 1;
        }
    }

    public class S2F49_STAGE : SXFY
    {
        [SecsElement(Index = 1, ListSpreadOut = true, ListElementType = SecsElement.SecsElementType.TYPE_4_BYTE_UNSIGNED_INTEGER, Length = 1)]
        public string DATAID;
        [SecsElement(Index = 2, ListSpreadOut = true, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
        public string OBJSPEC;
        [SecsElement(Index = 3, ListSpreadOut = true, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
        public string RCMD;
        [SecsElement(Index = 4)]
        public REPITEM REPITEMS;
        public class REPITEM : SXFY
        {
            [SecsElement(Index = 1)]
            public REPITEM_STAGEINOF STAGEINOF;

            [SecsElement(Index = 2)]
            public REPITEM_TRANSFERINFO TRANSFERINFO;


            public class REPITEM_STAGEINOF : SXFY
            {
                [SecsElement(Index = 1, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
                public string CPNAME;
                [SecsElement(Index = 2)]
                public REPITEM_STAGEINOF_CPVALUE CPVALUE;
                public class REPITEM_STAGEINOF_CPVALUE : SXFY
                {
                    [SecsElement(Index = 1)]
                    public REPITEM_ASCII STAGEID_CP;
                    [SecsElement(Index = 2)]
                    public REPITEM_U2 PRIORITY_CP;
                    [SecsElement(Index = 3)]
                    public REPITEM_U2 REPLACE_CP;
                    [SecsElement(Index = 4)]
                    public REPITEM_U2 EXPECTEDDURATION_CP;
                    [SecsElement(Index = 5)]
                    public REPITEM_U2 NOBLOCKINGTIME_CP;
                    [SecsElement(Index = 6)]
                    public REPITEM_U2 WAITTIMEOUT_CP;
                }
            }
            public class REPITEM_TRANSFERINFO : SXFY
            {
                [SecsElement(Index = 1, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
                public string CPNAME;
                [SecsElement(Index = 2)]
                public REPITEM_TRANSFERINFO_CPVALUE CPVALUE;
                public class REPITEM_TRANSFERINFO_CPVALUE : SXFY
                {
                    [SecsElement(Index = 1)]
                    public REPITEM_ASCII CARRIERID_CP;
                    [SecsElement(Index = 2)]
                    public REPITEM_ASCII SOURCEPORT_CP;
                    [SecsElement(Index = 3)]
                    public REPITEM_ASCII DESTPORT_CP;
                }
            }

            public class REPITEM_ASCII : SXFY
            {
                [SecsElement(Index = 1, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
                public string CPNAME2;
                [SecsElement(Index = 2, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
                public string CPVAL_ASCII;
            }
            public class REPITEM_U2 : SXFY
            {
                [SecsElement(Index = 1, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
                public string CPNAME3;
                [SecsElement(Index = 2, ListElementType = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                public string CPVAL_U2;
            }


        }



        public S2F49_STAGE()
        {
            StreamFunction = "S2F49";
            StreamFunctionName = "Enhanced Remote Command Extension";
            W_Bit = 1;
        }


    }

    public class S2F49_TRANSFER : SXFY
    {
        [SecsElement(Index = 1, ListSpreadOut = true, ListElementType = SecsElement.SecsElementType.TYPE_4_BYTE_UNSIGNED_INTEGER, Length = 1)]
        public string DATAID;
        [SecsElement(Index = 2, ListSpreadOut = true, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
        public string OBJSPEC;
        [SecsElement(Index = 3, ListSpreadOut = true, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
        public string RCMD;
        [SecsElement(Index = 4)]
        public REPITEM REPITEMS;

        public S2F49_TRANSFER()
        {
            StreamFunction = "S2F49";
            StreamFunctionName = "Enhanced Remote Command Extension";
            W_Bit = 1;
        }

        public class REPITEM : SXFY
        {
            [SecsElement(Index = 1)]
            public COMM COMMINFO;
            [SecsElement(Index = 2)]
            public TRAN TRANINFO;
            [SecsElement(Index = 3)]
            public TRAN_CST TRANINFO_CST;
            [SecsElement(Index = 4)]
            public STAGE STAGEINFO;

            public class COMM : SXFY
            {
                [SecsElement(Index = 1, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
                public string COMMANDINFONAME;
                [SecsElement(Index = 2)]
                public COMMVALUE COMMAINFOVALUE;

                public class COMMVALUE : SXFY
                {
                    [SecsElement(Index = 1)]
                    public CP_ASCII COMMANDID;
                    [SecsElement(Index = 2)]
                    public CP_U2 PRIORITY;
                    [SecsElement(Index = 3)]
                    public CP_U2 REPLACE;
                }
            }
            public class TRAN : SXFY
            {
                [SecsElement(Index = 1, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
                public string TRANSFERINFONAME;
                [SecsElement(Index = 2)]
                public TRANVALUE TRANSFERINFOVALUE;

                public class TRANVALUE : SXFY
                {
                    [SecsElement(Index = 1)]
                    public CP_ASCII CARRIERIDINFO;
                    [SecsElement(Index = 2)]
                    public CP_ASCII SOUINFO;
                    [SecsElement(Index = 3)]
                    public CP_ASCII DESTINFO;
                }
            }
            public class STAGE : SXFY
            {
                [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
                public string STAGEINFONAME;
                [SecsElement(Index = 2, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, ListElementLength = 64)]
                public string[] STAGEINFOVALUE;
            }

            public class TRAN_CST : SXFY
            {
                [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
                public string TRAN_CSTNAME;
                [SecsElement(Index = 2)]
                public TRAN_CST_VALUE TRAN_CSTVALUE;
            }

            public class TRAN_CST_VALUE : SXFY
            {
                [SecsElement(Index = 1)]
                public CP_ASCII LOTINFOVALUE;
                [SecsElement(Index = 2)]
                public CP_ASCII CARRIERTYPE;
            }

            public class CP_ASCII : SXFY
            {
                [SecsElement(Index = 1, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
                public string CPNAME;
                [SecsElement(Index = 2, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)] //Modify Length 1 > 10 By Kevin
                public string CPVAL;
            }

            public class CP_U2 : SXFY
            {
                [SecsElement(Index = 1, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
                public string CPNAME;
                [SecsElement(Index = 2, ListElementType = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                public string CPVAL;
            }

        }
        public ATRANSFER ToTRANSFER(BLL.PortStationBLL portStationBLL, BLL.EqptBLL eqprBLL)
        {
            string cmdID = REPITEMS.COMMINFO.COMMAINFOVALUE.COMMANDID.CPVAL;
            string priority = REPITEMS.COMMINFO.COMMAINFOVALUE.PRIORITY.CPVAL;
            string replace = REPITEMS.COMMINFO.COMMAINFOVALUE.REPLACE.CPVAL;
            string cstID = REPITEMS.TRANINFO.TRANSFERINFOVALUE.CARRIERIDINFO.CPVAL;
            string source = REPITEMS.TRANINFO.TRANSFERINFOVALUE.SOUINFO.CPVAL;
            string dest = REPITEMS.TRANINFO.TRANSFERINFOVALUE.DESTINFO.CPVAL;
            dest = tryGetVirturePortID(portStationBLL, eqprBLL, dest);
            string lot_id = tryGetLotID();
            string cst_type = tryGetCSTType();
            if (!int.TryParse(priority, out int ipriority))
            {
                NLog.LogManager.GetCurrentClassLogger().Warn("command id :{0} of priority parse fail. priority valus:{1}"
                            , cmdID
                            , priority);
            }
            if (!int.TryParse(replace, out int ireplace))
            {
                NLog.LogManager.GetCurrentClassLogger().Warn("command id :{0} of priority parse fail. priority valus:{1}"
                            , cmdID
                            , replace);
            }
            int port_priority = 0;
            if (!SCUtility.isEmpty(source))
            {
                APORTSTATION source_portStation = portStationBLL.OperateCatch.getPortStation(source);
                if (source_portStation == null)
                {
                    NLog.LogManager.GetCurrentClassLogger().Warn($"MCS cmd of hostsource port[{source} not exist.]");
                }
                else
                {
                    port_priority = source_portStation.PRIORITY;
                }
            }

            return new ATRANSFER
            {
                CARRIER_ID = cstID,
                ID = cmdID,
                COMMANDSTATE = 0,
                HOSTSOURCE = source,
                HOSTDESTINATION = dest,
                PRIORITY = ipriority,
                PAUSEFLAG = "0",
                CMD_INSER_TIME = DateTime.Now,
                TIME_PRIORITY = 0,
                PORT_PRIORITY = port_priority,
                PRIORITY_SUM = ipriority + port_priority,
                REPLACE = ireplace,
                LOT_ID = lot_id,
                cst_type = cst_type
            };
        }

        public static string tryGetVirturePortID(BLL.PortStationBLL portStationBLL, BLL.EqptBLL eqptBLL, string dest)
        {
            APORTSTATION port_station = portStationBLL.OperateCatch.getPortStation(dest);
            if (port_station == null) return dest;
            var eqpt = port_station.GetEqpt(eqptBLL);
            if (eqpt is AGVStation)
            {
                var agv_station = eqpt as AGVStation;
                var virtrue_port = agv_station.getAGVVirtruePort();
                if (agv_station.IsVirtrueUse)
                {
                    return SCUtility.Trim(virtrue_port.PORT_ID, true);
                }
                else
                {
                    return dest;
                }

            }
            else
            {
                return dest;
            }
        }

        private string tryGetLotID()
        {
            string lot_id = "";
            var lot_info = this.REPITEMS.TRANINFO_CST.TRAN_CSTVALUE.LOTINFOVALUE;
            if (lot_info != null)
                lot_id = lot_info.CPVAL;
            return lot_id;
        }
        private string tryGetCSTType()
        {
            string cst_type = "";
            var cst_type_info = this.REPITEMS.TRANINFO_CST.TRAN_CSTVALUE.CARRIERTYPE;
            if (cst_type_info != null)
                cst_type = cst_type_info.CPVAL;
            return cst_type;
        }
    }
}
