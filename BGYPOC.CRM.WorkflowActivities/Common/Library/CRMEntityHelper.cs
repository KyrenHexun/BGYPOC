using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
namespace MSCRM.CRM.WorkflowActivities.BGY
{
    public class CRMEntityHelper
    {
        IOrganizationService orgService;
        public CRMEntityHelper(IOrganizationService _orgService)
        {
            orgService = _orgService;
        }

        /// <summary>
        /// 根据Fetch查询字符串获取第一条记录
        /// </summary>
        /// <param name="strFetch">Fetch查询字符串</param>
        /// <returns></returns>
        public  Entity Retrive(string strFetch)
        {
            //var orgService = ContextContainer.GetValue<IOrganizationService>(ContextType.OrgService.ToString());

            FetchExpression fetch = new FetchExpression(strFetch);
            var response = orgService.RetrieveMultiple(fetch);
            if (response.Entities.Count > 0)
            {
                return response.Entities[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 根据Fetch查询字符串获取所有记录
        /// </summary>
        /// <param name="strFetch">Fetch查询字符串</param>
        /// <param name="callBack"></param>
        public  void RetriveAll(string strFetch, Action<Entity> callBack)
        {
            int page = 1, count = 500;
            var doc = XDocument.Parse(strFetch);

            //var orgService = ContextContainer.GetValue<IOrganizationService>(ContextType.OrgService.ToString());


            while (true)
            {
                doc.Root.SetAttributeValue("page", page.ToString());
                doc.Root.SetAttributeValue("count", count.ToString());

                FetchExpression fetchExpression = new FetchExpression(doc.ToString());
                var queryResponse = orgService.RetrieveMultiple(fetchExpression);
                foreach (var entityItem in queryResponse.Entities.ToList())
                {
                    callBack(entityItem);
                }

                if (!queryResponse.MoreRecords)
                {
                    break;
                }

                page++;
            }

        }

        public  void RetrievTop(string strFetch, int size, Action<Entity> callBack)
        {
            var doc = XDocument.Parse(strFetch);

            //var orgService = ContextContainer.GetValue<IOrganizationService>(ContextType.OrgService.ToString());

            doc.Root.SetAttributeValue("count", size.ToString());

            FetchExpression fetchExpression = new FetchExpression(doc.ToString());
            var queryResponse = orgService.RetrieveMultiple(fetchExpression);
            foreach (var entityItem in queryResponse.Entities.ToList())
            {
                callBack(entityItem);
            }


        }
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
}
