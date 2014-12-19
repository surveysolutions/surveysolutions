using System;
using System.Net;

namespace WB.Core.GenericSubdomains.Rest
{
    public class RestException : Exception
    {
        public RestException() {}

        public RestException(string message, int statusCode = 200)
            : base(message)
        {
            this.StatusCode = statusCode;
        }

        public RestException(string message, Exception innerException, int statusCode = 200)
            : base(message, innerException)
        {
            this.StatusCode = statusCode;
        }

        public int StatusCode { get; private set; }
    }
}