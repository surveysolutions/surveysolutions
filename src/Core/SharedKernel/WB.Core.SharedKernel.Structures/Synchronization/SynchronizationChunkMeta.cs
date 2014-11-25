using System;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    public class SynchronizationChunkMeta
    {
        public SynchronizationChunkMeta(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; private set; }
    }
}
