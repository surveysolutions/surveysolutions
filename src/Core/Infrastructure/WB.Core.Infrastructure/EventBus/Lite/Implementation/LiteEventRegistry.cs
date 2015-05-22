using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.Infrastructure.EventBus.Lite.Implementation
{
    public class LiteEventRegistry : ILiteEventRegistry
    {
        private readonly Dictionary<string, List<WeakReference<ILiteEventHandler>>> handlers = new Dictionary<string, List<WeakReference<ILiteEventHandler>>>();
        
        private static readonly object LockObject = new object();

        public void Subscribe(ILiteEventHandler handler)
        {
            var eventTypes = GetHandledEventTypes(handler);

            foreach (Type eventType in eventTypes)
            {
                RegisterHandlerForEvent(handler, eventType);
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

        public IEnumerable<Action<object>> GetHandlers(object @event)
        {
            Type eventType = @event.GetType();

            lock (LockObject)
            {
                List<WeakReference<ILiteEventHandler>> handlersForEventType;
                if (!handlers.TryGetValue(eventType.Name, out handlersForEventType))
                    return Enumerable.Empty<Action<object>>();

                var actualHandlers = GetExistingHandlers(@event, handlersForEventType);

                return actualHandlers.Select(GetActionHandler);
            }
        }

        private void RegisterHandlerForEvent(ILiteEventHandler handler, Type eventType)
        {
            lock (LockObject)
            {
                List<WeakReference<ILiteEventHandler>> handlersForEventType = this.handlers.GetOrAdd(eventType.Name, () => new List<WeakReference<ILiteEventHandler>>());

                if (IsHandlerAlreadySubscribed(handler, handlersForEventType))
                    throw new InvalidOperationException("This handler {0} already subscribed to event {1}".FormatString(handler.ToString(), eventType.Name));

                handlersForEventType.Add(new WeakReference<ILiteEventHandler>(handler));
            }
        }

        private void UnregisterHandlerForEvent(Type eventType, ILiteEventHandler handler = null)
        {
            lock (LockObject)
            {
                if (this.handlers.ContainsKey(eventType.Name))
                {
                    this.handlers[eventType.Name].RemoveAll(registeredHandler => ShouldRemoveHandler(registeredHandler, handler));
                }
            }
        }

        private static bool ShouldRemoveHandler(WeakReference<ILiteEventHandler> handlerWeakReference, ILiteEventHandler unregisteringHandler)
        {
            ILiteEventHandler handlerFromWeakReference;
            var handlerNoLongerExists = !handlerWeakReference.TryGetTarget(out handlerFromWeakReference);

            return handlerNoLongerExists || unregisteringHandler == handlerFromWeakReference;
        }

        private IEnumerable<ILiteEventHandler> GetExistingHandlers(object @event, List<WeakReference<ILiteEventHandler>> handlersForEventType)
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
                    this.handlers[@event.GetType().Name].Remove(weakReference);
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