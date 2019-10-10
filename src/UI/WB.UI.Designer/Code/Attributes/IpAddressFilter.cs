using System;
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
    public class IpAddressFilter : IAsyncAuthorizationFilter
    {
        private readonly IIpAddressProvider ipAddressProvider;
        private readonly IAllowedAddressService allowedAddressService;
        private readonly bool onlyAllowedAddresses;
        private readonly UserManager<DesignerIdentityUser> userManager;

        public IpAddressFilter(
            bool onlyAllowedAddresses,
            IIpAddressProvider ipAddressProvider,
            IAllowedAddressService allowedAddressService, 
            UserManager<DesignerIdentityUser> userManager)
        {
            this.onlyAllowedAddresses = onlyAllowedAddresses;
            this.ipAddressProvider = ipAddressProvider;
            this.allowedAddressService = allowedAddressService;
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            BasicCredentials credentials = ParseCredentials(context);

            if (this.onlyAllowedAddresses)
            {
                var user = await this.userManager.FindByNameAsync(credentials.Username)
                           ?? await this.userManager.FindByEmailAsync(credentials.Username);

                if (user?.CanImportOnHq == false)
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


        private void ThrowForbiddenException(AuthorizationFilterContext actionContext, string errorMessage)
        {
            ThrowException(actionContext,
                StatusCodes.Status403Forbidden,
                errorMessage);

            var host = actionContext.HttpContext.Request.Host;
            actionContext.HttpContext.Response.Headers.Add("WWW-Authenticate", $"Basic realm=\"{host}\"");
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
    }
}
