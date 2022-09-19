using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ncqrs.Eventing.Storage
{
    public class RawEvent
    {
        public Guid Id { get; set; }
        public Guid EventSourceId { get; set; }
        public string Origin { get; set; }
        public int EventSequence { get; set; }
        public DateTime TimeStamp { get; set; }
        public long GlobalSequence { get; set; }
        public string EventType { get; set; }
        public string Value { get; set; }
    }

    public interface IHeadquartersEventStore : IEventStore
    {
        bool HasEventsAfterSpecifiedSequenceWithAnyOfSpecifiedTypes(long sequence, Guid eventSourceId,
            params string[] typeNames);

        int? GetMaxEventSequenceWithAnyOfSpecifiedTypes(Guid eventSourceId, params string[] typeNames);

        IEnumerable<RawEvent> GetRawEventsFeed(long startWithGlobalSequence, int pageSize, long limitSize = long.MaxValue);

        Task<long> GetMaximumGlobalSequence();

        Task<List<CommittedEvent>> GetEventsInReverseOrderAsync(Guid aggregateRootId, int offset, int limit);

        Task<int> TotalEventsCountAsync(Guid aggregateRootId);
        Guid? GetLastEventId(Guid interviewId, params string[] excludeTypeNames);
        IEnumerable<CommittedEvent> ReadAfter(Guid aggregateRootId, Guid eventId);
    }
}
