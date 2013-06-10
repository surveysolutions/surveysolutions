namespace WB.Core.Synchronization.SyncManager
{
    using System;

    public interface ISyncManager
    {
        HandshakePackage ItitSync(ClientIdentifier identifier);

        bool InitSending(ClientIdentifier identifier);

        bool ItitReceiving(ClientIdentifier identifier);

        bool SendSyncPackage(SyncPackage package);

        SyncPackage ReceiveSyncPackage(ClientIdentifier identifier, Guid id, string itemType);
    }
}
