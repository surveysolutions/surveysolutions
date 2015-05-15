using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ncqrs;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
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
            using (var inMemoryStorage = new InMemoryViewWriter<TEntity>(this.readSideStorage, eventSourceId))
            {
                foreach (var publishableEvent in publishableEvents)
                {
                    this.Handle(publishableEvent, inMemoryStorage);
                }
            }
        }

        private void Handle(IPublishableEvent evt, IReadSideStorage<TEntity> storage)
        {
            var eventType = typeof(IPublishedEvent<>).MakeGenericType(evt.Payload.GetType());

            if (!this.Handles(evt))
                return;

            TEntity currentState = GetViewById(evt.EventSourceId, storage);

            var newState = (TEntity) this
                .GetType()
                .GetMethod("Update", new[] { typeof(TEntity), eventType })
                .Invoke(this, new object[] { currentState, this.CreatePublishedEvent(evt) });

            if (newState != null)
            {
                SaveView(evt.EventSourceId, newState, storage);
            }
            else
            {
                RemoveView(evt.EventSourceId, storage);
            }
        }

        private static void RemoveView(Guid id, IReadSideStorage<TEntity> storage)
        {
            storage.Remove(id);
        }

        public void RegisterHandlersInOldFashionNcqrsBus(InProcessEventBus oldEventBus)
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

        private void RegisterOldFashionHandler(InProcessEventBus oldEventBus, MethodInfo method)
        {
            var evntType = ExtractEventType(method);
            NcqrsEnvironment.RegisterEventDataType(evntType);
            oldEventBus.RegisterHandler(evntType, this.Handle);
        }

        private static void SaveView(Guid id, TEntity newState, IReadSideStorage<TEntity> storage)
        {
            storage.Store(newState, id);
        }

        private static TEntity GetViewById(Guid id, IReadSideStorage<TEntity> storage)
        {
            return storage.GetById(id);
        }

        private PublishedEvent CreatePublishedEvent(IPublishableEvent evt)
        {
            var publishedEventClosedType = typeof(PublishedEvent<>).MakeGenericType(evt.Payload.GetType());
            return (PublishedEvent)Activator.CreateInstance(publishedEventClosedType, evt);
        }

        private bool Handles(IPublishableEvent evt)
        {
            Type genericUpgrader = typeof(IUpdateHandler<,>);
            return genericUpgrader.MakeGenericType(typeof(TEntity), evt.Payload.GetType()).IsInstanceOfType(this.GetType());
        }

        public string Name { get { return this.GetType().Name; } }

        public virtual object[] Writers { get { return new object[] { this.readSideStorage }; } }

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