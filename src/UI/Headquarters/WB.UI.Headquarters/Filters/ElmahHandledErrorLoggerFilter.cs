using System.Web;
using System.Web.Http.Filters;
using Prometheus;
using StackExchange.Exceptional;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Filters
{
    public class ElmahHandledErrorLoggerFilter : ExceptionFilterAttribute
    {
        private static readonly Counter ExceptionsLogged = Prometheus.Metrics.CreateCounter(@"exceptions_raised", @"Total exceptions raised");

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            ExceptionsLogged.Inc();
            base.OnException(actionExecutedContext);

            if (actionExecutedContext.Exception is WebInterviewAccessException)
                return;

            actionExecutedContext.Exception
                .AddLogData("Handler", "ElmahHandledErrorLoggerFilter").Log(HttpContext.Current);
        }
    }
}