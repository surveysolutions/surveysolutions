using System;
using System.Collections.Generic;
using Ncqrs.Eventing;
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

        public IEnumerable<CommittedEvent> CommitUncommittedEvents(IEventSourcedAggregateRoot aggregateRoot, string origin)
        {
            var eventStream = new UncommittedEventStream(origin, aggregateRoot.GetUnCommittedChanges());

            return this.eventStore.Store(eventStream);
        }

        public void PublishCommittedEvents(IEnumerable<CommittedEvent> committedEvents)
        {
            foreach (var uncommittedChange in committedEvents)
            {
                var handlers = this.liteEventRegistry.GetHandlers(uncommittedChange);

                var exceptions = new List<Exception>();

                foreach (var handler in handlers)
                {
                    try
                    {
                        handler.Invoke(uncommittedChange);
                    }
                    catch (Exception exception)
                    {
                        exceptions.Add(exception);
                    }
                }

                if (exceptions.Count > 0)
                {
                    var message = string.Format("{0} handler(s) failed to handle published event '{1}' of type '{4} by event source '{2}' with sequence '{3}'.", 
                        exceptions.Count, uncommittedChange.EventIdentifier, uncommittedChange.EventSourceId, uncommittedChange.EventSequence, uncommittedChange.Payload.GetType().Name);
                    throw new AggregateException(message, exceptions);
                }
            }
        }
    }
}
