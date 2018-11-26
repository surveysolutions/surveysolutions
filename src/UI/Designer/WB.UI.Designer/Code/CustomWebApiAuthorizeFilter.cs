using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Security;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.UI.Designer.Code
{
    public class CustomWebApiAuthorizeFilter : AuthorizationFilterAttribute
    {
        private IMembershipUserService UserService => ServiceLocator.Current.GetInstance<IMembershipUserService>();

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            bool skipAuthorization = actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Count > 0
                      || actionContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Count > 0;

            if (skipAuthorization)
            {
                return;
            }

            bool isInvalidUser = false;

            MembershipUser user = this.UserService.WebUser.MembershipUser;

            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                isInvalidUser = user == null || user.IsLockedOut || !user.IsApproved;
            }

            if (isInvalidUser)
            {
                this.UserService.Logout();
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
        }
    }
}
