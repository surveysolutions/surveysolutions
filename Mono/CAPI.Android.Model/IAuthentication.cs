using CAPI.Android.Core.Model.Authorization;
using Main.Core.Entities.SubEntities;

namespace CAPI.Android.Core.Model
{
    public interface IAuthentication
    {
        UserLight CurrentUser { get; }
        bool IsLoggedIn { get; }
        bool LogOn(string userName, string password);
        void LogOff();
        SyncCredentials? RequestSyncCredentials();
    }
}