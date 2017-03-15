using System.Web.Mvc;
using NLog;

namespace WB.UI.Headquarters
{
    public class NlogExceptionFilter : IExceptionFilter
    {
        private static readonly Logger Nlog = LogManager.GetCurrentClassLogger();

        public void OnException(ExceptionContext filterContext)
        {
            Nlog.Error(filterContext.Exception, filterContext.RequestContext.HttpContext.Request.RawUrl);
        }
    }
}