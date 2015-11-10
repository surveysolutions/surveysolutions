using System;
using WB.Core.Infrastructure.EventBus.Lite;

namespace Ncqrs.Eventing.ServiceModel.Bus
{
    /// <summary>
    /// Base clas for representing published events. Can be used when type of the event payload does not matter.
    /// </summary>
    public abstract class PublishedEvent : IUncommittedEvent
    {
        private readonly ILiteEvent _payload;
        private readonly int _eventSequence;
        private readonly Guid _eventIdentifier;
        private readonly DateTime _eventTimeStamp;
        private readonly Guid _eventSourceId;
        private readonly Guid _commitId;
        private readonly string _origin;
        private readonly long globalSequence;

        /// <summary>
        /// Id of the commit this event belongs to (usually corresponds to command id).
        /// </summary>
        public Guid CommitId
        {
            get { return this._commitId; }
        }

        public string Origin
        {
            get { return this._origin; }
        }

        /// <summary>
        /// Gets the payload of the event.
        /// </summary>
        public ILiteEvent Payload
        {
            get { return this._payload; }
        }

        /// <summary>
        /// Gets the unique identifier for this event.
        /// </summary>
        public Guid EventIdentifier
        {
            get { return this._eventIdentifier; }
        }

        /// <summary>
        /// Gets the time stamp for this event.
        /// </summary>
        /// <value>a <see cref="DateTime"/> UTC value that represents the point
        /// in time where this event occurred.</value>
        public DateTime EventTimeStamp
        {
            get { return this._eventTimeStamp; }
        }

        /// <summary>
        /// Gets the id of the event source that caused the event.
        /// </summary>
        /// <value>The id of the event source that caused the event.</value>
        public Guid EventSourceId
        {
            get { return this._eventSourceId; }
        }

        /// <summary>
        /// Gets the event sequence number.
        /// </summary>
        /// <remarks>
        /// An sequence of events always starts with <c>1</c>. So the first event in a sequence has the <see cref="EventSequence"/> value of <c>1</c>.
        /// </remarks>
        /// <value>A number that represents the order of where this events occurred in the sequence.</value>
        public int EventSequence
        {
            get { return this._eventSequence; }
        }

        public long GlobalSequence
        {
            get { return this.globalSequence; }
        }

        protected PublishedEvent(IPublishableEvent evnt)            
        {            
            this._payload = evnt.Payload;           
            this._eventSourceId = evnt.EventSourceId;
            this._eventSequence = evnt.EventSequence;
            this._eventIdentifier = evnt.EventIdentifier;
            this._eventTimeStamp = evnt.EventTimeStamp;
            this._commitId = evnt.CommitId;
            this._origin = evnt.Origin;
            this.globalSequence = evnt.GlobalSequence;
        }
    }

    /// <summary>
    /// Provides default <see cref="IPublishedEvent{TEvent}"/> interface implementation.
    /// </summary>
    /// <typeparam name="TEvent">Type of the event.</typeparam>
    public class PublishedEvent<TEvent> : PublishedEvent, IPublishedEvent<TEvent>
        where TEvent : ILiteEvent
    {
        /// <summary>
        /// Gets the payload of the event.
        /// </summary>
        public new TEvent Payload
        {
            get { return (TEvent)base.Payload; }
        }

        public PublishedEvent(IPublishableEvent evnt) : base(evnt)
        {
        }
    }
}