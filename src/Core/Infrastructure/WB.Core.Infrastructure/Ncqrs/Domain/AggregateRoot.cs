﻿using System;
using System.Collections.Generic;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing;
using WB.Core.Infrastructure.Aggregates;

namespace Ncqrs.Domain
{
    /// <summary>
    /// The abstract concept of an aggregate root.
    /// </summary>
    public abstract class AggregateRoot : EventSource, IAggregateRoot
    {
        // 628426 13 Feb 2011
        // Previous ThreadStatic was null referencing at random times under load 
        // These things work great
        private static System.Threading.ThreadLocal<List<Action<AggregateRoot, UncommittedEvent>>> _eventAppliedCallbacks = new System.Threading.ThreadLocal<List<Action<AggregateRoot, UncommittedEvent>>>(() => new List<Action<AggregateRoot, UncommittedEvent>>());

        private readonly List<UncommittedEvent> changes = new List<UncommittedEvent>();

        public static void RegisterThreadStaticEventAppliedCallback(Action<AggregateRoot, UncommittedEvent> callback)
        {
            _eventAppliedCallbacks.Value.Add(callback);
        }

        public static void UnregisterThreadStaticEventAppliedCallback(Action<AggregateRoot, UncommittedEvent> callback)
        {
            _eventAppliedCallbacks.Value.Remove(callback);
        }

        protected AggregateRoot()
        {}

        protected AggregateRoot(Guid id) : base(id)
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

        public IEnumerable<UncommittedEvent> GetUncommittedChanges()
        {
            return this.changes;
        }

        public void SetId(Guid id)
        {
            this.EventSourceId = id;
        }

        public void MarkChangesAsCommitted()
        {
            this.changes.Clear();
        }
    }
}
