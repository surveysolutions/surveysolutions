using System;
using Microsoft.Extensions.Caching.Memory;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces.Impl
{
    public static class MemoryCacheExtensions
    {
        public static TItem GetOrCreateWithDoubleLock<TItem>(this IMemoryCache cache,
            object key, Func<ICacheEntry, TItem> factory) where TItem : notnull
        {
            if (!cache.TryGetValue(key, out object result))
            {
                lock (cache)
                {
                    if (cache.TryGetValue(key, out result)) return (TItem)result;

                    using ICacheEntry entry = cache.CreateEntry(key);

                    result = factory(entry);
                    entry.Value = result;
                }
            }

            return (TItem)result;
        }
        
    }
}
