namespace WB.Core.Synchronization.SyncProvider
{
    using System;

    public interface ISyncProvider
    {
        SyncItem GetSyncItem(Guid id, string type);

        Guid CheckAndCreateNewSyncActivity(ClientIdentifier identifier);

        bool HandleSyncItem(SyncItem item);
    }
}
