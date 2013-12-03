using System;
using System.Linq;
using System.Reflection;
using Ncqrs;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.StorageStrategy;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.FunctionalDenormalization
{
    public abstract class AbstractFunctionalDenormalizer<T> : IFunctionalDenormalizer where T : class, IReadSideRepositoryEntity
    {
        private IStorageStrategy<T> storageStrategy;
        private IStorageStrategy<T> percistantStorageStrategy;
        protected AbstractFunctionalDenormalizer(IReadSideRepositoryWriter<T> readsideRepositoryWriter)
        {
            this.storageStrategy = new ReadSideStorageStrategy<T>(readsideRepositoryWriter);
        }

        public void Handle(IPublishableEvent evt)
        {
            var eventType = typeof(IPublishedEvent<>).MakeGenericType(evt.Payload.GetType());

            if (this.IsUpgrader(evt))
            {
                T currentState = this.GetViewById(evt.EventSourceId);
                var newState = (T)this.GetType().GetMethod("Update", new Type[] { typeof(T), eventType }).Invoke(this, new object[] { currentState, this.CreatePublishedEvent(evt) });
                this.SaveView(evt.EventSourceId, newState);
                return;
            }

            if (this.IsCreator(evt))
            {
                var newObject =
                    (T)this.GetType()
                        .GetMethod("Create", new Type[] { eventType })
                        .Invoke(this, new object[] { this.CreatePublishedEvent(evt) });
                this.SaveView(evt.EventSourceId, newObject);
                return;
            }

            if (this.IsDeleter(evt))
            {
                T currentState = this.GetViewById(evt.EventSourceId);
                this.GetType().GetMethod("Delete", new Type[] { eventType }).Invoke(this, new object[] { currentState, this.CreatePublishedEvent(evt) });
                this.SaveView(evt.EventSourceId, currentState);
            }
        }

        public void RegisterHandlersInOldFashionNcqrsBus(InProcessEventBus oldEventBus)
        {
            var handlerMethodNames = new string[] { "Create", "Update", "Delete" };

            var handlers = this.GetType().GetMethods().Where(m => handlerMethodNames.Contains(m.Name)).ToList();

            handlers.ForEach((handler) => RegisterOldFashionHandler(oldEventBus, handler));
        }

        private Type ExtractEventType(MethodInfo method)
        {
            var parameters = method.GetParameters();
            var eventParameter = parameters.Last().ParameterType;
            return eventParameter.GetGenericArguments()[0];
        }

        private void RegisterOldFashionHandler(InProcessEventBus oldEventBus, MethodInfo method)
        {
            var evntType = ExtractEventType(method);
            NcqrsEnvironment.RegisterEventDataType(evntType);
            oldEventBus.RegisterHandler(evntType, this.Handle);
        }

        private void SaveView(Guid id, T newState)
        {
            this.storageStrategy.AddOrUpdate(newState, id);
        }

        private T GetViewById(Guid id)
        {
            return this.storageStrategy.Select(id);
        }

        public void ChangeForSingleEventSource(Guid eventSourceId)
        {
            var single = new SingleEventSourceStorageStrategy<T>(this.storageStrategy.Select(eventSourceId));
            this.percistantStorageStrategy = this.storageStrategy;
            this.storageStrategy = single;
        }

        public void FlushDataToPersistentStorage(Guid eventSourceId)
        {
            this.percistantStorageStrategy.AddOrUpdate(this.storageStrategy.Select(eventSourceId), eventSourceId);
            this.storageStrategy = this.percistantStorageStrategy;
        }

        protected PublishedEvent CreatePublishedEvent(IPublishableEvent evt)
        {
            var publishedEventClosedType = typeof(PublishedEvent<>).MakeGenericType(evt.Payload.GetType());
            return (PublishedEvent)Activator.CreateInstance(publishedEventClosedType, evt);
        }

        protected bool IsUpgrader(IPublishableEvent evt)
        {
            Type genericUpgrader = typeof(IUpdateHandler<,>);
            return genericUpgrader.MakeGenericType(typeof(T), evt.Payload.GetType()).IsInstanceOfType(this);
        }

        protected bool IsCreator(IPublishableEvent evt)
        {
            Type genericUpgrader = typeof(ICreateHandler<,>);
            return genericUpgrader.MakeGenericType(typeof(T), evt.Payload.GetType()).IsInstanceOfType(this);
        }

        protected bool IsDeleter(IPublishableEvent evt)
        {
            Type genericDeleter = typeof(IDeleteHandler<,>);
            return genericDeleter.MakeGenericType(typeof(T), evt.Payload.GetType()).IsInstanceOfType(this);
        }

        public string Name { get { return this.GetType().Name; } }

        public abstract Type[] UsesViews { get; }

        public Type[] BuildsViews
        {
            get { return new[] { typeof(T) }; }
        }
    }
}