using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Supervisor.Views.Reposts.Views
{
    public class HeadquarterSupervisorsAndStatusesReportView : IListView<HeadquarterSupervisorsAndStatusesReportLine>
    {
        public int TotalCount { get; set; }
        public IEnumerable<HeadquarterSupervisorsAndStatusesReportLine> Items { get; set; }
    }
}
