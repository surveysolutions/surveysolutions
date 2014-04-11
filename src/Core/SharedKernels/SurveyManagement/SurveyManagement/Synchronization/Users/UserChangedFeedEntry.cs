using System;
using System.Diagnostics;

namespace WB.Core.SharedKernels.SurveyManagement.Synchronization.Users
{
    [DebuggerDisplay("EntryId = {EntryId}; Timestamp = {Timestamp}")]
    public class UserChangedFeedEntry : SupervisorFeedEntry
    {
        public UserChangedFeedEntry(string supervisorId, string entryId)
            : base(supervisorId, entryId) {}

        public string ChangedUserId { get; set; }

        public DateTime Timestamp { get; set; }
    }
}