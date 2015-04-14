using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WB.Core.SharedKernels.DataCollection
{
    public interface IRosterLevel
    {
        decimal @rowcode { get; }
        string @rowname { get; }
        void SetRowName(string rowname);
        int @rowindex { get; }
    }
    public interface IRosterLevel<T> : IRosterLevel where T : class, IRosterLevel
    {
        RosterRowList<T> @roster { get; }
    }
}
