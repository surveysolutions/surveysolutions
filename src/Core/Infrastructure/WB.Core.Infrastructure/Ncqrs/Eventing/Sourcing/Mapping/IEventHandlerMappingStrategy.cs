using System;
using System.Collections.Generic;

namespace Ncqrs.Eventing.Sourcing.Mapping
{
    public interface IEventHandlerMappingStrategy
    {
        IEnumerable<ISourcedEventHandler> GetEventHandlers(object target);
        bool CanHandleEvent(object target, Type committedEvent);
    }
}