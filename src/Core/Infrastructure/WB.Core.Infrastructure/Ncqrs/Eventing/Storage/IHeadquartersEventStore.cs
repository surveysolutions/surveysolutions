using System;

namespace Ncqrs.Eventing.Storage
{
    public interface IHeadquartersEventStore : IEventStore
    {
        int CountOfAllEvents();
        bool HasEventsAfterSpecifiedSequenceWithAnyOfSpecifiedTypes(long sequence, Guid eventSourceId,
            params string[] typeNames);

        int? GetMaxEventSequenceWithAnyOfSpecifiedTypes(Guid eventSourceId, params string[] typeNames);
    }
}
