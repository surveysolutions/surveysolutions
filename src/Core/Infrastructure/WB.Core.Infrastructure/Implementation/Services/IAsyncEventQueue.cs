using System.Collections.Generic;
using Ncqrs.Eventing;

namespace WB.Core.Infrastructure.Implementation.Services
{
    public interface IAsyncEventQueue
    {
        void Enqueue(IReadOnlyCollection<CommittedEvent> @events);
    }
}
