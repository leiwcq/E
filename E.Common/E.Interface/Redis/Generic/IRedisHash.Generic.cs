using System.Collections.Generic;
using E.Interface.Model;

namespace E.Interface.Redis.Generic
{
    public interface IRedisHash<TKey, TValue> : IDictionary<TKey, TValue>, IHasStringId
    {
        Dictionary<TKey, TValue> GetAll();
    }

}
