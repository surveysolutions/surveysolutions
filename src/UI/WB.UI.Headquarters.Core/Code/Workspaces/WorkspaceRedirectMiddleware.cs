#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
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
            var contextAccessor = context.RequestServices.GetRequiredService<IWorkspaceContextAccessor>();

            var currentWorkspace = contextAccessor.CurrentWorkspace();

            // Let status-code re-execution render the error page after access is denied.
            if (currentWorkspace != null && currentWorkspace.IsAdministrationWorkspace()
                && context.Features.Get<IStatusCodeReExecuteFeature>() == null)
            {
                var authorizedUser = context.RequestServices.GetRequiredService<IAuthorizedUser>();
                if (!authorizedUser.IsAuthenticated)
                {
                    var returnUrl = $"{context.Request.PathBase}{context.Request.Path}{context.Request.QueryString}";
                    context.Response.Redirect(QueryHelpers.AddQueryString(
                        $"{currentWorkspace.PathBase}/Account/LogOn", "ReturnUrl", returnUrl));
                    return;
                }

                if (!authorizedUser.IsAdministrator)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                if (!context.Request.Path.HasValue || context.Request.Path == "/")
                {
                    context.Response.Redirect($"{context.Request.PathBase}/Workspaces");
                    return;
                }
            }

            if (context.Request.Path.StartsWithSegments("/UnderConstruction"))
            {
                await next(context).ConfigureAwait(false);
                return;
            }

            if (currentWorkspace == null
                && !NotScopedToWorkspacePaths
                    .Any(w => context.Request.Path.StartsWithSegments(w, StringComparison.InvariantCultureIgnoreCase)))
            {
                // Redirect into default workspace for old urls
                var authorizedUser = context.RequestServices.GetRequiredService<IAuthorizedUser>();

                var targetWorkspace = HandleAnonymousPublicUrls(context) 
                                      ?? HandleCookieRedirect(context, authorizedUser) 
                                      // redirecting user to first enabled workspace if any
                                      ?? authorizedUser.GetEnabledWorkspaces().FirstOrDefault()?.Name
                                      // redirect to any workspace, even if it's disabled
                                      ?? authorizedUser.Workspaces.FirstOrDefault();
                
                if (targetWorkspace != null)
                {
                    context.Response.Redirect(
                        $"{context.Request.PathBase}/{targetWorkspace}/{context.Request.Path.Value!.TrimStart('/')}");
                    return;
                }
            }

            await next(context).ConfigureAwait(false);
        }

        private static string? HandleAnonymousPublicUrls(HttpContext context)
        {
            //redirect for public urls available before workspaces were introduced
            var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
            var allowsFallbackToPrimaryWorkspace = endpoint?.Metadata.GetMetadata<AllowPrimaryWorkspaceFallbackAttribute>();
            var allowAnonymous = endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>();

            if (allowAnonymous != null && allowsFallbackToPrimaryWorkspace != null)
            {
                return WorkspaceConstants.DefaultWorkspaceName;
            }

            return null;
        }

        private string? HandleCookieRedirect(HttpContext context, IAuthorizedUser authorizedUser)
        {
            if (context.Request.Cookies.TryGetValue(WorkspaceInfoFilter.CookieName, out var cookieValue)
                && cookieValue != null)
            {
                try
                {
                    var workspaceName = dataProtector.Unprotect(cookieValue);

                    if (context.User.HasClaim(WorkspaceConstants.ClaimType, workspaceName))
                    {
                        var cache = context.RequestServices.GetRequiredService<IWorkspacesCache>();

                        var workspace = cache.GetWorkspace(workspaceName);

                        if (workspace?.IsEnabled() == true)
                        {
                            if (authorizedUser.IsAuthenticated && !authorizedUser.HasAccessToWorkspace(workspaceName))
                            {
                                return null;
                            }

                            return workspaceName;
                        }
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
            "/graphql", 
            "/Account", 
            "/api", 
            "/.hc", 
            "/metrics", 
            "/" + WorkspaceConstants.WorkspaceNames.AdminWorkspaceName,
            "/" + WorkspaceConstants.WorkspaceNames.UsersWorkspaceName,
            "/.vite",
            "/@vite",
            "/js",
            "/css",
            "/fonts",
            "/locale",
            "/img",
            "/error/report",
        };
    }
}
