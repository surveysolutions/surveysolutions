namespace Main.Synchronization.Credentials
{
    public interface ISyncAuthenticator
    {
        bool ValidateUser();

        event  RequestCredentialsCallBack RequestCredentials;
    }

    public delegate SyncCredentials? RequestCredentialsCallBack(object sender);

    public struct SyncCredentials
    {
        public SyncCredentials(string login, string password) : this()
        {
            Login = login;
            Password = password;
        }

        public string Login { get; private set; }
        public string Password { get; private set; }
    }
}