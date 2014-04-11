using System;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Users;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public class LocalUserChangedFeedEntry : UserChangedFeedEntry
    {
        public LocalUserChangedFeedEntry(string supervisorId, string entryId)
            : base(supervisorId, entryId) {}

        public bool IsProcessed { get; set; }

        public Uri UserDetailsUri { get; set; }
        public bool ProcessedWithError { get; set; }
    }
}