﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Ncqrs;
using Ncqrs.Domain;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;

using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.Infrastructure.Implementation.EventDispatcher
{
    public class NcqrCompatibleEventDispatcher : IEventDispatcher
    {
        private readonly Dictionary<Type, EventHandlerWrapper> registredHandlers = new Dictionary<Type, EventHandlerWrapper>();
        private readonly Type[] handlersToIgnore;
        private readonly Func<InProcessEventBus> getInProcessEventBus;
        private readonly IEventStore eventStore;
        private readonly EventBusSettings eventBusSettings;
        private readonly ILogger logger;

        public ITransactionManagerProvider TransactionManager { get; set; }

        public NcqrCompatibleEventDispatcher(IEventStore eventStore, EventBusSettings eventBusSettings, ILogger logger)
        {
            this.eventStore = eventStore;
            this.eventBusSettings = eventBusSettings;
            this.logger = logger;
            this.handlersToIgnore = eventBusSettings.DisabledEventHandlerTypes;
            this.getInProcessEventBus = () => new InProcessEventBus(eventStore, eventBusSettings, logger);
        }

        public event EventHandlerExceptionDelegate OnCatchingNonCriticalEventHandlerException;

        public void Publish(IPublishableEvent eventMessage)
        {
            var occurredExceptions = new List<Exception>();

            foreach (EventHandlerWrapper handler in this.registredHandlers.Values.ToList())
            {
                handler.Bus.OnCatchingNonCriticalEventHandlerException +=
                        this.OnCatchingNonCriticalEventHandlerException;
                try
                {
                    handler.Bus.Publish(eventMessage);
                }
                catch (Exception exception)
                {
                    occurredExceptions.Add(exception);
                }
                finally
                {
                    handler.Bus.OnCatchingNonCriticalEventHandlerException -=
                        this.OnCatchingNonCriticalEventHandlerException;
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

            if (!events.Any())
                return;

            var functionalHandlers =
               this.registredHandlers.Values.Where(h => (h.Handler as IFunctionalEventHandler) != null).ToList();

            var oldStyleHandlers =
               this.registredHandlers.Values.Except(functionalHandlers).ToList();

            Guid firstEventSourceId = events.First().EventSourceId;

            if (this.eventBusSettings.IgnoredAggregateRoots.Contains(firstEventSourceId.FormatGuid()))
                return;

            var errorsDuringHandling = new List<Exception>();

            foreach (var functionalEventHandler in functionalHandlers)
            {
                var handler = (IFunctionalEventHandler) functionalEventHandler.Handler;

                functionalEventHandler.Bus.OnCatchingNonCriticalEventHandlerException +=
                        this.OnCatchingNonCriticalEventHandlerException;
                try
                {
                    this.TransactionManager.GetTransactionManager().BeginCommandTransaction();
                    handler.Handle(events, firstEventSourceId);
                    this.TransactionManager.GetTransactionManager().CommitCommandTransaction();
                }
                catch (Exception exception)
                {
                    var eventHandlerType = handler.GetType();
                    var shouldIgnoreException =
                        this.eventBusSettings.EventHandlerTypesWithIgnoredExceptions.Contains(eventHandlerType);

                    var eventHandlerException = new EventHandlerException(eventHandlerType: eventHandlerType,
                        eventType: events.First().GetType(), isCritical: !shouldIgnoreException,
                        innerException: exception);

                    if (shouldIgnoreException)
                    {
                        this.logger.Error(
                            $"Failed to handle {eventHandlerException.EventType.Name} in {eventHandlerException.EventHandlerType} by event source '{firstEventSourceId}'.",
                            eventHandlerException);

                        this.OnCatchingNonCriticalEventHandlerException?.Invoke(
                            eventHandlerException);
                    }
                    else
                    {
                        errorsDuringHandling.Add(eventHandlerException);
                    }

                    this.TransactionManager.GetTransactionManager().RollbackCommandTransaction();
                }
                finally
                {
                    functionalEventHandler.Bus.OnCatchingNonCriticalEventHandlerException -=
                        this.OnCatchingNonCriticalEventHandlerException;
                }
            }

            foreach (var publishableEvent in events)
            {
                foreach (var handler in oldStyleHandlers)
                {
                    if(!handler.Bus.CanHandleEvent(publishableEvent))
                        continue;
					
                    handler.Bus.OnCatchingNonCriticalEventHandlerException +=
                        this.OnCatchingNonCriticalEventHandlerException;
                    try
                    {
                        this.TransactionManager.GetTransactionManager().BeginCommandTransaction();
                        handler.Bus.Publish(publishableEvent);
                        this.TransactionManager.GetTransactionManager().CommitCommandTransaction();
                    }
                    catch (Exception exception)
                    {
                        errorsDuringHandling.Add(exception);
                        this.TransactionManager.GetTransactionManager().RollbackCommandTransaction();
                    }
                    finally
                    {
                        handler.Bus.OnCatchingNonCriticalEventHandlerException -=
                            this.OnCatchingNonCriticalEventHandlerException;
                    }
                }
            }

            if (errorsDuringHandling.Count > 0)
                throw new AggregateException(
                    string.Format("One or more handlers failed when publishing {0} events. First event source id: {1}.",
                        events.Count, firstEventSourceId.FormatGuid()),
                    errorsDuringHandling);
        }

        public CommittedEventStream CommitUncommittedEvents(IEventSourcedAggregateRoot aggregateRoot, string origin)
        {
            var eventStream = new UncommittedEventStream(origin, aggregateRoot.GetUnCommittedChanges());
            return this.eventStore.Store(eventStream);
        }

        public void PublishCommittedEvents(CommittedEventStream committedEvents)
        {
            this.Publish(committedEvents);
        }

        public void PublishEventToHandlers(IPublishableEvent eventMessage,
            IReadOnlyDictionary<IEventHandler, Stopwatch> handlersWithStopwatch)
        {
            var occurredExceptions = new ConcurrentBag<Exception>();

            foreach (var handlerWithStopwatch in handlersWithStopwatch)
            {
                this.PublishEventToHandlerWithStopwatch(eventMessage, handlerWithStopwatch.Key, handlerWithStopwatch.Value, occurredExceptions);
            }

            if (occurredExceptions.Count > 0)
                throw new AggregateException(
                    $"{occurredExceptions.Count} handler(s) failed to handle published event '{eventMessage.EventIdentifier}' by event source '{eventMessage.EventSourceId}' with sequence '{eventMessage.EventSequence}'.",
                    occurredExceptions);
        }

        private void PublishEventToHandlerWithStopwatch(IPublishableEvent eventMessage, IEventHandler handler, Stopwatch stopwatch, ConcurrentBag<Exception> occurredExceptions)
        {
            var handlerType = handler.GetType();

            if (!this.registredHandlers.ContainsKey(handlerType))
                return;

            var bus = this.registredHandlers[handlerType].Bus;

            stopwatch.Start();

            bus.OnCatchingNonCriticalEventHandlerException += this.OnCatchingNonCriticalEventHandlerException;
            try
            {
                this.TransactionManager.GetTransactionManager().BeginCommandTransaction();
                bus.Publish(eventMessage);
                this.TransactionManager.GetTransactionManager().CommitCommandTransaction();
            }
            catch (Exception exception)
            {
                occurredExceptions.Add(exception);
                this.TransactionManager.GetTransactionManager().RollbackCommandTransaction();
            }
            finally
            {
                bus.OnCatchingNonCriticalEventHandlerException -= this.OnCatchingNonCriticalEventHandlerException;
            }

            stopwatch.Stop();
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

        public IEventHandler[] GetAllRegistredEventHandlers()
        {
            return this.registredHandlers.Values.Select(v => v.Handler).ToArray();
        }

        private static bool IsIEventHandlerInterface(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsInterface && typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof (IEventHandler<>);
        }
    }
}