using System.Collections.Generic;

namespace WB.Core.Infrastructure.Storage.EventStore
{
    public class EventStoreSettings
    {
        public EventStoreSettings()
        {
            InitializeProjections = true;
            MaxCountToRead = 1024;
            this.EventStreamsToIgnore=new HashSet<string>();
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

        public int MaxCountToRead { get; set; }

        public HashSet<string> EventStreamsToIgnore { get; set; } 
    }
}