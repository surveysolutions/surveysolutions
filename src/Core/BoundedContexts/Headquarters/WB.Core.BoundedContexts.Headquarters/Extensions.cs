using System;
using System.Globalization;

namespace WB.Core.BoundedContexts.Headquarters
{
    public static class Extensions
    {
        public static string ClientDateTimeFormat = "MMM DD, YYYY HH:mm";
        public static string ServerDateTimeFormat = "MMM DD, yyy HH:mm";

        public static string FormatDateWithTime(this DateTime dateTime) 
            => dateTime.ToString(ServerDateTimeFormat, CultureInfo.CurrentUICulture);
    }
}