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

    /// <summary>
    /// The global helper.
    /// </summary>
    public static class GlobalHelper
    {
        #region Constants

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

        #endregion
    }
}