using System;

namespace WB.Core.BoundedContexts.Headquarters
{
    public static class Extensions
    {
        public static string FormatDateWithTime(this DateTime dateTime)
        {
            string format = "MMM d, yyy HH:mm";
            return dateTime.ToString(format);
        }
    }
}