using System;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts
{
    public class PeriodicStatusReportModel
    {
        public PeriodicStatusReportWebApiActionName WebApiActionName { get; set; }
        public bool CanNavigateToQuantityByTeamMember { get; set; }
        public bool CanNavigateToQuantityBySupervisors { get; set; }
        public TemplateViewItem[] Questionnaires { get; set; }
        public string ReportName { get; set; }
        public string ResponsibleColumnName { get; set; }
        public Guid? SupervisorId { get; set; }
        public string SupervisorName { get; set; }
        public PeriodiceReportType[] ReportTypes { get; set; }
        public string ReportNameDescription { get; set; }
        public bool TotalRowPresent { get; set; }
        public DateTime MinAllowedDate { get; set; }
    }
}
