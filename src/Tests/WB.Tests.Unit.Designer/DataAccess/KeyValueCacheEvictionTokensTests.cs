#nullable enable
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.DataAccess;

namespace WB.Tests.Unit.Designer.DataAccess;

// Covers the reference-counted eviction-token registry, in particular the expiration/repopulation-vs-invalidation
// races: an old entry's eviction callback must never unmap a source that a newer, in-flight population still leases,
// otherwise a concurrent commit would find nothing to cancel and could leave a pre-commit value cached.
[TestFixture]
[TestOf(typeof(KeyValueCacheEvictionTokens))]
public class KeyValueCacheEvictionTokensTests
{
    [Test]
    public void when_old_entry_is_released_while_a_new_entry_leases_the_same_generation_then_invalidate_still_cancels_it()
    {
        var registry = new KeyValueCacheEvictionTokens();

        // Old cached entry and a new/in-flight entry both lease the current generation before any eviction runs.
        var oldEntry = registry.Acquire("key");
        var newEntry = registry.Acquire("key");

        // The old entry is evicted and its post-eviction callback releases its lease.
        oldEntry.Release();

        // A concurrent commit invalidates the key; the source the new entry relies on must still be cancelled.
        registry.Invalidate("key");

        newEntry.Token.IsCancellationRequested.Should().BeTrue();
    }

    [Test]
    public void when_invalidate_runs_it_cancels_every_outstanding_lease_for_the_key()
    {
        var registry = new KeyValueCacheEvictionTokens();

        var first = registry.Acquire("key");
        var second = registry.Acquire("key");

        registry.Invalidate("key");

        first.Token.IsCancellationRequested.Should().BeTrue();
        second.Token.IsCancellationRequested.Should().BeTrue();
    }

    [Test]
    public void when_all_entries_are_released_then_the_next_acquire_starts_a_fresh_generation()
    {
        var registry = new KeyValueCacheEvictionTokens();

        var first = registry.Acquire("key");
        first.Release();

        // The previous generation was unmapped, so a new lease is not already cancelled.
        var second = registry.Acquire("key");
        second.Token.IsCancellationRequested.Should().BeFalse();
    }

    [Test]
    public async Task expiration_repopulation_racing_with_invalidation_never_orphans_a_leased_source()
    {
        var registry = new KeyValueCacheEvictionTokens();
        var orphaned = 0;

        void Hammer(int worker)
        {
            // One writer per key mirrors the real per-questionnaire flow; the 8 workers still run concurrently.
            var key = "key-" + worker;
            for (var i = 0; i < 50000; i++)
            {
                var oldEntry = registry.Acquire(key); // previously cached entry
                var newEntry = registry.Acquire(key); // in-flight repopulation reusing the generation

                oldEntry.Release();                    // old entry expires/evicts first
                registry.Invalidate(key);              // a commit invalidates the key

                // The new entry's source must survive the old entry's eviction so this commit can cancel it.
                if (!newEntry.Token.IsCancellationRequested)
                    Interlocked.Increment(ref orphaned);

                newEntry.Release();
            }
        }

        var workers = Enumerable.Range(0, 8).Select(w => Task.Run(() => Hammer(w))).ToArray();
        await Task.WhenAll(workers);

        orphaned.Should().Be(0);
    }
}
