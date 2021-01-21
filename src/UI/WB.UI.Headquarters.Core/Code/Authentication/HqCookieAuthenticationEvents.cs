using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Infrastructure.Native.Workspaces;

namespace WB.UI.Headquarters.Code.Authentication
{
    public class HqCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        private readonly IWorkspacesCache workspacesCache;
        private readonly IUserRepository userRepository;
        private readonly IUserClaimsPrincipalFactory<HqUser> claimFactory;

        public HqCookieAuthenticationEvents
        (
            IWorkspacesCache workspacesCache,
            IUserRepository userRepository,
            IUserClaimsPrincipalFactory<HqUser> claimFactory)
        {
            this.workspacesCache = workspacesCache;
            this.userRepository = userRepository;
            this.claimFactory = claimFactory;
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
                    ctx.RedirectUri += "&reason=" + reason.ToString();
                }

                ctx.Response.Redirect(ctx.RedirectUri);
            }

            return Task.CompletedTask;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var userPrincipal = context.Principal;

            // Look for the LastChanged claim.
            var workspaceRevision = (from c in userPrincipal.Claims
                where c.Type == WorkspaceConstants.RevisionClaimType
                select c.Value).FirstOrDefault();

            if (string.IsNullOrEmpty(workspaceRevision) ||
                workspacesCache.Revision().ToString() != workspaceRevision)
            {
                var id = (userPrincipal?.Identity as ClaimsIdentity)?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (id != null && Guid.TryParse(id, out var userId))
                {
                    var hqUser = await this.userRepository.FindByIdAsync(userId);
                    
                    var newPrincipal = await claimFactory.CreateAsync(hqUser);
                    context.ReplacePrincipal(newPrincipal);
                    context.ShouldRenew = true;
                }
            }
        }
    }
}
