using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Supervisor.Implementation
{
    public interface ISampleRecordsAccessor
    {
        IEnumerable<string[]> Records { get; }
    }
}