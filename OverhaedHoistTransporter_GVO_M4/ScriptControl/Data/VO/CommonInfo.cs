//*********************************************************************************
//      CommonInfo.cs
//*********************************************************************************
// File Name: CommonInfo.cs
// Description: CCommonInfo類別
//
//(c) Copyright 2014, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
//**********************************************************************************
using com.mirle.ibg3k0.bcf.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using System.Data;
using System.ComponentModel;
using com.mirle.ibg3k0.sc.ObjectRelay;

namespace com.mirle.ibg3k0.sc.Data.VO
{
    public class CommonInfo : PropertyChangedVO, IAlarmHisList
    {
        #region MPC Tip Message
        private readonly static int MAX_MPC_TIP_MSG_COUNT = 50;
        private List<MPCTipMessage> mpcTipMsgList = new List<MPCTipMessage>();
        public List<MPCTipMessage> MPCTipMsgList
        {
            get
            {
                return mpcTipMsgList;
            }
        }
        private DataTable mpcTipMsgDataTable = null;
        private void initMPCTipMsgDataTable()
        {
            mpcTipMsgDataTable = new DataTable("MPCTipMSG");
            mpcTipMsgDataTable.Columns.Add("Time", typeof(string));
            mpcTipMsgDataTable.Columns.Add("MsgSource", typeof(string));
            mpcTipMsgDataTable.Columns.Add("MsgLevel", typeof(string));
            mpcTipMsgDataTable.Columns.Add("Msg", typeof(string));
        }
        public void addMPCTipMsg(MPCTipMessage mpcTipMsg)
        {
            lock (mpcTipMsgList)
            {
                if (mpcTipMsgList.Count > MAX_MPC_TIP_MSG_COUNT)
                {
                    mpcTipMsgList.RemoveAt(mpcTipMsgList.Count - 1);
                }
                mpcTipMsgList.Insert(0, mpcTipMsg);
                OnPropertyChanged(BCFUtility.getPropertyName(() => this.MPCTipMsgList));
            }
        }
        #endregion
        #region AlarmHis
        private AlarmHisList alarmHisList = new AlarmHisList();
        public AlarmHisList AlarmHisList { get { return alarmHisList; } }
        public void resetAlarmHis(List<ALARM> alarmList)
        {
            alarmHisList.resetAlarmHis(alarmList);
        }
        #endregion AlarmHis

        public BindingList<VehicleObjToShow> ObjectToShow_list = new BindingList<VehicleObjToShow>();

        public Dictionary<string, CommuncationInfo> dicCommunactionInfo = null;
    }


    public class MPCTipMessage
    {
        //訊息時間
        public String Time { set; get; }
        //訊息等級
        public ProtocolFormat.OHTMessage.MsgLevel MsgLevel { get; set; }
        //訊息
        public String Msg { get; set; }
        //訊息
        public String XID { get; set; }
        public bool IsConfirm { get; set; }
        public MPCTipMessage()
        {
            Time = BCFUtility.formatDateTime(DateTime.Now, SCAppConstants.DateTimeFormat_22);
        }

     

    }

}
