// =====================================================================
// 文件名：ConfigHelper.cs
// 功能描述：配置实体辅助类
// 创建时间：01/08/2019 作者：甘之怿
// =====================================================================

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace MSCRM.CRM.WorkflowActivities.BGY
{
    public class ConfigHelper
    {
        public const string RepertoryManagerApiUrlConfigKeyName = "RepertoryManagerApiUrlConfig";
        public const string SalesManageApiUrlConfigKeyName = "SalesManageApiUrlConfig";
        public const string VehicleSalesApiUrlConfigKeyName = "VehicleSalesApiUrlConfig";
        public const string SalesManageWebApiUrlConfig = "SalesManageWebApiUrlConfig"; 
        public const string MSCRMApiOutUrlConfig = "MSCRMApiOutUrlConfig";
        /// <summary>
        /// 获取配置记录
        /// </summary>
        /// <param name="service"></param>
        /// <param name="configName"></param>
        /// <returns></returns>
        public static Entity GetCepConfigEntityByKey(IOrganizationService service, string configName)
        {
            if (string.IsNullOrWhiteSpace(configName))
            {
                return null;
            }
            string fetchStr =
                $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' no-lock='true' > 
                      <entity name='mcs_cepconfig' > 
                          <attribute name = ""mcs_name"" />
                          <attribute name = ""mcs_val"" />
                          <filter> 
                              <condition attribute='mcs_name' operator='eq' value='{configName}' /> 
                          </filter> 
                      </entity> 
                  </fetch> ";
            var entityCollection = service.RetrieveMultiple(new FetchExpression(fetchStr));
            return entityCollection.Entities.Count > 0 ? entityCollection.Entities[0] : null;
        }

        //获取api的url
        public static string GetApiUrl(IOrganizationService service, string keyName)
        {
            var cepConfigEntity = GetCepConfigEntityByKey(service, keyName);
            if (cepConfigEntity == null || !cepConfigEntity.Attributes.Contains("mcs_val"))
            {
                return null;
            }
            else
            {
                return cepConfigEntity.GetAttributeValue<string>("mcs_val");
            }
        }

        public static string GetApiActionUrl(IOrganizationService service, string keyName)
        {
            var cepConfigEntity = GetCepConfigEntityByKey(service, keyName);
            if (cepConfigEntity.Attributes.Contains("mcs_val"))
            {
                return cepConfigEntity.GetAttributeValue<string>("mcs_val");
            }
            else
            {
                return "";
            }
        }
    }
}
