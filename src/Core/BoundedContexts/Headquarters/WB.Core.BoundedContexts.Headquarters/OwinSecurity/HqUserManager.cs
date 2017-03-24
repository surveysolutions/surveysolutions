using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity.Providers;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;

using IPasswordHasher = Microsoft.AspNet.Identity.IPasswordHasher;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public class HqUserManager : UserManager<HqUser, Guid>
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly IHashCompatibilityProvider hashCompatibilityProvider;
        private IApiTokenProvider<Guid> ApiTokenProvider { get; set; }

        public HqUserManager(IUserStore<HqUser, Guid> store, IAuthorizedUser authorizedUser, 
            IHashCompatibilityProvider hashCompatibilityProvider, IApiTokenProvider<Guid> tokenProvider = null)
            : base(store)
        {
            this.authorizedUser = authorizedUser;
            this.hashCompatibilityProvider = hashCompatibilityProvider;
            this.ApiTokenProvider = tokenProvider ?? new ApiAuthTokenProvider<HqUser, Guid>(this);
        }

        public Task<IdentityResult> ChangePasswordAsync( HqUser user, string newPassword)
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
            if (this.hashCompatibilityProvider.IsInSha1CompatibilityMode() && user.IsInRole(UserRoles.Interviewer))
            {
                user.PasswordHashSha1 = this.hashCompatibilityProvider.GetSHA1HashFor(user, newPassword);
            }

            return base.UpdatePassword(passwordStore, user, newPassword);
        }

        protected override async Task<bool> VerifyPasswordAsync(IUserPasswordStore<HqUser, Guid> store, HqUser user, string password)
        {
            if (user == null) return false;

            var result = this.PasswordHasher.VerifyHashedPassword(user.PasswordHash, password) == PasswordVerificationResult.Success;

            if (!result)
            {
                // migrating passwords
                if (!user.IsInRole(UserRoles.Interviewer) 
                    && user.PasswordHash == user.PasswordHashSha1 
                    && user.PasswordHash == this.hashCompatibilityProvider.GetSHA1HashFor(user, password))
                {
                    user.PasswordHashSha1 = null;
                    result = await this.ChangePasswordAsync(user, password) == IdentityResult.Success;
                }
            }

            if (!result && this.hashCompatibilityProvider.IsInSha1CompatibilityMode() && user.IsInRole(UserRoles.Interviewer))
            {
                result = user.PasswordHashSha1 == this.hashCompatibilityProvider.GetSHA1HashFor(user, password);
            }

            return result ;
        }

        public Task<string> GenerateApiAuthTokenAsync(Guid userId)
        {
            return this.ApiTokenProvider.GenerateTokenAsync(userId);
        }

        public Task<bool> ValidateApiAuthTokenAsync(Guid userId, string token)
        {
            return this.ApiTokenProvider.ValidateTokenAsync(userId, token);
        }

        public static HqUserManager Create(IdentityFactoryOptions<HqUserManager> options, IOwinContext context)
        {
            var store = new HqUserStore(context.Get<HQIdentityDbContext>());
            var authorizedUser = ServiceLocator.Current.GetInstance<IAuthorizedUser>();
            var hashCompatibility = ServiceLocator.Current.GetInstance<IHashCompatibilityProvider>();

            var manager = new HqUserManager(store, authorizedUser, hashCompatibility)
            {
                PasswordHasher = ServiceLocator.Current.GetInstance<IPasswordHasher>(),
                PasswordValidator = ServiceLocator.Current.GetInstance<IIdentityValidator<string>>()
            };
            return manager;
        }

        public virtual IdentityResult CreateUser(HqUser user, string password, UserRoles role)
        {
            user.CreationDate = DateTime.UtcNow;

            var creationStatus = this.Create(user, password);
            if (creationStatus.Succeeded)
                creationStatus = this.AddToRole(user.Id, Enum.GetName(typeof(UserRoles), role));

            return creationStatus;
        }

        public virtual async Task<IdentityResult> CreateUserAsync(HqUser user, string password, UserRoles role)
        {
            user.CreationDate = DateTime.UtcNow;

            var creationStatus = await this.CreateAsync(user, password);
            if (creationStatus.Succeeded)
                creationStatus = await this.AddToRoleAsync(user.Id, Enum.GetName(typeof(UserRoles), role));

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
                accountToArchive.IsArchived = true;
                var archiveResult = await this.UpdateUserAsync(accountToArchive, null);
                result.Add(archiveResult);
            }

            return result;
        }

        public virtual void LinkDeviceToCurrentInterviewer(string deviceId)
        {
            if (this.authorizedUser.Role != UserRoles.Interviewer)
                throw new AuthenticationException(@"Only interviewer can be linked to device");

            var currentUser = this.Users?.FirstOrDefault(user => user.Id == this.authorizedUser.Id);
            currentUser.Profile.DeviceId = deviceId;

            this.UpdateUser(currentUser, null);
        }

        public virtual async Task<IdentityResult[]> ArchiveUsersAsync(Guid[] userIds, bool archive)
        {
            var archiveUserResults = new List<IdentityResult>();

            var usersToArhive = this.Users.Where(user => userIds.Contains(user.Id)).ToList();
            foreach (var userToArchive in usersToArhive)
            {
                userToArchive.IsArchived = archive;
                var archiveResult = await this.UpdateUserAsync(userToArchive, null);
                archiveUserResults.Add(archiveResult);
            }

            return archiveUserResults.ToArray();
        }
    }
}