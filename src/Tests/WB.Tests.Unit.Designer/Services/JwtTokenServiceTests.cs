#nullable enable
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.UI.Designer.Services;

namespace WB.Tests.Unit.Designer.Services
{
    [TestFixture]
    [TestOf(typeof(JwtTokenService))]
    public class JwtTokenServiceTests
    {
        private const string Secret = "assistant-secret-key-at-least-32-chars!!";

        private static JwtTokenService CreateService(Dictionary<string, string?>? values = null)
        {
            var all = new Dictionary<string, string?> { ["Providers:Assistant:JwtSecretKey"] = Secret };
            if (values != null)
                foreach (var kv in values) all[kv.Key] = kv.Value;

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(all).Build();
            return new JwtTokenService(configuration);
        }

        private static DesignerIdentityUser CreateUser(string? email = "user@example.org", string? userName = "user")
            => new DesignerIdentityUser { Id = Guid.NewGuid(), Email = email, UserName = userName };

        private static JwtSecurityToken Read(string token) => new JwtSecurityTokenHandler().ReadJwtToken(token);

        [Test]
        public void should_throw_when_secret_key_is_missing()
        {
            var service = new JwtTokenService(new ConfigurationBuilder().Build());

            Assert.Throws<InvalidOperationException>(() => service.GenerateToken(CreateUser()));
        }

        [TestCase("")]
        [TestCase("   ")]
        public void should_throw_when_secret_key_is_blank(string secret)
        {
            var service = CreateService(new Dictionary<string, string?> { ["Providers:Assistant:JwtSecretKey"] = secret });

            Assert.Throws<InvalidOperationException>(() => service.GenerateToken(CreateUser()));
        }

        [Test]
        public void should_include_user_claims()
        {
            var user = CreateUser("a@b.c", "john");

            var jwt = Read(CreateService().GenerateToken(user));

            jwt.Subject.Should().Be(user.Id.ToString());
            jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value.Should().Be("a@b.c");
            jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.UniqueName).Value.Should().Be("john");
            jwt.Claims.Should().Contain(c => c.Value == user.Id.ToString());
            jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti && c.Value.Length > 0);
        }

        [Test]
        public void should_use_empty_strings_when_email_and_username_are_null()
        {
            var jwt = Read(CreateService().GenerateToken(CreateUser(null, null)));

            jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value.Should().BeEmpty();
            jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.UniqueName).Value.Should().BeEmpty();
        }

        [Test]
        public void should_generate_unique_jti_for_each_token()
        {
            var service = CreateService();
            var user = CreateUser();

            var jti1 = Read(service.GenerateToken(user)).Id;
            var jti2 = Read(service.GenerateToken(user)).Id;

            jti1.Should().NotBe(jti2);
        }

        [Test]
        public void should_use_default_issuer_and_audience()
        {
            var jwt = Read(CreateService().GenerateToken(CreateUser()));

            jwt.Issuer.Should().Be("WB.Designer");
            jwt.Audiences.Should().ContainSingle().Which.Should().Be("WB.AssistantService");
        }

        [Test]
        public void should_use_configured_issuer_and_audience()
        {
            var service = CreateService(new Dictionary<string, string?>
            {
                ["Providers:Assistant:JwtIssuer"] = "custom-issuer",
                ["Providers:Assistant:JwtAudience"] = "custom-audience"
            });

            var jwt = Read(service.GenerateToken(CreateUser()));

            jwt.Issuer.Should().Be("custom-issuer");
            jwt.Audiences.Should().ContainSingle().Which.Should().Be("custom-audience");
        }

        [Test]
        public void should_default_expiration_to_30_minutes_when_not_configured()
        {
            var before = DateTime.UtcNow;

            var jwt = Read(CreateService().GenerateToken(CreateUser()));

            jwt.ValidTo.Should().BeCloseTo(before.AddMinutes(30), TimeSpan.FromMinutes(1));
        }

        [Test]
        public void should_use_configured_expiration()
        {
            var service = CreateService(new Dictionary<string, string?> { ["Providers:Assistant:JwtExpirationMinutes"] = "5" });
            var before = DateTime.UtcNow;

            var jwt = Read(service.GenerateToken(CreateUser()));

            jwt.ValidTo.Should().BeCloseTo(before.AddMinutes(5), TimeSpan.FromMinutes(1));
        }

        [Test]
        public void should_sign_token_with_hmac_sha256_using_configured_key()
        {
            var token = CreateService().GenerateToken(CreateUser());

            Read(token).SignatureAlgorithm.Should().Be(SecurityAlgorithms.HmacSha256);

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret))
            };
            new JwtSecurityTokenHandler().ValidateToken(token, parameters, out var validated);
            validated.Should().NotBeNull();
        }

        [Test]
        public void should_fail_validation_with_a_different_key()
        {
            var token = CreateService().GenerateToken(CreateUser());
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("another-secret-key-at-least-32-chars!!"))
            };

            Assert.Catch<SecurityTokenValidationException>(
                () => new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _));
        }

        [Test]
        public void should_expose_scheme_constants()
        {
            JwtTokenService.AssistantScheme.Should().Be("assistant");
            JwtTokenService.QuestionnaireIdClaimType.Should().Be("questionnaire_id");
        }
    }
}
