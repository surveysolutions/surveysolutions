using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity.Providers;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public class HqUserManager : UserManager<HqUser, Guid>
    {
        private readonly IAuthorizedUser authorizedUser;

        public HqUserManager(IUserStore<HqUser, Guid> store, IAuthorizedUser authorizedUser)
        private readonly IHashCompatibilityProvider hashCompatibilityProvider;
        private IApiTokenProvider<Guid> ApiTokenProvider { get; set; }

        public HqUserManager(IUserStore<HqUser, Guid> store, IHashCompatibilityProvider hashCompatibilityProvider, IApiTokenProvider<Guid> tokenProvider = null)
            : base(store)
        {
            this.authorizedUser = authorizedUser;
            this.ApiTokenProvider = tokenProvider ?? new ApiAuthTokenProvider<HqUser, Guid>(this);
        }

        public Task<IdentityResult> ChangePasswordAsync( HqUser user, string newPassword)
        {
            var passwordStore = this.Store as IUserPasswordStore<HqUser, Guid>;
            if (passwordStore == null) throw new NotImplementedException();
            
            return this.UpdatePassword(passwordStore, user, newPassword);
        }
        
        protected override async Task<IdentityResult> UpdatePassword(IUserPasswordStore<HqUser, Guid> passwordStore, HqUser user, string newPassword)
        {
            var result = await base.UpdatePassword(passwordStore, user, newPassword).ConfigureAwait(false);

            if (result == IdentityResult.Success && this.hashCompatibilityProvider.IsSHA1Required(user))
            {
                user.PasswordHashSha1 = this.hashCompatibilityProvider.GetSHA1HashFor(user, newPassword);
            }

            return result;
        }

        protected override Task<bool> VerifyPasswordAsync(IUserPasswordStore<HqUser, Guid> store, HqUser user, string password)
        {
            if (user == null) return Task.FromResult(false);

            var result = this.PasswordHasher.VerifyHashedPassword(user.PasswordHash, password) == PasswordVerificationResult.Success;

            if (!result && this.hashCompatibilityProvider.IsSHA1Required(user))
            {
                result = user.PasswordHashSha1 == this.hashCompatibilityProvider.GetSHA1HashFor(user, password);
            }

            return Task.FromResult(result);
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
          
            var hashCompatibility = ServiceLocator.Current.GetInstance<IHashCompatibilityProvider>();
            
            var manager = new HqUserManager(store, hashCompatibility)
            {
                PasswordHasher = ServiceLocator.Current.GetInstance<IPasswordHasher>()
                MaxFailedAccessAttemptsBeforeLockout = 5,
                PasswordValidator = new PasswordValidator
                {
                    RequireDigit = false,
                    RequireLowercase = false,
                    RequireNonLetterOrDigit = false,
                    RequireUppercase = false,
                    RequiredLength = 1
                }
            };
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
                user.PasswordHash = this.PasswordHasher.HashPassword(password);

            return await this.UpdateAsync(user);
        }

        public virtual IdentityResult UpdateUser(HqUser user, string password)
        {
            if (!string.IsNullOrWhiteSpace(password))
                user.PasswordHash = this.PasswordHasher.HashPassword(password);

            return this.Update(user);
        }

        public virtual async Task<IEnumerable<IdentityResult>> ArchiveSupervisorAndDependentInterviewersAsync(Guid supervisorId)
        {
            var supervisorAndDependentInterviewers = this.Users.Where(
                user => user.Profile != null && user.Profile.SupervisorId == supervisorId || user.Id == supervisorId).ToList();

            List<IdentityResult> result = new List<IdentityResult>();
            foreach (var accountToArchive in supervisorAndDependentInterviewers)
            {
                accountToArchive.IsArchived = true;
                var archiveResult = await this.UpdateUserAsync(accountToArchive, null).ConfigureAwait(false);
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