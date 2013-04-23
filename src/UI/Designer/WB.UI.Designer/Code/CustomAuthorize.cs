// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomAuthorize.cs" company="">
//   
// </copyright>
// <summary>
//   The custom authorize.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.Practices.ServiceLocation;
using NinjectAdapter;
using WB.UI.Designer.Code;

namespace WB.UI.Designer
{
    using System.Web.Mvc;
    using System.Web.Routing;

    using WB.UI.Designer.Controllers;
    using WB.UI.Designer.Models;

    /// <summary>
    /// The custom authorize.
    /// </summary>
    public class CustomAuthorize : AuthorizeAttribute
    {
        #warning remove this shit
        private static IUserHelper UserHelperInstance
        {
            get { return UserHelper.Instance; }
        }

        #region Methods

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var isInvalidUser = false;
            var user = UserHelperInstance.CurrentUser;

            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                isInvalidUser = user == null || user.IsLockedOut
                                      || !user.IsApproved;

                if (user != null)
                {
                    var baseController = filterContext.Controller as BaseController;
                    if (baseController != null)
                    {
                        if (!user.IsApproved)
                        {
                            baseController.Attention(
                                string.Format(
                                    "Please, confirm your account first. We've sent a confirmation link to {0}. Didn't get it? <a href='{1}'>Request another one.</a>",
                                    user.Email,
                                    GlobalHelper.GenerateUrl(
                                        "ResendConfirmation", "Account", new { id = user.UserName })));
                        }
                        else if (user.IsLockedOut)
                        {
                            baseController.Attention(
                                "Your account is blocked. Contact the administrator to unblock your account");
                        }
                    }
                }
            }

            if (isInvalidUser)
            {
                UserHelperInstance.Logout();
                filterContext.Result =
                    new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Account", action = "Login" }));
            }
            else
            {
                base.OnAuthorization(filterContext);
            }
        }

        #endregion
    }
}