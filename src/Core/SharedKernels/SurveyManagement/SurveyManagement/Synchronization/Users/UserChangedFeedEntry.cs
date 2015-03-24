using System;
using System.Diagnostics;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.SharedKernels.SurveyManagement.Synchronization.Users
{
    [DebuggerDisplay("EntryId = {EntryId}; Timestamp = {Timestamp}")]
    public class UserChangedFeedEntry : IReadSideRepositoryEntity
    {
        protected UserChangedFeedEntry()
        {
        }

        public UserChangedFeedEntry(string supervisorId, string entryId)
        {
            this.SupervisorId = supervisorId;
            this.EntryId = entryId;
        }

        public virtual string ChangedUserId { get; set; }

        public virtual DateTime Timestamp { get; set; }

        public virtual string SupervisorId { get; set; }

        public virtual string EntryId { get; protected set; }
    }
}