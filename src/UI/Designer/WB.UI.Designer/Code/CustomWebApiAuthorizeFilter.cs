using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Security;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.Transactions;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Code
{
    public class CustomWebApiAuthorizeFilter : AuthorizationFilterAttribute
    {
        private IPlainTransactionManagerProvider TransactionManagerProvider => ServiceLocator.Current.GetInstance<IPlainTransactionManagerProvider>();

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

            this.TransactionManagerProvider.GetPlainTransactionManager().BeginTransaction();
            try
            {

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
            finally
            {
                this.TransactionManagerProvider.GetPlainTransactionManager().RollbackTransaction();
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