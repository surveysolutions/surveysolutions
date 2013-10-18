using System;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    public class SynchronizationChunkMeta
    {
        public SynchronizationChunkMeta(Guid id, long sequence)
        {
            Id = id;
            Sequence = sequence;
        }

        public Guid Id { get; private set; }
        public long Sequence { get; private set; }
    }
}
