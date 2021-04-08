using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Ncqrs.Eventing.ServiceModel.Bus;

namespace WB.Core.Infrastructure.EventHandlers
{
    public abstract class FunctionalEventHandlerBase<TEntity>
    {
        protected virtual TEntity ApplyEventOnEntity(IPublishableEvent evt,  TEntity currentState)
        {
            var eventType = typeof(IPublishedEvent<>).MakeGenericType(evt.Payload.GetType());

            var update = updateMethodsCache.GetOrAdd((this.GetType(), eventType), k =>
                k.eventHandler.GetMethod("Update", new[] { typeof(TEntity), k.eventType }));

            var newState = (TEntity)update
                ?.Invoke(this, new object[] { currentState, this.CreatePublishedEvent(evt) });
            return newState;
        }

        protected PublishedEvent CreatePublishedEvent(IUncommittedEvent evt)
        {
            var publishedEventClosedType = typeof(PublishedEvent<>).MakeGenericType(evt.Payload.GetType());
            return (PublishedEvent)Activator.CreateInstance(publishedEventClosedType, evt);
        }

        public void Handle(TEntity state, IEnumerable<IPublishableEvent> evts)
        {
            var newState = state;

            foreach (var evt in evts)
            {
                var eventType = typeof(IPublishedEvent<>).MakeGenericType(evt.Payload.GetType());

                var updateMethod = updateMethodsCache.GetOrAdd((this.GetType(), eventType), t =>
                {
                    return this
                        .GetType()
                        .GetMethod("Update", new[] { typeof(TEntity), t.eventType });
                });

                if (updateMethod == null)
                    continue;

                newState = (TEntity)updateMethod
                    .Invoke(this, new object[] { newState, this.CreatePublishedEvent(evt) });
            }
        }

        static readonly ConcurrentDictionary<(Type eventType, Type handler), bool> handleCache =
            new ConcurrentDictionary<(Type eventType, Type handler), bool>();

        static readonly ConcurrentDictionary<(Type eventHandler, Type eventType), MethodInfo> updateMethodsCache
            = new ConcurrentDictionary<(Type eventHandler, Type eventType), MethodInfo>();

        protected virtual bool Handles(IUncommittedEvent evt)
        {
            return handleCache.GetOrAdd((evt.Payload.GetType(), this.GetType()), key =>
            {
                var updateHandler = typeof(IUpdateHandler<,>).MakeGenericType(typeof(TEntity), key.eventType);
                return updateHandler.IsAssignableFrom(key.handler);
            });
        }
    }
}
