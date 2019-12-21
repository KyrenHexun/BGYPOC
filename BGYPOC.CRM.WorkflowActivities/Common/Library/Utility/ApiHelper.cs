using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MSCRM.CRM.WorkflowActivities.BGY
{
    public static class ApiHelper
    {
        static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(5);
        private static HttpClient GetHttpClient(IOrganizationService service,string configKey)
        {
            var baseAddress =ConfigHelper.GetApiActionUrl(service, configKey);
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(baseAddress); ;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
        [DataContract]
        public class ActionResult
        {

            /// <summary>
            /// 返回码
            /// </summary>
            [DataMember]
            public string Code { get; set; }

            /// <summary>
            /// 返回提示信息
            /// </summary>
            [DataMember]
            public string Message { get; set; }
        }
        /// <summary>
        /// MSCRM.API.Inventory接口Post请求
        /// </summary>
        /// <param name="service"></param>
        /// <param name="relativeUri"></param>
        /// <param name="parameters"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<ActionResult> InventoryPost(IOrganizationService service, string relativeUri, Dictionary<string, string> parameters, TimeSpan? timeout = null)
        {
       
            using (var client = GetHttpClient(service,ConfigHelper.RepertoryManagerApiUrlConfigKeyName))
            {
                client.Timeout = timeout.HasValue ? timeout.Value : DefaultTimeout;
                MemoryStream ms = new MemoryStream();
                parameters.FillFormDataStream(ms);
                HttpContent hc = new StreamContent(ms);
                hc.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response =  client.PostAsync(relativeUri, hc).Result;
             
                return await Task.FromResult(HandleApiResult(response)); 
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="relativeUri">接口URL</param>
        /// <param name="parameters">json格式字符串</param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<ActionResult> DealerVsPost(IOrganizationService service, string relativeUri, string parameters, TimeSpan? timeout = null)
        {
            using (var client = GetHttpClient(service, ConfigHelper.MSCRMApiOutUrlConfig))
            {
                client.Timeout = timeout.HasValue ? timeout.Value : DefaultTimeout;
                MemoryStream ms = new MemoryStream();
                byte[] byteArray = Encoding.UTF8.GetBytes(parameters);
                ms.Write(byteArray, 0, byteArray.Length);
                StringContent sc = new StringContent(parameters);
                sc.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = client.PostAsync(relativeUri, sc).Result;
                return await Task.FromResult(HandleApiResult(response));
            }
        }
        /// <summary>
        /// MSCRM.API.Sales接口Post请求
        /// </summary>
        /// <param name="service"></param>
        /// <param name="relativeUri"></param>
        /// <param name="parameters"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<ActionResult> SalesPost(IOrganizationService service, string relativeUri, Dictionary<string, string> parameters, TimeSpan? timeout = null)
        {
            using (var client = GetHttpClient(service, ConfigHelper.SalesManageApiUrlConfigKeyName))
            {
                client.Timeout = timeout.HasValue ? timeout.Value : DefaultTimeout;
                var data = ProgrammeHelper.ToJsJson(parameters);
                StringContent sc = new StringContent(data);
                sc.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var completeUrl = $"{client.BaseAddress}{relativeUri}";
                var response = client.PostAsync(completeUrl, sc).Result;
                return await Task.FromResult(HandleApiResult(response));
            }
        }

        /// <summary>
        /// MSCRM.WebSite.VS.Api接口Post请求
        /// </summary>
        /// <param name="service"></param>
        /// <param name="relativeUri"></param>
        /// <param name="parameters"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<ActionResult> SalesApiPost(IOrganizationService service, string relativeUri, string parameters, TimeSpan? timeout = null)
        {
            using (var client = GetHttpClient(service, ConfigHelper.SalesManageApiUrlConfigKeyName))
            {
                client.Timeout = timeout.HasValue ? timeout.Value : DefaultTimeout;
                StringContent sc = new StringContent(parameters);
                sc.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var completeUrl = $"{client.BaseAddress}{relativeUri}";
                var response = client.PostAsync(completeUrl, sc).Result;
                return await Task.FromResult(HandleApiResult(response));
            }
        }
        /// <summary>
        /// MSCRM.API.Sales接口Post请求
        /// </summary>
        public static async Task<ActionResult> SalesPost(IOrganizationService service, string relativeUri, string data, TimeSpan? timeout = null)
        {
            using (var client = GetHttpClient(service, ConfigHelper.SalesManageApiUrlConfigKeyName))
            {
                client.Timeout = timeout.HasValue ? timeout.Value : DefaultTimeout; 
                StringContent sc = new StringContent(data);
                sc.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var completeUrl = $"{client.BaseAddress}{relativeUri}";
                var response = client.PostAsync(completeUrl, sc).Result;
                return await Task.FromResult(HandleApiResult(response));
            }
        }
        /// <summary>
        /// MSCRM.WebSite.VS.WebApi接口Post请求
        /// </summary>
        /// <param name="service"></param>
        /// <param name="relativeUri"></param>
        /// <param name="parameters"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<ActionResult> SalesWebApiPost(IOrganizationService service, string relativeUri, string parameters, TimeSpan? timeout = null)
        {
            using (var client = GetHttpClient(service, ConfigHelper.SalesManageWebApiUrlConfig))
            {
                client.Timeout = timeout.HasValue ? timeout.Value : DefaultTimeout;
                StringContent sc = new StringContent(parameters);
                sc.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var completeUrl = $"{client.BaseAddress}{relativeUri}";
                var response = client.PostAsync(completeUrl, sc).Result;
                return await Task.FromResult(HandleApiResult(response));
            }
        }
        /// <summary>
        /// 填充表单信息的Stream
        /// </summary>
        /// <param name="formData"></param>
        /// <param name="stream"></param>
        public static void FillFormDataStream(this Dictionary<string, string> formData, Stream stream)
        {
            string dataString = GetQueryString(formData);
            var formDataBytes = formData == null ? new byte[0] : Encoding.UTF8.GetBytes(dataString);
            stream.Write(formDataBytes, 0, formDataBytes.Length);
            stream.Seek(0, SeekOrigin.Begin);//设置指针读取位置
        }
        private static ActionResult HandleApiResult(HttpResponseMessage response)
        {
            var actionResult = new ActionResult();
           
            if (response.IsSuccessStatusCode)
            {
                actionResult= response.Content.ReadAsStringAsync().Result.FromJsonTo<ActionResult>();
            }
            else
            {
                ParseErrorResponse(response, actionResult);
            }
            return actionResult;
        }
        private static void ParseErrorResponse(HttpResponseMessage response, ActionResult apiResult)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                apiResult.Code = "NotFound";
                apiResult.Message = string.Format("接口地址\"{0}\"不存在。请核对该接口地址是否真实存在或配置文件是否配置正确。",
                    response.RequestMessage.RequestUri);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout)
            {
                apiResult.Code = "GatewayTimeout";
                apiResult.Message = "服务器响应已超时。";
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                apiResult.Code = "ServiceUnavailable";
                apiResult.Message = "服务器暂时不可用。";
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed)
            {
                apiResult.Code = "MethodNotAllowed";
                apiResult.Message = string.Format("请求的资源\"{0}\"上不允许请求方法（{1}）。",
                    response.RequestMessage.RequestUri, response.RequestMessage.Method.ToString());
            }
            else
            {
                try
                {
                    var innerResult = response.Content.ReadAsStringAsync().Result.FromJsonTo<ActionResult>();
                    apiResult.Code = innerResult.Code;
                    apiResult.Message = innerResult.Message;
                }
                catch
                {
                    try
                    {
                        var innerResult = response.Content.ReadAsStringAsync().Result.FromJsonTo<ActionResult>();
                        apiResult.Code = "Error";
                        apiResult.Message = innerResult.Message;
                    }
                    catch (Exception exp)
                    {
                        apiResult.Code = "RestfulApiReadResponseError";
                        apiResult.Message = exp.Message;
                    }
                }
            }
        }
        /// <summary>
        /// 组装QueryString的方法
        /// 参数之间用&连接，首位没有符号，如：a=1&b=2&c=3
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
        public static string GetQueryString(this Dictionary<string, string> formData)
        {
            if (formData == null || formData.Count == 0)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();

            var i = 0;
            foreach (var kv in formData)
            {
                i++;
                sb.AppendFormat("{0}={1}", kv.Key, kv.Value);
                if (i < formData.Count)
                {
                    sb.Append("&");
                }
            }

            return sb.ToString();
        }


        public static Dictionary<string, string> ToDictionary(this object obj)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (obj == null) return dic;
            PropertyInfo[] pis = obj.GetType().GetProperties();
            for (int i = 0; i < pis.Length; i++)
            {
                object objValue = pis[i].GetValue(obj, null);
                objValue = (objValue == null) ? DBNull.Value : objValue;
                if (!dic.ContainsKey(pis[i].Name))
                {
                    dic.Add(pis[i].Name, objValue.ToString());
                }
            }
            return dic;
        }
    }
}
