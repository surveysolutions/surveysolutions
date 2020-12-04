#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WB.Infrastructure.Native.Workspaces;

namespace WB.UI.Headquarters.Code.Workspaces
{
    public class WorkspaceRedirectMiddleware
    {
        private readonly RequestDelegate next;

        public WorkspaceRedirectMiddleware(RequestDelegate next)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
        }
        
        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/UnderConstruction"))
            {
                await next(context).ConfigureAwait(false);
                return;
            }

            var contextAccessor = context.RequestServices.GetRequiredService<IWorkspaceContextAccessor>();

            var currentWorkspace = contextAccessor.CurrentWorkspace();

            if (currentWorkspace == null
                && !NotScopedToWorkspacePaths
                    .Any(w => context.Request.Path.StartsWithSegments(w, StringComparison.InvariantCultureIgnoreCase)))
            {
                // Redirect into default workspace for old urls
                string? targetWorkspace = null;
                if (context.Request.Cookies.ContainsKey(WorkspaceInfoFilter.CookieName))
                {
                    targetWorkspace = context.Request.Cookies[WorkspaceInfoFilter.CookieName];
                }
                else
                if (context.User.HasClaim(x => x.Type == WorkspaceConstants.ClaimType))
                {
                    var userFirstWorkspace = context.User.Claims.First(x => x.Type == WorkspaceConstants.ClaimType);
                    targetWorkspace = userFirstWorkspace.Value;
                }

                if (targetWorkspace != null)
                {
                    context.Response.Redirect(
                        $"{context.Request.PathBase}/{targetWorkspace}/{context.Request.Path.Value.TrimStart('/')}");
                    return;
                }
            }

            await next(context).ConfigureAwait(false);
        }

        public static readonly string[] NotScopedToWorkspacePaths =
        {
            "/graphql", "/Account", "/api", "/.hc", "/metrics"
        };
    }
}
