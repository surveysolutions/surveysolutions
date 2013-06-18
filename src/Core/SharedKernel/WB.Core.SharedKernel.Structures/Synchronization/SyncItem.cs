using System;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    public class SyncItem
    {
        public Guid Id;

        public string Content;

        public string ItemType;

        public bool IsCompressed;

        public DateTime LastChangeDate;


        public SyncItem(Guid id, string content, string itemType, bool isCompressed, DateTime lastChangedDate)
        {
            Id = id;
            Content = content;
            ItemType = itemType;
            IsCompressed = isCompressed;
            LastChangeDate = lastChangedDate;
        }
    }
}
