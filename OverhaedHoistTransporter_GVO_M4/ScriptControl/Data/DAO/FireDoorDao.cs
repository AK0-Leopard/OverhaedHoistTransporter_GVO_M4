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
    public class FireDoorDao : DaoBase
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
        public List<string> loadFireDoorSegmentID(SCApplication app, string fireDoorID)
        {
            try
            {
                DataTable dt = app.OHxCConfig.Tables["FIREDOORSEGMENT"];
                var query = from c in dt.AsEnumerable()
                            where c.Field<string>("NAME").Trim() == fireDoorID.Trim()
                            select c.Field<string>("SEGMENT");
                return query.ToList();
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public string getFireDoorIDBySegmentID(SCApplication app, string segment_id)
        {
            try
            {
                DataTable dt = app.OHxCConfig.Tables["FIREDOORSEGMENT"];
                var query = from c in dt.AsEnumerable()
                            where c.Field<string>("SEGMENT").Trim() == segment_id.Trim()
                            select c.Field<string>("NAME");
                return query.FirstOrDefault();
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }
        public bool isSegmentIDatFireDoorArea(SCApplication app, string segment_id)
        {
            try
            {
                DataTable dt = app.OHxCConfig.Tables["FIREDOORSEGMENT"];
                var query = from c in dt.AsEnumerable()
                            where c.Field<string>("SEGMENT").Trim() == segment_id.Trim()
                            select c.Field<string>("NAME");
                if (query.FirstOrDefault() != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
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
