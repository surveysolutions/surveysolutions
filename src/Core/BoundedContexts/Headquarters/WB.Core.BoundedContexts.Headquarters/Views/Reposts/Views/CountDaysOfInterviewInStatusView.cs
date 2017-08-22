using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reports.Views
{
    public class CountDaysOfInterviewInStatusView
    {
        public CountDaysOfInterviewInStatusRow[] Rows { get; set; }
    }

    public class CountDaysOfInterviewInStatusRow
    {
        public int DaysCountStart { get; set; }
        public int? DaysCountEnd { get; set; }

        public string RowHeader { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int SupervisorAssignedCount { get; set; }
        public int InterviewerAssignedCount { get; set; }
        public int CompletedCount { get; set; }
        public int RejectedBySupervisorCount { get; set; }
        public int ApprovedBySupervisorCount { get; set; }
        public int RejectedByHeadquartersCount { get; set; }
        public int ApprovedByHeadquartersCount { get; set; }

        public int TotalCount => InterviewerAssignedCount + SupervisorAssignedCount +
                                 CompletedCount + ApprovedBySupervisorCount + RejectedBySupervisorCount +
                                 ApprovedByHeadquartersCount + RejectedByHeadquartersCount;
    }
}