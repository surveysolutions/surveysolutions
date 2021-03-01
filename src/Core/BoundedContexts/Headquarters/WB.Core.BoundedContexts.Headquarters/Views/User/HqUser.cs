using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Identity;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class HqUserLogin : IdentityUserLogin<Guid>
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
    }

    public class HqUserClaim : IdentityUserClaim<Guid>
    {
        public static HqUserClaim FromClaim(Claim claim)
        {
            var hqClaim = new HqUserClaim();
            hqClaim.InitializeFromClaim(claim);
            return hqClaim;
        }
    }

    public class HqRole : IdentityRole<Guid>
    {
        public HqRole()
        {
            Users = new List<HqUser>();
        }

        public virtual ICollection<HqUser> Users { get; set; }
    }

    public class HqUser : IdentityUser<Guid>
    {
        public HqUser()
        {
            Claims = new List<HqUserClaim>();
            Roles = new List<HqRole>();
            Logins = new List<HqUserLogin>();
            DeviceSyncInfos = new HashSet<DeviceSyncInfo>();
            WorkspacesProfile = new HqUserProfile();
            Workspaces = new HashSet<WorkspacesUsers>();
        }

        public virtual HqUserProfile WorkspacesProfile { get; set; }
        public virtual WorkspaceUserProfile Profile { get; set; }

        public virtual string FullName { get; set; }

        public virtual bool IsArchived { get; set; }
        public virtual bool IsLockedBySupervisor{get; set; }
        public virtual bool IsLockedByHeadquaters { get; set; }

        public virtual bool IsArchivedOrLocked => IsArchived || IsLockedByHeadquaters || IsLockedBySupervisor;

        public virtual DateTime CreationDate { get; set; }
        public virtual string PasswordHashSha1 { get; set; }

        public virtual DateTime? LastLoginDate { get; set; }

        public virtual bool IsInRole(UserRoles role)
        {
            return this.Roles.Any(r => r.Id.ToUserRole() == role);
        }

        /// <summary>
        ///     DateTime in UTC when lockout ends, any time in the past is considered not locked out.
        /// </summary>
        public virtual DateTime? LockoutEndDateUtc { get; set; }

        public virtual ICollection<HqRole> Roles { get; set; }

        public virtual ICollection<HqUserClaim> Claims { get; set; }

        public virtual ICollection<HqUserLogin> Logins { get; set; }

        public virtual ICollection<DeviceSyncInfo> DeviceSyncInfos { get; set; }
        
        public virtual ISet<WorkspacesUsers> Workspaces { get; protected set; }
    }

    public class HqUserToken : IdentityUserToken<Guid>
    {
        public override bool Equals(object obj) => obj is HqUserToken token &&
                                                   UserId == token.UserId &&
                                                   LoginProvider == token.LoginProvider &&
                                                   Name == token.Name;

        public override int GetHashCode() => HashCode.Combine(UserId, LoginProvider, Name);
    }

    public class HqUserProfile
    {
        public virtual int Id { get; set; }
        public virtual string DeviceId { get; set; }
        public virtual DateTime? DeviceRegistrationDate { get; set; }
        public virtual string DeviceAppVersion { get; set; }
        public virtual int? DeviceAppBuildVersion { get; set; }
        public virtual long? StorageFreeInBytes { get; set; }
    }
    
    public class WorkspaceUserProfile
    {
        public virtual int Id { get; protected set; }
        public virtual Guid? SupervisorId { get; protected set; }
        public virtual string DeviceId { get; protected set; }
        public virtual DateTime? DeviceRegistrationDate { get; protected set; }
        public virtual string DeviceAppVersion { get; protected set; }
        public virtual int? DeviceAppBuildVersion { get; protected set; }
        public virtual long? StorageFreeInBytes { get; protected set; }
    }
}
