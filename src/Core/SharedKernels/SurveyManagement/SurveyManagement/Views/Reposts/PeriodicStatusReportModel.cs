using System;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts
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
        public PeriodiceReportType[] ReportTypes { get; set; }
        public string ReportNameDescription { get; set; }
    }
}
