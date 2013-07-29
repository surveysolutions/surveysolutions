namespace WB.Core.Infrastructure.Raven
{
    public class RavenConnectionSettings
    {
        public RavenConnectionSettings(bool isEmbedded, string username, string password, string storagePath, string defaultDatabase)
        {
            this.IsEmbedded = isEmbedded;
            this.Username = username;
            this.Password = password;
            this.StoragePath = storagePath;
            this.DefaultDatabase = defaultDatabase;
        }

        public bool IsEmbedded { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string StoragePath { get; private set; }
        public string DefaultDatabase { get; private set; }
    }
}