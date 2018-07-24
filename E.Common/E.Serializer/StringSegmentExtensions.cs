using System;
using E.Common;
using E.Serializer.Csv;
using Microsoft.Extensions.Primitives;

namespace E.Serializer
{
    public static class StringSegmentExtensions
    {
        public static StringSegment FromCsvField(this StringSegment text)
        {
            return text.IsNullOrEmpty() || !text.StartsWith(CsvConfig.ItemDelimiterString, StringComparison.Ordinal)
                ? text
                : new StringSegment(
                    text.Subsegment(CsvConfig.ItemDelimiterString.Length, text.Length - CsvConfig.EscapedItemDelimiterString.Length)
                        .Value
                        .Replace(CsvConfig.EscapedItemDelimiterString, CsvConfig.ItemDelimiterString));
        }
    }
}
