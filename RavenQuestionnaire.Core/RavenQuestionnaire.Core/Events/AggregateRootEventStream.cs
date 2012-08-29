// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AggregateRootEventStream.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Defines the AggregateRootEventStream type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Ncqrs.Eventing;

    using Newtonsoft.Json;

    /// <summary>
    /// The aggregate root event stream.
    /// </summary>
    [Serializable]
    public class AggregateRootEventStream
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateRootEventStream"/> class.
        /// </summary>
        /// <param name="events">
        /// The events.
        /// </param>
        /// <param name="fromVersion">
        /// The from version.
        /// </param>
        /// <param name="toVersion">
        /// The to version.
        /// </param>
        /// <param name="sourceId">
        /// The source id.
        /// </param>
        [JsonConstructor]
        public AggregateRootEventStream(
            IEnumerable<AggregateRootEvent> events, long fromVersion, long toVersion, Guid sourceId)
        {
            this.Events = events.ToArray();
            this.FromVersion = fromVersion;
            this.ToVersion = toVersion;
            this.SourceId = sourceId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateRootEventStream"/> class.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        public AggregateRootEventStream(CommittedEventStream stream)
        {
            this.Events = stream.Select(e => new AggregateRootEvent(e)).ToArray();
            this.FromVersion = stream.FromVersion;
            this.ToVersion = stream.ToVersion;
            this.SourceId = stream.SourceId;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the events.
        /// </summary>
        public AggregateRootEvent[] Events { get; set; }

        /// <summary>
        /// Gets or sets the from version.
        /// </summary>
        public long FromVersion { get; set; }

        // public bool IsEmpty { get; set; }

        /// <summary>
        /// Gets or sets the source id.
        /// </summary>
        public Guid SourceId { get; set; }

        /// <summary>
        /// Gets or sets the to version.
        /// </summary>
        public long ToVersion { get; set; }

        #endregion

        // public long CurrentSourceVersion { get; set; }
    }

    /// <summary>
    /// The aggregate root event.
    /// </summary>
    [Serializable]
    public class AggregateRootEvent
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateRootEvent"/> class.
        /// </summary>
        public AggregateRootEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateRootEvent"/> class.
        /// </summary>
        /// <param name="committedEvent">
        /// The committed Event.
        /// </param>
        public AggregateRootEvent(CommittedEvent committedEvent)
        {
            this.Payload = committedEvent.Payload;
            this.EventIdentifier = committedEvent.EventIdentifier;
            this.EventSequence = committedEvent.EventSequence;
            this.EventSourceId = committedEvent.EventSourceId;
            this.EventTimeStamp = committedEvent.EventTimeStamp;
            this.EventVersion = committedEvent.EventVersion;
            this.CommitId = committedEvent.CommitId;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the CommitId.
        /// If of a commit in which this event was stored (usually corresponds to a command id which caused this event).
        /// </summary>
        public Guid CommitId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for this event.
        /// </summary>
        public Guid EventIdentifier { get; set; }

        /// <summary>
        /// Gets the event sequence number.
        /// </summary>
        /// <remarks>
        /// An sequence of events always starts with <c>1</c>. So the first event in a sequence has the <see cref="EventSequence"/> value of <c>1</c>.
        /// </remarks>
        /// <value>A number that represents the order of where this events occurred in the sequence.</value>
        public long EventSequence { get; set; }

        /// <summary>
        /// Gets the id of the event source that caused the event.
        /// </summary>
        /// <value>The id of the event source that caused the event.</value>
        public Guid EventSourceId { get; set; }

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
        /// Gets or sets the payload.
        /// </summary>
        public object Payload { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The create uncommited event.
        /// </summary>
        /// <param name="eventSequence">
        /// The event sequence.
        /// </param>
        /// <param name="initialVersionOfEventSource">
        /// The initial version of event source.
        /// </param>
        /// <returns>
        /// The Ncqrs.Eventing.UncommittedEvent.
        /// </returns>
        public UncommittedEvent CreateUncommitedEvent(long eventSequence, long initialVersionOfEventSource)
        {
            return new UncommittedEvent(
                this.EventIdentifier, 
                this.EventSourceId, 
                eventSequence, 
                initialVersionOfEventSource, 
                this.EventTimeStamp, 
                this.Payload, 
                this.EventVersion);
        }

        #endregion
    }
}