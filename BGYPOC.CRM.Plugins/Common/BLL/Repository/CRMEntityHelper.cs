using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
namespace MSCRM.CRM.Plugins.BGY
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
}
