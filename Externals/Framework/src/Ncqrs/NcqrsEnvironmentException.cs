using System;

namespace Ncqrs
{
    /// <summary>
    /// Occurs when there was an exception in the Ncqrs environment configuration.
    /// </summary>
    public class NcqrsEnvironmentException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NcqrsEnvironmentException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public NcqrsEnvironmentException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NcqrsEnvironmentException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public NcqrsEnvironmentException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
