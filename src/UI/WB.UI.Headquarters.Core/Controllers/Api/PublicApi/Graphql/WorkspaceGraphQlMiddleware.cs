#nullable enable
using System.Linq;
using System.Threading.Tasks;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public class WorkspaceGraphQlMiddleware
    {
        private readonly FieldDelegate next;

        public WorkspaceGraphQlMiddleware(FieldDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(IMiddlewareContext context,
            IWorkspaceContextAccessor workspaceContextAccessor,
            IWorkspaceContextSetter workspaceContextSetter)
        {
            if (workspaceContextAccessor.CurrentWorkspace() == null)
            {
                var workspace = context.GetWorkspaceNameOrDefault();

                if (workspace != null)
                {
                    workspaceContextSetter.Set(workspace);
                }
            }

            await next(context);
        }
    }
}
