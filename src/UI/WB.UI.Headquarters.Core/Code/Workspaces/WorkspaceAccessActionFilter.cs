using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Code.Authentication;

namespace WB.UI.Headquarters.Code.Workspaces
{
    public class WorkspaceAccessActionFilter : IAuthorizationFilter
    {
        private readonly IWorkspaceContextAccessor workspaceContextAccessor;
        private readonly IWorkspacesCache workspacesCache;

        public WorkspaceAccessActionFilter(IWorkspaceContextAccessor workspaceContextAccessor,
            IWorkspacesCache workspacesCache)
        {
            this.workspaceContextAccessor = workspaceContextAccessor;
            this.workspacesCache = workspacesCache;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if(context.HttpContext.User.Identity.AuthenticationType == AuthType.TenantToken)
                return;

            var hasAuthorizedAttribute = context.ActionDescriptor.EndpointMetadata.Any(m => m is AuthorizeAttribute);

            if (hasAuthorizedAttribute && context.HttpContext.User.Identity.IsAuthenticated)
            {
                var targetWorkspace = workspaceContextAccessor.CurrentWorkspace();
                var userWorkspaces = this.workspacesCache.CurrentUserWorkspaces();
                
                var allowsFallbackToPrimaryWorkspace = context.ActionDescriptor.EndpointMetadata.OfType<AllowPrimaryWorkspaceFallbackAttribute>().Any();
                if (targetWorkspace != null && !userWorkspaces.Any(x => x.Name.Equals(targetWorkspace.Name, StringComparison.OrdinalIgnoreCase)))
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
