using System;

namespace Ncqrs.Commanding.CommandExecution
{
    /// <summary>
    /// Occurs when no executor was not found to execute the command.
    /// </summary>
    public class ExecutorForCommandNotFoundException : Exception
    {
        /// <summary>
        /// Gets the type of the command.
        /// </summary>
        public Type CommandType
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutorForCommandNotFoundException"/> class.
        /// </summary>
        /// <param name="commandType">Type of the command.</param>
        /// <exception cref="ArgumentNullException">Occurs when <i>commandType</i> is a <c>null</c> dereference.</exception>
        public ExecutorForCommandNotFoundException(Type commandType) : this(commandType, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutorForCommandNotFoundException"/> class.
        /// </summary>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="message">The message.</param>
        /// <exception cref="ArgumentNullException">Occurs when <i>commandType</i> is a <c>null</c> dereference.</exception>
        public ExecutorForCommandNotFoundException(Type commandType, string message) : this(commandType, message, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutorForCommandNotFoundException"/> class.
        /// </summary>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner exception.</param>
        /// <exception cref="ArgumentNullException">Occurs when <i>commandType</i> is a <c>null</c> dereference.</exception>
        public ExecutorForCommandNotFoundException(Type commandType, string message, Exception inner) : base((String.IsNullOrEmpty(message) ? String.Format("No handler was found for command {0}.", commandType.FullName) : message), inner)
        {
            CommandType = commandType;
        }
    }
}
