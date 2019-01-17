using System;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reports.Factories
{
    public class SpeedReportInterviewItem : IReadSideRepositoryEntity
    {
        public virtual string InterviewId { get; set; }
        public virtual Guid QuestionnaireId { get; set; }
        public virtual long QuestionnaireVersion { get; set; }

        public virtual DateTime CreatedDate { get; set; }
        public virtual DateTime? FirstAnswerDate { get; set; }
        public virtual Guid? InterviewerId { get; set; }
        public virtual string InterviewerName { get; set; }
        public virtual Guid? SupervisorId { get; set; }
        public virtual string SupervisorName { get; set; }

        public virtual InterviewSummary InterviewSummary { get; set; }
    }
}
