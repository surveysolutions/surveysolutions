namespace WB.Core.Synchronization.SyncManager
{
    using System;

    public interface ISyncManager
    {
        bool InitSending(ClientIdentifier identifier);

        bool ItitReceiving(ClientIdentifier identifier);

        void SendSyncPackage();

        SyncPackage ReceiveSyncPackage(ClientIdentifier identifier, Guid id, string itemType);
    }
}
