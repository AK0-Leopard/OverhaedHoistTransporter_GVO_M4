using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace com.mirle.ibg3k0.sc.Scheduler
{
    public class TransferCommandDataBackupScheduler : IJob
    {
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        SCApplication scApp = SCApplication.getInstance();
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                var finish_cmd_mcs_list = scApp.CMDBLL.loadFinishCMD_MCS();
                if (finish_cmd_mcs_list != null && finish_cmd_mcs_list.Count > 0)
                {
                    using (TransactionScope tx = SCUtility.getTransactionScope())
                    {
                        using (DBConnection_EF con = DBConnection_EF.GetUContext())
                        {

                            scApp.CMDBLL.remoteCMD_MCSByBatch(finish_cmd_mcs_list);
                            List<HTRANSFER> hcmd_mcs_list = finish_cmd_mcs_list.Select(cmd => cmd.ToHCMD_MCS()).ToList();
                            scApp.CMDBLL.CreatHCMD_MCSs(hcmd_mcs_list);

                            tx.Complete();
                        }
                    }
                }
                var finish_cmd_list = scApp.CMDBLL.loadfinishCmd();
                if (finish_cmd_list != null && finish_cmd_list.Count > 0)
                {
                    using (TransactionScope tx = SCUtility.getTransactionScope())
                    {
                        using (DBConnection_EF con = DBConnection_EF.GetUContext())
                        {

                            scApp.CMDBLL.remoteCMDByBatch(finish_cmd_list);
                            List<HCMD> hcmd_list = finish_cmd_list.Select(cmd => cmd.ToHCMD()).ToList();
                            scApp.CMDBLL.CreatHCMD(hcmd_list);

                            tx.Complete();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
        }
    }

}
