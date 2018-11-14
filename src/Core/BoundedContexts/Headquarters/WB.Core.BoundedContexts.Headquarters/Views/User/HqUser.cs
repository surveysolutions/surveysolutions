using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity.EntityFramework;

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
        public virtual int? UserProfileId { get; set; }
        [ForeignKey(nameof(UserProfileId))]
        public virtual HqUserProfile Profile { get; set; }

        public virtual string FullName { get; set; }

        public virtual bool IsArchived { get; set; }
        public virtual bool IsLockedBySupervisor{get; set; }
        public virtual bool IsLockedByHeadquaters { get; set; }

        public bool IsArchivedOrLocked => IsArchived || IsLockedByHeadquaters || IsLockedBySupervisor;

        public virtual DateTime CreationDate { get; set; }
        public virtual string PasswordHashSha1 { get; set; }

        public bool IsInRole(UserRoles role)
        {
            return this.Roles.Any(r => r.Role == role);
        }
    }

    public class HqUserProfile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; set; }
        public virtual string DeviceId { get; set; }
        public virtual DateTime? DeviceRegistrationDate { get; set; }
        public virtual Guid? SupervisorId { get; set; }
        public virtual string DeviceAppVersion { get; set; }
        public virtual int? DeviceAppBuildVersion { get; set; }
    }
}
