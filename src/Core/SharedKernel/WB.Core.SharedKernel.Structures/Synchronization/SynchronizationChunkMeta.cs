using System;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    public class SynchronizationChunkMeta
    {
        public SynchronizationChunkMeta() { }
        public SynchronizationChunkMeta(string id, long sortIndex, Guid? userId, string itemType)
        {
            Id = id;
            SortIndex = sortIndex;
            UserId = userId;
            ItemType = itemType;
        }

        public string Id { get; set; }
        public long SortIndex { get; set; }
        public Guid? UserId { get; set; }
        public string ItemType { get; set; }
        public Guid InterviewId { get; set; }
    }
}
