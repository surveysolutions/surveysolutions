using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
                this.Track(request, response, duration);
            }
        }

        private long GetHeadersEstimatedSize(HttpHeaders headers)
        {
            return headers?.Sum(h => h.Key.Length + h.Value.Sum(hv => hv.Length) + 3 /* ': \n' */ ) ?? 0;
        }
    }
}