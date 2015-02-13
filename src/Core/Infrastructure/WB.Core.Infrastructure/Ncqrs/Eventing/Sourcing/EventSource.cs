using System;
using System.Collections.Generic;
using Ncqrs.Domain;
using Ncqrs.Eventing.Sourcing.Snapshotting;

namespace Ncqrs.Eventing.Sourcing
{
    public abstract class EventSource : IEventSource
    {
        [NonSerialized]
        private Guid _eventSourceId;
        
        public Guid EventSourceId
        {
            get { return _eventSourceId; }
            protected set
            {
                _eventSourceId = value;
            }
        }

        /// <summary>
        /// Gets the current version of the instance as it is known in the event store.
        /// </summary>
        /// <value>
        /// An <see cref="long"/> representing the current version of this aggregate root.
        /// </value>
        public long Version
        {
            get
            {
                return _currentVersion;
            }
        }
        [NonSerialized]
        private long _initialVersion;

        [NonSerialized]
        private long _currentVersion;

        /// <summary>
        /// Gets the initial version.
        /// <para>
        /// This represents the current version of this instance. When this instance was retrieved
        /// via history, it contains the version as it was at that time. For new instances this value is always 0.
        /// </para>
        /// 	<para>
        /// The version does not change until changes are accepted via the <see cref="AcceptChanges"/> method.
        /// </para>
        /// </summary>
        /// <value>The initial version.</value>
        public long InitialVersion
        {
            get { return _initialVersion; }            
        }

        /// <summary>
        /// A list that contains all the event handlers.
        /// </summary>
        [NonSerialized]
        private readonly List<ISourcedEventHandler> _eventHandlers = new List<ISourcedEventHandler>();

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSource"/> class.
        /// </summary>
        protected EventSource()
        {
            EventSourceId = Guid.NewGuid();
        }

        protected EventSource(Guid eventSourceId) 
            : this()
        {
            EventSourceId = eventSourceId;
        }

        public virtual void InitializeFromSnapshot(Snapshot snapshot)
        {
            _eventSourceId = snapshot.EventSourceId;
            _initialVersion = _currentVersion = snapshot.Version;
        }

        /// <summary>
        /// Initializes from history.
        /// </summary>
        /// <param name="history">The history.</param>
        public virtual void InitializeFromHistory(CommittedEventStream history)
        {
            if (history == null)
                throw new ArgumentNullException("The history cannot be null.");
                if (_initialVersion != Version)
            {
                throw new InvalidOperationException("Cannot apply history when instance has uncommitted changes.");
            }
            if (history.IsEmpty)
            {
                return;                
            }

            _eventSourceId = history.SourceId;

            foreach (var historicalEvent in history)
            {
                ApplyEventFromHistory(historicalEvent);                
            }

            _initialVersion = history.CurrentSourceVersion;
        }

        public event EventHandler<EventAppliedEventArgs> EventApplied;

        protected virtual void OnEventApplied(UncommittedEvent evnt)
        {
            if (EventApplied != null)
            {
                EventApplied(this, new EventAppliedEventArgs(evnt));
            }
        }

        internal protected void RegisterHandler(ISourcedEventHandler handler)
        {
            _eventHandlers.Add(handler);
        }

        protected virtual void HandleEvent(object evnt)
        {
            Boolean handled = false;

            // Get a copy of the handlers because an event
            // handler can register a new handler. This will
            // cause the _eventHandlers list to be modified.
            // And modification while iterating it not allowed.
            var handlers = new List<ISourcedEventHandler>(_eventHandlers);

            foreach (var handler in handlers)
            {
                handled |= handler.HandleEvent(evnt);
            }

            if (!handled)
                throw new EventNotHandledException(evnt);
        }

        internal protected void ApplyEvent(object evnt)
        {
            var eventSequence = GetNextSequence();
            var wrappedEvent = new UncommittedEvent(Guid.NewGuid(), EventSourceId, eventSequence, _initialVersion, DateTime.UtcNow, evnt);

            //Legacy stuff...
            var sourcedEvent = evnt as ISourcedEvent;
            if (sourcedEvent != null)
            {
                sourcedEvent.ClaimEvent(EventSourceId, eventSequence);
            }
            HandleEvent(wrappedEvent.Payload);
            OnEventApplied(wrappedEvent);
        }

        private long GetNextSequence()
        {

            // 628426 31 Feb 2011 - the following absolutely needed to ensure correct sequencing, as incorrect versions were being passed to event store
            // TODO: I don't think this should stay here
            if (_initialVersion > 0 && _currentVersion == 0)
            {
                _currentVersion = _initialVersion;
            }
        
            _currentVersion++;
            return _currentVersion;
        }

        private void ApplyEventFromHistory(CommittedEvent evnt)
        {
            ValidateHistoricalEvent(evnt);
            HandleEvent(evnt.Payload);
            _currentVersion++;
        }

        protected virtual void ValidateHistoricalEvent(CommittedEvent evnt)
        {
            if (evnt.EventSourceId != EventSourceId)
            {
                var message = String.Format("Cannot apply historical event from other event source.");
                throw new InvalidOperationException(message);
            }

            // TODO: Do we really really need this check? Why don't we trust IEventStore?

            if (evnt.EventSequence != Version + 1)
            {
                var message = String.Format("Cannot apply event with sequence {0}. Since the initial version of the " +
                                            "aggregate root is {1}. Only an event with sequence number {2} can be applied.",
                                            evnt.EventSequence, Version, Version + 1);
                throw new InvalidOperationException(message);
            }
        }
        
        public void AcceptChanges()
        {
            _initialVersion = Version;
        }

        public override string ToString()
        {
            return string.Format("{0}[{1}]", GetType().FullName, EventSourceId.ToString("D"));
        }
    }
}
