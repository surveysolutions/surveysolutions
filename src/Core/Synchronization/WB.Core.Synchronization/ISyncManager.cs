using System;
using System.Collections.Generic;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization
{
    public interface ISyncManager
    {
        HandshakePackage ItitSync(ClientIdentifier identifier);

        void SendSyncItem(string package);

        IEnumerable<SynchronizationChunkMeta> GetAllARIdsWithOrder(Guid userId, Guid clientRegistrationKey, string lastSyncedPackageId);

        SyncPackage ReceiveSyncPackage(Guid clientRegistrationId, string id);

        string GetPackageIdByTimestamp(Guid userId, DateTime timestamp);

        void LinkUserToDevice(Guid interviewerId, string deviceId);
    }
}
