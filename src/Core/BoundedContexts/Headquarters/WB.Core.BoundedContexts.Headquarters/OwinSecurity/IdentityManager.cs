using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Implementation;
using IPasswordHasher = WB.Core.GenericSubdomains.Portable.IPasswordHasher;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public class IdentityManager : IIdentityManager
    {
        private readonly HqUserManager userManager;
        private readonly HqSignInManager signInManager;
        private readonly IPasswordHasher oldPasswordHasher;
        private readonly IAuthenticationManager authenticationManager;
        
        const string observerClaimType = @"observer";

        public IdentityManager(HqUserManager userManager, HqSignInManager signInManager,
            IPasswordHasher oldPasswordHasher,
            IAuthenticationManager authenticationManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.oldPasswordHasher = oldPasswordHasher;
            this.authenticationManager = authenticationManager;
        }

        public bool IsCurrentUserSupervisor => this.IsCurrentUserInRole(UserRoles.Supervisor);

        public bool IsCurrentUserObserver => this.authenticationManager.User.HasClaim(claim => claim.Type == observerClaimType);

        public bool IsCurrentUserAdministrator => this.IsCurrentUserInRole(UserRoles.Administrator);
        public bool IsCurrentUserHeadquarter => this.IsCurrentUserInRole(UserRoles.Headquarter);

        private bool IsCurrentUserInRole(UserRoles role) => this.authenticationManager.User.IsInRole(role.ToString());

        public HqUser CurrentUser => this.userManager.Users?.FirstOrDefault(user=>user.Id == this.CurrentUserId);
        public Guid CurrentUserId => Guid.Parse(this.authenticationManager.User.Identity.GetUserId());
        public string CurrentUserName => this.authenticationManager.User.Identity.Name;

        public string CurrentUserDeviceId => this.authenticationManager.User.FindFirst(@"DeviceId")?.Value;

        public IdentityResult CreateUser(HqUser user, string password, UserRoles role)
        {
            user.CreationDate = DateTime.UtcNow;

            var creationStatus = this.userManager.Create(user, password);
            if (creationStatus.Succeeded)
                creationStatus = this.userManager.AddToRole(user.Id, Enum.GetName(typeof(UserRoles), role));

            return creationStatus;
        }

        public async Task<IdentityResult> CreateUserAsync(HqUser user, string password, UserRoles role)
        {
            user.CreationDate = DateTime.UtcNow;
            
            var creationStatus = await this.userManager.CreateAsync(user, password).ConfigureAwait(false);

            if (creationStatus.Succeeded)
                creationStatus = this.userManager.AddToRole(user.Id, Enum.GetName(typeof(UserRoles), role));

            return creationStatus;
        }

        public async Task<IdentityResult> UpdateUserAsync(HqUser user, string password)
        {
            if (!string.IsNullOrWhiteSpace(password))
            {
                var result = await this.userManager.ChangePasswordAsync(user, password).ConfigureAwait(false);

                if (result != IdentityResult.Success)
                    return result;
            }
            
            return await this.userManager.UpdateAsync(user).ConfigureAwait(false);
        }

        public IdentityResult UpdateUser(HqUser user, string password)
        {
            if (!string.IsNullOrWhiteSpace(password))
            {
                this.userManager.ChangePassword(user, password);
            }

            return this.userManager.Update(user);
        }

        public async Task<UserRoles> GetRoleForCurrentUserAsync()
        {
            var userRoles = await this.userManager.GetRolesAsync(this.CurrentUserId).ConfigureAwait(false);

            return (UserRoles) Enum.Parse(typeof(UserRoles), userRoles[0]);
        }

        public Task<HqUser> GetUserByIdAsync(Guid userId) => this.userManager.FindByIdAsync(userId);

        public HqUser GetUserById(Guid userId) => this.userManager.FindById(userId);

        public Task<HqUser> GetUserByNameAsync(string userName) => this.userManager.FindByNameAsync(userName);
        public HqUser GetUserByName(string userName) => this.userManager.FindByName(userName);

        public Task<HqUser> GetUserByEmailAsync(string email) =>  this.userManager.FindByEmailAsync(email);
        public HqUser GetUserByEmail(string email) => this.userManager.FindByEmail(email);

        public async Task<IEnumerable<IdentityResult>> ArchiveSupervisorAndDependentInterviewersAsync(Guid supervisorId)
        {
            var supervisorAndDependentInterviewers = this.userManager.Users.Where(
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
        
        public async Task<string> GenerateInterviewerTokenAsync(string userName)
        {
            var user = await this.userManager.FindByNameAsync(userName).ConfigureAwait(false);
            return await this.userManager.GenerateApiAuthTokenAsync(user.Id).ConfigureAwait(false);
        }

        public async Task<bool> ValidateInterviewerTokenAsync(string userName, string token)
        {
            var user = await this.userManager.FindByNameAsync(userName).ConfigureAwait(false);
            return await this.userManager.ValidateApiAuthTokenAsync(user.Id, token).ConfigureAwait(false);
        }

        public void LinkDeviceToCurrentInterviewer(string deviceId)
        {
            if (!this.IsCurrentUserInRole(UserRoles.Interviewer))
                throw new AuthenticationException(@"Only interviewer can be linked to device");

            var currentUser = this.CurrentUser;
            currentUser.DeviceId = deviceId;

            this.UpdateUser(currentUser, null);
        }

        public async Task<IdentityResult[]> ArchiveUsersAsync(Guid[] userIds, bool archive)
        {
            var archiveUserResults = new List<IdentityResult>();

            var usersToArhive = this.userManager.Users.Where(user => userIds.Contains(user.Id)).ToList();
            foreach (var userToArchive in usersToArhive)
            {
                userToArchive.IsArchived = archive;
                var archiveResult = await this.UpdateUserAsync(userToArchive, null).ConfigureAwait(false);
                archiveUserResults.Add(archiveResult);
            }

            return archiveUserResults.ToArray();
        }

        public async Task<SignInStatus> SignInAsync(string userName, string password, bool isPersistent = false)
        {
            var user = await this.GetUserByNameAsync(userName).ConfigureAwait(false);
            if (user == null || !await this.userManager.CheckPasswordAsync(user, password).ConfigureAwait(false))
                return SignInStatus.Failure;
            
            if(user.IsLockedByHeadquaters || user.IsLockedBySupervisor)
                return SignInStatus.LockedOut;

            return
                await this.signInManager.PasswordSignInAsync(userName, password, isPersistent: isPersistent, shouldLockout: false).ConfigureAwait(false);
        }

        public async Task SignInAsObserverAsync(string userName)
        {
            var userToObserve = await this.GetUserByNameAsync(userName).ConfigureAwait(false);
            userToObserve.Claims.Add(new HqUserClaim
            {
                UserId = userToObserve.Id,
                ClaimType = observerClaimType,
                ClaimValue = this.CurrentUserName
            });
            userToObserve.Claims.Add(new HqUserClaim
            {
                UserId = userToObserve.Id,
                ClaimType = ClaimTypes.Role,
                ClaimValue = Enum.GetName(typeof(UserRoles), UserRoles.Observer)
            });

            await this.signInManager.SignInAsync(userToObserve, true, true).ConfigureAwait(false);
        }

        public async Task SignInBackFromObserverAsync()
        {
            var observerName = this.authenticationManager.User.FindFirst(observerClaimType)?.Value;
            var observer = await this.GetUserByNameAsync(observerName).ConfigureAwait(false);
            
            this.authenticationManager.SignOut();

            await this.signInManager.SignInAsync(observer, true, true).ConfigureAwait(false);
        }

        public RestCredentials GetDesignerUserCredentials()
            => (RestCredentials) HttpContext.Current.Session[this.CurrentUserName];
    }
}