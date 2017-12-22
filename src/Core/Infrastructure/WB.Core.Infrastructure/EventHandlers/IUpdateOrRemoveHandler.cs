using System.Collections.Generic;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;

namespace WB.Core.Infrastructure.EventHandlers
{
    public class EntitiesState<TEntity>
    {
        public List<TEntity> AddedOrUpdated { get; set; } = new List<TEntity>();
        public List<TEntity> Removed { get; set; } = new List<TEntity>();
    }

    public interface IUpdateOrRemoveHandler<TEntity, TEvent>
        where TEvent : IEvent
    {
        void Update(EntitiesState<TEntity> state, IPublishedEvent<TEvent> @event);
    }
}