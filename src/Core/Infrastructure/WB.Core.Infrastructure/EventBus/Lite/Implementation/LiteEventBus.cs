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

        public CommittedEventStream CommitUncommittedEvents(IAggregateRoot aggregateRoot, string origin)
        {
            var eventStream = new UncommittedEventStream(origin, aggregateRoot.GetUnCommittedChanges());
            var commitUncommittedEvents = this.eventStore.Store(eventStream);
            return commitUncommittedEvents;
        }

        public void PublishCommittedEvents(CommittedEventStream committedEvents)
        {
            foreach (var uncommittedChange in committedEvents)
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