using System;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts
{
    public class PeriodicStatusReportModel
    {
        public string DataUrl { get; set; }
        public bool CanNavigateToQuantityByTeamMember { get; set; }
        public bool CanNavigateToQuantityBySupervisors { get; set; }
        public QuestionnaireVersionsComboboxViewItem[] Questionnaires { get; set; }
        public string ReportName { get; set; }
        public string ResponsibleColumnName { get; set; }
        public Guid? SupervisorId { get; set; }
        public string SupervisorName { get; set; }
        public ComboboxViewItem[] ReportTypes { get; set; }
        public string ReportNameDescription { get; set; }
        public bool TotalRowPresent { get; set; }
        public string MinAllowedDate { get; set; }

        public string ReportGroupName { get; set; }
        public string SupervisorsUrl { get; set; }
        public string InterviewersUrl { get; set; }
        public ComboboxViewItem[] Periods { get; set; }
        public ComboboxViewItem[] OverTheLasts { get; set; }
    }
}
