using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;

namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public class HttpCall
    {
        private const string requestPropertiesName = "CustomHttpCall";

        public HttpCall(HttpRequestMessage request)
        {
            Request = request;
            if (request?.Properties != null)
                request.Properties[requestPropertiesName] = this;
            DurationStopwatch = new Stopwatch();
        }

        internal static HttpCall Get(HttpRequestMessage request)
        {
            object obj;
            if (request?.Properties != null && request.Properties.TryGetValue(requestPropertiesName, out obj) && obj is HttpCall)
                return (HttpCall)obj;
            return null;
        }

        public HttpRequestMessage Request { get; }

        public HttpResponseMessage Response { get; set; }

        public Stopwatch DurationStopwatch { get; }

        public TimeSpan? Duration => DurationStopwatch.IsRunning ? (TimeSpan?) null : DurationStopwatch.Elapsed;

        public bool IsSucceeded => Response.IsSuccessStatusCode 
                                   || Response.StatusCode == HttpStatusCode.NotModified
                                   || Response.StatusCode == HttpStatusCode.NoContent;

        public Exception Exception { get; set; }
    }
}
