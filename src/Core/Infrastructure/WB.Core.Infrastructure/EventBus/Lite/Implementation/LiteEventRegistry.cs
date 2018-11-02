using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.Infrastructure.EventBus.Lite.Implementation
{
    public class LiteEventRegistry : ILiteEventRegistry
    {
        private readonly Dictionary<string, HashSet<LiteEventRegistryEntity>> handlers = new Dictionary<string, HashSet<LiteEventRegistryEntity>>();
        private readonly Dictionary<ILiteEventHandler, List<LiteEventRegistryEntity>> handlerCache = new Dictionary<ILiteEventHandler, List<LiteEventRegistryEntity>>();

        private static readonly object LockObject = new object();

        public void Subscribe(ILiteEventHandler handler, string aggregateRootId = null)
        {
            var eventTypes = GetHandledEventTypes(handler);

            foreach (Type eventType in eventTypes)
            {
                RegisterHandlerForEvent(
                    handler, 
                    eventType, 
                    aggregateRootId);
            }
        }

        public void Unsubscribe(ILiteEventHandler handler)
        {
            lock (LockObject)
            {
                if (handlerCache.ContainsKey(handler))
                {
                    var subscriptions = handlerCache[handler];
                    for (int i = 0; i < subscriptions.Count; i++)
                    {
                        subscriptions[i].EventHandler.SetTarget(null);
                    }
                }

                handlerCache.Remove(handler);
            }
        }

        public bool IsSubscribed(ILiteEventHandler handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            return handlers.Values.Any(x => x.Any(handlerRef =>
            {
                handlerRef.EventHandler.TryGetTarget(out var subscribedHandler);
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
                if (!this.handlers.TryGetValue(eventKey, out var handlersForEventType))
                {
                    return new List<Action<object>>();
                }

                var liteEventRegistryEntities = handlersForEventType
                    .Where(entity => entity.AggreateRootId == null || entity.AggreateRootId == @event.EventSourceId.FormatGuid())
                    .ToList();


                return liteEventRegistryEntities
                    .Select(entity => GetEventHandler(eventKey, entity))
                    .Where(handler => handler != null)
                    .Select(GetActionHandler)
                    .ToList();
            }
        }

        public void RemoveAggregateRoot(string aggregateRootId)
        {
            foreach (var handlerKey in this.handlers.Keys)
            {
                var handler = this.handlers[handlerKey];
                handler.RemoveWhere(x => x.AggreateRootId == aggregateRootId);
            }
        }

        private ILiteEventHandler GetEventHandler(string eventKey, LiteEventRegistryEntity liteEventRegistryEntity)
        {
            if (liteEventRegistryEntity.EventHandler.TryGetTarget(out var handler))
                return handler;

            this.handlers[eventKey].Remove(liteEventRegistryEntity);
            return null;
        }

        private void RegisterHandlerForEvent(ILiteEventHandler handler, Type eventType, string raiseFilter)
        {
            lock (LockObject)
            {
                var handlerKey = GetEventKey(eventType);
                var handlersForEventType = this.handlers.GetOrAdd(handlerKey, () => new HashSet<LiteEventRegistryEntity>());

                var liteEventRegistryEntity = new LiteEventRegistryEntity(handler, raiseFilter);
                handlersForEventType.Add(liteEventRegistryEntity);
                var subscribtionsCache = handlerCache.GetOrAdd(handler, () => new List<LiteEventRegistryEntity>());
                subscribtionsCache.Add(liteEventRegistryEntity);
            }
        }

        static string GetEventKey(Type eventType)
        {
            return eventType.Name;
        }

        private static Action<object> GetActionHandler(ILiteEventHandler handler)
        {
            return @event =>
            {
                var payload = ((CommittedEvent)@event).Payload;
                var eventType = payload.GetType();
                var handlerType = handler.GetType();

                var runtimeMethod = handlerType.GetRuntimeMethod("Handle", new[] {eventType});

                runtimeMethod?.Invoke(handler, new object[] {payload});

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
            return HandledEventTypesCache.GetOrAdd(handler.GetType(), type =>
            {
                return handler
                    .GetType()
                    .GetTypeInfo()
                    .ImplementedInterfaces
                    .Where(t => IsEventHandlerInterface(t) || IsPublishedEventHandlerInterface(t))
                    .Select(GetEventType)
                    .ToArray();
            });
        }

        private static readonly ConcurrentDictionary<Type, Type[]> HandledEventTypesCache = new ConcurrentDictionary<Type, Type[]>();

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
