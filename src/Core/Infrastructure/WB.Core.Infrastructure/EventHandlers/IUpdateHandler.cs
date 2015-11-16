using System;
using Ncqrs.Eventing.ServiceModel.Bus;

namespace WB.Core.Infrastructure.EventHandlers
{
    public interface IUpdateHandler<TState, TEvent>
    {
        TState Update(TState state, IPublishedEvent<TEvent> @event);
    }
}