using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using com.mirle.ibg3k0.sc.BLL;
using System.Collections;
using com.mirle.ibg3k0.sc.Common;

namespace com.mirle.ibg3k0.sc
{
    public partial class ASEGMENT
    {
        public List<ASECTION> Sections { get; private set; }

        public void SetSectionList(BLL.SectionBLL sectionBLL)
        {
            var sections_on_segment = sectionBLL.cache.GetSectionBySegmentID(SEG_ID);
            Sections = getSectionByOrder(sections_on_segment);
        }
        public void checkNodeAddressesIsEnhance(List<string> enhanceSubAddresses)
        {
            //var form_address = addressesBLL.cache.GetAddress(FROM_ADR_ID);
            //var to_address = addressesBLL.cache.GetAddress(TO_ADR_ID);
            IsEnhanceSubNodeAddresses = enhanceSubAddresses.Contains(FROM_ADR_ID) && enhanceSubAddresses.Contains(TO_ADR_ID);
            if (IsEnhanceSubNodeAddresses)
            {

            }
        }

        public string getNearSegmentAddress(string adrID)
        {
            if (SCUtility.isMatche(FROM_ADR_ID, adrID) || SCUtility.isMatche(TO_ADR_ID, adrID)) return adrID;

            ASECTION section_of_to_adr = Sections.Where(sec => sec.TO_ADR_ID.Trim() == adrID.Trim()).Single();
            int sec_index = Sections.IndexOf(section_of_to_adr);
            double with_from_adr_dis = 0;
            double with_to_adr_dis = 0;
            for (int i = 0; i < Sections.Count; i++)
            {
                if (i <= sec_index)
                    with_from_adr_dis += Sections[i].SEC_DIS;
                else
                    with_to_adr_dis += Sections[i].SEC_DIS;
            }
            if (with_from_adr_dis >= with_to_adr_dis)
                return RealToAddress;
            else
                return RealFromAddress;
        }

        private List<ASECTION> getSectionByOrder(List<ASECTION> sections)
        {
            if (sections.Count == 1) return sections;
            if (SCUtility.isMatche(SEG_ID, "900")) return new List<ASECTION>();
            List<ASECTION> order_sections = new List<ASECTION>();
            string next_from_adr = RealFromAddress;
            for (int i = 0; i < sections.Count; i++)
            {
                //ASECTION section = sections.Where(s => s.FROM_ADR_ID.Trim() == next_from_adr.Trim()).SingleOrDefault();
                ASECTION section = sections.Where(s => s.REAL_FROM_ADR_ID.Trim() == next_from_adr.Trim()).SingleOrDefault();
                order_sections.Add(section);
                //next_from_adr = section.TO_ADR_ID;
                if (section == null)
                {

                }
                next_from_adr = section.REAL_TO_ADR_ID;
            }
            return order_sections;
        }

        public string RealFromAddress
        {
            get
            {
                if (DIR == E_RAIL_DIR.F)
                    return FROM_ADR_ID;
                else
                    return TO_ADR_ID;
            }
        }
        public string RealToAddress
        {
            get
            {
                if (DIR == E_RAIL_DIR.F)
                    return TO_ADR_ID;
                else
                    return FROM_ADR_ID;
            }
        }
        public string[] NodeAddress
        {
            get
            {
                return new string[] { FROM_ADR_ID, TO_ADR_ID };
            }
        }
        public bool IsEnhanceSubNodeAddresses;

        public int GetSectionIndex(ASECTION section)
        {
            return Sections.IndexOf(section);
        }

    }
}
