using System;
using System.Collections.Generic;

namespace Main.Core
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface ICommandListSupplier
    {
        IEnumerable<Type> GetCommandList();
    }
}
