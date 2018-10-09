using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Transactions;
using WB.UI.Designer.Controllers;

namespace WB.UI.Designer
{

    public class CustomAuthorizeFilter : IAuthorizationFilter
    {
        private readonly IMembershipUserService userService;

        private IPlainTransactionManagerProvider TransactionManagerProvider => ServiceLocator.Current.GetInstance<IPlainTransactionManagerProvider>();

        public CustomAuthorizeFilter(IMembershipUserService userService)
        {
            this.userService = userService;
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            bool skipAuthorization = filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true)
                                  || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true);

            if (skipAuthorization)
            {
                return;
            }

            bool isInvalidUser = false;

            this.TransactionManagerProvider.GetPlainTransactionManager().BeginTransaction();
            try
            {
                MembershipUser user = this.userService.WebUser.MembershipUser;

                if (filterContext.HttpContext.User.Identity.IsAuthenticated)
                {
                    isInvalidUser = user == null || user.IsLockedOut || !user.IsApproved;

                    if (user != null)
                    {
                        var baseController = filterContext.Controller as BaseController;
                        if (baseController != null)
                        {
                            if (!user.IsApproved)
                            {
                                baseController.Error(
                                    string.Format(
                                        "Please, confirm your account first. We've sent a confirmation link to {0}. Didn't get it? <a href='{1}'>Request another one.</a>",
                                        user.Email,
                                        GlobalHelper.GenerateUrl(
                                            "ResendConfirmation", "Account", new {id = user.UserName})));
                            }
                            else if (user.IsLockedOut)
                            {
                                baseController.Error(
                                    "Your account is blocked. Contact the administrator to unblock your account");
                            }
                        }
                    }

                    if (!isInvalidUser && filterContext.Controller is AccountController &&
                        filterContext.ActionDescriptor.ActionName.NotIn(new[] {"logoff", "manage", "changepassword", "findbyemail"}))
                    {
                        filterContext.Result =
                            new RedirectToRouteResult(
                                new RouteValueDictionary(new {controller = "Questionnaire", action = "Index"}));
                    }
                }

                if (isInvalidUser)
                {
                    this.userService.Logout();
                    filterContext.Result =
                        new RedirectToRouteResult(
                            new RouteValueDictionary(new {controller = "Account", action = "Login"}));
                }
            }
            finally
            {
                this.TransactionManagerProvider.GetPlainTransactionManager().RollbackTransaction();
            }
        }
    }
}
