using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Script.Serialization;

namespace MSCRM.CRM.Plugins.BGY
{
    public static class GenerateSerialNumber
    {
        public static String GetNumber(String code, IOrganizationService service)
        {
            return string.Format("{0}{1}{2}", code, DateTime.Now.ToString("yyMMddHHmmssfff"), GenerateCoupon("0123456789", 3, new Random()));
        }

        private static string GenerateCoupon(string RandomChars, int Length, Random Random)
        {
            StringBuilder result = new StringBuilder(Length);
            for (int i = 0; i < Length; i++)
            {
                result.Append(RandomChars[Random.Next(RandomChars.Length)]);
            }
            return result.ToString();
        }

        public static List<TargetEntity> GetList()
        {
            return new List<TargetEntity>()
            {
                //new TargetEntity(){EntityName="mcs_uc_organization",PrefixCode="O-",WhichField="mcs_code"}
            };
        }
    }

    /// <summary>
    /// 实体类对象
    /// </summary>
    public class TargetEntity
    {
        /// <summary>
        /// 实体名
        /// </summary>
        public string EntityName { get; set; }
        /// <summary>
        /// 自动编号前缀
        /// </summary>
        public string PrefixCode { get; set; }
        /// <summary>
        /// 自动生成的字段
        /// </summary>
        public string WhichField { get; set; }
    }

    /// <summary>
    /// 管理员
    /// </summary>
    public static class CRMAdminHelper
    {
        /// <summary>
        /// 获取管理员guid
        /// </summary>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetCrmadminid(IOrganizationService service, IPluginExecutionContext context)
        {
            String userid = "";
            String fetchxml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' no-lock='true'>
                                  <entity name='mcs_configuration'>
                                    <attribute name='mcs_configurationid' />
                                    <attribute name='mcs_administrator' />
                                    <attribute name='mcs_name' />
                                    <filter type='and'>
                                      <condition attribute='mcs_name' operator='eq' value='SystemConfiguration' />
                                      <condition attribute='statecode' operator='eq' value='0' />
                                    </filter>
                                  </entity>
                                </fetch>";
            EntityCollection settings = service.RetrieveMultiple(new FetchExpression(fetchxml));
            if (settings.Entities.Count > 0)
            {
                Entity setting = settings.Entities[0];
                userid = setting.GetAttributeValue<EntityReference>("mcs_administrator").Id.ToString();
            }
            else
            {
                //配置信息中未能找到crmadminid，请联系管理员
                throw new InvalidPluginExecutionException("配置信息中未能找到crmadminid，请联系管理员");
            }
            return userid;
        }


        /// <summary>
        /// 获取管理员guid
        /// </summary>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetCrmadminid(IOrganizationService service)
        {
            String userid = "";
            String fetchxml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' no-lock='true'>
                                  <entity name='mcs_configuration'>
                                    <attribute name='mcs_configurationid' />
                                    <attribute name='mcs_administrator' />
                                    <attribute name='mcs_name' />
                                    <filter type='and'>
                                      <condition attribute='mcs_name' operator='eq' value='SystemConfiguration' />
                                      <condition attribute='statecode' operator='eq' value='0' />
                                    </filter>
                                  </entity>
                                </fetch>";
            EntityCollection settings = service.RetrieveMultiple(new FetchExpression(fetchxml));
            if (settings.Entities.Count > 0)
            {
                Entity setting = settings.Entities[0];
                userid = setting.GetAttributeValue<EntityReference>("mcs_administrator").Id.ToString();
            }
            else
            {
                //配置信息中未能找到crmadminid，请联系管理员
                throw new InvalidPluginExecutionException("配置信息中未能找到crmadminid，请联系管理员");
            }
            return userid;
        }
    }

    //CRM公共XML查询类
    public static class Commons
    {
        /// <summary>
        /// 应用参数配置查询方法
        /// </summary>
        /// <param name="key"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        public static Entity QueryByKey(string key, IOrganizationService service)
        {
            var strFetch = string.Format(@"<fetch version=""1.0"" output-format=""xml-platform"" mapping=""logical"" distinct=""false"">
                                <entity name = ""mcs_cepconfig"" >
                                    {1}
                                    <filter type = ""and"" >
                                        <condition attribute=""mcs_name"" operator=""eq"" uitype=""mcs_cepconfig"" value=""{0}"" />                    
                                    </filter>
                                </entity>
                            </fetch>", key, GetColumns());
            List<Entity> listData = new List<Entity>();
            CRMEntityHelper helper = new CRMEntityHelper(service);
            return helper.Retrive(strFetch);
        }
        private static string GetColumns()
        {
            return @"<attribute name = ""mcs_name"" />
                     <attribute name = ""mcs_val"" />";
        }

        /// <summary>
        /// 查询用户信息
        /// </summary>
        /// <param name="systeuserid"></param>
        /// <returns></returns>
        public static Entity QuerySystemUserById(Guid systeuserid, IOrganizationService service)
        {
            var strFetch = string.Format(@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                  <entity name='systemuser'>
                    <attribute name='fullname' />
                    <attribute name='businessunitid' />
                    <attribute name='mobilephone' />
                    <attribute name='systemuserid' />
                    <filter type='and'>
                      <condition attribute='systemuserid' operator='eq' value='{0}' />
                      <condition attribute='isdisabled' operator='eq' value='0' />
                    </filter>
                  </entity>
                </fetch>", systeuserid);
            List<Entity> listData = new List<Entity>();
            CRMEntityHelper helper = new CRMEntityHelper(service);
            return helper.Retrive(strFetch);
        }

        /// <summary>
        /// 对象序列化为JSON文本
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string ToJsJson(this object item)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(item.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, item);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        /// <summary>
        /// json文本转对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonText"></param>
        /// <returns></returns>
        public static T JSONToObject<T>(string jsonText)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            try
            {
                return jss.Deserialize<T>(jsonText);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }

}
