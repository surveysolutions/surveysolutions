using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views
{
    public class ReportLineCounters {
        
        public int SupervisorAssignedCount { get; set; }
        public int InterviewerAssignedCount { get; set; }
        public int CompletedCount { get; set; }
        public int ApprovedBySupervisorCount { get; set; }
        public int RejectedBySupervisorCount { get; set; }

        public int ApprovedByHeadquartersCount { get; set; }
        public int RejectedByHeadquartersCount { get; set; }

        public int TotalCount { get; set; }

        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }

        public Guid ResponsibleId { get; set; }
    }
}