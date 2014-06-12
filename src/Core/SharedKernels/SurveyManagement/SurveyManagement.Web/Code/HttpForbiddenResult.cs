using System.Net;
using System.Web.Mvc;

namespace WB.UI.Headquarters.Code
{
    public class HttpForbiddenResult : HttpStatusCodeResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            base.ExecuteResult(context);

            context.Controller.ViewData["Message"] = base.StatusDescription;
            // creates the ViewResult adding ViewData and TempData parameters
            var result = new ViewResult
            {
                ViewName = "AccessDenied",
                ViewData = context.Controller.ViewData,
                TempData = context.Controller.TempData
            };

            result.ExecuteResult(context);
        }

        public HttpForbiddenResult(string message)
            : base(HttpStatusCode.Forbidden, message)
        {
        }
    }
}