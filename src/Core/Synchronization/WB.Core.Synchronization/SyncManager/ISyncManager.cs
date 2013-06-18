using System.Collections.Generic;
using Main.Core.Events;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncManager
{
    using System;

    public interface ISyncManager
    {
        HandshakePackage ItitSync(ClientIdentifier identifier);

        bool InitSending(ClientIdentifier identifier);

        bool ItitReceiving(ClientIdentifier identifier);

        bool SendSyncPackage(SyncPackage package);

        bool SendSyncItem(SyncItem package);

        IEnumerable<Guid> GetAllARIds(Guid userId);

        SyncPackage ReceiveSyncPackage(ClientIdentifier identifier, Guid id, Guid userId);
    }
}
