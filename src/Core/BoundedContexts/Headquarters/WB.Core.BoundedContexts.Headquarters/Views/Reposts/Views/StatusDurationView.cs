using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reports.Views
{
    public class StatusDurationRow
    {
        public int DaysCountStart { get; set; }
        public int? DaysCountEnd { get; set; }

        public string RowHeader { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public long SupervisorAssignedCount { get; set; }
        public long InterviewerAssignedCount { get; set; }
        public long CompletedCount { get; set; }
        public long RejectedBySupervisorCount { get; set; }
        public long ApprovedBySupervisorCount { get; set; }
        public long RejectedByHeadquartersCount { get; set; }
        public long ApprovedByHeadquartersCount { get; set; }

        public long TotalCount => InterviewerAssignedCount + SupervisorAssignedCount +
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
