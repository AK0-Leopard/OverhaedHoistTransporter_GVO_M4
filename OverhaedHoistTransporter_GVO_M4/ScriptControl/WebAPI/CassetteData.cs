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
    public class CassetteData : NancyModule
    {
        SCApplication app = null;
        const string restfulContentType = "application/json; charset=utf-8";
        const string urlencodedContentType = "application/x-www-form-urlencoded; charset=utf-8";
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public CassetteData()
        {
            //app = SCApplication.getInstance();
            RegisterPortStationEvent();
            After += ctx => ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");

        }
        private void RegisterPortStationEvent()
        {
            Post["CassetteData/Install"] = (p) =>
            {
                var scApp = SCApplication.getInstance();
                string result = string.Empty;

                string cst_id = Request.Query.cst_id.Value ?? Request.Form.cst_id.Value ?? string.Empty;
                string cst_loc = Request.Query.cst_loc.Value ?? Request.Form.cst_loc.Value ?? string.Empty;
                try
                {
                    var check_result = scApp.TransferService.ForceInstallCarrierInVehicle(cst_loc, cst_id);
                    if (check_result.isSuccess)
                    {
                        result = "OK";
                    }
                    else
                    {
                        result = check_result.result;
                    }
                    // need to add function
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
            Post["CassetteData/Remove"] = (p) =>
            {
                var scApp = SCApplication.getInstance();
                string result = string.Empty;

                string cst_id = Request.Query.cst_id.Value ?? Request.Form.cst_id.Value ?? string.Empty;
                try
                {
                    var check_result = scApp.TransferService.ForceRemoveCarrierInVehicleByOP(cst_id);
                    if (check_result.isSuccess)
                    {
                        result = "OK";
                    }
                    else
                    {
                        result = check_result.result;
                    }
                    // need to add function
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
