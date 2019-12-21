using System;
using System.Reflection;
using Microsoft.Xrm.Sdk;


namespace MSCRM.CRM.Plugins.BGY
{
    public abstract class PluginBase : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ITracingService trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            trace.Trace("start");
            IOrganizationServiceFactory serviceFactory =
               (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            string crmadminid = CRMAdminHelper.GetCrmadminid(service, context);
            IOrganizationService adminservice = ((IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory))).CreateOrganizationService(new Guid(crmadminid));
            try
            {
                InnerExecute(context, trace, adminservice);
            }
            catch (Exception ex)
            {

                if (ex is UtilityException)
                {
                    throw new InvalidPluginExecutionException("系统业务错误：" + ex.Message);
                }
                else
                {
                    trace.Trace("系统内部错误：", ex.ToString() + ex.StackTrace);
                    throw;
                }
            }
        }



        public abstract void InnerExecute(IPluginExecutionContext context, ITracingService trace, IOrganizationService service);

    }
}
