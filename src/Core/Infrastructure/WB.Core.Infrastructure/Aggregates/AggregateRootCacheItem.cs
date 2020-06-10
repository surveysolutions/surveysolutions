#nullable enable
using System;
using System.Collections.Generic;

namespace WB.Core.Infrastructure.Aggregates
{
    public class AggregateRootCacheItem 
    {
        public AggregateRootCacheItem(Guid id)
        {
            this.Id = id;
        }

        public Guid Id { get; }
        public Dictionary<string, object?> Meta = new Dictionary<string, object?>();
    }
}
