using System.Collections.Generic;

namespace E.Interface.Redis
{
    public class RedisText
    {
        public string Text { get; set; }

        public List<RedisText> Children { get; set; }
    }
}