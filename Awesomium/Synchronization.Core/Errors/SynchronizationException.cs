using System;
using System.Runtime.Serialization;

namespace Synchronization.Core.Errors
{
    public class SynchronizationException : Exception
    {
        public SynchronizationException()
        {
        }

        public SynchronizationException(string message) : base(message)
        {
        }

        public SynchronizationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SynchronizationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
