using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using System.Threading;
using System.Web;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public class HqSignInManager : SignInManager<HqUser, Guid>
    {
        public HqSignInManager(HqUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public static HqSignInManager Create(IdentityFactoryOptions<HqSignInManager> options, IOwinContext context)
            => new HqSignInManager(context.GetUserManager<HqUserManager>(), context.Authentication);

        public async Task<SignInStatus> SignInAsync(string userName, string password, bool isPersistent = false)
        {
            var user = await this.UserManager.FindByNameAsync(userName);
            if (user == null || !await this.UserManager.CheckPasswordAsync(user, password))
                return SignInStatus.Failure;

            if (user.IsInRole(UserRoles.Interviewer) || user.IsLockedByHeadquaters || user.IsLockedBySupervisor || user.IsArchived)
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

        public async Task<IdentityResult> SignInWithAuthTokenAsync(string authorizationHeader, bool treatPasswordAsPlain, params UserRoles[] allowedRoles)
        {
            BasicCredentials basicCredentials = this.ExtractFromAuthorizationHeader(ApiAuthenticationScheme.Basic, authorizationHeader)
                                              ?? this.ExtractFromAuthorizationHeader(ApiAuthenticationScheme.AuthToken, authorizationHeader);

            if (basicCredentials == null)
            {
                return IdentityResult.Failed();
            }

            var userInfo = await UserManager.FindByNameAsync(basicCredentials.Username);

            if (userInfo == null || userInfo.IsArchived)
            {
                return IdentityResult.Failed();
            }

            switch (basicCredentials.Scheme)
            {
                case ApiAuthenticationScheme.Basic:
                    if (treatPasswordAsPlain && !UserManager.CheckPassword(userInfo, basicCredentials.Password)
                        || !treatPasswordAsPlain && !CheckHashedPassword(userInfo, basicCredentials))
                    {
                        return IdentityResult.Failed();
                    }
                    break;
                case ApiAuthenticationScheme.AuthToken:
                    if (!await (UserManager as HqUserManager).ValidateApiAuthTokenAsync(userInfo.Id, basicCredentials.Password))
                    {
                        return IdentityResult.Failed();
                    }
                    break;
            }
            
            if (userInfo.IsLockedBySupervisor || userInfo.IsLockedByHeadquaters)
            {
                return IdentityResult.Failed("Locked");
            }
            
            if (!allowedRoles.Contains(userInfo.Roles.First().Role))
            {
                return IdentityResult.Failed("Role");
            }

            var identity = await this.UserManager.CreateIdentityAsync(userInfo, basicCredentials.Scheme.ToString());
            
            var principal = new ClaimsPrincipal(identity);
            
            Thread.CurrentPrincipal = principal;
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }

            return IdentityResult.Success;
        }

        private bool CheckHashedPassword(HqUser userInfo, BasicCredentials basicCredentials)
        {
            var compatibilityProvider = ServiceLocator.Current.GetInstance<IHashCompatibilityProvider>();

            if (compatibilityProvider.IsInSha1CompatibilityMode() && userInfo.IsInRole(UserRoles.Interviewer))
            {
                return userInfo.PasswordHashSha1 == basicCredentials.Password;
            }

            return userInfo.PasswordHash == basicCredentials.Password;
        }

        private BasicCredentials ExtractFromAuthorizationHeader(ApiAuthenticationScheme scheme, string authorizationHeader)
        {
            try
            {
                string schemeString = scheme.ToString();
                string header = authorizationHeader;

                if (header == null) return null;
                if (!header.StartsWith(schemeString, StringComparison.OrdinalIgnoreCase)) return null;

                string credentials = Encoding.ASCII.GetString(Convert.FromBase64String(header.Substring(schemeString.Length + 1)));
                int splitOn = credentials.IndexOf(':');

                return new BasicCredentials
                {
                    Username = credentials.Substring(0, splitOn),
                    Password = credentials.Substring(splitOn + 1),
                    Scheme = scheme
                };
            }
            catch
            {
            }

            return null;
        }

        internal class BasicCredentials
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public ApiAuthenticationScheme Scheme { get; set; }
        }
    }
}