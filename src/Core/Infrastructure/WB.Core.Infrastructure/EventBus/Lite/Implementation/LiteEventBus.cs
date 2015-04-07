using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.Aggregates;


namespace WB.Core.Infrastructure.EventBus.Lite.Implementation
{
    public class LiteEventBus : ILiteEventBus
    {
        private readonly IEventRegistry eventRegistry;

        public LiteEventBus(IEventRegistry eventRegistry)
        {
            this.eventRegistry = eventRegistry;
        }

        public void PublishUncommitedEventsFromAggregateRoot(IAggregateRoot aggregateRoot, string origin)
        {
            UncommittedEvent[] uncommittedChanges = aggregateRoot.GetUncommittedChanges().ToArray();

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
                var handlers = this.eventRegistry.GetHandlers(uncommittedChange.Payload);

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
                    throw new AggregateException(
                       string.Format("{0} handler(s) failed to handle published event '{1}' by event source '{2}' with sequence '{3}'.", exceptions.Count, uncommittedChange.EventIdentifier, uncommittedChange.EventSourceId, uncommittedChange.EventSequence),
                       exceptions);
            }
        }
    }
}