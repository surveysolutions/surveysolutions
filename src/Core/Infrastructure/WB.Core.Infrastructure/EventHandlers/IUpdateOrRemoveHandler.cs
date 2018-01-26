using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;

namespace WB.Core.Infrastructure.EventHandlers
{
    public interface IUpdateOrRemoveHandler<TEntityState, TEvent>
        where TEvent : IEvent
    {
        void Update(TEntityState state, IPublishedEvent<TEvent> @event);
    }
}