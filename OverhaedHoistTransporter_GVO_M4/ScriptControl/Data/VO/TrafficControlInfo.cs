using com.mirle.ibg3k0.sc.Common;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.VO
{
    public class TrafficControlInfo
    {
        Logger logger = LogManager.GetCurrentClassLogger();

        public ConcurrentDictionary<string, string> CurrentVhs { get; private set; } = new ConcurrentDictionary<string, string>();

        public event EventHandler<string> TrafficControlSectionIsClear;

        private EventHandler<string> vehicleEntry;
        private object _vehicleEntryEventLock = new object();
        public event EventHandler<string> VehicleEntry
        {
            add
            {
                lock (_vehicleEntryEventLock)
                {
                    vehicleEntry -= value;
                    vehicleEntry += value;
                }
            }
            remove
            {
                lock (_vehicleEntryEventLock)
                {
                    vehicleEntry -= value;
                }
            }
        }

        public void registeredControlSectionLeaveEvent(BLL.SectionBLL sectionBLL)
        {
            foreach (string section_id in ControlSections)
            {
                ASECTION sec_obj = sectionBLL.cache.GetSection(section_id);
                sec_obj.VehicleLeave += Sec_obj_VehicleLeave;
                sec_obj.VehicleEntry += Sec_obj_VehicleEntry;
            }
            //再多註冊Entry Section的Entry event，這樣可以提早發現有人準備跑進被管制的區域了
            foreach (var entry_section_info in EntrySectionInfos)
            {
                ASECTION sec_obj = sectionBLL.cache.GetSection(entry_section_info.ReserveSectionID);
                sec_obj.VehicleEntry += Sec_obj_VehicleEntry;
            }
        }

        private void Sec_obj_VehicleEntry(object sender, string vhID)
        {
            onVehicleEntry(vhID);
            if (CurrentVhs.TryAdd(vhID, vhID))
            {
            }
        }
        private void onVehicleEntry(string vhID)
        {
            vehicleEntry?.Invoke(this, vhID);
        }

        private void Sec_obj_VehicleLeave(object sender, string vhID)
        {
            App.SCApplication app = App.SCApplication.getInstance();
            ASECTION section = sender as ASECTION;
            AVEHICLE leave_vh = app.VehicleBLL.cache.getVehicle(vhID);
            if (!ControlSections.Contains(SCUtility.Trim(leave_vh.CUR_SEC_ID, true)))
            {
                string Vh_id = string.Empty;
                if (CurrentVhs.TryRemove(vhID, out Vh_id))
                {
                }
            }

            //判斷是否所有車子都已經不再該管制道路中了。
            var vhs = app.VehicleBLL.cache.loadAllVh();
            foreach (AVEHICLE vh in vhs)
            {
                string vh_current_section = Common.SCUtility.Trim(vh.CUR_SEC_ID, true);
                if (ControlSections.Contains(vh_current_section))
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(TrafficControlInfo), Device: "OHx",
                       Data: $"vh:{vhID} is leave sec:{section.SEC_ID},but vh:{vh.VEHICLE_ID} in traffic contorl:{ID} of control section:{vh_current_section}," +
                             $"can't notify section clear event.",
                       VehicleID: vhID);
                    return;
                }
            }
            TrafficControlSectionIsClear?.Invoke(this, vhID);

        }

        public string ID;
        public List<ProtocolFormat.OHTMessage.ReserveInfo> EntrySectionInfos;
        public string[] ControlSections;
    }
}
