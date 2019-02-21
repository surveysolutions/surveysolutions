using System.Threading;
using System.Threading.Tasks;

namespace WB.Services.Export.Events
{
    public interface IEventProcessor
    {
        Task HandleNewEvents(long exportProcessId, CancellationToken token = default);
    }
}
