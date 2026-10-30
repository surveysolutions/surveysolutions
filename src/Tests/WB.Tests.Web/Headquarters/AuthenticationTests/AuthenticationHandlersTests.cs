using System;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.Domain;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Code.Authentication;

namespace WB.Tests.Web.Headquarters.AuthenticationTests
{
    [TestFixture]
    public class AuthenticationHandlersTests
    {
        private static readonly ILoggerFactory LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(_ => { });

        private static readonly AuthenticationScheme BasicScheme =
            new(AuthType.Basic, null, typeof(BasicAuthenticationHandler));

        private static readonly AuthenticationScheme AuthTokenScheme =
            new(AuthType.AuthToken, null, typeof(AuthTokenAuthenticationHandler));

        [Test]
        public async Task when_basic_handler_receives_authtoken_scheme_should_return_no_result()
        {
            var executor = new Mock<IInScopeExecutor>();
            var handler = new BasicAuthenticationHandler(
                CreateOptionsMonitor(new WB.UI.Shared.Web.Authentication.BasicAuthenticationSchemeOptions()),
                LoggerFactory,
                UrlEncoder.Default,
                Mock.Of<IUserClaimsPrincipalFactory<HqUser>>(),
                executor.Object);

            await InitializeHandler(handler, BasicScheme, $"AuthToken {EncodeCredentials()}");

            var result = await handler.AuthenticateAsync();

            Assert.That(result.None, Is.True);
            executor.Verify(x => x.ExecuteAsync(It.IsAny<Func<WB.Core.GenericSubdomains.Portable.ServiceLocation.IServiceLocator, Task<AuthenticateResult>>>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task when_auth_token_handler_receives_basic_scheme_should_return_no_result()
        {
            var userRepository = new Mock<IUserRepository>();
            var handler = new AuthTokenAuthenticationHandler(
                CreateOptionsMonitor(new AuthTokenAuthenticationSchemeOptions()),
                LoggerFactory,
                UrlEncoder.Default,
                userRepository.Object,
                Mock.Of<IUserClaimsPrincipalFactory<HqUser>>(),
                Mock.Of<IApiTokenProvider>(),
                Mock.Of<IWorkspaceContextAccessor>());

            await InitializeHandler(handler, AuthTokenScheme, $"Basic {EncodeCredentials()}");

            var result = await handler.AuthenticateAsync();

            Assert.That(result.None, Is.True);
            userRepository.Verify(x => x.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        private static async Task InitializeHandler<TOptions>(AuthenticationHandler<TOptions> handler,
            AuthenticationScheme scheme,
            string authorizationHeader)
            where TOptions : AuthenticationSchemeOptions, new()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Authorization = authorizationHeader;

            await handler.InitializeAsync(scheme, context);
        }

        private static IOptionsMonitor<TOptions> CreateOptionsMonitor<TOptions>(TOptions options)
            where TOptions : class, new()
        {
            var monitor = new Mock<IOptionsMonitor<TOptions>>();
            monitor.Setup(x => x.Get(It.IsAny<string>())).Returns(options);
            monitor.SetupGet(x => x.CurrentValue).Returns(options);
            return monitor.Object;
        }

        private static string EncodeCredentials() => Convert.ToBase64String("user:value"u8.ToArray());
    }
}
