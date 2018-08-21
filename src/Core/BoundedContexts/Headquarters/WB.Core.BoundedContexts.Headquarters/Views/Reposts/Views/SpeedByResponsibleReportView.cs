using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Resources;

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
            this.TotalRow = new SpeedByResponsibleTotalRow();
        }

        public int TotalCount { get; set; }

        public IEnumerable<SpeedByResponsibleReportRow> Items { get; set; }

        public SpeedByResponsibleTotalRow TotalRow
        {
            get;
            set;
        }

        public DateTimeRange[] DateTimeRanges { get; private set; }
    }

    public class SpeedByResponsibleTotalRow
    {
        public SpeedByResponsibleTotalRow()
        {
            this.SpeedByPeriod = new List<double?>();
        }

        public string ResponsibleName => Strings.Average;

        public double? Total => null;
        public double? Average => null;

        public List<double?> SpeedByPeriod { get; }
    }
}
