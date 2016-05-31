using System;
using System.Web.Mvc;

namespace WB.UI.Headquarters.Filters
{
    public class HandleUIExceptionAttribute : FilterAttribute, IExceptionFilter
    {
        public virtual void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }
            if (filterContext.Exception != null)
            {
                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.Clear();
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                filterContext.HttpContext.Response.StatusCode =
                    (int)((HttpStatusException) filterContext.Exception).StatusCode;
                filterContext.Result = new JsonResult() { Data = filterContext.Exception.Message};
            }
        }
    }
}
