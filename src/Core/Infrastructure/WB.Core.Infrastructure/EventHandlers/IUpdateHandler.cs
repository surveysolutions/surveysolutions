using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Core.Infrastructure.EventHandlers
{
    public interface IUpdateHandler<TState, TEvent>
        where TEvent : IEvent
    {
        TState Update(TState state, IPublishedEvent<TEvent> @event);
    }
}