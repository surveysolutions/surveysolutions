using System.Web;
using System.Web.Http.Filters;
using StackExchange.Exceptional;
using WB.Infrastructure.Native.Monitoring;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Filters
{
    public class ErrorLoggerFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            CommonMetrics.ExceptionsLogged.Inc();
            base.OnException(actionExecutedContext);

            if (actionExecutedContext.Exception is InterviewAccessException)
                return;

            actionExecutedContext.Exception.Log(HttpContext.Current);
        }
    }
}