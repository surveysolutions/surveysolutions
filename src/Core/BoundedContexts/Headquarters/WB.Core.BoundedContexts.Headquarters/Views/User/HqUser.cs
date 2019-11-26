using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Device;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class HqUserLogin
    {
        protected bool Equals(HqUserLogin other)
        {
            return LoginProvider == other.LoginProvider && ProviderKey == other.ProviderKey && UserId.Equals(other.UserId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((HqUserLogin) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (LoginProvider != null ? LoginProvider.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ProviderKey != null ? ProviderKey.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ UserId.GetHashCode();
                return hashCode;
            }
        }

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
            Users = new List<HqUser>();
        }
        public virtual ICollection<HqUser> Users { get; set; }

        public virtual Guid Id { get; set; }

        public virtual string Name { get; set; }
    }
    public class HqUser
    {
        public HqUser()
        {
            Claims = new List<HqUserClaim>();
            Roles = new List<HqRole>();
            Logins = new List<HqUserLogin>();
            DeviceSyncInfos = new HashSet<DeviceSyncInfo>();
        }

        public virtual HqUserProfile Profile { get; set; }

        public virtual string FullName { get; set; }

        public virtual bool IsArchived { get; set; }
        public virtual bool IsLockedBySupervisor{get; set; }
        public virtual bool IsLockedByHeadquaters { get; set; }

        public virtual bool IsArchivedOrLocked => IsArchived || IsLockedByHeadquaters || IsLockedBySupervisor;

        public virtual DateTime CreationDate { get; set; }
        public virtual string PasswordHashSha1 { get; set; }

        public virtual bool IsInRole(UserRoles role)
        {
            return this.Roles.Any(r => r.Id.ToUserRole() == role);
        }

        public virtual string Email { get; set; }

        public virtual bool EmailConfirmed { get; set; }

        public virtual string PasswordHash { get; set; }

        public virtual string SecurityStamp { get; set; }

        public virtual string PhoneNumber { get; set; }

        public virtual bool PhoneNumberConfirmed { get; set; }

        public virtual bool TwoFactorEnabled { get; set; }

        /// <summary>
        ///     DateTime in UTC when lockout ends, any time in the past is considered not locked out.
        /// </summary>
        public virtual DateTime? LockoutEndDateUtc { get; set; }

        /// <summary>
        ///     Is lockout enabled for this user
        /// </summary>
        public virtual bool LockoutEnabled { get; set; }

        public virtual int AccessFailedCount { get; set; }

        public virtual ICollection<HqRole> Roles { get; set; }

        public virtual ICollection<HqUserClaim> Claims { get; set; }

        public virtual ICollection<HqUserLogin> Logins { get; set; }

        public virtual Guid Id { get; set; }

        public virtual string UserName { get; set; }

        public virtual ICollection<DeviceSyncInfo> DeviceSyncInfos { get; set; }
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
        public virtual long? StorageFreeInBytes { get; set; }
    }
}
