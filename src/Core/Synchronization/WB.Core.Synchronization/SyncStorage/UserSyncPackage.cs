using System;
using WB.Core.GenericSubdomains.Utils;

namespace WB.Core.Synchronization.SyncStorage
{
    public class UserSyncPackage : ISyncPackage
    {
        [Obsolete("Probably used for deserialization")]
        public UserSyncPackage()
        {
        }

        public UserSyncPackage(
            Guid userId,
            string content,
            DateTime timestamp,
            int sortIndex)
        {
            this.UserId = userId;
            this.PackageId = userId.FormatGuid() + "$" + sortIndex;
            this.Content = content;
            this.Timestamp = timestamp;
            this.SortIndex = sortIndex;
        }

        public Guid UserId { get; private set; }

        public string PackageId { get; private set; }

        public DateTime Timestamp { get; private set; }

        public int SortIndex { get; private set; }

        public string Content { get; private set; }
    }
}