#nullable enable
using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Code.Authentication;
using WB.UI.Shared.Web.Attributes;

namespace WB.UI.Headquarters.Code.Workspaces
{
    public class WorkspaceAccessActionFilter : IAuthorizationFilter
    {
        private readonly IWorkspaceContextAccessor workspaceContextAccessor;
        private readonly IAuthorizedUser authorizedUser;

        public WorkspaceAccessActionFilter(
            IWorkspaceContextAccessor workspaceContextAccessor,
            IAuthorizedUser authorizedUser)
        {
            this.workspaceContextAccessor = workspaceContextAccessor;
            this.authorizedUser = authorizedUser;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var allowedDisabledWorkspace = ContextHasAttribute<AllowDisabledWorkspaceAccessAttribute>();
            var allowsFallbackToPrimaryWorkspace = ContextHasAttribute<AllowPrimaryWorkspaceFallbackAttribute>();
            var allowAnonymous = ContextHasAttribute<AllowAnonymousAttribute>();
            var hasAuthorization = ContextHasAttribute<AuthorizeAttribute>() || ContextHasAttribute<AuthorizeByRoleAttribute>();
            
            var workspace = workspaceContextAccessor.CurrentWorkspace();

            if (context.HttpContext.User.Identity?.AuthenticationType == AuthType.TenantToken)
            {
                if (workspace?.IsEnabled() != true)
                {
                    SetForbidResult(ForbidReason.WorkspaceDisabledReason);
                    return;
                }

                return;
            }

            // handling primary workspace fallback attribute, only is workspace is null
            if (workspace == null && allowsFallbackToPrimaryWorkspace)
            {
                workspace = WorkspaceContext.Default;
            }

            if (workspace?.IsEnabled() == true || allowedDisabledWorkspace)
            {

                if (hasAuthorization && !allowAnonymous && workspace.IsSystemDefinedWorkspace())
                {
                    if (workspace.IsAdministrationWorkspace())
                    {
                        // Only allow access to administration-appropriate paths in the admin workspace.
                        // Workspace-specific pages and APIs should not be accessible here.
                        var requestPath = context.HttpContext.Request.Path;
                        bool isAdminSafePath = AdministrationWorkspaceAllowedPaths
                            .Any(p => requestPath.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));
                        if (!isAdminSafePath)
                        {
                            context.Result = new NotFoundResult();
                            return;
                        }
                    }

                    // allow user to access to special workspace
                    return;
                }

                if (hasAuthorization && !allowAnonymous && !(workspace != null && authorizedUser.HasAccessToWorkspace(workspace.Name)))
                {
                    var hasIgnoreWorkspacesLimitation = ContextHasAttribute<IgnoreWorkspacesLimitationAttribute>();
                    if (!hasIgnoreWorkspacesLimitation)
                    {
                        SetForbidResult(ForbidReason.WorkspaceAccessDisabledReason);
                        return;
                    }
                }
            }
            else
            {
                SetForbidResult(ForbidReason.WorkspaceDisabledReason);
                return;
            }
            
            void SetForbidResult(ForbidReason? reason)
            {
                context.Result = new ForbidResult(new AuthenticationProperties().WithReason(reason));
            }

            bool ContextHasAttribute<T>() where T: class => context.ActionDescriptor.EndpointMetadata.Any(m => m is T);
        }

        // Paths (relative to the /administration prefix) that are valid within the administration workspace.
        // Any request path not starting with one of these segments will receive a 404 response,
        // preventing workspace-specific HQ pages from being served under /administration.
        internal static readonly string[] AdministrationWorkspaceAllowedPaths =
        {
            "/Account",
            "/Administration",
            "/Workspaces",
            "/Users",
            "/Profile",
            "/error",
            "/api/v1/workspaces",
            "/api/v1/users",
        };
    }
}
