using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using System.Reflection;

namespace MSCRM.CRM.WorkflowActivities.BGY.Common
{
    /// <summary>
    /// CRM Entity类的扩展方法
    /// </summary>
    public static class CrmEntityExtension
    {
        /// <summary>
        /// Int
        /// </summary> 
        public static int ToInt(this Entity entity, string name)
        {
            return entity.GetAttributeValue<int>(name);
        }

        public static int? ToIntNull(this Entity entity, string name)
        {
            return entity.GetAttributeValue<int?>(name);
        }


        /// <summary>
        /// string
        /// </summary> 
        public static string ToString(this Entity entity, string name)
        {
            return entity.GetAttributeValue<string>(name);
        }

        /// <summary>
        /// float
        /// </summary> 
        public static float ToFloat(this Entity entity, string name)
        {
            return entity.GetAttributeValue<float>(name);
        }

        /// <summary>
        /// Money
        /// </summary> 
        public static decimal ToMoney(this Entity entity, string name)
        {
            return entity.GetAttributeValue<Money>(name).Value;
        }

        /// <summary>
        /// OptionSetValue
        /// </summary> 
        public static OptionSetValue ToOptionSetValue(this Entity entity, string name)
        {
            return entity.GetAttributeValue<OptionSetValue>(name);
        }

        /// <summary>
        /// EntityReference
        /// </summary> 
        public static EntityReference ToEntityReference(this Entity entity, string name)
        {
            return entity.GetAttributeValue<EntityReference>(name);
        }


        public static T GetAttributeValue<T>(this Entity entity, string name)
        {
            if (entity.IsNotNull(name))
            {
                return entity.GetAttributeValue<T>(name);
            }
            return default(T);
        }

        /// <summary>
        /// 判断实体的某个字段是否为空
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="name">字段名称</param> 
        public static bool IsNotNull(this Entity entity, string name)
        {
            return entity.Contains(name) && entity.Attributes[name] != null;
        }
        public static Dictionary<string, object> AttributeKeyValueToDictionary(this Entity entity)
        {
            var list = new Dictionary<string, object>();
            foreach (var item in entity.Attributes)
            {
                list.Add(item.Key,item.Value);
            }
            return list;
        }
    }
}
