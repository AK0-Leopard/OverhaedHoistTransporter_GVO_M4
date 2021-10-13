using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.Data.DAO;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class BlockControlBLL
    {
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        SCApplication scApp = null;
        BlockZoneMasterDao blockZoneMasterDao = null;
        BlockZoneDetailDao blockZoneDetaiDao = null;
        BlockZoneQueueDao blockZoneQueueDao = null;

        public void start(SCApplication app)
        {
            scApp = app;
            blockZoneMasterDao = scApp.BlockZoneMasterDao;
            blockZoneDetaiDao = scApp.BolckZoneDetaiDao;
            blockZoneQueueDao = scApp.BlockZoneQueueDao;
        }

        public bool CraetBlockZoneQueueRequest(string car_id, string entry_sec_id)
        {
            bool isSeccess = true;
            BLOCKZONEQUEUE blockObj = new BLOCKZONEQUEUE
            {
                CAR_ID = car_id,
                ENTRY_SEC_ID = entry_sec_id,
                REQ_TIME = DateTime.Now,
                STATUS = SCAppConstants.BlockQueueState.Request
            };
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                blockZoneQueueDao.add(con, blockObj);
            }
            return isSeccess;
        }

        public bool CraetBlockZoneQueueBlocking(string car_id, string entry_sec_id)
        {
            bool isSeccess = true;
            BLOCKZONEQUEUE blockObj = new BLOCKZONEQUEUE
            {
                CAR_ID = car_id,
                ENTRY_SEC_ID = entry_sec_id,
                REQ_TIME = DateTime.Now,
                BLOCK_TIME = DateTime.Now,
                STATUS = SCAppConstants.BlockQueueState.Blocking
            };
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                blockZoneQueueDao.add(con, blockObj);
            }
            return isSeccess;
        }

        public bool updateBlockZoneQueueBlockTime(string car_id, string entry_sec_id)
        {
            bool isSeccess = true;

            //DBConnection_EF con = DBConnection_EF.GetContext(out isNew);
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                BLOCKZONEQUEUE blockObj = blockZoneQueueDao.getUsingBlockQueueByCarIDSecID(con, car_id, entry_sec_id);
                if (blockObj == null) return false;
                blockObj.BLOCK_TIME = DateTime.Now;
                blockObj.STATUS = SCAppConstants.BlockQueueState.Blocking;
                blockZoneQueueDao.Update(con, blockObj);
            }
            return isSeccess;
        }
        public bool updateBlockZoneQueue_ThrouTime(string car_id)
        {
            bool isSeccess = true;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                BLOCKZONEQUEUE throu_block_queue = blockZoneQueueDao.getThrouTimeNullBlockQueueByCarID(con, car_id);
                if (throu_block_queue != null)
                {

                    throu_block_queue.THROU_TIME = DateTime.Now;
                    throu_block_queue.STATUS = SCAppConstants.BlockQueueState.Through;
                    con.Entry(throu_block_queue).Property(p => p.THROU_TIME).IsModified = true;
                    con.Entry(throu_block_queue).Property(p => p.STATUS).IsModified = true;

                    blockZoneQueueDao.Update(con, throu_block_queue);
                }
            }
            return isSeccess;
        }

        public bool updateBlockZoneQueue_ReleasTime(string car_id, string current_sec_id)
        {
            bool isSeccess = true;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                BLOCKZONEQUEUE blockObj = blockZoneQueueDao.getUsingBlockQueueByCarIDSecID(con, car_id, current_sec_id);
                if (blockObj != null)
                {
                    blockObj.RELEASE_TIME = DateTime.Now;
                    blockObj.STATUS = SCAppConstants.BlockQueueState.Release;
                    con.Entry(blockObj).Property(p => p.RELEASE_TIME).IsModified = true;
                    con.Entry(blockObj).Property(p => p.STATUS).IsModified = true;

                    blockZoneQueueDao.Update(con, blockObj);
                }
            }
            return isSeccess;
        }

        public bool updateBlockZoneQueue_ForceRelease(string car_id, string current_sec_id)
        {
            bool isSeccess = true;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                BLOCKZONEQUEUE blockObj = blockZoneQueueDao.getUsingBlockQueueByCarIDSecID(con, car_id, current_sec_id);
                if (blockObj != null)
                {
                    blockObj.RELEASE_TIME = DateTime.Now;
                    blockObj.STATUS = SCAppConstants.BlockQueueState.Abnormal_Release_ForceRelease;
                    con.Entry(blockObj).Property(p => p.RELEASE_TIME).IsModified = true;
                    con.Entry(blockObj).Property(p => p.STATUS).IsModified = true;

                    blockZoneQueueDao.Update(con, blockObj);
                }
            }
            return isSeccess;
        }

        public bool updateBlockZoneQueue_ForceRelease(string vhID)
        {
            bool isSeccess = true;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                List<BLOCKZONEQUEUE> blockObjs = blockZoneQueueDao.loadUsingBlockQueueByCarID(con, vhID);
                if (blockObjs != null && blockObjs.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var blockObj in blockObjs)
                    {
                        blockObj.RELEASE_TIME = DateTime.Now;
                        blockObj.STATUS = SCAppConstants.BlockQueueState.Abnormal_Release_ForceRelease;
                        con.Entry(blockObj).Property(p => p.RELEASE_TIME).IsModified = true;
                        con.Entry(blockObj).Property(p => p.STATUS).IsModified = true;
                        blockZoneQueueDao.Update(con, blockObj);

                        sb.Append($"vh id:{vhID},block id:{blockObj.ENTRY_SEC_ID},request time:{blockObj.REQ_TIME.ToString(SCAppConstants.DateTimeFormat_19)}");
                        sb.AppendLine();
                    }
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(BlockControlBLL), Device: "OHTC",
                       Data: $"Excute block force release:{sb.ToString()},by vh id:{vhID}",
                       VehicleID: vhID);
                }
            }
            return isSeccess;
        }


        public List<string> getCurrentBlockID(string vhID)
        {
            List<string> block_ids = null;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                block_ids = blockZoneQueueDao.getCurrentBlockID(con, vhID);
            }
            return block_ids;
        }

        public List<BLOCKZONEQUEUE> getAllCurrentBlockID()
        {
            List<BLOCKZONEQUEUE> block_ids = null;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                block_ids = blockZoneQueueDao.getAllCurrentBlockID(con);
            }
            return block_ids;
        }

        public bool IsAskAgain(string vhID, string sectionID)
        {
            int count = 0;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                count = blockZoneQueueDao.getCountUsingBlockQueueByCarIDSecID(con, vhID, sectionID);
            }
            return count != 0;
        }

        public (bool isBlocking, BLOCKZONEQUEUE blockZoneQueue) IsBlocking(string entrySecID)
        {
            BLOCKZONEQUEUE blockZoneQueue = null;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                blockZoneQueue = blockZoneQueueDao.getCountBlockingQueueBySecID(con, entrySecID);
            }
            return (blockZoneQueue != null, blockZoneQueue);
        }

    }
}
