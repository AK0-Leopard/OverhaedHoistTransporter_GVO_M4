using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.Data.DAO;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class AddressesBLL
    {
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public SCApplication scApp;
        public Database dataBase { get; private set; }
        public Cache cache { get; private set; }
        public Redis redis { get; private set; }
        public AddressesBLL()
        {
        }
        public void start(SCApplication _app)
        {
            scApp = _app;
            cache = new Cache(scApp.getCommObjCacheManager());
            redis = new Redis(scApp.getRedisCacheManager(), scApp.SectionBLL, scApp);
        }


        public class Database
        {

        }
        public class Cache
        {
            CommObjCacheManager CommObjCacheManager = null;
            public Cache(CommObjCacheManager commObjCacheManager)
            {
                CommObjCacheManager = commObjCacheManager;
            }
            public List<AADDRESS> GetAddresses()
            {
                return CommObjCacheManager.getAddresses();
            }
            public List<AADDRESS> GetAddressesBySectionID(string secID)
            {
                return CommObjCacheManager.getAddresses().
                    Where(adr => SCUtility.isMatche(adr.SEC_ID, secID)).
                    OrderBy(adr => adr.DISTANCE).
                    ToList();
            }
            public List<AADDRESS> LoadCanAvoidAddresses()
            {
                return CommObjCacheManager.getAddresses().
                    Where(adr => adr.CanAvoid).
                    ToList();
            }
            public List<CouplerAddress> GetCouplerAddresses()
            {
                List<CouplerAddress> CouplerAddresses = CommObjCacheManager.getCouplerAddresses();
                return CouplerAddresses;
            }
            public List<CouplerAddress> LoadCouplerAddresses(string chargerID)
            {
                List<CouplerAddress> CouplerAddresses = CommObjCacheManager.getCouplerAddresses();
                CouplerAddresses = CouplerAddresses.Where(coupler => SCUtility.isMatche(coupler.ChargerID, chargerID)).ToList();
                return CouplerAddresses;
            }
            public List<CouplerAddress> GetEnableCouplerAddresses(UnitBLL unitBLL)
            {
                List<CouplerAddress> CouplerAddresses = CommObjCacheManager.getCouplerAddresses();
                //CouplerAddresses = CouplerAddresses.Where(coupler => coupler.IsEnable).ToList();
                //CouplerAddresses = CouplerAddresses.Where(coupler => IsCouplerWork(coupler, unitBLL)).ToList();
                CouplerAddresses = CouplerAddresses.Where(coupler => coupler.IsWork(unitBLL)).ToList();
                return CouplerAddresses;
            }

            public bool IsCouplerWork(CouplerAddress couplerAddress, UnitBLL unitBLL)
            {
                AUNIT charger = unitBLL.OperateCatch.getUnit(couplerAddress.ChargerID);
                if (charger != null)
                {
                    switch (couplerAddress.CouplerNum)
                    {
                        case CouplerNum.NumberOne:
                            return charger.Coupler1Status == SCAppConstants.CouplerStatus.Auto ||
                                   charger.Coupler1Status == SCAppConstants.CouplerStatus.Charging;
                        case CouplerNum.NumberTwo:
                            return charger.Coupler1Status == SCAppConstants.CouplerStatus.Auto ||
                                   charger.Coupler1Status == SCAppConstants.CouplerStatus.Charging;
                        case CouplerNum.NumberThree:
                            return charger.Coupler1Status == SCAppConstants.CouplerStatus.Auto ||
                                   charger.Coupler1Status == SCAppConstants.CouplerStatus.Charging;
                    }
                }
                return false;
            }

            public AADDRESS GetAddress(string id)
            {
                return CommObjCacheManager.getAddress(id);
            }


        }
        public class Redis
        {
            TimeSpan timeOut_10min = new TimeSpan(0, 10, 0);
            TimeSpan timeOut_4sec = new TimeSpan(0, 0, 4);

            RedisCacheManager redisCacheManager = null;
            SectionBLL sectionBLL = null;
            public SCApplication scApp;

            public Redis(RedisCacheManager _redisCacheManager, SectionBLL _sectionBLL, SCApplication _app)
            {
                scApp = _app;
                redisCacheManager = _redisCacheManager;
                sectionBLL = _sectionBLL;
            }



            public (bool hasReserve, string reserveVh, string reserveBySection) hasAddressBeenReserve(string adrID)
            {
                string adr_reserve_info = string.Format(SCAppConstants.REDIS_KEY_ADDRESS_RESERVE_INFO_0, adrID.Trim());
                string reserve_vh_section = redisCacheManager.StringGet(adr_reserve_info);
                string[] reserve_vh_section_array = reserve_vh_section.Split('#');
                string reserve_vh = reserve_vh_section_array[0];
                string reserve_by_section = string.Empty;
                if (reserve_vh_section_array.Count() > 1)
                    reserve_by_section = reserve_vh_section_array[1];
                return (!SCUtility.isEmpty(reserve_vh), reserve_vh, reserve_by_section);
            }

            //public void setVehicleInReserveList(string vhID, string adrID)
            public bool setVehicleInReserveList(string vhID, string adrID, string sectionID)
            {
                string adr_reserve_key = string.Format(SCAppConstants.REDIS_KEY_ADDRESS_RESERVE_INFO_0, adrID.Trim());
                string adr_reserve_value = $"{vhID}#{SCUtility.Trim(sectionID, true)}";
                //redisCacheManager.stringSetAsync(adr_reserve_key, vhID);
                return redisCacheManager.stringSet(adr_reserve_key, adr_reserve_value);
            }

            public bool setReleaseAddressInfo(string vhID, string adrID)
            {
                string adr_vh_release_key = string.Format(SCAppConstants.REDIS_KEY_ADDRESS_RELEASE_INFO_0_1, SCUtility.Trim(adrID, true), SCUtility.Trim(vhID, true));
                string adr_vh_release_value = $"{SCUtility.Trim(adrID, true)}#{SCUtility.Trim(vhID, true)}";

                bool is_success = redisCacheManager.stringSet(adr_vh_release_key, adr_vh_release_value, timeOut_4sec);

                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(AddressesBLL), Device: "OHTC",
                   Data: $"Excute add address id:{adrID} release time out info by vh id:{vhID},result:{is_success}");

                return is_success;
            }

            public bool hasAddressBeenReleaseNotLongAgo(string adrID, string vhID)
            {
                string adr_vh_release_key = string.Format(SCAppConstants.REDIS_KEY_ADDRESS_RELEASE_INFO_0_1, SCUtility.Trim(adrID, true), SCUtility.Trim(vhID, true));
                bool is_exist = redisCacheManager.KeyExists(adr_vh_release_key);
                return is_exist;
            }


            public void AddressRelease(string vhID, string adrID)
            {
                string adr_id = adrID.Trim();
                string adr_reserve_info = string.Format(SCAppConstants.REDIS_KEY_ADDRESS_RESERVE_INFO_0, adr_id);
                string reserve_vh_section_id = redisCacheManager.StringGet(adr_reserve_info);
                string[] reserve_vh_section_array = reserve_vh_section_id.Split('#');
                string reserve_vh = reserve_vh_section_array[0];

                bool is_success = false;
                if (SCUtility.isMatche(vhID, reserve_vh))
                    is_success = redisCacheManager.KeyDelete(adr_reserve_info);
                if (is_success)
                {
                    if (scApp.getCommObjCacheManager().isSectionAtFireDoorArea(reserve_vh_section_array[1]))
                    {
                        scApp.getCommObjCacheManager().sectionUnreserveAtFireDoorArea(reserve_vh_section_array[1]);
                    }
                }
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(AddressesBLL), Device: "OHTC",
                   Data: $"Excute address id:{adr_id} release by vh id:{vhID},result:{is_success}");
            }
            //public string getReserveSectionByAddress(string address_id)
            //{
            //    string adr_id = address_id.Trim();
            //    string adr_reserve_info = string.Format(SCAppConstants.REDIS_KEY_ADDRESS_RESERVE_INFO_0, adr_id);
            //    string reserve_vh_section_id = redisCacheManager.StringGet(adr_reserve_info);
            //    string[] reserve_vh_section_array = reserve_vh_section_id.Split('#');
            //    string reserve_by_section = string.Empty;
            //    if (reserve_vh_section_array.Count() > 1)
            //        reserve_by_section = reserve_vh_section_array[1];
            //    return reserve_by_section;
            //}



            public bool AllAddressRelease(string vhID, string[] byPassAddress = null)
            {
                string key_pattern = string.Format(SCAppConstants.REDIS_KEY_ADDRESS_RESERVE_INFO_0, "*");
                var all_reserve_key = redisCacheManager.SearchKey(key_pattern).ToArray();
                if (all_reserve_key.Count() > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var reserve_key in all_reserve_key)
                    {
                        string value = redisCacheManager.StringGet(reserve_key, false);
                        string[] reserve_vh_section_array = value.Split('#');
                        string reserve_vh = reserve_vh_section_array[0];
                        if (SCUtility.isMatche(vhID, reserve_vh))
                        {
                            if (byPassAddress != null && byPassAddress.Count() > 0)
                            {
                                string s_reserve_key = reserve_key.ToString();
                                string[] reserve_key_array = s_reserve_key.Split('_');
                                string address_id = reserve_key_array.Last();
                                if (byPassAddress.Contains(address_id))
                                {
                                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(AddressesBLL), Device: "OHTC",
                                       Data: $"vh:{vhID} try to release address:{address_id}, but it is need pass address id:{string.Join(",", byPassAddress)} one of them",
                                       VehicleID: vhID);
                                    continue;
                                }
                            }
                            bool is_success = redisCacheManager.KeyDelete(reserve_key, false);
                            if (is_success)
                            {
                                if (scApp.getCommObjCacheManager().isSectionAtFireDoorArea(reserve_vh_section_array[1]))
                                {
                                    scApp.getCommObjCacheManager().sectionUnreserveAtFireDoorArea(reserve_vh_section_array[1]);
                                }
                            }

                            sb.Append($"key:{reserve_key},value:{value},is_success:{is_success}");
                            sb.AppendLine();
                        }
                    }
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(AddressesBLL), Device: "OHTC",
                       Data: $"Excute addresses release:{sb.ToString()},by vh id:{vhID}",
                       VehicleID: vhID);
                }
                return true;
            }



            public void ForceAllAddressRelease()
            {
                string key_pattern = string.Format(SCAppConstants.REDIS_KEY_ADDRESS_RESERVE_INFO_0, "*");
                var all_reserve_key = redisCacheManager.SearchKey(key_pattern).ToArray();
                string[] all_reserve_value_vh_sec_id = null;
                if (all_reserve_key.Count() > 0)
                {
                    all_reserve_value_vh_sec_id = redisCacheManager.StringGet(all_reserve_key).
                                                                Select(redis_value => redis_value.ToString()).
                                                                ToArray();
                    long count = redisCacheManager.KeyDelete(all_reserve_key);


                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < all_reserve_key.Count(); i++)
                    {
                        sb.Append($"key:{all_reserve_key[i]},value:{all_reserve_value_vh_sec_id[i]}");
                        sb.Append(",");
                        if (scApp.getCommObjCacheManager().isSectionAtFireDoorArea(all_reserve_value_vh_sec_id[i]))
                        {
                            scApp.getCommObjCacheManager().sectionUnreserveAtFireDoorArea(all_reserve_value_vh_sec_id[i]);
                        }
                    }


                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(AddressesBLL), Device: "OHTC",
                       Data: $"Excute force addresses release {sb.ToString()}");
                }
            }


            public List<string> loadReserveSection(string vhID)
            {
                string key_pattern = string.Format(SCAppConstants.REDIS_KEY_ADDRESS_RESERVE_INFO_0, "*");
                var all_reserve_key = redisCacheManager.SearchKey(key_pattern).ToArray();
                List<string> all_reserve_value_vh_sec_id = null;
                HashSet<string> sections = new HashSet<string>();
                if (all_reserve_key.Count() > 0)
                {

                    all_reserve_value_vh_sec_id = redisCacheManager.StringGet(all_reserve_key).
                                                            Select(redis_value => redis_value.ToString()).
                                                            ToList();
                    foreach (string reserve_vh_section in all_reserve_value_vh_sec_id)
                    {
                        string[] reserve_vh_section_array = reserve_vh_section.Split('#');
                        string reserve_vh = reserve_vh_section_array[0];
                        if (SCUtility.isMatche(vhID, reserve_vh))
                        {
                            string reserve_by_section = string.Empty;
                            if (reserve_vh_section_array.Count() > 1)
                                reserve_by_section = reserve_vh_section_array[1];
                            if (!SCUtility.isEmpty(reserve_by_section))
                            {
                                sections.Add(reserve_by_section);
                            }
                        }
                    }
                }
                return sections.ToList();
            }
        }
    }
}
