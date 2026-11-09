using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    public class TesterEventStore : IEventStore
    {
        private readonly Dictionary<Guid, List<CommittedEvent>> events = new Dictionary<Guid, List<CommittedEvent>>();
        private readonly object sync = new object();

        public IEnumerable<CommittedEvent> Read(Guid id, int minVersion)
        {
            lock (sync)
                return events.TryGetValue(id, out var stream)
                    ? stream.Where(e => e.EventSequence >= minVersion).ToArray()
                    : Array.Empty<CommittedEvent>();
        }

        public IEnumerable<CommittedEvent> Read(Guid id, int minVersion, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Read(id, minVersion);
        }

        public int? GetLastEventSequence(Guid id)
        {
            lock (sync)
                return events.TryGetValue(id, out var stream) && stream.Count > 0
                    ? stream[stream.Count - 1].EventSequence
                    : null;
        }

        public bool IsDirty(Guid eventSourceId, long lastKnownEventSequence)
            => GetLastEventSequence(eventSourceId) != lastKnownEventSequence;

        public CommittedEventStream Store(UncommittedEventStream eventStream)
        {
            if (!eventStream.IsNotEmpty)
                return new CommittedEventStream(Guid.Empty);

            lock (sync)
            {
                var sourceId = eventStream.SourceId;
                if (!events.TryGetValue(sourceId, out var storedEvents))
                {
                    storedEvents = new List<CommittedEvent>();
                    events.Add(sourceId, storedEvents);
                }

                var committedEvents = new List<CommittedEvent>();
                foreach (var evnt in eventStream)
                {
                    var committed = new CommittedEvent(eventStream.CommitId, evnt.Origin, evnt.EventIdentifier,
                        sourceId, evnt.EventSequence, evnt.EventTimeStamp, storedEvents.Count + committedEvents.Count,
                        evnt.Payload);
                    committedEvents.Add(committed);
                }

                var result = new CommittedEventStream(sourceId, committedEvents);
                storedEvents.AddRange(committedEvents);
                return result;
            }
        }
    }
}
