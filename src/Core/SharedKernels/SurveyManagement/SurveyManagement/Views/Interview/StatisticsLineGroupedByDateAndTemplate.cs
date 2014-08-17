using System;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class StatisticsLineGroupedByDateAndTemplate : IView
    {
        public DateTime Date { get; set; }

        public Guid QuestionnaireId { get; set; }

        public string QuestionnaireTitle { get; set; }

        public long QuestionnaireVersion { get; set; }

        public int SupervisorAssignedCount { get; set; }

        public int InterviewerAssignedCount { get; set; }

        public int CompletedCount { get; set; }

        public int ApprovedBySupervisorCount { get; set; }

        public int RejectedBySupervisorCount { get; set; }

        public int ApprovedByHeadquartersCount { get; set; }

        public int RejectedByHeadquartersCount { get; set; }
    }
}