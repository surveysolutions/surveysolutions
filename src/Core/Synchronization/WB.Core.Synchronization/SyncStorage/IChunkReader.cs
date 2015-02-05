using System;
using System.Collections.Generic;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncStorage
{
    public interface IChunkReader
    {
        SyncItem ReadChunk(string id);

        IEnumerable<SynchronizationChunkMeta> GetChunkMetaDataCreatedAfter(string lastSyncedPackageId, IEnumerable<Guid> users);
        SynchronizationChunkMeta GetChunkMetaDataByTimestamp(DateTime timestamp, IEnumerable<Guid> userId);
        int GetNumberOfSyncPackagesWithBigSize();
    }
}
