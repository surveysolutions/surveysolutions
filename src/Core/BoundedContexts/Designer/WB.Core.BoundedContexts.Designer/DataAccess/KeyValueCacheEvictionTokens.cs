using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace WB.Core.BoundedContexts.Designer.DataAccess
{
    // Leases a per-key eviction token that a reader attaches to its cache entry BEFORE it reads the store.
    // If the key is invalidated (after a writer commits) while that read is still in flight, the token is
    // cancelled, so the entry the reader is about to publish is evicted instead of caching a pre-commit value.
    // Each generation is reference-counted by its live cache entries: an evicted entry's cleanup unmaps the
    // source only once no entry still leases it, so it can never remove a source a concurrent, in-flight
    // population still relies on (which would leave a later commit with nothing to cancel).
    public interface IKeyValueCacheEvictionTokens
    {
        ICacheEvictionLease Acquire(string cacheKey);
        void Invalidate(string cacheKey);
    }

    public interface ICacheEvictionLease
    {
        CancellationToken Token { get; }
        void Release();
    }

    public class KeyValueCacheEvictionTokens : IKeyValueCacheEvictionTokens
    {
        private readonly ConcurrentDictionary<string, Generation> sources = new();

        public ICacheEvictionLease Acquire(string cacheKey)
        {
            while (true)
            {
                var generation = this.sources.GetOrAdd(cacheKey, _ => new Generation(this, cacheKey));
                if (generation.TryLease())
                    return generation;

                // The generation retired between GetOrAdd and our lease; drop it if still mapped and take a fresh one.
                this.sources.TryRemove(new KeyValuePair<string, Generation>(cacheKey, generation));
            }
        }

        public void Invalidate(string cacheKey)
        {
            // Runs after the writer commits: cancel the current source so any entry that captured it is evicted,
            // and the next reader starts a fresh generation. The source is not disposed so a reader that already
            // captured its token can still attach it (a disposed source would throw on registration).
            if (this.sources.TryRemove(cacheKey, out var generation))
                generation.Cancel();
        }

        private void Unmap(string cacheKey, Generation generation)
            => this.sources.TryRemove(new KeyValuePair<string, Generation>(cacheKey, generation));

        private sealed class Generation : ICacheEvictionLease
        {
            private readonly KeyValueCacheEvictionTokens owner;
            private readonly string cacheKey;
            private readonly CancellationTokenSource source = new();
            private int leases;
            private bool retired;

            public Generation(KeyValueCacheEvictionTokens owner, string cacheKey)
            {
                this.owner = owner;
                this.cacheKey = cacheKey;
            }

            public CancellationToken Token => this.source.Token;

            // Reserve this generation for one more cache entry; fails once the last entry released it so the
            // caller loops and starts a fresh generation instead of reusing one that is being unmapped.
            public bool TryLease()
            {
                lock (this)
                {
                    if (this.retired)
                        return false;

                    this.leases++;
                    return true;
                }
            }

            public void Release()
            {
                lock (this)
                {
                    if (--this.leases > 0)
                        return;

                    this.retired = true;
                }

                // Last live entry is gone: unmap this exact generation so read-only keys don't stay rooted forever.
                this.owner.Unmap(this.cacheKey, this);
            }

            public void Cancel() => this.source.Cancel();
        }
    }
}
