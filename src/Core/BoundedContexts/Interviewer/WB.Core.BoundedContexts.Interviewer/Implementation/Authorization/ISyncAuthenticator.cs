namespace WB.Core.BoundedContexts.Interviewer.Implementation.Authorization
{
    public interface ISyncAuthenticator
    {
        SyncCredentials? RequestCredentials();

        event RequestCredentialsCallBack RequestCredentialsCallback;
    }

    public delegate SyncCredentials? RequestCredentialsCallBack(object sender);
}