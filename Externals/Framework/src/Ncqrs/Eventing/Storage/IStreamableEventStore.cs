using System;

namespace Ncqrs.Eventing.Storage
{
    using System.Collections.Generic;

    public interface IStreamableEventStore : IEventStore
    {
        IEnumerable<CommittedEvent> GetEventStream();
        
    }
}
