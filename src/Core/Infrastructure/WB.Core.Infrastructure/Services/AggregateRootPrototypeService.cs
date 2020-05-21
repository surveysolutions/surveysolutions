﻿using System;
 using Microsoft.Extensions.Caching.Memory;

 namespace WB.Core.Infrastructure.Services
{
    class AggregateRootPrototypeService : IAggregateRootPrototypeService
    {
        private readonly IMemoryCache memoryCache;

        public AggregateRootPrototypeService(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }
        
        public PrototypeType? GetPrototypeType(Guid id)
        {
            if(memoryCache.TryGetValue(CacheKey(id), out PrototypeType type))
            {
                return type;
            }
            
            return null;
        }

        public void MarkAsPrototype(Guid id, PrototypeType type)
        {
            memoryCache.Set(CacheKey(id), type,
                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5)));
        }

        private string CacheKey(Guid id) => $"arps::" + id.ToString();

        public void RemovePrototype(Guid id)
        {
            memoryCache.Remove(CacheKey(id));
        }
    }
}
