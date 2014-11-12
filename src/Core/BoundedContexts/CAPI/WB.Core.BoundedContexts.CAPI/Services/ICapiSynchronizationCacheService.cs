using System;

namespace WB.Core.BoundedContexts.Capi.Services
{
    public interface ICapiSynchronizationCacheService
    {
        bool SaveItem(Guid itemId, string itemContent);

        string LoadItem(Guid itemId);

        bool DoesCachedItemExist(Guid itemId);
        
        bool DeleteItem(Guid itemId);
    }
}