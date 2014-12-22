using System;
using System.Net;

namespace WB.Core.GenericSubdomains.Utils.Implementation
{
    public class RestSerializationException : RestException
    {
        public RestSerializationException(string message, Exception innerException = null)
            : base(message, HttpStatusCode.BadRequest, innerException)
        {
        }
    }
}