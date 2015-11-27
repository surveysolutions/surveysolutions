using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;

namespace Ncqrs.Eventing.ServiceModel.Bus
{
    /// <summary>
    /// An interface that represents an event during its publishing and handling. At this stage event objects are genericaly typed
    /// to the actual payload type.
    /// </summary>
    /// <remarks>
    /// This interface is internal and is not mean to be implemented in user code. It is necessary because "out" type parameters can
    /// only be declared by interfaces (not classes). <see cref="IEventHandler{TEvent}"/> needs to declare "in" type parameter so
    /// <see cref="IPublishedEvent{TEvent}"/> have to have "out" modifier.
    /// </remarks>
    /// <typeparam name="TEvent">Type of the payload.</typeparam>
    public interface IPublishedEvent<out TEvent> : IPublishableEvent
        where TEvent : WB.Core.Infrastructure.EventBus.IEvent
    {        
        /// <summary>
        /// Gets the payload of the event.
        /// </summary>
        new TEvent Payload { get;}
    }
}