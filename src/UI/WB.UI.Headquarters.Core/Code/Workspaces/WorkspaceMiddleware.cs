#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Infrastructure.Native.Workspaces;

namespace WB.UI.Headquarters.Code.Workspaces
{
    public class WorkspaceMiddleware
    {
        private readonly RequestDelegate next;

        public WorkspaceMiddleware(RequestDelegate next)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
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

            List<WorkspaceContext> workspaces;

            using (var scope = context.RequestServices.CreateScope())
            {
                var workspacesService = scope.ServiceProvider.GetRequiredService<IWorkspacesCache>();
                workspaces = workspacesService.GetWorkspaces().ToList();
            }

            foreach (var workspace in workspaces)
            {
                if (!context.Request.Path.StartsWithSegments("/" + workspace.Name, out var matchedPath,
                    out var remainingPath))
                    continue;

                var originalPath = context.Request.Path;
                var originalPathBase = context.Request.PathBase;
                context.Request.Path = remainingPath;
                context.Request.PathBase = originalPathBase.Add(matchedPath);

                try
                {
                    workspace.PathBase = originalPathBase;
                    workspace.UsingFallbackToDefaultWorkspace = false;

                    context.SetWorkspace(workspace);

                    await next(context);
                }
                finally
                {
                    context.Request.Path = originalPath;
                    context.Request.PathBase = originalPathBase;
                }

                return;
            }

            if (NotScopedToWorkspacePaths.Any(w =>
                context.Request.Path.StartsWithSegments(w, StringComparison.InvariantCultureIgnoreCase)))
            {
                var workSpace = workspaces.First(w => w.Name == Workspace.Default.Name);
                workSpace.UsingFallbackToDefaultWorkspace = true;
                workSpace.PathBase = context.Request.PathBase;

                if (context.Request.Headers.ContainsKey("referer"))
                {
                    var requestHeader = context.Request.Headers["referer"];
                    var referer = new Uri(requestHeader);

                    if (workspaces.Any(w => referer.AbsolutePath.StartsWith("/" + w.Name)))
                    {
                        throw new ArgumentException("Cannot call primary");
                    }
                }

                context.SetWorkspace(workSpace);
            }

            await next(context);
        }

        public static readonly string[] NotScopedToWorkspacePaths =
        {
            "/graphql", "/Account", "/api"
        };
    }
}
