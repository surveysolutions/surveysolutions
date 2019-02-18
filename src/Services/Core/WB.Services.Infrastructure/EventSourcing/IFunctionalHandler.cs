﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Services.Infrastructure.EventSourcing
{
    /// <summary>
    /// Stateful handler for events.
    /// Functional handler is the only one that responsible for own state
    /// </summary>
    public interface IFunctionalHandler : IStatefulDenormalizer
    {
    }

    public interface IStatefulDenormalizer
    {
        Task SaveStateAsync(CancellationToken cancellationToken = default);
    }

    public interface IEventsFilter
    {
        Task<List<Event>> FilterAsync(ICollection<Event> feed);
    }
}
