using System.Web;
using System.Web.Mvc;

namespace WB.UI.Headquarters.Models
{
    /// <summary>
    ///     The global helper.
    /// </summary>
    public static class GlobalHelper
    {
        #region Constants

        /// <summary>
        /// The empty string.
        /// </summary>
        public const string EmptyString = "--//--";

        /// <summary>
        ///     The grid page items count.
        /// </summary>
        public const int GridPageItemsCount = 10;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the current action.
        /// </summary>
        public static string CurrentAction
        {
            get
            {
                return (string)HttpContext.Current.Request.RequestContext.RouteData.Values["action"];
            }
        }

        /// <summary>
        ///     Gets the current controller.
        /// </summary>
        public static string CurrentController
        {
            get
            {
                return (string)HttpContext.Current.Request.RequestContext.RouteData.Values["controller"];
            }
        }

        /// <summary>
        /// Gets the previous page.
        /// </summary>
        public static  string PreviousPage
        {
            get
            {
                var urlRequest = HttpContext.Current.Request.UrlReferrer;
                return urlRequest == null ? string.Empty : urlRequest.ToString();
            }
        }

        #endregion

        public static string GenerateUrl(string action, string controller, object routes)
        {
            var url = new UrlHelper(HttpContext.Current.Request.RequestContext);

            return url.Action(action, controller, routes, HttpContext.Current.Request.Url.Scheme);
        }
    }
}