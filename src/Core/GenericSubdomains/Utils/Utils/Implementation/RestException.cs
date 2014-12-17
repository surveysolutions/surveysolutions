using System;
using System.Net;

namespace WB.Core.GenericSubdomains.Utils.Implementation
{
    public class RestException : Exception
    {
        public RestException(string message) : this(message, (int) HttpStatusCode.ServiceUnavailable)
        {
        }

        public RestException(string message, int statusCode) : base(message)
        {
            this.StatusCode = statusCode;
        }

        public int StatusCode { get; private set; }
    }
}