using WB.Core.GenericSubdomains.Portable.Services;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.UI.Designer.Exceptions
{
    public class CustomHandleErrorAttribute : FilterAttribute, IExceptionFilter
    {
        public CustomHandleErrorAttribute()
        {
            this.Order = 1;
        }

        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled || !filterContext.HttpContext.IsCustomErrorEnabled)
            {
                return;
            }

            if (new HttpException(null, filterContext.Exception).GetHttpCode() != 500)
            {
                return;
            }

            // if the request is AJAX return JSON else view.
            if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                filterContext.Result = new JsonResult
                                           {
                                               JsonRequestBehavior = JsonRequestBehavior.AllowGet, 
                                               Data =
                                                   new
                                                       {
                                                           error = true, 
                                                           message = filterContext.Exception.Message
                                                       }
                                           };
            }
            else
            {
                filterContext.Result =
                    new RedirectToRouteResult(
                        new RouteValueDictionary { { "controller", "Error" }, { "action", "Index" } });
            }

            // log the error 
            var logger = ServiceLocator.Current.GetInstance<ILoggerProvider>().GetFor<CustomHandleErrorAttribute>();
            logger.Error(filterContext.Exception.Message, filterContext.Exception);

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = 500;

            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }
    }
}