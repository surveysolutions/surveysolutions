#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WB.Infrastructure.Native.Workspaces;

namespace WB.UI.Headquarters.Code.Workspaces
{
    public class WorkspaceRedirectMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IDataProtector dataProtector;

        public WorkspaceRedirectMiddleware(RequestDelegate next,
            IDataProtectionProvider dataProtectionProvider)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.dataProtector = dataProtectionProvider.CreateProtector("ws_cookie");
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/UnderConstruction"))
            {
                await next(context).ConfigureAwait(false);
                return;
            }

            var contextAccessor = context.RequestServices.GetRequiredService<IWorkspaceContextAccessor>();
            var workspacesCache = context.RequestServices.GetRequiredService<IWorkspacesCache>();

            var currentWorkspace = contextAccessor.CurrentWorkspace();

            if (currentWorkspace == null
                && !NotScopedToWorkspacePaths
                    .Any(w => context.Request.Path.StartsWithSegments(w, StringComparison.InvariantCultureIgnoreCase)))
            {
                // Redirect into default workspace for old urls
                string? targetWorkspace = null;

                if (context.Request.Cookies.ContainsKey(WorkspaceInfoFilter.CookieName))
                {
                    var cookieValue = context.Request.Cookies[WorkspaceInfoFilter.CookieName];

                    try
                    {
                        var workspace = dataProtector.Unprotect(cookieValue);

                        if (context.User.HasClaim(WorkspaceConstants.ClaimType, workspace))
                        {
                            targetWorkspace = workspace;
                        }
                       
                    }
                    catch
                    {
                        /* Unprotect can throw exception if keys have changed.
                         We can and should ignore it and remove wrong cookie */
                        context.Response.Cookies.Delete(WorkspaceInfoFilter.CookieName);
                    }
                }

                if (targetWorkspace == null && context.User.HasClaim(x => x.Type == WorkspaceConstants.ClaimType))
                {
                    var userFirstWorkspace = context.User.Claims.First(x => x.Type == WorkspaceConstants.ClaimType);
                    targetWorkspace = userFirstWorkspace.Value;
                }

                if (targetWorkspace != null && workspacesCache.IsWorkspaceAccessAllowedForCurrentUser(targetWorkspace))
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
