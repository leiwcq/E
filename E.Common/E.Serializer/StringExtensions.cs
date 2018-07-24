using System;
using E.Common;
using E.Serializer.Common;
using E.Serializer.Csv;

namespace E.Serializer
{
    public static class StringExtensions
    {
        public static string EncodeJsv(this string value)
        {
            if (JsState.QueryStringMode)
            {
                return value.UrlEncode();
            }
            return String.IsNullOrEmpty(value) || !JsWriter.HasAnyEscapeChars(value)
                ? value
                : string.Concat
                (
                    JsWriter.QuoteString,
                    value.Replace(JsWriter.QuoteString, TypeSerializer.DoubleQuoteString),
                    JsWriter.QuoteString
                );
        }

        public static string DecodeJsv(this string value)
        {
            const int startingQuotePos = 1;
            const int endingQuotePos = 2;
            return String.IsNullOrEmpty(value) || value[0] != JsWriter.QuoteChar
                ? value
                : value.Substring(startingQuotePos, value.Length - endingQuotePos)
                    .Replace(TypeSerializer.DoubleQuoteString, JsWriter.QuoteString);
        }

        public static string ToJsv<T>(this T obj)
        {
            return TypeSerializer.SerializeToString(obj);
        }

        public static string ToSafeJsv<T>(this T obj)
        {
            return TypeSerializer.HasCircularReferences(obj)
                ? obj.ToSafePartialObjectDictionary().ToJsv()
                : obj.ToJsv();
        }

        public static T FromJsv<T>(this string jsv)
        {
            return TypeSerializer.DeserializeFromString<T>(jsv);
        }

        public static string ToCsv<T>(this T obj)
        {
            return CsvSerializer.SerializeToString(obj);
        }

        public static T FromCsv<T>(this string csv)
        {
            return CsvSerializer.DeserializeFromString<T>(csv);
        }

        public static string ToXml<T>(this T obj)
        {
            return XmlSerializer.SerializeToString(obj);
        }

        public static T FromXml<T>(this string csv)
        {
            return XmlSerializer.DeserializeFromString<T>(csv);
        }
    }
}
