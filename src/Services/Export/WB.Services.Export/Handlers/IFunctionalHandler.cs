using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Handlers
{
    public interface IFunctionalHandler<TState>
    {
        TState GetState(CancellationToken cancellationToken);

        Task ApplyEvent<TEvent>(TState state, PublishedEvent<TEvent> @event, CancellationToken cancellationToken) where TEvent: IEvent;

        Task SaveState(TState state, CancellationToken cancellationToken);
    }
}
