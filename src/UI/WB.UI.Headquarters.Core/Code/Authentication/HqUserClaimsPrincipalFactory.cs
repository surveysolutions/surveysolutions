using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.Infrastructure.Domain;

namespace WB.UI.Headquarters.Code.Authentication
{
    // https://stackoverflow.com/a/48654385/72174
    public class HqUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<HqUser, HqRole>
    {
        private readonly IInScopeExecutor inScopeExecutor;

        public HqUserClaimsPrincipalFactory(UserManager<HqUser> userManager,
            RoleManager<HqRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            IInScopeExecutor inScopeExecutor) : base(userManager, roleManager, optionsAccessor)
        {
            this.inScopeExecutor = inScopeExecutor;
        }

        public override async Task<ClaimsPrincipal> CreateAsync(HqUser user)
        {
            var principal = await base.CreateAsync(user);

            this.inScopeExecutor.Execute(sl =>
            {
                var workspacesService = sl.GetInstance<IWorkspacesService>();
                var userWorkspaces = workspacesService.GetWorkspacesForUser(user.Id);
                foreach (var workspace in userWorkspaces)
                {
                    ((ClaimsIdentity) principal.Identity).AddClaims(new[]
                    {
                        new Claim("Workspace", workspace)
                    });
                }
            });
            
            return principal;
        }
    }
}
