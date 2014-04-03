using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views
{
    public class SupervisorTeamMembersAndStatusesReportView : IListView<SupervisorTeamMembersAndStatusesReportLine>
    {
        public int TotalCount { get; set; }
        public IEnumerable<SupervisorTeamMembersAndStatusesReportLine> Items { get; set; }
    }
}
