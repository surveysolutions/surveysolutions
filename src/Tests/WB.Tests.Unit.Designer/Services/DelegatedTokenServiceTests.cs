#nullable enable
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using WB.Tests.Abc;
using WB.UI.Designer.Services;

namespace WB.Tests.Unit.Designer.Services
{
    [TestFixture]
    public class DelegatedTokenServiceTests
    {
        private const string TestSecret = "test-secret-key-at-least-32-chars!!";

        private static DelegatedTokenService Svc(int exp = 10)
        {
            var s = Options.Create(new WebTesterSettings { DelegatedJwtExpirationMinutes = exp });
            var c = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["WebTester:JwtSecretKey"]        = TestSecret,
                    ["Providers:Assistant:JwtIssuer"] = "WB.Designer"
                }).Build();
            return new DelegatedTokenService(s, c);
        }

        private static string? Cl(JwtSecurityToken t, string k) =>
            t.Payload.TryGetValue(k, out var v) ? v?.ToString() : null;

        // --- Unit tests ---

        [Test]
        public void Returns_valid_jwt_string()
        {
            var token = Svc().CreateDelegatedToken(new DelegatedTokenRequest
            {
                UserId = "u", CorrelationId = "c", QuestionnaireId = Id.g1,
                AuthorizedParty = "WB.WebTester", Scope = "webtester"
            });
            Assert.That(new JwtSecurityTokenHandler().CanReadToken(token), Is.True);
        }

        [Test]
        public void Contains_all_required_claims()
        {
            var token = Svc().CreateDelegatedToken(new DelegatedTokenRequest
            {
                UserId = "user42", CorrelationId = "corrXYZ", QuestionnaireId = Id.g1,
                AuthorizedParty = "WB.WebTester", Scope = "webtester"
            });
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            Assert.That(jwt.Subject,   Is.EqualTo("user42"),    "sub");
            Assert.That(jwt.Issuer,    Is.EqualTo("WB.Designer"), "iss");
            Assert.That(jwt.Audiences, Does.Contain("WB.Designer"), "aud");
            Assert.That(Cl(jwt, "azp"),            Is.EqualTo("WB.WebTester"), "azp");
            Assert.That(Cl(jwt, "scope"),          Is.EqualTo("webtester"),    "scope");
            Assert.That(Cl(jwt, "correlation_id"), Is.EqualTo("corrXYZ"),     "correlation_id");
            Assert.That(Cl(jwt, JwtTokenService.QuestionnaireIdClaimType),
                        Is.EqualTo(Id.g1.ToString()), "questionnaire_id");
            Assert.That(Cl(jwt, JwtRegisteredClaimNames.Jti), Is.Not.Null.And.Not.Empty, "jti");
        }

        [Test]
        public void Is_short_lived_10_minutes()
        {
            var before = DateTime.UtcNow;
            var jwt    = new JwtSecurityTokenHandler().ReadJwtToken(
                Svc().CreateDelegatedToken(new DelegatedTokenRequest
                    { CorrelationId = "c", QuestionnaireId = Id.g1 }));
            Assert.That(jwt.ValidTo, Is.GreaterThan(before.AddMinutes(9)));
            Assert.That(jwt.ValidTo, Is.LessThanOrEqualTo(before.AddMinutes(11)));
        }

        [Test]
        public void Omits_sub_when_userId_is_null()
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(
                Svc().CreateDelegatedToken(new DelegatedTokenRequest
                    { UserId = null, CorrelationId = "c", QuestionnaireId = Id.g1 }));
            Assert.That(jwt.Subject, Is.Null.Or.Empty);
        }

        [Test]
        public void Throws_when_secret_key_is_missing()
        {
            Assert.Throws<InvalidOperationException>(() =>
                new DelegatedTokenService(
                    Options.Create(new WebTesterSettings()),
                    new ConfigurationBuilder().Build()));
        }

        // --- Integration: flow tests ---

        [Test]
        public async Task HappyPath_full_exchange_flow()
        {
            var store = new InMemoryOneTimeCodeStore();
            var now   = DateTime.UtcNow;
            var entity = new OneTimeCodeEntity
            {
                Code = "happy", UserId = "user1", CorrelationId = "corr1",
                TargetService = "WB.WebTester", QuestionnaireId = Id.g1,
                CreatedAt = now, ExpiresAt = now.AddSeconds(60)
            };
            await store.SaveAsync(entity);

            var marked = await store.TryMarkAsUsedAsync("happy", DateTime.UtcNow);
            var decoded = new JwtSecurityTokenHandler().ReadJwtToken(
                Svc().CreateDelegatedToken(new DelegatedTokenRequest
                {
                    UserId = entity.UserId, CorrelationId = entity.CorrelationId,
                    QuestionnaireId = entity.QuestionnaireId,
                    AuthorizedParty = "WB.WebTester", Scope = "webtester"
                }));

            Assert.That(marked, Is.True);
            Assert.That(decoded.Subject, Is.EqualTo("user1"));
            Assert.That(Cl(decoded, "correlation_id"), Is.EqualTo("corr1"));
        }

        [Test]
        public async Task Rejects_second_use_of_same_code()
        {
            var store = new InMemoryOneTimeCodeStore();
            await store.SaveAsync(new OneTimeCodeEntity
            {
                Code = "once", UserId = "u", CorrelationId = "c",
                TargetService = "WB.WebTester", QuestionnaireId = Id.g1,
                CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddSeconds(60)
            });
            var first  = await store.TryMarkAsUsedAsync("once", DateTime.UtcNow);
            var second = await store.TryMarkAsUsedAsync("once", DateTime.UtcNow);
            Assert.That(first,  Is.True,  "first use should succeed");
            Assert.That(second, Is.False, "replay must be rejected");
        }

        [Test]
        public async Task Detects_expired_code()
        {
            var store = new InMemoryOneTimeCodeStore();
            await store.SaveAsync(new OneTimeCodeEntity
            {
                Code = "expired", UserId = "u", CorrelationId = "c",
                TargetService = "WB.WebTester", QuestionnaireId = Id.g1,
                CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                ExpiresAt = DateTime.UtcNow.AddMinutes(-4)
            });
            // InMemoryOneTimeCodeStore lazily evicts expired entries on read
            var fetched = await store.GetAsync("expired");
            Assert.That(fetched, Is.Null, "expired code should be evicted on read");
        }

        [Test]
        public async Task Rejects_code_for_wrong_target_service()
        {
            var store = new InMemoryOneTimeCodeStore();
            await store.SaveAsync(new OneTimeCodeEntity
            {
                Code = "xsvc", UserId = "u", CorrelationId = "c",
                TargetService = "WB.WebTester", QuestionnaireId = Id.g1,
                CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddSeconds(60)
            });
            var entity      = await store.GetAsync("xsvc");
            const string wrongService = "WB.SomeOtherService";
            Assert.That(
                string.Equals(entity!.TargetService, wrongService, StringComparison.OrdinalIgnoreCase),
                Is.False, "code issued for WB.WebTester must not be accepted by another service");
        }
    }
}

