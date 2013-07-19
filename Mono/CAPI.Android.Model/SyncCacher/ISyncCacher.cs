using System;

namespace CAPI.Android.Core.Model.SyncCacher
{
    public interface ISyncCacher
    {
        bool SaveItem(Guid itemId, string itemContent);

        string LoadItem(Guid itemId);
        
        bool DeleteItem(Guid itemId);
    }
}