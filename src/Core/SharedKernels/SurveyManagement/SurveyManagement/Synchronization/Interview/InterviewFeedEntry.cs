using System;
using WB.Core.Infrastructure.ReadSide.Repository;

namespace WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview
{
    public class InterviewFeedEntry : IReadSideRepositoryEntity
    {
        public string EntryId { get; set; }

        public string SupervisorId { get; set; }

        public EntryType EntryType { get; set; }

        public DateTime Timestamp { get; set; }

        public string InterviewId { get; set; }

        public string UserId { get; set; }

        public string InterviewerId { get; set; }
    }
}