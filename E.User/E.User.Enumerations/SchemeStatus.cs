using System;

namespace E.User.Enumerations
{
    /// <summary>
    /// 方案状态
    /// </summary>
    [Flags]
    public enum SchemeStatus
    {
        Receive = 0,
        Paying = 10,
        Wating =20,
        Bonus = 30,
        Complete = 40,
        Refund = 50,
        Error = 90
    }
}
