using System.Web;
using System.Web.Http.Filters;
using StackExchange.Exceptional;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Monitoring;

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
