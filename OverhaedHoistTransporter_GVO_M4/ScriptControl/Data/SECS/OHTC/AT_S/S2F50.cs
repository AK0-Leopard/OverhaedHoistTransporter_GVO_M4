using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.stc.Common;
using com.mirle.ibg3k0.stc.Data.SecsData;

namespace com.mirle.ibg3k0.sc.Data.SECS.OHTC.AT_S
{
    public class S2F50 : SXFY
    {
        [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_BINARY, Length = 1)]
        public string HCACK;
        [SecsElement(Index = 2)]
        public SXFY[] CEPCOLLECT;

        public S2F50()
        {
            StreamFunction = "S2F50";
            StreamFunctionName = "Enhanced Remote Command Ack";
            W_Bit = 0;
        }

        //public class REPITEM : SXFY
        //{
        //    //[SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
        //    [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]//修改長度為64 MarkChou 20190329
        //    public string CPNAME;
        //    [SecsElement(Index = 2, Type = SecsElement.SecsElementType.TYPE_BINARY, Length = 1)]
        //    public string CPACK;
        //}


        public class CEPITEM : SXFY
        {
            [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
            public string NAME;
            [SecsElement(Index = 2)]
            public CP_U1[] CPINFO;
        }

        public class CEPITEMS : SXFY
        {
            [SecsElement(Index = 1, ListSpreadOut = true)]
            public CEPITEM[] CEPINFO;
        }

        public class CP_U1 : SXFY
        {
            [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
            public string CPNAME;
            [SecsElement(Index = 2, Type = SecsElement.SecsElementType.TYPE_1_BYTE_UNSIGNED_INTEGER, Length = 1)]
            public string CEPACK;
        }


        //public class CEPCOLLECT : SXFY
        //{
        //    [SecsElement(Index = 1, ListSpreadOut = true)]
        //    public CEP[] CEPS;
        //    public class CEP : SXFY
        //    {
        //        [SecsElement(Index = 1, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
        //        public string NAME;
        //        [SecsElement(Index = 2)]
        //        public VALUE CPINFO;
        //        public class VALUE : SXFY
        //        {
        //            [SecsElement(Index = 1, ListSpreadOut = true)]
        //            public CP_U1[] CPS;
        //        }
        //        public class CP_U1 : SXFY
        //        {
        //            [SecsElement(Index = 1, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]
        //            public string CPNAME;
        //            [SecsElement(Index = 2, ListElementType = SecsElement.SecsElementType.TYPE_1_BYTE_UNSIGNED_INTEGER, Length = 64)]
        //            public string CEPACK;
        //        }

        //    }


        //}

    }
}
