using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Code.Authentication;

namespace WB.UI.Headquarters.Code.Workspaces
{
    public class WorkspaceAccessActionFilter : IAuthorizationFilter
    {
        private readonly IWorkspaceContextAccessor workspaceContextAccessor;

        public WorkspaceAccessActionFilter(IWorkspaceContextAccessor workspaceContextAccessor)
        {
            this.workspaceContextAccessor = workspaceContextAccessor;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if(context.HttpContext.User.Identity.AuthenticationType == AuthType.TenantToken)
                return;

            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                var workspace = workspaceContextAccessor.CurrentWorkspace();

                var allowsFallbackToPrimaryWorkspace = context.ActionDescriptor.EndpointMetadata.OfType<AllowPrimaryWorkspaceFallbackAttribute>().Any();

                if (workspace != null && !context.HttpContext.User.HasClaim(WorkspaceConstants.ClaimType, workspace.Name))
                {
                    if (workspace.Name == WorkspaceConstants.DefaultWorkspaceName && allowsFallbackToPrimaryWorkspace)
                    {
                        return;
                    }
                    
                    context.Result = new ForbidResult();
                }
            }
        }
    }
}
