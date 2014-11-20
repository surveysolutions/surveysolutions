using System;
using System.Runtime.Serialization;

namespace Ncqrs.Domain
{
    /// <summary>
    /// Thrown when a <see cref="IUnitOfWork"/> is requested but was not available in the context.
    /// </summary>
    public class NoUnitOfWorkAvailableInThisContextException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoUnitOfWorkAvailableInThisContextException"/> class.
        /// </summary>
        public NoUnitOfWorkAvailableInThisContextException() : this("There is no unit of work available in this context.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoUnitOfWorkAvailableInThisContextException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public NoUnitOfWorkAvailableInThisContextException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoUnitOfWorkAvailableInThisContextException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner exception.</param>
        public NoUnitOfWorkAvailableInThisContextException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
