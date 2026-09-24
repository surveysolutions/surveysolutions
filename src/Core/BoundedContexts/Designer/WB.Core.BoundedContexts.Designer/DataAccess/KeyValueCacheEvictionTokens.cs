using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace WB.Core.BoundedContexts.Designer.DataAccess
{
    // Hands out a per-key eviction token that a reader attaches to its cache entry BEFORE it reads the store.
    // If the key is invalidated (after a writer commits) while that read is still in flight, the token is
    // cancelled, so the entry the reader is about to publish is evicted instead of caching a pre-commit value.
    // This closes the race where a read begun before invalidation repopulates the shared cache afterwards.
    public interface IKeyValueCacheEvictionTokens
    {
        CancellationTokenSource Acquire(string cacheKey);
        void Invalidate(string cacheKey);
        void Release(string cacheKey, CancellationTokenSource source);
    }

    public class KeyValueCacheEvictionTokens : IKeyValueCacheEvictionTokens
    {
        private readonly ConcurrentDictionary<string, CancellationTokenSource> sources = new();

        public CancellationTokenSource Acquire(string cacheKey)
            => this.sources.GetOrAdd(cacheKey, _ => new CancellationTokenSource());

        public void Invalidate(string cacheKey)
        {
            // Drop the current source and cancel it: any entry that captured this token is evicted immediately,
            // and the next reader gets a fresh source. The source is not disposed so a reader that captured its
            // token can still attach it (a disposed source would throw on registration).
            if (this.sources.TryRemove(cacheKey, out var source))
            {
                source.Cancel();
            }
        }

        // Called when the key's cache entry is evicted, so read-only keys don't stay rooted for the process lifetime.
        // Compare-and-remove: only this exact generation is dropped, never a newer source a concurrent reader installed.
        public void Release(string cacheKey, CancellationTokenSource source)
            => this.sources.TryRemove(new KeyValuePair<string, CancellationTokenSource>(cacheKey, source));
    }
}
