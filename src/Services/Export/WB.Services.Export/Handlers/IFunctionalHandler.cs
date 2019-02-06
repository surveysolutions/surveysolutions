using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Events.Interview;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Handlers
{
    public interface IFunctionalHandler<TState>
    {
        TState GetState(CancellationToken cancellationToken);

        Task SaveState(TState state, CancellationToken cancellationToken);
    }

    public interface IEventHandler<TState, TEvent> where TEvent : IEvent
    {
        Task<TState> Handle(TState state, PublishedEvent<TEvent> @event, CancellationToken cancellationToken);
    }
}
