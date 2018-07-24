using System;
using System.Globalization;
using System.IO;

namespace E.Serializer
{
    /// <summary>
    /// Reusable StringWriter ThreadStatic Cache
    /// </summary>
    public static class StringWriterCache
    {
        [ThreadStatic]
        static StringWriter _cache;

        public static StringWriter Allocate()
        {
            var ret = _cache;
            if (ret == null)
                return new StringWriter(CultureInfo.InvariantCulture);

            var sb = ret.GetStringBuilder();
            sb.Length = 0;
            _cache = null;  //don't re-issue cached instance until it's freed
            return ret;
        }

        public static void Free(StringWriter writer)
        {
            _cache = writer;
        }

        public static string ReturnAndFree(StringWriter writer)
        {
            var ret = writer.ToString();
            _cache = writer;
            return ret;
        }
    }

    /// <summary>
    /// Alternative Reusable StringWriter ThreadStatic Cache
    /// </summary>
    public static class StringWriterCacheAlt
    {
        [ThreadStatic]
        static StringWriter _cache;

        public static StringWriter Allocate()
        {
            var ret = _cache;
            if (ret == null)
                return new StringWriter(CultureInfo.InvariantCulture);

            var sb = ret.GetStringBuilder();
            sb.Length = 0;
            _cache = null;  //don't re-issue cached instance until it's freed
            return ret;
        }

        public static void Free(StringWriter writer)
        {
            _cache = writer;
        }

        public static string ReturnAndFree(StringWriter writer)
        {
            var ret = writer.ToString();
            _cache = writer;
            return ret;
        }
    }

    //Use separate cache internally to avoid reallocations and cache misses
    internal static class StringWriterThreadStatic
    {
        [ThreadStatic]
        static StringWriter _cache;

        public static StringWriter Allocate()
        {
            var ret = _cache;
            if (ret == null)
                return new StringWriter(CultureInfo.InvariantCulture);

            var sb = ret.GetStringBuilder();
            sb.Length = 0;
            _cache = null;  //don't re-issue cached instance until it's freed
            return ret;
        }

        public static void Free(StringWriter writer)
        {
            _cache = writer;
        }

        public static string ReturnAndFree(StringWriter writer)
        {
            var ret = writer.ToString();
            _cache = writer;
            return ret;
        }
    }
}