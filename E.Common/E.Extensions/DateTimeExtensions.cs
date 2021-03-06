using System;
using System.Globalization;

namespace E.Extensions
{
    /// <summary>
    /// A fast, standards-based, serialization-issue free DateTime serailizer.
    /// </summary>
    public static class DateTimeExtensions
    {
        public const long UNIX_EPOCH = 621355968000000000L;
        private static readonly DateTime UnixEpochDateTimeUtc = new DateTime(UNIX_EPOCH, DateTimeKind.Utc);
        private static readonly DateTime UnixEpochDateTimeUnspecified = new DateTime(UNIX_EPOCH, DateTimeKind.Unspecified);
        private static readonly DateTime MinDateTimeUtc = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime FromUnixTime(this int unixTime)
        {
            return UnixEpochDateTimeUtc + TimeSpan.FromSeconds(unixTime);
        }

        public static DateTime FromUnixTime(this double unixTime)
        {
            return UnixEpochDateTimeUtc + TimeSpan.FromSeconds(unixTime);
        }

        public static DateTime FromUnixTime(this long unixTime)
        {
            return UnixEpochDateTimeUtc + TimeSpan.FromSeconds(unixTime);
        }

        public static long ToUnixTimeMsAlt(this DateTime dateTime)
        {
            return (dateTime.ToStableUniversalTime().Ticks - UNIX_EPOCH) / TimeSpan.TicksPerMillisecond;
        }

        public static long ToUnixTimeMs(this DateTime dateTime)
        {
            var universal = ToDateTimeSinceUnixEpoch(dateTime);
            return (long)universal.TotalMilliseconds;
        }

        public static long ToUnixTime(this DateTime dateTime)
        {
            return (dateTime.ToDateTimeSinceUnixEpoch().Ticks) / TimeSpan.TicksPerSecond;
        }

        private static TimeSpan ToDateTimeSinceUnixEpoch(this DateTime dateTime)
        {
            var dtUtc = dateTime;
            if (dateTime.Kind != DateTimeKind.Utc)
            {
                dtUtc = dateTime.Kind == DateTimeKind.Unspecified && dateTime > DateTime.MinValue && dateTime < DateTime.MaxValue
                    ? DateTime.SpecifyKind(dateTime.Subtract(TimeZoneInfo.Local.GetUtcOffset(dateTime)), DateTimeKind.Utc)
                    : dateTime.ToStableUniversalTime();
            }

            var universal = dtUtc.Subtract(UnixEpochDateTimeUtc);
            return universal;
        }

        public static long ToUnixTimeMs(this long ticks)
        {
            return (ticks - UNIX_EPOCH) / TimeSpan.TicksPerMillisecond;
        }

        public static DateTime FromUnixTimeMs(this double msSince1970)
        {
            return UnixEpochDateTimeUtc + TimeSpan.FromMilliseconds(msSince1970);
        }

        public static DateTime FromUnixTimeMs(this long msSince1970)
        {
            return UnixEpochDateTimeUtc + TimeSpan.FromMilliseconds(msSince1970);
        }

        public static DateTime FromUnixTimeMs(this long msSince1970, TimeSpan offset)
        {
            return DateTime.SpecifyKind(UnixEpochDateTimeUnspecified + TimeSpan.FromMilliseconds(msSince1970) + offset, DateTimeKind.Local);
        }

        public static DateTime FromUnixTimeMs(this double msSince1970, TimeSpan offset)
        {
            return DateTime.SpecifyKind(UnixEpochDateTimeUnspecified + TimeSpan.FromMilliseconds(msSince1970) + offset, DateTimeKind.Local);
        }

        public static DateTime FromUnixTimeMs(string msSince1970)
        {
            if (long.TryParse(msSince1970, out var ms)) return ms.FromUnixTimeMs();

            // Do we really need to support fractional unix time ms time strings??
            return double.Parse(msSince1970).FromUnixTimeMs();
        }

        public static DateTime FromUnixTimeMs(string msSince1970, TimeSpan offset)
        {
            if (long.TryParse(msSince1970, out var ms)) return ms.FromUnixTimeMs(offset);

            // Do we really need to support fractional unix time ms time strings??
            return double.Parse(msSince1970).FromUnixTimeMs(offset);
        }

        public static DateTime RoundToMs(this DateTime dateTime)
        {
            return new DateTime(dateTime.Ticks / TimeSpan.TicksPerMillisecond * TimeSpan.TicksPerMillisecond, dateTime.Kind);
        }

        public static DateTime RoundToSecond(this DateTime dateTime)
        {
            return new DateTime(dateTime.Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond, dateTime.Kind);
        }

        public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
        {
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }

        public static bool IsEqualToTheSecond(this DateTime dateTime, DateTime otherDateTime)
        {
            return dateTime.ToStableUniversalTime().RoundToSecond().Equals(otherDateTime.ToStableUniversalTime().RoundToSecond());
        }

        public static string ToTimeOffsetString(this TimeSpan offset, string seperator = "")
        {
            var hours = Math.Abs(offset.Hours).ToString(CultureInfo.InvariantCulture);
            var minutes = Math.Abs(offset.Minutes).ToString(CultureInfo.InvariantCulture);
            return (offset < TimeSpan.Zero ? "-" : "+")
                + (hours.Length == 1 ? "0" + hours : hours)
                + seperator
                + (minutes.Length == 1 ? "0" + minutes : minutes);
        }

        public static TimeSpan FromTimeOffsetString(this string offsetString)
        {
            if (!offsetString.Contains(":"))
                offsetString = offsetString.Insert(offsetString.Length - 2, ":");

            offsetString = offsetString.TrimStart('+');

            return TimeSpan.Parse(offsetString);
        }

        public static DateTime ToStableUniversalTime(this DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
                return dateTime;
            if (dateTime == DateTime.MinValue)
                return MinDateTimeUtc;

            return dateTime.ToUniversalTime();
        }

        public static string FmtSortableDate(this DateTime from)
        {
            return from.ToString("yyyy-MM-dd");
        }

        public static string FmtSortableDateTime(this DateTime from)
        {
            return from.ToString("u");
        }

        public static DateTime LastMonday(this DateTime from)
        {
            var mondayOfWeek = from.Date.AddDays(-(int)from.DayOfWeek + 1);
            return mondayOfWeek;
        }

        public static DateTime StartOfLastMonth(this DateTime from)
        {
            return new DateTime(from.Date.Year, from.Date.Month, 1).AddMonths(-1);
        }

        public static DateTime EndOfLastMonth(this DateTime from)
        {
            return new DateTime(from.Date.Year, from.Date.Month, 1).AddDays(-1);
        }
    }

}
