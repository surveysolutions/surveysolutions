using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WB.Core.GenericSubdomains.Portable;

namespace Ncqrs.Eventing.Storage
{
    /// <summary>
    /// An in memory event store that can be used for unit testing purpose. We can't
    /// think of any situation where you want to use this in production.
    /// </summary>
    public class InMemoryEventStore : IInMemoryEventStore
    {
        private readonly Dictionary<Guid, Queue<CommittedEvent>> _events = new Dictionary<Guid, Queue<CommittedEvent>>();
        
        public IEnumerable<CommittedEvent> Read(Guid id, int minVersion)
            => this._events.GetOrNull(id)?.Where(x => x.EventSequence >= minVersion)
            ?? Enumerable.Empty<CommittedEvent>();

        public IEnumerable<CommittedEvent> Read(Guid id, int minVersion, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
        {
            return Read(id, minVersion);
        }

        public virtual int? GetLastEventSequence(Guid id)
             => this._events.GetOrNull(id)?.LastOrDefault()?.EventSequence;

        public bool IsDirty(Guid eventSourceId, long lastKnownEventSequence)
        {
            return GetLastEventSequence(eventSourceId) != lastKnownEventSequence;
        }

        public CommittedEventStream Store(UncommittedEventStream eventStream)
        {
            if (eventStream.IsNotEmpty)
            {
                List<CommittedEvent> result = new List<CommittedEvent>();

                if (!_events.TryGetValue(eventStream.SourceId, out var events))
                {
                    events = new Queue<CommittedEvent>();
                    _events.Add(eventStream.SourceId, events);
                }

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
