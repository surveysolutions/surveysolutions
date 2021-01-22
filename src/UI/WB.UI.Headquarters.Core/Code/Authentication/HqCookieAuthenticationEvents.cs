#nullable enable
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.Infrastructure.Domain;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Services.Impl;

namespace WB.UI.Headquarters.Code.Authentication
{
    public class HqCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        private readonly IWorkspacesCache workspacesCache;
        private readonly IInScopeExecutor<IUserRepository> userRepository;
        private readonly IUserClaimsPrincipalFactory<HqUser> claimFactory;
        private readonly IWorkspacesUsersCache workspacesUsersCache;

        public HqCookieAuthenticationEvents
        (
            IWorkspacesCache workspacesCache,
            IInScopeExecutor<IUserRepository> userRepository,
            IUserClaimsPrincipalFactory<HqUser> claimFactory,
            IWorkspacesUsersCache workspacesUsersCache)
        {
            this.workspacesCache = workspacesCache;
            this.userRepository = userRepository;
            this.claimFactory = claimFactory;
            this.workspacesUsersCache = workspacesUsersCache;
        }

        public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> ctx)
        {
            if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            }
            else
            {
                ctx.Response.Redirect(ctx.RedirectUri);
            }

            return Task.CompletedTask;
        }

        public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> ctx)
        {
            if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            }
            else
            {
                if (ctx.Properties.TryGetForbidReason(out var reason))
                {
                    ctx.RedirectUri += "&reason=" + reason;
                }

                ctx.Response.Redirect(ctx.RedirectUri);
            }

            return Task.CompletedTask;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var userPrincipal = context.Principal;
            var userIdentity = userPrincipal?.Identity as ClaimsIdentity;

            if (userIdentity == null) return;

            Claim? GetFirstClaim(string type) => userIdentity.Claims.FirstOrDefault(c => c.Type == type);
            var id = GetFirstClaim(ClaimTypes.NameIdentifier)?.Value;

            if (id == null || !Guid.TryParse(id, out var userId))
            {
                return;
            }

            // Look for the LastChanged claim.
            var workspaceRevisionClaim = GetFirstClaim(WorkspaceConstants.RevisionClaimType);

            var hasWorkspacesChanged = workspaceRevisionClaim == null ||
                                       workspacesCache.Revision().ToString() != workspaceRevisionClaim?.Value;

            var userWorkspaces = await this.workspacesUsersCache.GetUserWorkspaces(userId);

            var claimedWorkspaces = userIdentity.Claims.Where(c => c.Type == WorkspaceConstants.ClaimType)
                .Select(c => c.Value).OrderBy(o => o).ToList();

            var userWorkspacesChanged = !userWorkspaces.SequenceEqual(claimedWorkspaces);

            if (hasWorkspacesChanged || userWorkspacesChanged)
            {
                var newPrincipal = await this.userRepository.ExecuteAsync(async u =>
                {
                    var hqUser = await u.FindByIdAsync(userId);
                    
                    var observerClaim = GetFirstClaim(AuthorizedUser.ObserverClaimType);

                    if (observerClaim != null)
                    {
                        hqUser.Claims.Add(HqUserClaim.FromClaim(observerClaim));

                        hqUser.Claims.Add(new HqUserClaim
                        {
                            ClaimType = ClaimTypes.Role,
                            ClaimValue = Enum.GetName(typeof(UserRoles), UserRoles.Observer)
                        });
                    }

                    return await claimFactory.CreateAsync(hqUser);
                });

                context.ReplacePrincipal(newPrincipal);
                context.ShouldRenew = true;
            }
        }
    }
}
