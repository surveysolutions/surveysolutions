using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using WB.Core.Infrastructure.CommandBus;

namespace Ncqrs.Commanding
{
    /// <summary>
    /// The base of a command message. A command should contain all the
    /// information and intend that is needed to execute an corresponding
    /// action.
    /// </summary>
    [DataContract]
    public abstract class CommandBase : ICommand
    {
        /// <summary>
        /// Gets the unique identifier for this command.
        /// </summary>
        public Guid CommandIdentifier
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBase"/> class.
        /// This initializes the <see cref="CommandIdentifier"/> with the given
        /// id from <paramref name="commandIdentifier"/>.
        /// </summary>
        /// <param name="commandIdentifier">The command identifier.</param>
        protected CommandBase(Guid commandIdentifier)
        {
            CommandIdentifier = commandIdentifier;
        }

        protected CommandBase()
        {
        }
    }
}
