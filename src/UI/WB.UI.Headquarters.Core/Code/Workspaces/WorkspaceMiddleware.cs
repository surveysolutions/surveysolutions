#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.BoundedContexts.Headquarters.Workspaces.Mappings;
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

            if (NotScopedToWorkspacePaths.Any(w => context.Request.Path.StartsWithSegments(w, StringComparison.InvariantCultureIgnoreCase)))
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
            else
            {
                // Redirect into default workspace for old urls
                string? targetWorkspace = null;
                if (context.Request.Cookies.ContainsKey(WorkspaceInfoFilter.CookieName))
                {
                    targetWorkspace = context.Request.Cookies[WorkspaceInfoFilter.CookieName];
                }
                else if(context.User.HasClaim(x => x.Type == WorkspaceConstants.ClaimType))
                {
                    var userFirstWorkspace = context.User.Claims.First(x => x.Type == WorkspaceConstants.ClaimType);
                    targetWorkspace = userFirstWorkspace.Value;
                }

                if (targetWorkspace != null)
                {
                    context.Response.Redirect( 
                        $"{context.Request.PathBase}/{targetWorkspace}/{context.Request.Path.Value.TrimStart('/')}");
                }
            }
            
            await next(context);
        }


        private static readonly string[] NotScopedToWorkspacePaths = {
            "/graphql", "/Account", "/api"
        };
    }
}
