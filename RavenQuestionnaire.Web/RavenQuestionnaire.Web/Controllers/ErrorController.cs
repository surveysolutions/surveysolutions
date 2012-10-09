// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorController.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   The error controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Web.Controllers
{
    using System.Web.Mvc;

    /// <summary>
    /// The error controller.
    /// </summary>
    public class ErrorController : Controller
    {
        #region Public Methods and Operators

        /// <summary>
        /// The general.
        /// </summary>
        /// <returns>
        /// </returns>
        public ActionResult General()
        {
            return this.View();
        }

        /// <summary>
        /// The http 403.
        /// </summary>
        /// <returns>
        /// </returns>
        public ActionResult Http403()
        {
            return this.View();
        }

        #endregion
    }
}