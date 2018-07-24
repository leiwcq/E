using System;
using System.Collections.Generic;
using System.Globalization;

namespace E.IdentityGeneration
{
    public static class IdentityGeneration
    {
        internal class IdGen
        {
            public long Index { get; set; }
            public DateTime DateTime { get; set; }
        }

        private static Dictionary<string, IdGen> _dicIdGen;
        private const string DATE_TIME_FORMAT = "yyyyMMddHHmmss";

        public static string BuildSingleId(string prefix)
        {
            lock ("SingleIdLocker")
            {
                string singleId;
                if (_dicIdGen == null)
                    _dicIdGen = new Dictionary<string, IdGen>();

                if (_dicIdGen.ContainsKey(prefix))
                {
                    var thisTime = DateTime.Now;
                    var gen = _dicIdGen[prefix];
                    if (thisTime.ToString(DATE_TIME_FORMAT).Equals(gen.DateTime.ToString(DATE_TIME_FORMAT)))
                    {
                        gen.Index++;
                    }
                    else
                    {
                        gen.Index = 1;
                        gen.DateTime = thisTime;
                    }

                    singleId =
                        $"{prefix}-{gen.DateTime.ToString(DATE_TIME_FORMAT)}-{gen.Index.ToString(CultureInfo.InvariantCulture).PadLeft(3, '0')}";
                    return singleId;
                }

                var idGen = new IdGen
                {
                    DateTime = DateTime.Now,
                    Index = 1
                };
                _dicIdGen.Add(prefix, idGen);
                singleId =
                    $"{prefix}-{idGen.DateTime.ToString(DATE_TIME_FORMAT)}-{idGen.Index.ToString(CultureInfo.InvariantCulture).PadLeft(3, '0')}";
                return singleId;
            }
        }
    }
}
