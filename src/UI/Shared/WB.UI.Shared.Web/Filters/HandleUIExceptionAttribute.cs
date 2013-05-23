using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using WB.UI.Shared.Web.Exceptions;

namespace WB.UI.Shared.Web.Filters
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
