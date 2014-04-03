using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views
{
    public class HeadquarterSupervisorsAndStatusesReportView : IListView<HeadquarterSupervisorsAndStatusesReportLine>
    {
        public int TotalCount { get; set; }
        public IEnumerable<HeadquarterSupervisorsAndStatusesReportLine> Items { get; set; }
    }
}
