using System;

namespace WB.Core.GenericSubdomains.Portable.Rest
{
    public class RestHttpTimeoutException : Exception
    {
        public RestHttpTimeoutException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}
