using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Implementation.StorageStrategy;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.EventHandlers
{
    public abstract class AbstractFunctionalEventHandler<TEntity, TStorage> : IFunctionalEventHandler, IEventHandler
        where TEntity : class, IReadSideRepositoryEntity
        where TStorage : class, IReadSideStorage<TEntity>
    {
        private readonly TStorage readSideStorage;

        protected AbstractFunctionalEventHandler(TStorage readSideStorage)
        {
            this.readSideStorage = readSideStorage;
        }

        protected void Handle(IPublishableEvent evt)
        {
            this.Handle(evt, this.readSideStorage);
        }

        public void Handle(IEnumerable<IPublishableEvent> publishableEvents, Guid eventSourceId)
        {
            if (publishableEvents.Any(this.Handles))
            {
                using (var inMemoryStorage = new InMemoryViewWriter<TEntity>(this.readSideStorage, eventSourceId))
                {
                    foreach (var publishableEvent in publishableEvents)
                    {
                        this.Handle(publishableEvent, inMemoryStorage);
                    }
                }
            }
        }

        private void Handle(IPublishableEvent evt, IReadSideStorage<TEntity> storage)
        {
            if (!this.Handles(evt))
                return;

            var eventType = typeof(IPublishedEvent<>).MakeGenericType(evt.Payload.GetType());

            TEntity currentState = GetViewById(evt.EventSourceId, storage);

            var newState = ApplyEventOnEntity(evt, eventType, currentState);

            if (newState != null)
            {
                SaveView(evt.EventSourceId, newState, storage);
            }
            else
            {
                RemoveView(evt.EventSourceId, storage);
            }
        }

        protected virtual TEntity ApplyEventOnEntity(IPublishableEvent evt, Type eventType, TEntity currentState)
        {
            var update = updateMethodsCache.GetOrAdd((this.GetType(), eventType), k =>
                k.eventHandler.GetMethod("Update", new[] { typeof(TEntity), k.eventType }));

            var newState = (TEntity) update
                ?.Invoke(this, new object[] { currentState, this.CreatePublishedEvent(evt) });
            return newState;
        }

        private static void RemoveView(Guid id, IReadSideStorage<TEntity> storage)
        {
            storage.Remove(id);
        }

        public virtual void RegisterHandlersInOldFashionNcqrsBus(InProcessEventBus oldEventBus)
        {
            var handlerMethodNames = new string[] { "Create", "Update", "Delete" };

            var handlers = this.GetType().GetMethods().Where(m => handlerMethodNames.Contains(m.Name)).ToList();

            handlers.ForEach((handler) => this.RegisterOldFashionHandler(oldEventBus, handler));
        }

        private static Type ExtractEventType(MethodInfo method)
        {
            var parameters = method.GetParameters();
            var eventParameter = parameters.Last().ParameterType;
            return eventParameter.GenericTypeArguments[0];
        }

        protected void RegisterOldFashionHandler(InProcessEventBus oldEventBus, MethodInfo method)
        {
            var evntType = ExtractEventType(method);
            oldEventBus.RegisterHandler(eventType: evntType, eventHandlerType: this.GetType(), handle: this.Handle);
        }

        private static void SaveView(Guid id, TEntity newState, IReadSideStorage<TEntity> storage)
        {
            storage.Store(newState, id);
        }

        protected virtual TEntity GetViewById(Guid id, IReadSideStorage<TEntity> storage)
        {
            return storage.GetById(id);
        }

        protected PublishedEvent CreatePublishedEvent(IUncommittedEvent evt)
        {
            var publishedEventClosedType = typeof(PublishedEvent<>).MakeGenericType(evt.Payload.GetType());

            return (PublishedEvent)Activator.CreateInstance(publishedEventClosedType, evt);
        }

        public string Name => this.GetType().Name;

        public virtual object[] Writers => new object[] { this.readSideStorage };
        public virtual object[] Readers => Array.Empty<object>();

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
