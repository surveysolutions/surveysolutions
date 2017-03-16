using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class HqUserLogin : IdentityUserLogin<Guid> { }

    public class HqUserRole : IdentityUserRole<Guid>
    {
        public UserRoles Role => this.RoleId.ToUserRole();
    }
    public class HqUserClaim : IdentityUserClaim<Guid> { }
    public class HqRole : IdentityRole<Guid, HqUserRole> { }
    public class HqUser : IdentityUser<Guid, HqUserLogin, HqUserRole, HqUserClaim>
    {
        public virtual string FullName { get; set; }
        public virtual bool IsArchived { get; set; }
        public virtual bool IsLockedBySupervisor{get; set; }
        public virtual bool IsLockedByHeadquaters { get; set; }
        public virtual Guid? SupervisorId { get; set; }
        public virtual DateTime CreationDate { get; set; }
        public virtual string DeviceId { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<HqUser, Guid> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie)
                .ConfigureAwait(false);

            userIdentity.AddClaims(new []
            {
                new Claim(AuthorizedUser.DeviceClaimType, this.DeviceId ?? string.Empty)
            });

            return userIdentity;
        }
    }
}