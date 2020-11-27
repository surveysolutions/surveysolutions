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

        /// <summary>
        /// Creates a new instance of <see cref="WorkspaceMiddleware"/>.
        /// </summary>
        /// <param name="next">The delegate representing the next middleware in the request pipeline.</param>
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
                if (!context.Request.Path.StartsWithSegments("/" + workspace.Name, out var matchedPath, out var remainingPath))
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

            if (!NotAWorkspace.Any(w => context.Request.Path.StartsWithSegments(w)))
            {
                var workSpace = workspaces.First(w => w.Name == Workspace.Default.Name);
                workSpace.UsingFallbackToDefaultWorkspace = true;
                workSpace.PathBase = context.Request.PathBase;

                var referer = new Uri(context.Request.Headers["referer"]);

                if (workspaces.Any(w => referer.AbsolutePath.StartsWith("/" + w.Name)))
                {
                    throw new ArgumentException("Cannot call primary");
                }

                context.SetWorkspace(workSpace);
            }

            // handling of default workspace
            await next(context);
        }


        private static readonly string[] NotAWorkspace = {
            "/graphql", "/Account"
        };
    }
}
