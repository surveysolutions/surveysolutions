using System;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class SynchronizationException : Exception
    {
        public readonly SynchronizationExceptionType Type;

        public SynchronizationException(SynchronizationExceptionType type, string message, Exception innerException)
            : base(message, innerException)
        {
            this.Type = type;
        }
    }
}