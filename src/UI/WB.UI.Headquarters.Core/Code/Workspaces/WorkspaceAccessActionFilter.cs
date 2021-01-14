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
            if (context.HttpContext.User.Identity?.AuthenticationType == AuthType.TenantToken)
                return;

            var hasAuthorizedAttribute = context.ActionDescriptor.EndpointMetadata.Any(m => m is AuthorizeAttribute);
            var hasAnonymousAccess = context.ActionDescriptor.EndpointMetadata.Any(m => m is AllowAnonymousAttribute);

            var targetWorkspace = workspaceContextAccessor.CurrentWorkspace();

            if (context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var isUserAdmin = context.HttpContext.User.IsInRole(UserRoles.Administrator.ToString());

                if (hasAuthorizedAttribute)
                {
                    var allowsFallbackToPrimaryWorkspace = context.ActionDescriptor.EndpointMetadata
                        .OfType<AllowPrimaryWorkspaceFallbackAttribute>().Any();
                    
                    if (targetWorkspace != null)
                    {
                        if (targetWorkspace.IsEnabled() == false)
                        {
                            SetForbidResult(ForbidReason.WorkspaceDisabledReason);
                            return;
                        }

                        if (targetWorkspace.IsServerAdministration() && isUserAdmin)
                        {
                            // allow admin to access to server administration workspace
                            return;
                        }

                        if (!authorizedUser.HasAccessToWorkspace(targetWorkspace.Name))
                        {
                            if (targetWorkspace.Name == WorkspaceConstants.DefaultWorkspaceName &&
                                allowsFallbackToPrimaryWorkspace)
                            {
                                return;
                            }

                            SetForbidResult(ForbidReason.WorkspaceAccessDisabledReason);
                            return;
                        }
                    }
                }

                // if there is anonymous access attribute, then show page even for disabled workspaces
                // handles Error pages
                if (!hasAnonymousAccess && targetWorkspace?.IsEnabled() != true)
                {
                    SetForbidResult(ForbidReason.WorkspaceDisabledReason);
                    return;
                }
            }

            // handling Web Interview over disabled workspace case
            else if (!hasAnonymousAccess && targetWorkspace?.IsEnabled() != true)
            {
                SetForbidResult(ForbidReason.WorkspaceAccessDisabledReason);
            }

            void SetForbidResult(ForbidReason? reason)
            {
                context.Result = new ForbidResult(new AuthenticationProperties().WithReason(reason));
            }
        }
    }
}
