//------------------------------------------------------------------------------
// <auto-generated>
//    這個程式碼是由範本產生。
//
//    對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//    如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace com.mirle.ibg3k0.sc
{
    using System;
    using System.Collections.Generic;
    
    public partial class ACARRIER
    {
        public string ID { get; set; }
        public System.DateTime INSER_TIME { get; set; }
        public string LOCATION { get; set; }
        public string RENAME_ID { get; set; }
        public string LOT_ID { get; set; }
        public string TYPE { get; set; }
        public Nullable<com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage.E_ID_READ_STSTUS> READ_STATUS { get; set; }
        public Nullable<System.DateTime> INSTALLED_TIME { get; set; }
        public Nullable<System.DateTime> FINISH_TIME { get; set; }
        public string HOSTSOURCE { get; set; }
        public string HOSTDESTINATION { get; set; }
        public decimal Stage { get; set; }
        public E_CSTState CSTState { get; set; }
        public string CSTType { get; set; }
        public string CSTInDT { get; set; }
        public string StoreDT { get; set; }
        public string WaitOutOPDT { get; set; }
        public string WaitOutLPDT { get; set; }
        public string TrnDT { get; set; }
        public com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage.E_CARRIER_STATE STATE { get; set; }
    }
}