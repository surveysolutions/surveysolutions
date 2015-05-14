using System.Collections.Generic;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views
{
    public class QuantityByResponsibleReportView : IListView<QuantityByInterviewersReportRow>
    {
        public QuantityByResponsibleReportView(
            QuantityByInterviewersReportRow[] items,
            DateTimeRange[] dateTimeRanges, int totalCount)
        {
            Items = items;
            DateTimeRanges = dateTimeRanges;
            TotalCount = totalCount;
        }

        public int TotalCount { get; set; }

        public IEnumerable<QuantityByInterviewersReportRow> Items { get; set; }

        public DateTimeRange[] DateTimeRanges { get; private set; }
    }
}