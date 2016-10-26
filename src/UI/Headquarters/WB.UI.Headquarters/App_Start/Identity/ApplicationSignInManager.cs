using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.UI.Headquarters.Identity
{
    public class ApplicationSignInManager : SignInManager<ApplicationUser, Guid>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
            => user.GenerateUserIdentityAsync((ApplicationUserManager) this.UserManager);

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
            => new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
    }
}