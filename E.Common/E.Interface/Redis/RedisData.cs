using System.Collections.Generic;

namespace E.Interface.Redis
{
    public class RedisData
    {
        public byte[] Data { get; set; }

        public List<RedisData> Children { get; set; } 
    }
}