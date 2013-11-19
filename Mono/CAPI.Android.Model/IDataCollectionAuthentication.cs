using CAPI.Android.Core.Model.Authorization;
using WB.Core.BoundedContexts.Capi;

namespace CAPI.Android.Core.Model
{
    public interface IDataCollectionAuthentication : IAuthentication
    {
        SyncCredentials? RequestSyncCredentials();
    }
}