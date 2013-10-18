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

        IEnumerable<SynchronizationChunkMeta> GetAllARIdsWithOrder(Guid userId, Guid clientRegistrationKey, long clientSequence);

        SyncPackage ReceiveSyncPackage(Guid clientRegistrationId, Guid id, long sequence);

        int GetNumberToGet(Guid userId, Guid clientRegistrationId, long sequence);
        
    }
}
