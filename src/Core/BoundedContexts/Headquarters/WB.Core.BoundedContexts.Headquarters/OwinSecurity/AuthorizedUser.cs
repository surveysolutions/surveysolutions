using System;
using System.Security.Claims;
using System.Threading;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public class AuthorizedUser : IAuthorizedUser
    {
        public const string ObserverClaimType = "observer";
        public const string DeviceClaimType = "device";

        private ClaimsPrincipal User => Thread.CurrentPrincipal as ClaimsPrincipal;

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
                var userId = this.User?.Identity.GetUserId();
                return userId != null ? Guid.Parse(userId) : Guid.Empty;
            }
        }

        public string UserName => this.User.Identity.Name;

        public string DeviceId => this.User.FindFirst(DeviceClaimType)?.Value;

        public UserRoles Role
            => (UserRoles)Enum.Parse(typeof(UserRoles), this.User.FindFirst(ClaimTypes.Role).Value);
    }
}