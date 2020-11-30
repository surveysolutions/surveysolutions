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
            var workspace = workspaceContextAccessor.CurrentWorkspace();
            
            if (context.User.IsInRole(UserRoles.Administrator.ToString()))
            {
                context.Succeed(requirement);
            }

            if (workspace == null)
            {
                if (!(context.Resource is IDirectiveContext ctx)) return Task.CompletedTask;

                var workspaceArgument = ctx.Argument<string>("workspace");

                if (context.User.HasClaim("Workspace", workspaceArgument))
                {
                    context.Succeed(requirement);
                }

                return Task.CompletedTask;
            } 
            
            if (context.User.HasClaim("Workspace", workspace.Name))
            {
                context.Succeed(requirement);
            }

            if (context.User.Identity.AuthenticationType == "TenantToken")
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
