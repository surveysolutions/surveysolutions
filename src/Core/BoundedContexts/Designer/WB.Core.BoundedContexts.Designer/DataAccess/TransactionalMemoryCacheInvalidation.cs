using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;

namespace WB.Core.BoundedContexts.Designer.DataAccess
{
    // Buffers cache keys written under a transaction so the shared cache is invalidated only when the
    // transaction completes (commit or rollback), never while uncommitted changes are still pending.
    public interface ITransactionalMemoryCacheInvalidation
    {
        void Enqueue(string cacheKey);
        void Flush();
    }

    public class TransactionalMemoryCacheInvalidation : ITransactionalMemoryCacheInvalidation
    {
        private readonly IMemoryCache memoryCache;
        private readonly HashSet<string> pendingKeys = new();

        public TransactionalMemoryCacheInvalidation(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        public void Enqueue(string cacheKey) => this.pendingKeys.Add(cacheKey);

        public void Flush()
        {
            foreach (var key in this.pendingKeys)
            {
                this.memoryCache.Remove(key);
            }

            this.pendingKeys.Clear();
        }
    }
}
