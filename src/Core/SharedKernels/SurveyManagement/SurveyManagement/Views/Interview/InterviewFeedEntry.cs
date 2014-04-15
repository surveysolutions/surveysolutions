using System;
using WB.Core.Infrastructure.ReadSide.Repository;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewFeedEntry : IReadSideRepositoryEntity
    {
        public string EntryId { get; set; }

        public string SupervisorId { get; set; }

        public EntryType EntryType { get; set; }

        public DateTime Timestamp { get; set; }

        public string InterviewId { get; set; }
    }

    public enum EntryType
    {
        SupervisorAssigned = 1,
        InterviewUnassigned = 2
    }
}