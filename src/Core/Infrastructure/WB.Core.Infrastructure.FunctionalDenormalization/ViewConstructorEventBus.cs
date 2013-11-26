using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using Ncqrs.Eventing.Storage;

namespace WB.Core.Infrastructure.FunctionalDenormalization
{
    public class ViewConstructorEventBus : IViewConstructorEventBus
    {
        private readonly Dictionary<Type, EventHandlerWrapper> handlers = new Dictionary<Type, EventHandlerWrapper>();
        private readonly IEventStore eventStore;

        public ViewConstructorEventBus(IEventStore eventStore)
        {
            this.eventStore = eventStore;
        }

        public void Publish(IPublishableEvent eventMessage)
        {
            foreach (var handler in this.handlers.Values.ToList())
            {
                handler.Bus.Publish(eventMessage);
            }
        }

        public void Publish(IEnumerable<IPublishableEvent> eventMessages)
        {
            foreach (var handler in this.handlers.Values.ToList())
            {
                handler.Bus.Publish(eventMessages);
            }
        }

        public void PublishForSingleEventSource(Guid eventSourceId, long sequence = 0)
        {
            var eventMessages = this.eventStore.ReadFrom(eventSourceId, sequence + 1, long.MaxValue);

            if (eventMessages.IsEmpty)
                return;

            var functionalDenormalizers = this.handlers.Values.Where(h => h.Handler is IFunctionalDenormalizer).ToList();
            foreach (var handler in functionalDenormalizers)
            {
                var functionalHandler = handler.Handler as IFunctionalDenormalizer;
                if (functionalHandler != null)
                {
                    functionalHandler.ChangeForSingleEventSource(eventSourceId);
                }
            }

            foreach (var publishableEvent in eventMessages)
            {
                foreach (var handler in functionalDenormalizers)
                {
                    handler.Bus.Publish(publishableEvent);
                }
            }

            foreach (var handler in functionalDenormalizers)
            {
                var functionalHandler = handler.Handler as IFunctionalDenormalizer;
                if (functionalHandler != null)
                {
                    functionalHandler.FlushDataToPersistentStorage(eventSourceId);
                }
            }
        }


        public void PublishEventsToHandlers(IPublishableEvent eventMessage, IEnumerable<IEventHandler> handlersForPublish)
        {
            foreach (var bus in this.GetListOfBusesForRebuild(handlersForPublish))
            {
                bus.Publish(eventMessage);
            }
        }

        private IEnumerable<InProcessEventBus> GetListOfBusesForRebuild(IEnumerable<IEventHandler> enabledHandlers)
        {
            return this.handlers.Values.Where(h => enabledHandlers.Contains(h.Handler)).Select(h=>h.Bus).ToList();
        }

        public void AddHandler(IEventHandler handler)
        {
            var inProcessBus = new InProcessEventBus(true);
            IEnumerable<Type> ieventHandlers = handler.GetType().GetInterfaces().Where(IsIEventHandlerInterface);
            foreach (Type ieventHandler in ieventHandlers)
            {
                inProcessBus.RegisterHandler(handler, ieventHandler.GetGenericArguments()[0]);
            }

            var functionalDenormalizer = handler as IFunctionalDenormalizer;
            if (functionalDenormalizer != null)
            {
                functionalDenormalizer.RegisterHandlersInOldFashionNcqrsBus(inProcessBus);
            }

            this.handlers.Add(handler.GetType(), new EventHandlerWrapper(handler, inProcessBus));
        }

        public void RemoveHandler(IEventHandler handler)
        {
            this.handlers.Remove(handler.GetType());
        }

        public IEnumerable<IEventHandler> GetAllRegistredEventHandlers()
        {
            return this.handlers.Values.Select(v => v.Handler).ToArray();
        }

        private static bool IsIEventHandlerInterface(Type type)
        {
            return type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof (IEventHandler<>);
        }
    }
}