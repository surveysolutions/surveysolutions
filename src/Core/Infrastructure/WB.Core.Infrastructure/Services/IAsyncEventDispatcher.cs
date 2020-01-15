using System.Collections.Generic;
using System.Threading.Tasks;
using Ncqrs.Eventing;

namespace WB.Core.Infrastructure.Services
{
    public interface IAsyncEventDispatcher
    {
        Task ExecuteAsync(IReadOnlyCollection<CommittedEvent> item);
    }
}
