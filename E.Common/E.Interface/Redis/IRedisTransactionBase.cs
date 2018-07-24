﻿using E.Interface.Redis.Pipeline;

namespace E.Interface.Redis
{
    /// <summary>
    /// Base transaction interface, shared by typed and non-typed transactions
    /// </summary>
    public interface IRedisTransactionBase : IRedisPipelineShared
    {
    }
}
