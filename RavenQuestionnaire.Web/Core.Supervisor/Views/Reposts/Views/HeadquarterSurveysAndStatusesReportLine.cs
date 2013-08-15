using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Supervisor.Views.Reposts.Views
{
    public class HeadquarterSurveysAndStatusesReportLine
    {
        public int CreatedCount { get; set; }

        public int SupervisorAssignedCount { get; set; }

        public int InterviewerAssignedCount { get; set; }

        public int SentToCapiCount { get; set; }

        public int CompletedCount { get; set; }

        public int ApprovedBySupervisorCount { get; set; }

        public int RejectedBySupervisorCount { get; set; }

        public int RestoredCount { get; set; }

        public int TotalCount { get; set; }

        public Guid QuestionnaireId { get; set; }

        public string QuestionnaireTitle { get; set; }

        public long QuestionnaireVersion { get; set; }

        public Guid ResponsibleId { get; set; }
    }
}
