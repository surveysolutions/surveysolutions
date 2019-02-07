using System;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Services.Infrastructure.EventSourcing
{
    /// <summary>
    /// Marker interface to find Event handlers
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IEventHandler<TEvent> where TEvent : IEvent
    {
        [Handler]
        Task HandleAsync(PublishedEvent<TEvent> @event, CancellationToken cancellationToken = default);
    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class HandlerAttribute : Attribute
    {
    }
}
