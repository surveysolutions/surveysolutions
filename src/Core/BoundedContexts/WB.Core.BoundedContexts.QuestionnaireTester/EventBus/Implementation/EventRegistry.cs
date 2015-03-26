using System;
using System.Collections.Concurrent;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.BoundedContexts.QuestionnaireTester.EventBus.Implementation
{
    public class EventRegistry: IEventRegistry
    {
        private readonly ILogger logger;

        private readonly ConcurrentDictionary<Type, EventSubscription> subscriptions = new ConcurrentDictionary<Type, EventSubscription>();

        public EventRegistry(ILogger logger)
        {
            this.logger = logger;
        }

        #region IMessageBus Members

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            var subscription = (EventSubscription<TEvent>)subscriptions.GetOrAdd(
                typeof(TEvent),
                t => new EventSubscription<TEvent>());
            subscription.Subscribe(handler);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            var eventType = typeof (TEvent);
            EventSubscription subscription;
            if (subscriptions.TryGetValue(eventType, out subscription))
            {
                ((EventSubscription<TEvent>) subscription).Unsubscribe(handler);
            }
            else
            {
                logger.Info("No subscribers for event {0} found.".FormatString(eventType.ToString()));
            }
        }

        #endregion
    }
}