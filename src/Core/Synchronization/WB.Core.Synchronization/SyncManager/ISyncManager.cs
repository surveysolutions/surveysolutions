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

        bool InitReceiving(ClientIdentifier identifier);

        bool SendSyncPackage(SyncPackage package);

        bool SendSyncItem(SyncItem package);

        HandshakePackage CheckAndCreateNewProcess(ClientIdentifier clientIdentifier);
        
        IEnumerable<Guid> GetAllARIds(Guid userId, Guid clientRegistrationKey);

        SyncPackage ReceiveSyncPackage(ClientIdentifier identifier, Guid id);
    }
}
