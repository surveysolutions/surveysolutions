using System;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    public class SynchronizationChunkMeta
    {
        public SynchronizationChunkMeta(Guid id, long timestamp)
        {
            Id = id;
            Timestamp = timestamp;
        }

        public Guid Id { get; private set; }
        public long Timestamp { get; private set; }
    }
}
