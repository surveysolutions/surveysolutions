using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Supervisor.Views.Reposts.Views
{
    public class SupervisorTeamMembersAndStatusesReportView : IListView<SupervisorTeamMembersAndStatusesReportLine>
    {
        public int TotalCount { get; set; }
        public IEnumerable<SupervisorTeamMembersAndStatusesReportLine> Items { get; set; }
    }
}
