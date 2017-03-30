using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using IPasswordHasher = Microsoft.AspNet.Identity.IPasswordHasher;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public class HqUserManager : UserManager<HqUser, Guid>
    {
        private readonly IHashCompatibilityProvider hashCompatibilityProvider;
        private readonly ILogger logger;

        public HqUserManager(IUserStore<HqUser, Guid> store, IHashCompatibilityProvider hashCompatibilityProvider, 
            IPasswordHasher passwordHasher, IIdentityValidator<string> identityValidator, ILoggerProvider logger)
            : base(store)
        {
            this.hashCompatibilityProvider = hashCompatibilityProvider;
            this.PasswordHasher = passwordHasher;
            this.PasswordValidator = identityValidator;
            this.logger = logger.GetFor<HqUserManager>();
        }

        public Task<IdentityResult> ChangePasswordAsync(HqUser user, string newPassword)
        {
            var passwordStore = this.Store as IUserPasswordStore<HqUser, Guid>;
            if (passwordStore == null) throw new NotImplementedException();
            
            return this.UpdatePassword(passwordStore, user, newPassword);
        }

        public override async Task<ClaimsIdentity> CreateIdentityAsync(HqUser user, string authenticationType)
        {
            var userIdentity = await base.CreateIdentityAsync(user, authenticationType);

            if (user.Profile?.DeviceId != null)
                userIdentity.AddClaim(new Claim(AuthorizedUser.DeviceClaimType, user.Profile.DeviceId));

            return userIdentity;
        }

        protected override Task<IdentityResult> UpdatePassword(IUserPasswordStore<HqUser, Guid> passwordStore, HqUser user, string newPassword)
        {
            this.UpdateSha1PasswordIfNeeded(user, newPassword);

            return base.UpdatePassword(passwordStore, user, newPassword);
        }

        [Obsolete("Since 5.19. Can be removed as soon as there is no usages of IN app version < 5.19")]
        private void UpdateSha1PasswordIfNeeded(HqUser user, string newPassword)
        {
            if (this.hashCompatibilityProvider.IsInSha1CompatibilityMode() && user.IsInRole(UserRoles.Interviewer))
            {
                user.PasswordHashSha1 = this.hashCompatibilityProvider.GetSHA1HashFor(user, newPassword);
            }
        }

        [Obsolete("Since 5.19. Can be removed as soon as there is no usages of IN app version < 5.19")]
        private void UpdateSha1PasswordIfNeeded(HqUser user, string newPassword, UserRoles futureRole)
        {
            if (this.hashCompatibilityProvider.IsInSha1CompatibilityMode() && (user.IsInRole(UserRoles.Interviewer) || futureRole == UserRoles.Interviewer))
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

                    if (changeResult != IdentityResult.Success)
                    {
                        this.logger.Warn($"Unable to migrate password for user: {user.UserName}. " +
                                $"Reason(s): {string.Join("\r\n\r\n", changeResult.Errors)}");
                    }

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
        
        public virtual async Task<IdentityResult> CreateUserAsync(HqUser user, string password, UserRoles role)
        {
            user.CreationDate = DateTime.UtcNow;
            
            var creationStatus = await this.CreateAsync(user, password);

            if (creationStatus.Succeeded)
            {
                UpdateSha1PasswordIfNeeded(user, password, role);
                creationStatus = await this.AddToRoleAsync(user.Id, Enum.GetName(typeof(UserRoles), role));
            }

            return creationStatus;
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

        public virtual void LinkDeviceToInterviewer(Guid interviewerId, string deviceId)
        {
            var currentUser = this.FindById(interviewerId);

            if (currentUser == null || currentUser.IsArchivedOrLocked)
            {
                throw new AuthenticationException(@"User not found or locked");
            }

            if(!currentUser.IsInRole(UserRoles.Interviewer))
                throw new AuthenticationException(@"Only interviewer can be linked to device");

            currentUser.Profile.DeviceId = deviceId;
            this.UpdateUser(currentUser, null);
        }

        public virtual async Task<IdentityResult[]> ArchiveUsersAsync(Guid[] userIds, bool archive)
        {
            var archiveUserResults = new List<IdentityResult>();

            var usersToArhive = this.Users.Where(user => userIds.Contains(user.Id)).ToList();
            foreach (var userToArchive in usersToArhive)
            {
                var archiveResult = archive ? await this.ArchiveUserAsync(userToArchive) : await this.UnarchiveUserAsync(userToArchive);
                archiveUserResults.Add(archiveResult);
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
    }
}