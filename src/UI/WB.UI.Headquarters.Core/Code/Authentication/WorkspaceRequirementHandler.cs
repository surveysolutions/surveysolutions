using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Workspaces;

namespace WB.UI.Headquarters.Code.Authentication
{
    public class WorkspaceRequirementHandler : AuthorizationHandler<WorkspaceRequirement>
    {
        private readonly IWorkspaceContextAccessor workspaceContextAccessor;

        public WorkspaceRequirementHandler(IWorkspaceContextAccessor workspaceContextAccessor)
        {
            this.workspaceContextAccessor = workspaceContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, WorkspaceRequirement requirement)
        {
            var hasClaim = context.User.HasClaim("Workspace", workspaceContextAccessor.CurrentWorkspace().Name);
            if (hasClaim || context.User.IsInRole(UserRoles.Administrator.ToString()))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
