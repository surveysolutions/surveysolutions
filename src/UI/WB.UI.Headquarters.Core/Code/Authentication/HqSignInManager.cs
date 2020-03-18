using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.UI.Headquarters.Code.Authentication
{
    public class HqSignInManager : SignInManager<HqUser>
    {
        public HqSignInManager(UserManager<HqUser> userManager, 
            IHttpContextAccessor contextAccessor, 
            IUserClaimsPrincipalFactory<HqUser> claimsFactory, 
            IOptions<IdentityOptions> optionsAccessor, 
            ILogger<SignInManager<HqUser>> logger, 
            IAuthenticationSchemeProvider schemes, 
            IUserConfirmation<HqUser> confirmation) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
        }

        protected override Task<bool> IsLockedOut(HqUser user)
        {
            if (user.IsArchivedOrLocked)
            {
                return Task.FromResult(true);
            }

            return base.IsLockedOut(user);
        }
    }
}
