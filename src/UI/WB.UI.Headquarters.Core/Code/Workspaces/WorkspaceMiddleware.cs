using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.Infrastructure.Domain;

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

            var workspaces = GetWorkspaces(context);

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
                    context.SetWorkspace(workSpace);
                    context.SetWorkspaceMatchPath(true);

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
                context.SetWorkspace(WorkspaceContext.From(Workspace.Default));
                context.SetWorkspaceMatchPath(false);
            }

            // handling of default workspace
            await next(context);
        }


        IEnumerable<WorkspaceContext> GetWorkspaces(HttpContext context)
        {
            var cache = context.RequestServices.GetRequiredService<IMemoryCache>();

            return cache.GetOrCreate("workspaces", entry =>
            {
                var workspaces = context.RequestServices.GetService<IInScopeExecutor>().Execute(scope => 
                    scope.GetInstance<IWorkspacesService>().GetWorkspaces().ToList());

                entry.AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(1);
                return workspaces;
            });
        }

        private static readonly string[] NotAWorkspace = {
            "/graphql", "/Account"
        };
    }
}
