using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using IPasswordHasher = Microsoft.AspNet.Identity.IPasswordHasher;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public class HqUserManager : UserManager<HqUser, Guid>
    {
        private readonly IHashCompatibilityProvider hashCompatibilityProvider;
        private readonly IAuditLog auditLog;
        private IUserPasswordStore<HqUser, Guid> PasswordStore => this.Store as IUserPasswordStore<HqUser, Guid>;

        public HqUserManager(IUserStore<HqUser, Guid> store, 
            IHashCompatibilityProvider hashCompatibilityProvider, 
            IPasswordHasher passwordHasher, 
            IIdentityValidator<string> identityValidator, 
            IAuditLog auditLog)
            : base(store)
        {
            this.hashCompatibilityProvider = hashCompatibilityProvider;
            this.auditLog = auditLog;
            this.PasswordHasher = passwordHasher;
            this.PasswordValidator = identityValidator;
        }

        public async Task<IdentityResult> ChangePasswordAsync(HqUser user, string newPassword)
        {
            var result = await this.UpdatePassword(PasswordStore, user, newPassword);

            if (result.Succeeded)
            {
                return await UpdateAsync(user);
            }
            
            return result;
        }

        public override async Task<ClaimsIdentity> CreateIdentityAsync(HqUser user, string authenticationType)
        {
            var userIdentity = await base.CreateIdentityAsync(user, authenticationType);

            if (user.Profile?.DeviceId != null)
                userIdentity.AddClaim(new Claim(AuthorizedUser.DeviceClaimType, user.Profile.DeviceId));

            return userIdentity;
        }

        protected override async Task<IdentityResult> UpdatePassword(IUserPasswordStore<HqUser, Guid> passwordStore, HqUser user, string newPassword)
        {
            this.UpdateSha1PasswordIfNeeded(user, newPassword);
            return await base.UpdatePassword(passwordStore, user, newPassword);
        }

        [Obsolete("Since 5.19. Can be removed as soon as there is no usages of IN app version < 5.19")]
        private void UpdateSha1PasswordIfNeeded(HqUser user, string newPassword)
        {
            if (this.hashCompatibilityProvider.IsInSha1CompatibilityMode() && user.IsInRole(UserRoles.Interviewer))
            {
                user.PasswordHashSha1 = this.hashCompatibilityProvider.GetSHA1HashFor(user, newPassword);
            }
        }
        
        protected override async Task<bool> VerifyPasswordAsync(IUserPasswordStore<HqUser, Guid> store, HqUser user, string password)
        {
            if (user == null || password == null) return false;

            var result = this.PasswordHasher.VerifyHashedPassword(user.PasswordHash, password) == PasswordVerificationResult.Success;

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
            user.Roles.Add(new HqUserRole {UserId = user.Id, RoleId = role.ToUserId()});

            var result = await this.UpdatePassword(PasswordStore, user, password);

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

        public virtual IdentityResult UpdateUser(HqUser user, string password)
        {
            if (!string.IsNullOrWhiteSpace(password))
                this.ChangePasswordAsync(user, password).RunSynchronously();

            return this.Update(user);
        }

        public virtual async Task<IEnumerable<IdentityResult>> ArchiveSupervisorAndDependentInterviewersAsync(Guid supervisorId)
        {
            var supervisorAndDependentInterviewers = this.Users.Where(
                user => user.Profile != null && user.Profile.SupervisorId == supervisorId || user.Id == supervisorId).ToList();

            var result = new List<IdentityResult>();
            foreach (var accountToArchive in supervisorAndDependentInterviewers)
            {
                var archiveResult = await this.ArchiveUserAsync(accountToArchive);
                result.Add(archiveResult);
            }

            return result;
        }

        public virtual void LinkDeviceToInterviewerOrSupervisor(Guid interviewerId, string deviceId, DateTime deviceRegistrationDate)
        {
            var currentUser = this.FindById(interviewerId);

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
            this.UpdateUser(currentUser, null);
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

            var usersToArhive = this.Users.Where(user => userIds.Contains(user.Id)).ToList();
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

            var usersToUnarhive = this.Users.Where(user => userIds.Contains(user.Id)).ToList();
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
            return await this.UpdateUserAsync(userToUnarchive, null);
        }

        private async Task<IdentityResult> ArchiveUserAsync(HqUser userToArchive)
        {
            userToArchive.IsArchived = true;
            return await this.UpdateUserAsync(userToArchive, null);
        }

        public Task<bool> IsExistAnyUser()
        {
            return this.Users.AnyAsync();
        }
    }
}
