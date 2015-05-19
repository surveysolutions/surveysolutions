using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ncqrs;
using WB.Core.GenericSubdomains.Utils;


namespace WB.Core.Infrastructure.EventBus.Lite.Implementation
{
    public class LiteEventRegistry : ILiteEventRegistry
    {
        private readonly Dictionary<string, List<WeakReference<ILiteEventHandler>>> handlers = new Dictionary<string, List<WeakReference<ILiteEventHandler>>>();
        
        private static readonly object LockObject = new object();

        public void Subscribe(ILiteEventHandler handler)
        {
            var eventTypes = GetHandledEventTypes(handler);
            RegisterHandlersForEvents(handler, eventTypes);
        }

        public void Unsubscribe(ILiteEventHandler handler)
        {
            var eventTypes = GetHandledEventTypes(handler);
            UnregisterHandlerForEvents(eventTypes, handler);
        }

        private void UnregisterHandlerForEvents(Type[] eventTypes, ILiteEventHandler handler)
        {
            foreach (Type eventType in eventTypes)
            {
                UnregisterHandlerForEvent(eventType, handler);
            }
        }

        private void UnregisterHandlerForEvent(Type eventType, ILiteEventHandler handler = null)
        {
            var eventTypeName = eventType.Name;

            lock (LockObject)
            {
                if (this.handlers.ContainsKey(eventTypeName))
                {
                    this.handlers[eventTypeName].RemoveAll(s =>
                    {
                        ILiteEventHandler handlerFromWeakReferance;
                        if (!s.TryGetTarget(out handlerFromWeakReferance))
                            return true; // remove if handler doesn't exist

                        return handler == handlerFromWeakReferance;
                    });
                }
            }
        }

        public IEnumerable<Action<object>> GetHandlers(object @event)
        {
            Type eventType = @event.GetType();

            lock (LockObject)
            {
                List<WeakReference<ILiteEventHandler>> handlersForEventType;
                if (handlers.TryGetValue(eventType.Name, out handlersForEventType))
                {
                    bool wasNotActualHandlers;
                    var actualHandlers = FilterActualHandlers(handlersForEventType, out wasNotActualHandlers);

                    if (wasNotActualHandlers)
                        UnregisterHandlerForEvent(@event.GetType());

                    return actualHandlers.Select(GetActionHandler);
                }

                return Enumerable.Empty<Action<object>>();
            }
        }

        private Action<object> GetActionHandler(ILiteEventHandler handler)
        {
            return @event => ((dynamic) handler).Handle((dynamic) @event);
        }

        private static IEnumerable<ILiteEventHandler> FilterActualHandlers(List<WeakReference<ILiteEventHandler>> weakReferenceHandlers, out bool wasNotActualHandlers)
        {
            wasNotActualHandlers = false;
            List<ILiteEventHandler> handlersForEvent = new List<ILiteEventHandler>();

            foreach (var weakReferance in weakReferenceHandlers)
            {
                ILiteEventHandler handlerFromWeakReferance;

                if (weakReferance.TryGetTarget(out handlerFromWeakReferance))
                    handlersForEvent.Add(handlerFromWeakReferance);
                else
                    wasNotActualHandlers = true;
            }

            return handlersForEvent;
        }

        private void RegisterHandlersForEvents(ILiteEventHandler handler, Type[] eventTypes)
        {
            foreach (Type eventType in eventTypes)
            {
                lock (LockObject)
                {
                    List<WeakReference<ILiteEventHandler>> handlersForEventType = this.handlers.GetOrAdd(eventType.Name, () => new List<WeakReference<ILiteEventHandler>>());

                    ILiteEventHandler handlerFromWeakReferance;

                    if (handlersForEventType.Any(h => h.TryGetTarget(out handlerFromWeakReferance) && handlerFromWeakReferance == handler))
                        throw new InvalidOperationException("This handler {0} already subscribed to event {1}".FormatString(handler.ToString(), eventType.Name));

                    handlersForEventType.Add(new WeakReference<ILiteEventHandler>(handler)); 
                }
            }
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