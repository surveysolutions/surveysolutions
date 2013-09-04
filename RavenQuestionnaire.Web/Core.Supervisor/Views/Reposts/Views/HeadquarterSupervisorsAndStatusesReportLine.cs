using System;

namespace Core.Supervisor.Views.Reposts.Views
{
    public class HeadquarterSupervisorsAndStatusesReportLine : ReportLineCounters
    {
        public int CreatedCount { get; set; }

        public string ResponsibleName { get; set; }
    }
}