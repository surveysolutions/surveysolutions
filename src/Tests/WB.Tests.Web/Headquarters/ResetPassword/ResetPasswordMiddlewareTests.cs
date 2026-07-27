using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Code.ResetPassword;
using WB.UI.Headquarters.Services.Impl;

namespace WB.Tests.Web.Headquarters.ResetPassword
{
    [TestOf(typeof(ResetPasswordMiddleware))]
    public class ResetPasswordMiddlewareTests
    {
        [Test]
        public async Task when_user_has_PasswordChangeRequired_and_is_not_observing_should_redirect_to_change_password()
        {
            var httpContext = CreateHttpContext("/Reports/Index",
                passwordChangeRequired: true,
                isObserving: false);

            var middleware = CreateMiddleware();
            await middleware.Invoke(httpContext);

            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status302Found);
            httpContext.Response.Headers["Location"].ToString().Should().Be("/ChangePassword");
        }

        [Test]
        public async Task when_user_has_PasswordChangeRequired_and_is_observing_should_not_redirect()
        {
            var httpContext = CreateHttpContext("/Reports/Index",
                passwordChangeRequired: true,
                isObserving: true);

            bool nextCalled = false;
            var middleware = CreateMiddleware(innerContext =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });
            await middleware.Invoke(httpContext);

            nextCalled.Should().BeTrue("observer should not be redirected to change password page");
            httpContext.Response.StatusCode.Should().NotBe(StatusCodes.Status302Found);
        }

        [Test]
        public async Task when_observing_and_navigating_to_ReturnToObserver_should_not_redirect()
        {
            var httpContext = CreateHttpContext("/Account/ReturnToObserver",
                passwordChangeRequired: true,
                isObserving: true);

            bool nextCalled = false;
            var middleware = CreateMiddleware(innerContext =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });
            await middleware.Invoke(httpContext);

            nextCalled.Should().BeTrue("observer should be able to navigate to ReturnToObserver");
        }

        private static HttpContext CreateHttpContext(string requestPath,
            bool passwordChangeRequired = false,
            bool isObserving = false)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = requestPath;

            var claims = new System.Collections.Generic.List<Claim>();
            if (passwordChangeRequired)
                claims.Add(new Claim(AuthorizedUser.PasswordChangeRequiredType, "true"));
            if (isObserving)
            {
                claims.Add(new Claim(AuthorizedUser.ObserverClaimType, "some-observer"));
                claims.Add(new Claim(ClaimTypes.Role, "Observer"));
            }

            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));

            var workspacesCache = Mock.Of<IWorkspacesCache>();
            var authorizedUser = new AuthorizedUser(
                Mock.Of<IHttpContextAccessor>(x => x.HttpContext == httpContext),
                workspacesCache);

            httpContext.RequestServices = Mock.Of<IServiceProvider>(
                x => x.GetService(typeof(IAuthorizedUser)) == authorizedUser
            );

            return httpContext;
        }

        private static ResetPasswordMiddleware CreateMiddleware(RequestDelegate next = null)
        {
            return new ResetPasswordMiddleware(next ?? (_ => Task.CompletedTask));
        }
    }
}
