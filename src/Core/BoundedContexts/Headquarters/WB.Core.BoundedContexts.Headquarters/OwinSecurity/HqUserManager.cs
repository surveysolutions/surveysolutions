using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Identity;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using PasswordVerificationResult = WB.Core.GenericSubdomains.Portable.PasswordVerificationResult;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public class HqUserManager :
        IDisposable
    {
        /// <summary>
        ///     If true, will enable user lockout when users are created
        /// </summary>
        public bool UserLockoutEnabledByDefault { get; set; }

        public IIdentityPasswordHasher PasswordHasher => this.passwordHasher;
        public virtual IQueryable<HqUser> Users => this.store.Users;
        public bool SupportsUserSecurityStamp => true;

        private readonly IUserRepository store;
        private readonly IHashCompatibilityProvider hashCompatibilityProvider;
        private readonly IIdentityPasswordHasher passwordHasher;
        private readonly IPasswordValidator passwordValidator;
        private readonly IIdentityValidator identityValidator;
        private readonly ISystemLog auditLog;

        public HqUserManager(IUserRepository store, 
            IHashCompatibilityProvider hashCompatibilityProvider, 
            IIdentityPasswordHasher passwordHasher, 
            IPasswordValidator passwordValidator, 
            IIdentityValidator identityValidator,
            ISystemLog auditLog)
        {
            this.store = store;
            this.hashCompatibilityProvider = hashCompatibilityProvider;
            this.passwordHasher = passwordHasher;
            this.passwordValidator = passwordValidator;
            this.identityValidator = identityValidator;
            this.auditLog = auditLog;
        }

        public async Task<IdentityResult> ChangePasswordAsync(HqUser user, string newPassword)
        {
            var result = await this.UpdatePasswordAsync(user, newPassword);

            if (result.Succeeded)
            {
                return await UpdateAsync(user);
            }
            
            return result;
        }

        public async Task<ClaimsIdentity> CreateIdentityAsync(HqUser user, string authenticationType)
        {
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var id = new ClaimsIdentity(authenticationType, ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            id.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(), ClaimValueTypes.String));
            id.AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName, ClaimValueTypes.String));
            id.AddClaim(new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "ASP.NET Identity",
                ClaimValueTypes.String));

            id.AddClaim(new Claim("AspNet.Identity.SecurityStamp",
                await this.GetSecurityStampAsync(user.Id)));

            IList<string> roles = await this.GetRolesAsync(user.Id);
            foreach (string roleName in roles)
                id.AddClaim(new Claim(ClaimsIdentity.DefaultRoleClaimType, roleName, ClaimValueTypes.String));

            id.AddClaims(await this.GetClaimsAsync(user.Id));

            if (user.Profile?.DeviceId != null)
                id.AddClaim(new Claim(AuthorizedUser.DeviceClaimType, user.Profile.DeviceId));

            return id;
        }

        protected async Task<IdentityResult> UpdatePasswordAsync(HqUser user, string newPassword)
        {
            this.UpdateSha1PasswordIfNeeded(user, newPassword);
            
            var result = await passwordValidator.ValidateAsync(newPassword);
            if (!result.Succeeded)
            {
                return result;
            }
            await store.SetPasswordHashAsync(user, passwordHasher.Hash(newPassword));
            await UpdateSecurityStampInternal(user);

            return IdentityResult.Success;
        }

        [Obsolete("Since 5.19. Can be removed as soon as there is no usages of IN app version < 5.19")]
        private void UpdateSha1PasswordIfNeeded(HqUser user, string newPassword)
        {
            if (this.hashCompatibilityProvider.IsInSha1CompatibilityMode() && user.IsInRole(UserRoles.Interviewer))
            {
                user.PasswordHashSha1 = this.hashCompatibilityProvider.GetSHA1HashFor(user, newPassword);
            }
        }
        
        protected async Task<bool> VerifyPasswordAsync(HqUser user, string password)
        {
            if (user == null || password == null) return false;

            var result = passwordHasher.VerifyPassword(user.PasswordHash, password) == PasswordVerificationResult.Success;

            if (!result)
            {
                // migrating passwords
                if (!user.IsInRole(UserRoles.Interviewer)
                    && string.Equals(user.PasswordHash, user.PasswordHashSha1, StringComparison.Ordinal)
                    && string.Equals(user.PasswordHash, this.hashCompatibilityProvider.GetSHA1HashFor(user, password), StringComparison.Ordinal))
                {
                    var changeResult = await this.ChangePasswordAsync(user, password);

                    if (changeResult == IdentityResult.Success)
                    {
                        user.PasswordHashSha1 = null;
                        await this.UpdateAsync(user);
                    }

                    // We should not block user authorization if it's impossible to update SHA1 password to newer.
                    // This can happen if current password policy is more strict than provided password
                    result = true;
                }
            }

            if (!result && this.hashCompatibilityProvider.IsInSha1CompatibilityMode() && user.IsInRole(UserRoles.Interviewer))
            {
                result = string.Equals(user.PasswordHashSha1, this.hashCompatibilityProvider.GetSHA1HashFor(user, password), StringComparison.Ordinal);
            }

            return result;
        }

        private static readonly List<UserRoles> RolesToIncludeInAuditLog = new List<UserRoles>
        {
            UserRoles.Headquarter,
            UserRoles.ApiUser,
            UserRoles.Observer
        };

        public virtual async Task<IdentityResult> CreateUserAsync(HqUser user, string password, UserRoles role)
        {
            user.CreationDate = DateTime.UtcNow;

            var roleEntity = store.FindRole(role.ToUserId());
            user.Roles.Add(roleEntity);

            var result = await this.UpdatePasswordAsync(user, password);

            if (result.Succeeded)
            {
                var identityResult = await this.CreateAsync(user);
                if (identityResult.Succeeded && RolesToIncludeInAuditLog.Contains(role))
                {
                    this.auditLog.UserCreated(role, user.UserName);
                }
                return identityResult;
            }

            return result;
        }
        
        public virtual async Task<IdentityResult> UpdateUserAsync(HqUser user, string password)
        {
            if (!string.IsNullOrWhiteSpace(password))
                await this.ChangePasswordAsync(user, password);

            return await this.UpdateAsync(user);
        }

        public virtual async Task<IEnumerable<IdentityResult>> ArchiveSupervisorAndDependentInterviewersAsync(Guid supervisorId)
        {
            var supervisorAndDependentInterviewers = this.store.Users.Where(
                user => user.Profile != null && user.Profile.SupervisorId == supervisorId || user.Id == supervisorId).ToList();

            var result = new List<IdentityResult>();
            foreach (var accountToArchive in supervisorAndDependentInterviewers)
            {
                var archiveResult = await this.ArchiveUserAsync(accountToArchive);
                result.Add(archiveResult);
            }

            return result;
        }

        public virtual async Task LinkDeviceToInterviewerOrSupervisorAsync(Guid interviewerId, string deviceId, DateTime deviceRegistrationDate)
        {
            var currentUser = await this.FindByIdAsync(interviewerId);

            if (currentUser == null || currentUser.IsArchivedOrLocked)
            {
                throw new AuthenticationException(@"User not found or locked");
            }

            if(!currentUser.IsInRole(UserRoles.Interviewer) && !currentUser.IsInRole(UserRoles.Supervisor))
                throw new AuthenticationException(@"Only interviewer or supervisor can be linked to device");

            if (currentUser.Profile == null)
                currentUser.Profile = new HqUserProfile();

            currentUser.Profile.DeviceId = deviceId;
            currentUser.Profile.DeviceRegistrationDate = deviceRegistrationDate;
            await this.UpdateUserAsync(currentUser, null);
        }

        public virtual async Task<IdentityResult> MoveUserToAnotherTeamAsync(Guid interviewerId, Guid newSupervisorId, Guid previousSupervisorId)
        {
            var interviewer = await this.FindByIdAsync(interviewerId);
            var newSupervisor = await this.FindByIdAsync(newSupervisorId);
            var previousSupervisor = await this.FindByIdAsync(previousSupervisorId);

            interviewer.Profile.SupervisorId = newSupervisorId;

            this.auditLog.UserMovedToAnotherTeam(interviewer.UserName, newSupervisor.UserName, previousSupervisor.UserName);

            return await this.UpdateUserAsync(interviewer, null);
        }


        public virtual async Task<IdentityResult[]> ArchiveUsersAsync(Guid[] userIds)
        {
            var archiveUserResults = new List<IdentityResult>();

            var usersToArhive = this.store.Users.Where(user => userIds.Contains(user.Id)).ToList();
            foreach (HqUser userToArchive in usersToArhive)
            {
                if (userToArchive.IsInRole(UserRoles.Supervisor))
                {
                    archiveUserResults.AddRange(await this.ArchiveSupervisorAndDependentInterviewersAsync(userToArchive.Id));
                }
                else
                {
                    var archiveResult = await this.ArchiveUserAsync(userToArchive);
                    archiveUserResults.Add(archiveResult);
                }
            }

            return archiveUserResults.ToArray();
        }

        public virtual async Task<IdentityResult[]> UnarchiveUsersAsync(Guid[] userIds)
        {
            var archiveUserResults = new List<IdentityResult>();

            var usersToUnarhive = this.store.Users.Where(user => userIds.Contains(user.Id)).ToList();
            foreach (var userToUnarchive in usersToUnarhive)
            {
                var unArchiveResult = await this.UnarchiveUserAsync(userToUnarchive);
                archiveUserResults.Add(unArchiveResult);
            }

            return archiveUserResults.ToArray();
        }

        private async Task<IdentityResult> UnarchiveUserAsync(HqUser userToUnarchive)
        {
            if (userToUnarchive.IsInRole(UserRoles.Interviewer))
            {
                var supervisor = await this.FindByIdAsync(userToUnarchive.Profile.SupervisorId.Value);
                if (supervisor.IsArchived)
                    return IdentityResult.Failed(string.Format(HeadquarterUserCommandValidatorMessages.YouCantUnarchiveInterviewerUntilSupervisorIsArchivedFormat,
                            userToUnarchive.UserName));
            }

            userToUnarchive.IsArchived = false;
            var result = await this.UpdateUserAsync(userToUnarchive, null);

            if (result.Succeeded)
            {
                if (userToUnarchive.IsInRole(UserRoles.Supervisor))
                    this.auditLog.SupervisorUnArchived(userToUnarchive.UserName);
                else if (userToUnarchive.IsInRole(UserRoles.Interviewer))
                    this.auditLog.InterviewerUnArchived(userToUnarchive.UserName);
            }

            return result;
        }

        private async Task<IdentityResult> ArchiveUserAsync(HqUser userToArchive)
        {
            userToArchive.IsArchived = true;
            var result =  await this.UpdateUserAsync(userToArchive, null);

            if (result.Succeeded)
            {
                if (userToArchive.IsInRole(UserRoles.Supervisor))
                    this.auditLog.SupervisorArchived(userToArchive.UserName);
                else if (userToArchive.IsInRole(UserRoles.Interviewer))
                    this.auditLog.InterviewerArchived(userToArchive.UserName);
            }

            return result;
        }

        public bool IsExistAnyUser() => this.store.Users.Any();

        /// <summary>
        ///     Create a user with no password
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual async Task<IdentityResult> CreateAsync(HqUser user)
        {
            ThrowIfDisposed();
            await UpdateSecurityStampInternal(user);
            var result = await identityValidator.ValidateAsync(user);
            if (!result.Succeeded)
            {
                return result;
            }
            if (UserLockoutEnabledByDefault)
            {
                await store.SetLockoutEnabledAsync(user, true);
            }
            await store.CreateAsync(user);
            return IdentityResult.Success;
        }

        /// <summary>
        ///     Update a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual async Task<IdentityResult> UpdateAsync(HqUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var result = await identityValidator.ValidateAsync(user);
            if (!result.Succeeded)
            {
                return result;
            }
            await store.UpdateAsync(user);
            return IdentityResult.Success;
        }

        /// <summary>
        ///     Find a user by id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public virtual Task<HqUser> FindByIdAsync(Guid userId)
        {
            ThrowIfDisposed();
            return store.FindByIdAsync(userId);
        }

        /// <summary>
        ///     Find a user by user name
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public virtual Task<HqUser> FindByNameAsync(string userName)
        {
            ThrowIfDisposed();
            if (userName == null)
            {
                throw new ArgumentNullException("userName");
            }
            return store.FindByNameAsync(userName);
        }

        /// <summary>
        ///     Returns true if the password is valid for the user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public virtual async Task<bool> CheckPasswordAsync(HqUser user, string password)
        {
            ThrowIfDisposed();
            return await VerifyPasswordAsync(user, password);
        }
        
        /// <summary>
        ///     Returns the current security stamp for a user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public virtual async Task<string> GetSecurityStampAsync(Guid userId)
        {
            ThrowIfDisposed();
            var user = await FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The UserId cannot be found.",
                    userId));
            }
            return await this.store.GetSecurityStampAsync(user);
        }

        
        // Update the security stamp if the store supports it
        internal async Task UpdateSecurityStampInternal(HqUser user) 
            => await store.SetSecurityStampAsync(user, NewSecurityStamp());

        private static string NewSecurityStamp() => Guid.NewGuid().ToString();

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        public Task<IList<string>> GetRolesAsync(Guid userId) => this.store.GetRolesAsync(userId);

        public Task<IEnumerable<Claim>> GetClaimsAsync(Guid userId) => this.store.GetClaimsAsync(userId);


        /// <summary>
        ///     Dispose this object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        private bool _disposed;
        /// <summary>
        ///     When disposing, actually dispose the store
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
            }
        }
    }
}
