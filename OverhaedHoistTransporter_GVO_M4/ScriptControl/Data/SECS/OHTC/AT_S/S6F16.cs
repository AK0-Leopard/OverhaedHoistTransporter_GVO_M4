using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.stc.Common;
using com.mirle.ibg3k0.stc.Data.SecsData;

namespace com.mirle.ibg3k0.sc.Data.SECS.OHTC.AT_S
{
    /// <summary>
    /// Event Report Data
    /// </summary>
    [Serializable]
    public class S6F16 : SXFY
    {
        [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_4_BYTE_UNSIGNED_INTEGER, Length = 1)]
        public string DATAID;
        [SecsElement(Index = 2, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
        public string CEID;
        [SecsElement(Index = 3)]
        public RPTINFO INFO;

        public S6F16()
        {
            StreamFunction = "S6F16";
            StreamFunctionName = "Event Report Data";
            W_Bit = 0;
            IsBaseType = true;
            INFO = new RPTINFO();
        }
        public override string ToString()
        {
            return string.Concat(StreamFunction, "-", CEID);
        }

        [Serializable]
        public class RPTINFO : SXFY
        {
            [SecsElement(Index = 1, ListSpreadOut = true)]
            public RPTITEM[] ITEM;
            [Serializable]
            public class RPTITEM : SXFY
            {
                [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                public string RPTID;
                [SecsElement(Index = 2)]
                public SXFY[] VIDITEM;


                [Serializable]
                public class VIDITEM_01 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string ALID;
                    public VIDITEM_01()
                    {
                        ALID = string.Empty;
                    }
                }

                [Serializable]
                public class VIDITEM_02 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string ECT;
                    public VIDITEM_02()
                    {
                        ECT = string.Empty;
                    }
                }

                [Serializable]
                public class VIDITEM_03 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, ListElementType = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, ListElementLength = 1)]
                    public string[] ALIDs;
                    public VIDITEM_03()
                    {
                        ALIDs = new string[0];
                    }
                }

