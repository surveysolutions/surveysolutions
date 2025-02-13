using System;
using System.Globalization;

namespace WB.UI.Designer.Utils
{
    public static class FormatHelper
    {
        public static string ConvertToLocalAndFormatDate(this DateTime date)
        {
            var localDate = date.ToLocalTime();
            if (DateTime.Now == localDate)
            {
                return string.Format("Today at {0}", localDate.ToString("HH:mm", CultureInfo.CurrentUICulture));
            }
            return localDate.ToString("MMM dd, yyy HH:mm", CultureInfo.CurrentUICulture);
        }


        public static string FormatDate(this DateTime date)
        {
            return date.ToString("MMM dd, yyy HH:mm", CultureInfo.CurrentUICulture);
        }
    }
}
