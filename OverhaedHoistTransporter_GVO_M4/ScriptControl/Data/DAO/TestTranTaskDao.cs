using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace com.mirle.ibg3k0.sc.Data.DAO
{
    public class TestTranTaskDao
    {

        public TestTranTaskDao()
        {
        }

        public List<TranTask> loadTransferTasks_ACycle(DataTable TranCmdPeriodicDataTable)
        {
            List<TranTask> lstTranTask = new List<TranTask>();
            var query = TranCmdPeriodicDataTable.AsEnumerable();
            foreach (DataRow row in query)
            {
                for (int i = 0; i < row.ItemArray.Count(); i++)
                {
                    if (i == 0)
                        continue; //跳過第一行
                    string cellData = row[i].ToString();
                    double icellData = 0;
                    if (double.TryParse(cellData, out icellData))
                    {
                        icellData = Math.Round(icellData, 0);
                    }

                    if (icellData != 0)
                    {
                        lstTranTask.Add(new TranTask()
                        {
                            SourcePort = row[0].ToString(),
                            DestinationPort = row.Table.Columns[i].ColumnName,
                            EachPeriodicCount = icellData.ToString()
                        });
                    }
                }
            }
            return lstTranTask;
        }

        public List<TranTask> loadTransferTasks_24Hour(DataTable TranCmdPeriodicDataTable)
        {
            List<TranTask> lstTranTask = new List<TranTask>();

            var query = TranCmdPeriodicDataTable.AsEnumerable();
            foreach (DataRow row in query)
            {
                string source = row["HOSTSOURCE"].ToString();
                string destination = row["HOSTDESTINATION"].ToString();
                if (string.IsNullOrWhiteSpace(source) ||
                    string.IsNullOrWhiteSpace(destination))
                    continue;
                string sSec = row["SEC"].ToString();
                string sPriority = row["PRIORITY"].ToString();
                string CSTID = row["CARRIER_ID"].ToString();

                Double iSec = 0;
                Double.TryParse(sSec, out iSec);
                //iSec = Math.Round(iSec, 0, MidpointRounding.AwayFromZero);
                lstTranTask.Add(new TranTask()
                {
                    Sec = (int)iSec,
                    SourcePort = source,
                    DestinationPort = destination,
                    Priority = sPriority,
                    CSTID = CSTID,
                    CarType = sPriority
                });

            }
            return lstTranTask;
        }
        //public List<TranTask> loadTransferTasks_245Hour()
        //{
        //    List<TranTask> lstTranTask = new List<TranTask>();
        //    Dictionary<int, List<TranTask>> dicTemp = null;
        //    var query = from row in TranCmdTaskDataSet.Tables[1].AsEnumerable()
        //                group row by row.Field<string>("MIN");
        //    dicTemp = query.ToDictionary(item => item.Key, item => new TranTask { SourcePort = item. });

        //    var query = TranCmdTaskDataSet.Tables[1].AsEnumerable();

        //    foreach (DataRow row in query)
        //    {
        //        lstTranTask.Add(new TranTask()
        //        {
        //            SourcePort = row[2].ToString(),
        //            DestinationPort = row[3].ToString()
        //        });
        //    }
        //    return lstTranTask;
        //}

    }

    public class TranTask
    {
        private int min;
        public int Min { get { return min; } set { min = value; } }
        private int sec;
        public int Sec { get { return sec; } set { sec = value; } }
        private string cstid;
        public string CSTID { get { return cstid; } set { cstid = value; } }
        private string priority;
        public string Priority { get { return priority; } set { priority = value; } }
        private string sourceport;
        public string SourcePort { get { return sourceport; } set { sourceport = value; } }
        private string destinationport;
        public string DestinationPort { get { return destinationport; } set { destinationport = value; } }
        private string eachperiodiccount;
        public string EachPeriodicCount { get { return eachperiodiccount; } set { eachperiodiccount = value; } }
        private string cartype;
        public string CarType { get { return cartype; } set { cartype = value; } }

    }

}
