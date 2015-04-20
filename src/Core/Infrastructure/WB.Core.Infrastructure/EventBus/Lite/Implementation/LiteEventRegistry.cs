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
        private class Subscription
        {
            public ILiteEventBusEventHandler HandlerRef { get; set; }

            public Action<object> HandlerAction { get; set; }
        }

        private readonly Dictionary<string, List<Subscription>> handlers = new Dictionary<string, List<Subscription>>();
        
        private static readonly object LockObject = new object();


        public void Subscribe(ILiteEventBusEventHandler handler)
        {
            var eventTypes = GetHandledEventTypes(handler);
            RegisterHandlersForEvents(handler, eventTypes);
        }


        public void Unsubscribe(ILiteEventBusEventHandler handler)
        {
            var eventTypes = GetHandledEventTypes(handler);
            UnregisterHandlersForEvents(handler, eventTypes);
        }

        private void UnregisterHandlersForEvents(ILiteEventBusEventHandler handler, Type[] eventTypes)
        {
            foreach (Type eventType in eventTypes)
            {
                var eventTypeName = eventType.Name;

                lock (LockObject)
                {
                    if (this.handlers.ContainsKey(eventTypeName))
                    {
                        this.handlers[eventTypeName].RemoveAll(s => s.HandlerRef == handler);
                    }
                }
            }
        }

        public IEnumerable<Action<object>> GetHandlers(object @event)
        {
            Type eventType = @event.GetType();

            lock (LockObject)
            {
                List<Subscription> subscriptionsList;
                if (handlers.TryGetValue(eventType.Name, out subscriptionsList))
                    return subscriptionsList.Select(s => s.HandlerAction).ToList();

                return Enumerable.Empty<Action<object>>();
            }
        }

        private void RegisterHandlersForEvents(ILiteEventBusEventHandler handler, Type[] eventTypes)
        {
            foreach (Type eventType in eventTypes)
            {
                lock (LockObject)
                {
                    List<Subscription> handlersList = this.handlers.GetOrAdd(eventType.Name, () => new List<Subscription>());

                    if (handlersList.Any(h => h.HandlerRef == handler))
                        throw new InvalidOperationException("This handler {0} already subscribed to event {1}".FormatString(handler.ToString(), eventType.Name));

                    handlersList.Add(new Subscription
                    {
                        HandlerRef = handler,
                        HandlerAction = @event => ((dynamic)handler).Handle((dynamic)@event)
                    });
                }
            }
        }

        private static Type[] GetHandledEventTypes(ILiteEventBusEventHandler handler)
        {
            return handler
                .GetType()
                .GetTypeInfo()
                .ImplementedInterfaces
                .Where(IsEventHandlerInterface)
                .Select(GetEventType)
                .ToArray();
        }

        private static Type GetEventType(Type k)
        {
            return k.GetTypeInfo().GenericTypeArguments.Single();
        }

        private static bool IsEventHandlerInterface(Type i)
        {
            return i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(ILiteEventBusEventHandler<>);
        }
    }
}