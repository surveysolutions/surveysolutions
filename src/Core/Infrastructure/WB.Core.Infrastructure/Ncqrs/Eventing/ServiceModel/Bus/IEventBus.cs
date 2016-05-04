using System.Collections.Generic;
using Ncqrs.Domain;
using WB.Core.Infrastructure.EventBus.Lite;

namespace Ncqrs.Eventing.ServiceModel.Bus
{
    public delegate void EventHandlerExceptionDelegate(EventHandlerException e);

    public interface IEventBus : ILiteEventBus
    {
        event EventHandlerExceptionDelegate OnCatchingNonCriticalEventHandlerException;

        void Publish(IPublishableEvent eventMessage);

        void Publish(IEnumerable<IPublishableEvent> eventMessages);
    }
}