// ***********************************************************************
// Assembly         : ScriptControl
// Author           : 
// Created          : 03-31-2016
//
// Last Modified By : 
// Last Modified On : 03-24-2016
// ***********************************************************************
// <copyright file="AlarmMapDao.cs" company="">
//     Copyright ©  2014
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.bcf.Data;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Data.VO;
using NLog;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;

namespace com.mirle.ibg3k0.sc.Data.DAO
{
    /// <summary>
    /// Class AlarmMapDao.
    /// </summary>
    /// <seealso cref="com.mirle.ibg3k0.bcf.Data.DaoBase" />
    public class TrafficControlInfoDao : DaoBase
    {
        /// <summary>
        /// The logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public List<TrafficControlInfo> getTrafficControlInfos(SCApplication app)
        {
            try
            {
                DataTable dt = app.OHxCConfig.Tables["TRAFFICCONTROLINFO"];
                var query = from c in dt.AsEnumerable()
                            select new TrafficControlInfo
                            {
                                ID = c.Field<string>("ID"),
                                EntrySectionInfos = GetEntrySectionInfos(c.Field<string>("ENTRY_SECTION")),
                                ControlSections = stringToStringArray(c.Field<string>("CONTROL_SECTION"))
                            };
                return query.ToList();
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }


        List<ProtocolFormat.OHTMessage.ReserveInfo> GetEntrySectionInfos(string entrySectionInfos)
        {
            List<ProtocolFormat.OHTMessage.ReserveInfo> reserve_infos = new List<ReserveInfo>();
            if (entrySectionInfos.Contains("-"))
            {
                string[] infos = entrySectionInfos.Split('-');
                foreach (string reserve in infos)
                {
                    string[] reserve_temp = reserve.Split('/'); // section_id/dir
                    var reserve_info = new ProtocolFormat.OHTMessage.ReserveInfo()
                    {
                        ReserveSectionID = reserve_temp[0],
                        DriveDirction = GetDriveDirction(reserve_temp[1])
                    };
                    reserve_infos.Add(reserve_info);
                }
            }
            else
            {
                string[] reserve_temp = entrySectionInfos.Split('/'); // section_id/dir
                var reserve_info = new ProtocolFormat.OHTMessage.ReserveInfo()
                {
                    ReserveSectionID = reserve_temp[0],
                    DriveDirction = GetDriveDirction(reserve_temp[1])
                };
                reserve_infos.Add(reserve_info);
            }
            return reserve_infos;
        }

        DriveDirction GetDriveDirction(string dir)
        {
            switch (dir)
            {
                case "F":
                    return DriveDirction.DriveDirForward;
                case "R":
                    return DriveDirction.DriveDirReverse;
                default:
                    return DriveDirction.DriveDirNone;
            }
        }

        string[] stringToStringArray(string value)
        {
            if (value.Contains("-"))
            {
                return value.Split('-');
            }
            else
            {
                return new string[] { value };
            }
        }

    }
}
