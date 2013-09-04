using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Supervisor.Views.Reposts.Views
{
    public class SupervisorSurveysAndStatusesReportView : IListView<SupervisorSurveysAndStatusesReportLine>
    {
        public int TotalCount { get; set; }
        public IEnumerable<SupervisorSurveysAndStatusesReportLine> Items { get; set; }
    }
}
