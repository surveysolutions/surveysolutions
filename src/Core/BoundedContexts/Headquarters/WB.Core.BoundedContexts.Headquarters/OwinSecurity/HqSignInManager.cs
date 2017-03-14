using System;
using System.Security.Claims;
using System.Threading.Tasks;
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
            => user.GenerateUserIdentityAsync((HqUserManager) this.UserManager);

        public static HqSignInManager Create(IdentityFactoryOptions<HqSignInManager> options, IOwinContext context)
            => new HqSignInManager(context.GetUserManager<HqUserManager>(), context.Authentication);
    }
}