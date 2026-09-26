#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WB.Tests.Abc;
using WB.UI.Designer.Controllers.Api.Internal;
using WB.UI.Designer.Models;
using WB.UI.Designer.Services;

namespace WB.Tests.Unit.Designer.Services
{
    [TestFixture]
    [TestOf(typeof(AuthExchangeController))]
    public class AuthExchangeControllerTests
    {
        private const string ValidServiceKey = "a]9mZ#Lk2!vRqW8xTdP7nYf3jBu6hC0s"; // 32 chars

        private static AuthExchangeController CreateController(
            IOneTimeCodeStore? codeStore = null,
            IDelegatedTokenService? tokenService = null,
            string? serviceApiKey = null,
            int delegatedJwtMinutes = 10)
        {
            codeStore ??= new InMemoryOneTimeCodeStore(new MemoryCache(Options.Create(new MemoryCacheOptions())));
            tokenService ??= Mock.Of<IDelegatedTokenService>(
                s => s.CreateDelegatedToken(It.IsAny<DelegatedTokenRequest>()) == "test-jwt-token");

            var settings = Options.Create(new WebTesterSettings
            {
                ServiceApiKey = serviceApiKey ?? ValidServiceKey,
                DelegatedJwtExpirationMinutes = delegatedJwtMinutes
            });

            return new AuthExchangeController(
                codeStore,
                tokenService,
                settings,
                Mock.Of<ILogger<AuthExchangeController>>());
        }

        private static async Task SaveValidCode(IOneTimeCodeStore store, string code,
            Guid questionnaireId, string targetService = "WB.WebTester")
        {
            var now = DateTime.UtcNow;
            await store.SaveAsync(new OneTimeCodeEntity
            {
                Code = code,
                UserId = "user1",
                CorrelationId = "corr1",
                TargetService = targetService,
                QuestionnaireId = questionnaireId,
                CreatedAt = now,
                ExpiresAt = now.AddSeconds(60)
            });
        }

        // --- Happy path ---

