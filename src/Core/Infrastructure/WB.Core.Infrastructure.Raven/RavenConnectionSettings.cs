namespace WB.Core.Infrastructure.Raven
{
    public class RavenConnectionSettings
    {
        public RavenConnectionSettings(string storagePath, bool isEmbedded = false,
            string username = null, string password = null, string defaultDatabase = null)
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