#nullable enable
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Infrastructure.Native.Workspaces;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public class WorkspaceGraphQlMiddleware
    {
        private readonly FieldDelegate next;

        public WorkspaceGraphQlMiddleware(FieldDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(IMiddlewareContext context, IWorkspaceContextSetter workspaceContextSetter)
        {
            var workspace = context.Argument<string>("workspace") ?? WorkspaceConstants.DefaultWorkspaceName;
            workspaceContextSetter.Set(workspace);
            await next(context);
        }
    }
}
