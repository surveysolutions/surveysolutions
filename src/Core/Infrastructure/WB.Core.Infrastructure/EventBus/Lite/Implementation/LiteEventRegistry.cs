using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.CustomCollections;
using WB.Core.Infrastructure.EventBus.Lite.Implementation.RaiseFilters;

namespace WB.Core.Infrastructure.EventBus.Lite.Implementation
{
    public class LiteEventRegistry : ILiteEventRegistry
    {
        private readonly ConcurrentDictionary<string, ConcurrentHashSet<WeakReference<ILiteEventHandler>>> handlers = new ConcurrentDictionary<string, ConcurrentHashSet<WeakReference<ILiteEventHandler>>>();

        private static readonly object LockObject = new object();

        public void Subscribe(ILiteEventHandler handler, string aggregateRootId = null)
        {
            var eventTypes = GetHandledEventTypes(handler);

            foreach (Type eventType in eventTypes)
            {
                RegisterHandlerForEvent(handler, eventType, aggregateRootId != null ? new AggregateRootRaiseFilter(aggregateRootId) : null);
            }
        }

        public void Unsubscribe(ILiteEventHandler handler, string aggregateRootId = null)
        {
            var eventTypes = GetHandledEventTypes(handler);

            foreach (Type eventType in eventTypes)
            {
                UnregisterHandlerForEvent(eventType, handler);
            }
        }

        public bool IsSubscribed(ILiteEventHandler handler, string eventSourceId = null)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            return handlers.Values.Any(x => x.Any(handlerRef =>
            {
                ILiteEventHandler subscribedHandler;
                handlerRef.TryGetTarget(out subscribedHandler);
                return ReferenceEquals(subscribedHandler, handler);
            }));
        }

        public IReadOnlyCollection<Action<object>> GetHandlers(CommittedEvent @event)
        {
            Type eventType = @event.Payload.GetType();
            string eventKey = GetEventKey(eventType);

            lock (LockObject)
            {
                ConcurrentHashSet<WeakReference<ILiteEventHandler>> handlersForEventType;
                if (!this.handlers.TryGetValue(eventKey, out handlersForEventType))
                {
                    return new List<Action<object>>();
                }

                var actualHandlers = GetExistingHandlers(eventKey, handlersForEventType);

                return actualHandlers.Select(GetActionHandler).ToList();
            }
        }

        private void RegisterHandlerForEvent(ILiteEventHandler handler, Type eventType, ILiteEventRaiseFilter raiseFilter)
        {
            lock (LockObject)
            {
                var handlerKey = GetEventKey(eventType);
                ICollection<WeakReference<ILiteEventHandler>> handlersForEventType = this.handlers.GetOrAdd(handlerKey, new ConcurrentHashSet<WeakReference<ILiteEventHandler>>());

                if (IsHandlerAlreadySubscribed(handler, handlersForEventType))
                    throw new InvalidOperationException("This handler {0} already subscribed to event {1}".FormatString(handler.ToString(), eventType.Name));

                handlersForEventType.Add(new WeakReference<ILiteEventHandler>(handler));
            }
        }

        static string GetEventKey(Type eventType)
        {
            return eventType.Name;
        }

        private void UnregisterHandlerForEvent(Type eventType, ILiteEventHandler handler)
        {
            lock (LockObject)
            {
                var eventName = GetEventKey(eventType);
                if (this.handlers.ContainsKey(eventName))
                {
                    ICollection<WeakReference<ILiteEventHandler>> subsribedRefences = this.handlers[eventName];
                    foreach (var weakReference in subsribedRefences)
                    {
                        if (ShouldRemoveHandler(weakReference, handler))
                        {
                            subsribedRefences.Remove(weakReference);
                        }
                    }
                }
            }
        }

        private static bool ShouldRemoveHandler(WeakReference<ILiteEventHandler> handlerWeakReference, ILiteEventHandler unregisteringHandler)
        {
            ILiteEventHandler handlerFromWeakReference;
            var handlerNoLongerExists = !handlerWeakReference.TryGetTarget(out handlerFromWeakReference);

            return handlerNoLongerExists || unregisteringHandler == handlerFromWeakReference;
        }

        private IEnumerable<ILiteEventHandler> GetExistingHandlers(string eventKey, ICollection<WeakReference<ILiteEventHandler>> handlersForEventType)
        {
            var registeredHandlers = handlersForEventType.ToList();

            foreach (var weakReference in registeredHandlers)
            {
                ILiteEventHandler handlerFromWeakReference;

                if (weakReference.TryGetTarget(out handlerFromWeakReference))
                {
                    yield return handlerFromWeakReference;
                }
                else
                {
                    this.handlers[eventKey].Remove(weakReference);
                }
            }
        }

        private static bool IsHandlerAlreadySubscribed(ILiteEventHandler handler, IEnumerable<WeakReference<ILiteEventHandler>> handlersForEventType)
        {
            ILiteEventHandler handlerFromWeakReference;

            return handlersForEventType.Any(h => h.TryGetTarget(out handlerFromWeakReference) && handlerFromWeakReference == handler);
        }

        private static Action<object> GetActionHandler(ILiteEventHandler handler)
        {
            return @event =>
            {
                var payload = ((CommittedEvent)@event).Payload;
                var eventType = payload.GetType();
                var handlerType = handler.GetType();

                if (handlerType.GetMethod("Handle", new[] {eventType}) != null)
                    ((dynamic) handler).Handle((dynamic) payload);

                var publishedEventInterfaceType = typeof(IPublishedEvent<>).MakeGenericType(eventType);
                var methodInfoForPublishedEvent = handlerType.GetMethod("Handle", new[] { publishedEventInterfaceType });
                if (methodInfoForPublishedEvent != null)
                {
                    var publishedEventType = typeof(PublishedEvent<>).MakeGenericType(eventType);
                    var publishedEvent = Activator.CreateInstance(publishedEventType, @event);
                    methodInfoForPublishedEvent.Invoke(handler, new [] { publishedEvent });
                }
            };
        }

        private static Type[] GetHandledEventTypes(ILiteEventHandler handler)
        {
            return handler
                .GetType()
                .GetTypeInfo()
                .ImplementedInterfaces
                .Where(type => IsEventHandlerInterface(type) || IsPublishedEventHandlerInterface(type))
                .Select(GetEventType)
                .ToArray();
        }

        private static Type GetEventType(Type type)
        {
            return type.GetTypeInfo().GenericTypeArguments.Single();
        }

        private static bool IsEventHandlerInterface(Type type)
        {
            return type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(ILiteEventHandler<>);
        }
        private static bool IsPublishedEventHandlerInterface(Type type)
        {
            return type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(ILitePublishedEventHandler<>);
        }
    }
}