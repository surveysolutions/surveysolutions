using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reports.Views
{
    public class CountDaysOfInterviewInStatusView
    {
        public string[] Header { get; set; }

        public CountDaysOfInterviewInStatusRow[] Rows { get; set; }
    }

    public class CountDaysOfInterviewInStatusRow
    {
        public int DaysCount { get; set; }

        public int[] Cells { get; set; }
        public DateTime Date { get; set; }
    }
}