using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Modularity;

namespace WB.Core.Infrastructure
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class EventFreeInfrastructureModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IEventSourcedAggregateRootRepository, DummyEventSourcedAggregateRootRepository>();
            registry.Bind<ILiteEventBus, DummyEventBus>();
            registry.Bind<IAggregateSnapshotter, DummyAggregateSnapshotter>();
            registry.Bind<IEventStore, DummyEventStore>();
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }

        public class DummyEventStore : IEventStore
        {
            public IEnumerable<CommittedEvent> Read(Guid id, int minVersion)
            {
                throw new NotImplementedException("This application is event free.");
            }

            public IEnumerable<CommittedEvent> Read(Guid id, int minVersion, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
            {
                throw new NotImplementedException("This application is event free.");
            }

            public int? GetLastEventSequence(Guid id)
            {
                throw new NotImplementedException("This application is event free.");
            }

            public CommittedEventStream Store(UncommittedEventStream eventStream)
            {
                throw new NotImplementedException("This application is event free.");
            }
        }

        public class DummyEventSourcedAggregateRootRepository : IEventSourcedAggregateRootRepository
        {
            public IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId)
            {
                throw new NotImplementedException("This application is event free.");
            }

            public IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
            {
                throw new NotImplementedException("This application is event free.");
            }

            public IEventSourcedAggregateRoot GetStateless(Type aggregateType, Guid aggregateId)
            {
                throw new NotImplementedException("This application is event free.");
            }
        }

        public class DummyEventBus : ILiteEventBus
        {
            public void PublishCommittedEvents(IEnumerable<CommittedEvent> committedEvents)
            {
                throw new NotImplementedException("This application is event free.");
            }
        }

        public class DummyAggregateSnapshotter : IAggregateSnapshotter
        {
            public bool TryLoadFromSnapshot(Type aggregateRootType, Snapshot snapshot, CommittedEventStream committedEventStream,
                out EventSourcedAggregateRoot aggregateRoot)
            {
                throw new NotImplementedException("This application is event free.");
            }

            public bool TryLoadFromSnapshot(Type aggregateRootType, Snapshot snapshot, IEnumerable<CommittedEvent> history,
                out EventSourcedAggregateRoot aggregateRoot)
            {
                throw new NotImplementedException("This application is event free.");
            }

            public bool TryTakeSnapshot(IEventSourcedAggregateRoot aggregateRoot, out Snapshot snapshot)
            {
                throw new NotImplementedException("This application is event free.");
            }

            public void CreateSnapshotIfNeededAndPossible(IEventSourcedAggregateRoot aggregateRoot)
            {
                throw new NotImplementedException("This application is event free.");
            }
        }
    }
}
