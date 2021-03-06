using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using E.Common;
using E.Serializer.Common;

namespace E.Serializer.Jsv
{
    internal class JsvSerializer<T>
    {
        private Dictionary<Type, ParseStringDelegate> _deserializerCache = new Dictionary<Type, ParseStringDelegate>();

        public T DeserializeFromString(string value, Type type)
        {
            if (_deserializerCache.TryGetValue(type, out var parseFn)) return (T)parseFn(value);

            var genericType = typeof(T).MakeGenericType(type);
            var mi = genericType.GetMethodInfo("DeserializeFromString", new[] { typeof(string) });
            parseFn = (ParseStringDelegate)mi.MakeDelegate(typeof(ParseStringDelegate));

            Dictionary<Type, ParseStringDelegate> snapshot, newCache;
            do
            {
                snapshot = _deserializerCache;
                newCache = new Dictionary<Type, ParseStringDelegate>(_deserializerCache)
                {
                    [type] = parseFn
                };

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref _deserializerCache, newCache, snapshot), snapshot));

            return (T)parseFn(value);
        }

        public T DeserializeFromString(string value)
        {
            if (typeof(T) == typeof(string)) return (T)(object)value;

            return (T)JsvReader<T>.Parse(value);
        }

        public void SerializeToWriter(T value, TextWriter writer)
        {
            JsvWriter<T>.WriteObject(writer, value);
        }

        public string SerializeToString(T value)
        {
            if (value == null) return null;
            if (value is string) return value as string;

            var writer = StringWriterThreadStatic.Allocate();
            JsvWriter<T>.WriteObject(writer, value);
            return StringWriterThreadStatic.ReturnAndFree(writer);
        }
    }
}