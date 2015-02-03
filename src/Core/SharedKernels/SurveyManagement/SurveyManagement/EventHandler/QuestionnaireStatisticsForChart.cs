using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class QuestionnaireStatisticsForChart : IView
    {
        public QuestionnaireStatisticsForChart()
        {

        }
        public QuestionnaireStatisticsForChart(QuestionnaireStatisticsForChart statisticsToDublicate)
        {
            this.CreatedCount = statisticsToDublicate.CreatedCount;
            this.SupervisorAssignedCount = statisticsToDublicate.SupervisorAssignedCount;
            this.InterviewerAssignedCount = statisticsToDublicate.InterviewerAssignedCount;
            this.CompletedCount = statisticsToDublicate.CompletedCount;
            this.ApprovedBySupervisorCount = statisticsToDublicate.ApprovedBySupervisorCount;
            this.RejectedBySupervisorCount = statisticsToDublicate.RejectedBySupervisorCount;
            this.ApprovedByHeadquartersCount = statisticsToDublicate.ApprovedByHeadquartersCount;
            this.RejectedByHeadquartersCount = statisticsToDublicate.RejectedByHeadquartersCount;
            this.OtherStatusesCount = statisticsToDublicate.OtherStatusesCount;
        }

        public int CreatedCount { get; set; }
        public int SupervisorAssignedCount { get; set; }
        public int InterviewerAssignedCount { get; set; }
        public int CompletedCount { get; set; }
        public int ApprovedBySupervisorCount { get; set; }
        public int RejectedBySupervisorCount { get; set; }
        public int ApprovedByHeadquartersCount { get; set; }
        public int RejectedByHeadquartersCount { get; set; }
        public int OtherStatusesCount { get; set; }
    }
}