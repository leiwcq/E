using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using E.Common;
using E.Serializer.Common;
using E.Serializer.Json;

namespace E.Serializer
{
    public static class JsonExtensions
    {
        public static T JsonTo<T>(this Dictionary<string, string> map, string key)
        {
            return Get<T>(map, key);
        }

        /// <summary>
        /// Get JSON string value converted to T
        /// </summary>
        public static T Get<T>(this Dictionary<string, string> map, string key, T defaultValue = default(T))
        {
            return map.TryGetValue(key, out string strVal) ? JsonSerializer.DeserializeFromString<T>(strVal) : defaultValue;
        }

        public static T[] GetArray<T>(this Dictionary<string, string> map, string key)
        {
            return map.TryGetValue(key, out string value)
                ? (map is JsonObject obj ? value.FromJson<T[]>() : value.FromJsv<T[]>())
                : TypeConstants<T>.EmptyArray;
        }

        /// <summary>
        /// Get JSON string value
        /// </summary>
        public static string Get(this Dictionary<string, string> map, string key)
        {
            return map.TryGetValue(key, out string strVal) ? JsonTypeSerializer.Instance.UnescapeString(strVal) : null;
        }

        public static JsonArrayObjects ArrayObjects(this string json)
        {
            return JsonArrayObjects.Parse(json);
        }

        public static List<T> ConvertAll<T>(this JsonArrayObjects jsonArrayObjects, Func<JsonObject, T> converter)
        {
            var results = new List<T>();

            foreach (var jsonObject in jsonArrayObjects)
            {
                results.Add(converter(jsonObject));
            }

            return results;
        }

        public static T ConvertTo<T>(this JsonObject jsonObject, Func<JsonObject, T> converFn)
        {
            return jsonObject == null
                ? default(T)
                : converFn(jsonObject);
        }

        public static Dictionary<string, string> ToDictionary(this JsonObject jsonObject)
        {
            return jsonObject == null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string>(jsonObject);
        }
    }

    public class JsonObject : Dictionary<string, string>
    {
        /// <summary>
        /// Get JSON string value
        /// </summary>
        public new string this[string key]
        {
            get => this.Get(key);
            set => base[key] = value;
        }

        public static JsonObject Parse(string json)
        {
            return JsonSerializer.DeserializeFromString<JsonObject>(json);
        }

        public static JsonArrayObjects ParseArray(string json)
        {
            return JsonArrayObjects.Parse(json);
        }

        public JsonArrayObjects ArrayObjects(string propertyName)
        {
            return TryGetValue(propertyName, out string strValue)
                ? JsonArrayObjects.Parse(strValue)
                : null;
        }

        public JsonObject Object(string propertyName)
        {
            return TryGetValue(propertyName, out string strValue)
                ? Parse(strValue)
                : null;
        }

        /// <summary>
        /// Get unescaped string value
        /// </summary>
        public string GetUnescaped(string key)
        {
            return base[key];
        }

        /// <summary>
        /// Get unescaped string value
        /// </summary>
        public string Child(string key)
        {
            return base[key];
        }

        /// <summary>
        /// Write JSON Array, Object, bool or number values as raw string
        /// </summary>
        public static void WriteValue(TextWriter writer, object value)
        {
            var strValue = value as string;
            if (!string.IsNullOrEmpty(strValue))
            {
                var firstChar = strValue[0];
                var lastChar = strValue[strValue.Length - 1];
                if ((firstChar == JsWriter.MapStartChar && lastChar == JsWriter.MapEndChar)
                    || (firstChar == JsWriter.ListStartChar && lastChar == JsWriter.ListEndChar)
                    || JsonUtils.TRUE == strValue
                    || JsonUtils.FALSE == strValue
                    || IsJavaScriptNumber(strValue))
                {
                    writer.Write(strValue);
                    return;
                }
            }
            JsonUtils.WriteString(writer, strValue);
        }

        private static bool IsJavaScriptNumber(string strValue)
        {
            var firstChar = strValue[0];
            if (firstChar == '0')
            {
                if (strValue.Length == 1)
                    return true;
                if (!strValue.Contains("."))
                    return false;
            }

            if (!strValue.Contains("."))
            {
                if (long.TryParse(strValue, out long longValue))
                {
                    return longValue < JsonUtils.MAX_INTEGER && longValue > JsonUtils.MIN_INTEGER;
                }
                return false;
            }

            if (double.TryParse(strValue, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out double doubleValue))
            {
                return doubleValue < JsonUtils.MAX_INTEGER && doubleValue > JsonUtils.MIN_INTEGER;
            }
            return false;
        }

        public T ConvertTo<T>()
        {
            return (T)ConvertTo(typeof(T));
        }

        public object ConvertTo(Type type)
        {
            var map = new Dictionary<string, object>();

            foreach (var entry in this)
            {
                map[entry.Key] = entry.Value;
            }

            return map.FromObjectDictionary(type);
        }
    }

    public class JsonArrayObjects : List<JsonObject>
    {
        public static JsonArrayObjects Parse(string json)
        {
            return JsonSerializer.DeserializeFromString<JsonArrayObjects>(json);
        }
    }

    public interface IValueWriter
    {
        void WriteTo(ITypeSerializer serializer, TextWriter writer);
    }

    public struct JsonValue : IValueWriter
    {
        private readonly string json;

        public JsonValue(string json)
        {
            this.json = json;
        }

        public T As<T>() => JsonSerializer.DeserializeFromString<T>(json);

        public override string ToString() => json;

        public void WriteTo(ITypeSerializer serializer, TextWriter writer) => writer.Write(json ?? JsonUtils.NULL);
    }
}