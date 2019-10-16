using System;
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
    public class IpAddressFilter : IAsyncActionFilter
    {
        private readonly IIpAddressProvider ipAddressProvider;
        private readonly IAllowedAddressService allowedAddressService;
        private readonly UserManager<DesignerIdentityUser> userManager;

        public IpAddressFilter(IIpAddressProvider ipAddressProvider,
            IAllowedAddressService allowedAddressService, 
            UserManager<DesignerIdentityUser> userManager)
        {
            this.ipAddressProvider = ipAddressProvider;
            this.allowedAddressService = allowedAddressService;
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = await this.userManager.GetUserAsync(context.HttpContext.User);

            if (user?.CanImportOnHq == false)
            {
                var clientIpAddress = ipAddressProvider.GetClientIpAddress();
                if (!this.allowedAddressService.IsAllowedAddress(clientIpAddress))
                {
                    this.ReturnForbidden(context,
                        string.Format(ErrorMessages.UserNeedToContactSupportFormat, clientIpAddress));
                    return;
                }
            }

            await next();
        }

        private void ReturnForbidden(ActionExecutingContext actionContext, string errorMessage)
        {
            actionContext.Result = new ContentResult
            {
                StatusCode = StatusCodes.Status403Forbidden,
                Content = errorMessage
            };

            var feature = actionContext.HttpContext.Response.HttpContext?.Features?.Get<IHttpResponseFeature>();
            if (feature != null)
                feature.ReasonPhrase = errorMessage;
        }
    }
}
