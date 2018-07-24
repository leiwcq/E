using System;

namespace E.User.Enumerations
{
    /// <summary>
    /// 用户状态
    /// </summary>
    [Flags]
    public enum UserStatus
    {
        Normal = 0,
        Locked = 9
    }
}
