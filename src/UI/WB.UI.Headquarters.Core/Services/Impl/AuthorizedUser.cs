using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Http;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Infrastructure.Native.Workspaces;

namespace WB.UI.Headquarters.Services.Impl
{
    public class AuthorizedUser : IAuthorizedUser
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        public const string ObserverClaimType = "observer";

        public AuthorizedUser(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal User => httpContextAccessor.HttpContext?.User;

        public bool IsSupervisor => this.IsCurrentUserInRole(UserRoles.Supervisor);

        public bool IsObserver => this.IsCurrentUserInRole(UserRoles.Observer);
        public bool IsObserving => this.User.HasClaim(claim => claim.Type == ObserverClaimType);
        public bool IsAuthenticated => this.User.Identity.IsAuthenticated;
        public bool IsInterviewer => this.IsCurrentUserInRole(UserRoles.Interviewer);
        public bool IsAdministrator => this.IsCurrentUserInRole(UserRoles.Administrator);
        public bool IsHeadquarter => this.IsCurrentUserInRole(UserRoles.Headquarter);

        private bool IsCurrentUserInRole(UserRoles role) => this.User.IsInRole(role.ToString());
        
        public Guid Id
        {
            get
            {
                var userId = (this.User?.Identity as ClaimsIdentity)?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return userId != null ? Guid.Parse(userId) : Guid.Empty;
            }
        }

        public string UserName => this.User?.Identity?.Name;

        public bool HasNonDefaultWorkspace => User.Claims.Any(x =>
            x.Type == WorkspaceConstants.ClaimType && x.Value != WorkspaceConstants.DefaultWorkspaceName);

        public IEnumerable<string> Workspaces => User.Claims.Where(x =>
            x.Type == WorkspaceConstants.ClaimType).Select(x => x.Value);
    }
}
