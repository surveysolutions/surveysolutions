using System;

namespace Ncqrs.Domain.Storage
{
    public class AggregateRootNotFoundException : Exception
    {
        public AggregateRootNotFoundException() { }

        public AggregateRootNotFoundException(string message) : base(message) {}

        public AggregateRootNotFoundException(string message, Exception inner) : base(message, inner) {}
    }
}
