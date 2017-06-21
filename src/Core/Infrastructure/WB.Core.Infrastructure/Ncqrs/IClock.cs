using System;

namespace Ncqrs
{
    /// <summary>
    /// Represents a clock that can give the current time.
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// Gets the current UTC date and time from the clock.
        /// </summary>
        /// <returns>The current UTC date and time.</returns>
        DateTime UtcNow();
    }
}