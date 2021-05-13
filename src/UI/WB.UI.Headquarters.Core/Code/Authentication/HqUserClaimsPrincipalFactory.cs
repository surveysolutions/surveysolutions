#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.Domain;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Services.Impl;

namespace WB.UI.Headquarters.Code.Authentication
{
    // https://stackoverflow.com/a/48654385/72174
    public class HqUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<HqUser, HqRole>
    {
        private readonly IInScopeExecutor<IWorkspacesCache> inScopeExecutor;
        private readonly IAuthorizedUser authorizedUser;

        public HqUserClaimsPrincipalFactory(UserManager<HqUser> userManager,
            RoleManager<HqRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            IInScopeExecutor<IWorkspacesCache> inScopeExecutor, 
            IAuthorizedUser authorizedUser) : base(userManager, roleManager, optionsAccessor)
        {
            this.inScopeExecutor = inScopeExecutor;
            this.authorizedUser = authorizedUser;
        }

        public override async Task<ClaimsPrincipal> CreateAsync(HqUser user)
        {
            var principal = await base.CreateAsync(user);

            HashSet<string>? workspaces = this.authorizedUser.IsAuthenticated && this.authorizedUser.IsObserver
                ? this.authorizedUser.Workspaces.ToHashSet()
                : null;

            if (workspaces == null 
                && principal.Identity is ClaimsIdentity principalIdentity
                && principalIdentity.HasClaim(c => c.Type == AuthorizedUser.ObserverClaimType))
            {
                workspaces = principalIdentity.Claims
                    .Where(c => c.Type == WorkspaceConstants.ClaimType)
                    .Select(c => c.Value)
                    .ToHashSet();
            }
            
            if (principal.Identity is ClaimsIdentity claimIde && user.PasswordChangeRequired)
                claimIde.AddClaim(new Claim(AuthorizedUser.PasswordChangeRequiredType, "true"));

            this.inScopeExecutor.Execute(workspacesService =>
            {
                var userWorkspaces = user.IsInRole(UserRoles.Administrator)
                    ? workspacesService.AllWorkspaces()
                    : user.Workspaces.Select(x => x.Workspace.AsContext());
                
                if (principal.Identity is ClaimsIdentity principalIdentity)
                {
                    foreach (var workspace in userWorkspaces)
                    {
                        if(workspaces != null && !workspaces.Contains(workspace.Name)) continue;
                        if(principalIdentity.HasClaim(WorkspaceConstants.ClaimType, workspace.Name)) continue;

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
