using System.Web.Http.Filters;
using Elmah;
using Prometheus;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters
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

            ErrorSignal.FromCurrentContext().Raise(actionExecutedContext.Exception);

        }
    }
}