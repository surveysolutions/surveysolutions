using System;

namespace WB.Core.Infrastructure.CommandBus
{
    public class CommandRegistryException : Exception
    {
        public CommandRegistryException(string message)
            : base(message) {}

        public CommandRegistryException(string message, Exception innerException)
            : base(message, innerException) {}
    }
}