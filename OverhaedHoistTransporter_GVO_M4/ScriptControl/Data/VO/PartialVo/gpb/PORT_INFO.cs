using com.mirle.ibg3k0.sc.App;
using System;
using System.Globalization;

namespace com.mirle.ibg3k0.sc.ProtocolFormat.SystemClass.PortInfo
{
    public partial class PORT_INFO
    {
        public DateTime dTimeStamp
        {
            get
            {
                DateTime dateTime = DateTime.MinValue;
                DateTime.TryParseExact(this.Timestamp, SCAppConstants.TimestampFormat_17, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
                return dateTime;
            }
        }
    }

}
