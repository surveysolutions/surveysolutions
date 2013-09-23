using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Supervisor.Views.Reposts.Views
{
    public class HeadquarterSurveysAndStatusesReportLine : ReportLineCounters
    {
        public int CreatedCount { get; set; }

        public string QuestionnaireTitle { get; set; }
    }
}
