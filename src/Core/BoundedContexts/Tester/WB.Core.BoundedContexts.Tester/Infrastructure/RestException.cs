using System;
using System.Net;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure
{
    public class RestException : Exception
    {
        public RestException(string message, HttpStatusCode statusCode = HttpStatusCode.ServiceUnavailable, Exception innerException = null)
            : base(message, innerException)
        {
            this.StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; private set; }
    }
}