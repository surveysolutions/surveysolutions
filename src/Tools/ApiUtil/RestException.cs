using System;
using System.Net;

namespace ApiUtil
{
    public class RestException : Exception
    {
        public RestException(string message, HttpStatusCode statusCode = HttpStatusCode.ServiceUnavailable,
            RestExceptionType type = RestExceptionType.Unexpected, Exception innerException = null)
            : base(message, innerException)
        {
            this.StatusCode = statusCode;
            this.Type = type;
        }

        public HttpStatusCode StatusCode { get; private set; }
        public RestExceptionType Type { get; private set; }
    }
}