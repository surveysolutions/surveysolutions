using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Prometheus;

namespace WB.UI.Headquarters.Metrics
{
    public class HeadquartersHttpMetricsMiddleware
    {
        private readonly RequestDelegate next;
        private readonly Gauge httpInProgress = Prometheus.Metrics.CreateGauge("http_requests_in_progress", "Number or requests in progress", "system");
        private readonly Histogram httpRequestsDuration = Prometheus.Metrics.CreateHistogram("http_requests_duration_seconds", "Duration of http requests per tracking system", "system");
        
        public HeadquartersHttpMetricsMiddleware(RequestDelegate next)
        {
            this.next = next;

        }

        public async Task Invoke(HttpContext context)
        {
            string[] subsystem = new string[1];
            var path = context.Request.Path;

            // will not track request to diagnostics endpoints
            if (path.StartsWithSegments("/metrics") || path.StartsWithSegments("/.hc") ||
                path.StartsWithSegments("/.version"))
            {
                await next(context);
                return;
            }

            // web interview commands requests
            if (path.StartsWithSegments("/api/webinterview/commands")) subsystem[0] = "webcommand";

            // interviewer app synchronization requests
            else if (path.StartsWithSegments("/api/interviewer")) subsystem[0] = "interviewer";

            // supervisor app synchronization requests
            else if (path.StartsWithSegments("/api/supervisor")) subsystem[0] = "supervisor";

            // web interview requests for interview details
            else if (path.StartsWithSegments("/api/webinterview")) subsystem[0] = "webinterview";

            // all other requests
            else subsystem[0] = "other";

            using var inprogress = httpInProgress.Labels(subsystem).TrackInProgress();
            using var timer = this.httpRequestsDuration.Labels(subsystem).NewTimer();
            await next(context);
        }
    }
}
