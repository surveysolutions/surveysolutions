using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class HqUserLogin
    {
        /// <summary>
        ///     The login provider for the login (i.e. facebook, google)
        /// </summary>
        public virtual string LoginProvider { get; set; }

        /// <summary>
        ///     Key representing the login for the provider
        /// </summary>
        public virtual string ProviderKey { get; set; }

        /// <summary>
        ///     User Id for the user who owns this login
        /// </summary>
        public virtual Guid UserId { get; set; }
    }

    public class HqUserRole
    {
        /// <summary>
        ///     UserId for the user that is in the role
        /// </summary>
        public virtual Guid UserId { get; set; }

        /// <summary>
        ///     RoleId for the role
        /// </summary>
        public virtual Guid RoleId { get; set; }

        public UserRoles Role => this.RoleId.ToUserRole();
    }

    public class HqUserClaim
    {
        /// <summary>
        /// Gets or sets the identifier for this user claim.
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// Gets or sets the primary key of the user associated with this claim.
        /// </summary>
        public virtual Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the claim type for this claim.
        /// </summary>
        public virtual string ClaimType { get; set; }

        /// <summary>
        /// Gets or sets the claim value for this claim.
        /// </summary>
        public virtual string ClaimValue { get; set; }
    }

    public class HqRole
    {
        public HqRole()
        {
            Users = new List<HqUserRole>();
        }
        /// <summary>
        ///     Navigation property for users in the role
        /// </summary>
        [ForeignKey("RoleId")]
        public virtual ICollection<HqUserRole> Users { get; }

        /// <summary>
        ///     Role id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        ///     Role name
        /// </summary>
        public string Name { get; set; }
    }
    public class HqUser
    {
        public HqUser()
        {
            Claims = new List<HqUserClaim>();
            Roles = new List<HqUserRole>();
            Logins = new List<HqUserLogin>();
        }

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

        /// <summary>
        ///     Email
        /// </summary>
        public virtual string Email { get; set; }

        /// <summary>
        ///     True if the email is confirmed, default is false
        /// </summary>
        public virtual bool EmailConfirmed { get; set; }

        /// <summary>
        ///     The salted/hashed form of the user password
        /// </summary>
        public virtual string PasswordHash { get; set; }

        /// <summary>
        ///     A random value that should change whenever a users credentials have changed (password changed, login removed)
        /// </summary>
        public virtual string SecurityStamp { get; set; }

        /// <summary>
        ///     PhoneNumber for the user
        /// </summary>
        public virtual string PhoneNumber { get; set; }

        /// <summary>
        ///     True if the phone number is confirmed, default is false
        /// </summary>
        public virtual bool PhoneNumberConfirmed { get; set; }

        /// <summary>
        ///     Is two factor enabled for the user
        /// </summary>
        public virtual bool TwoFactorEnabled { get; set; }

        /// <summary>
        ///     DateTime in UTC when lockout ends, any time in the past is considered not locked out.
        /// </summary>
        public virtual DateTime? LockoutEndDateUtc { get; set; }

        /// <summary>
        ///     Is lockout enabled for this user
        /// </summary>
        public virtual bool LockoutEnabled { get; set; }

        /// <summary>
        ///     Used to record failures for the purposes of lockout
        /// </summary>
        public virtual int AccessFailedCount { get; set; }

        /// <summary>
        ///     Navigation property for user roles
        /// </summary>
        [ForeignKey("UserId")]
        public virtual ICollection<HqUserRole> Roles { get; }

        /// <summary>
        ///     Navigation property for user claims
        /// </summary>
        [ForeignKey("UserId")]
        public virtual ICollection<HqUserClaim> Claims { get; }

        /// <summary>
        ///     Navigation property for user logins
        /// </summary>
        [ForeignKey("UserId")]
        public virtual ICollection<HqUserLogin> Logins { get; }

        /// <summary>
        ///     User ID (Primary Key)
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        ///     User name
        /// </summary>
        public virtual string UserName { get; set; }
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
