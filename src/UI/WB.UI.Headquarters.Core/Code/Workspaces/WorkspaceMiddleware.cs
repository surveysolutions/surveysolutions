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

            if (context.Request.Path.StartsWithSegments("/UnderConstruction"))
            {
                await next(context);
                return;
            }

            var workspacesService = context.RequestServices.GetRequiredService<IWorkspacesCache>();
            List<WorkspaceContext> workspaces = workspacesService.AllWorkspaces().ToList();

            async Task InvokeNextWithScope(WorkspaceContext workspaceContext)
            {
                using var scope = context.RequestServices.CreateWorkspaceScope(workspaceContext);
                context.RequestServices = scope.ServiceProvider;
                await next(context);
            }

            foreach (var path in InfrastructureEndpoints)
            {
                if (context.Request.Path.StartsWithSegments(path))
                {
                    await InvokeNextWithScope(workspaces.First(w => w.Name == Workspace.Default.Name));
                    return;
                }
            }

            foreach (var workspace in workspaces)
            {
                var workspaceMatched = context.Request.Path.StartsWithSegments("/" + workspace.Name,
                    out var matchedPath,
                    out var remainingPath);

                if (!workspaceMatched)
                    continue;

                var originalPath = context.Request.Path;
                var originalPathBase = context.Request.PathBase;
                context.Request.Path = remainingPath;
                context.Request.PathBase = originalPathBase.Add(matchedPath);

                try
                {
                    workspace.PathBase = originalPathBase;
                    await InvokeNextWithScope(workspace);
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

        static readonly string[] InfrastructureEndpoints = { "/.hc", "/metrics", "/api", "/.version" };
    }
}
