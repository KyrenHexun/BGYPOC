using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MSCRM.CRM.WorkflowActivities.BGY.Common
{
    /// <summary>
    ///  CRM中的对象容器的默认实现
    /// </summary>
    public class CRMContainerDefault : ICRMContainer
    {
        
        private static ConcurrentDictionary<string, object> _noNameList = new ConcurrentDictionary<string, object>();

        private static ConcurrentDictionary<string, ConcurrentDictionary<string, object>> _nameList = new ConcurrentDictionary<string, ConcurrentDictionary<string, object>>();

        public void Register<T, V>(V obj)
        {
            var tType = typeof(T);
            var vType = typeof(V);

            //如果是接口，需要判断V是否实现了接口
            if (tType.IsInterface)
            {
                if (!tType.IsAssignableFrom(vType))
                {
                    throw new Exception(string.Format("in CRMContainerDefault.Register type {0} is not implement {1}",vType.FullName,tType.FullName));
                }
            }
            else 
            {
                throw new Exception(string.Format("in CRMContainerDefault.Register type {0} is not a interface", tType.FullName));
            }

            _noNameList[tType.FullName] = obj;
        }

        public void Register<T, V>(string name, V obj)
        {
            var tType = typeof(T);
            var vType = typeof(V);

            //如果是接口，需要判断V是否实现了接口
            if (tType.IsInterface)
            {
                if (!tType.IsAssignableFrom(vType))
                {
                    throw new Exception(string.Format("in CRMContainerDefault.Register type {0} is not implement {1}", vType.FullName, tType.FullName));
                }
            }
            else
            {
                throw new Exception(string.Format("in CRMContainerDefault.Register type {0} is not a interface", tType.FullName));
            }

            ConcurrentDictionary<string, object> dict;
            if (!_nameList.TryGetValue(tType.FullName,out dict))
            {
                lock(_nameList)
                {
                    if (!_nameList.TryGetValue(tType.FullName, out dict))
                    {
                        dict = new ConcurrentDictionary<string, object>();
                        _nameList[tType.FullName] = dict;
                    }
                }
            }

            dict[name] = obj;
        }

        public T Get<T>()
        {
            var tType = typeof(T);
            object value;
            if (!_noNameList.TryGetValue(tType.FullName,out value))
            {
                return default(T);
            }
            else
            {
                return (T)value;
            }
        }

        public T Get<T>(string name)
        {
            var tType = typeof(T);
            ConcurrentDictionary<string, object> dict;
            object value;
            if (!_nameList.TryGetValue(tType.FullName, out dict))
            {
                return default(T);
            }

            if (!dict.TryGetValue(name, out value))
            {
                return default(T);
            }
            else
            {
                return (T)value;
            }
        }
    }
}
