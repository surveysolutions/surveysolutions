using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ncqrs;
using WB.Core.GenericSubdomains.Utils;


namespace WB.Core.Infrastructure.EventBus.Lite.Implementation
{
    public class EventRegistry : IEventRegistry
    {
        private class Subscription
        {
            public object Pointer { get; set; }

            public Action<object> MethodAction { get; set; }
        }

        private readonly string registerHandlerMethodName = Reflect<EventRegistry>.MethodName(r => new Action<object>(r.RegisterHandler<object>));

        private readonly Dictionary<Type, List<Subscription>> handlers = new Dictionary<Type, List<Subscription>>();
        
        private static readonly object LockObject = new object();


        public void Subscribe(IEventBusEventHandler handler)
        {
            var eventTypes = GetHandledEventTypes(handler);
            RegisterHandlersForEvents(handler, eventTypes);
        }


        public void Unsubscribe(IEventBusEventHandler handler)
        {
            foreach (Type type in GetHandledEventTypes(handler))
            {
                lock (LockObject)
                {
                    if (this.handlers.ContainsKey(type))
                    {
                        handlers[type].RemoveAll(s => s.Pointer == handler);
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
                if (handlers.TryGetValue(eventType, out subscriptionsList))
                    return subscriptionsList.Select(s => s.MethodAction).ToList();

                return Enumerable.Empty<Action<object>>();
            }
        }

        private void RegisterHandler<TEvent>(object handler)
        {
            var eventType = typeof(TEvent);
            var eventBusEventHandler = (IEventBusEventHandler<TEvent>) handler;

            lock (LockObject)
            {
                List<Subscription> handlersList = this.handlers.GetOrAdd(eventType, () => new List<Subscription>());

                handlersList.Add(new Subscription()
                {
                    Pointer = handler,
                    MethodAction = (obj => eventBusEventHandler.Handle((TEvent)obj))
                });
            }
        }

        private void RegisterHandlersForEvents(object handler, Type[] eventTypes)
        {
            MethodInfo method = typeof(EventRegistry).GetMethod(registerHandlerMethodName);

            foreach (Type eventType in eventTypes)
            {
                MethodInfo genericMethod = method.MakeGenericMethod(eventType);
                genericMethod.Invoke(this, new[] { handler });
            }
        }

        private static Type[] GetHandledEventTypes(object handler)
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
            return i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventBusEventHandler<>);
        }
    }
}