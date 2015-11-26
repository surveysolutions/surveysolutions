using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Core.Infrastructure.EventHandlers
{
    public interface IUpdateHandler<TState, TEvent>
        where TEvt : ILiteEvent
    {
        TState Update(TState state, IPublishedEvent<TEvent> @event);
    }
}