namespace WB.Core.Synchronization.SyncProvider
{
    using System;

    public interface ISyncProvider
    {
        SyncItem GetSyncItem(Guid id, string type);
    }
}
