using System.IO;
using System.Runtime.CompilerServices;
using E.Common;
using E.Serializer.Common;
using Microsoft.Extensions.Primitives;

namespace E.Serializer.Json
{
    public static class JsonUtils
    {
        public const long MAX_INTEGER = 9007199254740992;
        public const long MIN_INTEGER = -9007199254740992;

        public const char ESCAPE_CHAR = '\\';

        public const char QUOTE_CHAR = '"';
        public const string NULL = "null";
        public const string TRUE = "true";
        public const string FALSE = "false";

        public const char SPACE_CHAR = ' ';
        public const char TAB_CHAR = '\t';
        public const char CARRIAGE_RETURN_CHAR = '\r';
        public const char LINE_FEED_CHAR = '\n';
        public const char FORM_FEED_CHAR = '\f';
        public const char BACKSPACE_CHAR = '\b';

        /// <summary>
        /// Micro-optimization keep pre-built char arrays saving a .ToCharArray() + function call (see .net implementation of .Write(string))
        /// </summary>
        private static readonly char[] EscapedBackslash = { ESCAPE_CHAR, ESCAPE_CHAR };
        private static readonly char[] EscapedTab = { ESCAPE_CHAR, 't' };
        private static readonly char[] EscapedCarriageReturn = { ESCAPE_CHAR, 'r' };
        private static readonly char[] EscapedLineFeed = { ESCAPE_CHAR, 'n' };
        private static readonly char[] EscapedFormFeed = { ESCAPE_CHAR, 'f' };
        private static readonly char[] EscapedBackspace = { ESCAPE_CHAR, 'b' };
        private static readonly char[] EscapedQuote = { ESCAPE_CHAR, QUOTE_CHAR };

        public static readonly char[] WhiteSpaceChars = { ' ', TAB_CHAR, CARRIAGE_RETURN_CHAR, LINE_FEED_CHAR };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhiteSpace(char c)
        {
            return c == ' ' || (c >= '\x0009' && c <= '\x000d') || c == '\x00a0' || c == '\x0085';
        }

        public static void WriteString(TextWriter writer, string value)
        {
            if (value == null)
            {
                writer.Write(NULL);
                return;
            }

            var escapeHtmlChars = JsConfig.EscapeHtmlChars;
            var escapeUnicode = JsConfig.EscapeUnicode;

            if (!HasAnyEscapeChars(value, escapeHtmlChars))
            {
                writer.Write(QUOTE_CHAR);
                writer.Write(value);
                writer.Write(QUOTE_CHAR);
                return;
            }

            var hexSeqBuffer = new char[4];
            writer.Write(QUOTE_CHAR);

            var len = value.Length;
            for (var i = 0; i < len; i++)
            {
                char c = value[i];

                switch (c)
                {
                    case LINE_FEED_CHAR:
                        writer.Write(EscapedLineFeed);
                        continue;

                    case CARRIAGE_RETURN_CHAR:
                        writer.Write(EscapedCarriageReturn);
                        continue;

                    case TAB_CHAR:
                        writer.Write(EscapedTab);
                        continue;

                    case QUOTE_CHAR:
                        writer.Write(EscapedQuote);
                        continue;

                    case ESCAPE_CHAR:
                        writer.Write(EscapedBackslash);
                        continue;

                    case FORM_FEED_CHAR:
                        writer.Write(EscapedFormFeed);
                        continue;

                    case BACKSPACE_CHAR:
                        writer.Write(EscapedBackspace);
                        continue;
                }

                if (escapeHtmlChars)
                {
                    switch (c)
                    {
                        case '<':
                            writer.Write("\\u003c");
                            continue;
                        case '>':
                            writer.Write("\\u003e");
                            continue;
                        case '&':
                            writer.Write("\\u0026");
                            continue;
                        case '=':
                            writer.Write("\\u003d");
                            continue;
                        case '\'':
                            writer.Write("\\u0027");
                            continue;
                    }
                }

                if (c.IsPrintable())
                {
                    writer.Write(c);
                    continue;
                }

                // http://json.org/ spec requires any control char to be escaped
                if (escapeUnicode || char.IsControl(c))
                {
                    // Default, turn into a \uXXXX sequence
                    IntToHex(c, hexSeqBuffer);
                    writer.Write("\\u");
                    writer.Write(hexSeqBuffer);
                }
                else
                {
                    writer.Write(c);
                }
            }

            writer.Write(QUOTE_CHAR);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsPrintable(this char c)
        {
            return c >= 32 && c <= 126;
        }

        /// <summary>
        /// Searches the string for one or more non-printable characters.
        /// </summary>
        /// <param name="value">The string to search.</param>
        /// <param name="escapeHtmlChars"></param>
        /// <returns>True if there are any characters that require escaping. False if the value can be written verbatim.</returns>
        /// <remarks>
        /// Micro optimizations: since quote and backslash are the only printable characters requiring escaping, removed previous optimization
        /// (using flags instead of value.IndexOfAny(EscapeChars)) in favor of two equality operations saving both memory and CPU time.
        /// Also slightly reduced code size by re-arranging conditions.
        /// TODO: Possible Linq-only solution requires profiling: return value.Any(c => !c.IsPrintable() || c == QuoteChar || c == EscapeChar);
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasAnyEscapeChars(string value, bool escapeHtmlChars)
        {
            var len = value.Length;
            for (var i = 0; i < len; i++)
            {
                var c = value[i];

                // c is not printable
                // OR c is a printable that requires escaping (quote and backslash).
                if (!c.IsPrintable() || c == QUOTE_CHAR || c == ESCAPE_CHAR)
                    return true;

                if (escapeHtmlChars && (c == '<' || c == '>' || c == '&' || c == '=' || c == '\\'))
                    return true;
            }
            return false;
        }

        // Micro optimized
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IntToHex(int intValue, char[] hex)
        {
            // TODO: test if unrolling loop is faster
            for (var i = 3; i >= 0; i--)
            {
                var num = intValue & 0xF; // intValue % 16

                // 0x30 + num == '0' + num
                // 0x37 + num == 'A' + (num - 10)
                hex[i] = (char)((num < 10 ? 0x30 : 0x37) + num);

                intValue >>= 4;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsJsObject(string value)
        {
            return !string.IsNullOrEmpty(value)
                && value[0] == '{'
                && value[value.Length - 1] == '}';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsJsObject(StringSegment value)
        {
            return !value.IsNullOrEmpty()
                   && value.GetChar(0) == '{'
                   && value.GetChar(value.Length - 1) == '}';
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsJsArray(string value)
        {
            return !string.IsNullOrEmpty(value)
                && value[0] == '['
                && value[value.Length - 1] == ']';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsJsArray(StringSegment value)
        {
            return !value.IsNullOrEmpty()
                   && value.GetChar(0) == '['
                   && value.GetChar(value.Length - 1) == ']';
        }

    }
}
