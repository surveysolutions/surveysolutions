using System;

namespace WB.Core.SharedKernels.DataCollection
{
    public interface IRosterLevel
    {
        decimal @rowcode { get; }
        int @rowindex { get; }
    }
    public interface IRosterLevel<T> : IRosterLevel where T : class, IRosterLevel
    {
        RosterRowList<T> @roster { get; }
    }
}
