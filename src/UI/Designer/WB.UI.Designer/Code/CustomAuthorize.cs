// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomAuthorize.cs" company="">
//   
// </copyright>
// <summary>
//   The custom authorize.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using WB.UI.Designer.Code;

namespace WB.UI.Designer
{
    using System.Web.Mvc;
    using System.Web.Routing;


    using WB.UI.Designer.Controllers;


    ///marker attribute
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
    }

    /// <summary>
    /// The custom authorize.
    /// </summary>
    public class CustomAuthorizeFilter : IAuthorizationFilter
    {
        private readonly IUserHelper _userService;

        public CustomAuthorizeFilter(IUserHelper userService)
        {
            _userService = userService;
        }

        #region Methods

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var isInvalidUser = false;
            var user = this._userService.CurrentUser;

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
                this._userService.Logout();
                filterContext.Result =
                    new RedirectToRouteResult(
                        new RouteValueDictionary(new { controller = "Account", action = "Login" }));
            }
        }

        #endregion
    }
}