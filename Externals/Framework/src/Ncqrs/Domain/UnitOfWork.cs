﻿using System;
using System.Collections.Generic;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Logging;

namespace Ncqrs.Domain
{
    public class UnitOfWork : UnitOfWorkBase
    {
        /// <summary>
        /// A queue that holds a reference to all instances that have themself registered as a dirty instance during the lifespan of this unit of work instance.
        /// </summary>
        private readonly Queue<AggregateRoot> _dirtyInstances;
        private readonly UncommittedEventStream _eventStream;
        private readonly IDomainRepository _repository;
        private readonly IEventStore _eventStore;
        private readonly ISnapshotStore _snapshotStore;
        private readonly IEventBus _eventBus;
        private readonly ISnapshottingPolicy _snapshottingPolicy;

        public UnitOfWork(Guid commandId, string origin, IDomainRepository domainRepository, IEventStore eventStore, ISnapshotStore snapshotStore, IEventBus eventBus, ISnapshottingPolicy snapshottingPolicy) : base(commandId)
        {
            _repository = domainRepository;
            _snapshottingPolicy = snapshottingPolicy;
            _eventBus = eventBus;
            _snapshotStore = snapshotStore;
            _eventStore = eventStore;
            _eventStream = new UncommittedEventStream(commandId, origin);
            _dirtyInstances = new Queue<AggregateRoot>();
        }

        protected override void AggregateRootEventAppliedHandler(AggregateRoot aggregateRoot, UncommittedEvent evnt)
        {
            RegisterDirtyInstance(aggregateRoot);            
            _eventStream.Append(evnt);
        }
        /// <summary>
        /// Gets aggregate root by its id.
        /// </summary>
        /// <param name="aggregateRootType">Type of the aggregate root.</param>
        /// <param name="eventSourceId">The eventSourceId of the aggregate root.</param>
        /// <param name="lastKnownRevision">If specified, the most recent version of event source observed by the client (used for optimistic concurrency).</param>
        /// <returns>
        /// A new instance of the aggregate root that contains the latest known state.
        /// </returns>
        public override AggregateRoot GetById(Type aggregateRootType, Guid eventSourceId, long? lastKnownRevision)
        {
            long maxVersion = lastKnownRevision.HasValue ? lastKnownRevision.Value : long.MaxValue;
            Snapshot snapshot = null;
            long minVersion = long.MinValue;
            snapshot = _snapshotStore.GetSnapshot(eventSourceId, maxVersion);
            if (snapshot != null)
            {
                minVersion = snapshot.Version + 1;
            }
            var eventStream = _eventStore.ReadFrom(eventSourceId, minVersion, maxVersion);
            return _repository.Load(aggregateRootType, snapshot, eventStream);
        }

        /// <summary>
        /// Accepts the unit of work and persist the changes.
        /// </summary>
        public override void Accept()
        {
            _eventStore.Store(_eventStream);
           
#if MONODROID
            _eventBus.Publish(_eventStream
            .Select(ev => (IPublishableEvent)ev)
            .ToList());
#else
           _eventBus.Publish(_eventStream);
#endif
            CreateSnapshots();
        }

        private void CreateSnapshots()
        {
            foreach (AggregateRoot savedInstance in _dirtyInstances)
            {
                TryCreateCreateSnapshot(savedInstance);
            }
        }

        private void TryCreateCreateSnapshot(AggregateRoot savedInstance)
        {
            if (_snapshottingPolicy.ShouldCreateSnapshot(savedInstance))
            {
                var snapshot = _repository.TryTakeSnapshot(savedInstance);
                if (snapshot != null)
                {
                    _snapshotStore.SaveShapshot(snapshot);
                }
            }
        }

        /// <summary>
        /// Registers the dirty.
        /// </summary>
        /// <param name="dirtyInstance">The dirty instance.</param>
        private void RegisterDirtyInstance(AggregateRoot dirtyInstance)
        {
            if (!_dirtyInstances.Contains(dirtyInstance))
            {
                _dirtyInstances.Enqueue(dirtyInstance);
            }
        }

        public override string ToString()
        {
#warning Slava: restore Thread.CurrentThread.ManagedThreadId
            return string.Format("{0}@{1}", CommandId, "Thread.CurrentThread.ManagedThreadId");
        }
    }
}