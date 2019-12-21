using System;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using MSCRM.CRM.WorkflowActivities.BGY.Common;

namespace MSCRM.CRM.WorkflowActivities.BGY
{
    public abstract class ActivityBase : CodeActivity
    {

        protected override void Execute(CodeActivityContext executionContext)
        {
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();
            tracingService.Trace("start");
            //Create the context
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            IWorkflowContext wfContext = executionContext.GetExtension<IWorkflowContext>();

            //Create Admin context
            //string crmadminid = ProgrammeHelper.GetCrmadminid(service);
            //IOrganizationService adminservice = serviceFactory.CreateOrganizationService(new Guid(crmadminid));
            try
            {
                InnerExecute(executionContext, wfContext, tracingService, service);
            }
            catch (Exception ex)
            {

                if (ex is UtilityException)
                {
                    throw new InvalidPluginExecutionException("系统业务错误：" + ex.Message);
                }
                else
                {
                    tracingService.Trace("系统内部错误：", ex.ToString() + ex.StackTrace);
                    throw;
                }
            }
        }

        public abstract void InnerExecute(CodeActivityContext context, IWorkflowContext wfContext, ITracingService tracingService, IOrganizationService service);

    }
}
