using E.Interface.Redis.Pipeline;

namespace E.Interface.Redis.Generic
{
    /// <summary>
    /// Interface to redis typed pipeline
    /// </summary>
    public interface IRedisTypedPipeline<T> : IRedisPipelineShared, IRedisTypedQueueableOperation<T>
    {
    }
}
