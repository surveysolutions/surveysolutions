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
            return memoryCache.GetPrototypeType(id);
        }

        public void MarkAsPrototype(Guid id, PrototypeType type)
        {
            memoryCache.SetPrototypeType(id, type);
        }

        public void RemovePrototype(Guid id)
        {
            memoryCache.SetPrototypeType(id, null);
        }
    }
}
