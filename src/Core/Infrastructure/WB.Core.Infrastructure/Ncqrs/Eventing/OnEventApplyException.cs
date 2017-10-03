using System;
using Ncqrs.Eventing;

namespace Ncqrs.Domain
{
    public class OnEventApplyException : Exception
    {
        public UncommittedEvent Event { get; }

        public OnEventApplyException(UncommittedEvent evnt)
            : this(evnt, evnt != null ? $"Error during handling the {evnt.GetType().FullName} event." : null)
        {
        }

        public OnEventApplyException(UncommittedEvent evnt, String message)
            : this(evnt, message, (Exception) null)
        {
        }

        public OnEventApplyException(UncommittedEvent evnt, String message, Exception inner)
            : base(message, inner)
        {
            Event = evnt;
        }
    }
}