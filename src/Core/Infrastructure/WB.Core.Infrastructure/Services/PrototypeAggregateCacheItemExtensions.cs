using System;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.Services
{
    public static class PrototypeAggregateCacheItemExtensions
    {
        private const string Key = "prototype";

        public static PrototypeType? GetPrototypeType(this IAggregateRootCache cache, Guid aggregateId)
        {
            return (PrototypeType?) cache.Get(aggregateId)?.Meta.GetOrNull(Key);
        }

        public static void SetPrototypeType(this IAggregateRootCache cache, Guid aggregateId, PrototypeType? value)
        {
            cache.GetOrCreate(aggregateId).Meta[Key] = value;
        }
    }
}
