using System;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Infrastructure.Native.Storage;

namespace WB.UI.WebTester.Services.Implementation
{
    public class WebTesterAggregateRootRepository : EventSourcedAggregateRootRepositoryWithWebCache
    {
        private readonly IObserver<Guid> notify;

        public WebTesterAggregateRootRepository(IEventStore eventStore, 
            ISnapshotStore snapshotStore, 
            IDomainRepository repository, 
            IAggregateLock aggregateLock,
            IObserver<Guid> notify) : base(eventStore, snapshotStore, repository, aggregateLock)
        {
            this.notify = notify;
        }

        const string CachePrefix = "cache:";

        protected override string Key(Guid id) => CachePrefix + id;

        protected override void CacheItemRemoved(string key)
        {
            notify.OnNext(Guid.Parse(key.Substring(CachePrefix.Length)));
            base.CacheItemRemoved(key);
        }
    }
}