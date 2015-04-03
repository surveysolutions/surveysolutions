using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ncqrs;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;



namespace WB.Core.Infrastructure.EventBus.Implementation
{
    public class EventRegistry : IEventRegistry
    {
        private readonly ILogger logger;

        private readonly ConcurrentDictionary<Type, IEventSubscription> subscriptions = new ConcurrentDictionary<Type, IEventSubscription>();
        private readonly string SubscribeMethodName = Reflect<EventRegistry>.MethodName(c => new Action<IEventBusEventHandler<object>>(c.Subscribe));
        private readonly string UnsubscribeMethodName = Reflect<EventRegistry>.MethodName(c => new Action<IEventBusEventHandler<object>>(c.Unsubscribe));

        public EventRegistry(ILogger logger)
        {
            this.logger = logger;
        }

        public void Subscribe<TEvent>(IEventBusEventHandler<TEvent> handler)
        {
            Subscribe<TEvent>(handler.Handle);
        }

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            var subscription = (EventSubscription<TEvent>)subscriptions.GetOrAdd(
                typeof(TEvent),
                t => new EventSubscription<TEvent>());
            subscription.Subscribe(handler);
        }

        public void Unsubscribe<TEvent>(IEventBusEventHandler<TEvent> handler)
        {
            Unsubscribe<TEvent>(handler.Handle);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            var eventType = typeof (TEvent);
            IEventSubscription subscription;
            if (subscriptions.TryGetValue(eventType, out subscription))
            {
                ((EventSubscription<TEvent>) subscription).Unsubscribe(handler);
            }
            else
            {
                logger.Info("No subscribers for event {0} found.".FormatString(eventType.ToString()));
            }
        }

        public void Subscribe(object obj)
        {
            var eventTypes = GetEventsListToHandleFromClass(obj);
            CallMethodForEvent(SubscribeMethodName, obj, eventTypes);
        }

        public void Unsubscribe(object obj)
        {
            var eventTypes = GetEventsListToHandleFromClass(obj);
            CallMethodForEvent(UnsubscribeMethodName, obj, eventTypes);
        }

        private Type[] GetEventsListToHandleFromClass(object obj)
        {
            Type type = obj.GetType();
            IEnumerable<Type> interfaces = type.GetTypeInfo().ImplementedInterfaces;
            return interfaces
                .Where(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventBusEventHandler<>))
                .Select(k => k.GetTypeInfo().GenericTypeArguments.Single())
                .ToArray();
        }


        private void CallMethodForEvent(string methodName, object obj, Type[] eventTypes)
        {
            MethodInfo method = typeof(EventRegistry).GetMethod(methodName);

            foreach (Type eventType in eventTypes)
            {
                MethodInfo genericMethod = method.MakeGenericMethod(eventType);
                genericMethod.Invoke(this, new[] { obj });
            }
        }

        public IEventSubscription<TEvent> GetSubscription<TEvent>()
        {
            var eventType = typeof(TEvent);

            IEventSubscription subscription;
            if (subscriptions.TryGetValue(eventType, out subscription))
            {
                return (IEventSubscription<TEvent>)subscription;
            }

            return null;
        }
    }
}