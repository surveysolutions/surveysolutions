using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views
{
    public class SurveysAndStatusesReportView : IListView<HeadquarterSurveysAndStatusesReportLine>
    {
        public int TotalCount { get; set; }
        public IEnumerable<HeadquarterSurveysAndStatusesReportLine> Items { get; set; }
    }
}
