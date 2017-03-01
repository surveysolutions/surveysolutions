using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing;
using WB.Core.Infrastructure.Aggregates;

namespace Ncqrs.Domain
{
    /// <summary>
    /// The abstract concept of an aggregate root.
    /// </summary>
    public abstract class EventSourcedAggregateRoot : EventSource, IEventSourcedAggregateRoot
    {
        // 628426 13 Feb 2011
        // Previous ThreadStatic was null referencing at random times under load 
        // These things work great
        private static System.Threading.ThreadLocal<List<Action<EventSourcedAggregateRoot, UncommittedEvent>>> _eventAppliedCallbacks = new System.Threading.ThreadLocal<List<Action<EventSourcedAggregateRoot, UncommittedEvent>>>(() => new List<Action<EventSourcedAggregateRoot, UncommittedEvent>>());

        private readonly List<UncommittedEvent> changes = new List<UncommittedEvent>();

        public static void RegisterThreadStaticEventAppliedCallback(Action<EventSourcedAggregateRoot, UncommittedEvent> callback)
        {
            _eventAppliedCallbacks.Value.Add(callback);
        }

        public static void UnregisterThreadStaticEventAppliedCallback(Action<EventSourcedAggregateRoot, UncommittedEvent> callback)
        {
            _eventAppliedCallbacks.Value.Remove(callback);
        }

        protected EventSourcedAggregateRoot()
        {}

        protected EventSourcedAggregateRoot(Guid id) : base(id)
        {}

        protected override void OnEventApplied(UncommittedEvent appliedEvent)
        {
            base.OnEventApplied(appliedEvent);

            this.changes.Add(appliedEvent);

            var callbacks = _eventAppliedCallbacks.Value;

            foreach(var callback in callbacks)
            {
                callback(this, appliedEvent);
            }
        }

        public bool HasUncommittedChanges() => this.changes.Any();

        public List<UncommittedEvent> GetUnCommittedChanges()
        {
            return this.changes.ToList();
        }

        public void SetId(Guid id)
        {
            this.EventSourceId = id;
        }

        public void MarkChangesAsCommitted()
        {
            this.AcceptChanges();
            this.changes.Clear();
        }
    }
}
