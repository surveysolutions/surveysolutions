using System;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    public class SyncItem
    {
        public Guid RootId
        {
            get
            {
                return this.rootId == Guid.Empty ? this.Id : this.rootId;
            }
            set
            {
                this.rootId = value;
            }
        }

        [Obsolete("Remove when all clients are updated to 3.1 version")]
        public Guid Id;

        public string Content;
        public string ItemType;
        public bool IsCompressed;

        public long ChangeTracker = 0;

        public string MetaInfo;
        private Guid rootId;
    }
}
