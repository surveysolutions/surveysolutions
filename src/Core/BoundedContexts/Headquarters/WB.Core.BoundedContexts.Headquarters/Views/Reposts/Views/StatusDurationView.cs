using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reports.Views
{
    public class StatusDurationRow
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

    public class StatusDurationView : IListView<StatusDurationRow>
    {
        public StatusDurationView()
        {
            this.Items = new List<StatusDurationRow>();
        }

        public int TotalCount { get; set; }
        public IEnumerable<StatusDurationRow> Items { get; set; }

        public StatusDurationRow TotalRow { get; set; }
    }
}