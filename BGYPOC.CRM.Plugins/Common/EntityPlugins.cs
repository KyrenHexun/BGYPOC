using Microsoft.Xrm.Sdk;
using System;
using System.Linq;

namespace MSCRM.CRM.Plugins.BGY.Common
{
    /// <summary>
    /// 实体自动编号生成插件
    /// </summary>
    public class EntityPreCreateForSerialNo : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            // If you are not registering the plug-in in the sandbox, then you do
            // not have to add any tracing service related code.
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory =
                   (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            string crmadminid = CRMAdminHelper.GetCrmadminid(service, context);
            IOrganizationService adminservice = serviceFactory.CreateOrganizationService(new Guid(crmadminid));
            // The InputParameters collection contains all the data passed in the message request.
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.
                Entity entity = (Entity)context.InputParameters["Target"];

                Do(entity,adminservice,tracingService);
            }
        }

        public void Do(Entity entity, IOrganizationService service, ITracingService tracingService)
        {
            try
            {
                // Plug-in business logic goes here.
                //获取对应实体配置
                var entityConfig = GenerateSerialNumber.GetList().FirstOrDefault(t => t.EntityName == entity.LogicalName);
                if (entityConfig != null)
                {
                    tracingService.Trace("找到相应配置，开始生成实体编号");
                    if (string.IsNullOrEmpty(entityConfig.PrefixCode))
                    {
                        throw new InvalidPluginExecutionException(string.Format("实体编号未配置：实体名称为{0},实体Id为{1}", entity.LogicalName, entity.Id));
                    }
                    //获取生成的自动编号
                    var SerialNumbe = GenerateSerialNumber.GetNumber(entityConfig.PrefixCode, service);
                    if (!entity.Attributes.Contains(entityConfig.WhichField))
                    {
                        entity.Attributes.Add(entityConfig.WhichField, SerialNumbe);
                    }
                    else
                    {
                        entity.Attributes[entityConfig.WhichField] = SerialNumbe;
                    }
                }
                
            }

            catch (Exception ex)
            {
                tracingService.Trace("系统内部错误: {0},实体名称为{1},实体记录Id为{2}", ex.ToString(), entity.LogicalName, entity.Id);
                throw;
            }
        }

        private Entity GetOrgById(IOrganizationService service, Guid id)
        {
            var fetch = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                          <entity name='mcs_uc_organization'>
                            <attribute name='mcs_systemorganization' />
                            <filter type='and'>
                              <condition attribute='mcs_uc_organizationid' operator='eq' value='{id}' />
                            </filter>
                          </entity>
                        </fetch>";
            var helper = new CRMEntityHelper(service);
            var model = helper.Retrive(fetch);
            return model;
        }
        private Entity GetBuByName(IOrganizationService service, string name, Guid parentBuID)
        {
            var fetch = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                          <entity name='businessunit'>
                            <attribute name='name' />
                            <filter type='and'>
                              <condition attribute='parentbusinessunitid' operator='eq' value='{parentBuID}' />
                              <condition attribute='name' operator='eq' value='{name}' />
                            </filter>
                          </entity>
                        </fetch>";
            var helper = new CRMEntityHelper(service);
            var model = helper.Retrive(fetch);
            return model;
        }
    }

    /// <summary>
    /// Demo-测试插件-mcs_spreadtask
    /// </summary>
    public class EntityCreatePostForSpreadTask : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Extract the tracing service for use in debugging sandboxed plug-ins.
            // If you are not registering the plug-in in the sandbox, then you do
            // not have to add any tracing service related code.
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory =
                   (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            // The InputParameters collection contains all the data passed in the message request.
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.
                Entity entity = (Entity)context.InputParameters["Target"];
                Do(entity,service,tracingService);
            }
        }

        public void Do(Entity entity, IOrganizationService service, ITracingService tracingService)
        {
            try
            {
                Entity spreadTaskUpdateEntity = new Entity(entity.LogicalName, entity.Id);
                spreadTaskUpdateEntity.Attributes.Add("mcs_totalquantity", 99999);
                service.Update(spreadTaskUpdateEntity);
            }
            catch (Exception ex)
            {
                tracingService.Trace("系统内部错误: {0},实体名称为{1},实体记录Id为{2}", ex.ToString(), entity.LogicalName, entity.Id);
                throw;
            }
        }
        #region 私有方法

        /// <summary>
        /// 获取配置信息公共方法
        /// </summary>
        /// <param name="service"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private Entity QueryCEPConfigByKey(IOrganizationService service, string key)
        {

            var strFetch = string.Format(@"<fetch version=""1.0"" output-format=""xml-platform"" mapping=""logical"" distinct=""false"">
                                    <entity name = ""mcs_cepconfig"" >
                                        <attribute name = ""mcs_name"" />
                                        <attribute name = ""mcs_val"" />
                                        <filter type = ""and"" >
                                            <condition attribute=""mcs_name"" operator=""eq"" uitype=""mcs_cepconfig"" value=""{0}"" />                    
                                        </filter>
                                    </entity>
                                </fetch>", key);
            var helper = new CRMEntityHelper(service);
            var chargingpilesite = helper.Retrive(strFetch);
            return chargingpilesite;

        }

        private Entity GetUserAccountById(IOrganizationService service, Guid id)
        {
            var fetch = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                          <entity name='mcs_uc_useraccount'>
                            <attribute name='mcs_organization' />
                            <filter type='and'>
                              <condition attribute='mcs_uc_useraccountid' operator='eq' value='{id}' />
                            </filter>
                          </entity>
                        </fetch>";
            var helper = new CRMEntityHelper(service);
            var model = helper.Retrive(fetch);
            return model;
        }


        private Entity GetTeamEntityByTeamName(IOrganizationService service, string temaName)
        {
            var fetch = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                          <entity name='team'>
                            <attribute name='teamid' />
                            <attribute name='name' />
                            <filter type='and'>
                              <condition attribute='name' operator='eq' value='{temaName}' />
                            </filter>
                          </entity>
                        </fetch>";
            var helper = new CRMEntityHelper(service);
            var model = helper.Retrive(fetch);
            return model;
        }
        #endregion
    }
}
