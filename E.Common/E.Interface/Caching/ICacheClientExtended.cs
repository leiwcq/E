using System;
using System.Collections.Generic;

namespace E.Interface.Caching
{
    /// <inheritdoc />
    /// <summary>
    /// Extend ICacheClient API with shared, non-core features
    /// </summary>
    public interface ICacheClientExtended : ICacheClient
    {
        TimeSpan? GetTimeToLive(string key);

        IEnumerable<string> GetKeysByPattern(string pattern);
    }
}