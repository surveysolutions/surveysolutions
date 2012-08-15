using System;
using System.Linq;
using System.Collections.Generic;
using Ncqrs.Eventing;
using Newtonsoft.Json;

namespace RavenQuestionnaire.Core.Events
{
    [Serializable]
    public class AggregateRootEventStream
    {
        [JsonConstructor]
        public AggregateRootEventStream(IEnumerable<AggregateRootEvent> events, long fromVersion, long toVersion, Guid sourceId)
        {
            this.Events = events.ToArray();
            this.FromVersion = fromVersion;
            this.ToVersion = toVersion;
            this.SourceId = sourceId;
        }
        public AggregateRootEventStream(CommittedEventStream stream)
        {
            this.Events = stream.Select(e => new AggregateRootEvent(e)).ToArray();
            this.FromVersion = stream.FromVersion;
            this.ToVersion = stream.ToVersion;
            this.SourceId = stream.SourceId;
        }
        

        public AggregateRootEvent[] Events { get; set; }
        public long FromVersion { get; set; }

        public long ToVersion { get; set; }

       // public bool IsEmpty { get; set; }

        public Guid SourceId { get; set; }

       // public long CurrentSourceVersion { get; set; }
    }
    [Serializable]
    public class AggregateRootEvent
    {
        public AggregateRootEvent()
        {
        }
        public AggregateRootEvent(CommittedEvent cEvent)
        {
            this.Payload = cEvent.Payload;
            this.EventIdentifier = cEvent.EventIdentifier;
            this.EventSequence = cEvent.EventSequence;
            this.EventSourceId = cEvent.EventSourceId;
            this.EventTimeStamp = cEvent.EventTimeStamp;
            this.EventVersion = cEvent.EventVersion;
            this.CommitId = cEvent.CommitId;
        }

        public UncommittedEvent CreateUncommitedEvent(long initialVersionOfEventSource)
        {
            return new UncommittedEvent(this.EventIdentifier,
                                        this.EventSourceId,
                                        this.EventSequence, initialVersionOfEventSource,
                                        this.EventTimeStamp,
                                        this.Payload,
                                        this.EventVersion);
        }

        /// <summary>
        /// Gets the payload of the event.
        /// </summary>
        public object Payload { get; set; }

        /// <summary>
        /// If of a commit in which this event was stored (usually corresponds to a command id which caused this event).
        /// </summary>
        public Guid CommitId { get; set; }

        /// <summary>
        /// Gets the unique identifier for this event.
        /// </summary>
        public Guid EventIdentifier { get; set; }
        /// <summary>
        /// Gets the time stamp for this event.
        /// </summary>
        /// <value>a <see cref="DateTime"/> UTC value that represents the point
        /// in time where this event occurred.</value>
        public DateTime EventTimeStamp { get; set; }

        /// <summary>
        /// Gets the CLR version of event type that was used to persist data.
        /// </summary>
        public Version EventVersion { get; set; }

        /// <summary>
        /// Gets the id of the event source that caused the event.
        /// </summary>
        /// <value>The id of the event source that caused the event.</value>
        public Guid EventSourceId { get; set; }

        /// <summary>
        /// Gets the event sequence number.
        /// </summary>
        /// <remarks>
        /// An sequence of events always starts with <c>1</c>. So the first event in a sequence has the <see cref="EventSequence"/> value of <c>1</c>.
        /// </remarks>
        /// <value>A number that represents the order of where this events occurred in the sequence.</value>
        public long EventSequence { get; set; }

    }
}
