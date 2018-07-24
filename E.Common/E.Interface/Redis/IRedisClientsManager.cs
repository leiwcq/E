using System;
using E.Interface.Caching;

namespace E.Interface.Redis
{
    public interface IRedisClientsManager : IDisposable
    {
        /// <summary>
        /// Returns a Read/Write client (The default) using the hosts defined in ReadWriteHosts
        /// </summary>
        /// <returns></returns>
        IRedisClient GetClient();

        /// <summary>
        /// Returns a ReadOnly client using the hosts defined in ReadOnlyHosts.
        /// </summary>
        /// <returns></returns>
        IRedisClient GetReadOnlyClient();

        /// <summary>
        /// Returns a Read/Write ICacheClient (The default) using the hosts defined in ReadWriteHosts
        /// </summary>
        /// <returns></returns>
        ICacheClient GetCacheClient();

        /// <summary>
        /// Returns a ReadOnly ICacheClient using the hosts defined in ReadOnlyHosts.
        /// </summary>
        /// <returns></returns>
        ICacheClient GetReadOnlyCacheClient();
    }
}