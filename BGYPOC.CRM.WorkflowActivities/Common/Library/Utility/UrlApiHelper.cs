using Microsoft.Xrm.Sdk;
using MSCRM.CRM.WorkflowActivities.BGY.Common;
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
    public static class UrlApiHelper
    {
        static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(5);
        private static HttpClient GetHttpClient(IOrganizationService service, string configKey)
        {
            var baseAddress = ConfigHelper.GetApiActionUrl(service, configKey);
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(baseAddress); ;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
        public class ActionResult
        {
            public int state { get; set; } = 1;

            public string msg { get; set; } = "操作成功";

            public RetrunValue results { get; set; }
        }

        public class RetrunValue
        {
            public List<Res> results { get; set; }
        }
        public class Res
        {
            public string code { get; set; }
            public int state { get; set; }
            public string msg { get; set; }
        }

        [DataContract]
        public class ErrorMessage
        {
            [DataMember]
            public int Code { get; set; }
            [DataMember]
            public string Message { get; set; }
        }
        /// <summary>
        /// MSCRM.API.OUT.VehicleSales接口Post请求
        /// </summary>
        /// <param name="service"></param>
        /// <param name="relativeUri"></param>
        /// <param name="parameters"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<ActionResult> VehicleSalesPost(IOrganizationService service, string relativeUri, Dictionary<string, string> parameters, TimeSpan? timeout = null)
        {

            using (var client = GetHttpClient(service, ConfigHelper.VehicleSalesApiUrlConfigKeyName))
            {
                client.Timeout = timeout.HasValue ? timeout.Value : DefaultTimeout;
                MemoryStream ms = new MemoryStream();
                parameters.FillFormDataStream(ms);
                HttpContent hc = new StreamContent(ms);
                hc.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = client.PostAsync(relativeUri, hc).Result;

                return await Task.FromResult(HandleApiResult(response));
            }
        }


        /// <summary>
        ///  Post提交对象，返回值类型为V
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="data"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static V Post<T, V>(IOrganizationService service, T data, string url, Dictionary<string, string> httpHeaders)
        {
            V result = default(V);

            StringContent httpContent = null;

            if (data != null)
            {
                var json = ProgrammeHelper.ToJsJson(data);

               // var json = JsonSerializerHelper.Serializer(data);
                httpContent = new StringContent(json, new UTF8Encoding(), "application/json");
            }

            using (var client = GetHttpClient(service, ConfigHelper.VehicleSalesApiUrlConfigKeyName))
            {
                if (httpHeaders != null)
                {
                    if (httpContent != null)
                    {
                        foreach (var headerItem in httpHeaders)
                        {
                            httpContent.Headers.Add(headerItem.Key, headerItem.Value);
                        }
                    }
                    else
                    {
                        foreach (var headerItem in httpHeaders)
                        {
                            client.DefaultRequestHeaders.Add(headerItem.Key, headerItem.Value);
                        }
                    }
                }

                var response = client.PostAsync(url, httpContent).Result;

                //Logger.WriteLog("SerialNumberServiceProxy StatusCode:"+response.StatusCode.ToString(), System.Diagnostics.EventLogEntryType.Warning);
                if (!response.IsSuccessStatusCode)
                {
                    //Logger.WriteLog(response.Content.ReadAsStringAsync().Result, System.Diagnostics.EventLogEntryType.Error);
                    ErrorMessage errorResult = null;
                    try
                    {
                        errorResult = new ErrorMessage();
                    }
                    catch
                    {
                        string errorMessage = response.Content.ReadAsStringAsync().Result;
                        throw new Exception(errorMessage);

                    }
                    if (errorResult != null)
                    {
                        if (errorResult.Message == null)
                        {
                            string errorMessage = response.Content.ReadAsStringAsync().Result;
                            throw new Exception(errorMessage);
                        }
                        else
                        {
                            throw new UtilityException(errorResult.Code, errorResult.Message);
                        }
                    }
                }
                else
                {
                    var strContent = response.Content.ReadAsStringAsync().Result;
                    //return JsonSerializerHelper.Deserialize<V>(strContent);
                    return  ProgrammeHelper.JSONToObject<V>(strContent);
                   
                    //return response.Content.ReadAsAsync<V>().Result;
                }
            }


            return result;
        }
        ///// <summary>
        ///// 填充表单信息的Stream
        ///// </summary>
        ///// <param name="formData"></param>
        ///// <param name="stream"></param>
        //public static void FillFormDataStream(this Dictionary<string, string> formData, Stream stream)
        //{
        //    string dataString = GetQueryString(formData);
        //    var formDataBytes = formData == null ? new byte[0] : Encoding.UTF8.GetBytes(dataString);
        //    stream.Write(formDataBytes, 0, formDataBytes.Length);
        //    stream.Seek(0, SeekOrigin.Begin);//设置指针读取位置
        //}
        private static ActionResult HandleApiResult(HttpResponseMessage response)
        {
            var actionResult = new ActionResult();

            if (response.IsSuccessStatusCode)
            {
                actionResult = response.Content.ReadAsStringAsync().Result.FromJsonTo<ActionResult>();
            }
            else
            {
               // ParseErrorResponse(response, actionResult);
            }
            return actionResult;
        }
        //private static void ParseErrorResponse(HttpResponseMessage response, ActionResult apiResult)
        //{
        //    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        //    {
        //        apiResult.Code = "NotFound";
        //        apiResult.Message = string.Format("接口地址\"{0}\"不存在。请核对该接口地址是否真实存在或配置文件是否配置正确。",
        //            response.RequestMessage.RequestUri);
        //    }
        //    else if (response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout)
        //    {
        //        apiResult.Code = "GatewayTimeout";
        //        apiResult.Message = "服务器响应已超时。";
        //    }
        //    else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
        //    {
        //        apiResult.Code = "ServiceUnavailable";
        //        apiResult.Message = "服务器暂时不可用。";
        //    }
        //    else if (response.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed)
        //    {
        //        apiResult.Code = "MethodNotAllowed";
        //        apiResult.Message = string.Format("请求的资源\"{0}\"上不允许请求方法（{1}）。",
        //            response.RequestMessage.RequestUri, response.RequestMessage.Method.ToString());
        //    }
        //    else
        //    {
        //        try
        //        {
        //            var innerResult = response.Content.ReadAsStringAsync().Result.FromJsonTo<ActionResult>();
        //            apiResult.Code = innerResult.Code;
        //            apiResult.Message = innerResult.Message;
        //        }
        //        catch
        //        {
        //            try
        //            {
        //                var innerResult = response.Content.ReadAsStringAsync().Result.FromJsonTo<ActionResult>();
        //                apiResult.Code = "Error";
        //                apiResult.Message = innerResult.Message;
        //            }
        //            catch (Exception exp)
        //            {
        //                apiResult.Code = "RestfulApiReadResponseError";
        //                apiResult.Message = exp.Message;
        //            }
        //        }
        //    }
        //}
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


        //public static Dictionary<string, string> ToDictionary(this object obj)
        //{
        //    Dictionary<string, string> dic = new Dictionary<string, string>();
        //    if (obj == null) return dic;
        //    PropertyInfo[] pis = obj.GetType().GetProperties();
        //    for (int i = 0; i < pis.Length; i++)
        //    {
        //        object objValue = pis[i].GetValue(obj, null);
        //        objValue = (objValue == null) ? DBNull.Value : objValue;
        //        if (!dic.ContainsKey(pis[i].Name))
        //        {
        //            dic.Add(pis[i].Name, objValue.ToString());
        //        }
        //    }
        //    return dic;
        //}
    }
}
