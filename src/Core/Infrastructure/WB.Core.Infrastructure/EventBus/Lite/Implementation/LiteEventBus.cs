using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Aggregates;


namespace WB.Core.Infrastructure.EventBus.Lite.Implementation
{
    public class LiteEventBus : ILiteEventBus
    {
        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly IEventStore eventStore;

        public LiteEventBus(ILiteEventRegistry liteEventRegistry, IEventStore eventStore)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.eventStore = eventStore;
        }

        public void PublishUncommitedEventsFromAggregateRoot(IAggregateRoot aggregateRoot, string origin, bool isBulk = false)
        {
            UncommittedEvent[] uncommittedChanges = aggregateRoot.GetUncommittedChanges().ToArray();
            var eventStream = new UncommittedEventStream(origin);

            foreach (UncommittedEvent @event in uncommittedChanges)
            {
                eventStream.Append(@event);
            }

            this.eventStore.Store(eventStream);

            try
            {
                Publish(uncommittedChanges);
            }
            finally
            {
                aggregateRoot.MarkChangesAsCommitted();
            }
        }

        private void Publish(UncommittedEvent[] uncommittedChanges)
        {
            foreach (var uncommittedChange in uncommittedChanges)
            {
                var handlers = this.liteEventRegistry.GetHandlers(uncommittedChange);

                var exceptions = new List<Exception>();

                foreach (var handler in handlers)
                {
                    try
                    {
                        handler.Invoke(uncommittedChange.Payload);
                    }
                    catch (Exception exception)
                    {
                        exceptions.Add(exception);
                    }
                }

                if (exceptions.Count > 0)
                {
                    var message = string.Format("{0} handler(s) failed to handle published event '{1}' by event source '{2}' with sequence '{3}'.", 
                        exceptions.Count, uncommittedChange.EventIdentifier, uncommittedChange.EventSourceId, uncommittedChange.EventSequence);
                    throw new AggregateException(message, exceptions);
                }
            }
        }
    }
}