using System.Collections.Generic;

namespace E.Interface.Redis
{
    public class ScanResult
    {
        public ulong Cursor { get; set; }
        public List<byte[]> Results { get; set; }
    }
}