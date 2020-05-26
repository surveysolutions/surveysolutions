using System;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.Services
{
    class AggregateRootPrototypeService : IAggregateRootPrototypeService
    {
        private readonly IAggregateRootCache memoryCache;

        public AggregateRootPrototypeService(IAggregateRootCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        public PrototypeType? GetPrototypeType(Guid id)
        {
            var cacheItem = memoryCache.Get(id);

            return cacheItem?.PrototypeType;
        }

        public void MarkAsPrototype(Guid id, PrototypeType type)
        {
            var cacheItem = memoryCache.GetOrCreate(id, item => item.SetPrototypeType(type));
            cacheItem.PrototypeType = type;
        }

        public void RemovePrototype(Guid id)
        {
            var cacheItem = memoryCache.Get(id);
            cacheItem.PrototypeType = null;
        }
    }
}
