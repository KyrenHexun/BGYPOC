using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSCRM.CRM.WorkflowActivities.BGY.Common
{
    /// <summary>
    /// CRM中的对象容器
    /// 用于plugin、workflowactivity中的应用层对象的注册
    /// </summary>
    public interface ICRMContainer
    {
        /// <summary>
        /// 注册对象
        /// </summary>
        /// <typeparam name="T">注册的类型</typeparam>
        /// <typeparam name="V">注册的类型的实现者的类型</typeparam>
        /// <param name="obj">注册的类型的实现者对象</param>
        void Register<T,V>(V obj);

        /// <summary>
        /// 注册对象
        /// 允许同一个类型可以注册多个实现，通过name来唯一标识
        /// </summary>
        /// <typeparam name="T">注册的类型</typeparam>
        /// <typeparam name="V">注册的类型的实现者的类型</typeparam>
        /// <param name="name">特定名称</param>
        /// <param name="obj">注册的类型的实现者对象</param>
        void Register<T, V>(string name,V obj);

        /// <summary>
        /// 获取已注册的对象
        /// </summary>
        /// <typeparam name="T">注册的类型</typeparam>
        /// <returns>注册的类型的实现者对象</returns>
        T Get<T>();

        /// <summary>
        /// 获取已注册的对象(按名称)
        /// </summary>
        /// <typeparam name="T">注册的类型</typeparam>
        /// <param name="name">注册的名称</param>
        /// <returns>注册的类型的实现者对象</returns>
        T Get<T>(string name);
    }
}
