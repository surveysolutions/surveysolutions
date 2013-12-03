using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;

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

        public void Publish(IPublishableEvent eventMessage)
        {
            foreach (var handler in this.registredHandlers.Values.ToList())
            {
                handler.Bus.Publish(eventMessage);
            }
        }

        public void Publish(IEnumerable<IPublishableEvent> eventMessages)
        {
            foreach (var handler in this.registredHandlers.Values.ToList())
            {
                handler.Bus.Publish(eventMessages);
            }
        }

        public void PublishByEventSource(Guid eventSourceId, long sequence = 0)
        {
            var eventMessages = this.eventStore.ReadFrom(eventSourceId, sequence + 1, long.MaxValue);

            if (eventMessages.IsEmpty)
                return;

            var functionalDenormalizers = this.registredHandlers.Values.Where(h => h.Handler is IFunctionalEventHandler).ToList();
            foreach (var handler in functionalDenormalizers)
            {
                var functionalHandler = handler.Handler as IFunctionalEventHandler;
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
                var functionalHandler = handler.Handler as IFunctionalEventHandler;
                if (functionalHandler != null)
                {
                    functionalHandler.FlushDataToPersistentStorage(eventSourceId);
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
    }
}