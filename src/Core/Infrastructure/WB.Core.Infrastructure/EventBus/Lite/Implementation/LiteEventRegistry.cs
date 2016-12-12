using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.CustomCollections;
using WB.Core.Infrastructure.EventBus.Lite.Implementation.RaiseFilters;

namespace WB.Core.Infrastructure.EventBus.Lite.Implementation
{
    public class LiteEventRegistry : ILiteEventRegistry
    {
        private readonly Dictionary<string, ConcurrentHashSet<LiteEventRegistryEntity>> handlers = new Dictionary<string, ConcurrentHashSet<LiteEventRegistryEntity>>();

        private static readonly object LockObject = new object();

        public void Subscribe(ILiteEventHandler handler, string aggregateRootId = null)
        {
            var eventTypes = GetHandledEventTypes(handler);

            foreach (Type eventType in eventTypes)
            {
                RegisterHandlerForEvent(
                    handler, 
                    eventType, 
                    aggregateRootId != null ? new AggregateRootRaiseFilter(aggregateRootId) : null);
            }
        }

        public void Unsubscribe(ILiteEventHandler handler)
        {
            var eventTypes = GetHandledEventTypes(handler);

            foreach (Type eventType in eventTypes)
            {
                UnregisterHandlerForEvent(eventType, handler);
            }
        }

        public bool IsSubscribed(ILiteEventHandler handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            return handlers.Values.Any(x => x.Any(handlerRef =>
            {
                ILiteEventHandler subscribedHandler;
                handlerRef.EventHandler.TryGetTarget(out subscribedHandler);
                if (subscribedHandler == null)
                    return false;
                return ReferenceEquals(subscribedHandler, handler);
            }));
        }

        public IReadOnlyCollection<Action<object>> GetHandlers(CommittedEvent @event)
        {
            Type eventType = @event.Payload.GetType();
            string eventKey = GetEventKey(eventType);

            lock (LockObject)
            {
                ConcurrentHashSet<LiteEventRegistryEntity> handlersForEventType;
                if (!this.handlers.TryGetValue(eventKey, out handlersForEventType))
                {
                    return new List<Action<object>>();
                }

                return handlersForEventType
                    .ToReadOnlyCollection()
                    .Where(entity => entity.Filter == null || entity.Filter.IsNeedRaise(@event))
                    .Select(entity => GetEventHandler(eventKey, entity))
                    .Where(handler => handler != null)
                    .Select(GetActionHandler)
                    .ToList();
            }
        }

        private ILiteEventHandler GetEventHandler(string eventKey, LiteEventRegistryEntity liteEventRegistryEntity)
        {
            ILiteEventHandler handler;
            if (liteEventRegistryEntity.EventHandler.TryGetTarget(out handler))
                return handler;

            this.handlers[eventKey].Remove(liteEventRegistryEntity);
            return null;
        }

        private void RegisterHandlerForEvent(ILiteEventHandler handler, Type eventType, ILiteEventRaiseFilter raiseFilter)
        {
            lock (LockObject)
            {
                var handlerKey = GetEventKey(eventType);
                ICollection<LiteEventRegistryEntity> handlersForEventType = this.handlers.GetOrAdd(handlerKey, () => new ConcurrentHashSet<LiteEventRegistryEntity>());

                if (IsHandlerAlreadySubscribed(handler, handlersForEventType))
                    throw new InvalidOperationException("This handler {0} already subscribed to event {1}".FormatString(handler.ToString(), eventType.Name));

                var liteEventRegistryEntity = new LiteEventRegistryEntity(handler, raiseFilter);
                handlersForEventType.Add(liteEventRegistryEntity);
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
                    ICollection<LiteEventRegistryEntity> subscribedRefences = this.handlers[eventName];
                    foreach (var weakReference in subscribedRefences)
                    {
                        if (ShouldRemoveHandler(weakReference, handler))
                        {
                            subscribedRefences.Remove(weakReference);
                        }
                    }
                }
            }
        }

        private static bool ShouldRemoveHandler(LiteEventRegistryEntity handlerWeakReference, ILiteEventHandler unregisteringHandler)
        {
            ILiteEventHandler handlerFromWeakReference;
            var handlerNoLongerExists = !handlerWeakReference.EventHandler.TryGetTarget(out handlerFromWeakReference);

            return handlerNoLongerExists || unregisteringHandler == handlerFromWeakReference;
        }

        private static bool IsHandlerAlreadySubscribed(ILiteEventHandler handler, IEnumerable<LiteEventRegistryEntity> handlersForEventType)
        {
            ILiteEventHandler handlerFromWeakReference;

            return handlersForEventType.Any(h => h.EventHandler.TryGetTarget(out handlerFromWeakReference) && handlerFromWeakReference == handler);
        }

        private static Action<object> GetActionHandler(ILiteEventHandler handler)
        {
            return @event =>
            {
                var payload = ((CommittedEvent)@event).Payload;
                var eventType = payload.GetType();
                var handlerType = handler.GetType();

                if (handlerType.GetRuntimeMethod("Handle", new[] {eventType}) != null)
                    ((dynamic) handler).Handle((dynamic) payload);

                var publishedEventInterfaceType = typeof(IPublishedEvent<>).MakeGenericType(eventType);
                var methodInfoForPublishedEvent = handlerType.GetRuntimeMethod("Handle", new[] { publishedEventInterfaceType });
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