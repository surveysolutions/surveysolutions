using System;
using System.Collections.Generic;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;

namespace Ncqrs.Domain.Storage
{
    public class DomainRepository : IDomainRepository
    {
        private readonly IServiceLocator serviceLocator;

        public DomainRepository(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public IEventSourcedAggregateRoot Load(Type aggreateRootType, CommittedEventStream eventStream)
            => this.Load(aggreateRootType, eventStream.SourceId, eventStream);

        public IEventSourcedAggregateRoot Load(Type aggreateRootType, Guid aggregateRootId, IEnumerable<CommittedEvent> events)
        {
            var aggregate = this.GetByIdFromScratch(aggreateRootType, aggregateRootId, events);

            return aggregate;
        }

        public IEventSourcedAggregateRoot LoadStateless(Type aggregateRootType, Guid aggregateRootId, int lastEventSequence)
        {
            var aggregateRoot = (EventSourcedAggregateRoot)this.serviceLocator.GetInstance(aggregateRootType);

            if (aggregateRoot == null)
                throw new ArgumentException($"Cannot create new instance of aggregate root of type {aggregateRootType.Name}");

            aggregateRoot.InitializeFromSnapshot(aggregateRootId, lastEventSequence);

            return aggregateRoot;
        }

        private EventSourcedAggregateRoot GetByIdFromScratch(Type aggregateRootType, Guid aggregateRootId, IEnumerable<CommittedEvent> events)
        {
            var aggregateRoot = (EventSourcedAggregateRoot) this.serviceLocator.GetInstance(aggregateRootType);

            if (aggregateRoot == null)
                throw new ArgumentException($"Cannot create new instance of aggregate root of type {aggregateRootType.Name}");

            aggregateRoot.InitializeFromHistory(aggregateRootId, events);

            bool atLeastOneEventApplied = aggregateRoot.InitialVersion > 0;

            return atLeastOneEventApplied ? aggregateRoot : null;
        }

    }
}
