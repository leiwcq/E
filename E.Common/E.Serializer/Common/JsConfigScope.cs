﻿using System;
using System.Collections.Generic;
using E.Common;

namespace E.Serializer.Common
{
    public sealed class JsConfigScope : IDisposable
    {
        private bool _disposed;
        private readonly JsConfigScope _parent;

        [ThreadStatic]
        private static JsConfigScope _head;

        internal JsConfigScope()
        {
            E.Common.PclExport.Instance.BeginThreadAffinity();

            _parent = _head;
            _head = this;
        }

        internal static JsConfigScope Current => _head;

        public static void DisposeCurrent()
        {
            _head?.Dispose();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _head = _parent;

                E.Common.PclExport.Instance.EndThreadAffinity();
            }
        }

        public bool? ConvertObjectTypesIntoStringDictionary { get; set; }
        public bool? TryToParsePrimitiveTypeValues { get; set; }
        public bool? TryToParseNumericType { get; set; }
        public bool? TryParseIntoBestFit { get; set; }
        public ParseAsType? ParsePrimitiveFloatingPointTypes { get; set; }
        public ParseAsType? ParsePrimitiveIntegerTypes { get; set; }
        public bool? ExcludeDefaultValues { get; set; }
        public bool? IncludeNullValues { get; set; }
        public bool? IncludeNullValuesInDictionaries { get; set; }
        public bool? IncludeDefaultEnums { get; set; }
        public bool? TreatEnumAsInteger { get; set; }
        public bool? ExcludeTypeInfo { get; set; }
        public bool? IncludeTypeInfo { get; set; }
        public string TypeAttr { get; set; }
        public string DateTimeFormat { get; set; }
        internal string JsonTypeAttrInObject { get; set; }
        internal string JsvTypeAttrInObject { get; set; }
        public Func<Type, string> TypeWriter { get; set; }
        public Func<string, Type> TypeFinder { get; set; }
        public Func<string, object> ParsePrimitiveFn { get; set; }
        public DateHandler? DateHandler { get; set; }
        public TimeSpanHandler? TimeSpanHandler { get; set; }
        public PropertyConvention? PropertyConvention { get; set; }
        public bool? EmitCamelCaseNames { get; set; }
        public bool? EmitLowercaseUnderscoreNames { get; set; }
        public bool? ThrowOnDeserializationError { get; set; }
        public bool? SkipDateTimeConversion { get; set; }
        public bool? AlwaysUseUtc { get; set; }
        public bool? AssumeUtc { get; set; }
        public bool? AppendUtcOffset { get; set; }
        public bool? EscapeUnicode { get; set; }
        public bool? EscapeHtmlChars { get; set; }
        public bool? PreferInterfaces { get; set; }
        public bool? IncludePublicFields { get; set; }
        public int? MaxDepth { get; set; }
        public DeserializationErrorDelegate OnDeserializationError { get; set; }
        public EmptyCtorFactoryDelegate ModelFactory { get; set; }
        public string[] ExcludePropertyReferences { get; set; }
        public HashSet<Type> ExcludeTypes { get; set; }
    }
}
