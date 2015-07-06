using System;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Users;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public class LocalUserChangedFeedEntry : UserChangedFeedEntry
    {
        protected LocalUserChangedFeedEntry()
        {
            
        }

        public LocalUserChangedFeedEntry(string supervisorId, string entryId, UserFeedEntryType entryType)
            : base(supervisorId, entryId, entryType) { }

        public virtual bool IsProcessed { get; set; }

        public virtual Uri UserDetailsUri { get; set; }

        public virtual bool ProcessedWithError { get; set; }
    }
}