using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.VO
{
    public class ReserveEnhanceInfo
    {
        public string AddressID;
        public string WillPassSection;
        public string[] EnhanceControlAddress;
    }
    public class ReserveEnhanceInfoSection
    {
        public string BlockID;
        public List<ProtocolFormat.OHTMessage.ReserveInfo> EntrySectionInfos;
        public string[] EnhanceControlSections;
        public string WayOutAddress;
    }


}