                [Serializable]
                public class VIDITEM_04 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, ListElementType = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, ListElementLength = 1)]
                    public string[] ALIDs;
                    public VIDITEM_04()
                    {
                        ALIDs = new string[0];
                    }
                }

                [Serializable]
                public class VIDITEM_05 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 16)]
                    public string CLOCK;
                    public VIDITEM_05()
                    {
                        CLOCK = string.Empty;
                    }
                }

                [Serializable]
                public class VIDITEM_06 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_BINARY, Length = 1)]
                    public string CONTROLSTATE;
                    public VIDITEM_06()
                    {
                        CONTROLSTATE = string.Empty;
                    }
                }

                [Serializable]
                public class VIDITEM_07 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, ListElementType = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, ListElementLength = 1)]
                    public string[] CEIDs;
                    public VIDITEM_07()
                    {
                        CEIDs = new string[0];
                    }
                }


                [Serializable]
                public class VIDITEM_51 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public VIDITEM_55[] CARRIER_INFO;
                }

                [Serializable]
                public class VIDITEM_52 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public VIDITEM_69[] TRANSFER_COMMANDs;
                    public VIDITEM_52()
                    {
                        TRANSFER_COMMANDs = new VIDITEM_69[1];
                        TRANSFER_COMMANDs[0] = new VIDITEM_69();
                    }
                }

                [Serializable]
                public class VIDITEM_53 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public VIDITEM_75[] VEHICLEINFO;
                    public VIDITEM_53()
                    {
                        VEHICLEINFO = new VIDITEM_75[1];
                        VEHICLEINFO[0] = new VIDITEM_75();
                    }
                }


                [Serializable]
                public class VIDITEM_54 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string CARRIER_ID;
                    public VIDITEM_54()
                    {
                        CARRIER_ID = string.Empty;
                    }
                }


                [Serializable]
                public class VIDITEM_55 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string CARRIER_ID;
                    [SecsElement(Index = 2, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 32)]
                    public string VEHICLE_ID;
                    [SecsElement(Index = 3, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string CARRIER_LOC;

                    public VIDITEM_55()
                    {
                        CARRIER_ID = String.Empty;
                        VEHICLE_ID = String.Empty;
                        CARRIER_LOC = String.Empty;
                    }
                }

                [Serializable]
                public class VIDITEM_56 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string CARRIER_LOC;
                    public VIDITEM_56()
                    {
                        CARRIER_LOC = string.Empty;
                    }
                }

                [Serializable]
                public class VIDITEM_57 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 20)]
                    public string COMMAND_NAME;
                    public VIDITEM_57()
                    {
                        COMMAND_NAME = string.Empty;
                    }
                }

                [Serializable]
                public class VIDITEM_58 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string COMMAND_ID;
                    public VIDITEM_58()
                    {
                        COMMAND_ID = string.Empty;
                    }
                }

                [Serializable]
                public class VIDITEM_59 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string COMMAND_ID;
                    [SecsElement(Index = 2, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string PRIORITY;
                    [SecsElement(Index = 3, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string PEPLACE;
                    public VIDITEM_59()
                    {
                        COMMAND_ID = string.Empty;
                        PRIORITY = string.Empty;
                        PEPLACE = string.Empty;
                    }
                }

                [Serializable]
                public class VIDITEM_60 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 20)]
                    public string COMMAND_TYPE;
                    public VIDITEM_60()
                    {
                        COMMAND_TYPE = string.Empty;
                    }
                }

                [Serializable]
                public class VIDITEM_61 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string DEST_PORT;
                    public VIDITEM_61()
                    {
                        DEST_PORT = string.Empty;
                    }
                }

                [Serializable]
                public class VIDITEM_62 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public ENHANCED_CARRIER_INFO[] ENHANCED_CARRIER_INFOs;

                    public VIDITEM_62()
                    {
                        ENHANCED_CARRIER_INFOs = new ENHANCED_CARRIER_INFO[1];
                        ENHANCED_CARRIER_INFOs[0] = new ENHANCED_CARRIER_INFO();
                    }
                    public class ENHANCED_CARRIER_INFO : SXFY
                    {
                        [SecsElement(Index = 1, ListSpreadOut = true)]
                        public VIDITEM_54 CARRIER_ID_OBJ;
                        [SecsElement(Index = 2, ListSpreadOut = true)]
                        public VIDITEM_74 VEHICLE_ID_OBJ;
                        [SecsElement(Index = 3, ListSpreadOut = true)]
                        public VIDITEM_56 CARRIER_LOC_OBJ;
                        [SecsElement(Index = 4, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 16)]
                        public string INSTALL_TIME_OBJ;

                        public ENHANCED_CARRIER_INFO()
                        {
                            CARRIER_ID_OBJ = new VIDITEM_54();
                            VEHICLE_ID_OBJ = new VIDITEM_74();
                            CARRIER_LOC_OBJ = new VIDITEM_56();
                            INSTALL_TIME_OBJ = string.Empty;
                        }
                    }
                }

                [Serializable]
                public class VIDITEM_63 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public ENHANCED_TRANSFER_COMMAND[] ENHANCED_CARRIER_INFOs;

                    public VIDITEM_63()
                    {
                        ENHANCED_CARRIER_INFOs = new ENHANCED_TRANSFER_COMMAND[1];
                        ENHANCED_CARRIER_INFOs[0] = new ENHANCED_TRANSFER_COMMAND();
                    }
                    public class ENHANCED_TRANSFER_COMMAND : SXFY
                    {
                        [SecsElement(Index = 1, ListSpreadOut = true)]
                        public VIDITEM_59 COMMAND_INFO_OBJ;
                        [SecsElement(Index = 2, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                        public string TRANSFER_STATE;
                        [SecsElement(Index = 3, ListSpreadOut = true)]
                        public VIDITEM_70[] TRANSFER_COMMANDS;

                        public ENHANCED_TRANSFER_COMMAND()
                        {
                            COMMAND_INFO_OBJ = new VIDITEM_59();
                            TRANSFER_STATE = string.Empty;
                            TRANSFER_COMMANDS = new VIDITEM_70[1];
                            TRANSFER_COMMANDS[0] = new VIDITEM_70();
                        }
                    }
                }

                [Serializable]
                public class VIDITEM_64 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 32)]
                    public string EQP_NAME;
                    public VIDITEM_64()
                    {
                        EQP_NAME = string.Empty;
                    }
                }

                [Serializable]
                public class VIDITEM_65 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string PRIORITY;
                    public VIDITEM_65()
                    {
                        PRIORITY = string.Empty;
                    }
                }

                [Serializable]
                public class VIDITEM_66 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string REPLACE;
                    public VIDITEM_66()
                    {
                        REPLACE = string.Empty;
                    }
                }
                [Serializable]
                public class VIDITEM_67 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string RESULT_CODE;
                    public VIDITEM_67()
                    {
                        RESULT_CODE = string.Empty;
                    }
                }
                [Serializable]
                public class VIDITEM_68 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string SOURCE_PORT;
                    public VIDITEM_68()
                    {
                        SOURCE_PORT = string.Empty;
                    }
                }
                [Serializable]
                public class VIDITEM_69 : SXFY
                {
                    [SecsElement(Index = 1)]
                    public VIDITEM_59 COMMAND_INFO;
                    [SecsElement(Index = 2)]
                    public VIDITEM_70[] TRANSFER_INFOs;
                    public VIDITEM_69()
                    {
                        COMMAND_INFO = new VIDITEM_59();
                        TRANSFER_INFOs = new VIDITEM_70[1];
                        TRANSFER_INFOs[0] = new VIDITEM_70();
                    }
                }

                [Serializable]
                public class VIDITEM_70 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public VIDITEM_54 CARRIER_ID;
                    [SecsElement(Index = 2, ListSpreadOut = true)]
                    public VIDITEM_68 SOURCE_PORT;
                    [SecsElement(Index = 3, ListSpreadOut = true)]
                    public VIDITEM_61 DEST_PORT;
                    public VIDITEM_70()
                    {
                        CARRIER_ID = new VIDITEM_54();
                        SOURCE_PORT = new VIDITEM_68();
                        DEST_PORT = new VIDITEM_61();
                    }
                }

                [Serializable]
                public class VIDITEM_71 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string TRANSFER_PORT;
                    public VIDITEM_71()
                    {
                        TRANSFER_PORT = string.Empty;
                    }
                }

                [Serializable]
                public class VIDITEM_72 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public VIDITEM_71[] TRANSFER_PORT_LIST;
                    public VIDITEM_72()
                    {
                        TRANSFER_PORT_LIST = new VIDITEM_71[1];
                        TRANSFER_PORT_LIST[0] = new VIDITEM_71();
                    }
                }

                [Serializable]
                public class VIDITEM_73 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string TSC_STATE;
                    public VIDITEM_73()
                    {
                        TSC_STATE = string.Empty;
                    }
                }


                [Serializable]
                public class VIDITEM_74 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 32)]
                    public string VEHICLE_ID;
                    public VIDITEM_74()
                    {
                        VEHICLE_ID = string.Empty;
                    }
                }

                [Serializable]
                public class VIDITEM_75 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public VEHICLEINFO VHINFO;
                    public VIDITEM_75()
                    {
                        VHINFO = new VEHICLEINFO()
                        {
                            VEHICLE_ID = string.Empty,
                            VEHICLE_STATE = string.Empty
                        };
                    }
                    [Serializable]
                    public class VEHICLEINFO : SXFY
                    {
                        [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 32)]
                        public string VEHICLE_ID;
                        [SecsElement(Index = 2, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                        public string VEHICLE_STATE;
                    }
                }

                [Serializable]
                public class VIDITEM_76 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string VEHICLE_STATE;
                    public VIDITEM_76()
                    {
                        VEHICLE_STATE = string.Empty;
                    }
                }

                [Serializable]
                public class VIDITEM_301 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public TRANSFER_COMPLETE_INFO[] TRANSFER_COMPLETE_INFOs;
                    public VIDITEM_301()
                    {
                        TRANSFER_COMPLETE_INFOs = new TRANSFER_COMPLETE_INFO[1];
                        TRANSFER_COMPLETE_INFOs[0] = new TRANSFER_COMPLETE_INFO();
                    }

                    [Serializable]
                    public class TRANSFER_COMPLETE_INFO : SXFY
                    {
                        [SecsElement(Index = 1)]
                        public VIDITEM_70 TRANSFER_INFO_OBJ;
                        [SecsElement(Index = 2)]
                        public VIDITEM_56 CARRIER_LOC_OBJ;
                        public TRANSFER_COMPLETE_INFO()
                        {
                            TRANSFER_INFO_OBJ = new VIDITEM_70();
                            CARRIER_LOC_OBJ = new VIDITEM_56();
                        }
                    }
                }

                [Serializable]
                public class VIDITEM_302 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string PORT_ID;
                    public VIDITEM_302()
                    {
                        PORT_ID = string.Empty;
                    }
                }

                [Serializable]
                public class VIDITEM_303 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string PORT_EVT_STATE;
                    public VIDITEM_303()
                    {
                        PORT_EVT_STATE = string.Empty;
                    }
                }

                [Serializable]
                public class VIDITEM_304 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public PORTEVENTSTATE PESTATE;
                    public VIDITEM_304()
                    {
                        PESTATE = new PORTEVENTSTATE()
                        {
                            PORT_ID = string.Empty,
                            PORT_EVT_STATE = new VIDITEM_303()
                        };
                    }
                    [Serializable]
                    public class PORTEVENTSTATE : SXFY
                    {
                        [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                        public string PORT_ID;
                        [SecsElement(Index = 2)]
                        public VIDITEM_303 PORT_EVT_STATE;
                    }
                }

                [Serializable]
                public class VIDITEM_305 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true)]
                    public VIDITEM_304[] PORT_EVENT_STATEs;
                    public VIDITEM_305()
                    {
                        PORT_EVENT_STATEs = new VIDITEM_304[1];
                        PORT_EVENT_STATEs[1] = new VIDITEM_304();
                    }
                }

                [Serializable]
                public class VIDITEM_310 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
                    public string NEAR_STOCKER_PORT;
                    public VIDITEM_310()
                    {
                        NEAR_STOCKER_PORT = string.Empty;
                    }
                }

                [Serializable]
                public class VIDITEM_311 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string CURRENT_NODE;
                    public VIDITEM_311()
                    {
                        CURRENT_NODE = string.Empty;
                    }
                }

                [Serializable]
                public class VIDITEM_901 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
                    public string ALARM_TEXT;
                    public VIDITEM_901()
                    {
                        ALARM_TEXT = string.Empty;
                    }
                }

                [Serializable]
                public class VIDITEM_902 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 32)]
                    public string CHANGER_ID;
                    public VIDITEM_902()
                    {
                        CHANGER_ID = string.Empty;
                    }
                }

                [Serializable]
                public class VIDITEM_903 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                    public string ERROR_CODE;
                    public VIDITEM_903()
                    {
                        ERROR_CODE = string.Empty;
                    }
                }

                [Serializable]
                public class VIDITEM_904 : SXFY
                {
                    [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 32)]
                    public string UNIT_ID;
                    public VIDITEM_904()
                    {
                        UNIT_ID = string.Empty;
                    }
                }

            }
        }
    }
}
