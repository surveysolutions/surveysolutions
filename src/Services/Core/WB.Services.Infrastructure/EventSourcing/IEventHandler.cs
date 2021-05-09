using System;
using System.Threading;
using System.Threading.Tasks;
using WB.ServicesIntegration.Export;

namespace WB.Services.Infrastructure.EventSourcing
{
    /// <summary>
    /// Marker interface to find Event handlers
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IEventHandler<TEvent> where TEvent : class, IEvent
    {
        [Handler]
        void Handle(PublishedEvent<TEvent> @event);
    }

    /// <summary>
    /// Marker interface to find Event handlers
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IAsyncEventHandler<TEvent> where TEvent : class, IEvent
    {
        [Handler]
        Task Handle(PublishedEvent<TEvent> @event, CancellationToken cancellationToken = default);
    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class HandlerAttribute : Attribute
    {
    }
}
