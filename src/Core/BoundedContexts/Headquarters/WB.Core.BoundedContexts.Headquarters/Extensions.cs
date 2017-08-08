using System;
using System.Globalization;

namespace WB.Core.BoundedContexts.Headquarters
{
    public static class Extensions
    {
        public static string ClientDateTimeFormat = "MMM d, YYYY HH:mm";
        public static string ServerDateTimeFormat = "MMM d, yyy HH:mm";

        public static string FormatDateWithTime(this DateTime dateTime) 
            => dateTime.ToString(ServerDateTimeFormat, CultureInfo.CurrentUICulture);
    }
}