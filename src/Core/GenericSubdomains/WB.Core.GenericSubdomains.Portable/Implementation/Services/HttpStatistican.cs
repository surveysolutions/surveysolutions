using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using Flurl.Http;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Implementation.Services
{
    public class HttpStatistican : IHttpStatistican
    {
        private long Downloaded = 0;
        private long Uploaded = 0;
        private TimeSpan TotalDuration = TimeSpan.Zero;
        private readonly Stopwatch Stopwatch = new Stopwatch();

        public void Track(long? uploaded, long? downloaded, TimeSpan duration)
        {
            this.Downloaded += downloaded ?? 0;
            this.Uploaded += uploaded ?? 0;
            this.TotalDuration = this.TotalDuration.Add(duration);
        }

        public void Reset()
        {
            this.Downloaded = 0;
            this.Uploaded = 0;
            this.TotalDuration = TimeSpan.Zero;
            this.Stopwatch.Restart();
        }

        public HttpStats GetStats()
        {
            return new HttpStats {Downloaded = this.Downloaded, Uploaded = this.Uploaded, Duration = this.TotalDuration};
        }

        public void CollectHttpCallStatistics(HttpCall call)
        {
            var request = (call.Request.Content?.Headers?.ContentLength ?? 0) + GetHeadersEstimatedSize(call.Request?.Headers);
            var response = (call.Response.Content?.Headers?.ContentLength ?? 0) + GetHeadersEstimatedSize(call.Response?.Headers);

            if (call.Duration.HasValue)
            {
                var duration = call.Duration.Value;

                // From total call duration substracting time that it took server to process response. 
                // this way interview upload speed will not be affected by very long server side interview processing
                IEnumerable<string> perfHeaders;
                if (call.Response.Headers.TryGetValues("Server-Timing", out perfHeaders))
                {
                    foreach (var perfHeader in perfHeaders)
                    {
                        var actionTiming = perfHeader.Split(',').FirstOrDefault(s => s.Trim().StartsWith("action", StringComparison.Ordinal));

                        if (actionTiming != null)
                        {
                            var actionTime = actionTiming.Split('=');
                            duration = duration.Subtract(TimeSpan.FromSeconds(double.Parse(actionTime[1])));
                            break;
                        }
                    }
                }

                this.Track(request, response, duration);
            }
        }

        private long GetHeadersEstimatedSize(HttpHeaders headers)
        {
            return headers?.Sum(h => h.Key.Length + h.Value.Sum(hv => hv.Length)) ?? 0;
        }
    }

    public static class HttpStatisticianHelper
    {
        public static IFlurlClient CollectHttpStats(this IFlurlClient client, IHttpStatistican statistician)
        {
            client.Settings.AfterCall = statistician.CollectHttpCallStatistics;
            return client;
        }
    }
}