using System;
using System.ComponentModel.DataAnnotations;

namespace E.User.Repository.Models
{
    /// <summary>
    /// 用户账户
    /// </summary>
    public class Account
    {
        public Account()
        {
            StartTime = DateTime.Now;
            ExpiryTime = DateTime.MaxValue;
        }

        /// <summary>
        /// 帐户ID
        /// </summary>
        [Key]
        public long AccountId { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 用户账户余额
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// 用户账户类型
        /// </summary>
        public int AccountType { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 到期时间
        /// </summary>
        public DateTime ExpiryTime { get; set; }
    }
}
