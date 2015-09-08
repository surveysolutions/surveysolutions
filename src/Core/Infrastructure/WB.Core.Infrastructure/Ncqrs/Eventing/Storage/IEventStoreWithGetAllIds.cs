using System;
using System.Collections.Generic;

namespace Ncqrs.Eventing.Storage
{
    public interface IEventStoreWithGetAllIds : IEventStore
    {
        IEnumerable<Guid> GetAllIds();
    }
}