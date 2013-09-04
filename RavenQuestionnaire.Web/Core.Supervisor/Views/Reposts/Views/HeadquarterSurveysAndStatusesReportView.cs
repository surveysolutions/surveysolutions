using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Supervisor.Views.Reposts.Views
{
    public class HeadquarterSurveysAndStatusesReportView : IListView<HeadquarterSurveysAndStatusesReportLine>
    {
        public int TotalCount { get; set; }
        public IEnumerable<HeadquarterSurveysAndStatusesReportLine> Items { get; set; }
    }
}
