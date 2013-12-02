using System;
using System.Linq;
using Ncqrs;
using Ncqrs.Eventing.ServiceModel.Bus;
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
                T currentState = this.storageStrategy.Select(evt.EventSourceId);
                var newState = (T)this.GetType().GetMethod("Update", new Type[] { typeof(T), eventType }).Invoke(this, new object[] { currentState, this.CreatePublishedEvent(evt) });
                this.storageStrategy.AddOrUpdate(newState, evt.EventSourceId);
                return;
            }

            if (this.IsCreator(evt))
            {
                var newObject =
                    (T)this.GetType()
                        .GetMethod("Create", new Type[] { eventType })
                        .Invoke(this, new object[] { this.CreatePublishedEvent(evt) });
                this.storageStrategy.AddOrUpdate(newObject, evt.EventSourceId);
                return;
            }

            if (this.IsDeleter(evt))
            {
                T currentState = this.storageStrategy.Select(evt.EventSourceId);
                this.GetType().GetMethod("Delete", new Type[] { eventType }).Invoke(this, new object[] { currentState, this.CreatePublishedEvent(evt) });
                this.storageStrategy.AddOrUpdate(currentState, evt.EventSourceId);
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