using System;

namespace StatData.Writers.Spss
{
    internal class SpssTimeStamp
    {
        public string Date { get; set; }
        public string Time { get; set; }

        public static SpssTimeStamp GetDateInfo(DateTime now)
        {
            const string dateFormat = "{0} {1} {2}";
            const string timeFormat = "{0}:{1}:{2}";

            var months = new[]
                             {
                                 "Jan", "Feb", "Mar",
                                 "Apr", "May", "Jun",
                                 "Jul", "Aug", "Sep",
                                 "Oct", "Nov", "Dec"
                             };

            var year = (now.Year % 100).ToString("00");
            var mon = months[now.Month - 1];
            var day = now.Day.ToString("00");

            var spssDate = String.Format(dateFormat, day, mon, year);

            var spssTime = String.Format(
                timeFormat,
                now.Hour.ToString("00"),
                now.Minute.ToString("00"),
                now.Second.ToString("00"));

            var realDate = new SpssTimeStamp() { Date = spssDate, Time = spssTime };

            return realDate;
        }

    }
}
