using com.mirle.ibg3k0.sc.App;
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
    public class PostStation : NancyModule
    {
        SCApplication app = null;
        const string restfulContentType = "application/json; charset=utf-8";
        const string urlencodedContentType = "application/x-www-form-urlencoded; charset=utf-8";
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public PostStation()
        {
            //app = SCApplication.getInstance();
            RegisterPortStationEvent();
            After += ctx => ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");

        }
        private void RegisterPortStationEvent()
        {
            Post["PortStation/PriorityUpdate"] = (p) =>
            {
                var scApp = SCApplication.getInstance();
                string result = string.Empty;

                string port_id = Request.Query.port_id.Value ?? Request.Form.port_id.Value ?? string.Empty;
                string priority = Request.Query.priority.Value ?? Request.Form.priority.Value ?? string.Empty;
                try
                {
                    int i_priority = 0;
                    if (int.TryParse(priority, out i_priority))
                    {
                        if (scApp.PortStationService.doUpdatePortStationPriority(port_id, i_priority))
                            result = "OK";
                        else
                            result = "Failed";
                    }
                    else result = "TryParse priority to int failed!";
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
            Post["PortStation/PortTypeUpdate"] = (p) =>
            {
                var scApp = SCApplication.getInstance();
                string result = string.Empty;

                string port_id = Request.Query.port_id.Value ?? Request.Form.port_id.Value ?? string.Empty;
                string port_type = Request.Query.port_type.Value ?? Request.Form.port_type.Value ?? string.Empty;
                try
                {
                    int i_port_type = 0;
                    if (int.TryParse(port_type, out i_port_type))
                    {
                        if (scApp.TransferService.PortTypeChange(port_id, (E_PortType)i_port_type, "Web API"))
                            result = "OK";
                        else
                            result = "Failed";
                    }
                    else result = "TryParse port_type to int failed!";
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
            Post["PortStation/BcrEnable"] = (p) =>
            {
                var scApp = SCApplication.getInstance();
                string result = string.Empty;

                string port_id = Request.Query.port_id.Value ?? Request.Form.port_id.Value ?? string.Empty;
                try
                {
                    if (scApp.TransferService.PortBCR_Enable(port_id, true))
                        result = "OK";
                    else
                        result = "Failed";
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
            Post["PortStation/BcrDisable"] = (p) =>
            {
                var scApp = SCApplication.getInstance();
                string result = string.Empty;

                string port_id = Request.Query.port_id.Value ?? Request.Form.port_id.Value ?? string.Empty;
                try
                {
                    if (scApp.TransferService.PortBCR_Enable(port_id, false))
                        result = "OK";
                    else
                        result = "Failed";
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
            Post["PortStation/ClearAlarm"] = (p) =>
            {
                var scApp = SCApplication.getInstance();
                string result = string.Empty;

                string port_id = Request.Query.port_id.Value ?? Request.Form.port_id.Value ?? string.Empty;
                try
                {
                    if (scApp.TransferService.PortAlarrmReset(port_id))
                        result = "OK";
                    else
                        result = "Failed";
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
            Post["PortStation/StatusChange"] = (p) =>
            {
                var scApp = SCApplication.getInstance();
                bool isSuccess = true;
                string result = string.Empty;

                string port_id = Request.Query.port_id.Value ?? Request.Form.port_id.Value ?? string.Empty;
                string status = Request.Query.status.Value ?? Request.Form.status.Value ?? string.Empty;
                com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage.PortStationServiceStatus service_status =
                default(com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage.PortStationServiceStatus);
                try
                {
                    isSuccess = Enum.TryParse(status, out service_status);
                    if (isSuccess)
                    {
                        isSuccess = scApp.PortStationService.doUpdatePortStationServiceStatus(port_id, service_status);
                    }
                }
                catch (Exception ex)
                {
                    isSuccess = false;
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
