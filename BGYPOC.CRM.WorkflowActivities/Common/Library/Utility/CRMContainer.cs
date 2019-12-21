using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSCRM.CRM.WorkflowActivities.BGY.Common
{
    /// <summary>
    /// CRM对象容器的静态包装
    /// 所有plugin,workflowactivity的应用层对象工厂都通过这个静态容器注册
    /// </summary>
    public static class CRMContainer
    {
        private static ICRMContainer _crmContainer=new CRMContainerDefault();

        public static ICRMContainer Current
        {
            set
            {
                _crmContainer = value;
            }
        }


        public static void Register<T, V>(V obj)
        {
            _crmContainer.Register<T, V>(obj);
        }

        public static void Register<T, V>(string name, V obj)
        {
            _crmContainer.Register<T, V>(name,obj);
        }

        public static T Get<T>()
        {
           return _crmContainer.Get<T>();
        }


        public static T Get<T>(string name)
        {
            return _crmContainer.Get<T>(name);
        }
    }
}
