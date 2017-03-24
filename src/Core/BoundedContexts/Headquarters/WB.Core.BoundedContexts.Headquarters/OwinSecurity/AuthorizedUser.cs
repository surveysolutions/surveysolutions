using System;
using System.Security.Claims;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public class AuthorizedUser : IAuthorizedUser
    {
        private readonly IAuthenticationManager authenticationManager;

        public const string ObserverClaimType = "observer";
        public const string DeviceClaimType = "device";

        public AuthorizedUser(IAuthenticationManager authenticationManager)
        {
            this.authenticationManager = authenticationManager;
        }

        public bool IsSupervisor => this.IsCurrentUserInRole(UserRoles.Supervisor);

        public bool IsObserver => this.IsCurrentUserInRole(UserRoles.Observer);
        public bool IsObserving => this.authenticationManager.User.HasClaim(claim => claim.Type == ObserverClaimType);

        public bool IsAdministrator => this.IsCurrentUserInRole(UserRoles.Administrator);
        public bool IsHeadquarter => this.IsCurrentUserInRole(UserRoles.Headquarter);

        private bool IsCurrentUserInRole(UserRoles role) => this.authenticationManager.User.IsInRole(role.ToString());
        
        public Guid Id => Guid.Parse(this.authenticationManager.User.Identity.GetUserId());
        public string UserName => this.authenticationManager.User.Identity.Name;

        public string DeviceId => this.authenticationManager.User.FindFirst(DeviceClaimType)?.Value;

        public UserRoles Role
            => (UserRoles)Enum.Parse(typeof(UserRoles), this.authenticationManager.User.FindFirst(ClaimTypes.Role).Value);
    }
}