using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Ncqrs.Domain;

namespace Ncqrs.Eventing.Sourcing
{
    public abstract class EventSource : IEventSource
    {
        [field: NonSerialized]
        public virtual Guid EventSourceId { get; protected set; }

        /// <summary>
        /// Gets the current version of the instance as it is known in the event store.
        /// </summary>
        /// <value>
        /// An <see cref="long"/> representing the current version of this aggregate root.
        /// </value>
        public int Version => this._currentVersion;

        [NonSerialized] 
        private int _initialVersion;

        [NonSerialized] 
        private int _currentVersion;

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
        public int InitialVersion => this._initialVersion;


        protected EventSource()
        {
            EventSourceId = Guid.NewGuid();
        }

        protected EventSource(Guid eventSourceId)
            : this()
        {
            EventSourceId = eventSourceId;
        }

        protected virtual bool CanHandleEvent(CommittedEvent committedEvent) 
            => GetForEvent(committedEvent.Payload) != null;

        public virtual void InitializeFromSnapshot(Guid eventSourceId, int version)
        {
            EventSourceId = eventSourceId;
            _initialVersion = _currentVersion = version;
        }

        public virtual void InitializeFromHistory(Guid eventSourceId, IEnumerable<CommittedEvent> history)
        {
            if (history == null)
                throw new ArgumentNullException(nameof(history));

            if (this._initialVersion != this.Version)
                throw new InvalidOperationException("Cannot apply history when instance has uncommitted changes.");

            EventSourceId = eventSourceId;

            foreach (var historicalEvent in history)
            {
                if (!this.CanHandleEvent(historicalEvent))
                {
                    this._initialVersion = 0;
                    return;
                }

                this.ApplyEventFromHistory(historicalEvent);
                this._initialVersion = historicalEvent.EventSequence;
            }
        }

        public event EventHandler<EventAppliedEventArgs> EventApplied;

        protected virtual void OnEventApplied(UncommittedEvent evnt)
        {
            EventApplied?.Invoke(this, new EventAppliedEventArgs(evnt));
        }

        private static readonly ConcurrentDictionary<(Type, Type), MethodInfo> _handlersCache 
            = new ConcurrentDictionary<(Type, Type), MethodInfo>(); 
        
        private MethodInfo GetForEvent(object evnt) => _handlersCache.GetOrAdd((GetType(), evnt.GetType()), key =>
        {
            var parameters = new[] { key.Item2 };
            return key.Item1.GetMethod("Apply", 
                       BindingFlags.Instance | BindingFlags.NonPublic, Type.DefaultBinder, parameters, null)
                   ??  key.Item1.GetMethod("Apply", 
                       BindingFlags.Instance | BindingFlags.Public, Type.DefaultBinder, parameters, null);
        });
        
        protected virtual void HandleEvent(object evnt)
        {
            var apply = GetForEvent(evnt);
            
            if (apply == null)
            {
                throw new EventNotHandledException(evnt);
            }
            
            apply.Invoke(this, new [] { evnt });
        }

        protected internal virtual void ApplyEvent(Guid eventIdentifier,
            DateTime eventTimeStamp,
            WB.Core.Infrastructure.EventBus.IEvent evnt)
        {
            var eventSequence = GetNextSequence();
            var wrappedEvent = new UncommittedEvent(eventIdentifier, EventSourceId, eventSequence, _initialVersion,
                eventTimeStamp, evnt);

            try
            {
                HandleEvent(wrappedEvent.Payload);
                OnEventApplied(wrappedEvent);
            }
            catch (EventNotHandledException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new OnEventApplyException(wrappedEvent, e.Message, e);
            }
        }

        protected internal void ApplyEvent(WB.Core.Infrastructure.EventBus.IEvent evnt)
        {
            ApplyEvent(
                eventIdentifier: Guid.NewGuid(),
                eventTimeStamp: DateTime.UtcNow,
                evnt: evnt
            );
        }

        private int GetNextSequence()
        {
            // 628426 31 Feb 2011 - the following absolutely needed to ensure correct sequencing, as incorrect versions were being passed to event store
            // I don't think this should stay here
            if (_initialVersion > 0 && _currentVersion == 0)
            {
                _currentVersion = _initialVersion;
            }

            return Interlocked.Increment(ref _currentVersion);
        }

        private void ApplyEventFromHistory(CommittedEvent evnt)
        {
            ValidateHistoricalEvent(evnt);
            
            try
            {
                HandleEvent(evnt.Payload);
            }
            catch (EventNotHandledException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new OnEventApplyException(evnt, e.Message, e);
            }
            
            _currentVersion++;
        }

        protected virtual void ValidateHistoricalEvent(CommittedEvent evnt)
        {
            if (evnt.EventSourceId != EventSourceId)
            {
                var message = "Cannot apply historical event from other event source.";
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

        public virtual void AcceptChanges()
        {
            _initialVersion = Version;
        }

        public override string ToString()
        {
            return string.Format("{0}[{1}]", GetType().FullName, EventSourceId.ToString("D"));
        }
    }
}
