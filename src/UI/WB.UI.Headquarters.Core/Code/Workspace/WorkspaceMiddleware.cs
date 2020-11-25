using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.BoundedContexts.Headquarters.Workspaces.Mappings;

namespace WB.UI.Headquarters.Code.Workspace
{
    public class WorkspaceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWorkspacesService workspacesService;
        private readonly IWorkspaceNameStorage workspaceNameStorage;

        /// <summary>
        /// Creates a new instance of <see cref="WorkspaceMiddleware"/>.
        /// </summary>
        /// <param name="next">The delegate representing the next middleware in the request pipeline.</param>
        /// <param name="pathBase">The path base to extract.</param>
        public WorkspaceMiddleware(RequestDelegate next, IWorkspacesService workspacesService, IWorkspaceNameStorage workspaceNameStorage)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            this.workspacesService = workspacesService;
            this.workspaceNameStorage = workspaceNameStorage;
        }

        /// <summary>
        /// Executes the middleware.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
        /// <returns>A task that represents the execution of this middleware.</returns>
        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var workspaces = this.workspacesService.GetWorkspaces();

            foreach (var workSpace in workspaces)
            {
                if (!context.Request.Path.StartsWithSegments("/" + workSpace.Name, out var matchedPath, out var remainingPath))
                    continue;

                var originalPath = context.Request.Path;
                var originalPathBase = context.Request.PathBase;
                context.Request.Path = remainingPath;
                context.Request.PathBase = originalPathBase.Add(matchedPath);

                try
                {
                    workspaceNameStorage.Set(workSpace.Name);
                    await _next(context);
                }
                finally
                {
                    context.Request.Path = originalPath;
                    context.Request.PathBase = originalPathBase;
                }

                return;
            }

            if (!NotAWorkspace.Any(w => context.Request.Path.StartsWithSegments(w)))
            {
                workspaceNameStorage.Set(WorkspaceConstants.DefaultWorkspacename);
            }

            // handling of default workspace
            await _next(context);
        }

        private static readonly string[] NotAWorkspace = {
            "/graphql", "/Account"
        };
    }
}
