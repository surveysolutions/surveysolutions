using System;
using System.Diagnostics;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;

namespace Ncqrs.Eventing
{
    /// <summary>
    /// Represents an event which has not been yet persisted.
    /// </summary>
    [DebuggerDisplay("{Payload.GetType().Name} {EventIdentifier}")]
    public class UncommittedEvent : IUncommittedEvent
    {
        /// <summary>
        /// Gets the initial version of event source (the version it was just after creating/retrieving from the store)
        /// </summary>
        public int InitialVersionOfEventSource { get; }

        /// <summary>
        /// Gets the payload of the event.
        /// </summary>
        public WB.Core.Infrastructure.EventBus.IEvent Payload { get; }

        public long GlobalSequence { get; set; }

        /// <summary>
        /// Gets the unique identifier for this event.
        /// </summary>
        public Guid EventIdentifier { get; }

        /// <summary>
        /// Gets the time stamp for this event.
        /// </summary>
        /// <value>a <see cref="DateTime"/> UTC value that represents the point
        /// in time where this event occurred.</value>
        public DateTime EventTimeStamp { get; }

        /// <summary>
        /// Gets the id of the event source that caused the event.
        /// </summary>
        /// <value>The id of the event source that caused the event.</value>
        public Guid EventSourceId { get; }

        /// <summary>
        /// Gets the event sequence number.
        /// </summary>
        /// <remarks>
        /// An sequence of events always starts with <c>1</c>. So the first event in a sequence has the <see cref="EventSequence"/> value of <c>1</c>.
        /// </remarks>
        /// <value>A number that represents the order of where this events occurred in the sequence.</value>
        public int EventSequence { get; }

        /// <summary>
        /// If of a commit in which this event is to be stored (usually corresponds to a command id which caused this event).
        /// </summary>
        public Guid CommitId { get; private set; }

        public string Origin { get; private set; }

        public void OnAppendedToStream(Guid streamCommitId, string streamOrigin)
        {
            CommitId = streamCommitId;
            Origin = streamOrigin;
        }

        public UncommittedEvent(Guid eventIdentifier, 
            Guid eventSourceId, 
            int eventSequence, 
            int initialVersionOfEventSource, 
            DateTime eventTimeStamp, 
            WB.Core.Infrastructure.EventBus.IEvent payload)            
        {
            Payload = payload;
            InitialVersionOfEventSource = initialVersionOfEventSource;
            EventSourceId = eventSourceId;
            EventSequence = eventSequence;
            EventIdentifier = eventIdentifier;
            EventTimeStamp = eventTimeStamp;
        }

        public override string ToString() => $"{Payload.GetType().FullName}[{EventIdentifier:D}]";
    }
}
