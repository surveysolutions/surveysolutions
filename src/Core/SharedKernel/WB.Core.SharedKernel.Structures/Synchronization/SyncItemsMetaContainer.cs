using System.Collections.Generic;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    public class SyncItemsMetaContainer
    {
        public IEnumerable<SynchronizationChunkMeta> SyncPackagesMeta { get; set; }
    }
}