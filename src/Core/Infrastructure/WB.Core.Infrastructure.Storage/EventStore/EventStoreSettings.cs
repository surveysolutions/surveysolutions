namespace WB.Core.Infrastructure.Storage.EventStore
{
    public class EventStoreSettings
    {
        public EventStoreSettings()
        {
            InitializeProjections = true;
            MaxCountToRead = 1024;
            UseJson = true;
        }

        public string ServerIP { get; set; }

        public int ServerTcpPort { get; set; }

        public int ServerHttpPort { get; set; }

        /// <summary>
        /// Flag that indicates if event store can create and use projections
        /// </summary>
        public bool InitializeProjections { get; set; }

        public bool UseJson { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public int MaxCountToRead { get; set; }
    }
}