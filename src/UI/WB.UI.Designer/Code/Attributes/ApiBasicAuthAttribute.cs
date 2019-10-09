using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Code.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ApiBasicAuthAttribute : TypeFilterAttribute
    {
        public ApiBasicAuthAttribute(bool onlyAllowedAddresses = false) : base(typeof(ApiBasicAuthFilter))
        {
            Arguments = new object[] { onlyAllowedAddresses };
        }
    }

    public class ApiBasicAuthFilter : IAsyncAuthorizationFilter
    {
        private readonly IIpAddressProvider ipAddressProvider;
        private readonly IAllowedAddressService allowedAddressService;
        private readonly UserManager<DesignerIdentityUser> userManager;
        private readonly SignInManager<DesignerIdentityUser> signInManager;
        private readonly bool onlyAllowedAddresses;

        public ApiBasicAuthFilter(
            bool onlyAllowedAddresses,
            IIpAddressProvider ipAddressProvider,
            IAllowedAddressService allowedAddressService,
            UserManager<DesignerIdentityUser> userManager,
            SignInManager<DesignerIdentityUser> signInManager)
        {
            this.onlyAllowedAddresses = onlyAllowedAddresses;
            this.ipAddressProvider = ipAddressProvider;
            this.allowedAddressService = allowedAddressService;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var credentials = ParseCredentials(context);
            if (credentials == null)
            {
                this.ThrowUnauthorizedException(context, ErrorMessages.User_Not_authorized);
                return;
            }

            var user = await this.userManager.FindByNameAsync(credentials.Username)
                    ?? await this.userManager.FindByEmailAsync(credentials.Username);

            if (!await this.Authorize(user, credentials.Username, credentials.Password))
            {
                this.ThrowUnauthorizedException(context, ErrorMessages.User_Not_authorized);
                return;
            }

            var identity = new GenericIdentity(user.UserName, "Basic");
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

            var roles = await userManager.GetRolesAsync(user);
            identity.AddClaims(roles.Select(x => new Claim(ClaimTypes.Role, x)));

            var email = await userManager.GetEmailAsync(user);
            identity.AddClaim(new Claim(ClaimTypes.Email, email));

            var principal = new ClaimsPrincipal(identity);

            context.HttpContext.User = principal;

            if (IsAccountLockedOut(user))
            {
                this.ThrowLockedOutException(context);
                return;
            }

            if (this.IsAccountNotApproved(user))
            {
                this.ThrowNotApprovedException(context, user.Email);
                return;
            }

            if (this.onlyAllowedAddresses)
            {
                if (!user.CanImportOnHq)
                {
                    var clientIpAddress = ipAddressProvider.GetClientIpAddress();
                    if (!this.allowedAddressService.IsAllowedAddress(clientIpAddress))
                    {
                        this.ThrowForbiddenException(context, string.Format(ErrorMessages.UserNeedToContactSupportFormat, clientIpAddress));
                    }
                }
            }
        }

        private static BasicCredentials ParseCredentials(AuthorizationFilterContext actionContext)
        {
            try
            {
                string authHeader = actionContext.HttpContext.Request.Headers["Authorization"];
                if (authHeader != null && authHeader.StartsWith("Basic "))
                {
                    var credentials = Encoding.ASCII.GetString(Convert.FromBase64String(authHeader.Substring(6)));
                    int splitOn = credentials.IndexOf(':');

                    return new BasicCredentials
                    {
                        Username = credentials.Substring(0, splitOn),
                        Password = credentials.Substring(splitOn + 1)
                    };
                }
            }
            catch { }

            return null;
        }

        internal class BasicCredentials
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        private void ThrowUnauthorizedException(AuthorizationFilterContext actionContext, string errorMessage)
        {
            ThrowException(actionContext,
               StatusCodes.Status401Unauthorized,
               errorMessage);

            var host = actionContext.HttpContext.Request.Host;            
            actionContext.HttpContext.Response.Headers.Add("WWW-Authenticate", $"Basic realm=\"{host}\"");
        }

        private void ThrowForbiddenException(AuthorizationFilterContext actionContext, string errorMessage)
        {
            ThrowException(actionContext,
                StatusCodes.Status403Forbidden,
                errorMessage);

            var host = actionContext.HttpContext.Request.Host;
            actionContext.HttpContext.Response.Headers.Add("WWW-Authenticate", $"Basic realm=\"{host}\"");
        }

        private void ThrowLockedOutException(AuthorizationFilterContext actionContext)
        {
            ThrowException(actionContext,
                StatusCodes.Status401Unauthorized,
                ErrorMessages.UserLockedOut);
        }

        private void ThrowNotApprovedException(AuthorizationFilterContext actionContext, string email)
        {
            ThrowException(actionContext,
                StatusCodes.Status401Unauthorized,
                string.Format(ErrorMessages.UserNotApproved, email));
        }

        private void ThrowException(AuthorizationFilterContext actionContext, int statusCode, string message)
        {
            actionContext.Result = new ContentResult()
            {
                StatusCode = statusCode,
                Content = message
            };

            var feature = actionContext.HttpContext.Response.HttpContext?.Features?.Get<IHttpResponseFeature>();
            if (feature != null)
                feature.ReasonPhrase = message;
        }

        private async Task<bool> Authorize(DesignerIdentityUser user, string username, string password)
        {
            if (user == null) return false;

            var signInResult = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
            return signInResult.Succeeded;
        }

        private bool IsAccountLockedOut(DesignerIdentityUser user)
        {
            return user.LockoutEnabled && user.LockoutEnd.HasValue;
        }

        private bool IsAccountNotApproved(DesignerIdentityUser user)
        {
            return !user.EmailConfirmed;
        }
    }
}
