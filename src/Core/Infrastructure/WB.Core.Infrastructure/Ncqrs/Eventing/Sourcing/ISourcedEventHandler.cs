using System;

namespace Ncqrs.Eventing.Sourcing
{
    /// <summary>
    /// An event handler that handles the domain events.
    /// </summary>
    public interface ISourcedEventHandler
    {
        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="sourcedEvent">The event to handle.</param>
        /// <returns><c>true</c> when the event was handled; otherwise, <c>false</c>.
        /// <remarks><c>false</c> does not mean that the handling failed, but that the 
        /// handler was not interested in handling this event.</remarks></returns>
        Boolean HandleEvent(object sourcedEvent);
    }
}