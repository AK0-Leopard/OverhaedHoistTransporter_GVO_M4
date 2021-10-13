
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace com.mirle.ibg3k0.sc.Data.DAO
{
    public class GroupPortStationDao
    {
        public void add(DBConnection_EF con, AGROUPPORTSTATION point)
        {
            con.AGROUPPORTSTATION.Add(point);
            con.SaveChanges();
        }

        public void Update(DBConnection_EF con, AGROUPPORTSTATION point)
        {
            //bool isDetached = con.Entry(point).State == EntityState.Modified;
            //if (isDetached)
            con.SaveChanges();
        }

        public List<AGROUPPORTSTATION> loadAll(DBConnection_EF con)
        {
            var query = from dbo in con.AGROUPPORTSTATION
                        select dbo;
            return query.ToList();
        }
    }
}
