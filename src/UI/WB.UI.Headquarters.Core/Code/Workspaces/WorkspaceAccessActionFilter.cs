#nullable enable
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

            var isUserAdmin = context.HttpContext.User?.IsInRole(UserRoles.Administrator.ToString()) ?? false;


            if (workspace?.IsEnabled() == true || allowedDisabledWorkspace)
            {

                if (isUserAdmin && workspace.IsServerAdministration())
                {
                    // allow admin to access to server administration workspace
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
    }
}
