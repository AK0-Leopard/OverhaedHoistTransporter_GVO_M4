using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using Nancy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.WebAPI
{
    public class EqptMaint : NancyModule
    {
        SCApplication app = null;
        const string restfulContentType = "application/json; charset=utf-8";
        const string urlencodedContentType = "application/x-www-form-urlencoded; charset=utf-8";
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public EqptMaint()
        {
            //app = SCApplication.getInstance();
            RegisterEqptMaintEvent();
            After += ctx => ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");

        }
        private void RegisterEqptMaintEvent()
        {
            Post["Eqpt/DateTimeSync", runAsync: true] = async (p, ct) =>
            {
                var scApp = SCApplication.getInstance();
                string result = string.Empty;

                string eqpt_id = Request.Query.eqpt_id.Value ?? Request.Form.eqpt_id.Value ?? string.Empty;
                try
                {
                    if (!string.IsNullOrEmpty(eqpt_id))
                    {
                        MaintainLift MTL = scApp.getEQObjCacheManager().getEquipmentByEQPTID(eqpt_id) as MaintainLift;
                        MTxValueDefMapActionBase MTLValueDefMapActionBase = MTL.getMapActionByIdentityKey(nameof(MTLValueDefMapAction)) as MTxValueDefMapActionBase;
                        await Task.Run(() => MTLValueDefMapActionBase.DateTimeSyncCommand(DateTime.Now));
                        result = "OK";
                    }
                    else
                        result = "Failed, eqpt_id should not be empty or null!";
                }
                catch (Exception ex)
                {
                    result = "Execption happend!";
                    logger.Error(ex, "Execption:");
                }

                var response = (Response)result;
                response.ContentType = restfulContentType;
                return response;
            };
            Post["Eqpt/ResetAllHandshake", runAsync: true] = async (p, ct) =>
            {
                var scApp = SCApplication.getInstance();
                string result = string.Empty;

                string eqpt_id = Request.Query.eqpt_id.Value ?? Request.Form.eqpt_id.Value ?? string.Empty;
                try
                {
                    if (!string.IsNullOrEmpty(eqpt_id))
                    {
                        MaintainLift MTL = scApp.getEQObjCacheManager().getEquipmentByEQPTID(eqpt_id) as MaintainLift;
                        MTxValueDefMapActionBase MTLValueDefMapActionBase = MTL.getMapActionByIdentityKey(nameof(MTLValueDefMapAction)) as MTxValueDefMapActionBase;
                        await Task.Run(() => MTLValueDefMapActionBase.OHxCResetAllhandshake());
                        result = "OK";
                    }
                    else
                        result = "Failed, eqpt_id should not be empty or null!";
                }
                catch (Exception ex)
                {
                    result = "Execption happend!";
                    logger.Error(ex, "Execption:");
                }

                var response = (Response)result;
                response.ContentType = restfulContentType;
                return response;
            };
            Post["Eqpt/ResetAlarm", runAsync: true] = async (p, ct) =>
            {
                var scApp = SCApplication.getInstance();
                string result = string.Empty;

                string eqpt_id = Request.Query.eqpt_id.Value ?? Request.Form.eqpt_id.Value ?? string.Empty;
                try
                {
                    if (!string.IsNullOrEmpty(eqpt_id))
                    {
                        MaintainLift MTL = scApp.getEQObjCacheManager().getEquipmentByEQPTID(eqpt_id) as MaintainLift;
                        MTxValueDefMapActionBase MTLValueDefMapActionBase = MTL.getMapActionByIdentityKey(nameof(MTLValueDefMapAction)) as MTxValueDefMapActionBase;
                        bool bResult = false;
                        await Task.Run(() => bResult = MTLValueDefMapActionBase.OHxC_AlarmResetRequest());
                        result = bResult ? "OK" : "Failed";
                    }
                    else
                        result = "Failed, eqpt_id should not be empty or null!";
                }
                catch (Exception ex)
                {
                    result = "Execption happend!";
                    logger.Error(ex, "Execption:");
                }

                var response = (Response)result;
                response.ContentType = restfulContentType;
                return response;
            };
            Post["Eqpt/SetCarInInterlock", runAsync: true] = async (p, ct) =>
            {
                var scApp = SCApplication.getInstance();
                string result = string.Empty;

                string eqpt_id = Request.Query.eqpt_id.Value ?? Request.Form.eqpt_id.Value ?? string.Empty;
                string set_value = Request.Query.setValue.Value ?? Request.Form.setValue.Value ?? string.Empty;
                bool setValue = false;
                try
                {
                    if (string.IsNullOrEmpty(eqpt_id))
                    {
                        result = "Failed, eqpt_id should not be empty or null!";
                    }
                    else if (!Boolean.TryParse(set_value, out setValue))
                    {
                        result = "Failed, setValue not given!";
                    }
                    else
                    {
                        MaintainLift MTL = scApp.getEQObjCacheManager().getEquipmentByEQPTID(eqpt_id) as MaintainLift;
                        MTxValueDefMapActionBase MTLValueDefMapActionBase = MTL.getMapActionByIdentityKey(nameof(MTLValueDefMapAction)) as MTxValueDefMapActionBase;
                        bool bResult = false;
                        await Task.Run(() => bResult = MTLValueDefMapActionBase.setOHxC2MTL_CarInInterlock(setValue));
                        result = bResult? "OK" : "Failed";
                    }
                }
                catch (Exception ex)
                {
                    result = "Execption happend!";
                    logger.Error(ex, "Execption:");
                }

                var response = (Response)result;
                response.ContentType = restfulContentType;
                return response;
            };
            Post["Eqpt/SetCarInMoving", runAsync: true] = async (p, ct) =>
            {
                var scApp = SCApplication.getInstance();
                string result = string.Empty;

                string eqpt_id = Request.Query.eqpt_id.Value ?? Request.Form.eqpt_id.Value ?? string.Empty;
                string set_value = Request.Query.setValue.Value ?? Request.Form.setValue.Value ?? string.Empty;
                bool setValue = false;
                try
                {
                    if (string.IsNullOrEmpty(eqpt_id))
                    {
                        result = "Failed, eqpt_id should not be empty or null!";
                    }
                    else if (!Boolean.TryParse(set_value, out setValue))
                    {
                        result = "Failed, setValue not given!";
                    }
                    else
                    {
                        MaintainLift MTL = scApp.getEQObjCacheManager().getEquipmentByEQPTID(eqpt_id) as MaintainLift;
                        MTxValueDefMapActionBase MTLValueDefMapActionBase = MTL.getMapActionByIdentityKey(nameof(MTLValueDefMapAction)) as MTxValueDefMapActionBase;
                        bool bResult = false;
                        await Task.Run(() => bResult = MTLValueDefMapActionBase.setOHxC2MTL_CarInMoving(setValue));
                        result = bResult ? "OK" : "Failed";
                    }
                }
                catch (Exception ex)
                {
                    result = "Execption happend!";
                    logger.Error(ex, "Execption:");
                }

                var response = (Response)result;
                response.ContentType = restfulContentType;
                return response;
            };
            Post["Eqpt/SetCarInMoveComplete", runAsync: true] = async (p, ct) =>
            {
                var scApp = SCApplication.getInstance();
                string result = string.Empty;

                string eqpt_id = Request.Query.eqpt_id.Value ?? Request.Form.eqpt_id.Value ?? string.Empty;
                string set_value = Request.Query.setValue.Value ?? Request.Form.setValue.Value ?? string.Empty;
                bool setValue = false;
                try
                {
                    if (string.IsNullOrEmpty(eqpt_id))
                    {
                        result = "Failed, eqpt_id should not be empty or null!";
                    }
                    else if (!Boolean.TryParse(set_value, out setValue))
                    {
                        result = "Failed, setValue not given!";
                    }
                    else
                    {
                        MaintainLift MTL = scApp.getEQObjCacheManager().getEquipmentByEQPTID(eqpt_id) as MaintainLift;
                        MTxValueDefMapActionBase MTLValueDefMapActionBase = MTL.getMapActionByIdentityKey(nameof(MTLValueDefMapAction)) as MTxValueDefMapActionBase;
                        bool bResult = false;
                        await Task.Run(() => bResult = MTLValueDefMapActionBase.setOHxC2MTL_CarInMoveComplete(setValue));
                        result = bResult ? "OK" : "Failed";
                    }
                }
                catch (Exception ex)
                {
                    result = "Execption happend!";
                    logger.Error(ex, "Execption:");
                }

                var response = (Response)result;
                response.ContentType = restfulContentType;
                return response;
            };
            Post["Eqpt/SetCarOutInterlock", runAsync: true] = async (p, ct) =>
            {
                var scApp = SCApplication.getInstance();
                string result = string.Empty;

                string eqpt_id = Request.Query.eqpt_id.Value ?? Request.Form.eqpt_id.Value ?? string.Empty;
                string set_value = Request.Query.setValue.Value ?? Request.Form.setValue.Value ?? string.Empty;
                bool setValue = false;
                try
                {
                    if (string.IsNullOrEmpty(eqpt_id))
                    {
                        result = "Failed, eqpt_id should not be empty or null!";
                    }
                    else if (!Boolean.TryParse(set_value, out setValue))
                    {
                        result = "Failed, setValue not given!";
                    }
                    else
                    {
                        MaintainLift MTL = scApp.getEQObjCacheManager().getEquipmentByEQPTID(eqpt_id) as MaintainLift;
                        MTxValueDefMapActionBase MTLValueDefMapActionBase = MTL.getMapActionByIdentityKey(nameof(MTLValueDefMapAction)) as MTxValueDefMapActionBase;
                        bool bResult = false;
                        await Task.Run(() => bResult = MTLValueDefMapActionBase.setOHxC2MTL_CarOutInterlock(setValue));
                        result = bResult ? "OK" : "Failed";
                    }
                }
                catch (Exception ex)
                {
                    result = "Execption happend!";
                    logger.Error(ex, "Execption:");
                }

                var response = (Response)result;
                response.ContentType = restfulContentType;
                return response;
            };
            Post["Eqpt/SetCarOutReady", runAsync: true] = async (p, ct) =>
            {
                var scApp = SCApplication.getInstance();
                string result = string.Empty;

                string eqpt_id = Request.Query.eqpt_id.Value ?? Request.Form.eqpt_id.Value ?? string.Empty;
                string set_value = Request.Query.setValue.Value ?? Request.Form.setValue.Value ?? string.Empty;
                bool setValue = false;
                try
                {
                    if (string.IsNullOrEmpty(eqpt_id))
                    {
                        result = "Failed, eqpt_id should not be empty or null!";
                    }
                    else if (!Boolean.TryParse(set_value, out setValue))
                    {
                        result = "Failed, setValue not given!";
                    }
                    else
                    {
                        MaintainLift MTL = scApp.getEQObjCacheManager().getEquipmentByEQPTID(eqpt_id) as MaintainLift;
                        MTxValueDefMapActionBase MTLValueDefMapActionBase = MTL.getMapActionByIdentityKey(nameof(MTLValueDefMapAction)) as MTxValueDefMapActionBase;
                        bool bResult = false;
                        await Task.Run(() => bResult = MTLValueDefMapActionBase.setOHxC2MTL_CarOutReady(setValue));
                        result = bResult ? "OK" : "Failed";
                    }
                }
                catch (Exception ex)
                {
                    result = "Execption happend!";
                    logger.Error(ex, "Execption:");
                }

                var response = (Response)result;
                response.ContentType = restfulContentType;
                return response;
            };
            Post["Eqpt/SetCarOutMoving", runAsync: true] = async (p, ct) =>
            {
                var scApp = SCApplication.getInstance();
                string result = string.Empty;

                string eqpt_id = Request.Query.eqpt_id.Value ?? Request.Form.eqpt_id.Value ?? string.Empty;
                string set_value = Request.Query.setValue.Value ?? Request.Form.setValue.Value ?? string.Empty;
                bool setValue = false;
                try
                {
                    if (string.IsNullOrEmpty(eqpt_id))
                    {
                        result = "Failed, eqpt_id should not be empty or null!";
                    }
                    else if (!Boolean.TryParse(set_value, out setValue))
                    {
                        result = "Failed, setValue not given!";
                    }
                    else
                    {
                        MaintainLift MTL = scApp.getEQObjCacheManager().getEquipmentByEQPTID(eqpt_id) as MaintainLift;
                        MTxValueDefMapActionBase MTLValueDefMapActionBase = MTL.getMapActionByIdentityKey(nameof(MTLValueDefMapAction)) as MTxValueDefMapActionBase;
                        bool bResult = false;
                        await Task.Run(() => bResult = MTLValueDefMapActionBase.setOHxC2MTL_CarOutMoving(setValue));
                        result = bResult ? "OK" : "Failed";
                    }
                }
                catch (Exception ex)
                {
                    result = "Execption happend!";
                    logger.Error(ex, "Execption:");
                }

                var response = (Response)result;
                response.ContentType = restfulContentType;
                return response;
            };
            Post["Eqpt/SetCarOutMoveComplete", runAsync: true] = async (p, ct) =>
            {
                var scApp = SCApplication.getInstance();
                string result = string.Empty;

                string eqpt_id = Request.Query.eqpt_id.Value ?? Request.Form.eqpt_id.Value ?? string.Empty;
                string set_value = Request.Query.setValue.Value ?? Request.Form.setValue.Value ?? string.Empty;
                bool setValue = false;
                try
                {
                    if (string.IsNullOrEmpty(eqpt_id))
                    {
                        result = "Failed, eqpt_id should not be empty or null!";
                    }
                    else if (!Boolean.TryParse(set_value, out setValue))
                    {
                        result = "Failed, setValue not given!";
                    }
                    else
                    {
                        MaintainLift MTL = scApp.getEQObjCacheManager().getEquipmentByEQPTID(eqpt_id) as MaintainLift;
                        MTxValueDefMapActionBase MTLValueDefMapActionBase = MTL.getMapActionByIdentityKey(nameof(MTLValueDefMapAction)) as MTxValueDefMapActionBase;
                        bool bResult = false;
                        await Task.Run(() => bResult = MTLValueDefMapActionBase.setOHxC2MTL_CarOutMoveComplete(setValue));
                        result = bResult ? "OK" : "Failed";
                    }
                }
                catch (Exception ex)
                {
                    result = "Execption happend!";
                    logger.Error(ex, "Execption:");
                }

                var response = (Response)result;
                response.ContentType = restfulContentType;
                return response;
            };
            Post["Eqpt/AutoCarOut", runAsync: true] = async (p, ct) =>
            {
                var scApp = SCApplication.getInstance();
                string result = string.Empty;

                string eqpt_id = Request.Query.eqpt_id.Value ?? Request.Form.eqpt_id.Value ?? string.Empty;
                string vh_id = Request.Query.vh_id.Value ?? Request.Form.vh_id.Value ?? string.Empty;
                try
                {
                    if (string.IsNullOrEmpty(eqpt_id))
                    {
                        result = "Failed, eqpt_id should not be empty or null!";
                    }
                    else if (string.IsNullOrEmpty(vh_id))
                    {
                        result = "Failed, vh_id should not be empty or null!";
                    }
                    else
                    {
                        MaintainLift MTL = scApp.getEQObjCacheManager().getEquipmentByEQPTID(eqpt_id) as MaintainLift;
                        AVEHICLE VH = scApp.VehicleBLL.cache.getVehicle(vh_id);
                        if (MTL == null)
                        {
                            result = $"Failed, Can't getEquipmentByEQPTID({eqpt_id})!";
                        }
                        else if (VH == null)
                        {
                            result = $"Failed, Can't getVehicle({vh_id})!";
                        }
                        else
                        {
                            await Task.Run(() => scApp.MTLService.AutoCarOut(MTL, VH));
                            result = "OK";
                        }
                    }
                }
                catch (Exception ex)
                {
                    result = "Execption happend!";
                    logger.Error(ex, "Execption:");
                }

                var response = (Response)result;
                response.ContentType = restfulContentType;
                return response;
            };
            Post["Eqpt/AutoCarOutCancel", runAsync: true] = async (p, ct) =>
            {
                var scApp = SCApplication.getInstance();
                string result = string.Empty;

                string eqpt_id = Request.Query.eqpt_id.Value ?? Request.Form.eqpt_id.Value ?? string.Empty;
                string vh_id = Request.Query.vh_id.Value ?? Request.Form.vh_id.Value ?? string.Empty;
                try
                {
                    if (string.IsNullOrEmpty(eqpt_id))
                    {
                        result = "Failed, eqpt_id should not be empty or null!";
                    }
                    else if (string.IsNullOrEmpty(vh_id))
                    {
                        result = "Failed, vh_id should not be empty or null!";
                    }
                    else
                    {
                        var r = default((bool isSuccess, string result));
                        MaintainLift MTL = scApp.getEQObjCacheManager().getEquipmentByEQPTID(eqpt_id) as MaintainLift;
                        await Task.Run(() => scApp.MTLService.carOutRequestCancel(MTL));
                        result = "OK";
                    }
                }
                catch (Exception ex)
                {
                    result = "Execption happend!";
                    logger.Error(ex, "Execption:");
                }

                var response = (Response)result;
                response.ContentType = restfulContentType;
                return response;
            };
        }

        private static StringBuilder setOhxCContent(StringBuilder sb, string key, int value, string description)
        {
            sb.AppendLine($"#{PROMETHEUS_TOKEN_HELP} ohxc_{key} {description}");
            sb.AppendLine($"#{PROMETHEUS_TOKEN_TYPE} ohxc_{key} gauge");
            sb.AppendLine($"ohxc_{key} {value}");
            return sb;
        }
        const string PROMETHEUS_TOKEN_HELP = "HELP";
        const string PROMETHEUS_TOKEN_TYPE = "TYPE";
    }
}
