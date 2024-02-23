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
        
        public static DateTime SetKind(this DateTime dateTime, DateTimeKind dateTimeKind)
            => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTimeKind);
        public static DateTime SetUtcKind(this DateTime dateTime)
            => SetKind(dateTime, DateTimeKind.Utc);
        public static DateTime SetLocalKind(this DateTime dateTime)
            => SetKind(dateTime, DateTimeKind.Local);

    }
}
