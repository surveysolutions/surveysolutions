using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.Infrastructure.Domain;
using WB.Infrastructure.Native.Workspaces;

namespace WB.UI.Headquarters.Code.Authentication
{
    // https://stackoverflow.com/a/48654385/72174
    public class HqUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<HqUser, HqRole>
    {
        private readonly IInScopeExecutor<IWorkspacesCache> inScopeExecutor;

        public HqUserClaimsPrincipalFactory(UserManager<HqUser> userManager,
            RoleManager<HqRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            IInScopeExecutor<IWorkspacesCache> inScopeExecutor) : base(userManager, roleManager, optionsAccessor)
        {
            this.inScopeExecutor = inScopeExecutor;
        }

        public override async Task<ClaimsPrincipal> CreateAsync(HqUser user)
        {
            var principal = await base.CreateAsync(user);

            this.inScopeExecutor.Execute(workspacesService =>
            {
                var userWorkspaces = user.IsInRole(UserRoles.Administrator)
                    ? workspacesService.AllWorkspaces()
                    : user.Workspaces.Select(x => x.Workspace.AsContext());

                if (principal.Identity is ClaimsIdentity principalIdentity)
                {
                    foreach (var workspace in userWorkspaces)
                    {
                        principalIdentity.AddClaims(new[]
                        {
                            new Claim(WorkspaceConstants.ClaimType, workspace.Name)
                        });
                    }

                    principalIdentity.AddClaim(new Claim(WorkspaceConstants.RevisionClaimType,
                        workspacesService.Revision().ToString()));
                }
            });

            return principal;
        }
    }
}
