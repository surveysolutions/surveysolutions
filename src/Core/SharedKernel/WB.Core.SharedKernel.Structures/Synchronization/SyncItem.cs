using System;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    public class SyncItem
    {
        public Guid Id;

        public string Content;
        public string ItemType;
        public bool IsCompressed;

        public long ChangeTracker = 0;

        public SyncItem()
        {
        }

        public SyncItem(Guid id, string content, string itemType, bool isCompressed, long changeTracker)
        {
            Id = id;
            Content = content;
            ItemType = itemType;
            IsCompressed = isCompressed;
            ChangeTracker = changeTracker;
        }
    }
}
