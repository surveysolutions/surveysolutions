using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.StorageStrategy;
using WB.Core.Infrastructure.ReadSide.Repository;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Implementation.EventDispatcher
{
    public class NcqrCompatibleEventDispatcher : IEventDispatcher
    {
        private readonly Dictionary<Type, EventHandlerWrapper> registredHandlers = new Dictionary<Type, EventHandlerWrapper>();
        private readonly IEventStore eventStore;

        public NcqrCompatibleEventDispatcher(IEventStore eventStore)
        {
            this.eventStore = eventStore;
        }

        private bool IsEventNeedToBeIgnored(Guid id)
        {
            if (this.eventsToBeIgnored.Contains(id))
            {
                this.eventsToBeIgnored.Remove(id);
                return true;
            }
            return false;
        }

        public void Publish(IPublishableEvent eventMessage)
        {
            if(this.IsEventNeedToBeIgnored(eventMessage.EventIdentifier))
                return;

            foreach (var handler in this.registredHandlers.Values.ToList())
            {
                handler.Bus.Publish(eventMessage);
            }
        }

        public void Publish(IEnumerable<IPublishableEvent> eventMessages)
        {
            foreach (var publishableEvent in eventMessages)
            {
                if (this.IsEventNeedToBeIgnored(publishableEvent.EventIdentifier))
                    continue;

                foreach (var handler in this.registredHandlers.Values.ToList())
                {
                    handler.Bus.Publish(publishableEvent);
                }
            }
        }

        public void PublishByEventSource<T>(IEnumerable<CommittedEvent> eventStream, IStorageStrategy<T> storage) where T : class, IReadSideRepositoryEntity
        {
            var functionalHandlers =
                this.registredHandlers.Values.Select(h => h.Handler as IFunctionalEventHandler<T>).Where(h => h != null).ToList();

            foreach (var publishableEvent in eventStream)
            {
                foreach (var handler in functionalHandlers)
                {
                    handler.Handle(publishableEvent, storage);
                }
            }
        }

        public void PublishEventToHandlers(IPublishableEvent eventMessage, IEnumerable<IEventHandler> handlers)
        {
            foreach (var bus in this.GetListOfBusesForRebuild(handlers))
            {
                bus.Publish(eventMessage);
            }
        }

        public void Register(IEventHandler handler)
        {
            var inProcessBus = new InProcessEventBus(true);
            IEnumerable<Type> ieventHandlers = handler.GetType().GetInterfaces().Where(IsIEventHandlerInterface);
            foreach (Type ieventHandler in ieventHandlers)
            {
                inProcessBus.RegisterHandler(handler, ieventHandler.GetGenericArguments()[0]);
            }

            var functionalDenormalizer = handler as IFunctionalEventHandler;
            if (functionalDenormalizer != null)
            {
                functionalDenormalizer.RegisterHandlersInOldFashionNcqrsBus(inProcessBus);
            }

            this.registredHandlers.Add(handler.GetType(), new EventHandlerWrapper(handler, inProcessBus));
        }

        public void Unregister(IEventHandler handler)
        {
            this.registredHandlers.Remove(handler.GetType());
        }

        public IEnumerable<IEventHandler> GetAllRegistredEventHandlers()
        {
            return this.registredHandlers.Values.Select(v => v.Handler).ToArray();
        }

        private static bool IsIEventHandlerInterface(Type type)
        {
            return type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof (IEventHandler<>);
        }

        private IEnumerable<InProcessEventBus> GetListOfBusesForRebuild(IEnumerable<IEventHandler> enabledHandlers)
        {
            return this.registredHandlers.Values.Where(h => enabledHandlers.Contains(h.Handler)).Select(h => h.Bus).ToList();
        }

        public void IgnoreEventWithId(Guid eventIdentifier)
        {
            eventsToBeIgnored.Add(eventIdentifier);
        }

        private readonly HashSet<Guid> eventsToBeIgnored=new HashSet<Guid>();
    }
}