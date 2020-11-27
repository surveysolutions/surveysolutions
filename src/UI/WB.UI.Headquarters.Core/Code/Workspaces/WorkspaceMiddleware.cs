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
    /// <summary>
    /// Workspace Middleware extract workspace information from route and create DI scope for execution
    /// Pass flow forward if no workspace detected
    /// </summary>
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
                var workspaceMatched = context.Request.Path.StartsWithSegments("/" + workspace.Name,
                    out var matchedPath,
                    out var remainingPath);

                if (workspace.Name == Workspace.Default.Name)
                {
                    workspaceMatched = context.Request.Path.StartsWithSegments("/api",
                        out matchedPath,
                        out remainingPath);
                }

                if (!workspaceMatched)
                    continue;

                var originalPath = context.Request.Path;
                var originalPathBase = context.Request.PathBase;
                context.Request.Path = remainingPath;
                context.Request.PathBase = originalPathBase.Add(matchedPath);

                try
                {
                    workspace.PathBase = originalPathBase;
                    using var scope = context.RequestServices.CreateScope();
                    context.RequestServices = scope.ServiceProvider;
                    scope.ServiceProvider.GetRequiredService<IWorkspaceContextSetter>().Set(workspace);
                    await next(context);
                }
                finally
                {
                    context.Request.Path = originalPath;
                    context.Request.PathBase = originalPathBase;
                }

                return;
            }

            await next(context);
        }
    }
}
