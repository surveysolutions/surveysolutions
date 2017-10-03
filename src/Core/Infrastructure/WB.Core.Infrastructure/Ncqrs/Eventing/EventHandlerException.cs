using System;

namespace Ncqrs.Domain
{
    public class EventHandlerException : Exception
    {
        public Type EventHandlerType { get; private set; }
        public Type EventType { get; private set; }
        public bool IsCritical { get; private set; }
        public EventHandlerException(Type eventHandlerType, Type eventType, bool isCritical, Exception innerException)
            : base("EventHandlerException: " + innerException?.Message ?? "", innerException)
        {
            this.EventHandlerType = eventHandlerType;
            this.EventType = eventType;
            this.IsCritical = isCritical;
        }
    }
}
