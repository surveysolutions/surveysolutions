using WB.Core.BoundedContexts.Interviewer.Implementation.Authorization;

namespace WB.UI.Interviewer.Syncronization
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
