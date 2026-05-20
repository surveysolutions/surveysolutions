using System;

namespace Ncqrs.Eventing.Storage
{
    /// <summary>
    /// Thrown when an event stream cannot be appended because another process has already
    /// committed events for the same aggregate root, causing a version conflict.
    /// </summary>
    public class AggregateConcurrencyException : InvalidOperationException
    {
        public AggregateConcurrencyException(string message) : base(message)
        {
        }
    }
}
