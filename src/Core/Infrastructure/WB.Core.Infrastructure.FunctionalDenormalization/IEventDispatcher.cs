using System;
using System.Collections.Generic;
using Ncqrs.Eventing.ServiceModel.Bus;

namespace WB.Core.Infrastructure.FunctionalDenormalization
{
    public interface IEventDispatcher : IEventBus
    {
        void PublishEventToHandlers(IPublishableEvent eventMessage, IEnumerable<IEventHandler> handlers);
        void PublishByEventSource(Guid eventSourceId, long sequence = 0);

        IEnumerable<IEventHandler> GetAllRegistredEventHandlers();

        void Register(IEventHandler handler);
        void Unregister(IEventHandler handler);
    }
}
