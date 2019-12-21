using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSCRM.CRM.Plugins.BGY.Common
{
    public enum Errors
    {
        /// <summary>
        /// 配置错误
        /// </summary>
        ConfigError = 10001,
        /// <summary>
        /// 初始化错误
        /// </summary>
        InitError = 10002,
        /// <summary>
        /// 提示信息
        /// </summary>
        Message = 10003,
        /// <summary>
        /// 未获取对象信息
        /// </summary>
        Miss = 10004,
        /// <summary>
        /// 通讯连接错误
        /// </summary>
        CommunicationError = 10005,
        /// <summary>
        /// 业务字段校验
        /// </summary>
        ValidateError = 10006,
        /// <summary>
        /// 鉴权信息错误（安全配置）
        /// </summary>
        Security = 10007,
        /// <summary>
        /// 特殊校验错误
        /// </summary>
        UniqueValidate = 10008
    }
}
