using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Supervisor.Views.Survey;

namespace Core.Supervisor.Views.Reposts.Views
{
    public class HeadquarterSurveysAndStatusesReportView : IListView<HeadquarterSurveysAndStatusesReportLine>
    {
        public int TotalCount { get; set; }
        public IEnumerable<HeadquarterSurveysAndStatusesReportLine> Items { get; set; }
        public IEnumerable<SurveyUsersViewItem> Supervisors { get; set; }
    }
}
