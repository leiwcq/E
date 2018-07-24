using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Reflection;
using E.Common;
using E.Serializer.Common;
using Microsoft.Extensions.Primitives;

namespace E.Serializer
{
    public class NetStandardPclExport : PclExport
    {
        static readonly string[] allDateTimeFormats = new string[]
        {
            "yyyy-MM-ddTHH:mm:ss.FFFFFFFzzzzzz",
            "yyyy-MM-ddTHH:mm:ss.FFFFFFF",
            "yyyy-MM-ddTHH:mm:ss.FFFFFFFZ",
            "HH:mm:ss.FFFFFFF",
            "HH:mm:ss.FFFFFFFZ",
            "HH:mm:ss.FFFFFFFzzzzzz",
            "yyyy-MM-dd",
            "yyyy-MM-ddZ",
            "yyyy-MM-ddzzzzzz",
            "yyyy-MM",
            "yyyy-MMZ",
            "yyyy-MMzzzzzz",
            "yyyy",
            "yyyyZ",
            "yyyyzzzzzz",
            "--MM-dd",
            "--MM-ddZ",
            "--MM-ddzzzzzz",
            "---dd",
            "---ddZ",
            "---ddzzzzzz",
            "--MM--",
            "--MM--Z",
            "--MM--zzzzzz",
        };

        public override GetMemberDelegate CreateGetter(PropertyInfo propertyInfo)
        {
            return
                E.Common.PclExport.Instance.SupportsEmit
                    ? PropertyInvoker.GetEmit(propertyInfo)
                    : E.Common.PclExport.Instance.SupportsExpression
                        ? PropertyInvoker.GetExpression(propertyInfo)
                        : base.CreateGetter(propertyInfo);
        }

        public override GetMemberDelegate<T> CreateGetter<T>(PropertyInfo propertyInfo)
        {
            return
                E.Common.PclExport.Instance.SupportsEmit
                    ? PropertyInvoker.GetEmit<T>(propertyInfo)
                    : E.Common.PclExport.Instance.SupportsExpression
                        ? PropertyInvoker.GetExpression<T>(propertyInfo)
                        : base.CreateGetter<T>(propertyInfo);
        }

        public override SetMemberDelegate CreateSetter(PropertyInfo propertyInfo)
        {
            return
                E.Common.PclExport.Instance.SupportsEmit
                    ? PropertyInvoker.SetEmit(propertyInfo)
                    : E.Common.PclExport.Instance.SupportsExpression
                        ? PropertyInvoker.SetExpression(propertyInfo)
                        : base.CreateSetter(propertyInfo);
        }

        public override SetMemberDelegate<T> CreateSetter<T>(PropertyInfo propertyInfo)
        {
            return E.Common.PclExport.Instance.SupportsExpression
                ? PropertyInvoker.SetExpression<T>(propertyInfo)
                : base.CreateSetter<T>(propertyInfo);
        }

        public override GetMemberDelegate CreateGetter(FieldInfo fieldInfo)
        {
            return
                E.Common.PclExport.Instance.SupportsEmit
                    ? FieldInvoker.GetEmit(fieldInfo)
                    : E.Common.PclExport.Instance.SupportsExpression
                        ? FieldInvoker.GetExpression(fieldInfo)
                        : base.CreateGetter(fieldInfo);
        }

        public override GetMemberDelegate<T> CreateGetter<T>(FieldInfo fieldInfo)
        {
            return
                E.Common.PclExport.Instance.SupportsEmit
                    ? FieldInvoker.GetEmit<T>(fieldInfo)
                    : E.Common.PclExport.Instance.SupportsExpression
                        ? FieldInvoker.GetExpression<T>(fieldInfo)
                        : base.CreateGetter<T>(fieldInfo);
        }

