using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using E.Common;
using E.Serializer.Json;
using E.Serializer.Jsv;

namespace E.Serializer.Common
{
    public static class
        JsConfig
    {
        static JsConfig()
        {
            Reset();
        }

        // force deterministic initialization of static constructor
        public static void InitStatics() { }

        public static JsConfigScope BeginScope()
        {
            return new JsConfigScope();
        }

        public static JsConfigScope CreateScope(string config, JsConfigScope scope = null)
        {
            if (string.IsNullOrEmpty(config))
                return scope;

            if (scope == null)
                scope = BeginScope();

            var items = config.Split(',');
            foreach (var item in items)
            {
                var parts = item.SplitOnFirst(':');
                var key = parts[0].ToLower();
                var value = parts.Length == 2 ? parts[1].ToLower() : null;
                var boolValue = parts.Length == 1 || (value != "false" && value != "0");

                switch (key)
                {
                    case "cotisd":
                    case "convertobjecttypesintostringdictionary":
                        scope.ConvertObjectTypesIntoStringDictionary = boolValue;
                        break;
                    case "ttpptv":
                    case "trytoparseprimitivetypevalues":
                        scope.TryToParsePrimitiveTypeValues = boolValue;
                        break;
                    case "ttpnt":
                    case "trytoparsenumerictype":
                        scope.TryToParseNumericType = boolValue;
                        break;
                    case "edv":
                    case "excludedefaultvalues":
                        scope.ExcludeDefaultValues = boolValue;
                        break;
                    case "inv":
                    case "includenullvalues":
                        scope.IncludeNullValues = boolValue;
                        break;
                    case "invid":
                    case "includenullvaluesindictionaries":
                        scope.IncludeNullValuesInDictionaries = boolValue;
                        break;
                    case "ide":
                    case "includedefaultenums":
                        scope.IncludeDefaultEnums = boolValue;
                        break;
                    case "eti":
                    case "excludetypeinfo":
                        scope.ExcludeTypeInfo = boolValue;
                        break;
                    case "iti":
                    case "includetypeinfo":
                        scope.IncludeTypeInfo = boolValue;
                        break;
                    case "eccn":
                    case "emitcamelcasenames":
                        scope.EmitCamelCaseNames = boolValue;
                        break;
                    case "elun":
                    case "emitlowercaseunderscorenames":
                        scope.EmitLowercaseUnderscoreNames = boolValue;
                        break;
                    case "pi":
                    case "preferinterfaces":
                        scope.PreferInterfaces = boolValue;
                        break;
                    case "tode":
                    case "throwondeserializationerror":
                        scope.ThrowOnDeserializationError = boolValue;
                        break;
                    case "teai":
                    case "treatenumasinteger":
                        scope.TreatEnumAsInteger = boolValue;
                        break;
                    case "sdtc":
                    case "skipdatetimeconversion":
                        scope.SkipDateTimeConversion = boolValue;
                        break;
                    case "auu":
                    case "alwaysuseutc":
                        scope.AlwaysUseUtc = boolValue;
                        break;
                    case "au":
                    case "assumeutc":
                        scope.AssumeUtc = boolValue;
                        break;
                    case "auo":
                    case "appendutcoffset":
                        scope.AppendUtcOffset = boolValue;
                        break;
                    case "eu":
                    case "escapeunicode":
                        scope.EscapeUnicode = boolValue;
                        break;
                    case "ehc":
                    case "escapehtmlchars":
                        scope.EscapeHtmlChars = boolValue;
                        break;
                    case "ipf":
                    case "includepublicfields":
                        scope.IncludePublicFields = boolValue;
                        break;
                    case "dh":
                    case "datehandler":
                        switch (value)
                        {
                            case "timestampoffset":
                            case "to":
                                scope.DateHandler = DateHandler.TimestampOffset;
                                break;
                            case "dcjsc":
                            case "dcjscompatible":
                                scope.DateHandler = DateHandler.DcjsCompatible;
                                break;
                            case "iso8601":
                                scope.DateHandler = DateHandler.Iso8601;
                                break;
                            case "iso8601do":
                            case "iso8601dateonly":
                                scope.DateHandler = DateHandler.Iso8601DateOnly;
                                break;
                            case "iso8601dt":
                            case "iso8601datetime":
                                scope.DateHandler = DateHandler.Iso8601DateTime;
                                break;
                            case "rfc1123":
                                scope.DateHandler = DateHandler.Rfc1123;
                                break;
                            case "ut":
                            case "unixtime":
                                scope.DateHandler = DateHandler.UnixTime;
                                break;
                            case "utm":
                            case "unixtimems":
                                scope.DateHandler = DateHandler.UnixTimeMs;
                                break;
                        }
                        break;
                    case "tsh":
                    case "timespanhandler":
                        switch (value)
                        {
                            case "df":
                            case "durationformat":
                                scope.TimeSpanHandler = TimeSpanHandler.DurationFormat;
                                break;
                            case "sf":
                            case "standardformat":
                                scope.TimeSpanHandler = TimeSpanHandler.StandardFormat;
                                break;
                        }
                        break;
                    case "pc":
                    case "propertyconvention":
                        switch (value)
                        {
                            case "l":
                            case "lenient":
                                scope.PropertyConvention = PropertyConvention.Lenient;
                                break;
                            case "s":
                            case "strict":
                                scope.PropertyConvention = PropertyConvention.Strict;
                                break;
                        }
                        break;
                }
            }

            return scope;
        }

        public static JsConfigScope With(
            bool? convertObjectTypesIntoStringDictionary = null,
            bool? tryToParsePrimitiveTypeValues = null,
            bool? tryToParseNumericType = null,
            ParseAsType? parsePrimitiveFloatingPointTypes = null,
            ParseAsType? parsePrimitiveIntegerTypes = null,
            bool? excludeDefaultValues = null,
            bool? includeNullValues = null,
            bool? includeNullValuesInDictionaries = null,
            bool? includeDefaultEnums = null,
            bool? excludeTypeInfo = null,
            bool? includeTypeInfo = null,
            bool? emitCamelCaseNames = null,
            bool? emitLowercaseUnderscoreNames = null,
            DateHandler? dateHandler = null,
            TimeSpanHandler? timeSpanHandler = null,
            PropertyConvention? propertyConvention = null,
            bool? preferInterfaces = null,
            bool? throwOnDeserializationError = null,
            string typeAttr = null,
            string dateTimeFormat = null,
            Func<Type, string> typeWriter = null,
            Func<string, Type> typeFinder = null,
            bool? treatEnumAsInteger = null,
            bool? skipDateTimeConversion = null,
            bool? alwaysUseUtc = null,
            bool? assumeUtc = null,
            bool? appendUtcOffset = null,
            bool? escapeUnicode = null,
            bool? includePublicFields = null,
            int? maxDepth = null,
            EmptyCtorFactoryDelegate modelFactory = null,
            string[] excludePropertyReferences = null,
            bool? useSystemParseMethods = null)
        {
            return new JsConfigScope
            {
                ConvertObjectTypesIntoStringDictionary = convertObjectTypesIntoStringDictionary ?? _sConvertObjectTypesIntoStringDictionary,
                TryToParsePrimitiveTypeValues = tryToParsePrimitiveTypeValues ?? _sTryToParsePrimitiveTypeValues,
                TryToParseNumericType = tryToParseNumericType ?? _sTryToParseNumericType,

                ParsePrimitiveFloatingPointTypes = parsePrimitiveFloatingPointTypes ?? _sParsePrimitiveFloatingPointTypes,
                ParsePrimitiveIntegerTypes = parsePrimitiveIntegerTypes ?? _sParsePrimitiveIntegerTypes,

                ExcludeDefaultValues = excludeDefaultValues ?? _sExcludeDefaultValues,
                IncludeNullValues = includeNullValues ?? _sIncludeNullValues,
                IncludeNullValuesInDictionaries = includeNullValuesInDictionaries ?? _sIncludeNullValuesInDictionaries,
                IncludeDefaultEnums = includeDefaultEnums ?? _sIncludeDefaultEnums,
                ExcludeTypeInfo = excludeTypeInfo ?? _sExcludeTypeInfo,
                IncludeTypeInfo = includeTypeInfo ?? _sIncludeTypeInfo,
                EmitCamelCaseNames = emitCamelCaseNames ?? _sEmitCamelCaseNames,
                EmitLowercaseUnderscoreNames = emitLowercaseUnderscoreNames ?? _sEmitLowercaseUnderscoreNames,
                DateHandler = dateHandler ?? _sDateHandler,
                TimeSpanHandler = timeSpanHandler ?? _sTimeSpanHandler,
                PropertyConvention = propertyConvention ?? _sPropertyConvention,
                PreferInterfaces = preferInterfaces ?? _sPreferInterfaces,
                ThrowOnDeserializationError = throwOnDeserializationError ?? _sThrowOnDeserializationError,
                DateTimeFormat = dateTimeFormat ?? _sDateTimeFormat,
                TypeAttr = typeAttr ?? _sTypeAttr,
                TypeWriter = typeWriter ?? _sTypeWriter,
                TypeFinder = typeFinder ?? _sTypeFinder,
                TreatEnumAsInteger = treatEnumAsInteger ?? _sTreatEnumAsInteger,
                SkipDateTimeConversion = skipDateTimeConversion ?? _sSkipDateTimeConversion,
                AlwaysUseUtc = alwaysUseUtc ?? _sAlwaysUseUtc,
                AssumeUtc = assumeUtc ?? _sAssumeUtc,
                AppendUtcOffset = appendUtcOffset ?? _sAppendUtcOffset,
                EscapeUnicode = escapeUnicode ?? _sEscapeUnicode,
                IncludePublicFields = includePublicFields ?? _sIncludePublicFields,
                MaxDepth = maxDepth ?? _sMaxDepth,
                ModelFactory = modelFactory ?? ModelFactory,
                ExcludePropertyReferences = excludePropertyReferences ?? _sExcludePropertyReferences,
            };
        }

        private static string _sDateTimeFormat;
        public static string DateTimeFormat
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.DateTimeFormat : null)
                   ?? _sDateTimeFormat;
            set
            {
                if (_sDateTimeFormat == null) _sDateTimeFormat = value;
            }
        }

        private static bool? _sConvertObjectTypesIntoStringDictionary;
        public static bool ConvertObjectTypesIntoStringDictionary
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.ConvertObjectTypesIntoStringDictionary : null)
                   ?? _sConvertObjectTypesIntoStringDictionary
                   ?? false;
            set
            {
                if (!_sConvertObjectTypesIntoStringDictionary.HasValue) _sConvertObjectTypesIntoStringDictionary = value;
            }
        }

        private static bool? _sTryToParsePrimitiveTypeValues;
        public static bool TryToParsePrimitiveTypeValues
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.TryToParsePrimitiveTypeValues : null)
                   ?? _sTryToParsePrimitiveTypeValues
                   ?? false;
            set
            {
                if (!_sTryToParsePrimitiveTypeValues.HasValue) _sTryToParsePrimitiveTypeValues = value;
            }
        }

        private static bool? _sTryToParseNumericType;
        public static bool TryToParseNumericType
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.TryToParseNumericType : null)
                   ?? _sTryToParseNumericType
                   ?? false;
            set
            {
                if (!_sTryToParseNumericType.HasValue) _sTryToParseNumericType = value;
            }
        }

        private static bool? _sTryParseIntoBestFit;
        public static bool TryParseIntoBestFit
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.TryParseIntoBestFit : null)
                   ?? _sTryParseIntoBestFit
                   ?? false;
            set
            {
                if (!_sTryParseIntoBestFit.HasValue) _sTryParseIntoBestFit = value;
            }
        }

        private static ParseAsType? _sParsePrimitiveFloatingPointTypes;
        public static ParseAsType ParsePrimitiveFloatingPointTypes
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.ParsePrimitiveFloatingPointTypes : null)
                   ?? _sParsePrimitiveFloatingPointTypes
                   ?? ParseAsType.Decimal;
            set
            {
                if (_sParsePrimitiveFloatingPointTypes == null) _sParsePrimitiveFloatingPointTypes = value;
            }
        }

        private static ParseAsType? _sParsePrimitiveIntegerTypes;
        public static ParseAsType ParsePrimitiveIntegerTypes
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.ParsePrimitiveIntegerTypes : null)
                   ?? _sParsePrimitiveIntegerTypes
                   ?? ParseAsType.Byte | ParseAsType.SByte | ParseAsType.Int16 | ParseAsType.UInt16 | ParseAsType.Int32 | ParseAsType.UInt32 | ParseAsType.Int64 | ParseAsType.UInt64;
            set
            {
                if (!_sParsePrimitiveIntegerTypes.HasValue) _sParsePrimitiveIntegerTypes = value;
            }
        }

        private static bool? _sExcludeDefaultValues;
        public static bool ExcludeDefaultValues
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.ExcludeDefaultValues : null)
                   ?? _sExcludeDefaultValues
                   ?? false;
            set
            {
                if (!_sExcludeDefaultValues.HasValue) _sExcludeDefaultValues = value;
            }
        }

        private static bool? _sIncludeNullValues;
        public static bool IncludeNullValues
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.IncludeNullValues : null)
                   ?? _sIncludeNullValues
                   ?? false;
            set
            {
                if (!_sIncludeNullValues.HasValue) _sIncludeNullValues = value;
            }
        }

        private static bool? _sIncludeNullValuesInDictionaries;
        public static bool IncludeNullValuesInDictionaries
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.IncludeNullValuesInDictionaries : null)
                   ?? _sIncludeNullValuesInDictionaries
                   ?? false;
            set
            {
                if (!_sIncludeNullValuesInDictionaries.HasValue) _sIncludeNullValuesInDictionaries = value;
            }
        }

        private static bool? _sIncludeDefaultEnums;
        public static bool IncludeDefaultEnums
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.IncludeDefaultEnums : null)
                   ?? _sIncludeDefaultEnums
                   ?? true;
            set
            {
                if (!_sIncludeDefaultEnums.HasValue) _sIncludeDefaultEnums = value;
            }
        }

        private static bool? _sTreatEnumAsInteger;
        public static bool TreatEnumAsInteger
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.TreatEnumAsInteger : null)
                   ?? _sTreatEnumAsInteger
                   ?? false;
            set
            {
                if (!_sTreatEnumAsInteger.HasValue) _sTreatEnumAsInteger = value;
            }
        }

        private static bool? _sExcludeTypeInfo;
        public static bool ExcludeTypeInfo
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.ExcludeTypeInfo : null)
                   ?? _sExcludeTypeInfo
                   ?? false;
            set
            {
                if (!_sExcludeTypeInfo.HasValue) _sExcludeTypeInfo = value;
            }
        }

        private static bool? _sIncludeTypeInfo;
        public static bool IncludeTypeInfo
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.IncludeTypeInfo : null)
                   ?? _sIncludeTypeInfo
                   ?? false;
            set
            {
                if (!_sIncludeTypeInfo.HasValue) _sIncludeTypeInfo = value;
            }
        }

        private static string _sTypeAttr;
        public static string TypeAttr
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.TypeAttr : null)
                   ?? _sTypeAttr
                   ?? JsWriter.TypeAttr;
            set
            {
                if (_sTypeAttr == null) _sTypeAttr = value;
                JsonTypeAttrInObject = JsonTypeSerializer.GetTypeAttrInObject(value);
                JsvTypeAttrInObject = JsvTypeSerializer.GetTypeAttrInObject(value);
            }
        }

        private static string _sJsonTypeAttrInObject;
        private static readonly string DefaultJsonTypeAttrInObject = JsonTypeSerializer.GetTypeAttrInObject(TypeAttr);
        internal static string JsonTypeAttrInObject
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.JsonTypeAttrInObject : null)
                   ?? _sJsonTypeAttrInObject
                   ?? DefaultJsonTypeAttrInObject;
            set
            {
                if (_sJsonTypeAttrInObject == null) _sJsonTypeAttrInObject = value;
            }
        }

        private static string _sJsvTypeAttrInObject;
        private static readonly string DefaultJsvTypeAttrInObject = JsvTypeSerializer.GetTypeAttrInObject(TypeAttr);
        internal static string JsvTypeAttrInObject
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.JsvTypeAttrInObject : null)
                   ?? _sJsvTypeAttrInObject
                   ?? DefaultJsvTypeAttrInObject;
            set
            {
                if (_sJsvTypeAttrInObject == null) _sJsvTypeAttrInObject = value;
            }
        }

        private static Func<Type, string> _sTypeWriter;
        public static Func<Type, string> TypeWriter
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.TypeWriter : null)
                   ?? _sTypeWriter
                   ?? AssemblyUtils.WriteType;
            set
            {
                if (_sTypeWriter == null) _sTypeWriter = value;
            }
        }

        private static Func<string, Type> _sTypeFinder;
        public static Func<string, Type> TypeFinder
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.TypeFinder : null)
                   ?? _sTypeFinder
                   ?? AssemblyUtils.FindType;
            set
            {
                if (_sTypeFinder == null) _sTypeFinder = value;
            }
        }

        private static Func<string, object> _sParsePrimitiveFn;
        public static Func<string, object> ParsePrimitiveFn
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.ParsePrimitiveFn : null)
                   ?? _sParsePrimitiveFn
                   ?? null;
            set
            {
                if (_sParsePrimitiveFn == null) _sParsePrimitiveFn = value;
            }
        }

        private static DateHandler? _sDateHandler;
        public static DateHandler DateHandler
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.DateHandler : null)
                   ?? _sDateHandler
                   ?? DateHandler.TimestampOffset;
            set
            {
                if (!_sDateHandler.HasValue) _sDateHandler = value;
            }
        }

        /// <summary>
        /// Sets which format to use when serializing TimeSpans
        /// </summary>
        private static TimeSpanHandler? _sTimeSpanHandler;
        public static TimeSpanHandler TimeSpanHandler
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.TimeSpanHandler : null)
                   ?? _sTimeSpanHandler
                   ?? TimeSpanHandler.DurationFormat;
            set
            {
                if (!_sTimeSpanHandler.HasValue) _sTimeSpanHandler = value;
            }
        }


        /// <summary>
        /// <see langword="true"/> if the <see cref="ITypeSerializer"/> is configured
        /// to take advantage of <see cref="CLSCompliantAttribute"/> specification,
        /// to support user-friendly serialized formats, ie emitting camelCasing for JSON
        /// and parsing member names and enum values in a case-insensitive manner.
        /// </summary>
        private static bool? _sEmitCamelCaseNames;
        public static bool EmitCamelCaseNames
        {
            // obeying the use of ThreadStatic, but allowing for setting JsConfig once as is the normal case
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.EmitCamelCaseNames : null)
                   ?? _sEmitCamelCaseNames
                   ?? false;
            set
            {
                if (!_sEmitCamelCaseNames.HasValue) _sEmitCamelCaseNames = value;
            }
        }

        /// <summary>
        /// <see langword="true"/> if the <see cref="ITypeSerializer"/> is configured
        /// to support web-friendly serialized formats, ie emitting lowercase_underscore_casing for JSON
        /// </summary>
        private static bool? _sEmitLowercaseUnderscoreNames;
        public static bool EmitLowercaseUnderscoreNames
        {
            // obeying the use of ThreadStatic, but allowing for setting JsConfig once as is the normal case
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.EmitLowercaseUnderscoreNames : null)
                   ?? _sEmitLowercaseUnderscoreNames
                   ?? false;
            set
            {
                if (!_sEmitLowercaseUnderscoreNames.HasValue) _sEmitLowercaseUnderscoreNames = value;
            }
        }

        /// <summary>
        /// Define how property names are mapped during deserialization
        /// </summary>
        private static PropertyConvention? _sPropertyConvention;
        public static PropertyConvention PropertyConvention
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.PropertyConvention : null)
                   ?? _sPropertyConvention
                   ?? PropertyConvention.Strict;
            set
            {
                if (!_sPropertyConvention.HasValue) _sPropertyConvention = value;
            }
        }


        /// <summary>
        /// Gets or sets a value indicating if the framework should throw serialization exceptions
        /// or continue regardless of deserialization errors. If <see langword="true"/>  the framework
        /// will throw; otherwise, it will parse as many fields as possible. The default is <see langword="false"/>.
        /// </summary>
        private static bool? _sThrowOnDeserializationError;
        public static bool ThrowOnDeserializationError
        {
            // obeying the use of ThreadStatic, but allowing for setting JsConfig once as is the normal case
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.ThrowOnDeserializationError : null)
                   ?? _sThrowOnDeserializationError
                   ?? false;
            set
            {
                if (!_sThrowOnDeserializationError.HasValue) _sThrowOnDeserializationError = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if the framework should always convert <see cref="DateTime"/> to UTC format instead of local time. 
        /// </summary>
        private static bool? _sAlwaysUseUtc;
        public static bool AlwaysUseUtc
        {
            // obeying the use of ThreadStatic, but allowing for setting JsConfig once as is the normal case
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.AlwaysUseUtc : null)
                   ?? _sAlwaysUseUtc
                   ?? false;
            set
            {
                if (!_sAlwaysUseUtc.HasValue) _sAlwaysUseUtc = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if the framework should skip automatic <see cref="DateTime"/> conversions.
        /// Dates will be handled literally, any included timezone encoding will be lost and the date will be treaded as DateTimeKind.Local
        /// Utc formatted input will result in DateTimeKind.Utc output. Any input without TZ data will be set DateTimeKind.Unspecified
        /// This will take precedence over other flags like AlwaysUseUtc 
        /// JsConfig.DateHandler = DateHandler.ISO8601 should be used when set true for consistent de/serialization.
        /// </summary>
        private static bool? _sSkipDateTimeConversion;
        public static bool SkipDateTimeConversion
        {
            // obeying the use of ThreadStatic, but allowing for setting JsConfig once as is the normal case
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.SkipDateTimeConversion : null)
                   ?? _sSkipDateTimeConversion
                   ?? false;
            set
            {
                if (!_sSkipDateTimeConversion.HasValue) _sSkipDateTimeConversion = value;
            }
        }
        /// <summary>
        /// Gets or sets a value indicating if the framework should always assume <see cref="DateTime"/> is in UTC format if Kind is Unspecified. 
        /// </summary>
        private static bool? _sAssumeUtc;
        public static bool AssumeUtc
        {
            // obeying the use of ThreadStatic, but allowing for setting JsConfig once as is the normal case
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.AssumeUtc : null)
                   ?? _sAssumeUtc
                   ?? false;
            set
            {
                if (!_sAssumeUtc.HasValue) _sAssumeUtc = value;
            }
        }

        /// <summary>
        /// Gets or sets whether we should append the Utc offset when we serialize Utc dates. Defaults to no.
        /// Only supported for when the JsConfig.DateHandler == JsonDateHandler.TimestampOffset
        /// </summary>
        private static bool? _sAppendUtcOffset;
        public static bool? AppendUtcOffset
        {
            // obeying the use of ThreadStatic, but allowing for setting JsConfig once as is the normal case
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.AppendUtcOffset : null)
                   ?? _sAppendUtcOffset
                   ?? null;
            set
            {
                if (_sAppendUtcOffset == null) _sAppendUtcOffset = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if unicode symbols should be serialized as "\uXXXX".
        /// </summary>
        private static bool? _sEscapeUnicode;
        public static bool EscapeUnicode
        {
            // obeying the use of ThreadStatic, but allowing for setting JsConfig once as is the normal case
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.EscapeUnicode : null)
                   ?? _sEscapeUnicode
                   ?? false;
            set
            {
                if (!_sEscapeUnicode.HasValue) _sEscapeUnicode = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if HTML entity chars [&gt; &lt; &amp; = '] should be escaped as "\uXXXX".
        /// </summary>
        private static bool? _sEscapeHtmlChars;
        public static bool EscapeHtmlChars
        {
            // obeying the use of ThreadStatic, but allowing for setting JsConfig once as is the normal case
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.EscapeHtmlChars : null)
                   ?? _sEscapeHtmlChars
                   ?? false;
            set
            {
                if (!_sEscapeHtmlChars.HasValue) _sEscapeHtmlChars = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if the framework should call an error handler when
        /// an exception happens during the deserialization.
        /// </summary>
        /// <remarks>Parameters have following meaning in order: deserialized entity, property name, parsed value, property type, caught exception.</remarks>
        private static DeserializationErrorDelegate _sOnDeserializationError;
        public static DeserializationErrorDelegate OnDeserializationError
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.OnDeserializationError : null)
                   ?? _sOnDeserializationError;
            set => _sOnDeserializationError = value;
        }

        internal static HashSet<Type> HasSerializeFn = new HashSet<Type>();

        internal static HashSet<Type> HasIncludeDefaultValue = new HashSet<Type>();

        public static HashSet<Type> TreatValueAsRefTypes = new HashSet<Type>();

        private static bool? _sPreferInterfaces;
        /// <summary>
        /// If set to true, Interface types will be prefered over concrete types when serializing.
        /// </summary>
        public static bool PreferInterfaces
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.PreferInterfaces : null)
                   ?? _sPreferInterfaces
                   ?? false;
            set
            {
                if (!_sPreferInterfaces.HasValue) _sPreferInterfaces = value;
            }
        }

        internal static bool TreatAsRefType(Type valueType)
        {
            return TreatValueAsRefTypes.Contains(valueType.IsGenericType ? valueType.GetGenericTypeDefinition() : valueType);
        }


        /// <summary>
        /// If set to true, Interface types will be prefered over concrete types when serializing.
        /// </summary>
        private static bool? _sIncludePublicFields;
        public static bool IncludePublicFields
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.IncludePublicFields : null)
                   ?? _sIncludePublicFields
                   ?? false;
            set
            {
                if (!_sIncludePublicFields.HasValue) _sIncludePublicFields = value;
            }
        }

        /// <summary>
        /// Sets the maximum depth to avoid circular dependencies
        /// </summary>
        private static int? _sMaxDepth;
        public static int MaxDepth
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.MaxDepth : null)
                   ?? _sMaxDepth
                   ?? int.MaxValue;
            set
            {
                if (!_sMaxDepth.HasValue) _sMaxDepth = value;
            }
        }

        /// <summary>
        /// Set this to enable your own type construction provider.
        /// This is helpful for integration with IoC containers where you need to call the container constructor.
        /// Return null if you don't know how to construct the type and the parameterless constructor will be used.
        /// </summary>
        private static EmptyCtorFactoryDelegate _sModelFactory;
        public static EmptyCtorFactoryDelegate ModelFactory
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.ModelFactory : null)
                   ?? _sModelFactory
                   ?? null;
            set
            {
                if (_sModelFactory != null) _sModelFactory = value;
            }
        }

        private static string[] _sExcludePropertyReferences;
        public static string[] ExcludePropertyReferences
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.ExcludePropertyReferences : null)
                   ?? _sExcludePropertyReferences;
            set
            {
                if (_sExcludePropertyReferences != null) _sExcludePropertyReferences = value;
            }
        }

        private static HashSet<Type> _sExcludeTypes;
        public static HashSet<Type> ExcludeTypes
        {
            get => (JsConfigScope.Current != null ? JsConfigScope.Current.ExcludeTypes : null)
                   ?? _sExcludeTypes;
            set
            {
                if (_sExcludePropertyReferences != null) _sExcludeTypes = value;
            }
        }

        public static string[] IgnoreAttributesNamed
        {
            set => ReflectionExtensions.IgnoreAttributesNamed = value;
            get => ReflectionExtensions.IgnoreAttributesNamed;
        }

        public static HashSet<string> AllowRuntimeTypeWithAttributesNamed { get; set; }

        public static HashSet<string> AllowRuntimeTypeWithInterfacesNamed { get; set; }

        public static HashSet<string> AllowRuntimeTypeInTypes { get; set; }

        public static HashSet<string> AllowRuntimeTypeInTypesWithNamespaces { get; set; }

        public static Func<Type, bool> AllowRuntimeType { get; set; }

        public static void Reset()
        {
            foreach (var rawSerializeType in HasSerializeFn.ToArray())
            {
                Reset(rawSerializeType);
            }
            foreach (var rawSerializeType in HasIncludeDefaultValue.ToArray())
            {
                Reset(rawSerializeType);
            }
            foreach (var uniqueType in UniqueTypes.ToArray())
            {
                Reset(uniqueType);
            }

            _sModelFactory = ReflectionExtensions.GetConstructorMethodToCache;
            _sTryToParsePrimitiveTypeValues = null;
            _sTryToParseNumericType = null;
            _sTryParseIntoBestFit = null;
            _sConvertObjectTypesIntoStringDictionary = null;
            _sExcludeDefaultValues = null;
            _sIncludeNullValues = null;
            _sIncludeNullValuesInDictionaries = null;
            _sIncludeDefaultEnums = null;
            _sExcludeTypeInfo = null;
            _sEmitCamelCaseNames = null;
            _sEmitLowercaseUnderscoreNames = null;
            _sDateHandler = null;
            _sTimeSpanHandler = null;
            _sPreferInterfaces = null;
            _sThrowOnDeserializationError = null;
            _sTypeAttr = null;
            _sDateTimeFormat = null;
            _sJsonTypeAttrInObject = null;
            _sJsvTypeAttrInObject = null;
            _sTypeWriter = null;
            _sTypeFinder = null;
            _sParsePrimitiveFn = null;
            _sTreatEnumAsInteger = null;
            _sAlwaysUseUtc = null;
            _sAssumeUtc = null;
            _sSkipDateTimeConversion = null;
            _sAppendUtcOffset = null;
            _sEscapeUnicode = null;
            _sEscapeHtmlChars = null;
            _sOnDeserializationError = null;
            _sIncludePublicFields = null;
            HasSerializeFn = new HashSet<Type>();
            HasIncludeDefaultValue = new HashSet<Type>();
            TreatValueAsRefTypes = new HashSet<Type> { typeof(KeyValuePair<,>) };
            _sPropertyConvention = null;
            _sExcludePropertyReferences = null;
            _sExcludeTypes = new HashSet<Type> { typeof(Stream) };
            UniqueTypes = new HashSet<Type>();
            _sMaxDepth = 50;
            _sParsePrimitiveIntegerTypes = null;
            _sParsePrimitiveFloatingPointTypes = null;
            AllowRuntimeType = null;
            AllowRuntimeTypeWithAttributesNamed = new HashSet<string>
            {
                nameof(DataContractAttribute),
                nameof(RuntimeSerializableAttribute),
            };
            AllowRuntimeTypeWithInterfacesNamed = new HashSet<string>
            {
                "IConvertible",
                "ISerializable",
                "IRuntimeSerializable",
                "IMeta",
                "IReturn`1",
                "IReturnVoid",
            };
            AllowRuntimeTypeInTypesWithNamespaces = new HashSet<string>
            {
                "ServiceStack.Messaging",
            };
            AllowRuntimeTypeInTypes = new HashSet<string>
            {
                "ServiceStack.RequestLogEntry"
            };
            E.Common.PlatformExtensions.ClearRuntimeAttributes();
            ReflectionExtensions.Reset();
            JsState.Reset();
        }

        static void Reset(Type cachesForType)
        {
            typeof(JsConfig<>).MakeGenericType(new[] { cachesForType }).InvokeReset();
            typeof(TypeConfig<>).MakeGenericType(new[] { cachesForType }).InvokeReset();
        }

        internal static void InvokeReset(this Type genericType)
        {
            var methodInfo = genericType.GetStaticMethod("Reset");
            methodInfo.Invoke(null, null);
        }

        internal static HashSet<Type> UniqueTypes = new HashSet<Type>();
        internal static int UniqueTypesCount = 0;

        internal static void AddUniqueType(Type type)
        {
            if (UniqueTypes.Contains(type))
                return;

            HashSet<Type> newTypes, snapshot;
            do
            {
                snapshot = UniqueTypes;
                newTypes = new HashSet<Type>(UniqueTypes) { type };
                UniqueTypesCount = newTypes.Count;

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref UniqueTypes, newTypes, snapshot), snapshot));
        }
    }

    public class JsConfig<T>
    {
        static JsConfig()
        {
            // Run the type's static constructor (which may set OnDeserialized, etc.) before we cache any information about it
            RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
        }

        /// <summary>
        /// Always emit type info for this type.  Takes precedence over ExcludeTypeInfo
        /// </summary>
        public static bool? IncludeTypeInfo = null;

        /// <summary>
        /// Never emit type info for this type
        /// </summary>
        public static bool? ExcludeTypeInfo = null;

        /// <summary>
        /// <see langword="true"/> if the <see cref="ITypeSerializer"/> is configured
        /// to take advantage of <see cref="CLSCompliantAttribute"/> specification,
        /// to support user-friendly serialized formats, ie emitting camelCasing for JSON
        /// and parsing member names and enum values in a case-insensitive manner.
        /// </summary>
        public static bool? EmitCamelCaseNames = null;

        public static bool? EmitLowercaseUnderscoreNames = null;

        public static bool IncludeDefaultValue
        {
            get => JsConfig.HasIncludeDefaultValue.Contains(typeof(T));
            set
            {
                if (value)
                    JsConfig.HasIncludeDefaultValue.Add(typeof(T));
                else
                    JsConfig.HasIncludeDefaultValue.Remove(typeof(T));

                ClearFnCaches();
            }
        }

        /// <summary>
        /// Define custom serialization fn for BCL Structs
        /// </summary>
        private static Func<T, string> _serializeFn;
        public static Func<T, string> SerializeFn
        {
            get => _serializeFn;
            set
            {
                _serializeFn = value;
                if (value != null)
                    JsConfig.HasSerializeFn.Add(typeof(T));
                else
                    JsConfig.HasSerializeFn.Remove(typeof(T));

                ClearFnCaches();
            }
        }

        /// <summary>
        /// Opt-in flag to set some Value Types to be treated as a Ref Type
        /// </summary>
        public static bool TreatValueAsRefType
        {
            get => JsConfig.TreatValueAsRefTypes.Contains(typeof(T));
            set
            {
                if (value)
                    JsConfig.TreatValueAsRefTypes.Add(typeof(T));
                else
                    JsConfig.TreatValueAsRefTypes.Remove(typeof(T));
            }
        }

        /// <summary>
        /// Whether there is a fn (raw or otherwise)
        /// </summary>
        public static bool HasSerializeFn => !JsState.InSerializer<T>() && (_serializeFn != null || _rawSerializeFn != null);

        /// <summary>
        /// Define custom raw serialization fn
        /// </summary>
        private static Func<T, string> _rawSerializeFn;
        public static Func<T, string> RawSerializeFn
        {
            get => _rawSerializeFn;
            set
            {
                _rawSerializeFn = value;
                if (value != null)
                    JsConfig.HasSerializeFn.Add(typeof(T));
                else
                    JsConfig.HasSerializeFn.Remove(typeof(T));

                ClearFnCaches();
            }
        }

        /// <summary>
        /// Define custom serialization hook
        /// </summary>
        private static Func<T, T> _onSerializingFn;
        public static Func<T, T> OnSerializingFn
        {
            get => _onSerializingFn;
            set { _onSerializingFn = value; RefreshWrite(); }
        }

        /// <summary>
        /// Define custom after serialization hook
        /// </summary>
        private static Action<T> _onSerializedFn;
        public static Action<T> OnSerializedFn
        {
            get => _onSerializedFn;
            set { _onSerializedFn = value; RefreshWrite(); }
        }

        /// <summary>
        /// Define custom deserialization fn for BCL Structs
        /// </summary>
        private static Func<string, T> _deSerializeFn;
        public static Func<string, T> DeSerializeFn
        {
            get => _deSerializeFn;
            set { _deSerializeFn = value; RefreshRead(); }
        }

        /// <summary>
        /// Define custom raw deserialization fn for objects
        /// </summary>
        private static Func<string, T> _rawDeserializeFn;
        public static Func<string, T> RawDeserializeFn
        {
            get => _rawDeserializeFn;
            set { _rawDeserializeFn = value; RefreshRead(); }
        }

        public static bool HasDeserializeFn => !JsState.InDeserializer<T>() && (DeSerializeFn != null || RawDeserializeFn != null);

        private static Func<T, T> _onDeserializedFn;
        public static Func<T, T> OnDeserializedFn
        {
            get => _onDeserializedFn;
            set { _onDeserializedFn = value; RefreshRead(); }
        }

        public static bool HasDeserialingFn => OnDeserializingFn != null;

        private static Func<T, string, object, object> _onDeserializingFn;
        public static Func<T, string, object, object> OnDeserializingFn
        {
            get => _onDeserializingFn;
            set { _onDeserializingFn = value; RefreshRead(); }
        }

        /// <summary>
        /// Exclude specific properties of this type from being serialized
        /// </summary>
        public static string[] ExcludePropertyNames;

        public static void WriteFn<TSerializer>(TextWriter writer, object obj)
        {
            if (RawSerializeFn != null && !JsState.InSerializer<T>())
            {
                JsState.RegisterSerializer<T>();
                try
                {
                    writer.Write(RawSerializeFn((T)obj));
                }
                finally
                {
                    JsState.UnRegisterSerializer<T>();
                }
            }
            else if (SerializeFn != null && !JsState.InSerializer<T>())
            {
                JsState.RegisterSerializer<T>();
                try
                {
                    var serializer = JsWriter.GetTypeSerializer<TSerializer>();
                    serializer.WriteString(writer, SerializeFn((T)obj));
                }
                finally
                {
                    JsState.UnRegisterSerializer<T>();
                }
            }
            else
            {
                var writerFn = JsonWriter.Instance.GetWriteFn<T>();
                writerFn(writer, obj);
            }
        }

        public static object ParseFn(string str)
        {
            return DeSerializeFn(str);
        }

        internal static object ParseFn(ITypeSerializer serializer, string str)
        {
            if (RawDeserializeFn != null && !JsState.InDeserializer<T>())
            {
                JsState.RegisterDeserializer<T>();
                try
                {
                    return RawDeserializeFn(str);
                }
                finally
                {
                    JsState.UnRegisterDeserializer<T>();
                }
            }
            else if (DeSerializeFn != null && !JsState.InDeserializer<T>())
            {
                JsState.RegisterDeserializer<T>();
                try
                {
                    return DeSerializeFn(serializer.UnescapeString(str));
                }
                finally
                {
                    JsState.UnRegisterDeserializer<T>();
                }
            }
            else
            {
                var parseFn = JsonReader.Instance.GetParseFn<T>();
                return parseFn(str);
            }
        }

        internal static void ClearFnCaches()
        {
            JsonWriter<T>.Reset();
            JsvWriter<T>.Reset();
        }

        public static void Reset()
        {
            RawSerializeFn = null;
            DeSerializeFn = null;
            ExcludePropertyNames = null;
            EmitCamelCaseNames = EmitLowercaseUnderscoreNames = IncludeTypeInfo = ExcludeTypeInfo = null;
        }

        public static void RefreshRead()
        {
            JsonReader<T>.Refresh();
            JsvReader<T>.Refresh();
        }

        public static void RefreshWrite()
        {
            JsonWriter<T>.Refresh();
            JsvWriter<T>.Refresh();
        }
    }

    public enum PropertyConvention
    {
        /// <summary>
        /// The property names on target types must match property names in the JSON source
        /// </summary>
        Strict,
        /// <summary>
        /// The property names on target types may not match the property names in the JSON source
        /// </summary>
        Lenient
    }

    public enum DateHandler
    {
        TimestampOffset,
        DcjsCompatible,
        Iso8601,
        Iso8601DateOnly,
        Iso8601DateTime,
        Rfc1123,
        UnixTime,
        UnixTimeMs,
    }

    public enum TimeSpanHandler
    {
        /// <summary>
        /// Uses the xsd format like PT15H10M20S
        /// </summary>
        DurationFormat,
        /// <summary>
        /// Uses the standard .net ToString method of the TimeSpan class
        /// </summary>
        StandardFormat
    }
}

