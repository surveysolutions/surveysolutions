using System;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class StatisticsLineGroupedByUserAndTemplate : IView
    {
        public int CreatedCount { get; set; }

        public int SupervisorAssignedCount { get; set; }

        public int InterviewerAssignedCount { get; set; }

        public int SentToCapiCount { get; set; }

        public int CompletedCount { get; set; }

        public int ApprovedBySupervisorCount { get; set; }

        public int RejectedBySupervisorCount { get; set; }

        public int ApprovedByHeadquartersCount { get; set; }

        public int RejectedByHeadquartersCount { get; set; }

        // TODO Remove it
        public int RestoredCount { get; set; }

        public int TotalCount { get; set; }

        public Guid QuestionnaireId { get; set; }

        public string QuestionnaireTitle { get; set; }

        public long QuestionnaireVersion { get; set; }

        public Guid ResponsibleId { get; set; }

        /// <summary>
        /// Name of resposible, which is a supervisor or an interviewer.
        /// </summary>
        public string ResponsibleName { get; set; }

        public Guid? TeamLeadId { get; set; }

        /// <summary>
        /// Name of supervisor (which is a team lead), needed for team-based reports.
        /// </summary>
        public string TeamLeadName { get; set; }
    }
}
