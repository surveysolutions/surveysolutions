using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Handlers
{
    public interface IFunctionalHandler
    {
        Task<object> GetStateAsync(CancellationToken cancellationToken);

        Task SaveStateAsync(object state, CancellationToken cancellationToken);
    }

    public interface IEventHandler<TState, TEvent> where TEvent : IEvent
    {
        Task<TState> HandleAsync(TState state, PublishedEvent<TEvent> @event, CancellationToken cancellationToken);
    }
}
