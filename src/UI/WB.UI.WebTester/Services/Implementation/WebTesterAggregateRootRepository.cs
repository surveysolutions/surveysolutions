using System;
using Microsoft.Extensions.Caching.Memory;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Services;
using WB.Infrastructure.Native.Storage;

namespace WB.UI.WebTester.Services.Implementation
{
    public class WebTesterAggregateRootRepository : EventSourcedAggregateRootRepositoryWithWebCache
    {
        private readonly IEvictionNotifier notify;

        public WebTesterAggregateRootRepository(
            IInMemoryEventStore eventStore,
            IDomainRepository repository,
            IAggregateLock aggregateLock,
            IServiceLocator serviceLocator,
            IEvictionNotifier notify,
            IAggregateRootPrototypeService prototypeService,
            IEvictionObservable evictionNotification,
            IMemoryCache memoryCache) : base(eventStore, eventStore, prototypeService, repository, serviceLocator, aggregateLock, memoryCache)
        {
            this.notify = notify;
            Expiration = TimeSpan.FromMinutes(10);
            evictionNotification.Subscribe(this.Evict);
        }

        const string CachePrefix = "cache:";

        protected override string Key(Guid id) => CachePrefix + id;

        protected override TimeSpan Expiration { get; }

        protected override void CacheItemRemoved(string key, EvictionReason reason)
        {
            var interviewId = Guid.Parse(key.Substring(CachePrefix.Length));
            notify.Evict(interviewId);
            base.CacheItemRemoved(key, reason);
        }
    }
}
