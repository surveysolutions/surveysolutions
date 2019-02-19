using System.Threading;
using System.Threading.Tasks;

namespace WB.Services.Export.Events
{
    public interface IEventProcessor
    {
        Task HandleNewEvents(long processId, CancellationToken token = default);
    }
}