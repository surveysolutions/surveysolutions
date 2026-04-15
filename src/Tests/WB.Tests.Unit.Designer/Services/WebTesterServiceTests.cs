#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WB.Tests.Abc;
using WB.UI.Designer.Services;

namespace WB.Tests.Unit.Designer.Services
{
    [TestFixture]
    public class WebTesterServiceTests
    {
        private static WebTesterService CreateService(
            IOneTimeCodeStore? store = null,
            int codeTtlSeconds = 60)
        {
            store ??= new InMemoryOneTimeCodeStore();
            var settings = Options.Create(new WebTesterSettings { CodeTtlSeconds = codeTtlSeconds });
            var logger = Mock.Of<ILogger<WebTesterService>>();
            return new WebTesterService(store, settings, logger);
        }

        [Test]
        public async Task CreateOneTimeCode_returns_nonempty_code()
        {
            var svc = CreateService();
            var code = await svc.CreateOneTimeCodeAsync(Id.g1, "user1", "corr1");
            Assert.That(code, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task CreateOneTimeCode_stores_entity_in_store()
        {
            var store = new InMemoryOneTimeCodeStore();
            var svc = CreateService(store);

            var code = await svc.CreateOneTimeCodeAsync(Id.g1, "user1", "corr1");

            var entity = await store.GetAsync(code);
            Assert.That(entity, Is.Not.Null);
            Assert.That(entity!.UserId, Is.EqualTo("user1"));
            Assert.That(entity.CorrelationId, Is.EqualTo("corr1"));
            Assert.That(entity.QuestionnaireId, Is.EqualTo(Id.g1));
            Assert.That(entity.TargetService, Is.EqualTo("WB.WebTester"));
            Assert.That(entity.Used, Is.False);
        }

        [Test]
        public async Task CreateOneTimeCode_generates_different_codes_each_time()
        {
            var svc = CreateService();
            var code1 = await svc.CreateOneTimeCodeAsync(Id.g1, null, "c1");
            var code2 = await svc.CreateOneTimeCodeAsync(Id.g1, null, "c2");
            Assert.That(code1, Is.Not.EqualTo(code2));
        }

        [Test]
        public async Task CreateOneTimeCode_sets_expiry_from_ttl_config()
        {
            var store2 = new InMemoryOneTimeCodeStore();
            var svc2   = CreateService(store2, codeTtlSeconds: 30);
            var before = DateTime.UtcNow;
            var code = await svc2.CreateOneTimeCodeAsync(Id.g1, null, "c");

            var entity = await store2.GetAsync(code);
            Assert.That(entity!.ExpiresAt, Is.GreaterThan(before.AddSeconds(25)));
            Assert.That(entity.ExpiresAt, Is.LessThan(before.AddSeconds(35)));
        }

        [Test]
        public async Task CreateOneTimeCode_accepts_null_userId_for_anonymous_sessions()
        {
            var store = new InMemoryOneTimeCodeStore();
            var svc = CreateService(store);
            var code = await svc.CreateOneTimeCodeAsync(Id.g1, null, "corr");
            var entity = await store.GetAsync(code);
            Assert.That(entity!.UserId, Is.Null);
        }
    }
}
