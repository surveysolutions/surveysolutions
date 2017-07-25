using System;
using System.Net;
using System.Net.Http;

namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public class HttpCall
    {
        private const string requestPropertiesName = "CustomHttpCall";

        internal HttpCall(HttpRequestMessage request)
        {
            Request = request;
            if (request?.Properties != null)
                request.Properties[requestPropertiesName] = this;
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

        public DateTime StartedUtc { get; set; }

        public DateTime? EndedUtc { get; set; }

        public TimeSpan? Duration => EndedUtc - StartedUtc;

        public bool IsSucceeded => Response.IsSuccessStatusCode 
                                   || Response.StatusCode == HttpStatusCode.NotModified
                                   || Response.StatusCode == HttpStatusCode.NoContent;

        public Exception Exception { get; set; }
    }
}