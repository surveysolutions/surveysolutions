using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.UI.Headquarters.Code.Workspaces;

namespace WB.Tests.Web.Headquarters.Workspaces
{
    [TestOf(typeof(WorkspaceMiddleware))]
    public class WorkspaceMiddlewareTests
    {
        [Test]
        public void when_executing_request_that_is_not_in_workspace_Should_redirect_into_workspace()
        {
            string requestPath = "/Reports/SurveysAndStatuses";
            
        }

        private WorkspaceMiddleware CreateMiddleware(
            RequestDelegate next = null,
            IWorkspacesService workspacesService = null)
        {
            RequestDelegate testNext = next;
            if (testNext == null)
            {
                testNext = innerHttpContext => Task.CompletedTask;
            }
            var middleware = new WorkspaceMiddleware(
                testNext,
                workspacesService ?? Mock.Of<IWorkspacesService>()
                );
            
            return middleware;
        }
    }
}
