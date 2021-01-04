#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.BoundedContexts.Headquarters.Services;
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

            var currentWorkspace = contextAccessor.CurrentWorkspace();
            
            if (currentWorkspace == null
                && !NotScopedToWorkspacePaths
                    .Any(w => context.Request.Path.StartsWithSegments(w, StringComparison.InvariantCultureIgnoreCase)))
            {
                // Redirect into default workspace for old urls
                string? targetWorkspace = null;
                var authorizedUser = context.RequestServices.GetRequiredService<IAuthorizedUser>();

                targetWorkspace = HandleCookieRedirect(context);
                
                if (targetWorkspace == null)
                {
                    targetWorkspace = authorizedUser.GetEnabledWorkspaces().FirstOrDefault()?.Name;
                }

                if (targetWorkspace != null && authorizedUser.HasAccessToWorkspace(targetWorkspace))
                {
                    context.Response.Redirect(
                        $"{context.Request.PathBase}/{targetWorkspace}/{context.Request.Path.Value!.TrimStart('/')}");
                    return;
                }
            }

            await next(context).ConfigureAwait(false);
        }

        private string? HandleCookieRedirect(HttpContext context)
        {
            if (context.Request.Cookies.TryGetValue(WorkspaceInfoFilter.CookieName, out var cookieValue)
                && cookieValue != null)
            {
                try
                {
                    var workspace = dataProtector.Unprotect(cookieValue);

                    if (context.User.HasClaim(WorkspaceConstants.ClaimType, workspace))
                    {
                        return workspace;
                    }
                }
                catch
                {
                    /* Unprotect can throw exception if keys have changed.
                         We can and should ignore it and remove wrong cookie */
                    context.Response.Cookies.Delete(WorkspaceInfoFilter.CookieName);
                }
            }

            return null;
        }

        public static readonly string[] NotScopedToWorkspacePaths =
        {
            "/graphql", "/Account", "/api", "/.hc", "/metrics", "/" + WorkspaceConstants.AdminWorkspaceName
        };
    }
}
