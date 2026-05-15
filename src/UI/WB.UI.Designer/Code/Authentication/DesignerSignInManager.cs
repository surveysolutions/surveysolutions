using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Designer.MembershipProvider;

namespace WB.UI.Designer.Code.Authentication
{
    public class DesignerSignInManager : SignInManager<DesignerIdentityUser>
    {
        public DesignerSignInManager(
            UserManager<DesignerIdentityUser> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<DesignerIdentityUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<DesignerIdentityUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<DesignerIdentityUser> confirmation)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
        }

        public override async Task<SignInResult> TwoFactorAuthenticatorSignInAsync(string code, bool isPersistent,
            bool rememberClient)
        {
            var result = await base.TwoFactorAuthenticatorSignInAsync(code, isPersistent, rememberClient);
            if (result.Succeeded)
                await this.UpdateLastLoginForTwoFactorAuthenticationUserAsync();

            return result;
        }

        public override async Task<SignInResult> TwoFactorRecoveryCodeSignInAsync(string recoveryCode)
        {
            var result = await base.TwoFactorRecoveryCodeSignInAsync(recoveryCode);
            if (result.Succeeded)
                await this.UpdateLastLoginForTwoFactorAuthenticationUserAsync();

            return result;
        }

        public override async Task<SignInResult> TwoFactorSignInAsync(string provider, string code, bool isPersistent,
            bool rememberClient)
        {
            var result = await base.TwoFactorSignInAsync(provider, code, isPersistent, rememberClient);
            if (result.Succeeded)
                await this.UpdateLastLoginForTwoFactorAuthenticationUserAsync();

            return result;
        }

        private async Task UpdateLastLoginForTwoFactorAuthenticationUserAsync()
        {
            var user = await this.GetTwoFactorAuthenticationUserAsync();
            if (user != null)
            {
                user.LastLoginAtUtc = DateTime.UtcNow;
                await this.UserManager.UpdateAsync(user);
            }
        }
    }
}
