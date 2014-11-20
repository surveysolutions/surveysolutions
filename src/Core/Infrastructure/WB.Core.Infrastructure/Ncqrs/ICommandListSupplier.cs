using System;
using System.Collections.Generic;

namespace Ncqrs
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface ICommandListSupplier
    {
        IEnumerable<Type> GetCommandList();
    }
}
