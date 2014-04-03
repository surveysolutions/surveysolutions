using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views
{
    public class SupervisorSurveysAndStatusesReportView : IListView<SupervisorSurveysAndStatusesReportLine>
    {
        public int TotalCount { get; set; }
        public IEnumerable<SupervisorSurveysAndStatusesReportLine> Items { get; set; }
    }
}
