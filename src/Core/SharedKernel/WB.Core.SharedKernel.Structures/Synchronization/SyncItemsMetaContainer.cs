using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    public class SyncItemsMetaContainer : BasePackage
    {
        public SyncItemsMetaContainer()
        {
            ChunksMeta = new List<SynchronizationChunkMeta>();
        }

        public List<SynchronizationChunkMeta> ChunksMeta { set; get; }

    }

}