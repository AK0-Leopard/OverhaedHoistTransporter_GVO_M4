using com.mirle.ibg3k0.bc.winform;
using com.mirle.ibg3k0.bc.winform.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.RouteKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com.mirle.ibg3k0.bc.winform.UI
{
    public partial class EngineerFormForAGVC : Form
    {

        public delegate void SegmentSelectedEventHandler(string[] sSeg_ID);
#pragma warning disable CS0067 // 事件 'EngineerFormForAGVC.evtSegmentSelected' 從未使用過
        public event SegmentSelectedEventHandler evtSegmentSelected;
#pragma warning restore CS0067 // 事件 'EngineerFormForAGVC.evtSegmentSelected' 從未使用過
        public delegate void SectionSelectedEventHandler(string[] sSec_ID, string startAdr, string fromAdr, string toAdr);
#pragma warning disable CS0067 // 事件 'EngineerFormForAGVC.evtSectionSelected' 從未使用過
        public event SectionSelectedEventHandler evtSectionSelected;
#pragma warning restore CS0067 // 事件 'EngineerFormForAGVC.evtSectionSelected' 從未使用過
        BCMainForm mainForm;
        IRouteGuide routeGuide;
        FloydAlgorithmRouteGuide.TimeWindow timeWindow;

        /// <summary>
        /// This Form Show
        /// </summary>
        public void PrcShow()
        {
            this.BringToFront();
            this.Show();
        }
        /// <summary>
        /// This Form Hide
        /// </summary>
        public void PrcHide()
        {
            this.Hide();
        }
        private int m_iMapSizeW = 0;
        public int p_MapSizeW
        {
            get { return (this.m_iMapSizeW); }
            set { this.m_iMapSizeW = value; }
        }

        private int m_iMapSizeH = 0;
        public int p_MapSizeH
        {
            get { return (this.m_iMapSizeH); }
            set { this.m_iMapSizeH = value; }
        }
        public EngineerFormForAGVC(BCMainForm _mainForm)
        {
            InitializeComponent();
            mainForm = _mainForm;
            routeGuide = mainForm.BCApp.SCApplication.NewRouteGuide;
            timeWindow = mainForm.BCApp.SCApplication.TimeWindow;
            string[] allvh = loadVhID();
            string[] allAdr = loadAllAdr();
            string[] allNodeAdr = loadAllNodeAdr();
            //string[] allPortAdr = loadAllPortAdr();
            string[] allSectionID = loadAllSectionID();
            setCombobox(cmb_fromto_AdrToAdr_From, allAdr.ToArray());
            setCombobox(cmb_fromto_AdrToAdr_To, allAdr.ToArray());
            setCombobox(cmb_fromTo_SecToAdr_From, allSectionID.ToArray());
            setCombobox(cmb_fromTo_SecToAdr_To, allAdr.ToArray());
            setCombobox(cmb_fromTo_SecToSec_From, allSectionID.ToArray());
            setCombobox(cmb_fromTo_SecToSec_To, allSectionID.ToArray());
            setCombobox(comboBox_banstart1, allNodeAdr.ToArray());
            setCombobox(comboBox_banstart2, allNodeAdr.ToArray());
            setCombobox(comboBox_banstart3, allNodeAdr.ToArray());
            setCombobox(comboBox_banend1, allNodeAdr.ToArray());
            setCombobox(comboBox_banend2, allNodeAdr.ToArray());
            setCombobox(comboBox_banend3, allNodeAdr.ToArray());
            setCombobox(comboBox_from1, allSectionID.ToArray());
            setCombobox(comboBox_to1, allAdr.ToArray());
            setCombobox(comboBox_from2, allAdr.ToArray());
            setCombobox(comboBox_to2, allAdr.ToArray());


            setCombobox(cmb_startFromTo_Start_SecToAdrToAdr, allSectionID.ToArray());
            setCombobox(cmb_startFromTo_From_SecToAdrToAdr, allAdr.ToArray());
            setCombobox(cmb_startFromTo_To_SecToAdrToAdr, allAdr.ToArray());

            setCombobox(cmb_startFromTo_Start_AdrToAdrToAdr, allAdr.ToArray());
            setCombobox(cmb_startFromTo_From_AdrToAdrToAdr, allAdr.ToArray());
            setCombobox(cmb_startFromTo_To_AdrToAdrToAdr, allAdr.ToArray());

            setCombobox(cb_startFromTo_Start_SecToAdrToAdr, allSectionID.ToArray());
            setCombobox(cb_startFromTo_From_SecToAdrToAdr, allAdr.ToArray());
            setCombobox(cb_startFromTo_To_SecToAdrToAdr, allAdr.ToArray());

            setCombobox(cb_startFromTo_Start_AdrToAdrToAdr, allAdr.ToArray());
            setCombobox(cb_startFromTo_From_AdrToAdrToAdr, allAdr.ToArray());
            setCombobox(cb_startFromTo_To_AdrToAdrToAdr, allAdr.ToArray());
            cmb_vhID.Items.Add("");
            setCombobox(cmb_vhID, allvh.ToArray());

            //setCombobox(com, allAdr.ToArray());


            cmb_pathInfo.DisplayMember = "PathName";
            cmb_pathInfo.ValueMember = "PathInfo";

            cmb_pathInfo_SecToAdr.DisplayMember = "PathName";
            cmb_pathInfo_SecToAdr.ValueMember = "PathInfo";

            cmb_pathInfo_SecToSec.DisplayMember = "PathName";
            cmb_pathInfo_SecToSec.ValueMember = "PathInfo";

            cmb_pathInfo_timeWindow.DisplayMember = "PathName";
            cmb_pathInfo_timeWindow.ValueMember = "PathInfo";

            cmb_pathInfo_StartFrom_SecToAdrToAdr.DisplayMember = "PathName";
            cmb_pathInfo_StartFrom_SecToAdrToAdr.ValueMember = "PathInfo";

            cmb_pathInfo_FromTo_SecToAdrToAdr.DisplayMember = "PathName";
            cmb_pathInfo_FromTo_SecToAdrToAdr.ValueMember = "PathInfo";

            cmb_pathInfo_StartFrom_AdrToAdrToAdr.DisplayMember = "PathName";
            cmb_pathInfo_StartFrom_AdrToAdrToAdr.ValueMember = "PathInfo";

            cmb_pathInfo_FromTo_AdrToAdrToAdr.DisplayMember = "PathName";
            cmb_pathInfo_FromTo_AdrToAdrToAdr.ValueMember = "PathInfo";

            cb_pathInfo_StartFrom_SecToAdrToAdr.DisplayMember = "PathName";
            cb_pathInfo_StartFrom_SecToAdrToAdr.ValueMember = "PathInfo";

            cb_pathInfo_FromTo_SecToAdrToAdr.DisplayMember = "PathName";
            cb_pathInfo_FromTo_SecToAdrToAdr.ValueMember = "PathInfo";


            //setCombobox(cmb_startFromTo_startAdr, allAdr.ToArray());
            //setCombobox(cmb_startFromTo_fromAdr, allAdr.ToArray());
            //setCombobox(cmb_startFromTo_ToAdr, allAdr.ToArray());
        }
        private string[] loadVhID()
        {
            string[] allVhID = null;
            allVhID = mainForm.BCApp.SCApplication.getEQObjCacheManager().
                getAllVehicle().Select(vh => vh.VEHICLE_ID).ToArray();
            return allVhID;
        }
        private string[] loadAllAdr()
        {
            string[] allAdrID = null;
            allAdrID = mainForm.BCApp.SCApplication.AddressesBLL.cache.GetAddresses().Select(adr => adr.ADR_ID).ToArray();
            return allAdrID;
        }
        private string[] loadAllNodeAdr()
        {
            string[] allNodeAdrID = null;
            allNodeAdrID = mainForm.BCApp.SCApplication.AddressesBLL.cache.GetAddresses().Where(adr => adr.IsSegment).Select(adr => adr.ADR_ID).ToArray();
            return allNodeAdrID;
        }
        //private string[] loadAllPortAdr()
        //{
        //    string[] allPortAdrID = null;
        //    allPortAdrID = mainForm.BCApp.SCApplication.AddressesBLL.cache.GetAddresses().Where(adr => adr.IsPort).Select(adr => adr.ADR_ID).ToArray();
        //    return allPortAdrID;
        //}
        private string[] loadAllSectionID()
        {
            string[] allSectionID = null;
            allSectionID = mainForm.BCApp.SCApplication.MapBLL.loadAllSectionID().ToArray();
            return allSectionID;
        }
        private void setCombobox(ComboBox cmb, string[] allAdr)
        {
            List<string> data_source = new List<string>();
            data_source.Add("");
            data_source.AddRange(allAdr);
            cmb.DataSource = data_source.ToArray();
            cmb.AutoCompleteCustomSource.AddRange(data_source.ToArray());
            cmb.AutoCompleteMode = AutoCompleteMode.Suggest;
            cmb.AutoCompleteSource = AutoCompleteSource.ListItems;
        }

        private void btn_search_for_fromto_Click(object sender, EventArgs e)
        {


            string from_adr = cmb_fromto_AdrToAdr_From.Text;
            string to_adr = cmb_fromto_AdrToAdr_To.Text;
            bool isSuccess = false;
            List<string> guide_start_to_from_segment_ids = null;
            List<string> guide_start_to_from_section_ids = null;
            List<string> guide_start_to_from_address_ids = null;
            int total_cost = 0;
            (isSuccess, guide_start_to_from_segment_ids, guide_start_to_from_section_ids, guide_start_to_from_address_ids, total_cost) = mainForm.BCApp.SCApplication.GuideBLL.getGuideInfo(from_adr, to_adr);

            //var vh = mainForm.BCApp.SCApplication.VehicleBLL.getVehicleByID("AGV01");
            //(guide_start_to_from_segment_ids, guide_start_to_from_section_ids, guide_start_to_from_address_ids) = vh.CheckTurningAngleHasOver180(mainForm.BCApp.SCApplication.SegmentBLL, mainForm.BCApp.SCApplication.GuideBLL,
            //    guide_start_to_from_segment_ids, guide_start_to_from_section_ids, guide_start_to_from_address_ids);

            mainForm.setSpecifyRail(guide_start_to_from_section_ids.ToArray());

            txt_sections_startFrom.Text = string.Join(",", guide_start_to_from_section_ids);
            txt_addresses_startFrom.Text = string.Join(",", guide_start_to_from_address_ids);
            //txt_segment_startFrom.Text = string.Join(",", guide_start_to_from_segment_ids);

            //if (!int.TryParse(from_adr, out int iFromAdr))
            //{
            //    return;
            //}
            //if (!int.TryParse(to_adr, out int iToAdr))
            //{
            //    return;
            //}


            //bool ban1 = checkBox_ban1.Checked;
            //bool ban2 = checkBox_ban2.Checked;
            //bool ban3 = checkBox_ban3.Checked;
            //List<List<int>> banPatternList = new List<List<int>>();
            //if (ban1)
            //{
            //    List<int> banPattern = new List<int>();
            //    banPattern.Add(int.Parse(comboBox_banstart1.Text));
            //    banPattern.Add(int.Parse(comboBox_banend1.Text));
            //    banPatternList.Add(banPattern);
            //}
            //if (ban2)
            //{
            //    List<int> banPattern = new List<int>();
            //    banPattern.Add(int.Parse(comboBox_banstart2.Text));
            //    banPattern.Add(int.Parse(comboBox_banend2.Text));
            //    banPatternList.Add(banPattern);
            //}
            //if (ban3)
            //{
            //    List<int> banPattern = new List<int>();
            //    banPattern.Add(int.Parse(comboBox_banstart3.Text));
            //    banPattern.Add(int.Parse(comboBox_banend3.Text));
            //    banPatternList.Add(banPattern);
            //}

            //var RouteInfos = routeGuide.getFromToRoutesAddrToAddr(iFromAdr, iToAdr);
            ////var RouteInfos = routeGuide.getFromToRoutesSectionToSection(iFromAdr, iToAdr);
            ////var RouteInfos = routeGuide.getFromToRoutesSectionToPort(iFromAdr, iToAdr);

            //cmb_pathInfo.Items.Clear();
            //int path_count = 1;
            //foreach (var routeInfo in RouteInfos)
            //{
            //    cmb_pathInfo.Items.Add(new ComboboxData($"Path {path_count++}({routeInfo.total_cost})", routeInfo));
            //}
            //mainForm.setSpecifyRail(routeSections.ToArray());
        }

        public class ComboboxData
        {
            public ComboboxData(string name, RouteInfo pathInfo)
            {
                PathName = name;
                PathInfo = pathInfo;
            }
            public string PathName { get; set; }
            public RouteInfo PathInfo { get; set; }
        }

        private void btn_search_for_startfromto_Click(object sender, EventArgs e)
        {
            //string strat_adr = cmb_startFromTo_startAdr.Text;
            //string from_adr = cmb_startFromTo_fromAdr.Text;
            //string to_adr = cmb_startFromTo_ToAdr.Text;
            //int banCount = 0;
            //if (checkBox_ban1.Checked)
            //{
            //    banPattern1.Add(int.Parse(comboBox_banstart1.Text));
            //    banPattern1.Add(int.Parse(comboBox_banend1.Text));
            //    banCount++;
            //}
            //if (checkBox_ban2.Checked)
            //    banCount++;
            //if (checkBox_ban3.Checked)
            //    banCount++;
            //List<int>[] bans = new List<int>[banCount];
            //List<string> routeSections = routeGuide.GetRouteGuideSections(strat_adr, from_adr, to_adr);
            //mainForm.setSpecifyRail(routeSections.ToArray());
        }

        private void cmb_pathInfo_SelectedIndexChanged(object sender, EventArgs e)
        {
            var path_info = (sender as ComboBox).SelectedItem as ComboboxData;
            string[] sections = path_info.PathInfo.sections.Select(sec => sec.section_id).ToArray();
            mainForm.setSpecifyRail(sections);

            int[] addresses = path_info.PathInfo.addresses.ToArray();

            txt_sections_startFrom.Text = string.Join(",", sections);
            txt_addresses_startFrom.Text = string.Join(",", addresses);
        }

        private void cmb_pathInfo_SecToAdr_SelectedIndexChanged(object sender, EventArgs e)
        {
            var path_info = cmb_pathInfo_SecToAdr.SelectedItem as ComboboxData;
            string[] sections = path_info.PathInfo.sections.Select(sec => sec.section_id).ToArray();
            mainForm.setSpecifyRail(sections);
        }

        private void cmb_pathInfo_SecToSec_SelectedIndexChanged(object sender, EventArgs e)
        {
            var path_info = cmb_pathInfo_SecToSec.SelectedItem as ComboboxData;
            string[] sections = path_info.PathInfo.sections.Select(sec => sec.section_id).ToArray();
            mainForm.setSpecifyRail(sections);
        }

        private void checkBox_ban1_CheckedChanged(object sender, EventArgs e)
        {
            bool ban = (sender as CheckBox).Checked;
            string banStartAdr = comboBox_banstart1.Text;
            string banEndAdr = comboBox_banend1.Text;
            setBanRouteOneDirect(ban, banStartAdr, banEndAdr);
            gb_BanRoute1.Enabled = !ban;

        }

        private void setBanRouteOneDirect(bool ban, string banStartAdr, string banEndAdr)
        {
            if (!int.TryParse(banStartAdr, out int iBanStartAdr))
            {
                return;
            }
            if (!int.TryParse(banEndAdr, out int iBanEndAdr))
            {
                return;
            }

            if (ban)
            {
                routeGuide.banRouteOneDirect(iBanStartAdr, iBanEndAdr);
            }
            else
            {
                routeGuide.unbanRouteOneDirect(iBanStartAdr, iBanEndAdr);
            }
        }

        private void checkBox_ban2_CheckedChanged(object sender, EventArgs e)
        {
            bool ban = (sender as CheckBox).Checked;
            string banStartAdr = comboBox_banstart2.Text;
            string banEndAdr = comboBox_banend2.Text;
            setBanRouteOneDirect(ban, banStartAdr, banEndAdr);
            gb_BanRoute2.Enabled = !ban;

        }

        private void checkBox_ban3_CheckedChanged(object sender, EventArgs e)
        {
            bool ban = (sender as CheckBox).Checked;
            string banStartAdr = comboBox_banstart3.Text;
            string banEndAdr = comboBox_banend3.Text;
            setBanRouteOneDirect(ban, banStartAdr, banEndAdr);
            gb_BanRoute3.Enabled = !ban;
        }

        private void btn_reset_ban_Click(object sender, EventArgs e)
        {
            routeGuide.resetBanRoute();
        }

        private async void btn_reloadRoude_Click(object sender, EventArgs e)
        {
            var segment = mainForm.BCApp.SCApplication.MapBLL.loadAllSegments();
            var sections = mainForm.BCApp.SCApplication.MapBLL.loadAllSection();
            var aaddresses = mainForm.BCApp.SCApplication.MapBLL.loadAllAddress();
            groupBox1.Enabled = false;
            groupBox2.Enabled = false;
            await Task.Run(() => routeGuide.reloadRoute(segment, sections, aaddresses));
            groupBox1.Enabled = true;
            groupBox2.Enabled = true;
        }

        private void button_time_window_Click(object sender, EventArgs e)
        {
            cmb_pathInfo_timeWindow.Items.Clear();

            List<RouteInfo> routeInfos = timeWindow.Compute();
            //for(int i = 0; i < timeWindow.computeResult.Count; i++)
            //{

            //    timeWindow.computeResult[i].new_pathInfo;
            //}

            //int from_1 = int.Parse(comboBox_from1.Text);
            //int to_1 = int.Parse(comboBox_to1.Text);
            //int from_2 = int.Parse(comboBox_from2.Text);
            //int to_2 = int.Parse(comboBox_to2.Text);

            //var Routes = routeGuide._TimeWindow(from_1, to_1, from_2, to_2);



            cmb_pathInfo_timeWindow.Items.Clear();
            int path_count = 1;
            foreach (var sections in routeInfos)
            {
                cmb_pathInfo_timeWindow.Items.Add(new ComboboxData($"Path {path_count++}", sections));
            }

        }

        private void cmb_pathInfo_timeWindow_SelectedIndexChanged(object sender, EventArgs e)
        {
            var path_info = cmb_pathInfo_timeWindow.SelectedItem as ComboboxData;
            string[] sections = path_info.PathInfo.sections.Select(sec => sec.section_id).ToArray();
            mainForm.setSpecifyRail(sections);
        }

        private void EngineerFormForAGVC_FormClosed(object sender, FormClosedEventArgs e)
        {
            mainForm.removeForm(nameof(EngineerFormForAGVC));
        }

        private void Btn_search_for_fromto_SecToAdr_Click(object sender, EventArgs e)
        {
            string from_adr = cmb_fromTo_SecToAdr_From.Text;
            string to_adr = cmb_fromTo_SecToAdr_To.Text;
            if (!int.TryParse(from_adr, out int iFromAdr))
            {
                return;
            }
            if (!int.TryParse(to_adr, out int iToAdr))
            {
                return;
            }


            bool ban1 = checkBox_ban1.Checked;
            bool ban2 = checkBox_ban2.Checked;
            bool ban3 = checkBox_ban3.Checked;
            List<List<int>> banPatternList = new List<List<int>>();
            if (ban1)
            {
                List<int> banPattern = new List<int>();
                banPattern.Add(int.Parse(comboBox_banstart1.Text));
                banPattern.Add(int.Parse(comboBox_banend1.Text));
                banPatternList.Add(banPattern);
            }
            if (ban2)
            {
                List<int> banPattern = new List<int>();
                banPattern.Add(int.Parse(comboBox_banstart2.Text));
                banPattern.Add(int.Parse(comboBox_banend2.Text));
                banPatternList.Add(banPattern);
            }
            if (ban3)
            {
                List<int> banPattern = new List<int>();
                banPattern.Add(int.Parse(comboBox_banstart3.Text));
                banPattern.Add(int.Parse(comboBox_banend3.Text));
                banPatternList.Add(banPattern);
            }

            //var RouteInfos = routeGuide.getFromToRoutes(iFromAdr, iToAdr);
            //var RouteInfos = routeGuide.getFromToRoutesSectionToSection(iFromAdr, iToAdr);
            var RouteInfos = routeGuide.getFromToRoutesSectionToAdr(iFromAdr, iToAdr);

            cmb_pathInfo_SecToAdr.Items.Clear();
            int path_count = 1;
            foreach (var routeInfo in RouteInfos)
            {
                cmb_pathInfo_SecToAdr.Items.Add(new ComboboxData($"Path {path_count++}({routeInfo.total_cost})", routeInfo));
            }
            //mainForm.setSpecifyRail(routeSections.ToArray());
        }

        private void btn_search_for_fromto_SecToSec_Click(object sender, EventArgs e)
        {
            string from_adr = cmb_fromTo_SecToSec_From.Text;
            string to_adr = cmb_fromTo_SecToSec_To.Text;
            if (!int.TryParse(from_adr, out int iFromAdr))
            {
                return;
            }
            if (!int.TryParse(to_adr, out int iToAdr))
            {
                return;
            }


            bool ban1 = checkBox_ban1.Checked;
            bool ban2 = checkBox_ban2.Checked;
            bool ban3 = checkBox_ban3.Checked;
            List<List<int>> banPatternList = new List<List<int>>();
            if (ban1)
            {
                List<int> banPattern = new List<int>();
                banPattern.Add(int.Parse(comboBox_banstart1.Text));
                banPattern.Add(int.Parse(comboBox_banend1.Text));
                banPatternList.Add(banPattern);
            }
            if (ban2)
            {
                List<int> banPattern = new List<int>();
                banPattern.Add(int.Parse(comboBox_banstart2.Text));
                banPattern.Add(int.Parse(comboBox_banend2.Text));
                banPatternList.Add(banPattern);
            }
            if (ban3)
            {
                List<int> banPattern = new List<int>();
                banPattern.Add(int.Parse(comboBox_banstart3.Text));
                banPattern.Add(int.Parse(comboBox_banend3.Text));
                banPatternList.Add(banPattern);
            }

            //var RouteInfos = routeGuide.getFromToRoutes(iFromAdr, iToAdr);
            var RouteInfos = routeGuide.getFromToRoutesSectionToSection(iFromAdr, iToAdr);
            //var RouteInfos = routeGuide.getFromToRoutesSectionToPort(iFromAdr, iToAdr);

            cmb_pathInfo_SecToSec.Items.Clear();
            int path_count = 1;
            foreach (var routeInfo in RouteInfos)
            {
                cmb_pathInfo_SecToSec.Items.Add(new ComboboxData($"Path {path_count++}({routeInfo.total_cost})", routeInfo));
            }
            //mainForm.setSpecifyRail(routeSections.ToArray());
        }

        private void btn_search_for_starFromTo_Click(object sender, EventArgs e)
        {
            string start_adr = cmb_startFromTo_Start_SecToAdrToAdr.Text;
            string from_adr = cmb_startFromTo_From_SecToAdrToAdr.Text;
            string to_adr = cmb_startFromTo_To_SecToAdrToAdr.Text;
            if (!int.TryParse(start_adr, out int iStartAdr))
            {
                return;
            }
            if (!int.TryParse(from_adr, out int iFromAdr))
            {
                return;
            }
            if (!int.TryParse(to_adr, out int iToAdr))
            {
                return;
            }


            bool ban1 = checkBox_ban1.Checked;
            bool ban2 = checkBox_ban2.Checked;
            bool ban3 = checkBox_ban3.Checked;
            List<List<int>> banPatternList = new List<List<int>>();
            if (ban1)
            {
                List<int> banPattern = new List<int>();
                banPattern.Add(int.Parse(comboBox_banstart1.Text));
                banPattern.Add(int.Parse(comboBox_banend1.Text));
                banPatternList.Add(banPattern);
            }
            if (ban2)
            {
                List<int> banPattern = new List<int>();
                banPattern.Add(int.Parse(comboBox_banstart2.Text));
                banPattern.Add(int.Parse(comboBox_banend2.Text));
                banPatternList.Add(banPattern);
            }
            if (ban3)
            {
                List<int> banPattern = new List<int>();
                banPattern.Add(int.Parse(comboBox_banstart3.Text));
                banPattern.Add(int.Parse(comboBox_banend3.Text));
                banPatternList.Add(banPattern);
            }

            //var RouteInfos = routeGuide.getFromToRoutes(iFromAdr, iToAdr);
            (var StartFromRouteInfos, var FromToRouteInfos) = routeGuide.getStartFromThenFromToRoutesSecToAddrToAddr(iStartAdr, iFromAdr, iToAdr);
            //var RouteInfos = routeGuide.getFromToRoutesSectionToPort(iFromAdr, iToAdr);
            cmb_pathInfo_StartFrom_SecToAdrToAdr.Items.Clear();
            int path_count = 1;
            foreach (var routeInfo in StartFromRouteInfos)
            {
                cmb_pathInfo_StartFrom_SecToAdrToAdr.Items.Add(new ComboboxData($"Path {path_count++}({routeInfo.total_cost})", routeInfo));
            }
            cmb_pathInfo_FromTo_SecToAdrToAdr.Items.Clear();
            path_count = 1;
            foreach (var routeInfo in FromToRouteInfos)
            {
                cmb_pathInfo_FromTo_SecToAdrToAdr.Items.Add(new ComboboxData($"Path {path_count++}({routeInfo.total_cost})", routeInfo));
            }




            //string start_sec = cmb_startFromTo_Start_SecToAdrToAdr.Text;
            //string from_adr = cmb_startFromTo_From_SecToAdrToAdr.Text;
            //string to_adr = cmb_startFromTo_To_SecToAdrToAdr.Text;
            //if (!int.TryParse(start_sec, out int iStartSec))
            //{
            //    return;
            //}
            //if (!int.TryParse(from_adr, out int iFromAdr))
            //{
            //    return;
            //}
            //if (!int.TryParse(to_adr, out int iToAdr))
            //{
            //    return;
            //}


            //bool ban1 = checkBox_ban1.Checked;
            //bool ban2 = checkBox_ban2.Checked;
            //bool ban3 = checkBox_ban3.Checked;
            //List<List<int>> banPatternList = new List<List<int>>();
            //if (ban1)
            //{
            //    List<int> banPattern = new List<int>();
            //    banPattern.Add(int.Parse(comboBox_banstart1.Text));
            //    banPattern.Add(int.Parse(comboBox_banend1.Text));
            //    banPatternList.Add(banPattern);
            //}
            //if (ban2)
            //{
            //    List<int> banPattern = new List<int>();
            //    banPattern.Add(int.Parse(comboBox_banstart2.Text));
            //    banPattern.Add(int.Parse(comboBox_banend2.Text));
            //    banPatternList.Add(banPattern);
            //}
            //if (ban3)
            //{
            //    List<int> banPattern = new List<int>();
            //    banPattern.Add(int.Parse(comboBox_banstart3.Text));
            //    banPattern.Add(int.Parse(comboBox_banend3.Text));
            //    banPatternList.Add(banPattern);
            //}

            //var guide_info_start_from = mainForm.BCApp.SCApplication.GuideBLL.getGuideInfo(start_sec, from_adr);
            //var guide_info_from_to = mainForm.BCApp.SCApplication.GuideBLL.getGuideInfo(start_sec, to_adr);

            //List<string> sections_start_from = guide_info_start_from.CompletelyRoute.GetSectionIDs();
            //List<string> addresses_start_from = guide_info_start_from.CompletelyRoute.GetAddressesIDs();
            //List<string> sections_from_to = guide_info_from_to.CompletelyRoute.GetSectionIDs();
            //List<string> addresses_from_to = guide_info_from_to.CompletelyRoute.GetAddressesIDs();
            ////判斷車子目前的位置是否與接下來要走的Address相同，相同代表在同一個點上就不用加，
            ////不同則要把車子目前的位置加進去

            //addresses_start_from[addresses_start_from.Count - 1] = from_adr;
            //addresses_from_to[addresses_from_to.Count - 1] = to_adr;

            //txt_sections_startFrom.Text = string.Join(",", sections_start_from);
            //txt_addresses_startFrom.Text = string.Join(",", addresses_start_from);
            //txt_sections_fromTo.Text = string.Join(",", sections_from_to);
            //txt_addresses_fromTo.Text = string.Join(",", addresses_from_to);
        }
        string section_address_selector(Section sec)
        {
            if (sec.direct == 1)
            {
                return sec.address_2.ToString();
            }
            else if (sec.direct == 2)
            {
                return sec.address_1.ToString();
            }
            else
            {
                throw new Exception();
            }
        }

        private void btn_search_for_starFromTo_AdrToAdrToAdr_Click(object sender, EventArgs e)
        {
            string start_adr = cmb_startFromTo_Start_AdrToAdrToAdr.Text;
            string from_adr = cmb_startFromTo_From_AdrToAdrToAdr.Text;
            string to_adr = cmb_startFromTo_To_AdrToAdrToAdr.Text;
            if (!int.TryParse(start_adr, out int iStartAdr))
            {
                return;
            }
            if (!int.TryParse(from_adr, out int iFromAdr))
            {
                return;
            }
            if (!int.TryParse(to_adr, out int iToAdr))
            {
                return;
            }


            bool ban1 = checkBox_ban1.Checked;
            bool ban2 = checkBox_ban2.Checked;
            bool ban3 = checkBox_ban3.Checked;
            List<List<int>> banPatternList = new List<List<int>>();
            if (ban1)
            {
                List<int> banPattern = new List<int>();
                banPattern.Add(int.Parse(comboBox_banstart1.Text));
                banPattern.Add(int.Parse(comboBox_banend1.Text));
                banPatternList.Add(banPattern);
            }
            if (ban2)
            {
                List<int> banPattern = new List<int>();
                banPattern.Add(int.Parse(comboBox_banstart2.Text));
                banPattern.Add(int.Parse(comboBox_banend2.Text));
                banPatternList.Add(banPattern);
            }
            if (ban3)
            {
                List<int> banPattern = new List<int>();
                banPattern.Add(int.Parse(comboBox_banstart3.Text));
                banPattern.Add(int.Parse(comboBox_banend3.Text));
                banPatternList.Add(banPattern);
            }

            //var RouteInfos = routeGuide.getFromToRoutes(iFromAdr, iToAdr);
            (var StartFromRouteInfos, var FromToRouteInfos) = routeGuide.getStartFromThenFromToRoutesAddrToAddrToAddr(iStartAdr, iFromAdr, iToAdr);
            //var RouteInfos = routeGuide.getFromToRoutesSectionToPort(iFromAdr, iToAdr);

            cmb_pathInfo_StartFrom_AdrToAdrToAdr.Items.Clear();
            int path_count = 1;
            foreach (var routeInfo in StartFromRouteInfos)
            {
                cmb_pathInfo_StartFrom_AdrToAdrToAdr.Items.Add(new ComboboxData($"Path {path_count++}({routeInfo.total_cost})", routeInfo));
            }
            cmb_pathInfo_FromTo_AdrToAdrToAdr.Items.Clear();
            path_count = 1;
            foreach (var routeInfo in FromToRouteInfos)
            {
                cmb_pathInfo_FromTo_AdrToAdrToAdr.Items.Add(new ComboboxData($"Path {path_count++}({routeInfo.total_cost})", routeInfo));
            }


            //foreach (var routeInfo in FromToRouteInfos)
            //{
            //    cmb_pathInfo_StartFromTo_AdrToAdrToAdr.Items.Add(new ComboboxData($"Path {path_count++}({routeInfo.total_cost})", routeInfo.sections));
            //}
            //mainForm.setSpecifyRail(routeSections.ToArray());
        }

        private void cmb_pathInfo_StartFrom_AdrToAdrToAdr_SelectedIndexChanged(object sender, EventArgs e)
        {
            var path_info = (sender as ComboBox).SelectedItem as ComboboxData;
            string[] sections = path_info.PathInfo.sections.Select(sec => sec.section_id).ToArray();
            mainForm.setSpecifyRail(sections);

            int[] addresses = path_info.PathInfo.addresses.ToArray();

            txt_sections_startFrom.Text = string.Join(",", sections);
            txt_addresses_startFrom.Text = string.Join(",", addresses);
        }

        private void cmb_pathInfo_FromTo_AdrToAdrToAdr_SelectedIndexChanged(object sender, EventArgs e)
        {
            var path_info = (sender as ComboBox).SelectedItem as ComboboxData;
            string[] sections = path_info.PathInfo.sections.Select(sec => sec.section_id).ToArray();
            mainForm.setSpecifyRail(sections);

            int[] addresses = path_info.PathInfo.addresses.ToArray();

            txt_sections_fromTo.Text = string.Join(",", sections);
            txt_addresses_fromTo.Text = string.Join(",", addresses);
        }

        private void cmb_pathInfo_FromTo_SecToAdrToAdr_SelectedIndexChanged(object sender, EventArgs e)
        {
            var path_info = (sender as ComboBox).SelectedItem as ComboboxData;
            string[] sections = path_info.PathInfo.sections.Select(sec => sec.section_id).ToArray();
            mainForm.setSpecifyRail(sections);

            int[] addresses = path_info.PathInfo.addresses.ToArray();

            txt_sections_fromTo.Text = string.Join(",", sections);
            txt_addresses_fromTo.Text = string.Join(",", addresses);

            //txt_sections_startFrom.Text = string.Join(",", sections_start_from);
            //txt_addresses_startFrom.Text = string.Join(",", addresses_start_from);

        }

        private void bn_search_for_starFromTo_Click(object sender, EventArgs e)
        {
            string start_sec = cb_startFromTo_Start_SecToAdrToAdr.Text;
            string from_adr = cb_startFromTo_From_SecToAdrToAdr.Text;
            string to_adr = cb_startFromTo_To_SecToAdrToAdr.Text;
            if (!int.TryParse(start_sec, out int iStartSec))
            {
                return;
            }
            if (!int.TryParse(from_adr, out int iFromAdr))
            {
                return;
            }
            if (!int.TryParse(to_adr, out int iToAdr))
            {
                return;
            }

            (var StartFromRouteInfos, var FromToRouteInfos) = routeGuide.getStartFromThenFromToRoutesSecToAddrToAddr(iStartSec, iFromAdr, iToAdr);
            cb_pathInfo_StartFrom_SecToAdrToAdr.Items.Clear();
            int path_count = 1;
            foreach (var routeInfo in StartFromRouteInfos)
            {
                cb_pathInfo_StartFrom_SecToAdrToAdr.Items.Add(new ComboboxData($"Path {path_count++}({routeInfo.total_cost})", routeInfo));
            }
            cb_pathInfo_FromTo_SecToAdrToAdr.Items.Clear();
            path_count = 1;
            foreach (var routeInfo in FromToRouteInfos)
            {
                cb_pathInfo_FromTo_SecToAdrToAdr.Items.Add(new ComboboxData($"Path {path_count++}({routeInfo.total_cost})", routeInfo));
            }
        }

        private void bn_search_for_starFromTo_AdrToAdrToAdr_Click(object sender, EventArgs e)
        {
            bool isSuccess = false;
            string start_adr = cb_startFromTo_Start_AdrToAdrToAdr.Text;
            string from_adr = cb_startFromTo_From_AdrToAdrToAdr.Text;
            string to_adr = cb_startFromTo_To_AdrToAdrToAdr.Text;
            List<string> guide_start_to_from_segment_ids = null;
            List<string> guide_start_to_from_section_ids = null;
            List<string> guide_start_to_from_address_ids = null;
            List<string> guide_to_dest_segment_ids = null;
            List<string> guide_to_dest_section_ids = null;
            List<string> guide_to_dest_address_ids = null;
            int total_cost = 0;

            if (!SCUtility.isEmpty(cmb_vhID.Text))
            {
                var vh = mainForm.BCApp.SCApplication.VehicleBLL.cache.getVehicle(cmb_vhID.Text);
                start_adr = vh.CUR_ADR_ID;
            }

            if (!SCUtility.isEmpty(start_adr) && !SCUtility.isMatche(start_adr, from_adr))
            {
                (isSuccess, guide_start_to_from_segment_ids, guide_start_to_from_section_ids, guide_start_to_from_address_ids, total_cost) = mainForm.BCApp.SCApplication.GuideBLL.getGuideInfo(start_adr, from_adr);
            }
            if (!SCUtility.isEmpty(to_adr) && !SCUtility.isMatche(from_adr, to_adr))
            {
                (isSuccess, guide_to_dest_segment_ids, guide_to_dest_section_ids, guide_to_dest_address_ids, total_cost) = mainForm.BCApp.SCApplication.GuideBLL.getGuideInfo(from_adr, to_adr);
            }

            txt_segments_startFrom_adrAdrAndAdr.Text = string.Empty;
            txt_sections_startFrom_adrAdrAndAdr.Text = string.Empty;
            txt_addresses_startFrom_adrAdrAndAdr.Text = string.Empty;
            txt_segments_fromTo_adrAdrAndAdr.Text = string.Empty;
            txt_sections_fromTo_adrAdrAndAdr.Text = string.Empty;
            txt_addresses_fromTo_adrAdrAndAdr.Text = string.Empty;


            if (guide_start_to_from_segment_ids != null)
                txt_segments_startFrom_adrAdrAndAdr.Text = string.Join(",", guide_start_to_from_segment_ids);
            if (guide_start_to_from_section_ids != null)
                txt_sections_startFrom_adrAdrAndAdr.Text = string.Join(",", guide_start_to_from_section_ids);
            if (guide_start_to_from_address_ids != null)
                txt_addresses_startFrom_adrAdrAndAdr.Text = string.Join(",", guide_start_to_from_address_ids);

            if (guide_to_dest_segment_ids != null)
                txt_segments_fromTo_adrAdrAndAdr.Text = string.Join(",", guide_to_dest_segment_ids);
            if (guide_to_dest_section_ids != null)
                txt_sections_fromTo_adrAdrAndAdr.Text = string.Join(",", guide_to_dest_section_ids);
            if (guide_to_dest_address_ids != null)
                txt_addresses_fromTo_adrAdrAndAdr.Text = string.Join(",", guide_to_dest_address_ids);

            List<string> total_section = new List<string>();
            if (guide_start_to_from_section_ids != null)
                total_section.AddRange(guide_start_to_from_section_ids);
            if (guide_to_dest_section_ids != null)
                total_section.AddRange(guide_to_dest_section_ids);
            mainForm.setSpecifyRail(total_section.ToArray());


            //if (!int.TryParse(start_adr, out int iStartAdr))
            //{
            //    return;
            //}
            //if (!int.TryParse(from_adr, out int iFromAdr))
            //{
            //    return;
            //}
            //if (!int.TryParse(to_adr, out int iToAdr))
            //{
            //    return;
            //}

            //(var StartFromRouteInfos, var FromToRouteInfos) = routeGuide.getStartFromThenFromToRoutesAddrToAddrToAddr(iStartAdr, iFromAdr, iToAdr);

            //cb_pathInfo_StartFrom_AdrToAdrToAdr.Items.Clear();
            //int path_count = 1;
            //foreach (var routeInfo in StartFromRouteInfos)
            //{
            //    cb_pathInfo_StartFrom_AdrToAdrToAdr.Items.Add(new ComboboxData($"Path {path_count++}({routeInfo.total_cost})", routeInfo));
            //}
            //cb_pathInfo_FromTo_AdrToAdrToAdr.Items.Clear();
            //path_count = 1;
            //foreach (var routeInfo in FromToRouteInfos)
            //{
            //    cb_pathInfo_FromTo_AdrToAdrToAdr.Items.Add(new ComboboxData($"Path {path_count++}({routeInfo.total_cost})", routeInfo));
            //}
        }

        private void cb_pathInfo_StartFrom_SecToAdrToAdr_SelectedIndexChanged(object sender, EventArgs e)
        {
            var path_info = (sender as ComboBox).SelectedItem as ComboboxData;
            string[] sections = path_info.PathInfo.sections.Select(sec => sec.section_id).ToArray();
            mainForm.setSpecifyRail(sections);

            int[] addresses = path_info.PathInfo.addresses.ToArray();

            text_sections_startFrom_secAdrAndAdr.Text = string.Join(",", sections);
            text_addresses_startFrom_secAdrAndAdr.Text = string.Join(",", addresses);
        }

        private void cb_pathInfo_FromTo_SecToAdrToAdr_SelectedIndexChanged(object sender, EventArgs e)
        {
            var path_info = (sender as ComboBox).SelectedItem as ComboboxData;
            string[] sections = path_info.PathInfo.sections.Select(sec => sec.section_id).ToArray();
            mainForm.setSpecifyRail(sections);

            int[] addresses = path_info.PathInfo.addresses.ToArray();

            text_sections_fromTo_secAdrAndAdr.Text = string.Join(",", sections);
            text_addresses_fromTo_secAdrAndAdr.Text = string.Join(",", addresses);
        }

        private void cb_pathInfo_StartFrom_AdrToAdrToAdr_SelectedIndexChanged(object sender, EventArgs e)
        {
            var path_info = (sender as ComboBox).SelectedItem as ComboboxData;
            string[] sections = path_info.PathInfo.sections.Select(sec => sec.section_id).ToArray();
            mainForm.setSpecifyRail(sections);

            int[] addresses = path_info.PathInfo.addresses.ToArray();

            txt_sections_startFrom_adrAdrAndAdr.Text = string.Join(",", sections);
            txt_addresses_startFrom_adrAdrAndAdr.Text = string.Join(",", addresses);
        }

        private void cb_pathInfo_FromTo_AdrToAdrToAdr_SelectedIndexChanged(object sender, EventArgs e)
        {
            var path_info = (sender as ComboBox).SelectedItem as ComboboxData;
            string[] sections = path_info.PathInfo.sections.Select(sec => sec.section_id).ToArray();
            mainForm.setSpecifyRail(sections);

            int[] addresses = path_info.PathInfo.addresses.ToArray();

            txt_sections_fromTo_adrAdrAndAdr.Text = string.Join(",", sections);
            txt_addresses_fromTo_adrAdrAndAdr.Text = string.Join(",", addresses);
        }



        private void btn_sendCmd_adrAdrAndAdr_Click(object sender, EventArgs e)
        {
            sc.App.SCApplication scApp = mainForm.BCApp.SCApplication;
            string vh_id = cmb_vhID.Text;
            if (string.IsNullOrWhiteSpace(vh_id))
            {
                MessageBox.Show("No find vehile.");
                return;
            }

            string cmd_id = scApp.SequenceBLL.getCommandID(sc.App.SCAppConstants.GenOHxCCommandType.Debug);
            string cst_id = txt_cst_id.Text;
            string from_adr = cb_startFromTo_From_AdrToAdrToAdr.Text;
            string to_adr = cb_startFromTo_To_AdrToAdrToAdr.Text;
            string segment_startFrom = txt_segments_startFrom_adrAdrAndAdr.Text;
            string sections_startFrom = txt_sections_startFrom_adrAdrAndAdr.Text;
            string addresses_startFrom = txt_addresses_startFrom_adrAdrAndAdr.Text;

            string segment_FromTo = txt_segments_fromTo_adrAdrAndAdr.Text;
            string sections_FromTo = txt_sections_fromTo_adrAdrAndAdr.Text;
            string addresses_FromTo = txt_addresses_fromTo_adrAdrAndAdr.Text;

            string[] start_from_segs = sc.Common.SCUtility.isEmpty(segment_startFrom) ?
                                       null : segment_startFrom.Split(',').Select(sec => sec.PadLeft(3, '0')).ToArray();
            string[] start_from_secs = sc.Common.SCUtility.isEmpty(sections_startFrom) ?
                                       null : sections_startFrom.Split(',').Select(sec => sec.PadLeft(4, '0')).ToArray();
            string[] start_from_addresses = sc.Common.SCUtility.isEmpty(addresses_startFrom) ?
                                       null : addresses_startFrom.Split(',').Select(sec => sec.PadLeft(5, '0')).ToArray();

            string[] from_to_segs = sc.Common.SCUtility.isEmpty(segment_FromTo) ?
                                       null : segment_FromTo.Split(',').Select(sec => sec.PadLeft(3, '0')).ToArray();
            string[] from_to_secs = sc.Common.SCUtility.isEmpty(sections_FromTo) ?
                                       null : sections_FromTo.Split(',').Select(sec => sec.PadLeft(4, '0')).ToArray();
            string[] from_to_addresses = sc.Common.SCUtility.isEmpty(addresses_FromTo) ?
                                       null : addresses_FromTo.Split(',').Select(sec => sec.PadLeft(5, '0')).ToArray();



            sc.ProtocolFormat.OHTMessage.ActiveType active_type = sc.ProtocolFormat.OHTMessage.ActiveType.Loadunload;
            if (raid_cmd_move.Checked)
            {
                from_adr = "";
                active_type = sc.ProtocolFormat.OHTMessage.ActiveType.Move;
            }
            else if (raid_cmd_load.Checked)
            {
                to_adr = "";
                active_type = sc.ProtocolFormat.OHTMessage.ActiveType.Load;
            }
            else if (raid_cmd_unload.Checked)
            {
                from_adr = "";
                active_type = sc.ProtocolFormat.OHTMessage.ActiveType.Unload;
            }
            else if (raid_cmd_loadunload.Checked)
            {
                active_type = sc.ProtocolFormat.OHTMessage.ActiveType.Loadunload;
            }
            else if (raid_cmd_override.Checked)
            {

            }
            else
            {
                return;
            }
            //Task.Run(() =>
            //        scApp.VehicleService.TransferRequset
            //        (vh_id, cmd_id
            //        , active_type, cst_id,
            //        start_from_segs, start_from_secs, start_from_addresses,
            //        from_to_segs, from_to_secs, from_to_addresses,
            //        from_adr, to_adr));
        }

        private void addTimeWindowCMDCurSecBtn_Click(object sender, EventArgs e)
        {
            int from = int.Parse(comboBox_from1.Text);
            int to = int.Parse(comboBox_to1.Text);
            string vehicle_ID = textBox1.Text;

            timeWindow.addCMDInfoCurrIsSection(vehicle_ID, from, to, null);
        }

        private void addTimeWindowCMDCurAddrBtn_Click(object sender, EventArgs e)
        {
            int from = int.Parse(comboBox_from2.Text);
            int to = int.Parse(comboBox_to2.Text);
            string vehicle_ID = textBox2.Text;

            timeWindow.addCMDInfoCurrIsAddress(vehicle_ID, from, to, null);
        }

        private void ClearTimeWindowCMDBtn_Click(object sender, EventArgs e)
        {
            timeWindow.clearAllCMDInfo();
        }

        private void btn_current_ban_Click(object sender, EventArgs e)
        {
            var all_ban = routeGuide.getAllBanDirectArray();

            txt_current_ban.Text = string.Join(",", all_ban);
        }

        private void bn_search_for_starFromTo_Click_1(object sender, EventArgs e)
        {

        }

        private void btn_all_adress_test_Click(object sender, EventArgs e)
        {
            sc.App.SCApplication scApp = mainForm.BCApp.SCApplication;
            var addresses = scApp.AddressesBLL.cache.GetAddresses();
            foreach (var adr1 in addresses)
            {
                foreach (var adr2 in addresses)
                {
                    if (adr1 == adr2) continue;
                    List<string> guide_start_to_from_segment_ids = null;
                    List<string> guide_start_to_from_section_ids = null;
                    List<string> guide_start_to_from_address_ids = null;
                    bool isSuccess = false;
                    int total_cost = 0;
                    (isSuccess, guide_start_to_from_segment_ids, guide_start_to_from_section_ids, guide_start_to_from_address_ids, total_cost) = mainForm.BCApp.SCApplication.GuideBLL.getGuideInfo(adr1.ADR_ID, adr2.ADR_ID);
                    if (!isSuccess)
                    {

                    }
                    else
                    {
                        if (adr1.IsControl)
                        {
                            if (!guide_start_to_from_section_ids.Contains(adr1.SEC_ID.Trim()))
                            {

                            }
                        }
                        if (adr2.IsControl)
                        {
                            if (!guide_start_to_from_section_ids.Contains(adr2.SEC_ID.Trim()))
                            {

                            }
                        }
                        Console.WriteLine($"form:{adr1.ADR_ID} to:{adr2.ADR_ID} OK");
                    }
                }
            }
            MessageBox.Show("OK");
        }

        private void btn_auto_override_Click(object sender, EventArgs e)
        {

        }
    }
}
