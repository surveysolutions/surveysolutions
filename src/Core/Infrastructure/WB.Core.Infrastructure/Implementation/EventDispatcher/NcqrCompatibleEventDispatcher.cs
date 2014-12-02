using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventHandlers;

namespace WB.Core.Infrastructure.Implementation.EventDispatcher
{
    public class NcqrCompatibleEventDispatcher : IEventDispatcher
    {
        private readonly Dictionary<Type, EventHandlerWrapper> registredHandlers = new Dictionary<Type, EventHandlerWrapper>();
        private readonly Func<InProcessEventBus> getInProcessEventBus;
        private readonly IEventStore eventStore;

        public NcqrCompatibleEventDispatcher(IEventStore eventStore)
        {
            this.eventStore = eventStore;
            this.getInProcessEventBus = () => new InProcessEventBus(true, eventStore);
        }

        internal NcqrCompatibleEventDispatcher(Func<InProcessEventBus> getInProcessEventBus, IEventStore eventStore)
        {
            this.getInProcessEventBus = getInProcessEventBus;
            this.eventStore = eventStore;
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

            var functionalHandlers =
               this.registredHandlers.Values.Select(h => h.Handler as IFunctionalEventHandler).Where(h => h != null).ToList();

            var oldStyleHandlers =
               this.registredHandlers.Values.Where(h => !(h.Handler is IFunctionalEventHandler)).ToList();

            foreach (var functionalEventHandler in functionalHandlers)
            {
                functionalEventHandler.Handle(eventMessages, eventMessages.First().EventSourceId);
            }

            foreach (var publishableEvent in eventMessages)
            {
                foreach (var handler in oldStyleHandlers)
                {
                    handler.Bus.Publish(publishableEvent);
                }
            }
        }

        public void PublishUncommitedEventsFromAggregateRoot(IAggregateRoot aggregateRoot, string origin)
        {
            var eventStream = new UncommittedEventStream(origin);

            foreach (UncommittedEvent @event in aggregateRoot.GetUncommittedChanges())
            {
                eventStream.Append(@event);
            }

            this.eventStore.Store(eventStream);
            this.Publish(eventStream);

            aggregateRoot.MarkChangesAsCommitted();
        }

        public void PublishEventToHandlers(IPublishableEvent eventMessage, IEnumerable<IEventHandler> handlers)
        {
            var handlersToPublishEvent = this.GetListOfBusesForRebuild(handlers).ToList();

            var occurredExceptions = new List<Exception>();
            foreach (var bus in handlersToPublishEvent)
            {
                try
                {
                    bus.Publish(eventMessage);
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

        public void Register(IEventHandler handler)
        {
            var inProcessBus = this.getInProcessEventBus();
            IEnumerable<Type> ieventHandlers = handler.GetType().GetTypeInfo().ImplementedInterfaces.Where(IsIEventHandlerInterface);
            foreach (Type ieventHandler in ieventHandlers)
            {
                inProcessBus.RegisterHandler(handler, ieventHandler.GenericTypeArguments[0]);
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
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsInterface && typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof (IEventHandler<>);
        }

        private IEnumerable<InProcessEventBus> GetListOfBusesForRebuild(IEnumerable<IEventHandler> enabledHandlers)
        {
            return this.registredHandlers.Values.Where(h => enabledHandlers.Contains(h.Handler)).Select(h => h.Bus);
        }
    }
}