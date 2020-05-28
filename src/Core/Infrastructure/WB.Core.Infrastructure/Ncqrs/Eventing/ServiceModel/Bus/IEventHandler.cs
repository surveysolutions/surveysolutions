using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;

namespace Ncqrs.Eventing.ServiceModel.Bus
{
    /// <summary>
    /// Represents an object which can handle events of a given type (and all types derived from it).
    /// </summary>
    /// <typeparam name="TEvent">Type of event this object can handle.</typeparam>
    public interface IEventHandler<in TEvent>
        where TEvent : WB.Core.Infrastructure.EventBus.IEvent
    {
        /// <summary>
        /// Processes provided event. Actual event type could be a subtype of <typeparamref name="TEvent"/>. In such cache,
        /// <paramref name="evnt"/> value would be also generically typed to a subtype, not the base <typeparamref name="TEvent"/> type.
        /// </summary>
        /// <param name="evnt">Object representing the event.</param>
        void Handle(IPublishedEvent<TEvent> evnt);
    }
}
