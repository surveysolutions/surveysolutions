using System.Diagnostics;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace WB.UI.Headquarters.API.Filters
{
    /// <summary>
    /// Reports amount of seconds action execution took. Used by interviewer to more accuratly track upload/download speed
    /// </summary>
    public class PerformanceTraceFilter : ActionFilterAttribute
    {
        private const string TraceId = @"__performance_trace_filter_";

        private Stopwatch Stopwatch
        {
            get { return HttpContext.Current?.GetOwinContext()?.Get<Stopwatch>(TraceId); }
            set
            {
                HttpContext.Current?.GetOwinContext()?.Set(TraceId, value);
            }
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            this.Stopwatch = new Stopwatch();
            this.Stopwatch?.Start();
            base.OnActionExecuting(actionContext);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var sw = this.Stopwatch;
            sw?.Stop();

            base.OnActionExecuted(actionExecutedContext);

            if (sw != null)
            {
                actionExecutedContext.Response?.Headers.Add(@"Server-Timing", @"action=" + sw.Elapsed.TotalSeconds);
            }
        }
    }
}