using System;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Net.Http.Headers;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Code.Workspaces;
using WB.UI.Headquarters.Services.Impl;

namespace WB.Tests.Web.Headquarters.Workspaces
{
    [TestOf(typeof(WorkspaceRedirectMiddleware))]
    public class WorkspaceRedirectMiddlewareTests
    {
        [Test]
        public async Task when_executing_request_that_is_not_in_workspace_Should_redirect_into_workspace()
        {
            string requestPath = "/Reports/SurveysAndStatuses";

            var middleware = CreateMiddleware();
            var userWorkspace = "primary";
            var httpContext = CreateHttpContext(requestPath,
                userClaimWorkspaces: new[] {userWorkspace},
                serverWorkspaces: new[] {userWorkspace});

            await middleware.Invoke(httpContext);

            httpContext.Response.Headers.Should()
                .Contain(HeaderNames.Location, $"/{userWorkspace}/Reports/SurveysAndStatuses");
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status302Found);
        }

        [Test]
        public async Task when_executing_request_with_cookie_to_workspace_that_user_has_access_should_redirect()
        {
            var cookieWorkspace = "2077";

            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(cookieWorkspace));

            string requestPath = $"/Reports/SurveysAndStatuses";

            var middleware = CreateMiddleware();
            var httpContext = CreateHttpContext(requestPath,
                userClaimWorkspaces: new[] {cookieWorkspace},
                cookies: (WorkspaceInfoFilter.CookieName, base64));

            await middleware.Invoke(httpContext);

            httpContext.Response.Headers.Should().ContainKey(HeaderNames.Location);
            httpContext.Response.Headers["Location"].ToString().StartsWith("/" + cookieWorkspace);
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status302Found);
        }

        [Test]
        public async Task
            when_executing_request_with_cookie_to_workspace_that_user_has_no_access_should_redirect_to_other()
        {
            var cookieWorkspace = "2077";

            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(cookieWorkspace));

            string requestPath = $"/Reports/SurveysAndStatuses";

            var middleware = CreateMiddleware();
            var httpContext = CreateHttpContext(requestPath,
                userClaimWorkspaces: new[] {"cyberpunk"},
                cookies: (WorkspaceInfoFilter.CookieName, base64));

            await middleware.Invoke(httpContext);

            httpContext.Response.Headers.Should().ContainKey(HeaderNames.Location);
            httpContext.Response.Headers["Location"].ToString().StartsWith("/cyberpunk");
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status302Found);
        }

        [Test]
        public async Task when_user_has_cookie_into_disabled_workspace_Should_redirect_to_enabled()
        {
            var cookieWorkspace = "2077";

            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(cookieWorkspace));

            string requestPath = $"/Reports/SurveysAndStatuses";

            var middleware = CreateMiddleware();

            var httpContext = CreateHttpContext(requestPath,
                serverWorkspaces: new[] { "primary" },
                userClaimWorkspaces: new[] { "primary", "2077" },
                disabledWorkspaces: new[] { "2077" },
                cookies: (WorkspaceInfoFilter.CookieName, base64));

            await middleware.Invoke(httpContext);

            httpContext.Response.Headers.Should().ContainKey(HeaderNames.Location);
            var location = httpContext.Response.Headers[HeaderNames.Location];
            StringAssert.StartsWith("/primary", location);
        }

        [Test]
        public async Task when_executing_request_into_workspace_Should_not_redirect_anywhere()
        {
            var userWorkspace = "primary";

            string requestPath = $"/{userWorkspace}/Reports/SurveysAndStatuses";

            var middleware = CreateMiddleware();
            var httpContext = CreateHttpContext(
                requestPath,
                userClaimWorkspaces: new[] {userWorkspace},
                currentWorkspace: userWorkspace);

            await middleware.Invoke(httpContext);

            httpContext.Response.Headers.Should().NotContainKey(HeaderNames.Location);
            httpContext.Response.StatusCode.Should().NotBe(StatusCodes.Status302Found);
        }

        private HttpContext CreateHttpContext(string requestPath = "/",
            string[] userClaimWorkspaces = null,
            string[] serverWorkspaces = null,
            string[] disabledWorkspaces = null,
            string currentWorkspace = null,
            params (string name, string value)[] cookies)
        {
            serverWorkspaces ??= userClaimWorkspaces;
            var workspacesCache = Create.Service.WorkspacesCache(serverWorkspaces, disabledWorkspaces);

            var result = new DefaultHttpContext();
            result.Request.Path = requestPath;

            if (cookies != null)
            {
                result.Request.Headers["Cookie"] = cookies.Select(c => $"{c.name}={c.value}").ToArray();
            }

            var workspaces = userClaimWorkspaces ?? new[] { WorkspaceConstants.DefaultWorkspaceName };

            var claims = workspaces.Select(x => new Claim(WorkspaceConstants.ClaimType, x)).ToList();
            result.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

            var authorizedUser = new AuthorizedUser(Mock.Of<IHttpContextAccessor>(x => x.HttpContext == result), workspacesCache);

            var workspacesAccessor = Mock.Of<IWorkspaceContextAccessor>(x => x.CurrentWorkspace() ==
                                                                             (currentWorkspace == null
                                                                                 ? null
                                                                                 : new WorkspaceContext(
                                                                                     currentWorkspace, String.Empty, null)));

            result.RequestServices = Mock.Of<IServiceProvider>(
                x => x.GetService(typeof(IWorkspaceContextAccessor)) == workspacesAccessor
                && x.GetService(typeof(IAuthorizedUser)) == authorizedUser
                && x.GetService(typeof(IWorkspacesCache)) == workspacesCache
            );

            return result;
        }

        private WorkspaceRedirectMiddleware CreateMiddleware(
            RequestDelegate next = null)
        {
            RequestDelegate testNext = next;
            if (testNext == null)
            {
                testNext = innerHttpContext => Task.CompletedTask;
            }

            var middleware = new WorkspaceRedirectMiddleware(
                testNext,
                new MockDataProtectorProvider()
            );

            return middleware;
        }
    }

    public class MockDataProtectorProvider : IDataProtectionProvider
    {
        public IDataProtector CreateProtector(string purpose)
        {
            return new MockDataProtector();
        }

        class MockDataProtector : IDataProtector
        {
            public IDataProtector CreateProtector(string purpose)
            {
                return this;
            }

            public byte[] Protect(byte[] plaintext)
            {
                return plaintext;
            }

            public byte[] Unprotect(byte[] protectedData)
            {
                return protectedData;
            }
        }
    }
}
