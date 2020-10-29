using System;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserProfile
{
    public class SyncStats
    {
        public SyncStats(int successSynchronizationsCount, int failedSynchronizationsCount, DateTime? lastSynchronizationDate)
        {
            SuccessSynchronizationsCount = successSynchronizationsCount;
            FailedSynchronizationsCount = failedSynchronizationsCount;
            LastSynchronizationDate = lastSynchronizationDate;
        }

        public int SuccessSynchronizationsCount { get; }
        public int FailedSynchronizationsCount { get; }

        public DateTime? LastSynchronizationDate { get; }
    }
}
