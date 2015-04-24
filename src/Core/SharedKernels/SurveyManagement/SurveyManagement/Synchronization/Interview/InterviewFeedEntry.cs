using System;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview
{
    public class InterviewFeedEntry : IReadSideRepositoryEntity
    {
        public virtual string EntryId { get; set; }
        public virtual string SupervisorId { get; set; }
        public virtual EntryType EntryType { get; set; }
        public virtual DateTime Timestamp { get; set; }
        public virtual string InterviewId { get; set; }
        public virtual string UserId { get; set; }
        public virtual string InterviewerId { get; set; }
    }
}