using System;
using System.Runtime.Serialization;
using WB.Core.Infrastructure.CommandBus;

namespace Ncqrs.Commanding.CommandExecution.Mapping
{
    /// <summary>
    /// Occurs when there is no auto mapping found for a <see cref="ICommand"/>.
    /// </summary>
    [Serializable]
    public class MappingNotFoundException : CommandMappingException
    {
        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <value>The command.</value>
        public ICommand Command
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="command">The command.</param>
        public MappingNotFoundException(string message, ICommand command)
            : this(message, command, null)
        {
        }

        public MappingNotFoundException(string message, ICommand command, Exception inner) : base(message, inner)
        {
            Command = command;
        }
    }
}
