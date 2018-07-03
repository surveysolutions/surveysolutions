using System;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
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
