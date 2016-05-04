using System;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class SynchronizationException : Exception
    {
        public readonly SynchronizationExceptionType Type;

        public SynchronizationException(SynchronizationExceptionType type, string message = null, Exception innerException = null)
            : base(message, innerException)
        {
            this.Type = type;
        }
    }
}