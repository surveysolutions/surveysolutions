using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.StorageStrategy;
using WB.Core.Infrastructure.ReadSide.Repository;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Implementation.EventDispatcher
{
    public class NcqrCompatibleEventDispatcher : IEventDispatcher
    {
        private readonly Dictionary<Type, EventHandlerWrapper> registredHandlers = new Dictionary<Type, EventHandlerWrapper>();
        private readonly Func<InProcessEventBus> getInProcessEventBus;

        public NcqrCompatibleEventDispatcher()
        {
            this.getInProcessEventBus = () => new InProcessEventBus(true);
        }

        internal NcqrCompatibleEventDispatcher(Func<InProcessEventBus> getInProcessEventBus)
        {
            this.getInProcessEventBus = getInProcessEventBus;
        }

        public void Publish(IPublishableEvent eventMessage)
        {
            var occurredExceptions = new List<Exception>();

            foreach (var handler in this.registredHandlers.Values.ToList())
            {
                try
                {
                    handler.Bus.Publish(eventMessage);
                }
                catch (Exception exception)
                {
                    occurredExceptions.Add(exception);
                }
            }

            if (occurredExceptions.Count > 0)
                throw new AggregateException(
                    string.Format("{0} handler(s) failed to handle published event '{1}'.", occurredExceptions.Count, eventMessage.EventIdentifier),
                    occurredExceptions);
        }

        public void Publish(IEnumerable<IPublishableEvent> eventMessages)
        {
            if(!eventMessages.Any())
                return;

            var oldStyleHandlers =
               this.registredHandlers.Values.Where(h => !(h.Handler is IFunctionalEventHandler)).ToList();

            foreach (var publishableEvent in eventMessages)
            {
                foreach (var handler in oldStyleHandlers)
                {
                    handler.Bus.Publish(publishableEvent);
                }
            }

            var functionalHandlers =
               this.registredHandlers.Values.Select(h => h.Handler as IFunctionalEventHandler).Where(h => h != null).ToList();

            foreach (var functionalEventHandler in functionalHandlers)
            {
                functionalEventHandler.Handle(eventMessages, eventMessages.First().EventSourceId);
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
            var inProcessBus = this.getInProcessEventBus();
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