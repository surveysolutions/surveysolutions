using System;
using System.Net;

namespace WB.Core.BoundedContexts.Headquarters.EmailProviders
{
    public class EmailServiceException : Exception
    {
        public string Email { get; }
        public HttpStatusCode StatusCode { get; }
        public string[] Errors { get; }

        public EmailServiceException(string email, HttpStatusCode statusCode, Exception innerException = null, params string[] errors) 
            : base(string.Join("; ", errors), innerException)
        {
            Email = email;
            StatusCode = statusCode;
            Errors = errors;
        }
    }
}
