namespace WB.Core.BoundedContexts.Capi.Implementation.Authorization
{
    public struct SyncCredentials
    {
        public SyncCredentials(string login, string password)
            : this()
        {
            this.Login = login;
            this.Password = password;
        }

        public string Login { get; private set; }
        public string Password { get; private set; }
    }
}