using System;

namespace Ncqrs.Eventing
{    
    /// <summary>
    /// Represents an event.
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// Gets the unique identifier for this event.
        /// </summary>
        /// <value>A <see cref="Guid"/> that represents the unique identifier of this event.</value>
        Guid EventIdentifier
        {
            get;
        }

        /// <summary>
        /// Gets the time stamp for this event.
        /// </summary>
        /// <value>a <see cref="DateTime"/> UTC value that represents the point
        /// in time where this event occurred.</value>
        DateTime EventTimeStamp
        {
            get;
        }
    }
}