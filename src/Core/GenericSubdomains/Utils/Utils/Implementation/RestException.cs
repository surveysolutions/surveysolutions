using System;
using System.Net;

namespace WB.Core.GenericSubdomains.Utils.Implementation
{
    public class RestException : Exception
    {
        public RestException(string message, HttpStatusCode statusCode = HttpStatusCode.ServiceUnavailable, Exception innerException = null, SyncStatusCode internalCode = SyncStatusCode.General)
            : base(message, innerException)
        {
            this.StatusCode = statusCode;
            this.InternalCode = internalCode;
        }

        public HttpStatusCode StatusCode { get; private set; }
        public SyncStatusCode InternalCode { get; private set; }
    }
}