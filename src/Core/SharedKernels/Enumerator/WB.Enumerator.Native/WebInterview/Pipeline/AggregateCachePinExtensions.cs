using System;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Enumerator.Native.WebInterview.Pipeline
{
    public static class AggregateCachePinExtensions
    {
        private const string Key = "web_connected";

        public static long GetConnectedCount(this IAggregateRootCache cache, Guid aggregateId)
        {
            return cache.GetOrCreate(aggregateId)?.Meta.GetOrNull(Key) as long? ?? 0;
        }

        public static void SetConnectedCount(this IAggregateRootCache cache, Guid aggregateId, long value)
        {
            var item = cache.GetOrCreate(aggregateId);
            item.Meta[Key] = value;
        }
    }
}
