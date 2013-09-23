using System.Collections.Generic;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization.SyncProvider
{
    using System;

    internal interface ISyncProvider
    {
        SyncItem GetSyncItem(Guid syncId, Guid id, long sequence);
        
        IEnumerable<SyncItem> GetSyncItemBulk(Guid userId, Guid clientRegistrationKey, long sequence);

        IEnumerable<SynchronizationChunkMeta> GetAllARIdsWithOrder(Guid userId, Guid clientRegistrationKey, long clientSequence);

        HandshakePackage CheckAndCreateNewSyncActivity(ClientIdentifier identifier);

        bool HandleSyncItem(SyncItem item, Guid syncActivityId);
    }
}
