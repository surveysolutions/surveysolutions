using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ncqrs.Eventing.Storage
{
    public interface IHeadquartersEventStore : IEventStore
    {
        int CountOfAllEvents();

        bool HasEventsAfterSpecifiedSequenceWithAnyOfSpecifiedTypes(long sequence, Guid eventSourceId,
            params string[] typeNames);

        int? GetMaxEventSequenceWithAnyOfSpecifiedTypes(Guid eventSourceId, params string[] typeNames);

        Task<EventsFeedPage> GetEventsFeedAsync(long startWithGlobalSequence, int pageSize);
    }
}
