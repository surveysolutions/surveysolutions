using System;
using System.Collections.Generic;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization
{
    public interface ISyncManager
    {
        HandshakePackage ItitSync(ClientIdentifier identifier);
        
        bool SendSyncPackage(SyncPackage package);
        bool SendSyncItem(SyncItem package);

        IEnumerable<Guid> GetAllARIds(Guid userId, Guid clientRegistrationKey);
        IEnumerable<KeyValuePair<long,Guid>> GetAllARIdsWithOrder(Guid userId, Guid clientRegistrationKey);

        SyncPackage ReceiveSyncPackage(Guid clientRegistrationId, Guid id, long sequence);

        int GetNumberToGet(Guid userId, Guid clientRegistrationId, long sequence);
        
    }
}
