using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views
{
    public class TeamsAndStatusesReportView : IListView<TeamsAndStatusesReportLine>
    {
        public int TotalCount { get; set; }
        public IEnumerable<TeamsAndStatusesReportLine> Items { get; set; }
        public TeamsAndStatusesReportLine TotalRow { get; set; }
    }
}
