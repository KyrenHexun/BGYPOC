﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSCRM.CRM.WorkflowActivities.BGY.Common
{
    public class ContextContainerDefault : IContextContainer
    {
        
        private ConcurrentDictionary<string, object> _contextList = new ConcurrentDictionary<string, object>();
        public T GetValue<T>(string name)
        {
            var context=GetContextFromList<T>(name);

            return context.Value;
        }

        public bool IsAuto<T>(string name)
        {
            var context = GetContextFromList<T>(name);
            return context.IsAuto();
        }

        public void Register<T>(string name, IContext<T> context)
        {
            _contextList[name] = context;
        }

        public void SetValue<T>(string name, T value)
        {
            var context = GetContextFromList<T>(name);
            context.Value = value;
        }

        private IContext<T> GetContextFromList<T>(string name)
        {
            object value;
            if (_contextList.TryGetValue(name, out value))
            {
                if (value == null)
                {
                    throw new Exception("the value of context names {0} in ContextContainerDefault is null");
                }

                IContext<T> context = null;
                try
                {
                    context = (IContext<T>)value;

                }
                catch
                {
                    throw new Exception(string.Format("the value type of context names {0} in ContextContainerDefault is {1}, it is not {2}", name, value.GetType().FullName, "IContext<" + typeof(T).FullName + ">"));
                }

                return context;
            }
            else
            {
                throw new Exception(string.Format("these is no context names {0} in ContextContainerDefault", name));
            }
        }
    }
}
