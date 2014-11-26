using System;
using System.Collections.Generic;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncStorage
{
    public interface IChunkReader
    {
        SyncItem ReadChunk(Guid id);

        IEnumerable<SynchronizationChunkMeta> GetChunkMetaDataCreatedAfter(Guid? lastSyncedPackageId, IEnumerable<Guid> users);
        SynchronizationChunkMeta GetChunkMetaDataByTimestamp(DateTime timestamp);
    }
}
