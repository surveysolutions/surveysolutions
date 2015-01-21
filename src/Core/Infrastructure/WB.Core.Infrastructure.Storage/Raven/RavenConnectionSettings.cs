using System;
using System.Configuration;
using Raven.Abstractions.Replication;

namespace WB.Core.Infrastructure.Storage.Raven
{
    public class RavenConnectionSettings
    {
        private readonly string eventsDatabase;

        public RavenConnectionSettings(string storagePath, string username = null, string password = null, string eventsDatabase = null, string viewsDatabase = "Views",
            string plainDatabase = "PlainStorage", string failoverBehavior = null, string activeBundles = null, string ravenFileSystemName = "FileSystem")
        {
            this.Username = username;
            this.Password = password;
            this.StoragePath = storagePath;
            this.RavenFileSystemName = ravenFileSystemName;
            this.eventsDatabase = eventsDatabase;
            this.ViewsDatabase = viewsDatabase;
            this.PlainDatabase = plainDatabase;

            FailoverBehavior failoverBehaviorValue;
            if (string.IsNullOrEmpty(failoverBehavior) || !Enum.TryParse(failoverBehavior, out failoverBehaviorValue))
                failoverBehaviorValue = FailoverBehavior.FailImmediately;

            this.FailoverBehavior = failoverBehaviorValue;
            this.ActiveBundles = activeBundles;
        }

        public string Username { get; private set; }
        public string Password { get; private set; }
        public string StoragePath { get; private set; }

        public string EventsDatabase
        {
            get
            {
                if (this.eventsDatabase == null)
                    throw new ConfigurationErrorsException("Raven events database is not set. This is because since January 2015 we no longer store events in Raven DB.");

                return this.eventsDatabase;
            }
        }

        public string ViewsDatabase { get; private set; }
        public string PlainDatabase { get; private set; }
        public string RavenFileSystemName { get; private set; }
        public FailoverBehavior FailoverBehavior { get; private set; }
        public string ActiveBundles { get; private set; }
    }
}