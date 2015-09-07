using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.WriteSide;

namespace WB.Core.Infrastructure.Implementation.Aggregates
{
    internal class AggregateRootRepositoryWithCache : AggregateRootRepository, IWriteSideCleaner
    {
        static readonly ConcurrentDictionary<Type, IAggregateRoot> memoryCache = new ConcurrentDictionary<Type, IAggregateRoot>();

        public AggregateRootRepositoryWithCache(IEventStoreWithGetAllIds eventStore, ISnapshotStore snapshotStore,
            IDomainRepository repository, IWriteSideCleanerRegistry writeSideCleanerRegistry)
            : base(eventStore, snapshotStore, repository)
        {
            writeSideCleanerRegistry.Register(this);
        }


        public override IAggregateRoot GetLatest(Type aggregateType, Guid aggregateId)
        {
            IAggregateRoot aggregateRoot;

            if (memoryCache.TryGetValue(aggregateType, out aggregateRoot)
                && aggregateRoot.EventSourceId == aggregateId)
                return aggregateRoot;

            aggregateRoot = base.GetLatest(aggregateType, aggregateId);

            if (aggregateRoot != null)
            {
                memoryCache[aggregateType] = aggregateRoot;
            }

            return aggregateRoot;
        }

        public void Clean(Guid aggregateId)
        {
            var aggregatesToDeleteFromCache = memoryCache.Where(a => a.Value.EventSourceId == aggregateId).Select(a => a.Key);
            foreach (var type in aggregatesToDeleteFromCache)
            {
                IAggregateRoot root;
                memoryCache.TryRemove(type, out root);
            }
        }
    }
}