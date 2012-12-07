using System;
using System.Runtime.Serialization;

namespace Synchronization.Core.Errors
{
    public class SynchronizationException : ServiceException
    {
        public SynchronizationException(string message)
            : base(message, null)
        {
        }

        public SynchronizationException(string message, Exception inner)
            : base(inner == null ? message : message + "\n" + inner.Message, inner)
        {
        }
    }
}
