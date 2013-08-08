using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Supervisor.Implementation
{
    internal interface ISampleRecordsAccessor
    {
        IEnumerable<string[]> Records { get; }
    }
}