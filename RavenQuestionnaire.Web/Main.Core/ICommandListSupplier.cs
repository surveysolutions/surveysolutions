namespace Main.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface ICommandListSupplier
    {
        IEnumerable<Type> GetCommandList();
    }
}
