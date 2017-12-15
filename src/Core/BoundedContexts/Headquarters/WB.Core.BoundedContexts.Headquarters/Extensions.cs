using System;
using System.Globalization;

namespace WB.Core.BoundedContexts.Headquarters
{
    public static class Extensions
    {
        public static string ClientDateTimeFormat = "MMM DD, YYYY HH:mm";
        public static string ServerDateTimeFormat = "MMM dd, yyy HH:mm";

        public static string FormatDateWithTime(this DateTime dateTime) 
            => dateTime.ToString(ServerDateTimeFormat, CultureInfo.CurrentUICulture);

        public static DateTime? TryParseDate(this string value)
        {
            var isParsed = DateTime.TryParseExact(value, @"yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime);

            return isParsed ? dateTime : (DateTime?) null;
        }
    }
}