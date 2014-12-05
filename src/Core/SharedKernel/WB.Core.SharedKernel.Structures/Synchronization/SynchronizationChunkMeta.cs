using System;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    public class SynchronizationChunkMeta
    {
        public SynchronizationChunkMeta(string id)
        {
            Id = id;
        }

        public string Id { get; private set; }
    }
}
