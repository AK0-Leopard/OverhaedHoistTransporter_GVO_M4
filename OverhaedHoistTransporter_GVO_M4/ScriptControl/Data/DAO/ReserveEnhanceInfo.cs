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
using com.mirle.ibg3k0.sc.Common;

namespace com.mirle.ibg3k0.sc.Data.DAO
{
    /// <summary>
    /// Class AlarmMapDao.
    /// </summary>
    /// <seealso cref="com.mirle.ibg3k0.bcf.Data.DaoBase" />
    public class ReserveEnhanceInfoDao : DaoBase
    {
        /// <summary>
        /// The logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Gets the alarm map.
        /// </summary>
        /// <param name="object_id">The eqpt_real_id.</param>
        /// <param name="vhID">The alarm_id.</param>
        /// <returns>AlarmMap.</returns>
        public ReserveEnhanceInfo getReserveInfo(SCApplication app, string adrID)
        {
            try
            {
                DataTable dt = app.OHxCConfig.Tables["RESERVEENHANCEINFO"];
                var query = from c in dt.AsEnumerable()
                            where c.Field<string>("ADDRESS_ID").Trim() == adrID.Trim()
                            select new ReserveEnhanceInfo
                            {
                                AddressID = c.Field<string>("ADDRESS_ID"),
                                WillPassSection = c.Field<string>("WILL_PASS_SECTION"),
                                EnhanceControlAddress = stringToStringArray(c.Field<string>("ENHAN_CECONTROL_ADDRESS"))
                            };
                return query.FirstOrDefault();
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }
        public List<ReserveEnhanceInfo> getReserveInfos(SCApplication app, string adrID)
        {
            try
            {
                DataTable dt = app.OHxCConfig.Tables["RESERVEENHANCEINFO"];
                var query = from c in dt.AsEnumerable()
                            where c.Field<string>("ADDRESS_ID").Trim() == adrID.Trim()
                            select new ReserveEnhanceInfo
                            {
                                AddressID = c.Field<string>("ADDRESS_ID"),
                                WillPassSection = c.Field<string>("WILL_PASS_SECTION"),
                                EnhanceControlAddress = stringToStringArray(c.Field<string>("ENHAN_CECONTROL_ADDRESS"))
                            };
                return query.ToList();
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }
        public List<ReserveEnhanceInfoSection> getReserveEnhanceInfoSections(SCApplication app)
        {
            try
            {
                DataTable dt = app.OHxCConfig.Tables["RESERVEENHANCEINFO"];
                var query = from c in dt.AsEnumerable()
                            select new ReserveEnhanceInfoSection
                            {
                                BlockID = c.Field<string>("BLOCK_ID"),
                                EntrySectionInfos = GetEntrySectionInfos(c.Field<string>("ENTRY_SECTION_ID")),
                                EnhanceControlSections = stringToStringArray(c.Field<string>("ENHAN_CONTROL_SECTION")),
                                WayOutAddress = c.Field<string>("WAY_OUT_ADDRESS")
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
            if (SCUtility.isEmpty(entrySectionInfos)) return reserve_infos;
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
