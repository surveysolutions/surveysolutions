using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.BoundedContexts.Headquarters.Workspaces.Mappings;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Code.Workspaces;

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
            var httpContext = CreateHttpContext(requestPath, userWorkspaces: new [] {userWorkspace});
            
            await middleware.Invoke(httpContext);

            httpContext.Response.Headers.Should().Contain(HeaderNames.Location, $"/{userWorkspace}/Reports/SurveysAndStatuses");
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status302Found);
        }

        [Test]
        public async Task when_executing_request_into_workspace_Should_not_redirect_anywhere()
        {
            var userWorkspace = "primary";

            string requestPath = $"/{userWorkspace}/Reports/SurveysAndStatuses";

            var middleware = CreateMiddleware();
            var httpContext = CreateHttpContext(requestPath, 
                userWorkspaces: new [] {userWorkspace},
                currentWorkspace: userWorkspace);
            
            await middleware.Invoke(httpContext);

            httpContext.Response.Headers.Should().NotContainKey(HeaderNames.Location);
            httpContext.Response.StatusCode.Should().NotBe(StatusCodes.Status302Found);
        }

        private HttpContext CreateHttpContext(string requestPath = "/",
            string[] userWorkspaces = null,
            string currentWorkspace = null)
        {
            var result = new DefaultHttpContext();
            result.Request.Path = requestPath;
            var workspaces = userWorkspaces ?? new[] {WorkspaceConstants.DefaultWorkspaceName};
            var claims = workspaces.Select(x => new Claim(WorkspaceConstants.ClaimType, x)).ToList();
            result.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

            var workspacesAccessor = Mock.Of<IWorkspaceContextAccessor>(x => x.CurrentWorkspace() == 
                                                                             (currentWorkspace == null ? null : new WorkspaceContext(currentWorkspace, String.Empty)));

            result.RequestServices = Mock.Of<IServiceProvider>(
                x => x.GetService(typeof(IWorkspaceContextAccessor)) == workspacesAccessor
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
                testNext
                );
            
            return middleware;
        }
    }
}
