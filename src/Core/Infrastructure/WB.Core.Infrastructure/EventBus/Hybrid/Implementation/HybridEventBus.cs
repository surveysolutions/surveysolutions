using System;
using System.Collections.Generic;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Core.Infrastructure.EventBus.Hybrid.Implementation
{
    public class HybridEventBus : IEventBus
    {
        private readonly ILiteEventBus liteEventBus;
        private readonly IEventBus cqrsEventBus;

        public HybridEventBus(ILiteEventBus liteEventBus, IEventBus cqrsEventBus)
        {
            this.liteEventBus = liteEventBus;
            this.cqrsEventBus = cqrsEventBus;
        }

        public void CommitUncommittedEvents(IAggregateRoot aggregateRoot, string origin)
        {
            this.liteEventBus.CommitUncommittedEvents(aggregateRoot, origin);
        }

        public void PublishUncommittedEvents(IAggregateRoot aggregateRoot, bool isBulk = false)
        {
            ExecuteAllThrowOneAggregate(
                () => this.liteEventBus.PublishUncommittedEvents(aggregateRoot, isBulk),
                () => this.cqrsEventBus.PublishUncommittedEvents(aggregateRoot, isBulk));
        }

        public void Publish(IPublishableEvent eventMessage)
        {
            this.cqrsEventBus.Publish(eventMessage);
        }

        public void Publish(IEnumerable<IPublishableEvent> eventMessages)
        {
            this.cqrsEventBus.Publish(eventMessages);
        }

        private static void ExecuteAllThrowOneAggregate(params Action[] actions)
        {
            var exceptions = new List<Exception>();

            foreach (var action in actions)
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}