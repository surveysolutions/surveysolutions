// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobalHelper.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Web.Supervisor.Models
{
    using System.Web;
    using System.Web.Mvc;

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

        #region Public Methods and Operators

        /// <summary>
        /// The generate url.
        /// </summary>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <param name="controller">
        /// The controller.
        /// </param>
        /// <param name="routes">
        /// The routes.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GenerateUrl(string action, string controller, object routes)
        {
            var url = new UrlHelper(HttpContext.Current.Request.RequestContext);

            return url.Action(action, controller, routes, HttpContext.Current.Request.Url.Scheme);
        }

        #endregion
    }
}