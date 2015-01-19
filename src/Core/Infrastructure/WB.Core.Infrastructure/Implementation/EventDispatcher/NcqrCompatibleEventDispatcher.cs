﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventHandlers;

namespace WB.Core.Infrastructure.Implementation.EventDispatcher
{
    public class NcqrCompatibleEventDispatcher : IEventDispatcher
    {
        private readonly Dictionary<Type, EventHandlerWrapper> registredHandlers = new Dictionary<Type, EventHandlerWrapper>();
        private readonly Type[] handlersToIgnore;
        private readonly Func<InProcessEventBus> getInProcessEventBus;
        private readonly IEventStore eventStore;

        public NcqrCompatibleEventDispatcher(IEventStore eventStore, IEnumerable<Type> handlersToIgnore)
        {
            this.eventStore = eventStore;
            this.handlersToIgnore = handlersToIgnore.ToArray();
            this.getInProcessEventBus = () => new InProcessEventBus(true, eventStore);
        }

        internal NcqrCompatibleEventDispatcher(Func<InProcessEventBus> getInProcessEventBus, IEventStore eventStore, IEnumerable<Type> handlersToIgnore)
        {
            this.getInProcessEventBus = getInProcessEventBus;
            this.eventStore = eventStore;
            this.handlersToIgnore = handlersToIgnore.ToArray();
        }

        public void Publish(IPublishableEvent eventMessage)
        {
            var occurredExceptions = new List<Exception>();

            foreach (EventHandlerWrapper handler in this.registredHandlers.Values.ToList())
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
                    string.Format("{0} handler(s) failed to handle published event '{1}' by event source '{2}' with sequence '{3}'.", occurredExceptions.Count, eventMessage.EventIdentifier, eventMessage.EventSourceId, eventMessage.EventSequence),
                    occurredExceptions);
        }

        public void Publish(IEnumerable<IPublishableEvent> eventMessages)
        {
            List<IPublishableEvent> events = eventMessages.ToList();

            if(!events.Any())
                return;

            var functionalHandlers =
               this.registredHandlers.Values.Select(h => h.Handler as IFunctionalEventHandler).Where(h => h != null).ToList();

            var oldStyleHandlers =
               this.registredHandlers.Values.Where(h => !(h.Handler is IFunctionalEventHandler)).ToList();

            Guid firstEventSourceId = events.First().EventSourceId;

            var errorsDuringHandling = new List<Exception>();

            foreach (var functionalEventHandler in functionalHandlers)
            {
                try
                {
                    functionalEventHandler.Handle(events, firstEventSourceId);
                }
                catch (Exception exception)
                {
                    errorsDuringHandling.Add(exception);
                }
            }

            foreach (var publishableEvent in events)
            {
                foreach (var handler in oldStyleHandlers)
                {
                    try
                    {
                        handler.Bus.Publish(publishableEvent);
                    }
                    catch (Exception exception)
                    {
                        errorsDuringHandling.Add(exception);
                    }
                }
            }

            if (errorsDuringHandling.Count > 0)
                throw new AggregateException(
                    string.Format("One or more handlers failed when publishing {0} events. First event source id: {1}.",
                        events.Count, firstEventSourceId.FormatGuid()),
                    errorsDuringHandling);
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
                     string.Format("{0} handler(s) failed to handle published event '{1}' by event source '{2}' with sequence '{3}'.", occurredExceptions.Count, eventMessage.EventIdentifier, eventMessage.EventSourceId, eventMessage.EventSequence),
                     occurredExceptions);
        }

        public void Register(IEventHandler handler)
        {
            if (handlersToIgnore.Any(h => h.IsAssignableFrom(handler.GetType())))
                return;

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