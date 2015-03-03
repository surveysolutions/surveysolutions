using System;

namespace WB.Core.Synchronization.SyncStorage
{
    public class UserSyncPackageMeta : IOrderableSyncPackage
    {
        [Obsolete("Probably used for deserialization")]
        public UserSyncPackageMeta()
        {
        }

        public UserSyncPackageMeta(
            Guid userId,
            DateTime timestamp)
        {
            this.UserId = userId;
            this.Timestamp = timestamp;
        }

        public Guid UserId { get; private set; }

        public string PackageId { get; set; }

        public DateTime Timestamp { get; private set; }

        public long SortIndex { get; set; }
    }
}