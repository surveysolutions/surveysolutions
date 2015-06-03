using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views
{
    public class SpeedByResponsibleReportView : IListView<SpeedByResponsibleReportRow>
    {
        public SpeedByResponsibleReportView(
            SpeedByResponsibleReportRow[] items,
            DateTimeRange[] dateTimeRanges, int totalCount)
        {
            Items = items;
            DateTimeRanges = dateTimeRanges;
            TotalCount = totalCount;
        }

        public int TotalCount { get; set; }

        public IEnumerable<SpeedByResponsibleReportRow> Items { get; set; }

        public DateTimeRange[] DateTimeRanges { get; private set; }
    }
}
