using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3;
using WB.Core.BoundedContexts.Headquarters.Workspaces;

namespace WB.UI.Headquarters.Code.Workspaces
{
    public class WorkspaceScopeMiddleware
    {
        private readonly RequestDelegate next;

        public WorkspaceScopeMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var requestServices = context.RequestServices;

            var workspace = context.GetCurrentWorkspace();

            if (workspace == null)
            {
                await next(context);
            }
            else
            {
                try
                {
                    using var scope = requestServices.CreateScope();
                    context.RequestServices = scope.ServiceProvider;
                    scope.ServiceProvider.GetRequiredService<IWorkspaceContextSetter>().Set(workspace);
                    await next(context);
                }
                finally
                {

                    context.RequestServices = requestServices;
                }
            }
        }
    }
}
