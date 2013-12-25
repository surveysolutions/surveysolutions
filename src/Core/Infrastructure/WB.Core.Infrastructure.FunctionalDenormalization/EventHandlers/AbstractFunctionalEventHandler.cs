using System;
using System.Linq;
using System.Reflection;
using Ncqrs;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.StorageStrategy;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers
{
    public abstract class AbstractFunctionalEventHandler<T> : IFunctionalEventHandler<T> where T : class, IReadSideRepositoryEntity
    {
        private IStorageStrategy<T> storageStrategy;
        protected AbstractFunctionalEventHandler(IReadSideRepositoryWriter<T> readsideRepositoryWriter)
        {
            this.storageStrategy = new ReadSideStorageStrategy<T>(readsideRepositoryWriter);
        }

        public void Handle(IPublishableEvent evt)
        {
            Handle(evt, storageStrategy);
        }

        public void Handle(IPublishableEvent evt, IStorageStrategy<T> storage)
        {
            var eventType = typeof(IPublishedEvent<>).MakeGenericType(evt.Payload.GetType());

            if (this.IsUpgrader(evt))
            {
                T currentState = this.GetViewById(evt.EventSourceId, storage);
                var newState = (T)this.GetType().GetMethod("Update", new Type[] { typeof(T), eventType }).Invoke(this, new object[] { currentState, this.CreatePublishedEvent(evt) });
                this.SaveView(evt.EventSourceId, newState, storage);
                return;
            }

            if (this.IsCreator(evt))
            {
                var newObject =
                    (T)this.GetType()
                        .GetMethod("Create", new Type[] { eventType })
                        .Invoke(this, new object[] { this.CreatePublishedEvent(evt) });
                this.SaveView(evt.EventSourceId, newObject, storage);
                return;
            }

            if (this.IsDeleter(evt))
            {
                T currentState = this.GetViewById(evt.EventSourceId, storage);
                this.GetType().GetMethod("Delete", new Type[] { eventType }).Invoke(this, new object[] { currentState, this.CreatePublishedEvent(evt) });
                this.SaveView(evt.EventSourceId, currentState, storage);
            }
        }

        public void RegisterHandlersInOldFashionNcqrsBus(InProcessEventBus oldEventBus)
        {
            var handlerMethodNames = new string[] { "Create", "Update", "Delete" };

            var handlers = this.GetType().GetMethods().Where(m => handlerMethodNames.Contains(m.Name)).ToList();

            handlers.ForEach((handler) => this.RegisterOldFashionHandler(oldEventBus, handler));
        }

        private Type ExtractEventType(MethodInfo method)
        {
            var parameters = method.GetParameters();
            var eventParameter = parameters.Last().ParameterType;
            return eventParameter.GetGenericArguments()[0];
        }

        private void RegisterOldFashionHandler(InProcessEventBus oldEventBus, MethodInfo method)
        {
            var evntType = this.ExtractEventType(method);
            NcqrsEnvironment.RegisterEventDataType(evntType);
            oldEventBus.RegisterHandler(evntType, this.Handle);
        }

        private void SaveView(Guid id, T newState, IStorageStrategy<T> storage)
        {
            storage.AddOrUpdate(newState, id);
        }

        private T GetViewById(Guid id, IStorageStrategy<T> storage)
        {
            return storage.Select(id);
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