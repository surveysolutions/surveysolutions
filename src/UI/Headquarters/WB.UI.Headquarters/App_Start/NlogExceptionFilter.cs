using System.Web.Mvc;
using NLog;
using WB.Infrastructure.Native.Monitoring;

namespace WB.UI.Headquarters
{
    public class NlogExceptionFilter : IExceptionFilter
    {
        private static readonly Logger Nlog = LogManager.GetCurrentClassLogger();

        public void OnException(ExceptionContext filterContext)
        {
            CommonMetrics.ExceptionsLogged.Inc();
            Nlog.Error(filterContext.Exception, filterContext.RequestContext.HttpContext.Request.RawUrl);
        }
    }
}