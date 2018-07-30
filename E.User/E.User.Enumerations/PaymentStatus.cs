using System;

namespace E.User.Enumerations
{
    /// <summary>
    /// 付款状态
    /// </summary>
    [Flags]
    public enum PaymentStatus
    {
        Wating = 0,
        Complete = 10,
        BalanceIsNotEnough = 20,
        Refund = 30
    }
}
