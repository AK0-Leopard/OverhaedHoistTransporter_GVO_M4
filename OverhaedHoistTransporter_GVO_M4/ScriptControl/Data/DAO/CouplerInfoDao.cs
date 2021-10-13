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

namespace com.mirle.ibg3k0.sc.Data.DAO
{
    /// <summary>
    /// Class AlarmMapDao.
    /// </summary>
    /// <seealso cref="com.mirle.ibg3k0.bcf.Data.DaoBase" />
    public class CouplerInfoDao : DaoBase
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
        public CouplerData getCouplerInfo(SCApplication app, string adrID)
        {
            try
            {
                DataTable dt = app.OHxCConfig.Tables["COUPLERINFO"];
                var query = from c in dt.AsEnumerable()
                            where c.Field<string>("ADDRESS_ID").Trim() == adrID.Trim()
                            select new CouplerData
                            {
                                AddressID = c.Field<string>("ADDRESS_ID"),
                                Priority = stringToInt(c.Field<string>("PRIORITY")),
                                TrafficControlSegment = c.Field<string>("TRAFFIC_CONTROL_SEGMENT"),
                                ChargerID = c.Field<string>("CHARGER_ID"),
                                CouplerNum = stringToInt(c.Field<string>("COUPLER_NUM"))
                            };
                return query.SingleOrDefault();
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        int stringToInt(string value)
        {
            int i_value = 0;
            int.TryParse(value, out i_value);
            return i_value;
        }

    }
}
