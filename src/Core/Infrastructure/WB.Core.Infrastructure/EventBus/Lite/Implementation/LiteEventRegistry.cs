using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.Infrastructure.EventBus.Lite.Implementation
{
    public class LiteEventRegistry : ILiteEventRegistry
    {
        private readonly ConcurrentDictionary<string, List<WeakReference<ILiteEventHandler>>> handlers = new ConcurrentDictionary<string, List<WeakReference<ILiteEventHandler>>>();
        
        private static readonly object LockObject = new object();

        public void Subscribe(ILiteEventHandler handler, string eventSourceId)
        {
            var eventTypes = GetHandledEventTypes(handler);

            foreach (Type eventType in eventTypes)
            {
                RegisterHandlerForEvent(handler, eventType, eventSourceId);
            }
        }

        public void Unsubscribe(ILiteEventHandler handler, string eventSourceId)
        {
            var eventTypes = GetHandledEventTypes(handler);

            foreach (Type eventType in eventTypes)
            {
                UnregisterHandlerForEvent(eventType, handler, eventSourceId);
            }
        }

        public IEnumerable<Action<object>> GetHandlers(UncommittedEvent @event)
        {
            Type eventType = @event.Payload.GetType();
            string eventKey = GetEventKey(eventType, @event.EventSourceId.FormatGuid());

            lock (LockObject)
            {
                List<WeakReference<ILiteEventHandler>> handlersForEventType;
                if (!handlers.TryGetValue(eventKey, out handlersForEventType))
                {
                    return Enumerable.Empty<Action<object>>();
                }

                var actualHandlers = GetExistingHandlers(eventKey, handlersForEventType);

                return actualHandlers.Select(GetActionHandler);
            }
        }

        private void RegisterHandlerForEvent(ILiteEventHandler handler, Type eventType, string eventSourceId)
        {
            lock (LockObject)
            {
                var handlerKey = GetEventKey(eventType, eventSourceId);
                List<WeakReference<ILiteEventHandler>> handlersForEventType = this.handlers.GetOrAdd(handlerKey, new List<WeakReference<ILiteEventHandler>>());

                if (IsHandlerAlreadySubscribed(handler, handlersForEventType))
                    throw new InvalidOperationException("This handler {0} already subscribed to event {1}".FormatString(handler.ToString(), eventType.Name));

                handlersForEventType.Add(new WeakReference<ILiteEventHandler>(handler));
            }
        }

        static string GetEventKey(Type eventType, string eventSourceId)
        {
            return eventType.Name + "$" + eventSourceId;
        }

        private void UnregisterHandlerForEvent(Type eventType, ILiteEventHandler handler, string eventSourceId)
        {
            lock (LockObject)
            {
                var eventName = GetEventKey(eventType, eventSourceId);
                if (this.handlers.ContainsKey(eventName))
                {
                    this.handlers[eventName].RemoveAll(registeredHandler => ShouldRemoveHandler(registeredHandler, handler));
                }
            }
        }

        private static bool ShouldRemoveHandler(WeakReference<ILiteEventHandler> handlerWeakReference, ILiteEventHandler unregisteringHandler)
        {
            ILiteEventHandler handlerFromWeakReference;
            var handlerNoLongerExists = !handlerWeakReference.TryGetTarget(out handlerFromWeakReference);

            return handlerNoLongerExists || unregisteringHandler == handlerFromWeakReference;
        }

        private IEnumerable<ILiteEventHandler> GetExistingHandlers(string eventKey, List<WeakReference<ILiteEventHandler>> handlersForEventType)
        {
            var registeredHandlers = handlersForEventType.ToList();

            foreach (var weakReference in registeredHandlers)
            {
                ILiteEventHandler handlerFromWeakReferance;

                if (weakReference.TryGetTarget(out handlerFromWeakReferance))
                {
                    yield return handlerFromWeakReferance;
                }
                else
                {
                    this.handlers[eventKey].Remove(weakReference);
                }
            }
        }

        private static bool IsHandlerAlreadySubscribed(ILiteEventHandler handler, List<WeakReference<ILiteEventHandler>> handlersForEventType)
        {
            ILiteEventHandler handlerFromWeakReferance;

            return handlersForEventType.Any(h => h.TryGetTarget(out handlerFromWeakReferance) && handlerFromWeakReferance == handler);
        }

        private static Action<object> GetActionHandler(ILiteEventHandler handler)
        {
            return @event => ((dynamic) handler).Handle((dynamic) @event);
        }

        private static Type[] GetHandledEventTypes(ILiteEventHandler handler)
        {
            return handler
                .GetType()
                .GetTypeInfo()
                .ImplementedInterfaces
                .Where(IsEventHandlerInterface)
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
    }
}