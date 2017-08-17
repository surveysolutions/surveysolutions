using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reports.Views
{
    public class CountDaysOfInterviewInStatusView
    {
        public CountDaysOfInterviewInStatusRow[] Rows { get; set; }
    }

    public class CountDaysOfInterviewInStatusRow
    {
        public int DaysCount { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int InterviewerAssignedCount { get; set; }
        public int CompletedCount { get; set; }
        public int RejectedBySupervisorCount { get; set; }
        public int ApprovedBySupervisorCount { get; set; }
    }
}