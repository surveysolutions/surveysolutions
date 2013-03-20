// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomAuthorize.cs" company="">
//   
// </copyright>
// <summary>
//   The custom authorize.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

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
        #region Methods

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var isInvalidUser = false;

            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                isInvalidUser = UserHelper.CurrentUser == null || UserHelper.CurrentUser.IsLockedOut
                                      || !UserHelper.CurrentUser.IsApproved;

                if (UserHelper.CurrentUser != null)
                {
                    var baseController = filterContext.Controller as AlertController;
                    if (baseController != null)
                    {
                        if (!UserHelper.CurrentUser.IsApproved)
                        {
                            baseController.Error("Please, confirm your account first. Check your emails.");
                        }
                        else if (UserHelper.CurrentUser.IsLockedOut)
                        {
                            baseController.Error(
                                "Your account is blocked. Contact the administrator for unblocking account");
                        }
                    }
                }
            }

            if (isInvalidUser)
            {
                UserHelper.Logout();
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