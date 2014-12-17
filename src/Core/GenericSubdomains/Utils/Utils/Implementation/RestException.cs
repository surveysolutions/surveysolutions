using System;
using System.Net;

namespace WB.Core.GenericSubdomains.Utils.Implementation
{
    public class RestException : Exception
    {
        public RestException(string message, int statusCode = (int) HttpStatusCode.ServiceUnavailable, Exception innerException = null)
            : base(message, innerException)
        {
            this.StatusCode = statusCode;
        }

        public int StatusCode { get; private set; }
    }
}