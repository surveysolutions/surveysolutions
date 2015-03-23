using System;

namespace WB.UI.Designer.Utils
{
    public static class FormatHelper
    {
        public static string FormatDate(this DateTime date)
        {
            var localDate = date.ToLocalTime();
            if (DateTime.Now == localDate)
            {
                return string.Format("Today at {0}", localDate.ToString("HH:mm"));
            }
            return localDate.ToString("d MMM, HH:mm");
        }
    }
}
