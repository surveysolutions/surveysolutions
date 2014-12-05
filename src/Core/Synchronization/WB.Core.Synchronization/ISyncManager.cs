using System;
using System.Collections.Generic;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization
{
    public interface ISyncManager
    {
        HandshakePackage ItitSync(ClientIdentifier identifier);

        bool SendSyncItem(SyncItem package);

        IEnumerable<SynchronizationChunkMeta> GetAllARIdsWithOrder(Guid userId, Guid clientRegistrationKey, string lastSyncedPackageId);

        SyncPackage ReceiveSyncPackage(Guid clientRegistrationId, string id);
        string GetPackageIdByTimestamp(DateTime timestamp);
    }
}
