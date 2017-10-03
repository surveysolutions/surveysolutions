using System.Web;
using System.Web.Http.Filters;
using Prometheus;
using StackExchange.Exceptional;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Filters
{
    public class ErrorLoggerFilter : ExceptionFilterAttribute
    {
        private static readonly Counter ExceptionsLogged = Metrics.CreateCounter(@"exceptions_raised", @"Total exceptions raised");

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            ExceptionsLogged.Inc();
            base.OnException(actionExecutedContext);

            if (actionExecutedContext.Exception is WebInterviewAccessException)
                return;

            actionExecutedContext.Exception.Log(HttpContext.Current);
        }
    }
}