using System;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Code.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ApiBasicAuthAttribute: TypeFilterAttribute
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
                this.ThrowUnathorizedException(context, ErrorMessages.User_Not_authorized);
                return;
            }

            if (!await this.Authorize(credentials.Username, credentials.Password))
            {
                this.ThrowUnathorizedException(context, ErrorMessages.User_Not_authorized);
                return;
            }

            var account = await this.userManager.FindByNameAsync(credentials.Username)
                       ?? await this.userManager.FindByEmailAsync(credentials.Username);
            var identity = new GenericIdentity(account.UserName, "Basic");
            var principal = new GenericPrincipal(identity, null);

            Thread.CurrentPrincipal = principal;
            if (context.HttpContext != null)
            {
                context.HttpContext.User = principal;
            }

            if (IsAccountLockedOut(account))
            {
                this.ThrowLockedOutException(context);
                return;
            }

            if (this.IsAccountNotApproved(account))
            {
                this.ThrowNotApprovedException(context, account.Email);
                return;
            }

            if (this.onlyAllowedAddresses)
            {
                if (!account.CanImportOnHq)
                {
                    var clientIpAddress = ipAddressProvider.GetClientIpAddress();
                    if (!this.allowedAddressService.IsAllowedAddress(clientIpAddress))
                    {
                        this.ThrowForbiddenException(context, string.Format(ErrorMessages.UserNeedToContactSupportFormat, clientIpAddress));
                        return;
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
            catch {}

            return null;
        }

        internal class BasicCredentials
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        private void ThrowUnathorizedException(AuthorizationFilterContext actionContext, string errorMessage)
        {
            var host = actionContext.HttpContext.Request.Host;
            actionContext.Result = new ContentResult()
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Content = errorMessage
            };
            actionContext.HttpContext.Response.Headers.Add("WWW-Authenticate", $"Basic realm=\"{host}\"");
        }

        private void ThrowForbiddenException(AuthorizationFilterContext actionContext, string errorMessage)
        {
            var host = actionContext.HttpContext.Request.Host;
            actionContext.Result = new ContentResult()
            {
                StatusCode = StatusCodes.Status403Forbidden,
                Content = errorMessage
            };
            actionContext.HttpContext.Response.Headers.Add("WWW-Authenticate", $"Basic realm=\"{host}\"");
        }

        private void ThrowLockedOutException(AuthorizationFilterContext actionContext)
        {
            actionContext.Result = new ContentResult()
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Content = ErrorMessages.UserLockedOut
            };
        }

        private void ThrowNotApprovedException(AuthorizationFilterContext actionContext, string email)
        {
            actionContext.Result = new ContentResult()
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Content = string.Format(ErrorMessages.UserNotApproved, email)
            };
        }

        private async Task<bool> Authorize(string username, string password)
        {
            var user = await this.userManager.FindByNameAsync(username)
                    ?? await this.userManager.FindByEmailAsync(username);
            if (user == null)
                return false;

            var signInResult = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
            return signInResult.Succeeded;
        }

        private bool IsAccountLockedOut(DesignerIdentityUser user)
        {
            return user.LockoutEnabled;
        }

        private bool IsAccountNotApproved(DesignerIdentityUser user)
        {
            return !user.EmailConfirmed;
        }
    }
}
