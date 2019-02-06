using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Events.Interview;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Handlers
{
    public interface IFunctionalHandler<TState>
    {
        TState GetStateAsync(CancellationToken cancellationToken);

        Task SaveStateAsync(TState state, CancellationToken cancellationToken);
    }

    public interface IEventHandler<TState, TEvent> where TEvent : IEvent
    {
        Task<TState> HandleAsync(TState state, PublishedEvent<TEvent> @event, CancellationToken cancellationToken);
    }
}
