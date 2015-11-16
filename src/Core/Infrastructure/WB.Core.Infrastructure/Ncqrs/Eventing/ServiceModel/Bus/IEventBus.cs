using System;
using System.Collections.Generic;
using Ncqrs.Domain;
using WB.Core.Infrastructure.EventBus.Lite;

namespace Ncqrs.Eventing.ServiceModel.Bus
{
    public interface IEventBus : ILiteEventBus
    {
        void Publish(IPublishableEvent eventMessage, Action<EventHandlerException> onCatchingNonCriticalEventHandlerException = null);

        void Publish(IEnumerable<IPublishableEvent> eventMessages);
    }
}