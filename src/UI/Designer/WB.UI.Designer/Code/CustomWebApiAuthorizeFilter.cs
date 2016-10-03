using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Routing;
using System.Web.Security;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.Transactions;
using WB.UI.Designer.Controllers;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Code
{
    public class CustomWebApiAuthorizeFilter : AuthorizationFilterAttribute
    {
        private ITransactionManagerProvider TransactionManagerProvider => ServiceLocator.Current.GetInstance<ITransactionManagerProvider>();

        private IReadSideStatusService readSideStatusService => ServiceLocator.Current.GetInstance<IReadSideStatusService>();

        private IMembershipUserService userService => ServiceLocator.Current.GetInstance<IMembershipUserService>();


        public override void OnAuthorization(HttpActionContext actionContext)
        {
            bool skipAuthorization = actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Count > 0
                      || actionContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Count > 0;

            if (skipAuthorization)
            {
                return;
            }

            if (readSideStatusService.AreViewsBeingRebuiltNow())
            {
                actionContext.Response = GetResponseWithErrorMessage(HttpStatusCode.Forbidden, "The application is now rebooting and this may take up to 10 minutes or more. Sorry for the inconvenience.");
                return;
            }

            bool isInvalidUser = false;

            this.TransactionManagerProvider.GetTransactionManager().BeginQueryTransaction();
            try
            {

                MembershipUser user = this.userService.WebUser.MembershipUser;

                if (HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    isInvalidUser = user == null || user.IsLockedOut || !user.IsApproved;
                }

                if (isInvalidUser)
                {
                    this.userService.Logout();
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }
            }
            finally
            {
                this.TransactionManagerProvider.GetTransactionManager().RollbackQueryTransaction();
            }
        }

        private static HttpResponseMessage GetResponseWithErrorMessage(HttpStatusCode httpStatusCode, string message = null)
        {
            if (!string.IsNullOrEmpty(message))
                message = $"{{\"Message\":\"{message}\"}}";

            return new HttpResponseMessage(httpStatusCode)
            {
                Content = new StringContent(message, Encoding.UTF8, "application/json")
            };
        }
    }
}