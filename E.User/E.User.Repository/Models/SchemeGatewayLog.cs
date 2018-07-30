using System;
using System.ComponentModel.DataAnnotations;

namespace E.User.Repository.Models
{
    /// <summary>
    /// 方案网关日志记录
    /// </summary>
    public class SchemeGatewayLog
    {
        public SchemeGatewayLog()
        {
            CreateTime = DateTime.Now;
        }

        [Key] public long GatewayLogId { get; set; }

        public long SchemeIndex { get; set; }

        public string GatewayName { get; set; }

        public string Antecode { get; set; }

        public string GatewayAntecode { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal GatewayMoney { get; set; }

        /// <summary>
        /// 中奖金额
        /// </summary>
        public decimal GatewayBonusMoney { get; set; }

        /// <summary>
        /// 最终派奖金额
        /// </summary>
        public decimal GatewayBonusPayMoney { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}