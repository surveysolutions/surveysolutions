﻿using System;

namespace Ncqrs.Eventing
{
    /// <summary>
    /// The base for all event messages. All sourced events should subclass from <see cref="Ncqrs.Eventing.Sourcing.ISourcedEvent"/>.
    /// </summary>
    public abstract class Event : IEvent
    {
        /// <summary>
        /// Gets the unique identifier for this event.
        /// </summary>
        public Guid EventIdentifier { get; internal protected set; }

        /// <summary>
        /// Gets the time stamp for this event.
        /// </summary>
        /// <value>a <see cref="DateTime"/> UTC value that represents the point
        /// in time where this event occurred.</value>
        public DateTime EventTimeStamp { get; internal protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Event"/> class.
        /// </summary>
        public Event()
        {
            var clock = NcqrsEnvironment.Get<IClock>();

            EventIdentifier = Guid.NewGuid();
            EventTimeStamp = clock.UtcNow();
        }

        public Event(Guid eventIdentifier, DateTime eventTimeStamp)
        {
            EventIdentifier = eventIdentifier;
            EventTimeStamp = eventTimeStamp;
        }
    }
}