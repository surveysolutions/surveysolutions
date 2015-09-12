using System;
using System.Net;

namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public class RestException : Exception
    {
        public RestException(string message, HttpStatusCode statusCode = HttpStatusCode.ServiceUnavailable,
            RestExceptionType type = RestExceptionType.ServiceUnavailable, Exception innerException = null)
            : base(message, innerException)
        {
            this.StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; private set; }
        public RestExceptionType Type { get; private set; }
    }

    public enum RestExceptionType { RequestCancelled, ServiceUnavailable }
}