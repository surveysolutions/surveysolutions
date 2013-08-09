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

        public string MetaInfo;

        public SyncItem()
        {
        }
        
    }
}
