using System;
using System.ComponentModel.DataAnnotations;
using E.User.Enumerations;

namespace E.User.Repository.Models
{
    /// <summary>
    /// 用户信息
    /// </summary>
    public class User
    {
        public User()
        {
            CreateTime = DateTime.Now;
        }
        /// <summary>
        /// 用户ID
        /// </summary>
        [Key]
        public long UserId { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        [MaxLength(20)]
        [Required]

        public string UserName { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        [MaxLength(200)]
        public string NickName { get; set; }

        /// <summary>
        /// 用户密码
        /// </summary>
        [MaxLength(32)]
        [Required]

        public string PassWord { get; set; }

        /// <summary>
        /// 用户密钥
        /// </summary>
        [MaxLength(32)]
        [Required]
        public string SecretKey { get; set; }

        /// <summary>
        /// 用户状态
        /// </summary>
        public UserStatus Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}