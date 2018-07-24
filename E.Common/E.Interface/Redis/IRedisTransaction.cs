using System;
using E.Interface.Redis.Pipeline;

namespace E.Interface.Redis
{
    /// <summary>
    /// Interface to redis transaction
    /// </summary>
    public interface IRedisTransaction
        : IRedisTransactionBase, IRedisQueueableOperation, IDisposable
    {
        bool Commit();
        void Rollback();
    }
}