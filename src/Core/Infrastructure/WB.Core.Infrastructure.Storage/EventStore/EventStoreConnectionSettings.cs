namespace WB.Core.Infrastructure.Storage.EventStore
{
    public class EventStoreConnectionSettings
    {
        public string ServerIP { get; set; }

        public int ServerTcpPort { get; set; }

        public int ServerHttpPort { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }
    }
}