using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class ApplicationUser : IdentityUser
    {
        public virtual string FullName { get; set; }
        public virtual bool IsArchived { get; set; }
        public virtual bool IsLockedBySupervisor{get; set; }
        public virtual bool IsLockedByHeadquaters { get; set; }
        public virtual string ObserverName { get; set; }
        public virtual Guid? SupervisorId { get; set; }
        public virtual DateTime CreationDate { get; set; }
        public virtual string DeviceId { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            userIdentity.AddClaims(new []
            {
                new Claim("ObserverName", this.ObserverName ?? string.Empty),
                new Claim("DeviceId", this.DeviceId ?? string.Empty)
            });

            return userIdentity;
        }
    }
}