using System;
using System.Runtime.Caching;

namespace WB.Services.Export.Infrastructure.Implementation
{
    public class Cache : ICache
    {
        private readonly MemoryCache memoryCache = new MemoryCache("ServiceCache");

        public object Get(string key, string tenantId)
        {
            return memoryCache.Get(tenantId + key);
        }

        public void Set(string key, object value, string tenantId)
        {
            this.memoryCache.Set(tenantId + key, 
                value,
                DateTime.Now.AddHours(1));
        }
    }
}
