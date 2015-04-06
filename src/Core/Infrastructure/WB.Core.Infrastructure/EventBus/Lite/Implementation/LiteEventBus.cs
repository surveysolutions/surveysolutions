using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.EventBus.Lite.Implementation
{
    public class LiteEventBus : ILiteEventBus
    {
        private readonly ILogger logger;
        private readonly IEventRegistry eventRegistry;

        public LiteEventBus(ILogger logger, IEventRegistry eventRegistry)
        {
            this.logger = logger;
            this.eventRegistry = eventRegistry;
        }

        public void Publish(IPublishableEvent eventMessage)
        {
            Publish<IPublishableEvent>(eventMessage);
        }

        public void Publish(IEnumerable<IPublishableEvent> eventMessages)
        {
            foreach (var eventMessage in eventMessages)
            {
                Publish(eventMessage);
            }
        }

        public void PublishUncommitedEventsFromAggregateRoot(IAggregateRoot aggregateRoot, string origin)
        {
            var uncommittedChanges = aggregateRoot.GetUncommittedChanges().ToArray();
            Publish(uncommittedChanges);

            aggregateRoot.MarkChangesAsCommitted();
        }

        public void Publish<TEvent>(TEvent @event)
        {
            var eventType = typeof (TEvent);
            logger.Info("Event {0} published".FormatString(eventType.ToString()));

            IEventSubscription<TEvent> subscription = eventRegistry.GetSubscription<TEvent>();
            if (subscription != null)
            {
                subscription.RaiseEvent(@event);
            }
            else
            {
                logger.Info("No subscribers for event {0} found.".FormatString(eventType.ToString()));
            }
        }
    }
}