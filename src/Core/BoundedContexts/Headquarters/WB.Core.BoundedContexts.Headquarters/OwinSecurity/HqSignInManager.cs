using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public class HqSignInManager : SignInManager<HqUser, Guid>
    {
        public HqSignInManager(HqUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(HqUser user)
            => user.GenerateUserIdentityAsync((HqUserManager)this.UserManager);

        public static HqSignInManager Create(IdentityFactoryOptions<HqSignInManager> options, IOwinContext context)
            => new HqSignInManager(context.GetUserManager<HqUserManager>(), context.Authentication);

        public async Task<SignInStatus> SignInAsync(string userName, string password, bool isPersistent = false)
        {
            var user = await this.UserManager.FindByNameAsync(userName);
            if (user == null || !await this.UserManager.CheckPasswordAsync(user, password))
                return SignInStatus.Failure;

            if (user.Roles.First().Role == UserRoles.Interviewer || user.IsLockedByHeadquaters || user.IsLockedBySupervisor)
                return SignInStatus.LockedOut;

            return await this.PasswordSignInAsync(userName, password, isPersistent: isPersistent,
                shouldLockout: false);
        }

        public async Task SignInAsObserverAsync(string userName)
        {
            var userToObserve = await this.UserManager.FindByNameAsync(userName);
            userToObserve.Claims.Add(new HqUserClaim
            {
                UserId = userToObserve.Id,
                ClaimType = AuthorizedUser.ObserverClaimType,
                ClaimValue = this.AuthenticationManager.User.Identity.Name
            });
            userToObserve.Claims.Add(new HqUserClaim
            {
                UserId = userToObserve.Id,
                ClaimType = ClaimTypes.Role,
                ClaimValue = Enum.GetName(typeof(UserRoles), UserRoles.Observer)
            });

            await this.SignInAsync(userToObserve, true, true);
        }

        public async Task SignInBackFromObserverAsync()
        {
            var observerName = this.AuthenticationManager.User.FindFirst(AuthorizedUser.ObserverClaimType)?.Value;
            var observer = await this.UserManager.FindByNameAsync(observerName);

            this.AuthenticationManager.SignOut();

            await this.SignInAsync(observer, true, true);
        }
    }
}