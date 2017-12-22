using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal abstract class AbstractDenormalizer<TEntity> : IFunctionalEventHandler where TEntity : class, IReadSideRepositoryEntity
    {
        public void Handle(IEnumerable<IPublishableEvent> publishableEvents, Guid eventSourceId)
        {
            if (!publishableEvents.Any(this.Handles)) return;

            var state = new EntitiesState<TEntity>();

            foreach (var publishableEvent in publishableEvents)
            {
                var eventType = typeof(IPublishedEvent<>).MakeGenericType(publishableEvent.Payload.GetType());
                if (!this.Handles(publishableEvent))
                    continue;

                this.GetType().GetTypeInfo().GetMethod("Update", new[] { typeof(EntitiesState<TEntity>), eventType })
                    .Invoke(this, new object[] { state, this.CreatePublishedEvent(publishableEvent) });

                if (state == null) break;
            }

            if (state != null && (state.Removed.Any() || state.AddedOrUpdated.Any()))
                this.SaveState(state);
        }

        protected abstract void SaveState(EntitiesState<TEntity> state);

        protected PublishedEvent CreatePublishedEvent(IUncommittedEvent evt)
        {
            var publishedEventClosedType = typeof(PublishedEvent<>).MakeGenericType(evt.Payload.GetType());
            return (PublishedEvent)Activator.CreateInstance(publishedEventClosedType, evt);
        }

        protected virtual bool Handles(IUncommittedEvent evt)
            => typeof(IUpdateOrRemoveHandler<,>).MakeGenericType(typeof(TEntity), evt.Payload.GetType()).GetTypeInfo().IsInstanceOfType(this);

        public void RegisterHandlersInOldFashionNcqrsBus(InProcessEventBus oldEventBus)
        {
            //no need in current implementation
        }
    }
}