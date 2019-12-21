using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSCRM.CRM.WorkflowActivities.BGY
{
    public class ActionResult
    {
        /// <summary>
        /// 
        /// </summary>
        public ActionResult() { }

        /// <summary>
        /// 
        /// </summary>
        public string Code { get; set; } = "000";

        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; } = "操作成功";
        /// <summary>
        /// 
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public ActionResult Error(string message)
        {
            ActionResult a = new ActionResult();
            a.Code = "001";
            a.Message = message;
            return a;
        }
    }
}
