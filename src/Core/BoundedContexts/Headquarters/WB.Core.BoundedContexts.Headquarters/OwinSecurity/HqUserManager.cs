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
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public class HqUserManager : UserManager<HqUser, Guid>
    {
        private readonly IAuthorizedUser authorizedUser;

        public HqUserManager(IUserStore<HqUser, Guid> store, IAuthorizedUser authorizedUser)
            : base(store)
        {
            this.authorizedUser = authorizedUser;
        }

        public static HqUserManager Create(IdentityFactoryOptions<HqUserManager> options,
            IOwinContext context)
        {
            var authorizedUser = ServiceLocator.Current.GetInstance<IAuthorizedUser>();
            var manager = new HqUserManager(new HqUserStore(context.Get<HQIdentityDbContext>()), authorizedUser)
            {
                PasswordHasher = ServiceLocator.Current.GetInstance<IPasswordHasher>(),
                UserLockoutEnabledByDefault = false,
                DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5),
                MaxFailedAccessAttemptsBeforeLockout = 5
            };
            manager.UserValidator = new UserValidator<HqUser, Guid>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = false
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
                user => user.SupervisorId == supervisorId || user.Id == supervisorId).ToList();

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
            
            currentUser.DeviceId = deviceId;

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