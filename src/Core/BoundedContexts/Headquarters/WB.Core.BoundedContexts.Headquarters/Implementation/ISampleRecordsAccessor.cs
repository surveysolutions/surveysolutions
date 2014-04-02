using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Implementation
{
    public interface ISampleRecordsAccessor
    {
        IEnumerable<string[]> Records { get; }
    }
}