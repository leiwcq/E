using System.Collections.Generic;
using E.Interface.Model;

namespace E.Interface.Redis
{
    public interface IRedisHash
        : IDictionary<string, string>, IHasStringId
    {
        bool AddIfNotExists(KeyValuePair<string, string> item);
        void AddRange(IEnumerable<KeyValuePair<string, string>> items);
        long IncrementValue(string key, int incrementBy);
    }
}