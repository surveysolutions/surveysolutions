using System;
using System.Diagnostics;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;

namespace Ncqrs.Eventing
{
    /// <summary>
    /// Represents an event which has been persisted.
    /// </summary>
    [DebuggerDisplay("CommittedEvent {Payload.GetType().Name}")]
    public class CommittedEvent : IPublishableEvent
    {
        public long GlobalSequence { get; private set; }
        private readonly int _eventSequence;
        private readonly Guid _eventIdentifier;
        private readonly DateTime _eventTimeStamp;
        private readonly Guid _eventSourceId;
        private readonly Guid _commitId;
        private readonly string _origin;

        /// <summary>
        /// If of a commit in which this event was stored (usually corresponds to a command id which caused this event).
        /// </summary>
        public Guid CommitId => this._commitId;

        public string Origin => this._origin;

        public WB.Core.Infrastructure.EventBus.IEvent Payload { get; }

        public Guid EventIdentifier => this._eventIdentifier;

        /// <summary>
        /// Gets the time stamp for this event.
        /// </summary>
        /// <value>a <see cref="DateTime"/> UTC value that represents the point
        /// in time where this event occurred.</value>
        public DateTime EventTimeStamp => this._eventTimeStamp;

        /// <summary>
        /// Gets the id of the event source that caused the event.
        /// </summary>
        /// <value>The id of the event source that caused the event.</value>
        public Guid EventSourceId => this._eventSourceId;

        /// <summary>
        /// Gets the event sequence number.
        /// </summary>
        /// <remarks>
        /// An sequence of events always starts with <c>1</c>. So the first event in a sequence has the <see cref="EventSequence"/> value of <c>1</c>.
        /// </remarks>
        /// <value>A number that represents the order of where this events occurred in the sequence.</value>
        public int EventSequence => this._eventSequence;

        public CommittedEvent(Guid commitId, 
            string origin, 
            Guid eventIdentifier, 
            Guid eventSourceId, 
            int eventSequence, 
            DateTime eventTimeStamp, 
            long globalSequence,
            WB.Core.Infrastructure.EventBus.IEvent payload)            
        {
            GlobalSequence = globalSequence;
            Payload = payload;
            _commitId = commitId;
            _origin = origin;
            _eventSourceId = eventSourceId;
            _eventSequence = eventSequence;
            _eventIdentifier = eventIdentifier;
            _eventTimeStamp = eventTimeStamp;
        }

        public override string ToString() => $"{this.Payload.GetType().FullName}[{this.EventIdentifier:D}]";
    }
}
