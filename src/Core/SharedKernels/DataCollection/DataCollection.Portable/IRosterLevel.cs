using System;

namespace WB.Core.SharedKernels.DataCollection
{
    public interface IRosterLevel
    {
        decimal @rowcode { get; }
        [Obsolete("obsolete since 5.10")]
        string @rowname { get; }
        [Obsolete("obsolete since 5.10")]
        void SetRowName(string rowname);
        int @rowindex { get; }
    }
    public interface IRosterLevel<T> : IRosterLevel where T : class, IRosterLevel
    {
        RosterRowList<T> @roster { get; }
    }
}
