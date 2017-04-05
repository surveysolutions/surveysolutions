using System;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    [Obsolete("Since v 5.8")]
    public class SyncItem
    {
        public Guid RootId { get; set; }

        public string Content;
        public string ItemType;
        public bool IsCompressed;

        public long ChangeTracker = 0;

        public string MetaInfo;
    }
}
