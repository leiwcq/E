using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace E.Serializer
{
    public static class JsonSerializer
    {
        public static string Serialize<T>(this T t) where T : class
        {
            return t.ToJson();
        }

        public static T Deserialize<T>(this string value) where T : class
        {
            return value.FromJson<T>();
        }

        private static JsonSerializerSettings _jsonSettings;

        private static JsonSerializerSettings JsonSettings =>
            _jsonSettings ?? (_jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                DefaultValueHandling = DefaultValueHandling.Include,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                DateFormatString = "yyyy-MM-ddTHH:mm:sszzz",
                Converters = new JsonConverter[] {new JsGuidConverter()},
                NullValueHandling = NullValueHandling.Ignore,
                ConstructorHandling = ConstructorHandling.Default,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

        private static JsonSerializerSettings _jsonSettingsFront;

        private static JsonSerializerSettings JsonSettingsFront =>
            _jsonSettingsFront ?? (_jsonSettingsFront = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                DefaultValueHandling = DefaultValueHandling.Include,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                DateFormatString = "yyyy-MM-ddTHH:mm:sszzz",
                Converters = new JsonConverter[] {new JsGuidConverter()},
                NullValueHandling = NullValueHandling.Ignore,
                ConstructorHandling = ConstructorHandling.Default,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });



        public static string ToJson<T>(this T obj)
        {
            return obj != null ? JsonConvert.SerializeObject(obj, JsonSettings) : string.Empty;
        }

        public static string ToJson<T>(this T obj, Formatting formatting)
        {
            return obj != null ? JsonConvert.SerializeObject(obj, formatting, JsonSettings) : string.Empty;
        }

        public static string ToJson(this object obj, Type type)
        {
            return obj != null ? JsonConvert.SerializeObject(obj, type, JsonSettings) : string.Empty;
        }

        public static string ToJsonWithFront<T>(this T obj)
        {
            return obj != null ? JsonConvert.SerializeObject(obj, JsonSettingsFront) : string.Empty;
        }

        public static T FromJson<T>(this string json)
        {
            return string.IsNullOrEmpty(json) ? default(T) : JsonConvert.DeserializeObject<T>(json, JsonSettings);
        }

        public static object FromJson(this string json, Type type)
        {
            return string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject(json, type, JsonSettings);
        }


        public static Encoding Utf8Encoding = E.Common.PclExport.Instance.GetUTF8Encoding();

        public static T DeserializeFromString<T>(string value)
        {
            if (string.IsNullOrEmpty(value)) return default(T);
            return value.FromJson<T>();
        }

        public static T DeserializeFromReader<T>(TextReader reader)
        {
            return DeserializeFromString<T>(reader.ReadToEnd());
        }

        public static object DeserializeFromString(string value, Type type)
        {
            return string.IsNullOrEmpty(value)
                ? null
                : value.FromJson(type);
        }

        public static object DeserializeFromReader(TextReader reader, Type type)
        {
            return DeserializeFromString(reader.ReadToEnd(), type);
        }

        public static string SerializeToString<T>(T value)
        {
            if (value == null || value is Delegate) return null;
            return value.ToJson();
        }

        public static string SerializeToString(object value, Type type)
        {
            return value?.ToJson(type);
        }

        public static void SerializeToWriter<T>(T value, TextWriter writer)
        {
            if (value == null) return;

            var json = value.ToJson();
            writer.Write(json);
        }

        public static void SerializeToWriter(object value, Type type, TextWriter writer)
        {
            if (value == null) return;

            if (type == typeof(string))
            {
                writer.Write(value as string);
                return;
            }

            var json = value.ToJson();
            writer.Write(json);
        }

        public static void SerializeToStream<T>(T value, Stream stream)
        {
            if (value == null) return;
            if (typeof(T) == typeof(object))
            {
                SerializeToStream(value, value.GetType(), stream);
            }
            else if (typeof(T).IsAbstract || typeof(T).IsInterface)
            {
                SerializeToStream(value, value.GetType(), stream);
            }
            else
            {
                var writer = new StreamWriter(stream, Utf8Encoding);
                var json = value.ToJson();
                writer.Write(json);
                writer.Flush();
            }
        }

        public static void SerializeToStream(object value, Type type, Stream stream)
        {
            var writer = new StreamWriter(stream, Utf8Encoding);
            var json = value.ToJson(type);
            writer.Write(json);
            writer.Flush();
        }

        public static T DeserializeFromStream<T>(Stream stream)
        {
            using (var reader = new StreamReader(stream, Utf8Encoding))
            {
                return DeserializeFromString<T>(reader.ReadToEnd());
            }
        }

        public static object DeserializeFromStream(Type type, Stream stream)
        {
            using (var reader = new StreamReader(stream, Utf8Encoding))
            {
                return DeserializeFromString(reader.ReadToEnd(), type);
            }
        }

        public static T DeserializeResponse<T>(WebRequest webRequest)
        {
            using (var webRes = E.Common.PclExport.Instance.GetResponse(webRequest))
            {
                using (var stream = webRes.GetResponseStream())
                {
                    return DeserializeFromStream<T>(stream);
                }
            }
        }

        public static object DeserializeResponse<T>(Type type, WebRequest webRequest)
        {
            using (var webRes = E.Common.PclExport.Instance.GetResponse(webRequest))
            {
                using (var stream = webRes.GetResponseStream())
                {
                    return DeserializeFromStream(type, stream);
                }
            }
        }

        public static T DeserializeRequest<T>(WebRequest webRequest)
        {
            using (var webRes = E.Common.PclExport.Instance.GetResponse(webRequest))
            {
                return DeserializeResponse<T>(webRes);
            }
        }

        public static object DeserializeRequest(Type type, WebRequest webRequest)
        {
            using (var webRes = E.Common.PclExport.Instance.GetResponse(webRequest))
            {
                return DeserializeResponse(type, webRes);
            }
        }

        public static T DeserializeResponse<T>(WebResponse webResponse)
        {
            using (var stream = webResponse.GetResponseStream())
            {
                return DeserializeFromStream<T>(stream);
            }
        }

        public static object DeserializeResponse(Type type, WebResponse webResponse)
        {
            using (var stream = webResponse.GetResponseStream())
            {
                return DeserializeFromStream(type, stream);
            }
        }
    }

    public class JsGuidConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            try
            {
                var type = value.GetType();
                if (typeof(Guid) == type)
                {
                    var item = (Guid) value;
                    writer.WriteValue($"{item:N}");
                    writer.Flush();
                }
                else if (typeof(Guid?) == type)
                {
                    var item = (Guid?) value;
                    writer.WriteValue($"{item.Value:N}");
                    writer.Flush();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            Newtonsoft.Json.JsonSerializer serializer)
        {

            if (reader.TokenType == JsonToken.Null)
            {
                return Guid.Empty;
            }
            if (!(typeof(Guid) == objectType || typeof(Guid?) == objectType))
                return Guid.Empty;
            try
            {
                var boolText = reader.Value.ToString();
                if (string.IsNullOrWhiteSpace(boolText))
                {
                    return Guid.Empty;
                }
                return Guid.TryParse(boolText, out Guid result) ? result : Guid.Empty;
            }
            catch (Exception)
            {
                throw new Exception($"Error converting value {reader.Value} to type '{objectType}'");
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Guid);
        }
    }
}
