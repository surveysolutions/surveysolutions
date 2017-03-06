using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class AppUserLogin : IdentityUserLogin<Guid> { }

    public class AppUserRole : IdentityUserRole<Guid>
    {
        public UserRoles Role => (UserRoles)this.RoleId.ToByteArray().Last();
    }
    public class AppUserClaim : IdentityUserClaim<Guid> { }
    public class AppRole : IdentityRole<Guid, AppUserRole> { }
    public class ApplicationUser : IdentityUser<Guid, AppUserLogin, AppUserRole, AppUserClaim>
    {
        public virtual string FullName { get; set; }
        public virtual bool IsArchived { get; set; }
        public virtual bool IsLockedBySupervisor{get; set; }
        public virtual bool IsLockedByHeadquaters { get; set; }
        public virtual Guid? SupervisorId { get; set; }
        public virtual DateTime CreationDate { get; set; }
        public virtual string DeviceId { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, Guid> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            userIdentity.AddClaims(new []
            {
                new Claim("DeviceId", this.DeviceId ?? string.Empty)
            });

            return userIdentity;
        }
    }
}