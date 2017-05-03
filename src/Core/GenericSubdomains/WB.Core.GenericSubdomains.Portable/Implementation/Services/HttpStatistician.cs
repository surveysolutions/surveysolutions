using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using Flurl.Http;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Implementation.Services
{
    public class HttpStatistician : IHttpStatistician
    {
        private long Downloaded = 0;
        private long Uploaded = 0;
        private TimeSpan TotalDuration = TimeSpan.Zero;
        private readonly Stopwatch Stopwatch = new Stopwatch();

        private void Track(long uploaded, long downloaded, TimeSpan duration)
        {
            this.Downloaded += downloaded;
            this.Uploaded += uploaded;
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
            return new HttpStats {DownloadedBytes = this.Downloaded, UploadedBytes = this.Uploaded, Duration = this.TotalDuration};
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
                var serverTimingHeaderValues = this.GetServerTimingKeyValuePairs(call.Response.Headers);
                var serverTimings = serverTimingHeaderValues.ToLookup(h => h.Key, h => h.Value);

                foreach (var actionDuration in serverTimings["action"])
                {
                    double serverReportedActionDuration;

                    if (double.TryParse(actionDuration, NumberStyles.Any, CultureInfo.InvariantCulture, out serverReportedActionDuration))
                    {
                        duration = duration.Subtract(TimeSpan.FromSeconds(serverReportedActionDuration));
                    }
                }
                
                this.Track(request, response, duration);
            }
        }

        private IEnumerable<KeyValuePair<string, string>> GetServerTimingKeyValuePairs(HttpResponseHeaders headers)
        {
            IEnumerable<string> perfHeaders;

            if (!headers.TryGetValues("Server-Timing", out perfHeaders)) yield break;

            // Server-Timing header can contain several key=value pairs, where value is time in seconds: "db=1, action=0.125 , serialization=2"
            foreach (var header in perfHeaders)
            {
                var serverTimingValues = header.Split(',');

                foreach (var serverTimingValue in serverTimingValues)
                {
                    var split = serverTimingValue.Split('=');
                    
                    yield return new KeyValuePair<string,string>(split[0].Trim(), split.Length > 1 ? split[1].Trim() : null);
                }
            }
        }

        private long GetHeadersEstimatedSize(HttpHeaders headers)
        {
            return headers?.Sum(h => h.Key.Length + h.Value.Sum(hv => hv.Length) + 3 /* ': \n' */ ) ?? 0;
        }
    }

    public static class HttpStatisticianHelper
    {
        public static IFlurlClient CollectHttpStats(this IFlurlClient client, IHttpStatistician statistician)
        {
            client.Settings.AfterCall = call =>
            {
                try
                {
                    statistician.CollectHttpCallStatistics(call);
                }
                catch (Exception)
                {
                    // om nom nom - ignore everything. Just work.
                }
            };
            return client;
        }
    }
}