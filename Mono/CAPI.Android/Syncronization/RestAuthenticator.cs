using System.Net;
using System.Security.Authentication;
using Main.Core.Utility;
using Main.Synchronization.Credentials;
using RestSharp;

namespace CAPI.Android.Syncronization
{
    public class RestAuthenticator : ISyncAuthenticator
    {

        public event RequestCredentialsCallBack RequestCredentialsCallback;

        protected SyncCredentials? OnRequestCredentials()
        {
            var handler = RequestCredentialsCallback;
            if (handler == null)
                return null;
            return handler(this);
        }

        public SyncCredentials RequestCredentials()
        {
            var credentials = OnRequestCredentials();
            if (!credentials.HasValue)
                throw new AuthenticationException("User wasn't authenticated");
            return credentials.Value;
        }

    }
}
