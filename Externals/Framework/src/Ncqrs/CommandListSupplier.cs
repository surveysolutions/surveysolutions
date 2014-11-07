using System;
using System.Collections.Generic;

namespace Ncqrs
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class CommandListSupplier:ICommandListSupplier
    {
        #region Implementation of ICommandListSupplier

        public CommandListSupplier(IEnumerable<Type> commands)
        {
            this.commands = commands;
        }

        public IEnumerable<Type> GetCommandList()
        {
            return commands;
        }

        #endregion

        private IEnumerable<Type> commands;
    }
}
