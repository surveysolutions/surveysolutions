namespace CAPI.Android.Core.Model.Authorization
{
    public struct SyncCredentials
    {
        public SyncCredentials(string login, string password)
            : this()
        {
            Login = login;
            Password = password;
        }

        public string Login { get; private set; }
        public string Password { get; private set; }
    }
}