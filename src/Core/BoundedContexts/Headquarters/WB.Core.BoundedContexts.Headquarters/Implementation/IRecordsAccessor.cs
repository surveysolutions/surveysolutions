using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Implementation
{
    public interface IRecordsAccessor
    {
        IEnumerable<string[]> Records { get; }
    }
}