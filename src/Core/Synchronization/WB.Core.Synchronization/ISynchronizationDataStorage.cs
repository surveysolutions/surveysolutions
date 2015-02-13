using System;
using System.Collections.Generic;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization
{
    public interface ISynchronizationDataStorage
    {
        SyncItem GetLatestVersion(string id);
        IEnumerable<SynchronizationChunkMeta> GetChunkPairsCreatedAfter(string lastSyncedPackageId, Guid userId);
        SynchronizationChunkMeta GetChunkInfoByTimestamp(DateTime timestamp, Guid userId);
    }
}