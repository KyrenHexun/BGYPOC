// =====================================================================
// 文件名：ClaimsRejectedEnum
// 功能描述： 保修索赔 涉及的字段枚举
// 创建时间：2019/6/17 作者：刘俊
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSCRM.CRM.Plugins.BGY
{
  public class ClaimsRejectedEnum
    {

        
        public enum SettleAccountStatus
        {
            待厅店验证 = 10,
            待索赔组验证 = 20,
            待财务验证 = 30,
            验证不通过 = 40, 
            修改完成 = 50,
            待开票 = 60,
            已寄出 = 70,
            已签收 = 80,
            已验收 = 90,
            已付款 = 100,
            已退回 = 110  
        }
        /// <summary>
        /// 供应商索赔单审批状态
        /// </summary>
        public enum SupplyclaimApprovalstatus
        {
            待提交 = 0,
            已提交 = 1,
            审批通过 = 2,
            审批不通过 = 3
        }
        /// <summary>
        /// 供应商索赔单审批状态
        /// </summary>
        public enum SupplyclaimStatus
        {
            预索赔 = 0,
            索赔中 = 1,
            已收款 = 2,
            拒赔 = 3
        }
        /// <summary>
        /// 保修索赔单的旧件返回状态
        /// </summary>
        public enum Returnpartsstatus
        {
            无需返还=1,
            已部分返还=2,
            已全部返还=3
        }
        /// <summary>
        /// 保修索赔单的审核状态
        /// </summary>
        public enum Approvalstatus
        { 
            未提交=10,
            预售权提报中=12,
            预售权审核中=15,
            审核中=20,
            驳回=30,
            拒赔=40,
            提交申诉=50,
            审核通过=60
        }
        
    }

  public class ConfigVariable
  {
      /// <summary>
      /// 专票金额：专票总金额/1.13
      /// </summary>
      public const decimal SupplierAmount = 1.13M;

      /// <summary>
      /// 专票总金额*0.13
      /// </summary>
      public const decimal SupplierTaxrate = 0.13M;

    }
}
