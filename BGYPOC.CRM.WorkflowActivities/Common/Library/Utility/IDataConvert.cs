using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSCRM.CRM.WorkflowActivities.BGY.Common
{
    /// <summary>
    /// 数据转换接口
    /// </summary>
    public interface IDataConvert<To, From>
    {
        /// <summary>
        /// 从From转换为To
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        To ConvertFrom(From from);
        /// <summary>
        /// 从To转换为From
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        From ConvertTo(To to);
    }
}
