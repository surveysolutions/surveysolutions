using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
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

            if (hasAuthorizedAttribute && context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var targetWorkspace = workspaceContextAccessor.CurrentWorkspace();

                if (targetWorkspace.IsServerAdministration()
                    && context.HttpContext.User.IsInRole(UserRoles.Administrator.ToString()))
                {
                    return;
                }

                var allowsFallbackToPrimaryWorkspace = context.ActionDescriptor.EndpointMetadata
                    .OfType<AllowPrimaryWorkspaceFallbackAttribute>().Any();

                if (targetWorkspace != null && !authorizedUser.HasAccessToWorkspace(targetWorkspace.Name))
                {
                    if (targetWorkspace.Name == WorkspaceConstants.DefaultWorkspaceName && allowsFallbackToPrimaryWorkspace)
                    {
                        return;
                    }

                    context.Result = new ForbidResult();
                }
            }
        }
    }
}
