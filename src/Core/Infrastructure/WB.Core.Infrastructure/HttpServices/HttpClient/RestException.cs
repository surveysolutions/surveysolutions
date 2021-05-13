using System;
using System.Net;

namespace WB.Core.Infrastructure.HttpServices.HttpClient
{
    public class RestException : Exception
    {
        public RestException(string message, 
            HttpStatusCode statusCode = HttpStatusCode.ServiceUnavailable,
            RestExceptionType type = RestExceptionType.Unexpected, 
            Exception innerException = null)
            : this(null, message, statusCode, type, innerException)
        {
        }
        
        public RestException(string serverErrorCode, string message, 
            HttpStatusCode statusCode = HttpStatusCode.ServiceUnavailable,
            RestExceptionType type = RestExceptionType.Unexpected, 
            Exception innerException = null)
            : base(message, innerException)
        {
            this.StatusCode = statusCode;
            this.Type = type;
            this.ServerErrorCode = serverErrorCode;
        }

        public HttpStatusCode StatusCode { get; private set; }
        public RestExceptionType Type { get; private set; }
        public string ServerErrorCode { get; private set; }
    }
}