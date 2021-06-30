using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.UI.Designer.Resources;
using WB.UI.Shared.Web.Authentication;

namespace WB.UI.Designer.Code.Attributes
{
    public class BasicBasicAuthenticationService : IBasicAuthenticationService
    {
        private readonly UserManager<DesignerIdentityUser> userManager;
        private readonly SignInManager<DesignerIdentityUser> signInManager;

        public BasicBasicAuthenticationService(UserManager<DesignerIdentityUser> userManager, 
            SignInManager<DesignerIdentityUser> signInManager)
        {
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        }

        public async Task<ClaimsPrincipal> AuthenticateAsync(BasicCredentials credentials)
        {
            var user = await this.userManager.FindByNameAsync(credentials.Username)
                       ?? await this.userManager.FindByEmailAsync(credentials.Username);

            if (!await this.Authorize(user, credentials.Username, credentials.Password))
            {
                throw new UnauthorizedException(ErrorMessages.User_Not_authorized, StatusCodes.Status401Unauthorized);
            }

            if (IsAccountLockedOut(user))
            {
                throw new UnauthorizedException(ErrorMessages.UserLockedOut, StatusCodes.Status401Unauthorized);
            }

            if (this.IsAccountNotApproved(user))
            {
                throw new UnauthorizedException(
                    string.Format(ErrorMessages.UserNotApproved, user.Email),
                    StatusCodes.Status401Unauthorized);
            }

            var identity = new GenericIdentity(user.UserName, "Basic");
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

            var roles = await userManager.GetRolesAsync(user);
            identity.AddClaims(roles.Select(x => new Claim(ClaimTypes.Role, x)));

            var email = await userManager.GetEmailAsync(user);
            identity.AddClaim(new Claim(ClaimTypes.Email, email));

            var principal = new ClaimsPrincipal(identity);
            return principal;
        }
        
        private bool IsAccountLockedOut(DesignerIdentityUser user)
        {
            return user.LockoutEnabled && user.LockoutEnd.HasValue;
        }

        private bool IsAccountNotApproved(DesignerIdentityUser user)
        {
            return !user.EmailConfirmed;
        }

        private async Task<bool> Authorize(DesignerIdentityUser user, string username, string password)
        {
            if (user == null) return false;

            var signInResult = await signInManager.CheckPasswordSignInAsync(user, password, true);
            return signInResult.Succeeded;
        }
    }
}
