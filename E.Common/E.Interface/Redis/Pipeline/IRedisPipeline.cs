namespace E.Interface.Redis.Pipeline
{
    /// <summary>
    /// Interface to redis pipeline
    /// </summary>
    public interface IRedisPipeline : IRedisPipelineShared, IRedisQueueableOperation
    {
    }
}