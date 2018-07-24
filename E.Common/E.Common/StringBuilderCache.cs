using System;
using System.Text;

namespace E.Common
{
    /// <summary>
    /// Reusable StringBuilder ThreadStatic Cache
    /// </summary>
    public static class StringBuilderCache
    {
        [ThreadStatic]
        static StringBuilder _cache;

        public static StringBuilder Allocate()
        {
            var ret = _cache;
            if (ret == null)
                return new StringBuilder();

            ret.Length = 0;
            _cache = null;  //don't re-issue cached instance until it's freed
            return ret;
        }

        public static void Free(StringBuilder sb)
        {
            _cache = sb;
        }

        public static string ReturnAndFree(StringBuilder sb)
        {
            var ret = sb.ToString();
            _cache = sb;
            return ret;
        }
    }

    /// <summary>
    /// Alternative Reusable StringBuilder ThreadStatic Cache
    /// </summary>
    public static class StringBuilderCacheAlt
    {
        [ThreadStatic]
        static StringBuilder _cache;

        public static StringBuilder Allocate()
        {
            var ret = _cache;
            if (ret == null)
                return new StringBuilder();

            ret.Length = 0;
            _cache = null;  //don't re-issue cached instance until it's freed
            return ret;
        }

        public static void Free(StringBuilder sb)
        {
            _cache = sb;
        }

        public static string ReturnAndFree(StringBuilder sb)
        {
            var ret = sb.ToString();
            _cache = sb;
            return ret;
        }
    }

    //Use separate cache internally to avoid reallocations and cache misses
    public static class StringBuilderThreadStatic
    {
        [ThreadStatic]
        static StringBuilder _cache;

        public static StringBuilder Allocate()
        {
            var ret = _cache;
            if (ret == null)
                return new StringBuilder();

            ret.Length = 0;
            _cache = null;  //don't re-issue cached instance until it's freed
            return ret;
        }

        public static void Free(StringBuilder sb)
        {
            _cache = sb;
        }

        public static string ReturnAndFree(StringBuilder sb)
        {
            var ret = sb.ToString();
            _cache = sb;
            return ret;
        }
    }
}