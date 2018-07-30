using System;
using System.ComponentModel.DataAnnotations;
using E.User.Enumerations;

namespace E.User.Repository.Models
{
    public class Scheme
    {
        public Scheme()
        {
            CreateTime = DateTime.Now;
        }

        /// <summary>
        /// 方案索引
        /// </summary>
        [Key]
        public long SchemeIndex { get; set; }

        /// <summary>
        /// 方案ID
        /// </summary>
        public string SchemeId { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 游戏ID
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 中奖金额
        /// </summary>
        public decimal BonusMoney { get; set; }

        /// <summary>
        /// 最终派奖金额
        /// </summary>
        public decimal BonusPayMoney { get; set; }

        /// <summary>
        /// 号码
        /// </summary>
        public string Antecode { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public SchemeStatus Status { get; set; }

        /// <summary>
        /// 付款状态
        /// </summary>
        public PaymentStatus PaymentStatus { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
