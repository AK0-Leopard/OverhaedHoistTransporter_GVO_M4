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
    public class MTLInfoDao : DaoBase
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
        public MTLInfo getMTLInfo(SCApplication app, string mtl_id)
        {
            try
            {
                DataTable dt = app.OHxCConfig.Tables["MTLINFO"];
                var query = from c in dt.AsEnumerable()
                            where c.Field<string>("NAME").Trim() == mtl_id.Trim()
                            select new MTLInfo
                            {
                                NAME = c.Field<string>("NAME"),
                                SEGMENT = c.Field<string>("SEGMENT"),
                                ADDRESS = c.Field<string>("ADDRESS"),
                                CAR_IN_BUFFER_ADDRESS = c.Field<string>("CAR_IN_BUFFER_ADDRESS"),
                                SYSTEM_IN_ADDRESS = c.Field<string>("SYSTEM_IN_ADDRESS"),
                                SYSTEM_OUT_ADDRESS = c.Field<string>("SYSTEM_OUT_ADDRESS")
                            };
                return query.FirstOrDefault();
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

    }
}
