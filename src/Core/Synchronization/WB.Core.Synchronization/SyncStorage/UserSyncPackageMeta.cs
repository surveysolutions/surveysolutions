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

        public virtual Guid UserId { get; protected set; }

        public virtual string PackageId { get; set; }

        public virtual DateTime Timestamp { get; protected set; }

        public virtual long SortIndex { get; set; }
    }
}