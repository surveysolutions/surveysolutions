#nullable enable
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public class WorkspaceMiddleware
    {
        private readonly FieldDelegate next;

        public WorkspaceMiddleware(FieldDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(IMiddlewareContext context, IWorkspaceNameStorage workspaceNameStorage)
        {
            var workspace = context.Argument<string>("workspace");
            if (workspace != null)
            {
                workspaceNameStorage.Set(workspace);
            }

            await next(context);
        }
    }
}