        public override SetMemberDelegate CreateSetter(FieldInfo fieldInfo)
        {
            return
                E.Common.PclExport.Instance.SupportsEmit
                    ? FieldInvoker.SetEmit(fieldInfo)
                    : E.Common.PclExport.Instance.SupportsExpression
                        ? FieldInvoker.SetExpression(fieldInfo)
                        : base.CreateSetter(fieldInfo);
        }

        public override SetMemberDelegate<T> CreateSetter<T>(FieldInfo fieldInfo)
        {
            return E.Common.PclExport.Instance.SupportsExpression
                ? FieldInvoker.SetExpression<T>(fieldInfo)
                : base.CreateSetter<T>(fieldInfo);
        }

        public override DateTime ParseXsdDateTimeAsUtc(string dateTimeStr)
        {
            return DateTime.ParseExact(dateTimeStr, allDateTimeFormats, DateTimeFormatInfo.InvariantInfo,
                    DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite |
                    DateTimeStyles.AdjustToUniversal)
                .Prepare(parsedAsUtc: true);
        }

        //public override DateTime ToStableUniversalTime(DateTime dateTime)
        //{
        //    // .Net 2.0 - 3.5 has an issue with DateTime.ToUniversalTime, but works ok with TimeZoneInfo.ConvertTimeToUtc.
        //    // .Net 4.0+ does this under the hood anyway.
        //    return TimeZoneInfo.ConvertTimeToUtc(dateTime);
        //}

        public override ParseStringDelegate GetSpecializedCollectionParseMethod<TSerializer>(Type type)
        {
            if (type == typeof(StringCollection))
            {
                return v => ParseStringCollection<TSerializer>(new StringSegment(v));
            }
            return null;
        }

        public override ParseStringSegmentDelegate GetSpecializedCollectionParseStringSegmentMethod<TSerializer>(
            Type type)
        {
            if (type == typeof(StringCollection))
            {
                return ParseStringCollection<TSerializer>;
            }
            return null;
        }

        private static StringCollection ParseStringCollection<TSerializer>(StringSegment value)
            where TSerializer : ITypeSerializer
        {
            if (!(value = DeserializeListWithElements<TSerializer>.StripList(value)).HasValue) return null;

            var result = new StringCollection();

            if (value.Length > 0)
            {
                foreach (var item in DeserializeListWithElements<TSerializer>.ParseStringList(value))
                {
                    result.Add(item);
                }
            }

            return result;
        }

        public override ParseStringDelegate GetJsReaderParseMethod<TSerializer>(Type type)
        {
            if (type.IsAssignableFrom(typeof(System.Dynamic.IDynamicMetaObjectProvider)) ||
                type.HasInterface(typeof(System.Dynamic.IDynamicMetaObjectProvider)))
            {
                return DeserializeDynamic<TSerializer>.Parse;
            }

            return null;
        }

        public override ParseStringSegmentDelegate GetJsReaderParseStringSegmentMethod<TSerializer>(Type type)
        {
            if (type.IsAssignableFrom(typeof(System.Dynamic.IDynamicMetaObjectProvider)) ||
                type.HasInterface(typeof(System.Dynamic.IDynamicMetaObjectProvider)))
            {
                return DeserializeDynamic<TSerializer>.ParseStringSegment;
            }

            return null;
        }

        public override Type UseType(Type type)
        {
            if (type.IsInterface || type.IsAbstract)
            {
                return DynamicProxy.GetInstanceFor(type).GetType();
            }
            return type;
        }


