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
    /// Individual Report Data
    /// </summary>
    [Serializable]
    public class S6F20 : SXFY
    {
        [SecsElement(Index = 1,ListSpreadOut = true)]
        public SXFY[] VIDITEM;
        public S6F20()
        {
            StreamFunction = "S6F20";
            StreamFunctionName = "Individual Report Data";
            W_Bit = 1;
            IsBaseType = true;
        }
    }
}
