using System;

namespace E.Interface.Messaging
{
    [Flags]
    public enum MessageOption : int
    {
        None = 0,
        All = int.MaxValue,

        NotifyOneWay = 1 << 0,
    }
}