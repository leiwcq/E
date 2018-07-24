using System;
using System.ComponentModel.DataAnnotations;
using E.User.Enumerations;

namespace E.User.Repository.Models
{
    public class AccountChangeLog
    {
        public AccountChangeLog()
        {
            OperatorTime = DateTime.Now;
        }

        /// <summary>
        /// 日志ID
        /// </summary>
        [Key]
        public long LogId { get; set; }

        /// <summary>
        /// 帐户ID
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 发起操作方案编号
        /// </summary>
        [MaxLength(32)]
        [Required]
        public string SchemeId { get; set; }

        /// <summary>
        /// 账户变更类型
        /// </summary>
        public AccountChangeType ChangeType { get; set; }

        /// <summary>
        /// 账户变更金额
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 账户变更前余额
        /// </summary>
        public decimal BeforeBalance { get; set; }

        /// <summary>
        /// 账户变更后余额
        /// </summary>
        public decimal AfterBalance { get; set; }

        /// <summary>
        /// 账户变更描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 操作员
        /// </summary>
        public long OperatorUserId { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime OperatorTime { get; set; }
    }
}
