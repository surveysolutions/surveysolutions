using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using WB.UI.Designer.Services;

namespace WB.Tests.Unit.Designer.Services
{
    [TestFixture]
    public class OneTimeCodeStoreTests
    {
        private static InMemoryOneTimeCodeStore MakeStore()
            => new InMemoryOneTimeCodeStore(new MemoryCache(Options.Create(new MemoryCacheOptions())));

        private static OneTimeCodeEntity MakeEntity(string code, int ttlSeconds = 60, bool used = false)
            => new OneTimeCodeEntity
            {
                Code          = code,
                UserId        = "user1",
                CorrelationId = "corr1",
                TargetService = "WB.WebTester",
                QuestionnaireId = Guid.NewGuid(),
                CreatedAt     = DateTime.UtcNow,
                ExpiresAt     = DateTime.UtcNow.AddSeconds(ttlSeconds),
                Used          = used
            };

        // --- SaveAsync / GetAsync ---

        [Test]
        public async Task GetAsync_returns_null_for_unknown_code()
        {
            var store = MakeStore();
            var result = await store.GetAsync("unknown");
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetAsync_returns_saved_entity()
        {
            var store = MakeStore();
            var entity = MakeEntity("abc123");
            await store.SaveAsync(entity);

            var result = await store.GetAsync("abc123");
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Code, Is.EqualTo("abc123"));
        }

        // --- TryMarkAsUsedAsync ---

        [Test]
        public async Task TryMarkAsUsedAsync_returns_false_for_unknown_code()
        {
            var store = MakeStore();
            var result = await store.TryMarkAsUsedAsync("nocode", DateTime.UtcNow);
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task TryMarkAsUsedAsync_returns_true_on_first_call()
        {
            var store = MakeStore();
            await store.SaveAsync(MakeEntity("code1"));

            var result = await store.TryMarkAsUsedAsync("code1", DateTime.UtcNow);
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task TryMarkAsUsedAsync_sets_Used_flag_and_timestamp()
        {
            var store = MakeStore();
            var entity = MakeEntity("code2");
            await store.SaveAsync(entity);

            // Capture the entity reference before mark-as-used removes it from the cache.
            var saved = await store.GetAsync("code2");
            Assert.That(saved, Is.Not.Null);

            var usedAt = DateTime.UtcNow;
            await store.TryMarkAsUsedAsync("code2", usedAt);

            // The in-memory entity object is mutated in place even after cache removal.
            Assert.That(saved!.Used, Is.True);
            Assert.That(saved.UsedAt, Is.EqualTo(usedAt).Within(TimeSpan.FromMilliseconds(1)));
        }

        [Test]
        public async Task TryMarkAsUsedAsync_returns_false_on_second_call_same_code()
        {
            var store = MakeStore();
            await store.SaveAsync(MakeEntity("code3"));

            var first  = await store.TryMarkAsUsedAsync("code3", DateTime.UtcNow);
            var second = await store.TryMarkAsUsedAsync("code3", DateTime.UtcNow);

            Assert.That(first,  Is.True);
            Assert.That(second, Is.False);
        }

        [Test]
        public async Task TryMarkAsUsedAsync_is_atomic_under_concurrent_calls()
        {
            var store = MakeStore();
            await store.SaveAsync(MakeEntity("race"));

            // Fire 20 concurrent mark-as-used calls
            var tasks = new Task<bool>[20];
            for (int i = 0; i < tasks.Length; i++)
                tasks[i] = store.TryMarkAsUsedAsync("race", DateTime.UtcNow);

            var results = await Task.WhenAll(tasks);

            int successCount = 0;
            foreach (var r in results)
                if (r) successCount++;

            Assert.That(successCount, Is.EqualTo(1), "Exactly one concurrent call should win.");
        }
    }
}
