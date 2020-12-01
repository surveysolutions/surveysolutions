using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WB.Infrastructure.Native.Workspaces;

namespace WB.UI.Headquarters.Filters
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
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                var workspace = workspaceContextAccessor.CurrentWorkspace();

                if (workspace != null && !context.HttpContext.User.HasClaim(WorkspaceConstants.ClaimType, workspace.Name))
                {
                    context.Result = new ForbidResult();
                }
            }
        }
    }
}
