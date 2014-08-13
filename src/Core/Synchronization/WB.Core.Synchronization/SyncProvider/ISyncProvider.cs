using System.Collections.Generic;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization.SyncProvider
{
    using System;

    internal interface ISyncProvider
    {
        SyncItem GetSyncItem(Guid syncId, Guid id, DateTime timestamp);

        IEnumerable<SyncItem> GetSyncItemBulk(Guid userId, Guid clientRegistrationKey, DateTime timestamp);

        IEnumerable<SynchronizationChunkMeta> GetAllARIdsWithOrder(Guid userId, Guid clientRegistrationKey, DateTime timestamp);

        HandshakePackage CheckAndCreateNewSyncActivity(ClientIdentifier identifier);

        bool HandleSyncItem(SyncItem item, Guid syncActivityId);
    }
}
