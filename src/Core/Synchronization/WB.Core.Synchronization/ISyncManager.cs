using System;
using System.Collections.Generic;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization
{
    public interface ISyncManager
    {
        HandshakePackage ItitSync(ClientIdentifier identifier);
        
        bool SendSyncPackage(SyncPackage package);
        bool SendSyncItem(SyncItem package);

        IEnumerable<SynchronizationChunkMeta> GetAllARIdsWithOrder(Guid userId, Guid clientRegistrationKey, DateTime timestamp);

        SyncPackage ReceiveSyncPackage(Guid clientRegistrationId, Guid id, DateTime timestamp);

        int GetNumberToGet(Guid userId, Guid clientRegistrationId, DateTime timestamp);
        
    }
}
