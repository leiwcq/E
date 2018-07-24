using System;

namespace E.User.Enumerations
{
    /// <summary>
    /// 账户变更类型
    /// </summary>
    [Flags]
    public enum AccountChangeType
    {
        Initialization = 0,
        Payment = 100,
        Recharge = 200,
        Refund = 300
    }
}
