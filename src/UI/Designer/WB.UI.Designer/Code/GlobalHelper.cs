// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobalHelper.cs" company="">
//   
// </copyright>
// <summary>
//   The global helper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Designer
{
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    /// <summary>
    /// The global helper.
    /// </summary>
    public static class GlobalHelper
    {
        #region Constants

        public const string EmptyString = "--//--";

        /// <summary>
        /// The grid page items count.
        /// </summary>
        public const int GridPageItemsCount = 10;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the current action.
        /// </summary>
        public static string CurrentAction
        {
            get
            {
                return (string)HttpContext.Current.Request.RequestContext.RouteData.Values["action"];
            }
        }

        /// <summary>
        /// Gets the current controller.
        /// </summary>
        public static string CurrentController
        {
            get
            {
                return (string)HttpContext.Current.Request.RequestContext.RouteData.Values["controller"];
            }
        }

        public static string GenerateUrl(string action, string controller, object routes)
        {
            var url = new UrlHelper(HttpContext.Current.Request.RequestContext);

            return url.Action(action, controller, routes);
        }

        #endregion
    }
}