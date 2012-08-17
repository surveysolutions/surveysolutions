using System;
using System.Runtime.Serialization;

namespace Synchronization.Core.Errors
{
    public class CancelledSynchronizationException : SynchronizationException
    {
        public CancelledSynchronizationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
