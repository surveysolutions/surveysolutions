using CAPI.Android.Core.Model;
using CAPI.Android.Core.Model.Authorization;

namespace WB.UI.Capi.DataCollection.Syncronization
{
    public class RestAuthenticator : ISyncAuthenticator
    {

        public event RequestCredentialsCallBack RequestCredentialsCallback;

        protected SyncCredentials? OnRequestCredentials()
        {
            var handler = this.RequestCredentialsCallback;
            if (handler == null)
                return null;
            return handler(this);
        }

        public SyncCredentials? RequestCredentials()
        {
            var credentials = this.OnRequestCredentials();

            return credentials;
        }

    }
}
