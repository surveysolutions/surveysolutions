using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views
{
    public class QuantityByResponsibleReportView : IListView<QuantityByResponsibleReportRow>
    {
        public QuantityByResponsibleReportView(
            IEnumerable<QuantityByResponsibleReportRow> items,
            QuantityTotalRow totalRow,
            DateTimeRange[] dateTimeRanges, int totalCount)
        {
            this.Items = items;
            this.TotalRow = totalRow;
            this.DateTimeRanges = dateTimeRanges;
            this.TotalCount = totalCount;
        }

        public int TotalCount { get; set; }

        public IEnumerable<QuantityByResponsibleReportRow> Items { get; set; }

        public QuantityTotalRow TotalRow { get; set; }

        public DateTimeRange[] DateTimeRanges { get; private set; }
    }
}