using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;

namespace Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus
{
    public class ViewConstructorEventBus : IViewConstructorEventBus
    {
        private readonly Dictionary<Type, EventHandlerWrapper> handlers = new Dictionary<Type, EventHandlerWrapper>();

        public void Publish(IPublishableEvent eventMessage)
        {
            foreach (var handler in handlers.Values.Where(h => h.Enabled).ToList())
            {
                handler.Bus.Publish(eventMessage);
            }
        }

        public void Publish(IEnumerable<IPublishableEvent> eventMessages)
        {
            foreach (var handler in handlers.Values.Where(h=>h.Enabled).ToList())
            {
                handler.Bus.Publish(eventMessages);
            }
        }

        public void PublishForSingleEventSource(IEnumerable<IPublishableEvent> eventMessages, Guid eventSourceId)
        {
            var functionalDenormalizers = handlers.Values.Where(h => h.Handler is IFunctionalDenormalizer).ToList();
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

        public void DisableEventHandler(Type handlerType)
        {
            if (handlers.ContainsKey(handlerType))
                return;
            handlers[handlerType].DisableBus();
        }

        public void EnableAllHandlers()
        {
            foreach (var handler in handlers.Values)
            {
                handler.EnableBus();
            }
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

            handlers.Add(handler.GetType(), new EventHandlerWrapper(handler, inProcessBus));
        }

        public void RemoveHandler(IEventHandler handler)
        {
            handlers.Remove(handler.GetType());
        }

        public IEnumerable<IEventHandler> GetAllRegistredEventHandlers()
        {
            return handlers.Values.Select(v => v.Handler).ToArray();
        }

        private static bool IsIEventHandlerInterface(Type type)
        {
            return type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof (IEventHandler<>);
        }
    }

    public class EventHandlerWrapper
    {
        public EventHandlerWrapper(IEventHandler handler, InProcessEventBus bus)
        {
            Handler = handler;
            Bus = bus;
            Enabled = true;
        }

        public IEventHandler Handler { get; private set; }
        public InProcessEventBus Bus { get; private set; }
        public bool Enabled { get; private set; }

        public void EnableBus()
        {
            Enabled = true;
        }

        public void DisableBus()
        {
            Enabled = false;
        }
    }
}
