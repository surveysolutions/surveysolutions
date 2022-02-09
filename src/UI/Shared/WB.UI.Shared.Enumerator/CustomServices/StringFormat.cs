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
        private readonly IMvxAndroidCurrentTopActivity mvxCurrentTopActivity;

        public StringFormat()
        {
            this.mvxCurrentTopActivity = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();
        }

        public string ShortTime(DateTimeOffset dateTime)
        {
            var timeFormat = DateFormat.GetTimeFormat(mvxCurrentTopActivity.Activity);
            var date = ToJavaDate(dateTime);
            var timeString = timeFormat?.Format(date);
            return timeString;
        }

        public string ShortDateTime(DateTimeOffset dateTime)
        {
            var time = ShortTime(dateTime);
            var date = dateTime.ToString("MMM dd, ", CultureInfo.CurrentUICulture);
            return date + time;
        }

        private Date ToJavaDate(DateTimeOffset dateTime)
            => new Date(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
            //=> new Date(dateTime.ToUnixTimeSeconds());
    }
}
