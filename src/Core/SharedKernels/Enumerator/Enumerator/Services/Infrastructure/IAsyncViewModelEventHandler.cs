using System.Threading.Tasks;
using WB.Core.Infrastructure.EventBus;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IAsyncViewModelEventHandler<TEvent> : IViewModelEventHandler
        where TEvent : IEvent
    {
        Task HandleAsync(TEvent @event);
    }
}
