using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.InterviewerProfiles
{
    public class InterviewerDailyTrafficUsage
    {
        public long UploadedBytes { get; set; }
        public long DownloadedBytes { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
    }

    public class InterviewerTrafficUsage
    {
        public IEnumerable<InterviewerMonthlyTrafficUsageView> TrafficUsages { get; set; } = new InterviewerMonthlyTrafficUsageView[0];
        public long TotalTrafficUsed { get; set; }
        public long MaxDailyUsage { get; set; }
    }
    public class InterviewerMonthlyTrafficUsageView
    {
        public string Month  { get; set; }
        public DateTime Date { get; set; }
        public List<InterviewerDailyTrafficUsageView>  DailyUsage  { get; set; }
    }

    public class InterviewerDailyTrafficUsageView
    {
        public int Day { get; set; }
        public long Up { get; set; }
        public long Down { get; set; }
        public long UpInPer { get; set; }
        public long DownInPer { get; set; }
    }
}
