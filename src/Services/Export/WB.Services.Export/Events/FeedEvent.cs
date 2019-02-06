using System;
using Newtonsoft.Json;
using WB.Services.Export.Events.Interview.Base;
using WB.Services.Export.Services;

namespace WB.Services.Export.Events
{
    [JsonConverter(typeof(FeedEventConverter))]
    public class FeedEvent
    {
        public string EventTypeName { get; set; }

        public int Sequence { get; set; }

        public Guid EventSourceId { get; set; }

        public long GlobalSequence { get; set; }

        public IEvent Payload { get; set; }
    }
}