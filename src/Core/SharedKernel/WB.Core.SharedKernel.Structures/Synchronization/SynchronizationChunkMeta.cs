using System;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    public class SynchronizationChunkMeta
    {
        public SynchronizationChunkMeta(string id, long sortIndex, Guid? userId, string itemType)
        {
            Id = id;
            SortIndex = sortIndex;
            UserId = userId;
            ItemType = itemType;
        }

        public string Id { get; private set; }
        public long SortIndex { get; private set; }
        public Guid? UserId { get; private set; }
        public string ItemType { get; private set; }
    }
}
