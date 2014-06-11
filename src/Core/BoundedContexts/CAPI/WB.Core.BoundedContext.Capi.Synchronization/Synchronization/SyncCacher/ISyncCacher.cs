using System;

namespace WB.Core.BoundedContext.Capi.Synchronization.Synchronization.SyncCacher
{
    public interface ISyncCacher
    {
        bool SaveItem(Guid itemId, string itemContent);

        string LoadItem(Guid itemId);

        bool DoesCachedItemExist(Guid itemId);
        
        bool DeleteItem(Guid itemId);
    }
}