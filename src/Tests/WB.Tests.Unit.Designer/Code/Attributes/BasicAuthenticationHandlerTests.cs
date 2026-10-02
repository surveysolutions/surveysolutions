#nullable enable
using System;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WB.UI.Designer.Code.Attributes;
using WB.UI.Shared.Web.Authentication;

namespace WB.Tests.Unit.Designer.Code.Attributes
{
    [TestFixture]
    [TestOf(typeof(BasicAuthenticationHandler))]
    public class BasicAuthenticationHandlerTests
    {
        private const string SchemeName = "Basic";
        private Mock<IBasicAuthenticationService> userService = null!;
        private DefaultHttpContext context = null!;
        private MemoryStream body = null!;

        [SetUp]
        public void Setup()
        {
            userService = new Mock<IBasicAuthenticationService>();
            body = new MemoryStream();
            context = new DefaultHttpContext();
            context.Response.Body = body;
        }

        private async Task<BasicAuthenticationHandler> CreateHandler()
        {
            var options = new Mock<IOptionsMonitor<BasicAuthenticationSchemeOptions>>();
            options.Setup(x => x.Get(It.IsAny<string>()))
                .Returns(new BasicAuthenticationSchemeOptions { Realm = "Designer" });

            var handler = new BasicAuthenticationHandler(options.Object, NullLoggerFactory.Instance,
                UrlEncoder.Default, userService.Object);
            await handler.InitializeAsync(
                new AuthenticationScheme(SchemeName, null, typeof(BasicAuthenticationHandler)), context);
            return handler;
        }

        private static string Basic(string raw) => "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));

        [Test]
        public async Task should_return_no_result_when_authorization_header_is_absent()
        {
            var handler = await CreateHandler();

            var result = await handler.AuthenticateAsync();

            result.None.Should().BeTrue();
            userService.Verify(x => x.AuthenticateAsync(It.IsAny<BasicCredentials>()), Times.Never);
        }

        [Test]
        public async Task should_return_no_result_when_header_has_no_parameter()
        {
            context.Request.Headers["Authorization"] = "Basic";
            var handler = await CreateHandler();

            var result = await handler.AuthenticateAsync();

            result.None.Should().BeTrue();
            userService.Verify(x => x.AuthenticateAsync(It.IsAny<BasicCredentials>()), Times.Never);
        }

        [Test]
        public async Task should_succeed_and_pass_parsed_credentials_to_service()
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "user") }, "Basic"));
            userService.Setup(x => x.AuthenticateAsync(It.IsAny<BasicCredentials>())).ReturnsAsync(principal);
            context.Request.Headers["Authorization"] = Basic("user:pwd");
            var handler = await CreateHandler();

            var result = await handler.AuthenticateAsync();

            result.Succeeded.Should().BeTrue();
            result.Ticket!.AuthenticationScheme.Should().Be(SchemeName);
            result.Principal.Should().BeSameAs(principal);
            userService.Verify(x => x.AuthenticateAsync(
                It.Is<BasicCredentials>(c => c.Username == "user" && c.Password == "pwd")), Times.Once);
        }

        [Test]
        public async Task should_fail_with_unauthorized_exception_from_service()
        {
            var exception = new UnauthorizedException("denied", StatusCodes.Status401Unauthorized);
            userService.Setup(x => x.AuthenticateAsync(It.IsAny<BasicCredentials>())).ThrowsAsync(exception);
            context.Request.Headers["Authorization"] = Basic("user:pwd");
            var handler = await CreateHandler();

            var result = await handler.AuthenticateAsync();

            result.Succeeded.Should().BeFalse();
            result.Failure.Should().BeSameAs(exception);
        }

        [Test]
        public async Task should_fail_with_generic_message_on_unexpected_exception()
        {
            userService.Setup(x => x.AuthenticateAsync(It.IsAny<BasicCredentials>()))
                .ThrowsAsync(new InvalidOperationException("boom"));
            context.Request.Headers["Authorization"] = Basic("user:pwd");
            var handler = await CreateHandler();

            var result = await handler.AuthenticateAsync();

            result.Succeeded.Should().BeFalse();
            result.Failure!.Message.Should().Be("Can't authorize user");
        }

        [Test]
        public async Task should_fail_when_credentials_are_not_valid_base64()
        {
            context.Request.Headers["Authorization"] = "Basic !!!not-base64!!!";
            var handler = await CreateHandler();

            var result = await handler.AuthenticateAsync();

            result.Succeeded.Should().BeFalse();
            result.Failure!.Message.Should().Be("Can't authorize user");
            userService.Verify(x => x.AuthenticateAsync(It.IsAny<BasicCredentials>()), Times.Never);
        }

        [Test]
        public async Task should_fail_when_credentials_have_no_password_separator()
        {
            context.Request.Headers["Authorization"] = Basic("userwithoutcolon");
            var handler = await CreateHandler();

            var result = await handler.AuthenticateAsync();

            result.Succeeded.Should().BeFalse();
            result.Failure!.Message.Should().Be("Can't authorize user");
        }

        [Test]
        public async Task should_fail_when_authorization_header_is_malformed()
        {
            context.Request.Headers["Authorization"] = "";
            var handler = await CreateHandler();

            var result = await handler.AuthenticateAsync();

            result.Succeeded.Should().BeFalse();
        }

        [Test]
        public async Task should_write_www_authenticate_header_on_challenge_without_prior_failure()
        {
            var handler = await CreateHandler();

            await handler.ChallengeAsync(new AuthenticationProperties());

            context.Response.Headers["WWW-Authenticate"].ToString()
                .Should().Be("Basic realm=\"Designer\", charset=\"UTF-8\"");
            body.Length.Should().Be(0);
        }

        [Test]
        public async Task should_write_status_and_message_on_challenge_after_unauthorized_failure()
        {
            userService.Setup(x => x.AuthenticateAsync(It.IsAny<BasicCredentials>()))
                .ThrowsAsync(new UnauthorizedException("User is locked", StatusCodes.Status401Unauthorized));
            context.Request.Headers["Authorization"] = Basic("user:pwd");
            var handler = await CreateHandler();
            await handler.AuthenticateAsync();

            await handler.ChallengeAsync(new AuthenticationProperties());

            context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            Encoding.UTF8.GetString(body.ToArray()).Should().Be("User is locked");
            context.Response.Headers["WWW-Authenticate"].ToString().Should().Contain("Basic realm=\"Designer\"");
        }

        [Test]
        public async Task should_use_same_failure_handling_on_forbid()
        {
            userService.Setup(x => x.AuthenticateAsync(It.IsAny<BasicCredentials>()))
                .ThrowsAsync(new UnauthorizedException("nope", StatusCodes.Status403Forbidden));
            context.Request.Headers["Authorization"] = Basic("user:pwd");
            var handler = await CreateHandler();
            await handler.AuthenticateAsync();

            await handler.ForbidAsync(new AuthenticationProperties());

            context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
            Encoding.UTF8.GetString(body.ToArray()).Should().Be("nope");
            context.Response.Headers.ContainsKey("WWW-Authenticate").Should().BeTrue();
        }
    }
}

