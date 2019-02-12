using System;
using Newtonsoft.Json;
using WB.Services.Infrastructure.EventSourcing.Json;

namespace WB.Services.Infrastructure.EventSourcing
{
    [JsonConverter(typeof(FeedEventConverter))]
    public class Event
    {
        public string EventTypeName { get; set; }

        public int Sequence { get; set; }

        public Guid EventSourceId { get; set; }

        public long GlobalSequence { get; set; }

        public IEvent Payload { get; set; }
        public DateTime EventTimeStamp { get; set; }
    }
}
