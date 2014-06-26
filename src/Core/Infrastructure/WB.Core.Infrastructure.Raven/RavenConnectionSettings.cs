namespace WB.Core.Infrastructure.Raven
{
    public class RavenConnectionSettings
    {
        public RavenConnectionSettings(string storagePath, bool isEmbedded = false,
            string username = null, string password = null, string eventsDatabase = "Events", string viewsDatabase = "Views", string plainDatabase = "PlainStorage", bool useReplication=false)
        {
            this.UseReplication = useReplication;
            this.IsEmbedded = isEmbedded;
            this.Username = username;
            this.Password = password;
            this.StoragePath = storagePath;
            this.EventsDatabase = eventsDatabase;
            this.ViewsDatabase = viewsDatabase;
            this.PlainDatabase = plainDatabase;
        }

        public bool IsEmbedded { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string StoragePath { get; private set; }
        public string EventsDatabase { get; private set; }
        public string ViewsDatabase { get; private set; }
        public string PlainDatabase { get; private set; }
        public bool UseReplication { get; private set; }
    }
}