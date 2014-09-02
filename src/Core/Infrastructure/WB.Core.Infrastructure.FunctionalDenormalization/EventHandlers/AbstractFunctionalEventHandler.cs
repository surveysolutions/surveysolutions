using System;
using System.Collections.Generic;
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
        private IReadSideRepositoryWriter<T> readsideRepositoryWriter;

        protected AbstractFunctionalEventHandler(IReadSideRepositoryWriter<T> readsideRepositoryWriter)
        {
            this.readsideRepositoryWriter = readsideRepositoryWriter;
        }

        public void Handle(IPublishableEvent evt)
        {
            Handle(evt, readsideRepositoryWriter);
        }

        public void Handle(IEnumerable<IPublishableEvent> publishableEvents, Guid eventSourceId)
        {
            using (var inMemoryStorage = new InMemoryViewWriter<T>(this.readsideRepositoryWriter, eventSourceId))
            {
                foreach (var publishableEvent in publishableEvents)
                {
                    Handle(publishableEvent, inMemoryStorage);
                }
            }
        }

        public void Handle(IPublishableEvent evt, IReadSideRepositoryWriter<T> storage)
        {
            var eventType = typeof(IPublishedEvent<>).MakeGenericType(evt.Payload.GetType());

            if (this.IsUpgrader(evt))
            {
                T currentState = this.GetViewById(evt.EventSourceId, storage);
                var newState = (T)this.GetType().GetMethod("Update", new[] { typeof(T), eventType }).Invoke(this, new object[] { currentState, this.CreatePublishedEvent(evt) });

                if (newState != null)
                {
                    this.SaveView(evt.EventSourceId, newState, storage);
                }
                else
                {
                    this.RemoveView(evt.EventSourceId, storage, currentState);
                }

                return;
            }

            if (this.IsCreator(evt))
            {
                var newObject =
                    (T) this.GetType()
                        .GetMethod("Create", new[] { eventType })
                        .Invoke(this, new object[] { this.CreatePublishedEvent(evt) });
                if (newObject != null)
                    this.SaveView(evt.EventSourceId, newObject, storage);
                return;
            }

            if (this.IsDeleter(evt))
            {
                T currentState = this.GetViewById(evt.EventSourceId, storage);
                var methodInfo = this.GetType().GetMethod("Delete", new[] { typeof(T), eventType });
                var newState = (T)methodInfo.Invoke(this, new object[] { currentState, this.CreatePublishedEvent(evt) });
                this.RemoveView(evt.EventSourceId, storage, newState);
            }
        }

        private void RemoveView(Guid id, IReadSideRepositoryWriter<T> storage, T currentState)
        {
            storage.Remove(id);
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

        private void SaveView(Guid id, T newState, IReadSideRepositoryWriter<T> storage)
        {
            storage.Store(newState, id);
        }

        private T GetViewById(Guid id, IReadSideRepositoryWriter<T> storage)
        {
            return storage.GetById(id);
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

        public virtual Type[] BuildsViews
        {
            get { return new[] { typeof(T) }; }
        }

        public virtual Type[] UsesViews
        {
            get { return new Type[0]; }
        }
    }
}