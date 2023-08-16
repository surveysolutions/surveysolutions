using System;
using System.Globalization;
using Android.Text.Format;
using Java.Util;
using MvvmCross;
using MvvmCross.Platforms.Android;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.CustomServices
{
    public class StringFormat : IStringFormat
    {
        public string ShortTime(DateTimeOffset dateTime)
        {
            var mvxCurrentTopActivity = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();
            if (mvxCurrentTopActivity == null) return null;
            
            var timeFormat = DateFormat.GetTimeFormat(mvxCurrentTopActivity.Activity);
            var date = ToJavaDate(dateTime);
            var timeString = timeFormat?.Format(date);
            return timeString;
        }

        public string ShortDateTime(DateTimeOffset dateTime)
        {
            var time = ShortTime(dateTime);
            if (time == null) return string.Empty;
            
            var date = dateTime.ToString("MMM dd, ", CultureInfo.CurrentUICulture);
            return date + time;
        }

        private Date ToJavaDate(DateTimeOffset dateTime)
            => new Date(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
            //=> new Date(dateTime.ToUnixTimeSeconds());
    }
}
