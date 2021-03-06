using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using E.Common;
using E.Serializer.Common;
using Microsoft.Extensions.Primitives;

namespace E.Serializer.Json
{
    public static class JsonReader
    {
        public static readonly JsReader<JsonTypeSerializer> Instance = new JsReader<JsonTypeSerializer>();

        private static Dictionary<Type, ParseFactoryDelegate> ParseFnCache = new Dictionary<Type, ParseFactoryDelegate>();

        internal static ParseStringDelegate GetParseFn(Type type) => v => GetParseStringSegmentFn(type)(new StringSegment(v));

        internal static ParseStringSegmentDelegate GetParseStringSegmentFn(Type type)
        {
            ParseFnCache.TryGetValue(type, out var parseFactoryFn);

            if (parseFactoryFn != null)
                return parseFactoryFn();

            var genericType = typeof(JsonReader<>).MakeGenericType(type);
            var mi = genericType.GetStaticMethod("GetParseStringSegmentFn");    
            parseFactoryFn = (ParseFactoryDelegate)mi.MakeDelegate(typeof(ParseFactoryDelegate));

            Dictionary<Type, ParseFactoryDelegate> snapshot, newCache;
            do
            {
                snapshot = ParseFnCache;
                newCache = new Dictionary<Type, ParseFactoryDelegate>(ParseFnCache)
                {
                    [type] = parseFactoryFn
                };

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref ParseFnCache, newCache, snapshot), snapshot));

            return parseFactoryFn();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void InitAot<T>()
        {
            Instance.GetParseFn<T>();
            JsonReader<T>.Parse(null);
            JsonReader<T>.GetParseFn();
            JsonReader<T>.GetParseStringSegmentFn();
        }
    }

    internal static class JsonReader<T>
    {
        private static ParseStringSegmentDelegate ReadFn;

        static JsonReader()
        {
            Refresh();
        }

        public static void Refresh()
        {
            JsConfig.InitStatics();

            if (JsonReader.Instance == null)
                return;

            ReadFn = JsonReader.Instance.GetParseStringSegmentFn<T>();
        }

        public static ParseStringDelegate GetParseFn() => ReadFn != null 
            ? (ParseStringDelegate)(v => ReadFn(new StringSegment(v))) 
            : Parse;

        public static ParseStringSegmentDelegate GetParseStringSegmentFn() => ReadFn ?? Parse;

        public static object Parse(string value) => value != null 
            ? Parse(new StringSegment(value))
            : null;

        public static object Parse(StringSegment value)
        {
            TypeConfig<T>.Init();

            if (ReadFn == null)
            {
                if (typeof(T).IsAbstract || typeof(T).IsInterface)
                {
                    if (value.IsNullOrEmpty()) return null;
                    var concreteType = DeserializeType<JsonTypeSerializer>.ExtractType(value);
                    if (concreteType != null)
                    {
                        return JsonReader.GetParseStringSegmentFn(concreteType)(value);
                    }
                    throw new NotSupportedException("Can not deserialize interface type: "
                        + typeof(T).Name);
                }

                Refresh();
            }

            return value.HasValue
                    ? ReadFn(value)
                    : null;
        }
    }
}