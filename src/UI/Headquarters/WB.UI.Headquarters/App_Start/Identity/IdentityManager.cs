using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;

namespace WB.UI.Headquarters.Identity
{
    internal class IdentityManager : IIdentityManager
    {
        private readonly ApplicationUserManager userManager;
        private readonly ApplicationSignInManager signInManager;
        private readonly IAuthenticationManager authenticationManager;
        private string currentUserId => this.authenticationManager.User.Identity.GetUserId();

        public IdentityManager(ApplicationUserManager userManager, ApplicationSignInManager signInManager,
            IAuthenticationManager authenticationManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.authenticationManager = authenticationManager;
        }

        public bool HasAdministrator => this.userManager.Users.Any(
            user => user.Roles.Any(role => role.RoleId == ((int)UserRoles.Administrator).ToString()));

        public bool IsCurrentUserSupervisor => this.IsCurrentUserInRole(UserRoles.Supervisor);

        public bool IsCurrentUserObserver => this.authenticationManager.User.HasClaim(
            claim => claim.Issuer == @"ObserverName" && !string.IsNullOrEmpty(claim.Value));

        public bool IsCurrentUserAdministrator => this.IsCurrentUserInRole(UserRoles.Administrator);
        public bool IsCurrentUserHeadquarter => this.IsCurrentUserInRole(UserRoles.Headquarter);

        private bool IsCurrentUserInRole(UserRoles role) => this.authenticationManager.User.IsInRole(role.ToString());

        public ApplicationUser CurrentUser => this.userManager.Users?.FirstOrDefault(user=>user.Id == this.currentUserId);
        public IQueryable<ApplicationUser> Users => this.userManager.Users;
        public Guid CurrentUserId => Guid.Parse(this.authenticationManager.User.Identity.GetUserId());
        public string CurrentUserName => this.authenticationManager.User.Identity.Name;

        public string CurrentUserDeviceId => this.authenticationManager.User.FindFirst(@"DeviceId")?.Value;

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

        public async Task<UserRoles> GetRoleForCurrentUserAsync()
        {
            var userRoles = await this.userManager.GetRolesAsync(this.currentUserId);

            return (UserRoles) Enum.Parse(typeof(UserRoles), userRoles[0]);
        }

        public void SignOut() => this.authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

        public async Task<ApplicationUser> GetUserById(string userId) => await this.userManager.FindByIdAsync(userId);

        public IEnumerable<IdentityResult> DeleteSupervisorAndDependentInterviewers(Guid supervisorId)
        {
            var supervisorAndDependentInterviewers = this.userManager.Users.Where(
                user => user.SupervisorId == supervisorId || user.Id == supervisorId.ToString());

            foreach (var supervisorAndDependentInterviewer in supervisorAndDependentInterviewers)
            {
                yield return this.userManager.Delete(supervisorAndDependentInterviewer);
            }
        }

        public async Task<ApplicationUser> GetUserByName(string userName) => await this.userManager.FindByNameAsync(userName);

        public Task<SignInStatus> SignInAsync(string userName, string password, bool isPersistent = false)
            => this.signInManager.PasswordSignInAsync(userName, password, isPersistent: isPersistent,
                shouldLockout: false);

        public async Task SignInAsObserverAsync(string userName)
        {
            var observer = await this.GetUserByName(userName);
            observer.ObserverName = this.CurrentUserName;

            await this.signInManager.SignInAsync(observer, true, true);
        }

        public async Task SignInBackFromObserverAsync()
        {
            var observerName = this.authenticationManager.User.FindFirst("ObserverName")?.Value;
            var observer = await this.GetUserByName(observerName);

            this.CurrentUser.ObserverName = string.Empty;
            this.authenticationManager.SignOut()

            await this.signInManager.SignInAsync(observer, true, true);
        }
    }
}