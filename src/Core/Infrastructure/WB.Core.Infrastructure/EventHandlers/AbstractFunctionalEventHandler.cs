using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ncqrs;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Implementation.StorageStrategy;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.EventHandlers
{
    public abstract class AbstractFunctionalEventHandler<TEntity, TStorage> : IFunctionalEventHandler, IAtomicEventHandler
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
            var eventType = typeof(IPublishedEvent<>).MakeGenericType(evt.Payload.GetType());

            if (!this.Handles(evt))
                return;

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
            var newState = (TEntity) this
                .GetType()
                .GetTypeInfo().GetMethod("Update", new[] {typeof(TEntity), eventType})
                .Invoke(this, new object[] {currentState, this.CreatePublishedEvent(evt)});
            return newState;
        }

        private static void RemoveView(Guid id, IReadSideStorage<TEntity> storage)
        {
            storage.Remove(id);
        }

        public virtual void RegisterHandlersInOldFashionNcqrsBus(InProcessEventBus oldEventBus)
        {
            var handlerMethodNames = new string[] { "Create", "Update", "Delete" };

            var handlers = this.GetType().GetTypeInfo().GetMethods().Where(m => handlerMethodNames.Contains(m.Name)).ToList();

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

        private static TEntity GetViewById(Guid id, IReadSideStorage<TEntity> storage)
        {
            return storage.GetById(id);
        }

        protected PublishedEvent CreatePublishedEvent(IUncommittedEvent evt)
        {
            var publishedEventClosedType = typeof(PublishedEvent<>).MakeGenericType(evt.Payload.GetType());
            return (PublishedEvent)Activator.CreateInstance(publishedEventClosedType, evt);
        }

        protected virtual bool Handles(IUncommittedEvent evt)
        {
            Type genericUpgrader = typeof(IUpdateHandler<,>);
            return genericUpgrader.MakeGenericType(typeof(TEntity), evt.Payload.GetType()).GetTypeInfo().IsAssignableFrom(this.GetType());
        }

        public string Name => this.GetType().Name;

        public virtual object[] Writers => new object[] { this.readSideStorage };

        public virtual void CleanWritersByEventSource(Guid eventSourceId)
        {
            if (this.Writers.Length > 1)
                throw new InvalidOperationException(
                    "default implementation on CleanWritersByEventSource can't be performed for FunctionalEvent handler which builds ore then one view. Please provide your own implementation of CleanWritersByEventSource");

            if (this.Writers.Length == 0)
                throw new InvalidOperationException(
                  "writers to clean up are missing");

            if (this.Writers[0] != this.readSideStorage)
                throw new InvalidOperationException("mismatch of view and writer");

            this.readSideStorage.Remove(eventSourceId);
        }

        public virtual object[] Readers
        {
            get { return new object[0]; }
        }
    }
}