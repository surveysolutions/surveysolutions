using CAPI.Android.Core.Model;

using WB.Core.BoundedContexts.Capi.Implementation.Authorization;

namespace WB.UI.Capi.Syncronization
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
