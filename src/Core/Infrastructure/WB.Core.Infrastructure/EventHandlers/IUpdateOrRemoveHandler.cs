using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;

namespace WB.Core.Infrastructure.EventHandlers
{
    public interface IUpdateOrRemoveHandler<TEntity, TEvent>
        where TEvent : IEvent
    {
        void Update(EntitiesState<TEntity> state, IPublishedEvent<TEvent> @event);
    }
}