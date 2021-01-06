using System.Threading.Tasks;
using HotChocolate.Resolvers;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
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
            if (context.User.IsInRole(UserRoles.Administrator.ToString()))
            {
                context.Succeed(requirement);
            }

            if (context.User.Identity.AuthenticationType == AuthType.TenantToken)
            {
                context.Succeed(requirement);
            }

            var workspace = workspaceContextAccessor.CurrentWorkspace();
            if (workspace == null)
            {
                if (context.Resource is IDirectiveContext ctx)
                {
                    var workspaceArgument = ctx.GetWorkspaceNameOrDefault();

                    if (context.User.HasClaim("Workspace", workspaceArgument))
                    {
                        context.Succeed(requirement);
                    }

                    return Task.CompletedTask;
                }
            }
            else
            if (context.User.HasClaim("Workspace", workspace.Name) && workspace.IsEnabled())
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
