namespace WB.UI.Designer
{
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.Security;

    using WB.UI.Designer.Controllers;

    /// <summary>
    ///     The custom authorize.
    /// </summary>
    public class CustomAuthorizeFilter : IAuthorizationFilter
    {
        #region Fields

        /// <summary>
        /// The _user service.
        /// </summary>
        private readonly IUserHelper userService;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAuthorizeFilter"/> class.
        /// </summary>
        /// <param name="userService">
        /// The user service.
        /// </param>
        public CustomAuthorizeFilter(IUserHelper userService)
        {
            this.userService = userService;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The on authorization.
        /// </summary>
        /// <param name="filterContext">
        /// The filter context.
        /// </param>
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            bool isInvalidUser = false;
            MembershipUser user = this.userService.CurrentUser;

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

                if (!isInvalidUser && filterContext.Controller is AccountController && filterContext.ActionDescriptor.ActionName.ToLower() != "logoff")
                {
                    filterContext.Result =
                        new RedirectToRouteResult(
                            new RouteValueDictionary(new { controller = "Questionnaire", action = "Index" }));
                }
            }

            if (isInvalidUser)
            {
                this.userService.Logout();
                filterContext.Result =
                    new RedirectToRouteResult(
                        new RouteValueDictionary(new { controller = "Account", action = "Login" }));
            }
        }

        #endregion
    }
}