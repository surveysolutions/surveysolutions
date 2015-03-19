using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views
{
    public class SurveysAndStatusesReportView : IListView<HeadquarterSurveysAndStatusesReportLine>
    {
        public int TotalCount { get; set; }
        public IEnumerable<HeadquarterSurveysAndStatusesReportLine> Items { get; set; }
    }
}
