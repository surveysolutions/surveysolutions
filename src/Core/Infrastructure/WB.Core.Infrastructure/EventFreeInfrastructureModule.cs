using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Domain;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.DependencyInjection;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Modularity;

namespace WB.Core.Infrastructure
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class EventFreeInfrastructureModule : IAppModule
    {
        public void Load(IDependencyRegistry registry)
        {
            registry.Bind<IEventSourcedAggregateRootRepository, DummyEventSourcedAggregateRootRepository>();
            registry.Bind<ILiteEventBus, DummyEventBus>();
            registry.Bind<IEventStore, DummyEventStore>();
        }

        public Task InitAsync(IServiceLocator serviceLocator, UnderConstructionInfo status)
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

            public bool IsDirty(Guid eventSourceId, long lastKnownEventSequence)
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
            public IReadOnlyCollection<CommittedEvent> CommitUncommittedEvents(IEventSourcedAggregateRoot aggregateRoot, string origin)
            {
                throw new NotImplementedException("This application is event free.");
            }

            public void PublishCommittedEvents(IReadOnlyCollection<CommittedEvent> committedEvents)
            {
                throw new NotImplementedException("This application is event free.");
            }
        }
    }
}
