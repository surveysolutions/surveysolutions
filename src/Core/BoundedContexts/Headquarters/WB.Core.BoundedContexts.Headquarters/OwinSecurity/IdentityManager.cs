using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Web;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public class IdentityManager : IIdentityManager
    {
        private readonly ApplicationUserManager userManager;
        private readonly ApplicationSignInManager signInManager;
        private readonly IAuthenticationManager authenticationManager;

        readonly Guid adminRole = ((byte)UserRoles.Administrator).ToGuid();

        public IdentityManager(ApplicationUserManager userManager, ApplicationSignInManager signInManager,
            IAuthenticationManager authenticationManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.authenticationManager = authenticationManager;
        }

        public bool HasAdministrator => this.userManager.Users.Any(
            user => user.Roles.Any(role => role.RoleId == this.adminRole));

        public bool IsCurrentUserSupervisor => this.IsCurrentUserInRole(UserRoles.Supervisor);

        public bool IsCurrentUserObserver => this.authenticationManager.User.HasClaim(
            claim => claim.Issuer == @"ObserverName" && !string.IsNullOrEmpty(claim.Value));

        public bool IsCurrentUserAdministrator => this.IsCurrentUserInRole(UserRoles.Administrator);
        public bool IsCurrentUserHeadquarter => this.IsCurrentUserInRole(UserRoles.Headquarter);

        private bool IsCurrentUserInRole(UserRoles role) => this.authenticationManager.User.IsInRole(role.ToString());

        public ApplicationUser CurrentUser => this.userManager.Users?.FirstOrDefault(user=>user.Id == this.CurrentUserId);
        public IQueryable<ApplicationUser> Users => this.userManager.Users;
        public Guid CurrentUserId => Guid.Parse(this.authenticationManager.User.Identity.GetUserId());
        public string CurrentUserName => this.authenticationManager.User.Identity.Name;

        public string CurrentUserDeviceId => this.authenticationManager.User.FindFirst(@"DeviceId")?.Value;

        public IdentityResult CreateUser(ApplicationUser user, string password, UserRoles role)
        {
            user.CreationDate = DateTime.UtcNow;

            var creationStatus = this.userManager.Create(user, password);
            if (creationStatus.Succeeded)
                creationStatus = this.userManager.AddToRole(user.Id, Enum.GetName(typeof(UserRoles), role));

            return creationStatus;
        }

        public async Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password, UserRoles role)
        {
            user.CreationDate = DateTime.UtcNow;
            
            var creationStatus = await this.userManager.CreateAsync(user, password);
            if (creationStatus.Succeeded)
                creationStatus = this.userManager.AddToRole(user.Id, Enum.GetName(typeof(UserRoles), role));

            return creationStatus;
        }

        public async Task<IdentityResult> UpdateUserAsync(ApplicationUser user, string password)
        {
            if (!string.IsNullOrWhiteSpace(password))
                user.PasswordHash = this.userManager.PasswordHasher.HashPassword(password);

            return await this.userManager.UpdateAsync(user);
        }

        public IdentityResult UpdateUser(ApplicationUser user, string password)
        {
            if (!string.IsNullOrWhiteSpace(password))
                user.PasswordHash = this.userManager.PasswordHasher.HashPassword(password);

            return this.userManager.Update(user);
        }

        public async Task<UserRoles> GetRoleForCurrentUserAsync()
        {
            var userRoles = await this.userManager.GetRolesAsync(this.CurrentUserId);

            return (UserRoles) Enum.Parse(typeof(UserRoles), userRoles[0]);
        }

        public void SignOut() => this.authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

        public async Task<ApplicationUser> GetUserByIdAsync(Guid userId) => await this.userManager.FindByIdAsync(userId);

        public ApplicationUser GetUserById(Guid userId) => this.userManager.FindById(userId);

        public async Task<ApplicationUser> GetUserByNameAsync(string userName) => await this.userManager.FindByNameAsync(userName);
        public ApplicationUser GetUserByName(string userName) => this.userManager.FindByName(userName);

        public async Task<ApplicationUser> GetUserByEmailAsync(string email) => await this.userManager.FindByEmailAsync(email);
        public ApplicationUser GetUserByEmail(string email) => this.userManager.FindByEmail(email);

        public IEnumerable<IdentityResult> DeleteSupervisorAndDependentInterviewers(Guid supervisorId)
        {
            var supervisorAndDependentInterviewers = this.userManager.Users.Where(
                user => user.SupervisorId == supervisorId || user.Id == supervisorId);

            foreach (var supervisorAndDependentInterviewer in supervisorAndDependentInterviewers)
            {
                yield return this.userManager.Delete(supervisorAndDependentInterviewer);
            }
        }

        public void LinkDeviceToCurrentInterviewer(string deviceId)
        {
            if (!this.IsCurrentUserInRole(UserRoles.Interviewer))
                throw new AuthenticationException(@"Only interviewer can be linked to device");

            var currentUser = this.CurrentUser;
            currentUser.DeviceId = deviceId;

            this.UpdateUser(currentUser, null);
        }

        public bool IsUserValidWithPassword(string userName, string password)
            => this.IsUserValidWithPasswordHash(userName, this.userManager.PasswordHasher.HashPassword(password));

        public bool IsUserValidWithPasswordHash(string userName, string passwordHash) =>
            this.userManager.Find(userName, passwordHash) != null;

        public async Task<IdentityResult[]> ArchiveUsersAsync(Guid[] userIds, bool archive)
        {
            var archiveUserResults = new List<IdentityResult>();

            var usersToArhive = this.userManager.Users.Where(user => userIds.Contains(user.Id)).ToList();
            foreach (var userToArchive in usersToArhive)
            {
                userToArchive.IsArchived = archive;
                var archiveResult = await this.UpdateUserAsync(userToArchive, null);
                archiveUserResults.Add(archiveResult);
            }

            return archiveUserResults.ToArray();
        }

        public Task<SignInStatus> SignInAsync(string userName, string password, bool isPersistent = false)
            => this.signInManager.PasswordSignInAsync(userName, password, isPersistent: isPersistent,
                shouldLockout: false);

        public async Task SignInAsObserverAsync(string userName)
        {
            var observer = await this.GetUserByNameAsync(userName);
            observer.ObserverName = this.CurrentUserName;

            await this.signInManager.SignInAsync(observer, true, true);
        }

        public async Task SignInBackFromObserverAsync()
        {
            var observerName = this.authenticationManager.User.FindFirst("ObserverName")?.Value;
            var observer = await this.GetUserByNameAsync(observerName);

            this.CurrentUser.ObserverName = string.Empty;
            this.authenticationManager.SignOut();

            await this.signInManager.SignInAsync(observer, true, true);
        }

        public RestCredentials GetDesignerUserCredentials()
            => (RestCredentials) HttpContext.Current.Session[this.CurrentUserName];
    }
}