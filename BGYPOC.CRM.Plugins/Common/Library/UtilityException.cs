using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSCRM.CRM.Plugins.BGY
{
    /// <summary>
    /// 系统错误
    /// 必须使用该类来抛出系统错误
    /// </summary>
    public class UtilityException : Exception
    {
        public int Code { get; set; }
        public UtilityException(int code, string message)
            : base(message)
        {
            throw new InvalidPluginExecutionException(message);
        }
    }
}
