using System.Web.Configuration;
using System.Web.Mvc;
using Ninject.Activation;
using WB.Infrastructure.Native;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Filters
{
    public class CustomErrorFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Startup.InitException != null)
            {
                if (ApplicationSettings.CustomErrorsMode != CustomErrorsMode.Off)
                {
                    string respondWithFile = null;
                    if (filterContext.RequestContext.HttpContext.Request.IsLocal &&
                        Startup.InitException.Subsystem == Subsystem.Database)
                    {
                        respondWithFile = "~/ErrorPages/databaseError.html";
                    }
                    else
                    {
                        respondWithFile = "~/ErrorPages/genericError.html";
                    }

                    filterContext.Result = new FilePathResult(filterContext.HttpContext.Server.MapPath(respondWithFile), "text/html");
                }
            }
        }
    }
}