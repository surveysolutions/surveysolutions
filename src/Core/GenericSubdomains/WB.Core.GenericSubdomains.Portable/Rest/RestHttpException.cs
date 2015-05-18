using System;
using System.Net;

namespace WB.Core.GenericSubdomains.Portable.Rest
{
    public class RestHttpException : Exception
    {
        public RestHttpException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }

        public string ReasonPhrase { get; set; }

        public HttpStatusCode StatusCode { get; set; }
    }
}
