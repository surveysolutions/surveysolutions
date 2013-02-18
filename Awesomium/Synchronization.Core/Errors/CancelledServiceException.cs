using System;
using System.Runtime.Serialization;

namespace Synchronization.Core.Errors
{
    public class CancelledServiceException : ServiceException
    {
        public CancelledServiceException(string message)
            : base(message, null)
        {
        }

        public CancelledServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