        /*
        public static void InitForAot()
        {
        }

        internal class Poco
        {
            public string Dummy { get; set; }
        }

        public override void RegisterForAot()
        {
            RegisterTypeForAot<Poco>();

            RegisterElement<Poco, string>();

            RegisterElement<Poco, bool>();
            RegisterElement<Poco, char>();
            RegisterElement<Poco, byte>();
            RegisterElement<Poco, sbyte>();
            RegisterElement<Poco, short>();
            RegisterElement<Poco, ushort>();
            RegisterElement<Poco, int>();
            RegisterElement<Poco, uint>();

            RegisterElement<Poco, long>();
            RegisterElement<Poco, ulong>();
            RegisterElement<Poco, float>();
            RegisterElement<Poco, double>();
            RegisterElement<Poco, decimal>();

            RegisterElement<Poco, bool?>();
            RegisterElement<Poco, char?>();
            RegisterElement<Poco, byte?>();
            RegisterElement<Poco, sbyte?>();
            RegisterElement<Poco, short?>();
            RegisterElement<Poco, ushort?>();
            RegisterElement<Poco, int?>();
            RegisterElement<Poco, uint?>();
            RegisterElement<Poco, long?>();
            RegisterElement<Poco, ulong?>();
            RegisterElement<Poco, float?>();
            RegisterElement<Poco, double?>();
            RegisterElement<Poco, decimal?>();

            //RegisterElement<Poco, JsonValue>();

            RegisterTypeForAot<DayOfWeek>(); // used by DateTime

            // register built in structs
            RegisterTypeForAot<Guid>();
            RegisterTypeForAot<TimeSpan>();
            RegisterTypeForAot<DateTime>();
            RegisterTypeForAot<DateTimeOffset>();

            RegisterTypeForAot<Guid?>();
            RegisterTypeForAot<TimeSpan?>();
            RegisterTypeForAot<DateTime?>();
            RegisterTypeForAot<DateTimeOffset?>();
        }

        public static void RegisterTypeForAot<T>()
        {
            AotConfig.RegisterSerializers<T>();
        }

        public static void RegisterQueryStringWriter()
        {
            var i = 0;
            if (QueryStringWriter<Poco>.WriteFn() != null) i++;
        }

        public static int RegisterElement<T, TElement>()
        {
            var i = 0;
            i += AotConfig.RegisterSerializers<TElement>();
            AotConfig.RegisterElement<T, TElement, JsonTypeSerializer>();
            AotConfig.RegisterElement<T, TElement, Jsv.JsvTypeSerializer>();
            return i;
        }

        internal class AotConfig
        {
            internal static JsReader<JsonTypeSerializer> jsonReader;
            internal static JsWriter<JsonTypeSerializer> jsonWriter;
            internal static JsReader<Jsv.JsvTypeSerializer> jsvReader;
            internal static JsWriter<Jsv.JsvTypeSerializer> jsvWriter;
            internal static JsonTypeSerializer jsonSerializer;
            internal static Jsv.JsvTypeSerializer jsvSerializer;

            static AotConfig()
            {
                jsonSerializer = new JsonTypeSerializer();
                jsvSerializer = new Jsv.JsvTypeSerializer();
                jsonReader = new JsReader<JsonTypeSerializer>();
                jsonWriter = new JsWriter<JsonTypeSerializer>();
                jsvReader = new JsReader<Jsv.JsvTypeSerializer>();
                jsvWriter = new JsWriter<Jsv.JsvTypeSerializer>();
            }

            internal static int RegisterSerializers<T>()
            {
                var i = 0;
                i += Register<T, JsonTypeSerializer>();
                if (jsonSerializer.GetParseFn<T>() != null) i++;
                if (jsonSerializer.GetWriteFn<T>() != null) i++;
                if (jsonReader.GetParseFn<T>() != null) i++;
                if (jsonWriter.GetWriteFn<T>() != null) i++;

                i += Register<T, Jsv.JsvTypeSerializer>();
                if (jsvSerializer.GetParseFn<T>() != null) i++;
                if (jsvSerializer.GetWriteFn<T>() != null) i++;
                if (jsvReader.GetParseFn<T>() != null) i++;
                if (jsvWriter.GetWriteFn<T>() != null) i++;

                //RegisterCsvSerializer<T>();
                RegisterQueryStringWriter();
                return i;
            }

            internal static void RegisterCsvSerializer<T>()
            {
                CsvSerializer<T>.WriteFn();
                CsvSerializer<T>.WriteObject(null, null);
                CsvWriter<T>.Write(null, default(IEnumerable<T>));
                CsvWriter<T>.WriteRow(null, default(T));
            }

            public static ParseStringDelegate GetParseFn(Type type)
            {
                var parseFn = JsonTypeSerializer.Instance.GetParseFn(type);
                return parseFn;
            }

            internal static int Register<T, TSerializer>() where TSerializer : ITypeSerializer
            {
                var i = 0;

                if (JsonWriter<T>.WriteFn() != null) i++;
                if (JsonWriter.Instance.GetWriteFn<T>() != null) i++;
                if (JsonReader.Instance.GetParseFn<T>() != null) i++;
                if (JsonReader<T>.Parse(null) != null) i++;
                if (JsonReader<T>.GetParseFn() != null) i++;
                //if (JsWriter.GetTypeSerializer<JsonTypeSerializer>().GetWriteFn<T>() != null) i++;
                if (new List<T>() != null) i++;
                if (new T[0] != null) i++;

                JsConfig<T>.ExcludeTypeInfo = false;

                if (JsConfig<T>.OnDeserializedFn != null) i++;
                if (JsConfig<T>.HasDeserializeFn) i++;
                if (JsConfig<T>.SerializeFn != null) i++;
                if (JsConfig<T>.DeSerializeFn != null) i++;
                //JsConfig<T>.SerializeFn = arg => "";
                //JsConfig<T>.DeSerializeFn = arg => default(T);
                if (TypeConfig<T>.Properties != null) i++;

                WriteListsOfElements<T, TSerializer>.WriteList(null, null);
                WriteListsOfElements<T, TSerializer>.WriteIList(null, null);
                WriteListsOfElements<T, TSerializer>.WriteEnumerable(null, null);
                WriteListsOfElements<T, TSerializer>.WriteListValueType(null, null);
                WriteListsOfElements<T, TSerializer>.WriteIListValueType(null, null);
                WriteListsOfElements<T, TSerializer>.WriteGenericArrayValueType(null, null);
                WriteListsOfElements<T, TSerializer>.WriteArray(null, null);

                TranslateListWithElements<T>.LateBoundTranslateToGenericICollection(null, null);
                TranslateListWithConvertibleElements<T, T>.LateBoundTranslateToGenericICollection(null, null);

                QueryStringWriter<T>.WriteObject(null, null);
                return i;
            }

            internal static void RegisterElement<T, TElement, TSerializer>() where TSerializer : ITypeSerializer
            {
                DeserializeDictionary<TSerializer>.ParseDictionary<T, TElement>(null, null, null, null);
                DeserializeDictionary<TSerializer>.ParseDictionary<TElement, T>(null, null, null, null);

                ToStringDictionaryMethods<T, TElement, TSerializer>.WriteIDictionary(null, null, null, null);
                ToStringDictionaryMethods<TElement, T, TSerializer>.WriteIDictionary(null, null, null, null);

                // Include List deserialisations from the Register<> method above.  This solves issue where List<Guid> properties on responses deserialise to null.
                // No idea why this is happening because there is no visible exception raised.  Suspect IOS is swallowing an AOT exception somewhere.
                DeserializeArrayWithElements<TElement, TSerializer>.ParseGenericArray(null, null);
                DeserializeListWithElements<TElement, TSerializer>.ParseGenericList(null, null, null);

                // Cannot use the line below for some unknown reason - when trying to compile to run on device, mtouch bombs during native code compile.
                // Something about this line or its inner workings is offensive to mtouch. Luckily this was not needed for my List<Guide> issue.
                // DeserializeCollection<JsonTypeSerializer>.ParseCollection<TElement>(null, null, null);

                TranslateListWithElements<TElement>.LateBoundTranslateToGenericICollection(null, typeof(List<TElement>));
                TranslateListWithConvertibleElements<TElement, TElement>.LateBoundTranslateToGenericICollection(null, typeof(List<TElement>));
            }*/
    }
}
