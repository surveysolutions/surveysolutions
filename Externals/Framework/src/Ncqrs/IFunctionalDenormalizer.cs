using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;

namespace Ncqrs
{
    public interface IFunctionalDenormalizer : IEventHandler
    {
        void Handle(IPublishableEvent evt);
        void RegisterHandlersInOldFashionNcqrsBus(InProcessEventBus oldEventBus);
        void ChangeForSingleEventSource(Guid eventSourceId);
        void FlushDataToPersistentStorage(Guid eventSourceId);
    }

    public interface IStorageStrategy<T> where T : class
    {
        T Select(Guid id);
        void AddOrUpdate(T projection, Guid id);
        void Delete(T projection, Guid id);
    }

    public abstract class FunctionalDenormalizer<T> : IFunctionalDenormalizer where T : class
    {
        private IStorageStrategy<T> storageStrategy;
        private IStorageStrategy<T> percistantStorageStrategy;
        protected FunctionalDenormalizer(IStorageStrategy<T> storageStrategy)
        {
            this.storageStrategy = storageStrategy;
        }

        public void Handle(IPublishableEvent evt)
        {
            var eventType = typeof(IPublishedEvent<>).MakeGenericType(evt.Payload.GetType());
            if (IsUpgrader(evt))
            {
                T currentState = storageStrategy.Select(evt.EventSourceId);
                var newState = (T)this.GetType().GetMethod("Update", new Type[] { typeof(T), eventType }).Invoke(this, new object[] { currentState, CreatePublishedEvent(evt) });
                storageStrategy.AddOrUpdate(newState, evt.EventSourceId);
                return;
            }

            if (IsCreator(evt))
            {
                var newObject =
                    (T)this.GetType()
                            .GetMethod("Create", new Type[] { eventType })
                            .Invoke(this, new object[] { CreatePublishedEvent(evt) });
                storageStrategy.AddOrUpdate(newObject, evt.EventSourceId);
                return;
            }

            if (IsDeleter(evt))
            {
                T currentState = storageStrategy.Select(evt.EventSourceId);
                this.GetType().GetMethod("Delete", new Type[] { eventType }).Invoke(this, new object[] { currentState, CreatePublishedEvent(evt) });
                storageStrategy.AddOrUpdate(currentState, evt.EventSourceId);
            }
        }

        public void RegisterHandlersInOldFashionNcqrsBus(InProcessEventBus oldEventBus)
        {
            var createMethods = this.GetType().GetMethods().Where(m => m.Name == "Create");
            foreach (var createMethod in createMethods)
            {
                var parameters = createMethod.GetParameters();
                var eventParameter = parameters[0].ParameterType;
                var evntType = eventParameter.GetGenericArguments()[0];
                NcqrsEnvironment.RegisterEventDataType(evntType);
                oldEventBus.RegisterHandler(evntType, this.Handle);
            }

            var updateMethods = this.GetType().GetMethods().Where(m => m.Name == "Update");
            foreach (var updateMethod in updateMethods)
            {
                var parameters = updateMethod.GetParameters();
                var eventParameter = parameters[1].ParameterType;
                var evntType = eventParameter.GetGenericArguments()[0];
                NcqrsEnvironment.RegisterEventDataType(evntType);
                oldEventBus.RegisterHandler(evntType, this.Handle);
            }

            var deleteMethods = this.GetType().GetMethods().Where(m => m.Name == "Delete");
            foreach (var deleteMethod in deleteMethods)
            {
                var parameters = deleteMethod.GetParameters();
                var eventParameter = parameters[1].ParameterType;
                var evntType = eventParameter.GetGenericArguments()[0];
                NcqrsEnvironment.RegisterEventDataType(evntType);
                oldEventBus.RegisterHandler(evntType, this.Handle);
            }
        }

        public void ChangeForSingleEventSource(Guid eventSourceId)
        {
            var single = new SingleEventSourceStorageStrategy<T>(storageStrategy.Select(eventSourceId));
            percistantStorageStrategy = storageStrategy;
            storageStrategy = single;
        }

        public void FlushDataToPersistentStorage(Guid eventSourceId)
        {
            percistantStorageStrategy.AddOrUpdate(storageStrategy.Select(eventSourceId), eventSourceId);
            storageStrategy = percistantStorageStrategy;
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

    public interface IUpdateHandler<T, TEvt>
    {
        T Update(T currentState, IPublishedEvent<TEvt> evnt);
    }

    public interface ICreateHandler<T, TEvt>
    {
        T Create(IPublishedEvent<TEvt> evnt);
    }

    public interface IDeleteHandler<T, TEvt>
    {
        void Delete(T currentState, IPublishedEvent<TEvt> evnt);
    }

    internal class SingleEventSourceStorageStrategy<T> : IStorageStrategy<T> where T : class
    {
        private  T view;
        public SingleEventSourceStorageStrategy(T view)
        {
            this.view = view;
        }

        public T Select(Guid id)
        {
            return view;
        }

        public void AddOrUpdate(T projection, Guid id)
        {
            view = projection;
        }

        public void Delete(T projection, Guid id)
        {
            view = null;
        }
    }
}
