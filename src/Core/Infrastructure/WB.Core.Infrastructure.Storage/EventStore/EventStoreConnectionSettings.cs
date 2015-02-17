namespace WB.Core.Infrastructure.Storage.EventStore
{
    public class EventStoreConnectionSettings
    {
        public EventStoreConnectionSettings()
        {
            InitializeProjections = true;
        }

        public string ServerIP { get; set; }

        public int ServerTcpPort { get; set; }

        public int ServerHttpPort { get; set; }

        /// <summary>
        /// Flag that indicates if event store can create and use projections
        /// </summary>
        public bool InitializeProjections { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }
    }
}