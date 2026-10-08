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
        private readonly IKeyValueCacheEvictionTokens evictionTokens;
        private readonly HashSet<string> pendingKeys = new();

        public TransactionalMemoryCacheInvalidation(IMemoryCache memoryCache, IKeyValueCacheEvictionTokens evictionTokens)
        {
            this.memoryCache = memoryCache;
            this.evictionTokens = evictionTokens;
        }

        public void Enqueue(string cacheKey) => this.pendingKeys.Add(cacheKey);

        public void Flush()
        {
            foreach (var key in this.pendingKeys)
            {
                // Runs after the transaction has committed, so cancelling the key's eviction token here evicts any
                // entry a concurrent reader populated from pre-commit data during the transaction.
                this.evictionTokens.Invalidate(key);
                this.memoryCache.Remove(key);
            }

            this.pendingKeys.Clear();
        }
    }
}
