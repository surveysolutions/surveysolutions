using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Handlers
{
    /// <summary>
    /// Marker interface to find Event handlers
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IEventHandler<TEvent> where TEvent : IEvent
    {
        Task HandleAsync(PublishedEvent<TEvent> @event, CancellationToken cancellationToken = default);
    }
}