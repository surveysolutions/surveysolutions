using System;
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
            IUserConfirmation<HqUser> confirmation) : base(userManager, contextAccessor, claimsFactory, optionsAccessor,
            logger, schemes, confirmation)
        {
        }

        protected override Task<bool> IsLockedOut(HqUser user)
        {
            return user.IsArchivedOrLocked 
                ? Task.FromResult(true) 
                : base.IsLockedOut(user);
        }

        public override async Task<SignInResult> PasswordSignInAsync(string userName, string password,
            bool isPersistent, bool lockoutOnFailure)
        {
            var user = await UserManager.FindByNameAsync(userName);
            if (user == null)
            {
                return SignInResult.Failed;
            }

            var result = await PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);

            if (result.Succeeded)
            {
                user.LastLoginDate = DateTime.UtcNow;
                await this.UserManager.UpdateAsync(user);
            }

            return result;
        }

        public override async Task<SignInResult> TwoFactorAuthenticatorSignInAsync(string code, bool isPersistent, 
            bool rememberClient)
        {
            var result = await base.TwoFactorAuthenticatorSignInAsync(code, isPersistent, rememberClient);
            if (result.Succeeded)
                await UpdateLastLoginForTwoFactorAuthenticationUserAsync();

            return result;
        }

        public override async Task<SignInResult> TwoFactorRecoveryCodeSignInAsync(string recoveryCode)
        {
            var result = await base.TwoFactorRecoveryCodeSignInAsync(recoveryCode);
            if (result.Succeeded)
                await UpdateLastLoginForTwoFactorAuthenticationUserAsync();

            return result;
        }

        public override async Task<SignInResult> TwoFactorSignInAsync(string provider, string code, bool isPersistent,
            bool rememberClient)
        {
            var result = await base.TwoFactorSignInAsync(provider, code, isPersistent, rememberClient);
            if (result.Succeeded)
                await UpdateLastLoginForTwoFactorAuthenticationUserAsync();

            return result;
        }

        private async Task UpdateLastLoginForTwoFactorAuthenticationUserAsync()
        {
            var user = await GetTwoFactorAuthenticationUserAsync();
            if (user != null)
            {
                user.LastLoginDate = DateTime.UtcNow;
                await UserManager.UpdateAsync(user);
            }
        }
    }
}
