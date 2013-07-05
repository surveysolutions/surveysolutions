namespace Web.CAPI.Controllers
{
    using System.Net;
    using System.Web.Mvc;

    /// <summary>
    /// The error controller.
    /// </summary>
    [HandleError]
    public class ErrorController : Controller
    {
        /// <summary>
        /// The internal server error.
        /// </summary>
        /// <returns>
        /// The System.Web.Mvc.ActionResult.
        /// </returns>
        public ActionResult InternalServerError()
        {
            Response.TrySkipIisCustomErrors = true;
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return View("InternalServerError");
        }

    }
}
