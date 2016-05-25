using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views
{
    public class SpeedByResponsibleReportView : IListView<SpeedByResponsibleReportRow>
    {
        public SpeedByResponsibleReportView(
            SpeedByResponsibleReportRow[] items,
            DateTimeRange[] dateTimeRanges, int totalCount)
        {
            this.Items = items;
            this.DateTimeRanges = dateTimeRanges;
            this.TotalCount = totalCount;
        }

        public int TotalCount { get; set; }

        public IEnumerable<SpeedByResponsibleReportRow> Items { get; set; }

        public DateTimeRange[] DateTimeRanges { get; private set; }
    }
}
