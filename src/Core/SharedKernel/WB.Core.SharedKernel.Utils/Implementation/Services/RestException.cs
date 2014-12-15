using System;
using System.Net;

namespace WB.Core.SharedKernel.Utils.Implementation.Services
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