using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using WB.Infrastructure.Native.Storage;

namespace WB.UI.Headquarters.Code.Authentication
{
    public class WorkspaceRequirementHandler : AuthorizationHandler<WorkspaceRequirement>
    {
        private readonly IWorkspaceNameProvider workspaceNameProvider;

        public WorkspaceRequirementHandler(IWorkspaceNameProvider workspaceNameProvider)
        {
            this.workspaceNameProvider = workspaceNameProvider;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, WorkspaceRequirement requirement)
        {
            var hasClaim = context.User.HasClaim("Workspace", workspaceNameProvider.CurrentWorkspace());
            if (hasClaim || context.User.IsInRole(UserRoles.Administrator.ToString()))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
