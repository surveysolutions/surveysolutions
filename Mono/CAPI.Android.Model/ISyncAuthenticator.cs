using CAPI.Android.Core.Model.Authorization;

namespace CAPI.Android.Core.Model
{
    public interface ISyncAuthenticator
    {
        SyncCredentials? RequestCredentials();

        event RequestCredentialsCallBack RequestCredentialsCallback;
    }

    public delegate SyncCredentials? RequestCredentialsCallBack(object sender);


}