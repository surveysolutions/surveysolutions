using System;

namespace WB.Core.Infrastructure.CommandBus
{
    public class CommandServiceException : Exception
    {
        public CommandServiceException(string message)
            : base(message) {}

        public CommandServiceException(string message, Exception innerException)
            : base(message, innerException) {}
    }
}