        [Test]
        public async Task when_valid_credentials_and_code_should_return_ok_with_token()
        {
            var store = new InMemoryOneTimeCodeStore(new MemoryCache(Options.Create(new MemoryCacheOptions())));
            await SaveValidCode(store, "validcode123", Id.g1);

            var controller = CreateController(codeStore: store);
            var result = await controller.Exchange(
                new ExchangeCodeRequest { Code = "validcode123" },
                AuthExchangeController.ExpectedServiceName,
                ValidServiceKey,
                CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
            var ok = (OkObjectResult)result;
            var response = ok.Value as ExchangeCodeResponse;
            response.Should().NotBeNull();
            response!.AccessToken.Should().Be("test-jwt-token");
            response.QuestionnaireId.Should().Be(Id.g1.ToString());
            response.UserId.Should().Be("user1");
            response.CorrelationId.Should().Be("corr1");
        }

        // --- Authentication failures ---

        [Test]
        public async Task when_missing_service_name_header_should_return_unauthorized()
        {
            var controller = CreateController();
            var result = await controller.Exchange(
                new ExchangeCodeRequest { Code = "test" },
                null, ValidServiceKey, CancellationToken.None);

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Test]
        public async Task when_missing_service_key_header_should_return_unauthorized()
        {
            var controller = CreateController();
            var result = await controller.Exchange(
                new ExchangeCodeRequest { Code = "test" },
                AuthExchangeController.ExpectedServiceName, null, CancellationToken.None);

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Test]
        public async Task when_wrong_service_name_should_return_unauthorized()
        {
            var controller = CreateController();
            var result = await controller.Exchange(
                new ExchangeCodeRequest { Code = "test" },
                "WB.SomeOtherService", ValidServiceKey, CancellationToken.None);

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Test]
        public async Task when_wrong_service_key_should_return_unauthorized()
        {
            var controller = CreateController();
            var result = await controller.Exchange(
                new ExchangeCodeRequest { Code = "test" },
                AuthExchangeController.ExpectedServiceName,
                "wrong-key-that-is-32-chars-long!",
                CancellationToken.None);

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Test]
        public void when_service_api_key_not_configured_should_throw()
        {
            var controller = CreateController(serviceApiKey: "");

            var act = async () => await controller.Exchange(
                new ExchangeCodeRequest { Code = "test" },
                AuthExchangeController.ExpectedServiceName,
                "any-key",
                CancellationToken.None);

            act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*must be configured*");
        }

        [Test]
        public void when_service_api_key_too_short_should_throw()
        {
            var controller = CreateController(serviceApiKey: "short");

            var act = async () => await controller.Exchange(
                new ExchangeCodeRequest { Code = "test" },
                AuthExchangeController.ExpectedServiceName,
                "short",
                CancellationToken.None);

            act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*at least 32*");
        }

        // --- Input validation ---

        [Test]
        public async Task when_empty_code_should_return_bad_request()
        {
            var controller = CreateController();
            var result = await controller.Exchange(
                new ExchangeCodeRequest { Code = "" },
                AuthExchangeController.ExpectedServiceName,
                ValidServiceKey,
                CancellationToken.None);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task when_code_too_long_should_return_bad_request()
        {
            var controller = CreateController();
            var result = await controller.Exchange(
                new ExchangeCodeRequest { Code = new string('A', 129) },
                AuthExchangeController.ExpectedServiceName,
                ValidServiceKey,
                CancellationToken.None);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task when_code_contains_invalid_chars_should_return_bad_request()
        {
            var controller = CreateController();
            var result = await controller.Exchange(
                new ExchangeCodeRequest { Code = "abc!@#$" },
                AuthExchangeController.ExpectedServiceName,
                ValidServiceKey,
                CancellationToken.None);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // --- Code validation ---

        [Test]
        public async Task when_unknown_code_should_return_bad_request()
        {
            var controller = CreateController();
            var result = await controller.Exchange(
                new ExchangeCodeRequest { Code = "nonexistentcode" },
                AuthExchangeController.ExpectedServiceName,
                ValidServiceKey,
                CancellationToken.None);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task when_expired_code_should_return_410_gone()
        {
            var store = new InMemoryOneTimeCodeStore(new MemoryCache(Options.Create(new MemoryCacheOptions())));
            var now = DateTime.UtcNow;
            await store.SaveAsync(new OneTimeCodeEntity
            {
                Code = "expiredcode",
                UserId = "u",
                CorrelationId = "c",
                TargetService = "WB.WebTester",
                QuestionnaireId = Id.g1,
                CreatedAt = now.AddMinutes(-10),
                ExpiresAt = now.AddMinutes(-5)
            });
            // Force the code to be retrievable (InMemoryOneTimeCodeStore evicts on read,
            // so we use a mock to keep it despite the expiry).
            var mockStore = new Mock<IOneTimeCodeStore>();
            mockStore.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OneTimeCodeEntity
                {
                    Code = "expiredcode",
                    UserId = "u",
                    CorrelationId = "c",
                    TargetService = "WB.WebTester",
                    QuestionnaireId = Id.g1,
                    CreatedAt = now.AddMinutes(-10),
                    ExpiresAt = now.AddMinutes(-5)
                });

            var controller = CreateController(codeStore: mockStore.Object);
            var result = await controller.Exchange(
                new ExchangeCodeRequest { Code = "expiredcode" },
                AuthExchangeController.ExpectedServiceName,
                ValidServiceKey,
                CancellationToken.None);

            var statusResult = result as ObjectResult;
            statusResult.Should().NotBeNull();
            statusResult!.StatusCode.Should().Be(410);
        }

        [Test]
        public async Task when_code_for_different_service_should_return_403()
        {
            var store = new InMemoryOneTimeCodeStore(new MemoryCache(Options.Create(new MemoryCacheOptions())));
            await SaveValidCode(store, "svcmismatch", Id.g1, targetService: "WB.OtherService");

            var controller = CreateController(codeStore: store);
            var result = await controller.Exchange(
                new ExchangeCodeRequest { Code = "svcmismatch" },
                AuthExchangeController.ExpectedServiceName,
                ValidServiceKey,
                CancellationToken.None);

            var statusResult = result as ObjectResult;
            statusResult.Should().NotBeNull();
            statusResult!.StatusCode.Should().Be(403);
        }

        [Test]
        public async Task when_code_already_used_should_return_conflict()
        {
            var store = new InMemoryOneTimeCodeStore(new MemoryCache(Options.Create(new MemoryCacheOptions())));
            await SaveValidCode(store, "usedonce", Id.g1);

            var controller = CreateController(codeStore: store);

            // First use succeeds
            var first = await controller.Exchange(
                new ExchangeCodeRequest { Code = "usedonce" },
                AuthExchangeController.ExpectedServiceName,
                ValidServiceKey,
                CancellationToken.None);
            first.Should().BeOfType<OkObjectResult>();

            // Second use with same code fails
            var second = await controller.Exchange(
                new ExchangeCodeRequest { Code = "usedonce" },
                AuthExchangeController.ExpectedServiceName,
                ValidServiceKey,
                CancellationToken.None);

            // Code is marked as used, so either BadRequest (code not found after eviction)
            // or Conflict (code already used)
            second.Should().Match<IActionResult>(r =>
                r is ConflictObjectResult || r is BadRequestObjectResult);
        }

        // --- Constant-time comparison ---

        [Test]
        public async Task when_service_key_differs_by_length_should_return_unauthorized()
        {
            var controller = CreateController();
            var result = await controller.Exchange(
                new ExchangeCodeRequest { Code = "test" },
                AuthExchangeController.ExpectedServiceName,
                "too-short",
                CancellationToken.None);

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Test]
        public async Task when_service_name_case_insensitive_should_accept()
        {
            var store = new InMemoryOneTimeCodeStore(new MemoryCache(Options.Create(new MemoryCacheOptions())));
            await SaveValidCode(store, "casetest", Id.g1);

            var controller = CreateController(codeStore: store);
            var result = await controller.Exchange(
                new ExchangeCodeRequest { Code = "casetest" },
                "wb.webtester", // lowercase
                ValidServiceKey,
                CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
