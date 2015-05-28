using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts
{
    public class PeriodicStatusReportModel
    {
        public string WebApiActionName { get; set; }
        public bool CanNavigateToQuantityByTeamMember { get; set; }
        public bool CanNavigateToQuantityBySupervisors { get; set; }
        public TemplateViewItem[] Questionnaires { get; set; }
        public string ReportTitle { get; set; }
        public string ReportName { get; set; }
        public string ResponsibleColumnName { get; set; }
    }
}
