#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WB.Core.Infrastructure.Aggregates;

namespace Ncqrs.Eventing.Storage
{
    /// <summary>
    /// An in memory event store that can be used for unit testing purpose.
    /// </summary>
    public class InMemoryEventStore : IInMemoryEventStore
    {
        private readonly IAggregateRootCache cache;

        public InMemoryEventStore(IAggregateRootCache cache)
        {
            this.cache = cache;
        }

        public IEnumerable<CommittedEvent> Read(Guid id, int minVersion)
        {
            return GetFromCache(id).Where(x => x.EventSequence >= minVersion);
        }

        public IEnumerable<CommittedEvent> Read(Guid id, int minVersion, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
        {
            return Read(id, minVersion);
        }

        private Queue<CommittedEvent> GetFromCache(Guid id)
        {
            var cacheItem = this.cache.GetOrCreate(id);

            if (!(cacheItem.GetEvents() is Queue<CommittedEvent> events))
            {
                events = new Queue<CommittedEvent>();
                cacheItem.SetEvents(events);
                return events;
            }

            return events;
        }

        public virtual int? GetLastEventSequence(Guid id) => GetFromCache(id).LastOrDefault()?.EventSequence;

        public bool IsDirty(Guid eventSourceId, long lastKnownEventSequence)
        {
            return GetLastEventSequence(eventSourceId) != lastKnownEventSequence;
        }

        public CommittedEventStream Store(UncommittedEventStream eventStream)
        {
            if (eventStream.IsNotEmpty)
            {
                List<CommittedEvent> result = new List<CommittedEvent>();

                var events = GetFromCache(eventStream.SourceId);

                foreach (var evnt in eventStream)
                {
                    var committedEvent = new CommittedEvent(eventStream.CommitId,
                        evnt.Origin,
                        evnt.EventIdentifier,
                        eventStream.SourceId,
                        evnt.EventSequence,
                        evnt.EventTimeStamp,
                        events.Count,
                        evnt.Payload);
                    events.Enqueue(committedEvent);
                    result.Add(committedEvent);
                }

                return new CommittedEventStream(eventStream.SourceId, result);
            }

            return new CommittedEventStream(eventStream.SourceId);
        }
    }
}
