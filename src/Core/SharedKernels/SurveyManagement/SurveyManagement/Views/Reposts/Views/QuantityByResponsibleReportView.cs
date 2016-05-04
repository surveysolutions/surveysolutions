using System.Collections.Generic;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views
{
    public class QuantityByResponsibleReportView : IListView<QuantityByResponsibleReportRow>
    {
        public QuantityByResponsibleReportView(
            QuantityByResponsibleReportRow[] items,
            QuantityTotalRow totalRow,
            DateTimeRange[] dateTimeRanges, int totalCount)
        {
            Items = items;
            TotalRow = totalRow;
            DateTimeRanges = dateTimeRanges;
            TotalCount = totalCount;
        }

        public int TotalCount { get; set; }

        public IEnumerable<QuantityByResponsibleReportRow> Items { get; set; }

        public QuantityTotalRow TotalRow { get; set; }

        public DateTimeRange[] DateTimeRanges { get; private set; }
    }
}