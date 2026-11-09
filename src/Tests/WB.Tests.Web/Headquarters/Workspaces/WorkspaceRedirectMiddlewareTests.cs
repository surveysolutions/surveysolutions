using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Code.Authentication;
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

        [Test]
        public async Task should_not_redirect_UnderConstruction_requests()
        {
            var userWorkspace = "primary";

            string requestPath = $"/UnderConstruction/request";

            var middleware = CreateMiddleware();
            var httpContext = CreateHttpContext(
                requestPath,
                userClaimWorkspaces: new[] { userWorkspace },
                currentWorkspace: userWorkspace);

            await middleware.Invoke(httpContext);

            httpContext.Response.Headers.Should().NotContainKey(HeaderNames.Location);
            httpContext.Response.StatusCode.Should().NotBe(StatusCodes.Status302Found);
        }

        [Test]
        public async Task when_executing_anonymous_request_to_public_url_should_redirect_default()
        {
            string requestPath = $"/WebInterview/Y6N4D3XY/Start";

            var middleware = CreateMiddleware();
            
            var endpoint = new Endpoint(context => null, 
                new EndpointMetadataCollection(
                    new AllowPrimaryWorkspaceFallbackAttribute(),
                    new AllowAnonymousAttribute()), null);

            var httpContext = new DefaultHttpContext();

            httpContext.Features[typeof(IEndpointFeature)] = new EndpointFeature()
            {
                Endpoint = endpoint
            };

            httpContext.Request.Path = requestPath;

            var workspacesCache = Create.Service.WorkspacesCache(null);

            var authorizedUser = new AuthorizedUser(
                Mock.Of<IHttpContextAccessor>(x => x.HttpContext == httpContext),
                workspacesCache);

            var workspacesAccessor = Mock.Of<IWorkspaceContextAccessor>(x => x.CurrentWorkspace() == null);

            httpContext.RequestServices = Mock.Of<IServiceProvider>(
                x => x.GetService(typeof(IWorkspaceContextAccessor)) == workspacesAccessor
                     && x.GetService(typeof(IAuthorizedUser)) == authorizedUser
                     && x.GetService(typeof(IWorkspacesCache)) == workspacesCache
            );
            
            await middleware.Invoke(httpContext);

            httpContext.Response.Headers.Should().ContainKey(HeaderNames.Location);
            var location = httpContext.Response.Headers[HeaderNames.Location];
            StringAssert.StartsWith("/primary", location);
        }

        [TestCase("", "")]
        [TestCase("/", "/hq")]
        [TestCase("/NewPage", "")]
        [TestCase("/api/v1/workspaces", "")]
        [TestCase("/Account/LogOn", "")]
        [TestCase("/UnderConstruction/request", "")]
        public async Task when_anonymous_user_requests_administration_should_redirect_to_login(
            string path, string applicationPathBase)
        {
            var context = CreateHttpContext(path,
                currentWorkspace: WorkspaceConstants.WorkspaceNames.AdminWorkspaceName,
                applicationPathBase: applicationPathBase);
            context.Request.QueryString = new QueryString("?test=value");
            var middleware = CreateMiddleware(_ => throw new AssertionException("The endpoint must not execute."));

            await middleware.Invoke(context);

            var returnUrl = $"{context.Request.PathBase}{path}?test=value";
            context.Response.StatusCode.Should().Be(StatusCodes.Status302Found);
            context.Response.Headers[HeaderNames.Location].ToString().Should().Be(
                $"{applicationPathBase}/Account/LogOn?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        [TestCase(UserRoles.Headquarter, "", "Cookie")]
        [TestCase(UserRoles.Supervisor, "/", "Cookie")]
        [TestCase(UserRoles.Interviewer, "/Workspaces", "Cookie")]
        [TestCase(UserRoles.Observer, "/NewPage", "Cookie")]
        [TestCase(UserRoles.ApiUser, "/api/v1/workspaces", "Cookie")]
        [TestCase(null, "/api/v1/workspaces", AuthType.TenantToken)]
        [TestCase(UserRoles.Headquarter, "/UnderConstruction/request", "Cookie")]
        public async Task when_non_admin_requests_administration_should_return_404(
            UserRoles? role, string path, string authenticationType)
        {
            var context = CreateHttpContext(path,
                currentWorkspace: WorkspaceConstants.WorkspaceNames.AdminWorkspaceName,
                authenticationType: authenticationType, role: role);
            context.SetEndpoint(new Endpoint(_ => Task.CompletedTask,
                new EndpointMetadataCollection(new AllowAnonymousAttribute()), null));
            var middleware = CreateMiddleware(_ => throw new AssertionException("The endpoint must not execute."));

            await middleware.Invoke(context);

            context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            context.Response.Headers.Should().NotContainKey(HeaderNames.Location);
        }

        [TestCase("", "")]
        [TestCase("/", "")]
        [TestCase("", "/hq")]
        [TestCase("/", "/hq")]
        public async Task when_admin_requests_administration_root_should_redirect_to_workspaces(
            string path, string applicationPathBase)
        {
            var context = CreateHttpContext(path,
                currentWorkspace: WorkspaceConstants.WorkspaceNames.AdminWorkspaceName,
                authenticationType: "Cookie", role: UserRoles.Administrator,
                applicationPathBase: applicationPathBase);
            var middleware = CreateMiddleware(_ => throw new AssertionException("The home page must not execute."));

            await middleware.Invoke(context);

            context.Response.StatusCode.Should().Be(StatusCodes.Status302Found);
            context.Response.Headers[HeaderNames.Location].ToString().Should().Be(
                $"{applicationPathBase}/administration/Workspaces");
        }

        [TestCase("/Workspaces")]
        [TestCase("/Administration/Diagnostics")]
        [TestCase("/Diagnostics/AuditLog")]
        [TestCase("/Diagnostics/TabletInfos")]
        [TestCase("/api/AdminSettings/GetSystemLog")]
        [TestCase("/api/v1/workspaces")]
        [TestCase("/NewPage")]
        [TestCase("/api/NewAction")]
        public async Task when_admin_requests_administration_subpage_should_continue_without_allowlist(string path)
        {
            var context = CreateHttpContext(path,
                currentWorkspace: WorkspaceConstants.WorkspaceNames.AdminWorkspaceName,
                authenticationType: "Cookie", role: UserRoles.Administrator);
            var nextCalled = false;
            var middleware = CreateMiddleware(_ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

            await middleware.Invoke(context);

            nextCalled.Should().BeTrue();
            context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
            context.Response.Headers.Should().NotContainKey(HeaderNames.Location);
        }

        [TestCase("primary")]
        [TestCase("users")]
        public async Task when_non_admin_requests_other_workspace_root_should_continue(string workspace)
        {
            var context = CreateHttpContext("/", currentWorkspace: workspace,
                authenticationType: "Cookie", role: UserRoles.Headquarter);
            var nextCalled = false;
            var middleware = CreateMiddleware(_ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

            await middleware.Invoke(context);

            nextCalled.Should().BeTrue();
            context.Response.Headers.Should().NotContainKey(HeaderNames.Location);
        }

        [Test]
        public async Task when_reexecuting_denied_administration_request_should_render_error_page()
        {
            var context = CreateHttpContext("/",
                currentWorkspace: WorkspaceConstants.WorkspaceNames.AdminWorkspaceName,
                authenticationType: "Cookie", role: UserRoles.Headquarter);
            using var body = new MemoryStream();
            context.Response.Body = body;
            using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
            var app = new ApplicationBuilder(services);
            app.UseStatusCodePagesWithReExecute("/error/{0}");
            app.Use(next => CreateMiddleware(next).Invoke);
            app.Run(async httpContext =>
            {
                httpContext.Request.Path.Value.Should().Be("/error/404");
                httpContext.Response.ContentType = "text/html";
                await httpContext.Response.WriteAsync("Not found");
            });

            await app.Build()(context);

            Encoding.UTF8.GetString(body.ToArray()).Should().Be("Not found");
            context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            context.Response.Headers.Should().NotContainKey(HeaderNames.Location);
        }

        class EndpointFeature : IEndpointFeature
        {
            public Endpoint Endpoint { get; set; }
        }

        private HttpContext CreateHttpContext(string requestPath = "/",
            string[] userClaimWorkspaces = null,
            string[] serverWorkspaces = null,
            string[] disabledWorkspaces = null,
            string currentWorkspace = null,
            string authenticationType = null,
            UserRoles? role = null,
            string applicationPathBase = "",
            params (string name, string value)[] cookies)
        {
            serverWorkspaces ??= userClaimWorkspaces;
            var workspacesCache = Create.Service.WorkspacesCache(serverWorkspaces, disabledWorkspaces);

            var result = new DefaultHttpContext();
            result.Request.Path = requestPath;
            result.Request.PathBase = currentWorkspace == null
                ? applicationPathBase
                : $"{applicationPathBase}/{currentWorkspace}";

            if (cookies != null)
            {
                result.Request.Headers["Cookie"] = cookies.Select(c => $"{c.name}={c.value}").ToArray();
            }

            var workspaces = userClaimWorkspaces ?? new[] { WorkspaceConstants.DefaultWorkspaceName };

            var claims = workspaces.Select(x => new Claim(WorkspaceConstants.ClaimType, x)).ToList();
            if (role.HasValue)
                claims.Add(new Claim(ClaimTypes.Role, role.Value.ToString()));
            result.User = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType));

            var authorizedUser = new AuthorizedUser(Mock.Of<IHttpContextAccessor>(x => x.HttpContext == result), workspacesCache);

            var workspacesAccessor = Mock.Of<IWorkspaceContextAccessor>(x => x.CurrentWorkspace() ==
                                                                             (currentWorkspace == null
                                                                                 ? null
                                                                                 : new WorkspaceContext(
                                                                                     currentWorkspace, String.Empty, null)
                                                                                  {
                                                                                     PathBase = applicationPathBase
                                                                                  }));

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
}
