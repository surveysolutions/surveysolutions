using System;
using System.Collections.Generic;
using Ncqrs.Eventing;
using Newtonsoft.Json;

namespace RavenQuestionnaire.Core.Events
{
    public class AggregateRootEventStream
    {
        [JsonConstructor]
        public AggregateRootEventStream(IEnumerable<CommittedEvent> events, long fromVersion, long toVersion, Guid sourceId)
        {
            this.Events = events;
            this.FromVersion = fromVersion;
            this.ToVersion = toVersion;
            this.SourceId = sourceId;
        }

        public AggregateRootEventStream(CommittedEventStream stream)
        {
            this.Events = stream;
            this.FromVersion = stream.FromVersion;
            this.ToVersion = stream.ToVersion;
            this.SourceId = stream.SourceId;
        }

        public IEnumerable<CommittedEvent> Events { get; set; }
        public long FromVersion { get; set; }

        public long ToVersion { get; set; }

       // public bool IsEmpty { get; set; }

        public Guid SourceId { get; set; }

       // public long CurrentSourceVersion { get; set; }
    }
}
